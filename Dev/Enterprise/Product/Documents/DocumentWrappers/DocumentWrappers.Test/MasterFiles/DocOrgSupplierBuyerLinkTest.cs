using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgSupplierBuyerLink))]
	public class DocOrgSupplierBuyerLinkTest : DocumentWrapperTestCase
	{
		public void TestRecommendedAgents()
		{
			SupplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();

			DocSupplierLink = (DocOrgSupplierLink)GetDocumentWrappers()[0];
			AssertEquals("No recommended agents", 0, DocSupplierLink.RecommendedAgents.Count);

			OrgHeader airAgentForSyd = OrgHeader.New(Factory);
			airAgentForSyd.OH_FullName = "Forwarder 1";
			airAgentForSyd.OH_RL_NKClosestPort = "INAMD";
			airAgentForSyd.MainAddress.OA_Address1 = "hello";
			airAgentForSyd.OH_IsForwarder = true;
			OrgAppointedAgentPorts newAgentAirPorts = airAgentForSyd.AppointedAgentPorts.AddNew();
			newAgentAirPorts.O5_PortOrCountry = "INAMD";
			newAgentAirPorts.O5_AirAgentStatus = "PUB";
			newAgentAirPorts.O5_OA_AgentOfficeAddress = airAgentForSyd.MainAddress.PK;
			airAgentForSyd.OH_Code = "FORW1";

			Factory.Save();

			DocSupplierLink = (DocOrgSupplierLink)GetDocumentWrappers()[0];
			AssertEquals("No recommended agents as first agent is for Air", 0, DocSupplierLink.RecommendedAgents.Count);

			OrgHeader seaAgentForSyd = OrgHeader.New(Factory);
			seaAgentForSyd.OH_FullName = "Forwarder 1";
			seaAgentForSyd.OH_RL_NKClosestPort = "INAMD";
			seaAgentForSyd.MainAddress.OA_Address1 = "hello";
			seaAgentForSyd.OH_IsForwarder = true;
			OrgAppointedAgentPorts newAgentSeaPorts = seaAgentForSyd.AppointedAgentPorts.AddNew();
			newAgentSeaPorts.O5_PortOrCountry = "INAMD";
			newAgentSeaPorts.O5_SeaAgentStatus = "PUB";
			newAgentSeaPorts.O5_OA_AgentOfficeAddress = seaAgentForSyd.MainAddress.PK;
			seaAgentForSyd.OH_Code = "FORW2";

			Factory.Save();

			DocSupplierLink = (DocOrgSupplierLink)GetDocumentWrappers()[0];
			AssertEquals("One recommended agent", 1, DocSupplierLink.RecommendedAgents.Count);

			SupplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";

			Factory.Save();

			DocSupplierLink = (DocOrgSupplierLink)GetDocumentWrappers()[0];
			AssertEquals("Two recommended agents - 1 for Air, 1 for Sea", 2, DocSupplierLink.RecommendedAgents.Count);
		}

		public void TestRecommendedAgentsSameForAirAndSea()
		{
			Factory.Save();

			DocSupplierLink = (DocOrgSupplierLink)GetDocumentWrappers()[0];
			AssertEquals("Precondition: No recommended agents", 0, DocSupplierLink.RecommendedAgents.Count);

			OrgHeader agent = OrgHeader.New(Factory);
			agent.OH_FullName = "Forwarder 1";
			agent.OH_RL_NKClosestPort = "INAMD";
			agent.MainAddress.OA_Address1 = "hello";
			agent.OH_IsForwarder = true;
			OrgAppointedAgentPorts newAgentPorts = agent.AppointedAgentPorts.AddNew();
			newAgentPorts.O5_PortOrCountry = "INAMD";
			newAgentPorts.O5_AirAgentStatus = "PUB";
			newAgentPorts.O5_SeaAgentStatus = "PUB";
			newAgentPorts.O5_OA_AgentOfficeAddress = agent.MainAddress.PK;
			agent.OH_Code = "FORW1";

			Factory.Save();

			DocSupplierLink = (DocOrgSupplierLink)GetDocumentWrappers()[0];
			AssertEquals("One recommended agents as the air and sea agent is the same org", 1, DocSupplierLink.RecommendedAgents.Count);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocOrgSupplierLink.New(SupplierLink, Factory) };
		}

		protected override void SetUp()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.OA_Address1 = "26 Myrtle Street";
			org.OH_RL_NKClosestPort = "INAMD";
			org.OH_Code = "ABC123";

			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.MainAddress.OA_Address1 = "88 Oakes Road";
			buyer.OH_RL_NKClosestPort = "INAMD";
			buyer.OH_Code = "ABC124";

			SupplierLink = org.SupplierLinks.AddNew();
			SupplierLink.OL_OH_Supplier = org.PK;
			SupplierLink.OL_OH_Buyer = buyer.PK;

			Factory.Save();

			base.SetUp();
		}

		OrgSupplierBuyerLink SupplierLink;
		DocOrgSupplierLink DocSupplierLink;

		#endregion
	}
}
