using System.Collections;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class BISIShipmentDataAccessor
	{
		public BISIShipmentDataAccessor(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public void UpdateUploadData(params IShipmentData[] shipments)
		{
			Queue shipmentQueue = new Queue(shipments);
			while (shipmentQueue.Count > 0)
			{
				UpdateUploadDataFor10Shipments(shipmentQueue);
				Factory.Save();
				Thread.Sleep(0);
			}
		}

		#region Implementation

		readonly BusinessObjectFactory Factory;

		void UpdateUploadDataFor10Shipments(Queue shipmentQueue)
		{
			for (int i = 0; i < 10 && shipmentQueue.Count > 0; i++)
			{
				IShipmentData shipmentData = (IShipmentData)shipmentQueue.Dequeue();

				if (Factory.Load<ClientBISIShipmentHeader>(new ZQuery(ClientBISIShipmentHeaderSchema.T8_CS, shipmentData.Identifier)).Length == 0)
				{
					ClientBISIShipmentHeader header = Factory.New<ClientBISIShipmentHeader>();
					header.T8_CS = shipmentData.Identifier;
					header.T8_UploadBatchNumber = UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber - 1;
					header.T8_ThirdPartyIndicator = shipmentData.ThirdPartyIndicator;

					AddShipmentChargesToHeader(shipmentData, header);
				}
			}
		}

		void AddShipmentChargesToHeader(IShipmentData shipmentData,ClientBISIShipmentHeader header)
		{
			foreach (ShipmentChargeData charge1 in shipmentData.ChargesData)
			{
				ClientBISIShipmentCharge charge = header.Charges.AddNew();
				charge.T9_ChargeType = charge1.TypeCode;
				charge.T9_GrossAmount = charge1.GrossAmount;
			}
		}

		#endregion
	}
}
