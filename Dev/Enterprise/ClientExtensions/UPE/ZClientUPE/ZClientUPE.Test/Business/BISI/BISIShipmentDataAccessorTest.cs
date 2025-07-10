using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	public class BISIShipmentDataAccessorTest : TestCaseWithFactory
	{
		public void TestUpdateUploadData()
		{
			UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber = 5;
			ShipmentDataForTest shipmentData = new ShipmentDataForTest();
			shipmentData.ThirdPartyIndicator = "2";
			shipmentData.ChargesData = new ShipmentChargeData[] { new ShipmentChargeData(ShipmentChargeTypeCode.Security, 5m, Core.Constants.CurrencyCodes.Australia), new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 11m, Core.Constants.CurrencyCodes.Australia) };
			Accessor.UpdateUploadData(shipmentData);
			// adding the same shipmentdata multiple times does nothing.
			Accessor.UpdateUploadData(shipmentData);
			Accessor.UpdateUploadData(shipmentData);
			Accessor.UpdateUploadData(shipmentData);
			Accessor.UpdateUploadData(shipmentData);
			ClientBISIShipmentHeader[] headers = Factory.Load<ClientBISIShipmentHeader>(new ZQuery());
			AssertEquals("There should only be one header", 1, headers.Length);
			ClientBISIShipmentHeader header = headers[0];
			AssertEquals("UploadBatchNumber", 4, header.T8_UploadBatchNumber);
			AssertEquals("ThirdPartyIndicator", "2", header.T8_ThirdPartyIndicator);
			AssertEquals("there should only be one shipment charge", 2, header.Charges.Count);
			ClientBISIShipmentCharge charge = header.Charges[0];
			AssertEquals("ChargeType should come from the first post", "348", charge.T9_ChargeType);
			AssertEquals("GrossAmount should come from the first post", 5m, charge.T9_GrossAmount);
			charge = header.Charges[1];
			AssertEquals("ChargeType should come from the first post", "201", charge.T9_ChargeType);
			AssertEquals("GrossAmount should come from the first post", 11m, charge.T9_GrossAmount);
		}

		[ExpectNoExceptions]
		public void TestUpdateUploadData_With2000Shipments()
		{
			IShipmentData[] shipments = new IShipmentData[2000];
			for (int i = 0; i < shipments.Length; i++)
			{
				ShipmentDataForTest shipmentData = new ShipmentDataForTest();
				shipmentData.ChargesData = new ShipmentChargeData[] { new ShipmentChargeData(ShipmentChargeTypeCode.Security, 5m, Core.Constants.CurrencyCodes.Australia) };
				shipments[i] = shipmentData;
			}

			Accessor.UpdateUploadData(shipments);
		}

		#region Implementation
		BISIShipmentDataAccessor Accessor;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Accessor = new BISIShipmentDataAccessor(Factory);
			base.SetUp();
		}
		#endregion
	}
}
