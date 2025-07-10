using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoJobComInvoiceHeaderLookups : EU.Business.Declaration.AddInfoJobComInvoiceHeaderLookups
	{
		public AddInfoJobComInvoiceHeaderLookups(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

		public CodeDescriptionPairList ValuationMethodList => Factory.GetCachedValue<ValuationMethodList>();
		public RefCountryCollection IncotermCountries => new RefCountryCollection(Factory);
	}
}
