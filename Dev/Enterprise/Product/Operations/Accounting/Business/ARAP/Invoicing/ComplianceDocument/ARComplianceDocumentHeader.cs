using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingIServices;
using Enterprise.Accounting.Business.ARAP.Invoicing.ComplianceDocument;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public partial class ARComplianceDocumentHeader : AccComplianceDocumentHeader, IComplianceNumberSequence
	{
		public ARComplianceDocumentHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ADH_Ledger = LedgerTypes.AccountsReceivable;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			CreateEInvoicingPivot();
		}

		#region E-Invoicing

		void CreateEInvoicingPivot()
		{
			var enableEInvoicingFunctinality = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;
			var complianceDate = AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetValueWithoutFallback(ADH_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);
			var hasComplianceDateReached = ADH_DocumentDate >= complianceDate;
			if (EInvoicingTransactionPivot == null
				&& !ADH_OH_Organisation.IsEmpty
				&& (IsVoided && !string.IsNullOrEmpty(ADH_DocumentNumber) || IsAllocated)
				&& enableEInvoicingFunctinality
				&& hasComplianceDateReached
				&& IsEligibleToCreateEInvoicingTransactionPivot)
			{
				var complianceDocumentPivot = Factory.New<AccEInvoicingTransactionPivot>();
				complianceDocumentPivot.AIP_ParentID = PK;
				complianceDocumentPivot.AIP_ParentTableCode = AccComplianceDocumentHeaderSchema.Constants.Prefix;
				complianceDocumentPivot.SetCompanyAndCountryCode(Company);
			}
		}

		public override bool IsEligibleToCreateEInvoicingTransactionPivot => ElectronicInvoicingEligibilityDecider.IsEligible(this);

		#endregion

		bool AllowToSetBookOrNumber => IsInDatabase || AllowToSetBookOrNumberOnTransactionType();

		bool AllowToSetBookOrNumberOnTransactionType()
		{
			bool result;
			if (ADH_TransactionType == TransactionTypes.CreditNote)
			{
				result = AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.Value || AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
			}
			else
			{
				result = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
			}
			return result;
		}

		bool AllowToSetNumberOnComplianceSubType()
		{
			var result = true;

			if (ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE)
			{
				result = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;
			}

			var countryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Company.Country.Code);
			var complianceDocumentNumberProvider = (countryFactory as IInstanceProvider<IComplianceDocumentNumberProvider>)?.Get();
			var allocateComplianceDocumentNumberErrorMessage = complianceDocumentNumberProvider?.AllocateComplianceDocumentNumberErrorMessage(new ARComplianceDocumentHeader[] { this });
			result = result && string.IsNullOrEmpty(allocateComplianceDocumentNumberErrorMessage);

			return result;
		}

		protected override bool ADH_XD_ComplianceBook_ReadOnly => IsFromCreditNoteAndConfigurationEnabled || !IsAdded;

		protected override bool ADH_ReportingPeriod_ReadOnly => true;

		protected override bool ADH_ComplianceSubType_ReadOnly => !IsAdded;

		protected override bool ADH_DocumentNumber_ReadOnly => !IsFromCreditNoteAndConfigurationEnabled || !IsAdded;

		protected override bool ADH_DocumentDate_ReadOnly => !IsAdded;

		protected override AccComplianceDocumentHeaderValidation GetNewValidation()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
			{
				return new ARComplianceDocumentHeaderValidationTaiwan(this);
			}
			else
			{
				return new ARComplianceDocumentHeaderValidation(this);
			}
		}

		protected override ZString InternalReference
		{
			get
			{
				INumberFountainProxy numberFountain;
				if (AccountingConfigurationRegistry.Instance.ShareSequentialARComplianceDocumentsReferenceNumbers.GetValueWithoutFallback(ADH_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).Value)
				{
					numberFountain = Environment.Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, ADH_GC_Company.ToGuid());
				}
				else
				{
					numberFountain = Environment.Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, ADH_TransactionType, ADH_GC_Company.ToGuid());
				}

				return numberFountain.GetNextFormatted(Factory);
			}
		}

		public override void SetComplianceDocumentNumber()
		{
			if (AllowToSetBookOrNumber && AllowToSetNumberOnComplianceSubType())
			{
				if (IsFromCreditNoteAndConfigurationEnabled)
				{
					ADH_DocumentNumber = OriginalComplianceDocumentHeader?.ADH_DocumentNumber ?? ZString.Empty;
				}
				else
				{
					if (ADH_XD_ComplianceBook.IsValid && ComplianceBook != null && ADH_DocumentNumber.IsDefault)
					{
						Factory.Saving += new BusinessObjectFactory.SavingEventHandler(SetComplianceDocumentNumberOnSaving);
					}
				}
			}
		}

		public override void SetComplianceSequenceBook()
		{
			if (AllowToSetBookOrNumber)
			{
				if (!IsFromCreditNoteAndConfigurationEnabled)
				{
					if (!ADH_ComplianceSubType.IsEmpty)
					{
						var findBook = new Func<AccComplianceSequence[], AccComplianceSequence>(findedBooks =>
						{
							return findedBooks.Length > 0 ? findedBooks[0] : null;
						});

						ADH_XD_ComplianceBook = findBook(AccComplianceSequence.FindSuitableSequenceBook(ADH_ComplianceSubType, ComplianceBookAllocationLevel.Counter, ADH_DocumentDate, ADH_DocumentDate, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK))?.PK ??
							findBook(AccComplianceSequence.FindSuitableSequenceBook(ADH_ComplianceSubType, ComplianceBookAllocationLevel.BranchDepartment, ADH_DocumentDate, ADH_DocumentDate, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK))?.PK ??
							findBook(AccComplianceSequence.FindSuitableSequenceBook(ADH_ComplianceSubType, ComplianceBookAllocationLevel.Branch, ADH_DocumentDate, ADH_DocumentDate, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK))?.PK ??
							findBook(AccComplianceSequence.FindSuitableSequenceBook(ADH_ComplianceSubType, ComplianceBookAllocationLevel.Company, ADH_DocumentDate, ADH_DocumentDate, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK))?.PK ?? ZGuid.Empty;
					}
				}
			}
		}

		public override ZString CheckCanVoid()
		{
			var message = base.CheckCanVoid();

			if (string.IsNullOrEmpty(message))
			{
				if (EInvoicingStatus != EInvoicingPivotState.Succeed && (ADH_ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE || ADH_ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE))
				{
					return Res.GetString("C2D09D3B-6160-474D-B7FD-CA60CB488B61", "Compliance Document with sub type TXE and TCE cannot be voided until the original document has been successfully uploaded.");
				}
			}

			return message;
		}

		void SetComplianceDocumentNumberOnSaving(BusinessObjectFactory factory)
		{
			ADH_DocumentNumber = ComplianceBook.GetNextNumber(this, ADH_DocumentDate);
			Factory.Saving -= new BusinessObjectFactory.SavingEventHandler(SetComplianceDocumentNumberOnSaving);
		}

		#region IComplianceNumberSequence

		public ZString GetMatchingComplianceSubType() => ZString.Empty;

		public ZBool IsCorrected
		{
			get
			{
				var invoice = Factory.Load<TransactionHeader>(TransactionHeaders[0].PK);
				return NumberFountainTransactionDataProvider.GetIsCorrected(invoice);
			}
		}

		public ZString ComplianceTransactionType => ADH_TransactionType;

		public ZString ComplianceSubType => ADH_ComplianceSubType;

		public ZDateTime ComplianceDocumentDate => ADH_DocumentDate;

		public ZDateTime PostDate => ZDateTime.Empty;

		public ZDateTime InvoiceDate => ZDateTime.Empty;

		#endregion
	}
}
