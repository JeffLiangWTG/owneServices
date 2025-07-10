using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocAppointedAgentPorts))]
	public class DocAppointedAgentPortsTest : DocumentWrapperTestCase
	{
		public void TestO5_PortOrCountry()
		{
			AssertEquals("AUBNE", AppAgPortsWrapper.AgentPortOrCountry);
		}

		public void TestO5_AgentDirection()
		{
			AssertEquals(AgentDirectionList.Codes.Export, AppAgPortsWrapper.AgentDirection);
		}

		public void TestO5_AirAgentStatus()
		{
			AssertEquals(AgentStatusList.Codes.Appointed, AppAgPortsWrapper.AirAgentStatus);
		}

		public void TestO5_SeaAgentStatus()
		{
			AssertEquals(AgentStatusList.Codes.Handles, AppAgPortsWrapper.SeaAgentStatus);
		}

		public void TestO5_RoadAgentStatus()
		{
			AssertEquals(AgentStatusList.Codes.Published, AppAgPortsWrapper.RoadAgentStatus);
		}

		public void TestO5_RailAgentStatus()
		{
			AssertEquals(AgentStatusList.Codes.Appointed, AppAgPortsWrapper.RailAgentStatus);
		}

		public void TestO5_OA_AgentOfficeAddress()
		{
			AssertEquals(DocAddress.New(Factory.Load<OrgAddress>(AppAgentPorts.O5_OA_AgentOfficeAddress), Factory).ToString(),
							AppAgPortsWrapper.CarrierOrForwarderAddress.ToString());
		}

		public void TestO5_SeaAirCarrierOrForwarderType()
		{
			AssertEquals(CarrierOrForwarderType.Codes.AirCTO, AppAgPortsWrapper.CarrierOrForwarderType);
		}

		#region Implementation

		OrgHeader NewHeaderForTest()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "Test";
			header.MainAddress.OA_Address1 = "TestAddress1";

			OrgAppointedAgentPorts appAgPort = header.AppointedAgentPorts.AddNew();
			appAgPort.O5_PortOrCountry = "AUBNE";
			appAgPort.O5_AgentDirection = AgentDirectionList.Codes.Export;
			appAgPort.O5_AirAgentStatus = AgentStatusList.Codes.Appointed;
			appAgPort.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			appAgPort.O5_RoadAgentStatus = AgentStatusList.Codes.Published;
			appAgPort.O5_RailAgentStatus = AgentStatusList.Codes.Appointed;
			appAgPort.O5_OA_AgentOfficeAddress = header.MainAddress.PK;
			appAgPort.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.AirCTO;
			return header;
		}

		OrgAppointedAgentPorts AppAgentPorts;
		DocAppointedAgentPorts AppAgPortsWrapper;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocAppointedAgentPorts.New(NewHeaderForTest().AppointedAgentPorts[0] ,Factory)
			};
		}

		protected override void SetUp()
		{
			AppAgentPorts = NewHeaderForTest().AppointedAgentPorts[0];
			AppAgPortsWrapper = DocAppointedAgentPorts.New(AppAgentPorts, Factory);
			AssertNotNull(AppAgPortsWrapper);
			base.SetUp();
		}

		#endregion
	}
}
