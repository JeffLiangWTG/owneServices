using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.Testing
{
	public class MFIForwardingShipmentDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetShippingLine()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultShippingLineAddress(Factory.LoadTop1<OrgHeader>(new ZQuery()));
			consol.Shipments.Add(shipment);
			var docSupporter = new MockMFIForwardingShipmentDocumentSupporter(shipment);
			AssertEquals("No menu name - should return consol's shipping line", consol.ShippingLinePK, docSupporter.GetShippingLine("").PK);
			AssertEquals("No declaration - should return null", null, docSupporter.GetShippingLine("Carrier Document Pack"));
			var jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_OH_ShippingLine = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consol.ShippingLinePK)).PK;
			jobDec.JE_JS = shipment.PK;
			jobDec.JE_OverrideFreightDefaults = true;
			AssertEquals("No menu name - should return consol's shipping line", consol.ShippingLinePK, docSupporter.GetShippingLine("").PK);
			AssertEquals("should return declaration's shipping line", jobDec.JE_OH_ShippingLine, docSupporter.GetShippingLine("Carrier Document Pack").PK);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			var shipment = Factory.New<MFIForwardingShipment>();
		}

		public class MockMFIForwardingShipmentDocumentSupporter : MFIForwardingShipmentDocumentSupporter
		{
			public MockMFIForwardingShipmentDocumentSupporter(ForwardingShipment shipment) : base(shipment)
			{
			}

			public new OrgHeader GetShippingLine(ZString menuName)
			{
				return base.GetShippingLine(menuName);
			}
		}
		#endregion
	}
}
