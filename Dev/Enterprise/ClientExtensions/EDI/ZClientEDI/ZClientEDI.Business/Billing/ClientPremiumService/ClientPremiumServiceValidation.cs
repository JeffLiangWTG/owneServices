//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientPremiumServiceValidation
//
//    This class should be used for overriding validation in AutoClientPremiumServiceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Client.EDI.Licencing.Business;

	public class ClientPremiumServiceValidation : AutoClientPremiumServiceValidation
	{
		public ClientPremiumServiceValidation(AutoClientPremiumService parent) : base(parent)
		{
		}

		protected override void CheckCPS_Type()
		{
			var parent = (ClientPremiumService)Parent;
			if (parent.CPS_EndDate.IsEmpty || parent.CPS_EndDate > ZDate.Today)
			{
				ListValidation.ErrorIfInvalidCode(parent.CPS_TypeInfo);
			}
		}

		protected override void CheckCPS_Units()
		{
			var parent = (ClientPremiumService)Parent;
			MandatoryValidation.CheckNotZero(parent.CPS_UnitsInfo);
			MandatoryValidation.CheckNotNegative(parent.CPS_UnitsInfo);
		}

		protected override void CheckCPS_StartDateIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CPS_StartDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 10,
				FutureYearsBeforeWarning = 5,
				PastYearsBeforeError = 50,
				PastYearsBeforeWarning = 15
			});
		}

		protected override void CheckCPS_StartDate()
		{
			base.CheckCPS_StartDate();
			CompareValidation.CheckDateIsBeforeAnotherDate(Parent.CPS_StartDateInfo, Parent.CPS_EndDateInfo);
		}

		protected override void CheckCPS_EndDateIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CPS_EndDateInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 50,
				FutureYearsBeforeWarning = 15,
				PastYearsBeforeError = 50,
				PastYearsBeforeWarning = 15
			});
		}

		protected override void CheckCPS_EndDate()
		{
			base.CheckCPS_EndDate();
			CompareValidation.CheckDateIsAfterAnotherDate(Parent.CPS_EndDateInfo, Parent.CPS_StartDateInfo);
		}

		protected override void CheckCPS_LD()
		{
			base.CheckCPS_LD();
			var parent = (ClientPremiumService)Parent;
			var db = parent.Database;

			if (db != null)
			{
				var products = parent.Factory.GetCachedValue("ClientPremiumServiceValidation.CheckCPS_LD.Products", () =>
					EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.GetProductCodes()
					.Select(x => x.ToString()).Concat(new[] { ProductTypes.Codes.CargoWiseOne, ProductTypes.Codes.Enterprise })
					.Distinct().ToHashSet());

				if (!products.Contains(db.LD_Product))
				{
					parent.CPS_LDInfo.AddError($"Premium services are only supported for certain database product codes ({string.Join(",", products.ToArray())})");
				}
			}
		}
	}
}

