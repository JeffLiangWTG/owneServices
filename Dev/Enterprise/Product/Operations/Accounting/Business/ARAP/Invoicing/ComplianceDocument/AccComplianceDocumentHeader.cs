using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	[CodeProperty(AccComplianceDocumentHeader.Schema.ADH_DocumentNumber)]
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public abstract class AccComplianceDocumentHeader : AutoAccComplianceDocumentHeader, IWorkflowProvider, IDocManagerSupport, IDocumentSupportable, IComplianceDocumentHeaderDetail, IEvaluateComplianceRule, IComplianceRuleParentTransaction, IHandleDeleteError, ISupportCriticalValidation, IEDocsParsingSupport
	{
		public new class Schema : AutoAccComplianceDocumentHeader.Schema
		{
			public const string DisplayInvoiceAddressOverrideForAddressControl = "DisplayInvoiceAddressOverrideForAddressControl";
			public const string DisplayInvoiceContactOverride = "DisplayInvoiceContactOverride";

			public const string EInvoicingStatus = "EInvoicingStatus";
			public const string EInvoicingError = "EInvoicingError";
			public const string EInvoicingLastResponseReceivedUtc = "EInvoicingLastResponseReceivedUtc";
			public const string EInvoicingLastSentTimeUtc = "EInvoicingLastSentTimeUtc";
			public const string EInvoicingBatchNumber = "EInvoicingBatchNumber";
			public const string EInvoicingBatchStatus = "EInvoicingBatchStatus";
		}

		public static readonly AccComplianceDocumentHeaderTypeDecider TypeDecider = new AccComplianceDocumentHeaderTypeDecider();

		protected AccComplianceDocumentHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ADH_DocumentStatus), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ADH_InternalReference), ConcurrencyPolicy.Strict);
		}

		#region Properties

		public ZBool ShouldPreventPrintDocument => ADH_ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE
				&& (Organisation?.OH_Category ?? ZString.Empty) == OrgConstants.Category.NaturalPersonIndividual
				&& (!Organisation?.CustomsCodes?.Cast<OrgCusCode>().FirstOrDefault(y => (y.OK_CodeType == OrgCusCode.TaiwanCodeTypes.MCI || y.OK_CodeType == OrgCusCode.TaiwanCodeTypes.PIG)
				&& y.OK_RN_NKCodeCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.OK_CustomsRegNo.IsEmpty ?? false);

		internal AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (periodCalculator == null)
				{
					periodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return periodCalculator;
			}
		}
		AccountingPeriodCalculator periodCalculator;

		AccTransactionHeaderCollection fTransactionHeaders;
		public AccTransactionHeaderCollection TransactionHeaders
		{
			get
			{
				if (fTransactionHeaders == null)
				{
					fTransactionHeaders = new AccTransactionHeaderCollection(Factory);
					ComplianceDocumentLines.ForEach(x => fTransactionHeaders.AddRange((x as AccComplianceDocumentLine).TransactionHeaders));
				}
				return fTransactionHeaders;
			}
		}

		AccComplianceDocumentLineCollection fComplianceDocumentLines;
		[ChildEditable]
		public AccComplianceDocumentLineCollection ComplianceDocumentLines
		{
			get
			{
				if (fComplianceDocumentLines == null)
				{
					fComplianceDocumentLines = new AccComplianceDocumentLineCollection(Factory);
					if (!IsVoided)
					{
						var query = new ZQuery(AccComplianceDocumentLineSchema.ADL_ADH, PK);
						fComplianceDocumentLines.Load(query);
					}

					RegisterEditableChildObject(fComplianceDocumentLines);
				}

				return fComplianceDocumentLines;
			}
		}

		protected void ReloadComplianceDocumentLines()
		{
			if (fComplianceDocumentLines != null)
			{
				fComplianceDocumentLines = null;
			}
		}

		AccComplianceDocumentHeader fOriginalComplianceDocumentHeader;
		protected AccComplianceDocumentHeader OriginalComplianceDocumentHeader
		{
			get
			{
				if (fOriginalComplianceDocumentHeader == null)
				{
					if (ComplianceDocumentLines.Any() && ComplianceDocumentLines[0].ComplianceDocumentPivots.Any())
					{
						var creditNoteLine = Factory.Load<ARCreditNoteLine>(ComplianceDocumentLines[0].ComplianceDocumentPivots[0].ADP_AL);
						if (creditNoteLine != null)
						{
							var invoicingLine = Factory.Load<ARInvoiceLine>(creditNoteLine.CopiedFromPK);
							fOriginalComplianceDocumentHeader = invoicingLine?.ComplianceDocumentHeader;
						}
					}
				}

				return fOriginalComplianceDocumentHeader;
			}
		}

		AccComplianceDocumentLineCollection fOriginalComplianceDocumentLines;
		[ChildEditable]
		public AccComplianceDocumentLineCollection OriginalComplianceDocumentLines
		{
			get
			{
				if (fOriginalComplianceDocumentLines == null)
				{
					fOriginalComplianceDocumentLines = new AccComplianceDocumentLineCollection(Factory);
					var query = new ZQuery(AccComplianceDocumentLineSchema.ADL_ADH, PK);
					fOriginalComplianceDocumentLines.Load(query);

					RegisterEditableChildObject(fOriginalComplianceDocumentLines);
				}

				return fOriginalComplianceDocumentLines;
			}
		}

		public CodeDescriptionPairList OrganisationCategoryList
		{
			get
			{
				if (fOrganisationCategoryList == null)
				{
					fOrganisationCategoryList = new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory);
					fOrganisationCategoryList.Insert(0, new CodeDescriptionPair(AccChargeTaxOverride.ALL, CategoryAdditionalDescriptions.All));
				}
				return fOrganisationCategoryList;
			}
		}

		public static class CategoryAdditionalDescriptions
		{
			public static string All
			{
				get { return Res.GetString("B75ADDB5-1456-4C9F-848C-34AB741D7F5A", "All Categories"); }
			}
		}

		CodeDescriptionPairList fOrganisationCategoryList;

		[List("ComplianceSubTypeInLocalLanguageList")]
		public override ZString ADH_ComplianceSubType { get => base.ADH_ComplianceSubType; set => base.ADH_ComplianceSubType = value; }

		public ICodeDescriptionPairList ComplianceSubTypeInLocalLanguageList
		{
			get
			{
				return AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.Country.Code);
			}
		}

		[DecimalPlaces(nameof(ADH_Calc_RXDecimals))]
		public virtual ZDecimal Amount => ComplianceDocumentLines.Sum(x => (x as AccComplianceDocumentLine).LocalAmount);

		[DecimalPlaces(nameof(ADH_Calc_RXDecimals))]
		public virtual ZDecimal TaxAmount => ComplianceDocumentLines.Sum(x => (x as AccComplianceDocumentLine).LocalTaxAmount);

		[DecimalPlaces(nameof(ADH_Calc_RXDecimals))]
		public virtual ZDecimal TotalAmount => ComplianceDocumentLines.Sum(x => (x as AccComplianceDocumentLine).LocalTotalAmount);

		public virtual ZString VATRegistrationNum
		{
			get
			{
				if (ADH_VATRegistrationNumberOverride.IsEmpty)
				{
					return Organisation?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode && x.OK_RN_NKCodeCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.OK_CustomsRegNo ?? ZString.Empty;
				}
				else
				{
					return ADH_VATRegistrationNumberOverride;
				}
			}
		}

		public virtual ZString OrgHeaderName => Organisation?.OH_Code ?? ZString.Empty;

		public ZString OrgAddressCompanyName
		{
			get
			{
				return AddressOverride?.CompanyName ?? ZString.Empty;
			}
		}

		public ZBool IsSequenceBookExpired
		{
			get
			{
				ZBool result = false;
				if (ComplianceBook != null &&
					(!ComplianceBook.XD_ExpiryDate.IsEmpty && ComplianceBook.XD_ExpiryDate < ADH_DocumentDate.Date.ToDateTime()) ||
					(!ComplianceBook.XD_StartDate.IsEmpty && ComplianceBook.XD_StartDate > ADH_DocumentDate))
				{
					result = true;
				}
				return result;
			}
		}

		[List("OrganisationCategoryList")]
		public virtual ZString OrgHeaderCategory => Organisation?.OH_Category ?? ZString.Empty;

		[List("Lookups.TransactionCurrencies")]
		public virtual ZString ADH_Readonly_RXCode => GlbCompany.CurrentCompany.LocalCurrency.RX_Code;

		public ZPropertyInfo ADH_RXCode_ReadonlyInfo => GetZPropertyInfo(nameof(ADH_Readonly_RXCode));

		public int ADH_Calc_RXDecimals => GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		protected virtual bool ADH_ReportingPeriod_ReadOnly => false;

		protected virtual bool ADH_DocumentNumber_ReadOnly => false;

		protected virtual bool ADH_ComplianceSubType_ReadOnly => false;

		protected virtual bool ADH_DocumentDate_ReadOnly => false;

		protected virtual bool ADH_XD_ComplianceBook_ReadOnly => false;

		protected virtual bool DisplayInvoiceContactOverride_ReadOnly => this.IsFinalised;

		[List("ComplianceDocumentStatus")]
		public override ZString ADH_DocumentStatus
		{
			get => base.ADH_DocumentStatus;
			set
			{
				if (!IsVoided)
				{
					base.ADH_DocumentStatus = value;
				}
			}
		}

		public ICodeDescriptionPairList ComplianceDocumentStatus => new CodeDescriptionPairList(OLookUpEditType.ComplianceDocumentStatus);

		[List("SupportingReasonCodesList")]
		public override ZString ADH_SupportingReason { get => base.ADH_SupportingReason; set => base.ADH_SupportingReason = value; }

		public ICodeDescriptionPairList SupportingReasonCodesList
		{
			get
			{
				return AccComplianceDocumentHeaderLookups.GetSupportingReasonCodesList(ADH_Ledger);
			}
		}

		[List("SupportingDocumentType")]
		public override ZString ADH_SupportingDocumentType { get => base.ADH_SupportingDocumentType; set => base.ADH_SupportingDocumentType = value; }

		public ICodeDescriptionPairList SupportingDocumentType => AccComplianceDocumentHeaderLookups.SupportingDocumentTypeList;

		public override ZDateTime ADH_DocumentDate
		{
			get { return base.ADH_DocumentDate; }
			set
			{
				base.ADH_DocumentDate = value;

				ADH_ReportingPeriod = PeriodCalculator.GetPeriodFromDate(value);
			}
		}

		public ZBool IsFromCreditNoteAndConfigurationEnabled => ADH_TransactionType == TransactionTypes.CreditNote && AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.Value;

		public AccComplianceDocumentHeader INVComplianceDocumentHeaderForCRD
		{
			get
			{
				if (fINVComplianceDocumentHeaderForCRD == null && !string.IsNullOrEmpty(ADH_DocumentNumber))
				{
					fINVComplianceDocumentHeaderForCRD = AccComplianceDocumentHeaderDetailValidationHelper.GetDuplicateNumberComplianceDocument(TransactionTypes.Invoice, ADH_OH_Organisation);
				}
				return fINVComplianceDocumentHeaderForCRD;
			}
		}
		AccComplianceDocumentHeader fINVComplianceDocumentHeaderForCRD;

		public AccComplianceDocumentHeaderDetailValidationHelper AccComplianceDocumentHeaderDetailValidationHelper
		{
			get
			{
				if (fAccComplianceDocumentHeaderDetailValidationHelper == null)
				{
					fAccComplianceDocumentHeaderDetailValidationHelper = new AccComplianceDocumentHeaderDetailValidationHelper(Factory, this);
				}

				return fAccComplianceDocumentHeaderDetailValidationHelper;
			}
		}

		AccComplianceDocumentHeaderDetailValidationHelper fAccComplianceDocumentHeaderDetailValidationHelper;

		public ZBool IsSpecialVoiding
		{
			get
			{
				if (!isSpecialVoiding)
				{
					isSpecialVoiding = !string.IsNullOrEmpty(ADH_VoidingReason);
				}

				return isSpecialVoiding;
			}
			set
			{
				isSpecialVoiding = value;
				var specialVoidingPrperties = new string[] { nameof(ADH_VoidingReason), nameof(ADH_ApprovalNumber) };

				if (!isSpecialVoiding)
				{
					ADH_VoidingReason = ZString.Empty;
					ADH_ApprovalNumber = ZString.Empty;

					RemoveWritableProperties(specialVoidingPrperties);
				}
				else
				{
					AddWritableProperties(specialVoidingPrperties);
				}
			}
		}
		ZBool isSpecialVoiding;

		protected bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = true;
			if (property.HasSetter())
			{
				result = !WritableProperties.Contains(property.Name);
			}
			return result && CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		List<string> WritableProperties
		{
			get
			{
				if (writableProperties == null)
				{
					writableProperties = new List<string>();
				}
				return writableProperties;
			}
		}
		List<string> writableProperties;

		public void AddWritableProperties(string[] writablePropertyList)
		{
			if (writablePropertyList != null && writablePropertyList.Length > 0)
			{
				foreach (string property in writablePropertyList)
				{
					if (!WritableProperties.Contains(property))
					{
						WritableProperties.Add(property);
					}
				}
			}
			RefreshBinding();
		}

		void RemoveWritableProperties(string[] writablePropertyList)
		{
			if (writablePropertyList != null && writablePropertyList.Length > 0)
			{
				foreach (string property in writablePropertyList)
				{
					if (WritableProperties.Contains(property))
					{
						WritableProperties.Remove(property);
					}
				}
			}
			RefreshBinding();
		}

		public override ZGuid ADH_OH_Organisation
		{
			get => base.ADH_OH_Organisation;
			set
			{
				base.ADH_OH_Organisation = value;

				CollectInfoForAddressNotRelatedToOrg();
			}
		}

		public override ZGuid ADH_OA_AddressOverride
		{
			get => base.ADH_OA_AddressOverride;
			set
			{
				base.ADH_OA_AddressOverride = value;

				CollectInfoForAddressNotRelatedToOrg();
			}
		}

		void CollectInfoForAddressNotRelatedToOrg()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderCallStack, () =>
			{
				if (AddressOverride != null && ADH_OH_Organisation != AddressOverride.Header.PK)
				{
					var parentTransactionHeader = ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().SelectMany(x => x.TransactionHeaders).FirstOrDefault();

					return System.FormattableString.Invariant($@"Compliance Document Organization = {Organisation.OH_Code}, Compliance Document Organization for Address = {AddressOverride.Header.OH_Code}, Parent Organization = {parentTransactionHeader?.Header.OH_Code ?? string.Empty}, Parent Organization for Address = {parentTransactionHeader?.InvoiceAddressOverride.Header.OH_Code ?? string.Empty}
{System.Environment.StackTrace}");
				}
				return null;
			});
		}

		#endregion

		#region OrganisationAddressWithContact

		ZAddressWithContact fOrganisationAddressWithContact;
		public ZAddressWithContact OrganisationAddressWithContact => fOrganisationAddressWithContact ?? (fOrganisationAddressWithContact = GetOrganisationAddressWithContact());

		ZAddressWithContact GetOrganisationAddressWithContact()
		{
			return new ZAddressWithContact(DisplayInvoiceContactOverrideInfo, DisplayInvoiceAddressOverrideForAddressControlInfo)
			{
				GetDefaultAddress = GetDefaultAddress
			};
		}

		[List("Lookups.Headers")]
		public ZGuid DisplayInvoiceAddressOverrideForAddressControl
		{
			get
			{
				return DisplayInvoiceAddressOverride;
			}
		}

		[List("DisplayInvoiceAddressOverrides")]
		public ZGuid DisplayInvoiceAddressOverride
		{
			get { return ADH_OA_AddressOverride.IsValid ? ADH_OA_AddressOverride : ZGuid.Empty; }
		}

		[List("DisplayInvoiceContactOverrides")]
		public ZGuid DisplayInvoiceContactOverride
		{
			get { return ADH_OC_ContactOverride.IsValid ? ADH_OC_ContactOverride : ZGuid.Empty; }
			set { ADH_OC_ContactOverride = value; }
		}

		public OrgContactDependentCollection DisplayInvoiceContactOverrides
		{
			get
			{
				OrgContactDependentCollection fContacts = new OrgContactDependentCollection(Factory);
				OrgHeader parent = Factory.Load<OrgHeader>(ADH_OH_Organisation);
				if (parent != null)
				{
					ZQuery filter = new ZQuery(OrgContactSchema.OC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					fContacts = new OrgContactDependentCollection(parent, filter);
					fContacts.Load();
				}
				return fContacts;
			}
		}

		public OrgAddressDependentCollection DisplayInvoiceAddressOverrides
		{
			get
			{
				OrgAddressDependentCollection fAddresses = new OrgAddressDependentCollection(Factory);
				OrgHeader parent = Factory.Load<OrgHeader>(ADH_OH_Organisation);
				if (parent != null)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					fAddresses = new OrgAddressDependentCollection(parent, filter);
					fAddresses.Load();
				}
				return fAddresses;
			}
		}

		public ZPropertyInfo DisplayInvoiceContactOverrideInfo => GetWrappedZPropertyInfo(Schema.DisplayInvoiceContactOverride, x => ADH_OC_ContactOverrideInfo);

		public ZPropertyInfo DisplayInvoiceAddressOverrideForAddressControlInfo => GetWrappedZPropertyInfo(Schema.DisplayInvoiceAddressOverrideForAddressControl, x => ADH_OA_AddressOverrideInfo);

		protected ZGuid GetDefaultAddress(IOrgHeader orgHeader)
		{
			OrgAddress defaultAddress = null;
			OrgHeader header = orgHeader as OrgHeader;

			if (header != null)
			{
				defaultAddress = header.AddressForSendingAPDocuments;
			}

			return defaultAddress == null ? ZGuid.Empty : defaultAddress.PK;
		}

		#endregion

		#region Workflow

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		ProcessTaskCollection workflowItems;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new AccComplianceDocumentProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get
			{
				if (ADH_Ledger == LedgerTypes.AccountsPayable)
				{
					return WorkflowDescriptors.APComplianceDocumentCode;
				}
				else
				{
					return WorkflowDescriptors.ARComplianceDocumentCode;
				}
			}
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria() => new ColumnValueRanker();

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo fDocManagerInfo;
		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (fDocManagerInfo == null)
				{
					fDocManagerInfo = GetNewDocManagerInfo();
				}

				return fDocManagerInfo;
			}
		}

		protected virtual DocManagerInfo GetNewDocManagerInfo() => new DocManagerInfo(this, Core.Constants.DocManagerCodes.ComplianceDocument);

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region Default Value

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ADH_DocumentDate = ZDateTime.Now;
			ADH_DocumentType = "VAT";
			ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Added;
			ADH_GC_Company = GlbCompany.CurrentCompany.PK;

#if DEBUG
			ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
#endif
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public DocumentSupporter DocumentSupporter => new AccComplianceDocumentSupporter(this);

		#endregion

		#region IComplianceDocumentHeaderDetail

		ZString IComplianceDocumentHeaderDetail.DocumentSubType => ADH_ComplianceSubType;

		ZString IComplianceDocumentHeaderDetail.DocumentNumber => ADH_DocumentNumber;

		ZDateTime IComplianceDocumentHeaderDetail.DocumentDate => ADH_DocumentDate;

		ZInt IComplianceDocumentHeaderDetail.DocumentReportingPeriod => ADH_ReportingPeriod;

		ZString IComplianceDocumentHeaderDetail.DocumentLedger => ADH_Ledger;

		ZGuid IComplianceDocumentHeaderDetail.DocumentPK => PK;

		ZGuid IComplianceDocumentHeaderDetail.DocumentCompany => Company?.PK ?? GlbCompany.CurrentCompany.PK;

		ZString IComplianceDocumentHeaderDetail.DocumentTransactionType => ADH_TransactionType;

		#endregion

		#region Save & Delete

		public override void Delete()
		{
			if (IsAdded)
			{
				WorkflowItems.RemoveAndDeleteAll();

				DeleteRelatedBusinessObjects();

				base.Delete();
			}
			else
			{
				throw new CannotDeleteException("This compliance document header was allocated and cannot be deleted.");
			}
		}

		protected void DeleteRelatedBusinessObjects()
		{
			ComplianceDocumentLines.ForEach(x => (x as AccComplianceDocumentLine).ComplianceDocumentPivots.DeleteAll());
			ComplianceDocumentLines.DeleteAll();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (!ADH_DocumentNumber.IsEmpty && IsAdded && !IsDeleting)
			{
				SetInternalReference();

				ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.NumberSet;
			}

			if (IsInDatabase && DisplayInvoiceContactOverrideInfo.HasChanges)
			{
				var originalOrgContact = Factory.Load<OrgContact>((ZGuid)DisplayInvoiceContactOverrideInfo.OriginalValue);
				Logs.AddNew(Events.InvoiceContactOverride, ZString.Format("Invoice contact override from '{0}' to '{1}'", originalOrgContact?.Name, ContactOverride?.Name));
			}

			QueueForComplianceReports();

			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();
		}

		protected virtual ZString InternalReference { get; }

		public virtual bool IsEligibleToCreateEInvoicingTransactionPivot { get; }

		public void SetInternalReference()
		{
			if (ADH_InternalReference.IsEmpty)
			{
				ADH_InternalReference = InternalReference;
			}
		}

		#endregion

		#region CheckVoid

		public virtual ZString CheckCanVoid()
		{
			if (IsVoided)
			{
				return Res.GetString("32F8BBF5-CC74-4DFA-BD64-D430D08F5C86", "This compliance document is already voided.");
			}

			if (IsFinalised && (ADH_Ledger == LedgerTypes.AccountsPayable || !AccountingMasterFilesRegistry.Instance.EnableFinalisedComplianceDocumentToBeSpecialVoided.Value))
			{
				return Res.GetString("64E7EEB6-EFED-45CD-AA1A-559ECF4FF032", "This compliance document is already finalized.");
			}

			var invoicesSql = new ZQuery(AccTransactionHeaderSchema.PK, TransactionHeaders.Select(x => x.PK));
			var invoicingBases = Factory.Load<InvoicingBase>(invoicesSql);
			var shouldPreventVoidAmendingInvoiceWithCreditNote = ((ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceDocumentVoidingProvider>)?.Get())?.ShouldPreventVoidAmendingInvoiceWithCreditNote(invoicingBases) ?? false;
			if (shouldPreventVoidAmendingInvoiceWithCreditNote)
			{
				return Res.GetString("D400644F-0A55-47CC-B193-152A65A09E89", "This record cannot be voided as a credit note has been posted.");
			}

			return ZString.Empty;
		}

		#endregion

		#region Document Status Related

		public bool IsAdded => ADH_DocumentStatus == Core.Constants.ComplianceDocumentStatus.Added;

		public bool IsAllocated => ADH_DocumentStatus == Core.Constants.ComplianceDocumentStatus.NumberSet;

		public bool IsVoided => ADH_DocumentStatus == Core.Constants.ComplianceDocumentStatus.Voided;

		public bool IsFinalised
		{
			get
			{
				var finalizeLogReference = (NoResString)"Compliance Document Finalized with Compliance Report";
				return ADH_DocumentStatus == Core.Constants.ComplianceDocumentStatus.Finalised || (this.IsVoided && Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference.Contains(finalizeLogReference)));
			}
		}

		#endregion

		#region  EInvoicing Memebers

		public ZString EInvoicingStatus => EInvoicingTransactionPivot?.AIP_Status ?? ZString.Empty;

		public ZPropertyInfo EInvoicingStatusInfo => GetZPropertyInfo(Schema.EInvoicingStatus);

		public ZString EInvoicingError => EInvoicingTransactionPivot?.AIP_ErrorDescription ?? ZString.Empty;

		public ZPropertyInfo EInvoicingErrorInfo => GetZPropertyInfo(Schema.EInvoicingError);

		public ZDateTime EInvoicingLastResponseReceivedUtc => EInvoicingTransactionPivot?.AIP_LastResponseReceivedUtc ?? ZDateTime.Empty;

		public ZPropertyInfo EInvoicingLastResponseReceivedUtcInfo => GetZPropertyInfo(Schema.EInvoicingLastResponseReceivedUtc);

		public ZDateTime EInvoicingLastSentTimeUtc => EInvoicingTransactionPivot?.AIP_LastSentTimeUtc ?? ZDateTime.Empty;

		public ZPropertyInfo EInvoicingLastSentTimeUtcInfo => GetZPropertyInfo(Schema.EInvoicingLastSentTimeUtc);

		public ZString EInvoicingBatchNumber => EInvoicingBatch?.AIB_BatchNumber.ToString() ?? ZString.Empty;

		public ZPropertyInfo EInvoicingBatchNumberInfo => GetZPropertyInfo(Schema.EInvoicingBatchNumber);

		public ZString EInvoicingBatchStatus => EInvoicingBatch?.AIB_Status.ToString() ?? ZString.Empty;

		public ZPropertyInfo EInvoicingBatchStatusInfo => GetZPropertyInfo(Schema.EInvoicingBatchStatus);

		protected AccEInvoicingBatch EInvoicingBatch
		{
			get
			{
				if (eInvoicingBatch == null && EInvoicingTransactionPivot != null)
				{
					eInvoicingBatch = Factory.Load<AccEInvoicingBatch>(EInvoicingTransactionPivot.AIP_AIB);
				}
				return eInvoicingBatch;
			}
		}
		AccEInvoicingBatch eInvoicingBatch;

		protected AccEInvoicingTransactionPivot EInvoicingTransactionPivot
		{
			get
			{
				if (eInvoicingTransactionPivot == null)
				{
					var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, PK);
					query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccComplianceDocumentHeaderSchema.Constants.Prefix);

					eInvoicingTransactionPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(query);
				}

				return eInvoicingTransactionPivot;
			}
		}
		AccEInvoicingTransactionPivot eInvoicingTransactionPivot;

		#endregion

		#region ComplianceDocumentHeader
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "void log reference information")]
		public const string VoidLogReference = "Compliance Document Voided";

		public override ZString ADH_DocumentNumber
		{
			get => base.ADH_DocumentNumber;
			set
			{
				base.ADH_DocumentNumber = value;
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.Taiwan && ADH_Ledger == LedgerTypes.AccountsReceivable && ADH_ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE)
				{
					var random = new Random(unchecked((int)ZDateTime.Now.Ticks));
					ADH_BarCode = (random.Next() % 10000).ToString("D4", CultureInfo.InvariantCulture);
				}
			}
		}

		TransactionLineSummaryCollection fLinesSummaries;
		public TransactionLineSummaryCollection LineSummaries
		{
			get
			{
				if (fLinesSummaries == null)
				{
					var query = new ZQuery(AccTransactionHeaderSchema.PK, TransactionHeaders.Select(x => x.PK));
					InvoicingBaseCollection collection = new InvoicingBaseCollection(Factory, query);
					collection.Load();
					fLinesSummaries = new TransactionLineSummaryCollection(Factory);
					foreach (InvoicingBase invoice in collection)
					{
						foreach (InvoicingLineBase line in invoice.GetComplianceRelatedLines(this))
						{
							fLinesSummaries.Add(new TransactionLineSummary(line));
						}
					}
				}

				return fLinesSummaries;
			}
		}

		ComplianceSubTypeRule ComplianceSubTypeRule
		{
			get
			{
				if (complianceSubTypeRule == null)
				{
					complianceSubTypeRule = new ComplianceSubTypeRule(Factory, this);
				}
				return complianceSubTypeRule;
			}
		}
		ComplianceSubTypeRule complianceSubTypeRule;

		#region SetProperties

		public virtual void SetComplianceSubType()
		{
			if (ADH_ComplianceSubType.IsEmpty)
			{
				ADH_ComplianceSubType = ComplianceSubTypeRule.GetMatchingComplianceSubType();
			}
		}

		public void SetComplianceDocumentHeaderAddress(ZBool isOverrideOrganization)
		{
			var orgCusCode = Organisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(ADH_DocumentType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (orgCusCode != null && orgCusCode.OK_OA_PremisesAddress.IsValid)
			{
				ADH_OA_AddressOverride = orgCusCode.OK_OA_PremisesAddress;
			}
			else if (!isOverrideOrganization)
			{
				var addressPKs = TransactionHeaders.Cast<AccTransactionHeader>().Select(x => x.AH_OA_InvoiceAddressOverride).Distinct().ToArray();
				if (addressPKs.Length == 1)
				{
					ADH_OA_AddressOverride = addressPKs[0];
				}
			}

			if (ADH_OA_AddressOverride.IsEmpty)
			{
				var address = ADH_Ledger == LedgerTypes.AccountsReceivable ? Organisation.AddressForSendingARDocuments : Organisation.AddressForSendingAPDocuments;
				if (address != null)
				{
					ADH_OA_AddressOverride = address.PK;
				}
			}
		}

		public virtual void SetComplianceSequenceBook() { }

		public virtual void SetComplianceDocumentNumber() { }

		#endregion

		bool IsVoidingAR => ADH_DocumentStatus == Core.Constants.ComplianceDocumentStatus.Voided && ADH_Ledger == LedgerTypes.AccountsReceivable && (!IsInDatabase || ADH_DocumentStatusInfo.HasChanges);

		public string GetComplianceBookMenuName()
		{
			AccComplianceSequence sequence = ComplianceBook;
			if (sequence != null)
			{
				if (!sequence.XD_SU_MenuItem.IsEmpty)
				{
					StmMenuItem menu = Factory.Load<StmMenuItem>(sequence.XD_SU_MenuItem);
					if (menu != null)
					{
						return menu.SU_MenuName;
					}
					else
					{
						throw new GovernmentInvoiceMenuNotFoundException(ADH_ComplianceSubType);
					}
				}
				else
				{
					throw new ComplianceSequenceHasNoDocumentMenuDefinedException();
				}
			}
			return ZString.Empty;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (IsFinalised || IsVoided)
			{
				ReadOnly = true;
				ComplianceDocumentLines.SetReadOnlyIncludingChildren(true);
			}
		}

		#region Delete

		public override bool CanDelete => IsAdded;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("359E34ED-43BA-4923-BCED-4AE33E1015FE", "This record can not be deleted, because it's document status is not ADD.");

		#endregion

		#region Void

		public void Void()
		{
			if (!IsFinalised)
			{
				DeleteComplianceReportQueue();
			}

			ADH_DocumentStatus = IsFinalised ? Core.Constants.ComplianceDocumentStatus.FinalisedSpecialVoided : Core.Constants.ComplianceDocumentStatus.Voided;
			ReloadComplianceDocumentLines();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(Events.EditedARecord, VoidLogReference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			EInvoicingTransactionPivot?.Requeue();
		}

		#endregion

		public void Finalise(AccComplianceReport complianceReport)
		{
			if (!IsFinalised)
			{
				ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Finalised;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Compliance Document Finalized with Compliance Report <{0}>", complianceReport.ACR_ReportType));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#region Compliance Reports

		void QueueForComplianceReports()
		{
			if (!ADH_DocumentNumber.IsEmpty)
			{
				if (!IsInDatabase || string.IsNullOrWhiteSpace(ADH_DocumentNumberInfo.OriginalValue.ToString()) || IsVoidingAR)
				{
					var reports = ComplianceReportTransactionQueueingHelper.GetComplianceReportsOfCompanyCountry();
					foreach (var report in reports)
					{
						var matchesSettings = ComplianceReportTransactionQueueingHelper.GetMatchingReportComplianceRuleSetting(ComplianceSubTypeRule, report) != null;
						if (matchesSettings && (!IsVoidingAR || report.ReportLineOrdering == ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber))
						{
							var subCode = ZString.Empty;
							if (report.ReportLineOrdering == ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber)
							{
								subCode = ComplianceDocumentHelper.GetFormatCode(ADH_ComplianceSubType, ADH_Ledger);
							}

							QueueDocumentHeaderForComplianceReport(report.ReportCode, subCode);
						}
					}
				}
			}
		}

		IEnumerable<AccComplianceReport> GetGeneratedComplianceReports()
		{
			var result = Enumerable.Empty<AccComplianceReport>();

			var documentQuery = new ZQuery(AccComplianceReportTransactionPivotSchema.ACL_ParentTableCode, AccComplianceDocumentHeaderSchema.Constants.Prefix);
			documentQuery.AddToFilter(AccComplianceReportTransactionPivotSchema.ACL_ParentID, PK);

			var sql = string.Format(CultureInfo.InvariantCulture, @"SELECT DISTINCT {0} FROM {1} {2}",
				AccComplianceReportTransactionPivotSchema.Constants.ACL_ACR_Report,
				AccComplianceReportTransactionPivotSchema.Constants.TableName,
				documentQuery.GetAsWhereClause(combineFilterAndParams: false));
			var reportPKs = new DynamicBusinessObjectCollection(Factory);
			reportPKs.Load(sql, documentQuery.Params);

			var pks = reportPKs.Select(x => (ZGuid)x[AccComplianceReportTransactionPivotSchema.Constants.ACL_ACR_Report]);
			if (pks.Any())
			{
				result = Factory.Load<AccComplianceReport>(new ZQuery(AccComplianceReportSchema.PK, pks));
			}

			return result;
		}

		void DeleteComplianceReportQueue()
		{
			DeleteExistingQueueEntries();

			foreach (var report in GetGeneratedComplianceReports())
			{
				report.Invalidate();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteExistingQueueEntries()
		{
			var reportTypes = ComplianceReportTransactionQueueingHelper
				.GetComplianceReportsOfCompanyCountry()
				.Where(x => x.ReportBaseTablePrefix == AccComplianceDocumentHeaderSchema.Constants.Prefix)
				.Select(x => x.ReportCode)
				.Distinct();
			if (!reportTypes.Any())
			{
				return;
			}

			// Performance: query adds additional columns to ensure clustered index seek.
			var reportCodesParameterInline = string.Join(",", reportTypes.Select(x => "'" + x + "'"));
			var sql = FormattableString.Invariant($@"
DELETE {AccTransactionComplianceReportQueueSchema.Constants.SqlSchemaName}.{AccTransactionComplianceReportQueueSchema.Constants.TableName}
WHERE {AccTransactionComplianceReportQueueSchema.Constants.ACQ_GC_Company}    = @CompanyPK
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportType}      IN ({reportCodesParameterInline})
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentTableCode} = '{AccComplianceDocumentHeaderSchema.Constants.Prefix}'
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentID}        = @ParentPK
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date}            IN (@CurrentDate, @OriginalDate)
");

			var command = ((IDbConnected)Factory).Connection.Command(sql);  // No BizO generated for this table. There is no sense to generate business objects for queue entry and pivot as they are not bound to GUI
			command.AddParameterBasedOnDbColumn("@CompanyPK", ADH_GC_Company.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			command.AddParameterBasedOnDbColumn("@ParentPK", PK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_ParentID);

			var currentAcqDate = DateForComplianceReport((pi) => pi.Value).ToDateTime().Date;
			var originalAcqDate = DateForComplianceReport((pi) => pi.OriginalValue).ToDateTime().Date;
			command.AddParameterBasedOnDbColumn("@CurrentDate", currentAcqDate, AccTransactionComplianceReportQueueSchema.ACQ_Date);
			command.AddParameterBasedOnDbColumn("@OriginalDate", originalAcqDate, AccTransactionComplianceReportQueueSchema.ACQ_Date);
			command.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void QueueDocumentHeaderForComplianceReport(ZString reportCode, ZString reportSubCode)
		{
			var sql = string.Format(CultureInfo.CurrentCulture, "INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}) " +
				"VALUES (newid(), @CompanyPK, @ReportType, @ParentTableCode, @ParentPK, @Date, @ReportSubCode)",
				AccTransactionComplianceReportQueueSchema.Constants.TableName,
				AccTransactionComplianceReportQueueSchema.Constants.PK,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_GC_Company,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportType,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentTableCode,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentID,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportSubCode);

			var command = ((IDbConnected)Factory).Connection.Command(sql);  // No BizO generated for this table. There is no sense to generate business objects for queue entry and pivot as they are not bound to GUI
			command.AddParameterBasedOnDbColumn("@CompanyPK", Environment.Env.CurrentCompany.PK, AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			command.AddParameterBasedOnDbColumn("@ReportType", reportCode.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ReportType);
			command.AddParameterBasedOnDbColumn("@ParentTableCode", AccComplianceDocumentHeaderSchema.Constants.Prefix, AccTransactionComplianceReportQueueSchema.ACQ_ParentTableCode);
			command.AddParameterBasedOnDbColumn("@ParentPK", PK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_ParentID);
			command.AddParameterBasedOnDbColumn("@ReportSubCode", reportSubCode.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ReportSubCode);

			var acqDate = DateForComplianceReport();
			command.AddParameterBasedOnDbColumn("@Date", acqDate.ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date);

			command.ExecuteNonQuery();
		}

		ZDate DateForComplianceReport(Func<ZPropertyInfo, object> currentOrOriginalGetter = null)
		{
			var getValue = currentOrOriginalGetter ?? GetCurrentValue;
			var firstDayOfPeriod = PeriodCalculator.GetFirstDayForPeriod((ZInt)getValue(ADH_ReportingPeriodInfo));
			var acqDate = ADH_Ledger == LedgerTypes.AccountsPayable && firstDayOfPeriod.IsValid
						? firstDayOfPeriod
						: (ZDateTime)getValue(ADH_DocumentDateInfo);
			return acqDate.Date;

			object GetCurrentValue(ZPropertyInfo propInfo) => propInfo.Value;
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}

		#endregion

		#region IEvaluateComplianceRule

		ZString IEvaluateComplianceRule.Ledger => ADH_Ledger;

		ZString IEvaluateComplianceRule.TransactionType => ADH_TransactionType;

		ZString IEvaluateComplianceRule.ComplianceSubType => ADH_ComplianceSubType;

		ZBool IEvaluateComplianceRule.IsDisbursementOrFinal => ZBool.False;

		ZBool IEvaluateComplianceRule.IsSelfBillingInvoice => ZBool.False;

		ZBool IEvaluateComplianceRule.IsAmendingTransaction => ZBool.False;

		ZBool IEvaluateComplianceRule.IsReversalTransaction => ZBool.False;

		OrgHeader IEvaluateComplianceRule.Header => base.Organisation;

		GlbCompany IEvaluateComplianceRule.Company => base.Company;

		[DecimalPlaces(nameof(ADH_Calc_RXDecimals))]
		ZDecimal IEvaluateComplianceRule.LocalTotalAmount => TotalAmount;

		IEnumerable<AccTransactionLines> IEvaluateComplianceRule.Lines
		{
			get
			{
				if (lines == null)
				{
					var invoiceLines = new List<AccTransactionLines>();
					foreach (AccComplianceDocumentLine documentLine in ComplianceDocumentLines)
					{
						invoiceLines.AddRange(documentLine.TransactionLines.ToArray<AccTransactionLines>());
					}
					lines = invoiceLines.ToArray();
				}

				return lines;
			}
		}
		AccTransactionLines[] lines;

		ZBool IEvaluateComplianceRule.EmptyLedgerMatchesAll => true;

		ZBool IEvaluateComplianceRule.EmptyTransactionTypeMatchesAll => true;

		IEnumerable<AccTaxTransaction> IEvaluateComplianceRule.TaxTransactions => Array.Empty<AccTaxTransaction>();

		#endregion

		#region IComplianceRuleParentTransaction

		IEvaluateComplianceRule IComplianceRuleParentTransaction.ParentTransaction => ADH_TransactionType == TransactionTypes.CreditNote ? OriginalComplianceDocumentHeader : null;

		#endregion

		#endregion

		#region IHandleDeleteError Members

		bool IHandleDeleteError.RollbackAfterDeleteError
		{
			get { return IsInDatabase && IsDeleted; }
		}

		bool IHandleDeleteError.RebindAfterDeleteError
		{
			get { return false; }
		}

		bool IHandleDeleteError.DisableFormOnDeleteConcurrencyError
		{
			get { return !((IHandleDeleteError)this).RollbackAfterDeleteError; }
		}

		#endregion

		public ZString CompanyVATRegistrationNum
		{
			get
			{
				return Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Company.GC_RN_NKCountryCode)?.OK_CustomsRegNo ?? ZString.Empty;
			}
		}

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return new AccComplianceDocumentHeaderCriticalValidation(this); }
		}
	}
}
