using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBLookups : Customs.Business.CusMAWBLookups
	{
		public CusMAWBLookups(CusMAWBBase parent)
			: base(parent)
		{
		}

		public RefUNLOCOCollection PortOfLoadingList
		{
			get
			{
				if (fPortOfLoadingList == null)
				{
					fPortOfLoadingList = new RefUNLOCOCollection(Factory);
				}
				return fPortOfLoadingList;
			}
		}
		RefUNLOCOCollection fPortOfLoadingList;

		public override OrgHeaderCollection ResponsibleParties
		{
			get
			{
				if (fResponsibleParties == null)
				{
					fResponsibleParties = new OrgHeaderCollection(Factory);
				}
				return fResponsibleParties;
			}
		}
		OrgHeaderCollection fResponsibleParties;

		public RefUNLOCOCollection PortOfDischargeList
		{
			get
			{
				if (fPortOfDischargeList == null)
				{
					fPortOfDischargeList = new RefUNLOCOCollection(Factory);
				}
				return fPortOfDischargeList;
			}
		}
		RefUNLOCOCollection fPortOfDischargeList;

		public CodeDescriptionPairList ApplicationCodeList
		{
			get
			{
				return Factory.GetCachedValue("CusMAWBLookups.ApplicationCodeList", () => new AUCusApplicationCodeList());
			}
		}

		public override CodeDescriptionPairList CustomsCargoStatusList
		{
			get
			{
				return Factory.GetCachedValue("AU Extended CustomsCargoStatusList",
					delegate
					{
						return base.CustomsCargoStatusList + CMRConsolidatedCargoStatuses.AllFilterStatuses;
					});
			}
		}

		public override CodeDescriptionPairList CustomsMessageStatusList
		{
			get
			{
				return Factory.GetCachedValue("AU Extended CustomsMessageStatusList",
					delegate
					{
						return base.CustomsCargoStatusList + new CMRBaseStatuses();
					});
			}
		}

		public OrganisationsFindBoxCollection UnpackDepotOrganisations
		{
			get { return fUnpackDepotOrganisations ?? (fUnpackDepotOrganisations = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection fUnpackDepotOrganisations;
	}
}
