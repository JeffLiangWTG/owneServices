using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class FaxPriceRegistryItem : StronglyTypedRegistryItem<FaxPriceCollection>
	{
		public FaxPriceRegistryItem(string category)
			: base(new RegistryItemImpl(
				"FaxPrice",
				(NoResString)category,
				(NoResString)"Faxing Service Prices",
				(NoResString)"Please specify fax page rate for each currency in use.",
				new FaxPriceDataType(),
				RegistryStorageFlags.System))
		{
		}
	}
}

