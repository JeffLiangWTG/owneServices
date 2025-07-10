using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class ShipmentReceiptLine : RecordBodyLine
	{
		public ShipmentReceiptLine(IShipmentData shipmentData, ShipmentReceiptData receiptData)
			: base(shipmentData)
		{
			this.receiptData = receiptData;
		}
		readonly ShipmentReceiptData receiptData;

		#region Overrides

		protected override ZString LineType
		{
			get { return LineTypes.CountryDetails; }
		}

		protected override void AppendContentFields(ZStringBuilder lineBuilder)
		{
			AppendFixedLengthField(lineBuilder, receiptData.TypeCode, Length.TypeCode);
			AppendFixedLengthField(lineBuilder, receiptData.TypeInfomation, Length.Information);
			AppendFixedLengthField(lineBuilder, "", Length.Filter);
		}

		#endregion

		#region New Properties

		public ZString GIROCode
		{
			get { return receiptData.ReceiptTypeCode == ShipmentReceiptTypeCode.GIROCode ? receiptData.TypeInfomation : ZString.Empty; }
		}

		public ZBool IsTaxCertificate
		{
			get { return receiptData.ReceiptTypeCode == ShipmentReceiptTypeCode.OBCTaxCertificateNumber; }
		}

		#endregion

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public class Length : BaseLength
		{
			public const int TypeCode = 3;
			public const int Information = 20;
			public const int Filter = 245;
		}

		#endregion
	}
}
