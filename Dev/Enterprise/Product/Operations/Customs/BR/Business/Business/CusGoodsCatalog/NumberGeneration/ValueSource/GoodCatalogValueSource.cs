using System.Collections;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Customs.BR.Business
{
	public sealed class GoodCatalogValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public GoodCatalogValueSource(CusGoodsCatalog goodsCatalog)
		{
			this.goodsCatalog = goodsCatalog;

			providers = new INumberGeneratorValueProvider[]
			{
				new NumberGeneratorValueProvider(Keys.Direction, Direction),
			};
		}

		readonly INumberGeneratorValueProvider[] providers;
		readonly CusGoodsCatalog goodsCatalog;

		#region IEnumerable<INumberGeneratorValueProvider> Members

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return ((IEnumerable<INumberGeneratorValueProvider>)providers).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		string Direction(NumberGenerator generator, string detail)
		{
			switch (goodsCatalog.CGC_Type)
			{
				case GoodsCatalogTypeList.Codes.Import:
					return "I";
				case GoodsCatalogTypeList.Codes.Export:
					return "E";
				default:
					return "O";
			}
		}
	}
}
