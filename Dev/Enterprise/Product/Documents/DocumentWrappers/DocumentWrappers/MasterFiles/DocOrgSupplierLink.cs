using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgSupplierLink : DocOrgSupplierBuyerLink
	{
		DocOrgSupplierLink(OrgSupplierBuyerLink supplierLink, BusinessObjectFactory factoryToWrap)
			: base(supplierLink, factoryToWrap)
		{
		}

		public static DocOrgSupplierLink New(OrgSupplierBuyerLink supplierLink, BusinessObjectFactory factoryToWrap)
		{
			return supplierLink != null ? new DocOrgSupplierLink(supplierLink, factoryToWrap) : null;
		}

		OrgSupplierBuyerLink OrgSupplierLink
		{
			get { return (OrgSupplierBuyerLink)WrappedObject; }
		}

		#region Organisations and Contacts

		public override DocOrganisation ToParty
		{
			get { return Supplier; }
		}

		public override DocOrganisation FromParty
		{
			get { return Buyer; }
		}

		public override DocContacts ToPartyContact
		{
			get { return SupplierContact; }
		}

		public override DocContacts FromPartyContact
		{
			get { return BuyerContact; }
		}

		#endregion

		#region Recommended Agents

		public override DocOrganisationCollection RecommendedAgents
		{
			get
			{
				DocOrganisationCollection result;

				if (OrgSupplierLink.ReplacementAgent != null)
				{
					result = new DocOrganisationCollection(Factory);
					result.Add(ReplacementAgent);
				}
				else
				{
					result = base.RecommendedAgents;
				}

				return result;
			}
		}

		public DocOrganisation ReplacementAgent
		{
			get { return DocOrganisation.New(OrgSupplierLink.ReplacementAgent, Factory); }
		}

		#endregion

		#region Routing Order Opening Text

		public override ZString RoutingOrderOpeningText
		{
			get
			{
				ZString result = "";

				if (ReplacementAgent != null && !string.IsNullOrEmpty(Env.Registry.AgentReplacementRoutingOrderOpeningText))
				{
					result = Env.Registry.AgentReplacementRoutingOrderOpeningText;
				}
				else
				{
					result = base.RoutingOrderOpeningText;
				}

				return result;
			}
		}

		#endregion

		#region Routing Order Opening Text

		public override ZString RoutingOrderClosingText
		{
			get
			{
				ZString result = "";

				if (ReplacementAgent != null && !string.IsNullOrEmpty(Env.Registry.AgentReplacementRoutingOrderClosingText))
				{
					result = Env.Registry.AgentReplacementRoutingOrderClosingText;
				}
				else
				{
					result = base.RoutingOrderClosingText;
				}

				return result;
			}
		}

		//protected override BusinessObject BusinessObjectToLogAgainst
		//{
		//    get
		//    {
		//        return FromParty.OrgHeader;
		//    }
		//}

		#endregion
	}
}
