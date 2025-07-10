using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCargoDescFeeTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsCargoDescFee>),
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.INctsCargoDescFee>),
			new CountrySpecificType(Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.INctsCargoDescFee>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsCargoDescFee);

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var pk = row == null ? ZGuid.Empty : new ZGuid(row[CusInBondFee.Schema.BFE_BY]);
			var parent = pk.IsValid ? factory.Load<NctsCommonCargoDesc>(pk) : null;
			return GetTypeForCountryCode(parent?.Header?.CountryCode ?? ZString.Empty);
		}
	}
}
