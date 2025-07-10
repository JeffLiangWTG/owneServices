using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocJobSupplierBookingLine : DocumentWrapper
	{
		protected DocJobSupplierBookingLine(JobSupplierBookingLine supplierBookingLine, BusinessObjectFactory factoryToWrap)
			: base(supplierBookingLine, factoryToWrap)
		{
		}

		public static DocJobSupplierBookingLine New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<JobSupplierBookingLine>(pK), factory);
		}

		public static DocJobSupplierBookingLine New(JobSupplierBookingLine supplierBookingLine, BusinessObjectFactory factoryToWrap)
		{
			if (supplierBookingLine == null)
			{
				return null;
			}
			else
			{
				return new DocJobSupplierBookingLine(supplierBookingLine, factoryToWrap);
			}
		}

		#region Overrides

		public override string ToString()
		{
			return ZString.Empty;
		}

		#endregion

		protected JobSupplierBookingLine BookingLine
		{
			get { return (JobSupplierBookingLine)WrappedObject; }
		}

		public DocOrderLine OrderLine => DocOrderLine.New(BookingLine.OrderLine, BookingLine.Factory);

		public ZDecimal BookedQuantity => BookingLine.JSL_BookedQuantity;

		public ZString QuantityUnit => BookingLine.OrderLine?.JO_OrderUnitOfQty ?? ZString.Empty;

		public ZDecimal BookedPackages => BookingLine.JSL_BookedPackages;

		public ZString PackagesUnit => BookingLine.JSL_F3_NKBookedPackagesUnit;

		public ZDecimal Volume => BookingLine.JSL_Volume;

		public ZString VolumeUnit => BookingLine.JSL_VolumeUnit;

		public ZDecimal GrossWeight => BookingLine.JSL_GrossWeight;

		public ZString GrossWeightUnit => BookingLine.JSL_GrossWeightUnit;

		public ZInt ReceivedPackages => BookingLine.JSL_ReceivedPackages;

		public ZDecimal ReceivedQuantity => BookingLine.JSL_ReceivedQuantity;

		public ZDecimal ReceivedVolume => BookingLine.JSL_ReceivedVolume;

		public ZDecimal ReceivedWeight => BookingLine.JSL_ReceivedWeight;

		public ZString MarksAndNumbers => BookingLine.JSL_MarksAndNumbers;

		public ZString BookingLineId => BookingLine.JSL_BookingLineId;

		public ZInt RemainingPackagesToBePacked => BookingLine.JSL_RemainingPackagesToBePacked;

		public ZDecimal RemainingQuantityToBePacked => BookingLine.JSL_RemainingQuantityToBePacked;

		public ZDecimal RemainingVolumeToBePacked => BookingLine.JSL_RemainingVolumeToBePacked;

		public ZDecimal RemainingWeightToBePacked => BookingLine.JSL_RemainingWeightToBePacked;

		public ZString CommodityCode => BookingLine.JSL_RH_NKCommodityCode;

		public ZDate ShipmentWindowEnd => BookingLine.JSL_ShipmentWindowEnd;

		public ZDate ShipmentWindowStart => BookingLine.JSL_ShipmentWindowStart;

		public ZDateTime FirstReceiptDateUtc => BookingLine.JSL_FirstReceiptDateUtc;

		public ZDateTime LastReceiptDateUtc => BookingLine.JSL_LastReceiptDateUtc;

		public ZInt DispatchedPackages => BookingLine.JSL_DispatchedPackages;

		public ZDecimal DispatchedQuantity => BookingLine.JSL_DispatchedQuantity;

		public ZDecimal DispatchedVolume => BookingLine.JSL_DispatchedVolume;

		public ZDecimal DispatchedWeight => BookingLine.JSL_DispatchedWeight;

		public ZDecimal OpenPackages => BookingLine.OpenPackages;

		public ZDecimal OpenQuantity => BookingLine.OpenQuantity;

		public ZDecimal OpenVolume => BookingLine.OpenVolume;

		public ZDecimal OpenWeight => BookingLine.OpenWeight;

		public ZDecimal RemainingPackagesToBeReceived => BookingLine.RemainingPackagesToBeReceived;

		public ZDecimal RemainingQuantityToBeReceived => BookingLine.RemainingQuantityToBeReceived;

		public ZDecimal RemainingVolumeToBeReceived => BookingLine.RemainingVolumeToBeReceived;

		public ZDecimal RemainingWeightToBeReceived => BookingLine.RemainingWeightToBeReceived;

		public DocPackLines LooseCargo => looseCargo ??= DocPackLines.New(Factory, BookingLine.LooseCargoPackLine.PK);
		DocPackLines looseCargo;
	}
}
