using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	public abstract class ClientSpecificRunDocsTest : BaseRunDocumentsTest
	{
		public ClientSpecificRunDocsTest()
		{
		}

		public override BusinessObject GetBusinessObject
		{
			get { return Shipment; }
		}

		protected abstract ZString ClientName { get; }

		protected CommonShipment Shipment;
		protected void SetShipmentHBLType(ZString hBLCode)
		{
			Shipment.JS_HouseBillOfLadingType = hBLCode;
		}

		protected override void SetUp()
		{
			ClientDocumentTestHelper.SetupClientDocumentsFromLocalEnterprisePath(ClientName);
			Shipment = GetNewShipmentForBOL();

			base.SetUp();
		}

		protected CommonShipment GetNewShipmentForBOL()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.Consols.AddNew();
			shipment.JS_NoCopyBills = 1;
			shipment.JS_NoOriginalBills = 1;
			return shipment;
		}
	}
}
