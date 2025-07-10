using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
	{
		public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new AddInfoJobComInvoiceLine Parent
		{
			get { return (AddInfoJobComInvoiceLine)base.Parent; }
		}

		public CodeDescriptionPairList TariffBypassCodeList => Factory.GetCachedValue<TariffBypassCodeList>();

		protected override ICollection GetCountriesOfDestinationCore() => UniversalReferenceDataHelper.GetCustomsApprovedCountryList(Factory, Parent.Parent.GetDefaultDataGroupingCode());

		protected override ICollection GetCountriesOfDispatchCore() => UniversalReferenceDataHelper.GetCustomsApprovedCountryList(Factory, Parent.Parent.GetDefaultDataGroupingCode());

		protected override ICollection CountryOfSupplyListCore() => UniversalReferenceDataHelper.GetCustomsApprovedCountryList(Factory, Parent.Parent.GetDefaultDataGroupingCode());
	}
}
