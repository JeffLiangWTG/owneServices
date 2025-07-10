using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public partial class EdificeIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>();
			result.Add(CustomsChargeCodeProvider.AdditionCharge);
			result.Add(EdificeBuyingCommission);
			result.Add(CustomsChargeCodeProvider.DeductionCharge);
			result.Add(CustomsChargeCodeProvider.Discount);
			result.Add(CustomsChargeCodeProvider.ExWorks);
			result.Add(CustomsChargeCodeProvider.ForeignInlandFreight);
			result.Add(CustomsChargeCodeProvider.LandingCharges);
			result.Add(CustomsChargeCodeProvider.OtherCharges);
			result.Add(EdificeOtherCommission);
			result.Add(EdificeOverseasFreight);
			result.Add(CustomsChargeCodeProvider.OverseasInsurance);
			result.Add(EdificePackingCost);
			return result.ToArray();
		}
	}
}
