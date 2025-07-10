using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class AUOrganisationMatching : Enterprise.DataTransfer.Business.OrganisationMatching
	{
		public AUOrganisationMatching(BusinessObjectFactoryProvider factoryProvider, Xsd.XmlInterchange interchange, INotifications notifications)
			: base(factoryProvider, interchange, notifications)
		{
		}

		public ZGuid Match(ZString name, MasterFiles.Integration.OrganisationTypes orgTypes)
		{
			ZGuid result = ZGuid.Empty;
			if (!name.IsEmpty)
			{
				OrgHeader[] candidateMatches = FactoryProvider.Current.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, name));
				foreach (OrgHeader candidate in candidateMatches)
				{
					if ((candidate.OrganisationTypes & orgTypes) != 0)
					{
						result = candidate.PK;
						break;
					}
				}
			}

			if (result.IsEmpty)
			{
				Xsd.Organisation criteria = new Xsd.Organisation();
				criteria.OrganisationDetails.Name = name;
				OrgMatchingResult orgMatchingResult = Match(criteria, orgTypes);
				if (orgMatchingResult.MatchFound || !orgMatchingResult.TempOrgCouldNotBeCreated)
				{
					result = orgMatchingResult.Match.PK;
				}
			}
			return result;
		}
	}
}
//tested in EXDOCMessageDecoder
