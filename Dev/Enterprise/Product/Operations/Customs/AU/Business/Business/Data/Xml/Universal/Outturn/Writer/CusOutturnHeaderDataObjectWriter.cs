using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderDataObjectWriter : DataTransfer.Universal.Outturn.CusOutturnHeaderDataObjectWriter<CusOutturnHeader>, DataTransfer.Universal.IAUCusOutturnHeaderDataObjectWriter
	{
		public CusOutturnHeaderDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override void PopulateCountrySpecificDetails(CusOutturnHeader sourceBO, UShipment shipment)
		{
		}

		protected override void PopulateSubShipmentCollection(CusOutturnHeader sourceBO, UShipment shipment)
		{
			var cargoLines = sourceBO.Outturns.Cast<DepotCusOutturn>().Where(d => d.C5_CargoType != CMRCargoTypes.Codes.FullContainerLoad).OrderBy(x => x.C5_MasterBill).ThenBy(x => x.C5_HouseBill).ThenBy(x => x.C5_ContainerNumber).ThenBy(x => x.C5_CargoType);
			var shipmentWriter = new DepotCusOutturnDataObjectWriter(writeManager);
			var containerWriter = new CusContainerDataObjectWriter(writeManager, shipmentWriter.LinkManager);
			var containerList = new DataObjectList<Container>();
			var cargoPackLinesToBuildContainerList = sourceBO.Outturns.Cast<DepotCusOutturn>().OrderBy(x => x.C5_MasterBill);
			foreach (var cargoPacklines in cargoPackLinesToBuildContainerList)
			{
				var dataObject = containerWriter.GetDataObject(cargoPacklines);
				if (dataObject != null)
				{
					containerList.Add(dataObject);
				}
			}
			shipment.SetContainerCollection(() => containerList);
			var data = ProcessCollection(cargoLines, shipmentWriter);
			shipment.SetSubShipmentCollection(() => data != null ? new DataObjectList<UShipment>(data) : null);
		}
	}
}
