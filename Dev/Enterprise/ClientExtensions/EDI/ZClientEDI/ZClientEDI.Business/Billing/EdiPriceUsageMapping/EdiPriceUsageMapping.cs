
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(ClientLicencePriceHeader), "UsageMaps")]
	public class EdiPriceUsageMapping : AutoEdiPriceUsageMapping
	{
		public EdiPriceUsageMapping(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ClientLicencePriceHeader PriceHeader
			=> Factory.Load<ClientLicencePriceHeader>(PUM_L6);

		public UsageCodeKey PriceKey => new UsageCodeKey(PUM_PriceCategory, PUM_PriceCode);

		[List(nameof(Lookups) + "." + nameof(EdiPriceUsageMappingLookups.PriceCategories))]
		public override ZString PUM_PriceCategory { get => base.PUM_PriceCategory; set => base.PUM_PriceCategory = value; }

		public ZString PUM_PriceCategoryDescription
			=> Lookups.PriceCategories.GetDescriptionFromCode(PUM_PriceCategory);

		[List(nameof(Lookups) + "." + nameof(EdiPriceUsageMappingLookups.PriceCodesForCategory))]
		public override ZString PUM_PriceCode { get => base.PUM_PriceCode; set => base.PUM_PriceCode = value; }

		public ZString PriceCodeDescription
			=> Lookups.PriceCodesForCategory.GetDescriptionFromCode(PUM_PriceCode);

		[List(nameof(Lookups) + "." + nameof(EdiPriceUsageMappingLookups.AllCategories))]
		public override ZString PUM_UsageCategory { get => base.PUM_UsageCategory; set => base.PUM_UsageCategory = value; }
	}
}

