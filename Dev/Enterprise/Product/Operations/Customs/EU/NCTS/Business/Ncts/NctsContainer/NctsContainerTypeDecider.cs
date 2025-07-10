using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainerTypeDecider : CountrySpecificTypeDecider
	{
		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsContainer);

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Enumerable.Empty<CountrySpecificType>();

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var pk = row == null ? ZGuid.Empty : new ZGuid(row[CusInBondContainer.Schema.BC_ParentID]);
			var goodsItem = pk.IsValid ? factory.Load<NctsCommonCargoDesc>(pk) : null;
			return GetTypeForCountryCode(goodsItem?.Header?.CountryCode ?? ZString.Empty);
		}
	}
}
