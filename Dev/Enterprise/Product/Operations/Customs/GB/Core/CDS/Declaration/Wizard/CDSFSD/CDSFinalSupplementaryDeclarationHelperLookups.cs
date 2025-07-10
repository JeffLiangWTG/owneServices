using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSFinalSupplementaryDeclarationHelperLookups : ZLookups
	{
		public CDSFinalSupplementaryDeclarationHelperLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AuthorisationTypeList => Factory.GetCachedValue("GB.CDSFinalSupplementaryDeclarationHelperLookups.AuthorisationTypeList", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(CDSAuthorisationHeaderTypeList.Codes.SimplifiedDeclaration, "Simplified Declaration");
			list.AddPair(CDSAuthorisationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "Entry in declarant’s records (EIDR)");
			return list;
		});

		public OrganisationsFindBoxCollection DeclarantList => Factory.GetCachedValue("GB.CDSFinalSupplementaryDeclarationHelperLookups.DeclarantList", () => new OrganisationsFindBoxCollection(Factory));

		public OrganisationsFindBoxCollection AuthorisationHolderList => Factory.GetCachedValue("GB.CDSFinalSupplementaryDeclarationHelperLookups.AuthorisationHolderList", () => new OrganisationsFindBoxCollection(Factory));

		public ConsigneeCollection ImporterList => Factory.GetCachedValue("GB.CDSFinalSupplementaryDeclarationHelperLookups.ImporterList", () => new ConsigneeCollection(Factory));
	}
}
