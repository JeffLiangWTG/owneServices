using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Wizards.EIDR
{
	public class EidrWizardLookups : JobDeclarationLookups
	{
		public EidrWizardLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public RefCusProcedureCollection CPCList => new RefCusProcedureCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, "", Customs.Common.EU.EUJobMessageTypeList.Codes.Import);

		public CodeDescriptionPairList PackageTypeList => RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);

		public RefCountryCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = new RefCountryCollection(Factory);
					countryList.ApplySort(RefCountrySchema.RN_Desc.Name, ListSortDirection.Ascending);
				}
				return countryList;
			}
		}
		RefCountryCollection countryList;

		public CodeDescriptionPairList EIDRTypeList => Factory.GetCachedValue<EidrTypeList>();

		public OrganisationsFindBoxCollection DeclarantList => Factory.GetCachedValue("GB.EidrWizardLookups.OrganisationsFindBoxCollection", () => new OrganisationsFindBoxCollection(Factory));

		public OrganisationsFindBoxCollection WarehouseList => Factory.GetCachedValue("GB.EidrWizardLookups.OrganisationsFindBoxCollection", () => new OrganisationsFindBoxCollection(Factory));
	}
}
