using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class ETailDepotCusOutturnDataObjectReader : DepotCusOutturnDataObjectReader
	{
		public ETailDepotCusOutturnDataObjectReader(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, CusOutturnHeader outturnheader, Shipment topShipment, Shipment parentShipment)
			: base(shipment, logger, factory, outturnheader)
		{
			this.parentShipment = parentShipment;
			this.topShipment = topShipment;
		}

		#region override
		protected override void PopulateBusinessObject(DepotCusOutturn targetBO)
		{
			var outturnRow = GetColumnIndexer(targetBO);
			var totalPieces = dataObject.TotalNoOfPieces;
			var outturnedPieces = ZInt.Zero;
			var goodsDesc = dataObject.GoodsDescription.GetValueOrDefault();
			var houseBill = "";
			if (dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House && !dataObject.WayBillNumber.GetValueOrDefault().IsEmpty)
			{
				houseBill = dataObject.WayBillNumber;
			}
			var masterBill = "";
			if (topShipment.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master && !topShipment.WayBillNumber.GetValueOrDefault().IsEmpty)
			{
				masterBill = topShipment.WayBillNumber;
			}
			var cargoType = topShipment.ContainerMode?.Code;
			var receiptDate = parentShipment.LocalProcessing?.LCLAvailable;
			var isDamaged = false;
			var isPillaged = false;
			ZString? packType = "";

			if (dataObject.PackingLineCollection?.Count > 0)
			{
				packType = dataObject.PackingLineCollection[0].PackType?.Code;
				foreach (var line in dataObject.PackingLineCollection)
				{
					if (!isDamaged && line.OutturnDamagedQty.GetValueOrDefault() > 0)
					{
						isDamaged = true;
					}
					if (!isPillaged && line.OutturnPillagedQty.GetValueOrDefault() > 0)
					{
						isPillaged = true;
					}
					outturnedPieces += line.OutturnQty.GetValueOrDefault();
				}
			}

			SetValue(outturnRow, CusOutturnSchema.C5_OuterPacks, totalPieces);
			SetValue(outturnRow, CusOutturnSchema.C5_PackagesOutturned, outturnedPieces);
			SetValue(outturnRow, CusOutturnSchema.C5_DamageIndicator, isDamaged);
			SetValue(outturnRow, CusOutturnSchema.C5_PillageIndicator, isPillaged);
			SetValue(outturnRow, CusOutturnSchema.C5_GoodsDescription, goodsDesc);
			SetValue(outturnRow, CusOutturnSchema.C5_HouseBill, houseBill);
			SetValue(outturnRow, CusOutturnSchema.C5_MasterBill, masterBill);
			SetValue(outturnRow, CusOutturnSchema.C5_CargoUnpackDate, ZDateTime.Now);
			SetValue(outturnRow, CusOutturnSchema.C5_CargoReceiptDate, receiptDate);
			SetValue(outturnRow, CusOutturnSchema.C5_CargoType, cargoType);
			SetValue(outturnRow, CusOutturnSchema.C5_OuterPackUnits, packType);
			SetValue(outturnRow, CusOutturnSchema.C5_PackagesUnits, packType);

			FillContainer(outturnRow);
		}

		void FillContainer(IColumnIndexer outturnRow)
		{
			var container = parentShipment.ContainerCollection?.FirstOrDefault();
			ZString? containerNumber = dataObject.PackingLineCollection?.FirstOrDefault().ContainerNumber;
			if (string.IsNullOrEmpty(containerNumber))
			{
				containerNumber = container?.ContainerNumber ?? ZString.Empty;
			}
			SetValue(outturnRow, CusOutturnSchema.C5_ContainerNumber, containerNumber);
			if (container != null)
			{
				SetValue(outturnRow, CusOutturnSchema.C5_ContainerSeal, container.Seal);
				SetValue(outturnRow, CusOutturnSchema.C5_SealIntactIndicator, container.IsSealOk);
			}
		}
		#endregion

		#region Shipment
		readonly Shipment parentShipment;
		readonly Shipment topShipment;
		#endregion
	}
}
