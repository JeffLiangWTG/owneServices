//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxTransactionValidation
//
//    This class should be used for overriding validation in AutoAccTaxTransactionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxTransactionValidation : AutoAccTaxTransactionValidation
	{
		public AccTaxTransactionValidation(AutoAccTaxTransaction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ValidateDataRefreshBusChanges(Parent);

			ValidateATT_Rate();
		}

		AccTaxTransaction ParentTaxRecord => (AccTaxTransaction)Parent;

		public void ValidateATT_Rate() => ValidateCalculatedProperty(ParentTaxRecord.ATT_RateInfo);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used via reflection (see GetValidationMethod() in ZValidation.cs)")]
		protected virtual void CheckATT_Rate()
		{
			if (ParentTaxRecord.IsInDatabase)
			{
				return;
			}

			if (ParentTaxRecord.ATT_Rate.IsEmpty)
			{
				var systemCalculatedValues = ParentTaxRecord.GetSystemCalculatedValuesIfAvailable();
				if (systemCalculatedValues != null && systemCalculatedValues.RateNumerator.IsEmpty)
				{
					var taxID = Parent.Factory.Load<AccTaxRate>(Parent.ATT_AT_TaxID);
					ParentTaxRecord.ATT_RateInfo.AddError(Res.GetString("C9742A08-6B8E-454F-BE87-A848B63560E3", @"No valid tax rate found for tax ID '{0}'. Please check the Rate Source of the tax ID and make sure there are valid tax rate for the Rate Source.", taxID?.AT_Code));
				}
				else
				{
					ParentTaxRecord.ATT_RateInfo.AddError(MandatoryValidation.ValueCannotBeZeroMessage(ParentTaxRecord.ATT_RateInfo.HumanReadableName));
				}
			}
			else
			{
				if (ParentTaxRecord.ATT_Rate > 100)
				{
					ParentTaxRecord.ATT_RateInfo.AddError(Res.GetString("60CF2E8A-2F5E-434C-9774-C2444E48FA80", "Rate must be less than 100%"));
				}
				else if (ParentTaxRecord.ATT_Rate < 0)
				{
					ParentTaxRecord.ATT_RateInfo.AddError(Res.GetString("7A8924D8-8EDB-471E-9192-5D954E6EBE27", "Rate must be positive."));
				}
			}
		}

		protected override void CheckATT_OSTaxAmount()
		{
			if (ParentTaxRecord.IsInDatabase)
			{
				return;
			}

			base.CheckATT_OSTaxAmount();

			var systemCalculatedValues = ParentTaxRecord.GetSystemCalculatedValuesIfAvailable();
			if (systemCalculatedValues != null && Math.Sign(ParentTaxRecord.ATT_OSTaxAmount) != Math.Sign(systemCalculatedValues.OSTaxAmount))
			{
				ParentTaxRecord.ATT_OSTaxAmountInfo.AddError(Res.GetString("C93A5A06-570F-472C-8D01-E3B0881C62EA", "Please do not change sign for Tax Amount."));
			}
		}

		protected override void CheckATT_OSTaxBaseAmount()
		{
			if (ParentTaxRecord.IsInDatabase)
			{
				return;
			}

			base.CheckATT_OSTaxBaseAmount();

			var systemCalculatedValues = ParentTaxRecord.GetSystemCalculatedValuesIfAvailable();
			if (systemCalculatedValues != null && Math.Sign(ParentTaxRecord.ATT_OSTaxBaseAmount) != Math.Sign(systemCalculatedValues.OSTaxBaseAmount))
			{
				ParentTaxRecord.ATT_OSTaxBaseAmountInfo.AddError(Res.GetString("2156E183-39D7-43FA-AA78-4C6221162BB2", "Please do not change sign for Tax Base Amount."));
			}
		}

		protected override void CheckATT_TaxAuthorityServiceCode()
		{
			if (ParentTaxRecord.IsInDatabase)
			{
				return;
			}

			if (!ParentTaxRecord.ATT_TaxAuthorityServiceCodeDescription.IsEmpty)
			{
				if (ParentTaxRecord.ATT_TaxAuthorityServiceCode.IsEmpty)
				{
					ParentTaxRecord.ATT_TaxAuthorityServiceCodeInfo.AddError(ServiceCodeAndDescriptionMandatoryErrorMessage);
				}
			}
		}

		protected override void CheckATT_TaxAuthorityServiceCodeDescription()
		{
			if (ParentTaxRecord.IsInDatabase)
			{
				return;
			}

			if (!ParentTaxRecord.ATT_TaxAuthorityServiceCode.IsEmpty)
			{
				if (ParentTaxRecord.ATT_TaxAuthorityServiceCodeDescription.IsEmpty)
				{
					ParentTaxRecord.ATT_TaxAuthorityServiceCodeDescriptionInfo.AddError(ServiceCodeAndDescriptionMandatoryErrorMessage);
				}
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == AutoAccTaxTransaction.Schema.ATT_AH)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}

		string ServiceCodeAndDescriptionMandatoryErrorMessage => Res.GetString("03B4E13F-CB66-4C20-8860-739CB01974EC", "Please check the Tax Authority Service Code and Tax Authority Service Code Description entered against this Tax Transaction Record. Please enter a value in both columns or ensure both columns are empty");
	}
}
