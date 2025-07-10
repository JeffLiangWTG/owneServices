using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;

namespace Enterprise.Customs.BR.Business.ProductCatalog
{
	public class ManufacturersToProductsProvider : Collection<ManufacturerToProductProvider>, IEnumerable<IManufacturerToProduct>
	{
		ManufacturersToProductsProvider(IEnumerable<ForeignOperator> foreignOperators)
		{
			foreignOperators.ForEach(x => Add(ManufacturerToProductProvider.New(x)));
		}

		public static ManufacturersToProductsProvider New(IEnumerable<ForeignOperator> foreignOperators) => new (foreignOperators);

		IEnumerator<IManufacturerToProduct> IEnumerable<IManufacturerToProduct>.GetEnumerator() => GetEnumerator();
	}
}
