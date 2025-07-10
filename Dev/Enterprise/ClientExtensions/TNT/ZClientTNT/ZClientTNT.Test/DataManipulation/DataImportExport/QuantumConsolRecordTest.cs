using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.Testing
{
	public class QuantumConsolRecordTest : FlightRecordTest
	{
		public override void TestFieldProperty()
		{
			QuantumConsolRecord record = new QuantumConsolRecord(DataString);
			AssertEquals("MasterBill", "12527013486", record.MasterBill);
			record.MasterBill = "   MasterBill    ";
			AssertEquals("MasterBill", "MasterBill", record.MasterBill);
			AssertEquals("Departure Date", new ZDateTime(2005, 7, 15), record.DepartureDate);
			record.FlightDate = new ZDateTime(2005, 9, 30);
			AssertEquals("Departure Date", new ZDateTime(2005, 9, 30), record.DepartureDate);
			record[FlightRecord.Schema.FlightDate.Name] = "221104";
			AssertEquals("Departure Date", new ZDateTime(2004, 11, 22), record.DepartureDate);
			AssertEquals("Port Of Loading", "SIN", record.PortOfLoading);
			record.PortOfLoading = "   Port Of Loading    ";
			AssertEquals("Port Of Loading", "Port Of Loading", record.PortOfLoading);
			AssertEquals("Port Of Discharge", "SYD", record.PortOfDischarge);
			record.PortOfDischarge = "   Port Of Discharge  ";
			AssertEquals("Port Of Discharge", "Port Of Discharge", record.PortOfDischarge);
		}

		public void TestCreateConsol()
		{
			QuantumConsolRecord record = new QuantumConsolRecord(DataString);
			NotificationBuffer buffer = new NotificationBuffer(null);
			int consolCount = Factory.GetDatabaseCount(typeof(ForwardingConsol));
			ForwardingConsol consol = record.CreateConsol(Factory, buffer);
			AssertNotNull("Consol should not be null", consol);
			AssertEquals("Transport Type", Constants.TransportModes.Air, consol.JK_TransportMode);
			AssertEquals("Flight no", "BA0151", consol.JK_JX_JV_VoyageFlight);
			AssertEquals("Origin", PortMappingHelper.SGSINUnLoco.Code, consol.JK_JX_JA_RL_NKPortOfLoading);
			AssertEquals("Destination", PortMappingHelper.AUSYDUnLoco.Code, consol.JK_JX_JB_RL_NKPortOfDischarge);
			AssertEquals("Departure Date", new ZDateTime(2005, 7, 15), consol.JK_JX_JA_E_DEP);
			AssertEquals("Neutral master", false, consol.JK_IsNeutralMaster);
			AssertEquals("MAWB", "12527013486", consol.JK_MasterBillNum);
			Factory.Save();
			AssertEquals("1 new ForwardingConsol should have been created", consolCount + 1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
		}

#region Implementation
		protected override IQDownBaseRecord GetRecord(ZString rawData)
		{
			return new QuantumConsolRecord(rawData);
		}

		protected override Type ExpectedRecordType
		{
			get
			{
				return typeof(QuantumConsolRecord);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			PortMappingHelper = new OrgProxyPortMappingTestHelper(Factory);
			PortMappingHelper.AddPortMappingToOrgProxy("SYD", PortMappingHelper.AUSYDUnLoco);
			PortMappingHelper.AddPortMappingToOrgProxy("SIN", PortMappingHelper.SGSINUnLoco);
			Factory.Save();
		}

		OrgProxyPortMappingTestHelper PortMappingHelper;
#endregion
	}
}
