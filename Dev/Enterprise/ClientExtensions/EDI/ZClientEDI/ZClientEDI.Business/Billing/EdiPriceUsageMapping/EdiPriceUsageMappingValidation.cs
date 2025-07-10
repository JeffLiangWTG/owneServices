//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceUsageMappingValidation
//
//    This class should be used for overriding validation in AutoEdiPriceUsageMappingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;

	public class EdiPriceUsageMappingValidation : AutoEdiPriceUsageMappingValidation
	{
		public EdiPriceUsageMappingValidation(AutoEdiPriceUsageMapping parent) : base(parent)
		{
		}

		protected override void CheckPUM_PriceCategory()
		{
			var map = (EdiPriceUsageMapping)Parent;
			var info = map.PUM_PriceCategoryInfo;
			MandatoryValidation.CheckEntered(info);
		}

		protected override void CheckPUM_PriceCode()
		{
			var map = (EdiPriceUsageMapping)Parent;
			var info = map.PUM_PriceCodeInfo;
			MandatoryValidation.CheckEntered(info);
		}

		protected override void CheckPUM_UsageCode()
		{
			var map = (EdiPriceUsageMapping)Parent;
			var info = map.PUM_UsageCodeInfo;
			MandatoryValidation.CheckEntered(info);
			CheckDuplicate(map, info);
		}

		void CheckDuplicate(EdiPriceUsageMapping map, ZPropertyInfo info)
		{
			if ((!map.IsInDatabase || map.PUM_UsageCodeInfo.HasChanges || map.PUM_UsageCategoryInfo.HasChanges)
				&& map.PriceHeader.UsageMaps.Any(x => x.PK != map.PK && x.PUM_UsageCode == map.PUM_UsageCode && x.PUM_UsageCategory == map.PUM_UsageCategory))
			{
				info.AddError("Category and Code is already mapped");
			}
		}

		protected override void CheckPUM_UsageCategory()
		{
			var map = (EdiPriceUsageMapping)Parent;
			var info = map.PUM_UsageCategoryInfo;
			MandatoryValidation.CheckEntered(info);
			CheckDuplicate(map, info);
		}
	}
}

