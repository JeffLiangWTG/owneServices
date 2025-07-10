using System;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocOrgSupplierBuyerLink : DocumentWrapper
	{
		protected DocOrgSupplierBuyerLink(OrgSupplierBuyerLink supplierBuyerLink, BusinessObjectFactory factoryToWrap)
			: base(supplierBuyerLink, factoryToWrap)
		{
		}

		OrgSupplierBuyerLink OrgSupplierBuyerLink
		{
			get { return (OrgSupplierBuyerLink)WrappedObject; }
		}

		public override string ToString()
		{
			return Supplier.Name + " - " + Buyer.Name;
		}

		#region Properties

		public abstract DocOrganisation ToParty { get; }
		public abstract DocOrganisation FromParty { get; }
		public abstract DocContacts ToPartyContact { get; }
		public abstract DocContacts FromPartyContact { get; }

		public Image DocumentLogo
		{
			get
			{
				Image result = null;
				if (FromParty.MiscServ.ClientDocumentLogo.Length > 0)
				{
					try
					{
						MemoryStream stream = new MemoryStream(FromParty.MiscServ.ClientDocumentLogo);
						result = Image.FromStream(stream);
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException()) { throw; }
						result = null;
					}
				}

				if (result == null)
				{
					result = SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				}
				return result;
			}
		}

		public virtual DocOrganisationCollection RecommendedAgents
		{
			get
			{
				if (ToParty != null)
				{
					DocRecommendedAgentsCollection agents = new DocRecommendedAgentsCollection(Factory, ToParty.Loco, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode, AgentDirectionList.Codes.Import);
					agents.Load();
					return agents;
				}

				return new DocOrganisationCollection(Factory);
			}
		}

		public DocOrganisation Buyer
		{
			get { return DocOrganisation.New(Factory, OrgSupplierBuyerLink.OL_OH_Buyer); }
		}

		public DocContacts BuyerContact
		{
			get
			{
				OrgContact result = new DefaultContactFinder(OrgSupplierBuyerLink.Buyer).DefaultContact(ContactType.Consignee, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode);
				return DocContacts.New(result, Factory);
			}
		}

		public DocOrganisation Supplier
		{
			get { return DocOrganisation.New(Factory, OrgSupplierBuyerLink.OL_OH_Supplier); }
		}

		public DocContacts SupplierContact
		{
			get
			{
				OrgContact result = new DefaultContactFinder(OrgSupplierBuyerLink.Supplier).DefaultContact(ContactType.Consignor, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode);
				return DocContacts.New(result, Factory);
			}
		}

		public virtual ZString RoutingOrderOpeningText
		{
			get { return Env.Registry.RoutingOrderOpeningText; }
		}

		public virtual ZString RoutingOrderClosingText
		{
			get { return Env.Registry.RoutingOrderClosingText; }
		}

		public ZString RoutingRecommendationOpeningText
		{
			get { return Env.Registry.RoutingRecommendationOpeningText; }
		}

		public ZString RoutingRecommendationClosingText
		{
			get { return Env.Registry.RoutingRecommendationClosingText; }
		}

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get
			{
				return FromParty.OrgHeader;
			}
		}

		#endregion
	}
}
