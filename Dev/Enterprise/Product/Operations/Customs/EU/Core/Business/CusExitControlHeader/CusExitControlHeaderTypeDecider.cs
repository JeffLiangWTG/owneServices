using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitControlHeaderTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusExitControlHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusExitControlHeader>),
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusExitControlHeader>),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitControlHeader);

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusExitControlHeader>();

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitControlHeaderCountryCode(row, factory));

		protected ZString GetCusExitControlHeaderCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var parentPK = (row != null) ? new ZGuid(row[CusExitControlHeader.Schema.CEH_ParentID]) : ZGuid.Invalid;
			var parentPrefix = (row != null) ? new ZString(row[CusExitControlHeader.Schema.CEH_ParentTableCode]) : ZString.Empty;
			var result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			switch (parentPrefix)
			{
				case JobDeclarationSchema.Constants.Prefix:
					var declaration = (parentPK.IsValid) ? factory.Load<JobDeclaration>(parentPK) : null;
					if (declaration != null)
					{
						result = declaration.CountryCode;
					}
					break;
				default:
					break;
			}

			return result;
		}
	}
}
