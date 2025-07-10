using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class ShipmentDetailsLine : RecordBodyLine
	{
		public ShipmentDetailsLine(IShipmentData shipmentData)
			: base(shipmentData)
		{
		}

		#region Overrides

		public IShipmentData ShipmentData
		{
			get { return (IShipmentData)base.LineKey; }
		}

		protected override void AppendContentFields(ZStringBuilder lineBuilder)
		{
			AppendFixedLengthField(lineBuilder, ShipmentData.ShipmentRef, Length.ShipmentNumber);
			AppendFixedLengthField(lineBuilder, ShipmentData.ImporterAccountNumber, Length.ImporterAccountNumber);
			AppendFixedLengthField(lineBuilder, ShipmentData.DutyType, Length.DutyType);
			AppendFixedLengthField(lineBuilder, ShipmentData.MasterBillNumber, Length.MAWBNumber);
			AppendFixedLengthField(lineBuilder, ShipmentData.CustomsValue, DecimalPlace.CustomsValue, Length.CustomsValue);
			AppendFixedLengthField(lineBuilder, ShipmentData.StatisticalValue, DecimalPlace.StatisticalValue, Length.StatisticalValue);
			AppendFixedLengthField(lineBuilder, ShipmentData.DVCCurrencyCode, Length.DVCCurrencyCode);
			AppendFixedLengthField(lineBuilder, ShipmentData.CustomsExchangeRate, DecimalPlace.CustomsExchangeRate, Length.CustomsExchangeRate);
			AppendFixedLengthField(lineBuilder, ShipmentData.BISICustomsEntryStatus, Length.CustomsEntryStatus);
			AppendFixedLengthField(lineBuilder, ShipmentData.EntryType, Length.EntryType);
			AppendFixedLengthField(lineBuilder, ShipmentData.CustomsEntryNumber, Length.CustomsEntryNumber);
			if (!ShipmentData.CustomsEntryDate.IsEmpty)
			{
				AppendFixedLengthField(lineBuilder, ShipmentData.CustomsEntryDate, false, Length.CustomsEntryDate);
			}
			else
			{
				AppendFixedLengthField(lineBuilder, "", Length.CustomsEntryDate);
			}
			AppendFixedLengthField(lineBuilder, ShipmentData.CustomsOfficeNumber, Length.CustomsOfficeNumber);
			AppendFixedLengthField(lineBuilder, ShipmentData.VATNumber, Length.VATNumber);
			AppendFixedLengthField(lineBuilder, ShipmentData.ImporterVATDefermentNumber, Length.ImporterVATDefNumber);
			AppendFixedLengthField(lineBuilder, ShipmentData.SplitDutyDefermentNumber, Length.SplitDutyDefNumber);
			AppendFixedLengthField(lineBuilder, "", Length.Filler);
		}

		protected override ZString LineType
		{
			get { return LineTypes.ShipmentDetails; }
		}

		#endregion

		#region Constants

		abstract class Length
		{
			public const int ShipmentNumber = 11;
			public const int ImporterAccountNumber = 10;
			public const int DutyType = 2;
			public const int MAWBNumber = 14;
			public const int CustomsValue = 13;
			public const int StatisticalValue = 13;
			public const int DVCCurrencyCode = 3;
			public const int CustomsExchangeRate = 13;
			public const int CustomsEntryStatus = 2;
			public const int EntryType = 2;
			public const int CustomsEntryNumber = 11;
			public const int CustomsEntryDate = 10;
			public const int CustomsOfficeNumber = 3;
			public const int VATNumber = 12;
			public const int ImporterVATDefNumber = 8;
			public const int SplitDutyDefNumber = 8;
			public const int Filler = 133;
		}

		abstract class DecimalPlace
		{
			public const int CustomsValue = 2;
			public const int StatisticalValue = 2;
			public const int CustomsExchangeRate = 9;
		}

		#endregion
	}
}
