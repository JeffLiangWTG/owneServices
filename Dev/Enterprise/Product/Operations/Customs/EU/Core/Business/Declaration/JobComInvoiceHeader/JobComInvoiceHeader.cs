using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceHeader : TypeSafeJobComInvoiceHeader
		, ICanBeImportOrExport
		, Integration.Customs.EU.IJobComInvoiceHeader
		, ICusSupportingInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, IAdditionalInfosProviderWithValidationDecider
		, ISupportingDocumentsProviderWithValidationDecider
		, IPreviousDocumentsProviderWithValidationDecider
		, IChargeHolder
		, ISupportMultipleResourceStringData
		, IEffectiveValueManagerSupporter
		, IUcc6ValueProvider
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobComInvoiceHeader.Schema
		{
			public const string InvoicerOrgPK = nameof(JobComInvoiceHeader.InvoicerOrgPK);
			public const string RelatedIndicator = nameof(JobComInvoiceHeader.RelatedIndicator);
			public const string RelatedIndicator2 = nameof(JobComInvoiceHeader.RelatedIndicator2);
			public const string RelatedIndicator3 = nameof(JobComInvoiceHeader.RelatedIndicator3);
			public const string RelatedIndicator4 = nameof(JobComInvoiceHeader.RelatedIndicator4);
		}

		public new static readonly JobComInvoiceHeaderTypeDecider TypeDecider = new JobComInvoiceHeaderTypeDecider();

		public new JobComInvoiceLineViewCollection InvoiceLines => (JobComInvoiceLineViewCollection)base.InvoiceLines;

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				return declaration.GetJobComInvoiceHeaderValidation(this) ?? new JobComInvoiceHeaderValidation(this);
			}
			else
			{
				return new JobComInvoiceHeaderValidation(this);
			}
		}

		[ResourceStringData("A4932102-434C-43A9-9798-68F97A043A36", Caption = "Supplier Address", MediumCaption = "Supplier Addr.", ShortCaption = "Supp. Addr.", FullDescription = SupplierFullDescription)]
		public override ZGuid JZ_OA_SupplierAddress { get => base.JZ_OA_SupplierAddress; set => base.JZ_OA_SupplierAddress = value; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string SupplierFullDescription = "This address determines which CID code is used in the message for each invoice";

		[ResourceStringData("1E7B8A7B-CBAE-471A-BC79-0A0A553D04DA", Caption = "Invoice Currency", MediumCaption = "Inv. Currency", ShortCaption = "Currency", FullDescription = "[14 05 000 000] Invoice Currency", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("33C74B68-8CF8-49CA-B8CC-E5D5F3473F72", Caption = "Invoice Currency", MediumCaption = "Inv. Currency", ShortCaption = "Currency", FullDescription = "[14 05 000 000] Invoice Currency", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString JZ_RX_NKInvoice_Currency { get => base.JZ_RX_NKInvoice_Currency; set => base.JZ_RX_NKInvoice_Currency = value; }

		[ResourceStringData("EUCustomsSupplierHeaderUserControl|DA385D73-B96D-498B-801F-FE56E16F72F2", Caption = "Delivery Text", ShortCaption = "Delivery")]
		public override ZString JZ_AdditionalTerms { get => base.JZ_AdditionalTerms; set => base.JZ_AdditionalTerms = value; }

		[ResourceStringData("EUCustomsSupplierHeaderUserControl|365e87c9-60c1-45f6-97e9-0f06ef9b3ff5", Caption = "[22] Inv. Amount", FullDescription = "The total amount of the invoice and its currency.", MultipleKey = JobDeclaration.CaptionKeySAD)]
		[ResourceStringData("4CFA4FD2-0DAA-41AD-933A-AB77FFDABD00", Caption = "[4/11] Invoice Amount", MediumCaption = "[4/11] Inv. Amount", ShortCaption = "Inv. Amount", MultipleKey = JobDeclaration.CaptionKeyUCC)]
		[ResourceStringData("182F6ED6-99E7-42E1-956B-812D515FF549", Caption = "Invoice Amount", MediumCaption = "Inv. Amount", ShortCaption = "Inv. Amount", FullDescription = "[14 06 000 000] Total Amount Invoiced and [14 05 000 000] Invoice Currency", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[ResourceStringData("448E2031-B6BA-4BD8-8EFC-0DB2F25800DE", Caption = "Invoice Amount", MediumCaption = "Inv. Amount", ShortCaption = "Inv. Amount", FullDescription = "[14 06 000 000] Total Amount Invoiced and [14 05 000 000] Invoice Currency", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZDecimal JZ_InvoiceAmount
		{
			get => base.JZ_InvoiceAmount;
			set => base.JZ_InvoiceAmount = value;
		}

		[ResourceStringData("FF3B9E9F-2108-4CEF-8F07-EB42890D7DD3", Caption = "Exchange Rate", MediumCaption = "Exch. Rate", ShortCaption = "Ex. Rate", MultipleKey = JobDeclaration.CaptionKeySAD)]
		[ResourceStringData("64932091-4D0E-4DA8-A16B-26467790B998", Caption = "[4/15] Exchange Rate", MediumCaption = "[4/15] Exch. Rate", ShortCaption = "Exch. Rate", MultipleKey = JobDeclaration.CaptionKeyUCC)]
		[ResourceStringData("0760D598-E7D8-4B98-BB7C-8877C4B8E5D0", Caption = "Exchange Rate", MediumCaption = "Exch. Rate", ShortCaption = "Ex. Rate", FullDescription = "[14 09 000 000] Exchange Rate", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[ResourceStringData("6FE51E5B-5B01-4E3E-BB3B-7336DDCC6D7E", Caption = "Exchange Rate", MediumCaption = "Exch. Rate", ShortCaption = "Ex. Rate", FullDescription = "[14 09 000 000] Exchange Rate", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZDecimal JZ_InvoiceCurrExRate
		{
			get => base.JZ_InvoiceCurrExRate;
			set => base.JZ_InvoiceCurrExRate = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationCodeList))]
		[ResourceStringData("6B7D5CBF-6EB0-4C0E-8FB0-0C7A9872344E", Caption = "[24] Tran. Nature", FullDescription = "The nature of the transaction.", MultipleKey = JobDeclaration.CaptionKeySAD)]
		[ResourceStringData("5909155C-D494-48CE-9C1D-78FAF88E756E", Caption = "[8/5] Transaction Nature", MediumCaption = "[8/5] Tran. Nature", ShortCaption = "Tran. Nature", MultipleKey = JobDeclaration.CaptionKeyUCC)]
		[ResourceStringData("DABBF7E3-07E4-494B-B497-A6220B6D3D55", Caption = "Nature of Transaction", FullDescription = "[99 05 000 000]  Nature of transaction", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString JZ_ValuationCode
		{
			get => base.JZ_ValuationCode;
			set => base.JZ_ValuationCode = value;
		}

		public override ZGuid JZ_OH_Buyer
		{
			get => base.JZ_OH_Buyer;
			set
			{
				base.JZ_OH_Buyer = value;
				MarkDeclarationAsNeedingValidation();
				MarkJobDocAddressesAsNeedingValidation();
			}
		}

		public override ZGuid JZ_JE
		{
			get => base.JZ_JE;
			set
			{
				var oldValue = JZ_JE;
				base.JZ_JE = value;
				if (!IsCopying && oldValue != JZ_JE)
				{
					JobDeclaration?.MarkFeesAsNeedingValidation();
				}
			}
		}

		void MarkJobDocAddressesAsNeedingValidation()
		{
			if (JobDeclaration != null)
			{
				if (JobDeclaration.ImporterDocumentaryAddress != null)
				{
					JobDeclaration.ImporterDocumentaryAddress.MarkAsNeedingValidation();
				}
				if (JobDeclaration.SupplierDocumentaryAddress != null)
				{
					JobDeclaration.SupplierDocumentaryAddress.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("ADB9F93C-D1E6-4D8B-B4F4-1D4F1285BA02", Caption = "Supplier", MultipleKey = JobDeclaration.CaptionKeySAD)]
		[ResourceStringData("EAE81F62-FE1D-474A-8C95-4FD3C46EE840", Caption = "[3/1] Supplier", ShortCaption = "Supplier", MultipleKey = JobDeclaration.CaptionKeyUCC)]
		public override ZGuid JZ_OH_Supplier
		{
			get => base.JZ_OH_Supplier;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier))
				{
					var oldValue = JZ_OH_Supplier;
					base.JZ_OH_Supplier = value;
					MarkDeclarationAsNeedingValidation();
					MarkJobDocAddressesAsNeedingValidation();
					if (!IsCopying && JZ_OH_Supplier != oldValue)
					{
						if (!JZ_OH_Supplier.IsEmpty)
						{
							AddDefaultSupportingDocumentIfNecessary();
						}
					}
				}
			}
		}

		void MarkDeclarationAsNeedingValidation()
		{
			if (JobDeclaration != null)
			{
				JobDeclaration.MarkAsNeedingValidation();
			}
		}

		public override ZString JZ_InvoiceNumber
		{
			get => base.JZ_InvoiceNumber;
			set
			{
				var oldValue = JZ_InvoiceNumber;
				base.JZ_InvoiceNumber = value;
				if (!IsCopying && JZ_InvoiceNumber != oldValue)
				{
					MarkDeclarationAsNeedingValidation();
					if (ShouldUpdateDefaultSupportingDocumentsOnInvoiceNumberChange())
					{
						UpdateDefaultSupportingDocuments(x => x.CSI_ReferenceNumberInfo, value);
					}
					AddDefaultSupportingDocumentIfNecessary();
				}
			}
		}

		public override ZDateTime JZ_InvoiceDate
		{
			get => base.JZ_InvoiceDate;
			set
			{
				var oldValue = JZ_InvoiceDate;
				base.JZ_InvoiceDate = value;
				if (!IsCopying && JZ_InvoiceDate != oldValue)
				{
					UpdateDefaultSupportingDocuments(x => x.CSI_DateOfIssueInfo, value);
					AddDefaultSupportingDocumentIfNecessary();
				}
			}
		}

		protected bool ShouldUpdateDefaultSupportingDocumentsOnInvoiceNumberChange() =>
			SupportingDocuments
				.Cast<SupportingDocument>()
				.Count(x => ApplicableForUpdateDefaultSupportingDocumentsCodes.Contains(x.CSI_Code)) == 1;

		void UpdateDefaultSupportingDocuments(Func<SupportingDocument, ZPropertyInfo> propertyInfoGetter, IZType updatedValue)
		{
			if (!updatedValue.IsEmpty && AddingSupportingDocumentAutomaticallyEnabled)
			{
				foreach (var doc in SupportingDocuments.Cast<SupportingDocument>()
					.Where(x => ApplicableForUpdateDefaultSupportingDocumentsCodes.Contains(x.CSI_Code)))
				{
					var propertyInfo = propertyInfoGetter(doc);
					if (propertyInfo.Value != updatedValue)
					{
						propertyInfo.Value = updatedValue;
					}
				}
			}
		}

		protected override void SetDefaultInvoiceDate()
		{
			//do not set JZ_InvoiceDate for EU.
		}

		#region InvoicerOrgPK

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SupplierList))]
		public ZGuid InvoicerOrgPK
		{
			get => JZ_OA_InvoicerAddress_ZAddress.OrgPK;
			set => JZ_OA_InvoicerAddress_ZAddress.OrgPK = value;
		}

		public ZPropertyInfo InvoicerOrgPKInfo => GetWrappedZPropertyInfo(Schema.InvoicerOrgPK, x => JZ_OA_InvoicerAddress_ZAddress.OrgPKInfo);

		[List(nameof(JZ_OA_InvoicerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_InvoicerAddress
		{
			get => base.JZ_OA_InvoicerAddress;
			set => base.JZ_OA_InvoicerAddress = value;
		}

		#endregion

		#region MultiLineAddInfo Collections
		#region public SupportingDocumentCollection SupportingDocuments
		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments => supportingDocuments ?? (supportingDocuments = GetSupportingDocuments());
		SupportingDocumentCollection supportingDocuments;

		SupportingDocumentCollection GetSupportingDocuments()
		{
			var result = CreateNewSupportingDocumentCollection();

			if (MaxSupportingDocuments != -1)
			{
				result.EnableMaxCountValidationWithMessageError(MaxSupportingDocuments, warnAtHalfway: false, SupportingDocumentsValidationMessage, GetSupportingDocumentsMaxCountReduction);
			}

			result.Load();
			RegisterEditableChildObject(result);

			return result;
		}
		public virtual int MaxSupportingDocuments => -1;
		public virtual ZString SupportingDocumentsValidationMessage { get; }
		public virtual Func<int> GetSupportingDocumentsMaxCountReduction => () => 0;

		protected virtual SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		public List<SupportingDocument> EffectiveSupportingDocuments()
		{
			var result = new List<SupportingDocument>();

			// Each Supporting Document can either be header only, header or line, or line only.
			// header or line are all currently sent at the line level so get added together for the merge key
			foreach (SupportingDocument sd in SupportingDocuments)
			{
				if (!sd.IsLineOnly)
				{
					result.Add(sd);
				}
			}

			return result;
		}

		public List<SupportingDocument> EffectiveSupportingDocumentsForLine()
		{
			var result = new List<SupportingDocument>();

			// Each Supporting Document can either be header only, header or line, or line only.
			// header or line are all currently sent at the line level so get added together for the merge key
			foreach (SupportingDocument sd in SupportingDocuments)
			{
				if (sd.IsEffectiveSupportingDocumentsForLine)
				{
					result.Add(sd);
				}
			}

			return result;
		}

		#endregion

		#region public AdditionalInfoCollection AdditionalInfos
		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos => fAdditionalInfos ?? (fAdditionalInfos = GetAdditionalInfos());
		AdditionalInfoCollection fAdditionalInfos;

		AdditionalInfoCollection GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);
		#endregion

		#region public PreviousDocumentCollection PreviousDocuments
		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments => fPreviousDocuments ?? (fPreviousDocuments = GetPreviousDocuments());
		PreviousDocumentCollection fPreviousDocuments;

		PreviousDocumentCollection GetPreviousDocuments()
		{
			var result = CreateNewPreviousDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);
		#endregion

		#endregion

		#region ICanBeImportOrExport Members

		string ICanBeImportOrExport.Level => UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
			Validation.ValidateJZ_InvoiceNumber();
			foreach (ICanBeImportOrExport line in JobComInvoiceLines)
			{
				line.ValidatePreviousDocuments();
			}
		}

		string ICanBeImportOrExport.TrueCountryCode => CountryCode;
		string ICanBeImportOrExport.DataGroupingCode => GetDefaultDataGroupingCode();

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			InvoiceLineDependentCollection collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		#endregion

		#region ICusLinkPackageSupporter

		protected override ZBool IsSupportEmptyPackType(BasePackage package)
		{
			var pack = package as Package;
			return pack != null && pack.IsEmptyPackTypeAllowed;
		}

		#endregion

		#region Descriptions

		[ChildEditable]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public InvoiceHeaderDescriptionCollection HeaderDescriptions
		{
			get
			{
				if (headerDescriptions == null)
				{
					headerDescriptions = GetNewHeaderDescriptions();

					if (IsSupportHeaderDescription)
					{
						RegisterEditableChildObject(headerDescriptions);
						headerDescriptions.Load();
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)headerDescriptions).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
					}
				}

				return headerDescriptions;
			}
		}
		InvoiceHeaderDescriptionCollection headerDescriptions;

		protected virtual InvoiceHeaderDescriptionCollection GetNewHeaderDescriptions() => new InvoiceHeaderDescriptionCollection(this);

		public ZBool IsSupportHeaderDescription => GetSupportHeaderDescriptionCore();
		protected virtual ZBool GetSupportHeaderDescriptionCore() => false;

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes() => GetCusCodeDataTypes();

		protected virtual Dictionary<ZString, Type> GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.DescriptionCode, typeof(InvoiceHeaderDescription));
			return result;
		}

		#endregion

		#region Default Added Automatically

		public virtual ZBool AddingSupportingDocumentAutomaticallyEnabled => (JobDeclaration?.Configuration.InvoiceHeaderConfiguration.SupportingDocumentsSupport(this) ?? ZBool.False) && !IsAddingSupportingDocumentAutomaticallySuspended;
		bool IsAddingSupportingDocumentAutomaticallySuspended => addingSupportingDocumentAutomaticallySuspenderIndex > 0;

		public ZBool NeedAtLeastOneInvoiceSupportingDocument => NeedAtLeastOneInvoiceSupportingDocumentCore;

		protected virtual ZBool NeedAtLeastOneInvoiceSupportingDocumentCore => !SupportingDocuments.Cast<SupportingDocument>().Any(x => x.IsCodeAnInvoiceType);

		protected virtual ZString DefaultInvoiceDocument => UniversalReferenceConstants.SupportingDocumentTypes.N380;

		protected virtual HashSet<ZString> ApplicableForUpdateDefaultSupportingDocumentsCodes { get; } =
			new()
			{
				UniversalReferenceConstants.SupportingDocumentTypes.N325,
				UniversalReferenceConstants.SupportingDocumentTypes.N380,
			};

		protected virtual void AddDefaultSupportingDocumentIfNecessary()
		{
			if (AddingSupportingDocumentAutomaticallyEnabled && IsApplicationCodeAllowedForDefaultingSupportingDocument && !JZ_InvoiceNumber.IsEmpty && !JZ_InvoiceDate.IsEmpty && NeedAtLeastOneInvoiceSupportingDocument && Supplier_Effective != null)
			{
				SupportingDocuments.AddNewInvoiceDocumentAndCopyDataFromInvoice(DefaultInvoiceDocument);
			}
		}

		bool IsApplicationCodeAllowedForDefaultingSupportingDocument => IsApplicationCodeAllowedForDefaultingSupportingDocumentCore;
		protected virtual bool IsApplicationCodeAllowedForDefaultingSupportingDocumentCore => (JobDeclaration?.JE_ApplicationCode ?? ZString.Empty) == DeclarationApplicationCodeList.Codes.Builtin;

		public IDisposable SuspendAddingSupportingDocumentAutomatically()
		{
			addingSupportingDocumentAutomaticallySuspenderIndex++;
			return new DisposableAction(() => addingSupportingDocumentAutomaticallySuspenderIndex--);
		}
		int addingSupportingDocumentAutomaticallySuspenderIndex;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new FetchStrategies.JobComInvoiceHeaderFetchStrategy(this);

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo));
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument));
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument));
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		protected override void UpdateWhenAnInvoiceIsLinkedToADeclaration(BaseJobComInvoiceLine invoiceLine, LinkedToDeclarationData linkedToDeclarationData, bool updatePartSyncManagerAndRefresh)
		{
			base.UpdateWhenAnInvoiceIsLinkedToADeclaration(invoiceLine, linkedToDeclarationData, updatePartSyncManagerAndRefresh);
			var dec = linkedToDeclarationData?.Declaration as JobDeclaration;
			var cei = dec.CustomsEntryInstructions.FirstOrDefault();
			if (dec != null && cei != null && cei.IsPersistent)
			{
				invoiceLine.JI_CEI = cei.PK;
			}
		}

		public ZBool RelatedIndicator
		{
			get => JZ_RelatedIndicator == RelatedIndicatorList.Codes.Yes;
			set
			{
				JZ_RelatedIndicator = value ? RelatedIndicatorList.Codes.Yes : RelatedIndicatorList.Codes.No;
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.RelatedIndicatorInfo.RefreshBinding());
			}
		}

		public bool RelatedIndicator_ReadOnly => !RelatedIndicator && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.RelatedIndicator);

		public ZPropertyInfo RelatedIndicatorInfo => GetWrappedZPropertyInfo(Schema.RelatedIndicator, x => JZ_RelatedIndicatorInfo);

		public ZBool RelatedIndicator2
		{
			get => ZG_RelatedIndicator2.EqualsIgnoringCase(RelatedIndicatorList.Codes.Yes);
			set
			{
				ZG_RelatedIndicator2 = value ? RelatedIndicatorList.Codes.Yes : RelatedIndicatorList.Codes.No;
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.RelatedIndicator2Info.RefreshBinding());
			}
		}

		public bool RelatedIndicator2_ReadOnly => !RelatedIndicator2 && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.RelatedIndicator2);

		public ZPropertyInfo RelatedIndicator2Info => GetWrappedZPropertyInfo(Schema.RelatedIndicator2, x => ZG_RelatedIndicator2Info);

		public ZBool RelatedIndicator3
		{
			get => ZG_RelatedIndicator3.EqualsIgnoringCase(RelatedIndicatorList.Codes.Yes);
			set
			{
				ZG_RelatedIndicator3 = value ? RelatedIndicatorList.Codes.Yes : RelatedIndicatorList.Codes.No;
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.RelatedIndicator3Info.RefreshBinding());
			}
		}

		public bool RelatedIndicator3_ReadOnly => !RelatedIndicator3 && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.RelatedIndicator3);

		public ZPropertyInfo RelatedIndicator3Info => GetWrappedZPropertyInfo(Schema.RelatedIndicator3, x => ZG_RelatedIndicator3Info);

		public ZBool RelatedIndicator4
		{
			get => ZG_RelatedIndicator4.EqualsIgnoringCase(RelatedIndicatorList.Codes.Yes);
			set
			{
				ZG_RelatedIndicator4 = value ? RelatedIndicatorList.Codes.Yes : RelatedIndicatorList.Codes.No;
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.RelatedIndicator4Info.RefreshBinding());
			}
		}

		public bool RelatedIndicator4_ReadOnly => !RelatedIndicator4 && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.RelatedIndicator4);

		public ZPropertyInfo RelatedIndicator4Info => GetWrappedZPropertyInfo(Schema.RelatedIndicator4, x => ZG_RelatedIndicator4Info);

		[ResourceStringData("B40D00F4-3B08-4393-8A91-751391FD4F4B", Caption = "Incoterm Place Code", ShortCaption = "Incoterm Place")]
		public override ZString ZG_AgreedPlaceCode { get => base.ZG_AgreedPlaceCode; set => base.ZG_AgreedPlaceCode = value; }

		[ResourceStringData("010952A7-4E19-49E9-8836-9E857C374388", Caption = "Transport Charges Method of Payment", MediumCaption = "Transp. Charges MoP", ShortCaption = "MoP")]
		public override ZString ZG_TransportChargesMethodOfPayment { get => base.ZG_TransportChargesMethodOfPayment; set => base.ZG_TransportChargesMethodOfPayment = value; }

		protected override CustomsValuationCalculator GetValuationCalculatorCore() => new EuCustomsValuationCalculator(this);

		public override ZDateTime JZ_ValuationDateOverride
		{
			get => base.JZ_ValuationDateOverride;
			set
			{
				var oldValue = JZ_ValuationDateOverride;
				base.JZ_ValuationDateOverride = value;
				if (!IsCopying && oldValue != JZ_ValuationDateOverride)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("E20E96DF-5295-486A-9EE6-A96B526346D6", Caption = "Incoterm", FullDescription = "[14 01 035 000] Incoterm Code", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("A39E13F5-499C-4ADA-B9F8-8503C0CA0383", Caption = "Incoterm", FullDescription = "[14 01 035 000] Incoterm Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString JZ_IncoTerm
		{
			get => base.JZ_IncoTerm;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_IncoTerm))
				{
					var oldValue = JZ_IncoTerm;
					base.JZ_IncoTerm = value;
					if (!IsCopying && oldValue != JZ_IncoTerm)
					{
						ClearIncoTermPlacesIfNeeded();
						MarkDeclarationAsNeedingValidation();
						AddInfo?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public bool ShouldClearIncoTermPlacesIfNeeded { get; set; } = true;

		protected virtual void ClearIncoTermPlacesIfNeeded()
		{
			if (!ShouldClearIncoTermPlacesIfNeeded)
			{
				return;
			}

			if (!AgreedPlaceCodeSupportAndVisible && AgreedPlaceCodeSupport)
			{
				ZG_AgreedPlaceCode = ZString.Empty;
			}
			if (!IncoTermsAgreedPlace.IsEmpty && JZ_IncoTerm != Core.Constants.IncoTerms.Other)
			{
				IncoTermsAgreedPlace = ZString.Empty;
			}
			if (!JZ_IncoTermPlace.IsEmpty && JZ_IncoTerm == Core.Constants.IncoTerms.Other && JobDeclaration.IsUCC6)
			{
				JZ_IncoTermPlace = ZString.Empty;
			}
		}

		public ZBool AgreedPlaceCodeSupportAndVisible => AgreedPlaceCodeSupportAndVisibleCore;

		protected virtual ZBool AgreedPlaceCodeSupportAndVisibleCore => JZ_IncoTerm != Core.Constants.IncoTerms.Other && AgreedPlaceCodeSupport;

		public bool AgreedPlaceCodeSupport => Factory.GetValue(ref agreedPlaceCodeSupport,
			() => JobDeclaration is JobDeclaration declaration && declaration.Configuration.InvoiceHeaderConfiguration.AgreedPlaceCodeSupport(declaration));
		CachedProperty<bool> agreedPlaceCodeSupport;

		[ResourceStringData("EC762749-943E-45E0-B2E1-9C1E69E7D9A2", Caption = "Agreed Place", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("E28087A8-900D-47E5-AB13-9D2CF19AAE95", Caption = "Agreed Place", MultipleKey = JobDeclaration.CaptionKeyUCC)]
		[ResourceStringData("BF3FDA41-75BC-4E23-BAB4-FCA9076F450F", Caption = "Incoterm Place")]
		[ReadOnlyMember(nameof(JZ_IncoTermPlace_ReadOnly))]
		public override ZString JZ_IncoTermPlace
		{
			get => base.JZ_IncoTermPlace;
			set => base.JZ_IncoTermPlace = value;
		}

		protected virtual bool JZ_IncoTermPlace_ReadOnly => AgreedPlaceCodeSupport && AddInfo.IsAgreedUnloco;

		[ResourceStringData("73052C99-45C4-44B3-BDDD-4CA4F19AB1CC", Caption = "Buyer")]
		[ResourceStringData("27082832-954D-44CA-8010-DE8E2E65A88F", Caption = "Buyer", FullDescription = "[13 09 016 000] Buyer Name", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("971C6154-34EC-4A1F-8421-BF011FDE3D30", Caption = "Buyer", FullDescription = "[13 09 016 000] Buyer Name", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid BuyerOrgPK { get => base.BuyerOrgPK; set => base.BuyerOrgPK = value; }

		[ResourceStringData("83969FA5-A588-41BE-8994-A84BC1F3371B", Caption = "Buyer Address")]
		[ResourceStringData("64CE8F1F-7A24-411B-BF58-66AA96E06AB2", Caption = "Buyer Address", FullDescription = "[13 09 018 000] Buyer's Address", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("4B85C44F-1612-40D7-B36E-790DD1D91A7C", Caption = "Buyer Address", FullDescription = "[13 09 018 000] Buyer's Address", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid JZ_OA_BuyerAddress { get => base.JZ_OA_BuyerAddress; set => base.JZ_OA_BuyerAddress = value; }

		[ResourceStringData("EA8AF467-0C1D-4333-A42A-4949D22D74C8", Caption = "Seller")]
		[ResourceStringData("623199BC-02FB-47D2-8270-84AE84A688A1", Caption = "Seller", FullDescription = "[13 08 016 000] Seller Name", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("8E1AA4D1-7EBB-457C-A912-F6B9C2A40CF1", Caption = "Seller", FullDescription = "[13 08 016 000] Seller Name", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid SellerOrgPK { get => base.SellerOrgPK; set => base.SellerOrgPK = value; }

		[ResourceStringData("0E7B0E30-983E-40B4-A89D-28A725657D3C", Caption = "Seller Address")]
		[ResourceStringData("933C5F38-A8EA-4BBE-968F-4FB682D996C5", Caption = "Seller Address", FullDescription = "[13 08 018 000] Seller's Address", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("5EB6902C-50BE-40AA-8FAC-569E9351C441", Caption = "Seller Address", FullDescription = "[13 08 018 000] Seller's Address", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid JZ_OA_SellerAddress { get => base.JZ_OA_SellerAddress; set => base.JZ_OA_SellerAddress = value; }

		[ResourceStringData("B17D1D97-E2A2-41EC-938C-E96375E6E209", Caption = "Exporter")]
		[ResourceStringData("C811FFA6-155C-4A08-A72D-6E5A4859E738", Caption = "Exporter", FullDescription = "[13 01 016 000] Exporter Name", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("A587FC14-AE00-4884-A45F-AF56855829B6", Caption = "Exporter", FullDescription = "[13 01 016 000] Exporter Name", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid ExporterOrgPK { get => base.ExporterOrgPK; set => base.ExporterOrgPK = value; }

		[ResourceStringData("228153CF-0FDE-40BA-8EFD-3E25FDE53C7B", Caption = "Exporter Address")]
		[ResourceStringData("9E8117B6-4C43-4BA3-BCCE-696D33A5A6B0", Caption = "Exporter Address", FullDescription = "[13 01 018 000] Exporter's Address", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("336BF9C5-7CBA-4B0D-A5FB-9C0E50738DF1", Caption = "Exporter Address", FullDescription = "[13 01 018 000] Exporter's Address", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid JZ_OA_ExporterAddress { get => base.JZ_OA_ExporterAddress; set => base.JZ_OA_ExporterAddress = value; }

		[ResourceStringData("EU.Business.Declaration.JobComInvoiceHeader|JZ_Weight", Caption = "Inv. Gross Weight")]
		[ResourceStringData("6647891A-FD5B-436F-87E1-B365A0C25AE6", Caption = "Inv. Gross Weight", FullDescription = "[18 04 001 000]  Gross mass", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZDecimal JZ_Weight
		{
			get => base.JZ_Weight;
			set => base.JZ_Weight = value;
		}

		[ResourceStringData("EU.Business.Declaration.JobComInvoiceHeader|JZ_NetWeight", Caption = "Inv. Net Weight")]
		[ResourceStringData("9C6FB397-F2A7-42F3-8EEB-8E59667AECA0", Caption = "Inv. Net Weight", FullDescription = "[18 01 001 000]  Invoice Net mass", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZDecimal JZ_NetWeight
		{
			get => base.JZ_NetWeight;
			set => base.JZ_NetWeight = value;
		}

		#region IChargeHolder
		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeType, JobComInvCharge charge) => GetDefaultCurrencyCode(chargeType, charge);

		protected virtual ZString GetDefaultCurrencyCode(ICustomsChargeCode chargeType, JobComInvCharge charge)
		{
			var parent = charge.Parent;
			var result = ZString.Empty;

			if (parent is JobComInvoiceHeader header)
			{
				var charges = header.Charges;

				if (charges.Cast<JobComInvCharge>().ElementInFrontOf(charge)?.J7_RX_NKCurrency.IsEmpty ?? true)
				{
					result = header.JZ_RX_NKInvoice_Currency;
				}
				else
				{
					result = charges.Cast<JobComInvCharge>().ElementInFrontOf(charge).J7_RX_NKCurrency;
				}
			}
			else if (parent is JobComInvoiceLine line)
			{
				var charges = line.Charges;
				if (charges.Cast<JobComInvCharge>().ElementInFrontOf(charge)?.J7_RX_NKCurrency.IsEmpty ?? true)
				{
					result = line.InvoiceHeader.JZ_RX_NKInvoice_Currency;
				}
				else
				{
					result = charges.Cast<JobComInvCharge>().ElementInFrontOf(charge).J7_RX_NKCurrency;
				}
			}

			return result;
		}

		#endregion

		public ZString DefaultCountryOfSupply
		{
			get
			{
				var result = ZString.Empty;
				var declaration = JobDeclaration;
				if (IsAttachedToPersistentDeclaration && declaration.Configuration.InvoiceLineConfiguration.DefaultCountryOfSupplyFromSupplier(declaration))
				{
					if (declaration.Supplier != null)
					{
						result = declaration.SupplierDocumentaryAddress?.Country?.Code ?? ZString.Empty;
					}
					else
					{
						result = SupplierAddress?.Country?.Code ?? ZString.Empty;
					}
				}
				return result;
			}
		}

		#region InvoiceHeaderPayments

		[ResourceStringData("178FD1DD-0D61-4F9B-A402-AF22511C6850", ShortCaption = "Payment Code", Caption = "Payment Code")]
		public override ZString ZG_CommercialPaymentCode { get => base.ZG_CommercialPaymentCode; set => base.ZG_CommercialPaymentCode = value; }

		[ResourceStringData("3499B340-0946-4406-81F6-0F1065D106B8", ShortCaption = "Amount", Caption = "Amount")]
		public override ZDecimal JZ_PaymentAmount { get => base.JZ_PaymentAmount; set => base.JZ_PaymentAmount = value; }

		[MaxLength(20)]
		[ResourceStringData("3136D909-6DC3-41F4-97B3-118957D05CD4", ShortCaption = "Payment Reference", Caption = "Payment Reference")]
		public override ZString JZ_PaymentNo { get => base.JZ_PaymentNo; set => base.JZ_PaymentNo = value; }

		[ResourceStringData("5408161C-8A42-4873-A0C1-CA5B5D6F5FA2", ShortCaption = "Payment Ref.Date", Caption = "Payment Ref.Date")]
		public override ZDateTime JZ_PaymentDate { get => base.JZ_PaymentDate; set => base.JZ_PaymentDate = value; }

		#endregion

		public virtual IReadOnlyList<string> MultipleKeysToUse => JobDeclaration?.MultipleKeysToUse ?? new[] { JobDeclaration.CaptionKeySAD };

		public EffectiveValueManager EffectiveValueManager => effectiveValueManager ?? (effectiveValueManager = new EffectiveValueManager());
		EffectiveValueManager effectiveValueManager;

		protected void ClearInvoiceLineValuesIfSame(IZType invoiceValue, string invoiceLineFieldName)
		{
			EffectiveValueManager.ClearValueIfSame(invoiceValue, invoiceLineFieldName, JobComInvoiceLines.Cast<JobComInvoiceLine>());
		}

		#region PreviousDocuments

		IPreviousDocumentValidationDecider IPreviousDocumentsProviderWithValidationDecider.ValidationDecider => JobDeclaration?.Configuration?.InvoiceHeaderConfiguration?.GetPreviousDocumentValidationDecider(this);

		#endregion

		#region SupportingDocuments

		ISupportingDocumentCollection<SupportingDocument> ISupportingDocumentsProvider.SupportingDocuments => SupportingDocuments;

		ISupportingDocumentValidationDecider ISupportingDocumentsProviderWithValidationDecider.ValidationDecider => JobDeclaration?.Configuration?.InvoiceHeaderConfiguration?.GetSupportingDocumentValidationDecider(this);

		#endregion

		#region AdditionalInfos

		IAdditionalInfoCollection<AdditionalInfo> IAdditionalInfosProvider.AdditionalInfos => AdditionalInfos;

		IAdditionalInfoValidationDecider IAdditionalInfosProviderWithValidationDecider.ValidationDecider => JobDeclaration?.Configuration?.InvoiceHeaderConfiguration?.GetAdditionalInfoValidationDecider(this);

		#endregion

		#region IUcc6ValueProvider

		bool IUcc6ValueProvider.IsUCC6 => JobDeclaration?.IsUCC6 ?? false;

		bool IUcc6ValueProvider.IsExport => IsExport;

		bool IUcc6ValueProvider.IsImport => IsImport;

		#endregion

		#region IncoTermsAgreedPlace

		[MaxLength(512)]
		[BusinessObjectTestExclude]
		[ResourceStringData("F83487F9-B294-4979-959E-731D78EE0389", Caption = "Agreed place")]
		public ZString IncoTermsAgreedPlace
		{
			get => IncoTermsAgreedPlaceNoteWriter.Value;
			set
			{
				var oldValue = IncoTermsAgreedPlace;
				CheckMaximumLength(IncoTermsAgreedPlaceInfo, value);
				IncoTermsAgreedPlaceNoteWriter.UpdateValue(value);
				IncoTermsAgreedPlaceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IncoTermsAgreedPlaceInfo => GetZPropertyInfo(nameof(IncoTermsAgreedPlace));

		PredefinedNoteWriter IncoTermsAgreedPlaceNoteWriter
		{
			get { return incoTermsAgreedPlaceNoteWriter ?? (incoTermsAgreedPlaceNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.IncoTermsAgreedPlace)); }
		}
		PredefinedNoteWriter incoTermsAgreedPlaceNoteWriter;

		#endregion

		[ResourceStringData("EU.JobComInvoiceHeader|ChargesAddDeductTotal", Caption = "Add/Deduct Stat. Value")]
		public ZDecimal ChargesAddDeductTotal => ZDecimal.Zero;
	}
}
