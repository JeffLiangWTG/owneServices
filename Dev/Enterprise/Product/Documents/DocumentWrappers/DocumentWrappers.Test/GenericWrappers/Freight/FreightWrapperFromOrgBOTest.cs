using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromOrgBO))]
	sealed class FreightWrapperFromOrgBOTest : FreightWrapperTest
	{
		#region Implementation

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Consignee : ERITREA
Consignor : ERITREA
OrgDocument : ";
			}
		}

		OrgHeader header;
		FreightWrapperFromOrgBO orgGenericFreightWrapper;

		protected override void SetUp()
		{
			header = Factory.New<OrgHeader>();
			header.OH_Code = "TST";
			orgGenericFreightWrapper = new FreightWrapperFromOrgBO(header, Factory);
			base.SetUp();
		}

		#endregion

		public void TestGetDocOrganisation()
		{
			header.OH_Code = "TESTCODE";
			var orgDocument = orgGenericFreightWrapper.OrgDocument;

			AssertType(typeof(DocOrganisation), orgDocument);
			AssertEquals("DocOrganisation wrapper should return the OrgHeader's properties", "TESTCODE", orgDocument.Code);
		}

		public void TestGetOrgSupplierLink()
		{
			var org = OrgHeader.New(Factory);
			org.MainAddress.OA_Address1 = "26 Myrtle Street";
			org.OH_RL_NKClosestPort = "INAMD";
			org.OH_Code = "SUPPLIER";
			org.OH_IsForwarder = true;

			var buyer = OrgHeader.New(Factory);
			buyer.MainAddress.OA_Address1 = "88 Oakes Road";
			buyer.OH_RL_NKClosestPort = "INAMD";
			buyer.OH_Code = "BUYER";

			var supplierLink = org.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = org.PK;
			supplierLink.OL_OH_Buyer = buyer.PK;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Core.Constants.TransportModes.All;

			var buyerAgentAirPorts = org.AppointedAgentPorts.AddNew();
			buyerAgentAirPorts.O5_PortOrCountry = "INAMD";
			buyerAgentAirPorts.O5_AirAgentStatus = "PUB";
			buyerAgentAirPorts.O5_OA_AgentOfficeAddress = org.MainAddress.PK;
			Factory.Save();

			var supplierWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.OrgSupplierLink, supplierLink);
			var wrapper = FreightWrapperFromOrgBO.New(header, supplierWrapper, null, Factory);

			AssertEquals("SUPPLIER", wrapper.OrgSupplierLink.RecommendedAgents[0].Code);
		}

		public void TestGetOrgBuyerLink()
		{
			var org = OrgHeader.New(Factory);
			org.MainAddress.OA_Address1 = "26 Myrtle Street";
			org.OH_RL_NKClosestPort = "INAMD";
			org.OH_Code = "SUPPLIER";

			var buyer = OrgHeader.New(Factory);
			buyer.MainAddress.OA_Address1 = "88 Oakes Road";
			buyer.OH_RL_NKClosestPort = "INAMD";
			buyer.OH_Code = "BUYER";
			buyer.OH_IsForwarder = true;

			var supplierLink = org.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = org.PK;
			supplierLink.OL_OH_Buyer = buyer.PK;
			supplierLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Core.Constants.TransportModes.All;

			var buyerAgentAirPorts = buyer.AppointedAgentPorts.AddNew();
			buyerAgentAirPorts.O5_PortOrCountry = "INAMD";
			buyerAgentAirPorts.O5_AirAgentStatus = "PUB";
			buyerAgentAirPorts.O5_OA_AgentOfficeAddress = buyer.MainAddress.PK;
			Factory.Save();

			var buyerWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.OrgBuyerLink, supplierLink);
			var wrapper = FreightWrapperFromOrgBO.New(header, null, buyerWrapper, Factory);

			AssertEquals("BUYER", wrapper.OrgBuyerLink.RecommendedAgents[0].Code);
		}

		public void TestGetJobNumber()
		{
			var wrapper = FreightWrapperFromOrgBO.New(header, Factory);
			AssertEquals(wrapper.OrgDocument.ToString(), wrapper.JobNumber);

			wrapper = FreightWrapperFromOrgBO.New(header, null, null, Factory);
			AssertEquals(wrapper.OrgDocument.ToString(), wrapper.JobNumber);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<OrgHeader>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var orgHeader = Factory.New<OrgHeader>();
			return new FreightWrapperFromOrgBO(orgHeader, Factory);
		}
	}
}
