using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureHeaderContainerTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			if (row != null)
			{
				var parentPk = (row != null) ? new ZGuid(row[CusInBondContainer.Schema.BC_ParentID]) : ZGuid.Empty;
				if (parentPk.IsValid)
				{
					var parentTableCode = new ZString(row[CusInBondContainer.Schema.BC_ParentTableCode]);
					if (parentTableCode.EqualsIgnoringCase(CusInBondHeaderSchema.Constants.Prefix)
						&& factory.Load<NctsHeader>(parentPk) is NctsHeader header)
					{
						countryCode = header.CountryCode;
					}
				}
			}
			return GetTypeForCountryCode(countryCode);
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context) => context?.Country is string country ? GetTypeForCountryCode(country) : GetTypeForLoad(null, null);

		protected override Type DefaultTypeForUnsupportedCountry => typeof(NctsDepartureHeaderContainer);

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IFRNctsDepartureHeaderContainer>),
			new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.INctsDepartureHeaderContainer>),
			new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.INctsDepartureHeaderContainer>),
		};
	}
}
