using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	/// <summary>
	/// Summary description for TransactionLineValidation.
	/// </summary>
	public partial class TransactionLineValidation : AccTransactionLinesValidation
	{
		public TransactionLineValidation(TransactionLine parent)
			: base(parent)
		{
		}

		protected static string AmountAndTaxAmountMustHaveSameSign
		{
			get { return Res.GetString("da54ade2-c65c-4617-898d-15f5d2397591", "Amount and tax amount should have the same sign."); }
		}

		TransactionLine TransactionLine
		{
			get { return (TransactionLine)Parent; }
		}

		protected override void CheckAL_AC()
		{
			base.CheckAL_AC();
			string error = TransactionLine.ErrorMessageIfInvalidAL_AC_AL_AG();
			if (!string.IsNullOrEmpty(error))
			{
				TransactionLine.AL_ACInfo.AddError(error);
			}
		}

		protected override void CheckAL_AG()
		{
			base.CheckAL_AG();
			string error = TransactionLine.ErrorMessageIfInvalidAL_AC_AL_AG();
			if (!string.IsNullOrEmpty(error))
			{
				TransactionLine.AL_AGInfo.AddError(error);
			}

			if (TransactionLine.GLHeader != null && !TransactionLine.GLHeader.AG_IsGlobal && !TransactionLine.GLHeader.CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Any(x => x.ACF_GC_Company == GlbCompany.CurrentCompany.PK))
			{
				TransactionLine.AL_AGInfo.AddError(Res.GetString("8d4c74f5-07fa-4b71-805a-09f5a3931faf", "This GL Account cannot be used"));
			}
		}

		protected override void CheckAL_GB()
		{
			base.CheckAL_GB();
			ListValidation.ErrorIfInvalidPK(TransactionLine.AL_GBInfo, TransactionLine.Lookups.Branches);
		}

		protected override void CheckAL_GE()
		{
			base.CheckAL_GE();
			ListValidation.ErrorIfInvalidPK(TransactionLine.AL_GEInfo, TransactionLine.DepartmentCollection);
		}

		protected override void CheckAL_LineAmount()
		{
			base.CheckAL_LineAmount();

			if (!TransactionLine.IsTransactionHeaderReversing)
			{
				var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.AL_GC, CriticalValidationHelpers.MaximumAmountLevel.Line, Parent.AL_LineAmount);
				if (message != null)
				{
					Parent.AL_LineAmountInfo.AddError(message);
				}
			}
		}

		protected override void CheckAL_GSTVAT()
		{
			base.CheckAL_GSTVAT();

			if (!TransactionLine.IsTransactionHeaderReversing)
			{
				var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.AL_GC, CriticalValidationHelpers.MaximumAmountLevel.Line, Parent.AL_GSTVAT);
				if (message != null)
				{
					Parent.AL_GSTVATInfo.AddError(message);
				}
			}
		}

		protected override void CheckAL_InputGSTVATRecoverable()
		{
			base.CheckAL_InputGSTVATRecoverable();

			if (TransactionLine.SupportsInputTaxRecoverable)
			{
				var taxCountrySpecificName = TransactionLine.Branch != null ? TransactionLine.Branch.Company.Country.ConsumptionTaxDescription : "";
				if (TransactionLine.AL_InputGSTVATRecoverable < 0M || TransactionLine.AL_InputGSTVATRecoverable > 1M)
				{
					TransactionLine.AL_InputGSTVATRecoverableInfo.AddError(Res.GetString("a1e9f850-3283-409d-8641-98642a72058d", "{0} Recoverable % must be between 0 and 100.", taxCountrySpecificName));
				}
				else if (!TransactionLine.IsInDatabase || TransactionLine.AL_InputGSTVATRecoverableInfo.HasChanges)
				{
					if (TransactionLine.IsCostWithNotOverheadChargeCode && TransactionLine.AL_InputGSTVATRecoverable != 1M)
					{
						TransactionLine.AL_InputGSTVATRecoverableInfo.AddError(Res.GetString("ED802456-02B0-4FCA-944A-B491D41AB11C", "{0} Recoverable % must be 100% for all Charge Codes that are not Overheads and for all GL Accounts.", taxCountrySpecificName));
					}
					else if (!TransactionLine.IsTransactionHeaderReversing && !TransactionLine.IsUserAllowedToOverrideVATRecoverablePercentage)
					{
						var originalValue = (ZDecimal)TransactionLine.AL_InputGSTVATRecoverableInfo.OriginalValue;

						if (!TransactionLine.IsInDatabase)
						{
							originalValue = TransactionLine.GetInputGSTVATRecoverableDefaultValue();
						}

						if (TransactionLine.AL_InputGSTVATRecoverable != originalValue)
						{
							ZDecimal originalPercentageValue = originalValue * 100;
							var errorMessage = Res.GetString("250eb094-b579-474b-844a-2dc209b4f56b", "You do not have sufficient security rights to override the Tax Recoverable Percentage, please set it back to {0}%", originalPercentageValue.ToString(2));
							TransactionLine.AL_InputGSTVATRecoverableInfo.AddError(errorMessage);
						}
					}

					if (TransactionLine.AL_LineType == TransactionLineTypes.Cost && TransactionLine.AL_InputGSTVATRecoverable != 1M &&
						TransactionLine.TaxRate != null && TransactionLine.TaxRate.IsReverseCharge)
					{
						TransactionLine.AL_InputGSTVATRecoverableInfo.AddError(Res.GetString("915F06EE-78CA-48AC-B136-9B6F4D9CAB5A", "{0} Recoverable % must be 100% when line Tax type is 'RVS'.", taxCountrySpecificName));
					}
				}
			}
		}

		protected override void CheckAL_PlaceOfSupply()
		{
			base.CheckAL_PlaceOfSupply();

			if (!string.IsNullOrEmpty(Parent.AL_PlaceOfSupply))
			{
				ListValidation.ErrorIfInvalidCode(Parent.AL_PlaceOfSupplyInfo);
			}
			else if (!Parent.AL_PlaceOfSupplyType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AL_PlaceOfSupplyInfo);
			}
		}

		protected override void CheckAL_PlaceOfSupplyType()
		{
			base.CheckAL_PlaceOfSupplyType();

			if (!string.IsNullOrEmpty(Parent.AL_PlaceOfSupplyType))
			{
				ListValidation.ErrorIfInvalidCode(Parent.AL_PlaceOfSupplyTypeInfo);
			}
			else if (!Parent.AL_PlaceOfSupply.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AL_PlaceOfSupplyTypeInfo);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAL_OSExTaxAmount();
			ValidateAL_OSTaxAmount();
			ValidateAL_OverseasTotal();
			ValidateAL_LocalExTaxAmount();
			ValidateAL_LocalTaxAmount();
		}

		#region AL_OSExTaxAmount

		public void ValidateAL_OSExTaxAmount()
		{
			ValidateCalculatedProperty(TransactionLine.AL_OSExTaxAmountInfo);
		}

		protected virtual void CheckAL_OSExTaxAmount()
		{
			TypeValidation.CheckValidDecimal(TransactionLine.AL_OSExTaxAmountInfo, 19, 4);
			if (!TransactionLine.IsCommentCharge)
			{
				MandatoryValidation.CheckEntered(TransactionLine.AL_OSExTaxAmountInfo);
			}
		}

		#endregion

		#region AL_OSTaxAmount

		public void ValidateAL_OSTaxAmount()
		{
			ValidateCalculatedProperty(TransactionLine.AL_OSTaxAmountInfo);
		}

		protected virtual void CheckAL_OSTaxAmount()
		{
			TypeValidation.CheckValidDecimal(TransactionLine.AL_OSTaxAmountInfo, 19, 4);
			if (ShouldValidateTaxAmountSign)
			{
				if ((TransactionLine.AL_OSExTaxAmount > 0 && TransactionLine.AL_OSTaxAmount < 0) || (TransactionLine.AL_OSExTaxAmount < 0 && TransactionLine.AL_OSTaxAmount > 0))
				{
					TransactionLine.AL_OSTaxAmountInfo.AddError(AmountAndTaxAmountMustHaveSameSign_ForTestOnly);
				}
			}
		}

		protected virtual bool ShouldValidateTaxAmountSign
		{
			get { return !Parent.IsInDatabase; }
		}

		#endregion

		#region AL_OverseasTotal

		public void ValidateAL_OverseasTotal()
		{
			ValidateCalculatedProperty(TransactionLine.AL_OverseasTotalInfo);
		}

		protected virtual void CheckAL_OverseasTotal()
		{
			TypeValidation.CheckValidDecimal(TransactionLine.AL_OverseasTotalInfo, 19, 4);
			if (TransactionLine.AL_OverseasTotal != (TransactionLine.AL_OSExTaxAmount + TransactionLine.AL_OSTaxAmount))
			{
				TransactionLine.AL_OverseasTotalInfo.AddError(Res.GetString("5BF76FE9-1567-414E-814E-B29AE81380A9", "Overseas Total does not equal Overseas Amount + Overseas Tax."));
			}
		}

		#endregion

		#region AL_LocalExTaxAmount

		public void ValidateAL_LocalExTaxAmount()
		{
			ValidateCalculatedProperty(TransactionLine.AL_LocalExTaxAmountInfo);
		}

		protected virtual void CheckAL_LocalExTaxAmount()
		{
		}

		#endregion

		#region AL_LocalTaxAmount

		public void ValidateAL_LocalTaxAmount()
		{
			ValidateCalculatedProperty(TransactionLine.AL_LocalTaxAmountInfo);
		}

		protected virtual void CheckAL_LocalTaxAmount()
		{
		}

		#endregion

		#region CheckAL_PostDate

		protected override void CheckAL_PostDate()
		{
			base.CheckAL_PostDate();
			//Don't run validation if transaction already posted to prevent errors when viewing/reversing old transactions
			//TODO: Need a more generic solution
			if (!Parent.IsInDatabase)
			{
				PeriodValidation.CheckDateFallsIntoValidPeriod(Parent.AL_PostDateInfo);
			}
		}

		#endregion

		protected override void CheckAL_SupplyType()
		{
			base.CheckAL_SupplyType();

			if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				if (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.Value && !Parent.HasContext(BusinessContext.SurchargeLine))
				{
					MandatoryValidation.CheckEntered(Parent.AL_SupplyTypeInfo);
				}
				else if (Parent.AL_SupplyType.IsEmpty)
				{
					Parent.AL_SupplyTypeInfo.AddWarning(Res.GetString("96D98C93-AC46-4671-B3E1-4B6A2441A507", "The Supply Type is not specified. Please check if a supply type is needed before posting."));
				}

				ListValidation.ErrorIfInvalidCode(Parent.AL_SupplyTypeInfo, TransactionLine.Lookups.SupplyTypes);
			}
		}

		protected override void CheckAL_GovtChargeCode()
		{
			if (TransactionLine.IsGovtChargeCodeApplicable
				&& AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				if (!TransactionLine.IsCommentCharge
					&& TransactionLine.TaxRate != null && !TransactionLine.TaxRate.IsIndiaServiceTax)
				{
					base.CheckAL_GovtChargeCode();

					MandatoryValidation.CheckEntered(TransactionLine.AL_GovtChargeCodeInfo);
				}
				else
				{
					if (TransactionLine.AL_GovtChargeCode.IsEmpty)
					{
						TransactionLine.AL_GovtChargeCodeInfo.AddWarning(Res.GetString("eeb90aae-f296-466d-a3f1-82b5e0ee1e8f", "Government Charge Code is empty."));
					}
				}
			}
		}

		protected override INotificationType NotificationTypeForBranchDepartmentCombination
		{
			get
			{
				return TransactionLine.IsTransactionHeaderReversing ? CargoWise.EntityFramework.NotificationType.Warning :
					base.NotificationTypeForBranchDepartmentCombination;
			}
		}

		#region Validation Providers

		public PeriodValidationProvider PeriodValidation
		{
			get
			{
				if (fPeriodValidation == null)
				{
					fPeriodValidation = GetPeriodValidationProvider();
				}
				return fPeriodValidation;
			}
		}

		protected virtual PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new PeriodValidationProvider(Parent.Factory);
		}

		PeriodValidationProvider fPeriodValidation;

		#endregion

		#region Error Messages

		public virtual ZString InvalidGLAccountError
		{
			get { return Res.GetString("fd374930-f068-42e6-ae25-b913b80fee96", "You can only post to Balance Sheet or Profit & Loss Accounts"); }
		}

		public virtual ZString PostDateMustBeAtOrBeforeTodaysDate
		{
			get { return Res.GetString("dbf04457-8065-481a-a46f-5e796a5695fc", "The post date must be equal to or prior to today's date"); }
		}

		public virtual ZString InactiveGLAccountError
		{
			get { return Res.GetString("2c894b8a-598d-45d7-abf6-798a11f744ed", "This account is currently marked as inactive"); }
		}

		#endregion

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == AccTransactionLinesSchema.AL_AH.Name && Parent is GLJournalLine)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}
	}
}
