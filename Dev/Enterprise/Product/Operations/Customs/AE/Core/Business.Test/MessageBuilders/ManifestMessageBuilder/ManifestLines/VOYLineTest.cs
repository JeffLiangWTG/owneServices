using System;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business.Testing;

public class VOYLineTest : ManifestLineTest.ManifestLineTesting
{
	public void TestProperties()
	{
		VOYLine line = (VOYLine)GetNewManifestLine();
		line.LineCode = Pad("LINECODE");
		line.VoyageAgentCode = Pad("VoyageAgentCode");
		line.VesselName = Pad("VesselName");
		line.AgentVoyageNumber = Pad("AgentVoyageNumber");
		line.PortCodeOfDischarge = Pad("PortCodeOfDischarge");
		line.ExpectedToArriveDate = new ZDateTime(2008, 8, 8, 10, 10, 10);
		line.RotationNumber = Pad("RotationNumber");
		line.MessageType = Pad("MessageType");
		line.NoOfInstalment = 12346798;
		line.AgentsManifestSequenceNumber = 123456789;
		AssertEquals("VOY", line.Identifier);
		AssertEquals("LINECO", line.LineCode);
		AssertEquals("Voyage", line.VoyageAgentCode);
		AssertEquals("VesselName                    ", line.VesselName);
		AssertEquals("AgentVoyag", line.AgentVoyageNumber);
		AssertEquals("PortC", line.PortCodeOfDischarge);
		AssertEquals("08-Aug-2008", line.GetField(VOYLine.Schema.ExpectedToArriveDate.Name));
		AssertEquals("Rotati", line.RotationNumber);
		AssertEquals("Mes", line.MessageType);
		AssertEquals(123, line.NoOfInstalment);
		AssertEquals(12345, line.AgentsManifestSequenceNumber);
	}

	#region override
	protected override ManifestLine GetNewManifestLine()
	{
		return new VOYLine();
	}

	protected override int FieldCount
	{
		get
		{
			return 11;
		}
	}

	protected override Type ExpectedType
	{
		get
		{
			return typeof(VOYLine);
		}
	}
	#endregion
}
