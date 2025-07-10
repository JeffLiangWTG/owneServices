using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentInfoBuilderTest : TestCaseWithFactory
	{
		public void TestCreateHouseBills()
		{
			var shipment = CreateShipment();
			Factory.Save();

			var billOfLadingMenuItem = GetIAUBillOfLadingMenuItem();

			AssertNotNull("prerequisite", billOfLadingMenuItem);

			var builder = new DocumentInfoBuilder((BusinessObject)shipment, billOfLadingMenuItem);

			var documentInfos = builder.CreateDocumentInfos();

			AssertEquals("created bills of lading (1 original and 1 copy)", 2, documentInfos.Count);
			AssertContainsExactElementsInAnyOrder("created bills of lading", new[]
			{
				"Original - PRN",
				"Copy - PRN",
				"Copy - EML",
				"Copy - FAX"
			},
			documentInfos.SelectMany(i => i.Descriptor.PrintInstructions.DeliveryModes.Select(m => $"{i.Document.Name} - {m}")));
		}

		StmMenuItem GetIAUBillOfLadingMenuItem() => Factory.Load<StmMenuItemBase>(new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244"));

		IForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRRIO";
			shipment.JS_HouseBillOfLadingType = "IAU";

			return shipment;
		}
	}
}
