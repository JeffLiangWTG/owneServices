using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CusStorageDocPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAvailableEDocs()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc2 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "InvoiceShipment.pdf", "CIV");

			var pivot = entryHeader.EDocPivotCollection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Available EDocs count is 2", 2, pivot.Lookups.AvailableEDocs.Count);
				AssertEquals("First edoc", eDoc1.UniqueKey, pivot.Lookups.AvailableEDocs[0].PK);
				AssertEquals("Second edoc", eDoc2.UniqueKey, pivot.Lookups.AvailableEDocs[1].PK);
			});
		}
	}
}
