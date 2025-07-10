using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ClientBISIShipmentHeader))]
	public class ClientBISIShipmentHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSaveAndLoadThirdPartyIndicatorWhichIsNotABool()
		{
			ClientBISIShipmentHeader shipment = Factory.New<ClientBISIShipmentHeader>();
			shipment.T8_ThirdPartyIndicator = "X";
			Factory.Save();
			ClientBISIShipmentHeader loadedShipment = new BusinessObjectFactory().Load<ClientBISIShipmentHeader>(shipment.PK);
			AssertEquals("X", loadedShipment.T8_ThirdPartyIndicator);
		}

		#region Related Business Objects
		public void TestCharges()
		{
			ClientBISIShipmentHeader header = Factory.New<ClientBISIShipmentHeader>();
			AssertNotNull(header.Charges);
		}

		#endregion
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
