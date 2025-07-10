//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvHeaderChargeValidation
//
//    This class should be used for overriding validation in AutoJobComInvHeaderChargeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common
{
	public class JobComInvHeaderChargeValidation : AutoJobComInvHeaderChargeValidation
	{
		public JobComInvHeaderChargeValidation(AutoJobComInvHeaderCharge parent)
			: base(parent)
		{
		}

		protected new JobComInvCharge Parent
		{
			get { return (JobComInvCharge)base.Parent; }
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();
			if (Parent.J7_Amount < 0)
			{
				Parent.J7_AmountInfo.AddError(Res.GetString("247889e3-c3d9-4950-8b4a-0a588d1b7b81", "Amount should not be negative. If you want to reduce FOB value by this amount, please enter it as a positive amount and a 'Non-Dutiable' charge."));
			}
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			if (!Parent.J7_RX_NKCurrency.IsEmpty && Parent.Currency == null)
			{
				Parent.J7_RX_NKCurrencyInfo.AddError(Res.GetString("65b1cafc-f44d-48b2-821a-1f6d39a69969", "Please enter a valid currency."));
			}
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			if (Parent.ChargeCode == null)
			{
				var iInvCharges = Parent as Integration.Customs.IBaseJobComInvHeaderCharge;
				if (iInvCharges?.NeedCheckChargeType ?? true)
				{
					Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("43245f1d-33d1-41aa-b403-89ea640aa4a2", "Please enter a valid Charge code."));
				}
			}

			if (!Parent.J7_ChargeTypeInfo.HasMessageErrors())
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.J7_ChargeTypeInfo);
			}

			ValidateJ7_Percentage();
		}

		protected override void CheckJ7_IsGSTApplicable()
		{
			base.CheckJ7_IsGSTApplicable();
			if (Parent.ChargeCode != null && Parent.ChargeCode.IsVATibleDeemedForThisCharge
				&& Parent.ChargeCode.IsVATible != Parent.J7_IsGSTApplicable)
			{
				string message = IsCIFComponentUsed ?
					Parent.ChargeCode.IsVATible ? Res.GetString("A90BA509-E151-4A17-9C7A-9C9A3FBFB9CD", "{0:G} is a CIF component.") : Res.GetString("F89C9419-FADB-4AF8-9169-8E11A07D0F99", "{0:G} is not a CIF component.") :
					Parent.ChargeCode.IsVATible ? Res.GetString("a424d658-e700-4c41-b996-c22a7ca68296", "{0:G} is GST-applicable.") : Res.GetString("b1e7f528-ce6e-4580-aff9-c8466cd1ef11", "{0:G} is not GST-applicable.");
				Parent.J7_IsGSTApplicableInfo.AddMessageError(string.Format(message, Parent.Lookups.ChargeTypeList.GetDescriptionFromCode(Parent.J7_ChargeType)));
			}
		}

		protected virtual bool IsCIFComponentUsed
		{
			get { return false; }
		}

		protected override void CheckJ7_IsNotIncludedInInvoice()
		{
			base.CheckJ7_IsNotIncludedInInvoice();
			ValidateJ7_Percentage();
		}

		protected override void CheckJ7_Percentage()
		{
			base.CheckJ7_Percentage();
			if (Parent.J7_Percentage != 0m)
			{
				if (Parent.J7_Percentage > 100 || Parent.J7_Percentage < 0)
				{
					Parent.J7_PercentageInfo.AddError(Res.GetString("70451ade-5542-4bb1-934c-f4069660377d", "Percentage should be a value between 0 and 100."));
				}

				if (Parent.ChargeCode != null && !Parent.ChargeCode.IsPercentageApplicable)
				{
					Parent.J7_PercentageInfo.AddError(Res.GetString("ca5e4817-36c4-44d2-adfb-ded7d331c025", "You can't enter a percentage for this charge type."));
				}
			}
		}

		protected override void CheckJ7_IsDutiable()
		{
			base.CheckJ7_IsDutiable();

			if (Parent.ChargeCode != null && Parent.ChargeCode.IsDutiableDeemedForThisCharge
				&& Parent.ChargeCode.IsDutiable != Parent.J7_IsDutiable)
			{
				string message = Parent.ChargeCode.IsDutiable ? Res.GetString("6c854396-60df-4d75-a1c0-6d08028b3fd1", "{0:G} is dutiable.") : Res.GetString("39f65f06-f7c9-493a-8f49-3dde1a954d00", "{0:G} is not dutiable.");
				Parent.J7_IsDutiableInfo.AddMessageError(string.Format(message, Parent.Lookups.ChargeTypeList.GetDescriptionFromCode(Parent.J7_ChargeType)));
			}
		}

		protected override void CheckJ7_DistributeBy()
		{
			base.CheckJ7_DistributeBy();
			ListValidation.MessageErrorIfInvalidCode(Parent.J7_DistributeByInfo, Parent.Lookups.ChargeDistributionBy);
		}

		protected override void CheckJ7_ChargeTypeIsWesternEuropean()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.J7_ChargeTypeInfo);
		}

		protected override void RunAdditionalValidationOnValidationObject(ZPropertyInfo propertyInfo)
		{
			base.RunAdditionalValidationOnValidationObject(propertyInfo);

			if (propertyInfo.Name == JobComInvHeaderChargeSchema.Constants.J7_ChargeType)
			{
				CheckChargeTypeShouldNotHaveErrors(propertyInfo);
			}
		}

		protected virtual void CheckChargeTypeShouldNotHaveErrors(ZPropertyInfo chargeTypeInfo)
		{
			if (chargeTypeInfo.HasErrors())
			{
				ErrorReporter.ReportOnce("ChargeTypeShouldNotHaveErrors", "Do not add errors on charge type, use message errors instead. Errors are: " + string.Join(",", chargeTypeInfo.GetErrors().Select(x => x.Message)));
			}
		}
	}
}

