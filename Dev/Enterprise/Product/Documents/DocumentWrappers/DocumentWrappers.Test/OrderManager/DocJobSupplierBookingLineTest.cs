using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobSupplierBookingLine))]
	public class DocJobSupplierBookingLineTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocJobSupplierBookingLine.New(BookingLine, Factory),
				DocJobSupplierBookingLine.New(BookingLine.Factory, BookingLine.PK)
			};
		}

		public void TestBasicProperties()
		{
			BookingLine.JSL_VolumeUnit = "M3";
			BookingLine.JSL_Volume = 157.65;
			BookingLine.JSL_BookedPackages = 4567.5;
			BookingLine.JSL_BookedQuantity = 123.4;
			BookingLine.JSL_F3_NKBookedPackagesUnit = "PAT";
			BookingLine.JSL_GrossWeight = 868.56;
			BookingLine.JSL_GrossWeightUnit = "T";
			BookingLine.JSL_ReceivedPackages = 857;
			BookingLine.JSL_ReceivedQuantity = 87.65;
			BookingLine.JSL_ReceivedVolume = 8574;
			BookingLine.JSL_ReceivedWeight = 874.8;
			BookingLine.JSL_MarksAndNumbers = "mark and numbers";
			BookingLine.JSL_BookingLineId = "JSL0002";
			BookingLine.JSL_RemainingPackagesToBePacked = 174;
			BookingLine.JSL_RemainingQuantityToBePacked = 875.5;
			BookingLine.JSL_RemainingVolumeToBePacked = 874.5;
			BookingLine.JSL_RemainingWeightToBePacked = 848.4;
			BookingLine.JSL_RH_NKCommodityCode = "GEN";
			BookingLine.JSL_ShipmentWindowEnd = new ZDate(2022, 7, 5);
			BookingLine.JSL_ShipmentWindowStart = new ZDate(2022, 7, 4);
			BookingLine.JSL_FirstReceiptDateUtc = new ZDateTime(2022, 4, 1, 5, 1, 2);
			BookingLine.JSL_LastReceiptDateUtc = new ZDateTime(2022, 4, 7, 5, 1, 2);
			BookingLine.JSL_DispatchedPackages = 857;
			BookingLine.JSL_DispatchedQuantity = 12.456;
			BookingLine.JSL_DispatchedVolume = 185.62;
			BookingLine.JSL_DispatchedWeight = 874.5;

			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();
			BookingLine.JSL_JO_OrderLine = orderLine.PK;

			AssertEquals("OrderLine", BookingLine.JSL_JO_OrderLine.ToString(), BookingLineWrapper.OrderLine.PrimaryKey);
			AssertEquals("BookedQuantity", BookingLine.JSL_BookedQuantity, BookingLineWrapper.BookedQuantity);
			AssertEquals("BookedPackages", BookingLine.JSL_BookedPackages, BookingLineWrapper.BookedPackages);
			AssertEquals("PackagesUnit", BookingLine.JSL_F3_NKBookedPackagesUnit, BookingLineWrapper.PackagesUnit);
			AssertEquals("Volume", BookingLine.JSL_Volume, BookingLineWrapper.Volume);
			AssertEquals("VolumeUnit", BookingLine.JSL_VolumeUnit, BookingLineWrapper.VolumeUnit);
			AssertEquals("GrossWeight", BookingLine.JSL_GrossWeight, BookingLineWrapper.GrossWeight);
			AssertEquals("GrossWeightUnit", BookingLine.JSL_GrossWeightUnit, BookingLineWrapper.GrossWeightUnit);
			AssertEquals("ReceivedPackages", BookingLine.JSL_ReceivedPackages, BookingLineWrapper.ReceivedPackages);
			AssertEquals("ReceivedQuantity", BookingLine.JSL_ReceivedQuantity, BookingLineWrapper.ReceivedQuantity);
			AssertEquals("ReceivedVolume", BookingLine.JSL_ReceivedVolume, BookingLineWrapper.ReceivedVolume);
			AssertEquals("ReceivedWeight", BookingLine.JSL_ReceivedWeight, BookingLineWrapper.ReceivedWeight);
			AssertEquals("MarksAndNumbers", BookingLine.JSL_MarksAndNumbers, BookingLineWrapper.MarksAndNumbers);
			AssertEquals("BookingLineId", BookingLine.JSL_BookingLineId, BookingLineWrapper.BookingLineId);
			AssertEquals("RemainingPackagesToBePacked", BookingLine.JSL_RemainingPackagesToBePacked, BookingLineWrapper.RemainingPackagesToBePacked);
			AssertEquals("RemainingQuantityToBePacked", BookingLine.JSL_RemainingQuantityToBePacked, BookingLineWrapper.RemainingQuantityToBePacked);
			AssertEquals("RemainingVolumeToBePacked", BookingLine.JSL_RemainingVolumeToBePacked, BookingLineWrapper.RemainingVolumeToBePacked);
			AssertEquals("RemainingWeightToBePacked", BookingLine.JSL_RemainingWeightToBePacked, BookingLineWrapper.RemainingWeightToBePacked);
			AssertEquals("RH_NKCommodityCode", BookingLine.JSL_RH_NKCommodityCode, BookingLineWrapper.CommodityCode);
			AssertEquals("ShipmentWindowEnd", BookingLine.JSL_ShipmentWindowEnd, BookingLineWrapper.ShipmentWindowEnd);
			AssertEquals("ShipmentWindowStart", BookingLine.JSL_ShipmentWindowStart, BookingLineWrapper.ShipmentWindowStart);
			AssertEquals("FirstReceiptDateUtc", BookingLine.JSL_FirstReceiptDateUtc, BookingLineWrapper.FirstReceiptDateUtc);
			AssertEquals("LastReceiptDateUtc", BookingLine.JSL_LastReceiptDateUtc, BookingLineWrapper.LastReceiptDateUtc);
			AssertEquals("DispatchedPackages", BookingLine.JSL_DispatchedPackages, BookingLineWrapper.DispatchedPackages);
			AssertEquals("DispatchedQuantity", BookingLine.JSL_DispatchedQuantity, BookingLineWrapper.DispatchedQuantity);
			AssertEquals("DispatchedVolume", BookingLine.JSL_DispatchedVolume, BookingLineWrapper.DispatchedVolume);
			AssertEquals("DispatchedWeight", BookingLine.JSL_DispatchedWeight, BookingLineWrapper.DispatchedWeight);
		}

		public void TestLooseCargo()
		{
			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			packLine.JL_JSL_BookingLine = bookingLine.PK;
			packLine.JL_RefNumber = "xxx2";

			bookingLine.SupplierBooking.JSB_Status = SupplierBookingStatusList.Codes.CNV;
			bookingLine.SupplierBooking.JSB_ContainerMode = SupplierBookingLoadModeList.Codes.LSE;
			AssertEquals("LooseCargo", packLine.JL_RefNumber, bookingLine.LooseCargoPackLine.JL_RefNumber);
		}

		public void TestCalcuatedProperties()
		{
			BookingLine.JSL_BookedPackages = 120;
			BookingLine.JSL_ReceivedPackages = 20;
			BookingLine.JSL_RemainingPackagesToBePacked = 10;

			BookingLine.JSL_BookedQuantity = 150;
			BookingLine.JSL_ReceivedQuantity = 30;
			BookingLine.JSL_RemainingQuantityToBePacked = 20;

			BookingLine.JSL_Volume = 100;
			BookingLine.JSL_ReceivedVolume = 50;
			BookingLine.JSL_RemainingVolumeToBePacked = 20;

			BookingLine.JSL_GrossWeight = 90;
			BookingLine.JSL_ReceivedWeight = 30;
			BookingLine.JSL_RemainingWeightToBePacked = 20;

			AssertEquals("OpenPackages", 110m, BookingLineWrapper.OpenPackages);
			AssertEquals("OpenQuantity", 140m, BookingLineWrapper.OpenQuantity);
			AssertEquals("OpenVolume", 70m, BookingLineWrapper.OpenVolume);
			AssertEquals("OpenWeight", 80m, BookingLineWrapper.OpenWeight);
			AssertEquals("RemainingPackagesToBeReceived", 100m, BookingLineWrapper.RemainingPackagesToBeReceived);
			AssertEquals("RemainingQuantityToBeReceived", 120m, BookingLineWrapper.RemainingQuantityToBeReceived);
			AssertEquals("RemainingVolumeToBeReceived", 50m, BookingLineWrapper.RemainingVolumeToBeReceived);
			AssertEquals("RemainingWeightToBeReceived", 60m, BookingLineWrapper.RemainingWeightToBeReceived);
		}

		#region Implementation

		JobSupplierBookingLine BookingLine;
		DocJobSupplierBookingLine BookingLineWrapper;

		protected override void SetUp()
		{
			BookingLine = Factory.New<JobSupplierBookingLine>();
			BookingLine.JSL_JO_OrderLine = Factory.NewWithValidTestData<OrderLine>().PK;
			BookingLineWrapper = DocJobSupplierBookingLine.New(BookingLine, Factory);
			base.SetUp();
		}

		#endregion
	}
}
