using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	#region enum ShipmentReceiptTypeCode

	public enum ShipmentReceiptTypeCode
	{
		Unknown,
		GIROCode = 4,
		MAWBNumber = 5,
		CycleDate = 6,
		CycleNumber = 7,
		OBCTaxCertificateNumber = 8,
		OBCPayDeclarationNumber = 9,
		InvoiceQuantity = 10
	}

	#endregion

	public static class ReceiptGIROCodeTypes
	{
		public const string GSTExempted = "G01";
		public const string GSTPaidAtCheckPoint = "G02";
		public const string GSTWaived = "G03";
		public const string GSTPaidDeducted = "G04";
		public const string GSTPaidThruGIRO = "G05";
	}

	public class ShipmentReceiptData
	{
		public ShipmentReceiptData(ShipmentReceiptTypeCode receiptTypeCode, ZString information)
		{
			this.receiptTypeCode = receiptTypeCode;
			this.information = information;
		}
		readonly ShipmentReceiptTypeCode receiptTypeCode;
		readonly ZString information;

		public ZString TypeCode
		{
			get { return ((int)receiptTypeCode).ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'); }
		}

		public ZString TypeInfomation
		{
			get { return information; }
		}

		public ShipmentReceiptTypeCode ReceiptTypeCode
		{
			get { return receiptTypeCode; }
		}
	}
}
