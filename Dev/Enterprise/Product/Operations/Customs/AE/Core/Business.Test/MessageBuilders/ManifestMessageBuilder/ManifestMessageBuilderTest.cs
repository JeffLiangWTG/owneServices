using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AE.Business.Testing;

public class ManifestMessageBuilderTest : TestCaseWithFactory
{
	public void TestErrors()
	{
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
		consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
		var builder = new ManifestMessageBuilder(consol);
		AssertEquals("Can't allocate the Agent's Reference Number from", builder.Errors.Trim());
		consol.JK_UniqueConsignRef = "C00001";
		builder = new ManifestMessageBuilder(consol);
		AssertEquals("", builder.Errors);
	}

	public void TestGetMessages()
	{
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
		consol.JK_ConsolMode = Core.Constants.ContainerModes.Liquid;
		consol.JK_UniqueConsignRef = "C00001";
		var shipment = consol.Shipments.AddNew();
		shipment.JS_HouseBill = "HouseBill";
		shipment.JS_RL_NKOrigin = "ZACPT";
		shipment.JS_RL_NKDestination = "USLAX";
		var outerPackLine = shipment.OuterPackLines.AddNew();
		outerPackLine.JL_RH_NKCommodityCode = "XX";
		var builder = new ManifestMessageBuilder(consol);
		AssertEquals("", builder.Errors);
		AssertEquals(true, builder.GetMessage().StartsWith("\"VOY\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"MFI\",\"1\",\"1\""));
		consol.JK_ConsolMode = Core.Constants.ContainerModes.Liquid;
		builder = new ManifestMessageBuilder(consol);
		AssertEquals("", builder.Errors);
		AssertEquals(true, builder.GetMessage().StartsWith("\"VOY\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"MFI\",\"1\",\"1\""));
	}
}
