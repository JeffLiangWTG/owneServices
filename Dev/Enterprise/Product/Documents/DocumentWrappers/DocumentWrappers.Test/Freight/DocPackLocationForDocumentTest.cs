using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocPackLocationForDocument))]
	sealed class DocPackLocationForDocumentTest : DocumentWrapperTestCase
	{
		#region Abstract members

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocPackLocationForDocument.New(Location, Factory)
				};
		}

		#endregion

		#region Properties

		public void TestShipmentNumber()
		{
			AssertEquals("ShipmentNumber", Location.ShipmentNumber, Wrapper.ShipmentNumber);
		}

		public void TestDestination()
		{
			AssertEquals("Destination", Location.Destination, Wrapper.Destination);
		}

		public void TestConsignor()
		{
			AssertEquals("Consignor", Location.ConsignorName, Wrapper.Consignor);
		}

		public void TestPackCount()
		{
			AssertEquals("PackCount", Location.PackCount, Wrapper.PackCount);
		}

		public void TestPackType()
		{
			AssertEquals("PackType", Location.PackType, Wrapper.PackType);
		}

		public void TestWhsLocation()
		{
			AssertEquals("WhsLocation", Location.WhsLocation, Wrapper.WhsLocation);
		}

		#endregion

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocPackLocationForDocument.New(Location, Factory);
		}

		#region Implementation

		protected override void SetUp()
		{
			Location = new PackLocationForDocument("S00002031", "AUSYD", "Shipping Inc", 23, "PKG", "Warehouse");
			Wrapper = DocPackLocationForDocument.New(Location, Factory);
			base.SetUp();
		}

		PackLocationForDocument Location;
		DocPackLocationForDocument Wrapper;

		#endregion
	}
}
