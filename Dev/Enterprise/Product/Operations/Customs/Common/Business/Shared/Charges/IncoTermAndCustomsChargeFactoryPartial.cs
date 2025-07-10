using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Common
{
	public abstract partial class IncoTermAndCustomsChargeFactory
	{
		public ICustomsChargeCode GetCharge(string chargeCode)
		{
			ICustomsChargeCode result;
			return AllChargesDictionary.TryGetValue(chargeCode, out result) ? result : null;
		}

		public ICustomsChargeCode[] GetAllCharges() => AllChargesDictionary.Values.ToArray();

		SortedList<string, ICustomsChargeCode> AllChargesDictionary => allChargesDictionary ?? (allChargesDictionary = new SortedList<string, ICustomsChargeCode>(GetCharges().ToDictionary(x => x.Code), GetChargeCodeComparer()));
		SortedList<string, ICustomsChargeCode> allChargesDictionary;

		protected virtual IComparer<string> GetChargeCodeComparer() => null;

		protected abstract ICustomsChargeCode[] GetCharges();
	}

	public partial class CommonIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		public virtual CustomsChargeCode GetOverseasFreight() => CustomsChargeCodeProvider.OverseasFreight;
		public virtual CustomsChargeCode GetOverseasInsurance() => CustomsChargeCodeProvider.OverseasInsurance;
		protected override ICustomsChargeCode[] GetCharges()
		{
			return new ICustomsChargeCode[]
				{
					CustomsChargeCodeProvider.PackingCost,
					GetOverseasFreight(),
					GetOverseasInsurance(),
					CustomsChargeCodeProvider.ExWorks,
					CustomsChargeCodeProvider.Commission,
					CustomsChargeCodeProvider.ForeignInlandFreight,
					CustomsChargeCodeProvider.LandingCharges,
					CustomsChargeCodeProvider.OtherCharges,
					CustomsChargeCodeProvider.AdditionCharge,
					CustomsChargeCodeProvider.DeductionCharge,
					CustomsChargeCodeProvider.Discount
				};
		}
	}
}

