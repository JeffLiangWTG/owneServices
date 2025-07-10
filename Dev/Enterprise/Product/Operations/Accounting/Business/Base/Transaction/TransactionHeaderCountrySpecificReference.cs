using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public abstract partial class TransactionHeader
	{
		#region Portugal

		public void CreateTransactionHeaderReferenceIVA()
		{
			if (IsFinalCustomer())
			{
				var transactionHeaderReferenceIVA = CreateTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IVA);
				transactionHeaderReferenceIVA.AH1_Reference = "NoIVA";
			}
		}

		bool IsFinalCustomer()
		{
			var result = false;
			if (Header != null
				&& Company != null
				&& Company.GC_RN_NKCountryCode == CountryCodes.Portugal
				&& AH_Ledger == LedgerTypes.AccountsReceivable
				&& (AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote))
			{
				var taxNumberIVA = Header.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(x => PortugalComplianceInfo.IsTaxRegistrationNumber(x.OK_RN_NKCodeCountry, x.OK_CodeType));
				result = Header.CountryCode == CountryCodes.Portugal
					? taxNumberIVA == null || AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA(taxNumberIVA.OK_CustomsRegNo)
					: taxNumberIVA != null && AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA(taxNumberIVA.OK_CustomsRegNo);
			}
			return result;
		}

		[MaxLength(AutoAccTransactionHeaderReference.Schema.AH1_ReferenceMaxLength)]
		public ZString AuthorizationNumberReference
		{
			get => TransactionHeaderReferenceATH?.AH1_Reference ?? string.Empty;
			set
			{
				if (AuthorizationNumberReference != value)
				{
					if (TransactionHeaderReferenceATH == null)
					{
						transactionHeaderReferenceATH = CreateTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
					}
					TransactionHeaderReferenceATH.AH1_Reference = value;
					AuthorizationNumberReferenceInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo AuthorizationNumberReferenceInfo => GetZPropertyInfo(Schema.AuthorizationNumberReference);

		AccTransactionHeaderReference TransactionHeaderReferenceATH
		{
			get
			{
				if (transactionHeaderReferenceATH == null && IsInDatabase)
				{
					transactionHeaderReferenceATH = GetTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
				}
				return transactionHeaderReferenceATH;
			}
		}
		AccTransactionHeaderReference transactionHeaderReferenceATH;

		[MaxLength(AutoAccTransactionHeaderReference.Schema.AH1_ReferenceMaxLength)]
		public virtual ZString SourceReference
		{
			get => TransactionHeaderReferencePIR?.AH1_Reference ?? string.Empty;
			set
			{
				if (SourceReference != value)
				{
					if (TransactionHeaderReferencePIR == null)
					{
						transactionHeaderReferencePIR = CreateTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.PIR);
					}

					TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(Factory).UnlockSourceReferenceMutex(Company.GC_Code, TransactionHeaderReferencePIR.AH1_Reference);

					TransactionHeaderReferencePIR.AH1_Reference = value;
					SourceReferenceInfo.RefreshBinding();
				}
			}
		}

		AccTransactionHeaderReference TransactionHeaderReferencePIR
		{
			get
			{
				if (transactionHeaderReferencePIR == null && IsInDatabase)
				{
					transactionHeaderReferencePIR = GetTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.PIR);
				}
				return transactionHeaderReferencePIR;
			}
		}
		AccTransactionHeaderReference transactionHeaderReferencePIR;

		public ZPropertyInfo SourceReferenceInfo => GetZPropertyInfo(Schema.SourceReference);

		protected bool SourceReference_ReadOnly => ReadOnly || !IsSourceReferenceUsed;

		public void UnlockAllSourceReferenceMutexes() => TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(Factory).UnlockAllSourceReferenceMutexes();

		public bool IsSourceReferenceEnabled => AH_Ledger == LedgerTypes.AccountsReceivable
			&& (AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote)
			&& (Company?.GC_RN_NKCountryCode ?? ZString.Empty) == CountryCodes.Portugal
			&& (Company?.Country?.SupportComplianceSubType ?? false);

		public bool IsSourceReferenceUsed => IsSourceReferenceEnabled && PortugalComplianceInfo.IsComplianceSubTypeCompatibleWithSourceReference(AH_ComplianceSubType);

		public ZString PortugalAccountCodeForMissingRegistrationNumber => PortugalComplianceInfo.PortugalAccountCodeForMissingRegistrationNumber;

		void ResetDefaultSourceReference()
		{
			if (IsSourceReferenceEnabled)
			{
				SourceReference = PortugalComplianceInfo.GetSourceReferencePrefix(AH_ComplianceSubType);
			}
		}

		void ResetInvoiceLineAmounts(string oldValue)
		{
			if (PortugalComplianceInfo.IsComplianceSubTypeCompatibleWithSourceReference(oldValue)
				&& IsSourceReferenceEnabled
				&& !IsSourceReferenceUsed
				&& this is TransactionHeaderWithLines headerWithLines)
			{
				headerWithLines?.Lines?.Cast<TransactionLine>().ToList().ForEach(x => x.RecalculatePropertiesBasedOnOSExTaxAmount());
			}
		}

		public string GetTransactionNumWhereSourceReferenceIsAlreadyUsed()
		{
			var result = string.Empty;

			if (!IsInDatabase && IsSourceReferenceUsed && !SourceReference.IsEmpty)
			{
				var referencesQuery = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.PIR);
				referencesQuery.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Reference, SourceReference);
				var sameSourceReference = new BusinessObjectFactory().Load<AccTransactionHeaderReference>(referencesQuery).Where(x => x.AH1_AH != PK);

				if (sameSourceReference.Any())
				{
					var transactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, AH_GC);
					transactionQuery.AddToFilter(AccTransactionHeaderSchema.PK, sameSourceReference.Select(x => x.AH1_AH));
					var transactionsWithSameSourceReference = Factory.LoadTop1<AccTransactionHeader>(transactionQuery);
					result = transactionsWithSameSourceReference?.AH_TransactionNum ?? string.Empty;
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		protected AccTransactionHeaderReference GetTransactionHeaderReference(ZString referenceType)
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, referenceType);
			query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, PK);
			var result = Factory.LoadTop1<AccTransactionHeaderReference>(query);

			if (result != null)
			{
				RegisterEditableChildObject(result);
				References.Add(result);
			}
			return result;
		}

		protected AccTransactionHeaderReference CreateTransactionHeaderReference(ZString referenceType)
		{
			var result = Factory.New<AccTransactionHeaderReference>();
			result.AH1_AH = PK;
			result.AH1_Type = referenceType;

			RegisterEditableChildObject(result);
			References.Add(result);

			return result;
		}

		readonly HashSet<AccTransactionHeaderReference> References = new HashSet<AccTransactionHeaderReference>();

		void DeleteTransactionHeaderReferenceIfEmpty()
		{
			var referencesToCheck = References.ToArray();
			foreach (var reference in referencesToCheck)
			{
				if (!reference.IsDeleted
					&& reference.AH1_Reference.IsEmpty) // could check other fields for being empty, if necessary
				{
					reference.Delete();
				}

				if (reference.IsDeleted)
				{
					References.Remove(reference);
				}
			}
		}

		#endregion

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				UnlockAllSourceReferenceMutexes();
			}
		}

		public override ZString AH_ComplianceSubType
		{
			get
			{
				return base.AH_ComplianceSubType;
			}
			set
			{
				if (value != AH_ComplianceSubType)
				{
					var oldValue = base.AH_ComplianceSubType;
					base.AH_ComplianceSubType = value;
					ResetDefaultSourceReference();
					ResetInvoiceLineAmounts(oldValue);
					if (HeaderValidation != null)
					{
						HeaderValidation.ValidateSourceReference();
					}

					if ((ObjectFactory.Get<Enterprise.Integration.Accounting.IGlobalAccountingCountryFactory>().GetCountryFactory(Company.GC_RN_NKCountryCode) as IInstanceProvider<ISourceReferenceEditableProvider>)?.Get().CheckReferenceSourceIsEditable(IsReverseTransaction, AH_ComplianceSubType) ?? false)
					{
						AddWritableProperties(new string[] { SourceReferenceInfo.Name });
					}
					else if (WritableProperties.Contains(SourceReferenceInfo.Name))
					{
						WritableProperties.Remove(SourceReferenceInfo.Name);
						RefreshBinding();
					}
				}
			}
		}

		public AccTransactionHeaderReference GetTransactionHeaderReferenceToValidateMissingRegistrationNumber(ZString referenceType )
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, referenceType);
			query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, PK);
			var result = Factory.LoadTop1<AccTransactionHeaderReference>(query);

			return result;
		}
	}
}
