using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentPDFWriterTest : TestCaseWithFactory
	{
		public void TestWriteToStream()
		{
			var shipment = CreateShipment();
			Factory.Save();

			var billOfLadingMenuItem = GetIAUBillOfLadingMenuItem();

			using (var stream = new MemoryStream())
			{
				var writer = new DocumentPDFWriter();
				writer.WriteToStream(shipment, billOfLadingMenuItem, stream);

				Assert("the document was written to stream", stream.Length > 0);
			}
		}

		StmMenuItem GetIAUBillOfLadingMenuItem() => Factory.Load<StmMenuItemBase>(new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244"));

		BusinessObject CreateShipment()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRSAO";
			shipment.JS_HouseBillOfLadingType = "IAU";

			return (BusinessObject)shipment;
		}

		public void TestIsRegisteredWithObjectFactory()
		{
			var writer = ObjectFactory.Get<IDocumentPDFWriter>();
			AssertNotNull("IDocumentPDFWriter has been registered with ObjectFactory", writer);
		}
	}
}
