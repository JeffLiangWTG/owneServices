using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsUnloadingMovementHeaderTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var headerPK = new ZGuid(row[NctsUnloadingMovementHeader.Schema.BM_BH]);
			var header = factory.Load<NctsHeader>(headerPK);
			return GetTypeForCountryCode(header?.CountryCode ?? ZString.Empty);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IUnloadingMovementHeader>)
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsUnloadingMovementHeader);
	}
}
