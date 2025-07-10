
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class ShipmentStatusLine : RecordBodyLine
	{
		public ShipmentStatusLine(IShipmentStatusData shipmentStatusData)
			: base(shipmentStatusData)
		{
		}

		#region Overrides

		public IShipmentStatusData ShipmentStatusData
		{
			get
			{
				return (IShipmentStatusData)base.LineKey;
			}
		}

		protected override ZString LineType
		{
			get
			{
				return LineTypes.ShipmentStatusDetails;
			}
		}

		protected override void AppendContentFields(ZStringBuilder lineBuilder)
		{
			AppendFixedLengthField(lineBuilder, ShipmentStatusData.ShipmentStatus, Length.ShipmentStatus);
			AppendFixedLengthField(lineBuilder, ShipmentStatusData.HoldReasonCode, Length.HoldReasonCode);
			var inspectFlag = (ShipmentStatusData.ShipmentStatus == "04") ? ZBool.False : ShipmentStatusData.InspectIndicator;
			AppendFixedLengthField(lineBuilder,inspectFlag, Length.InspectIndicator);

			AppendFixedLengthField(lineBuilder, ShipmentStatusData.AddressCorrectionIndicator, Length.AddressCorrectionIndicator);
			if (!ShipmentStatusData.ImportReleaseDate.IsEmpty)
			{
				AppendFixedLengthField(lineBuilder, ShipmentStatusData.ImportReleaseDate, false, Length.ImportReleaseDate);
			}
			else
			{
				AppendFixedLengthField(lineBuilder, "", Length.ImportReleaseDate);
			}
			AppendFixedLengthField(lineBuilder, ShipmentStatusData.CustomsRefNo, Length.CustomsRefNo);
			AppendFixedLengthField(lineBuilder, ShipmentStatusData.BrokerCode, Length.BrokerCode);
			AppendFixedLengthField(lineBuilder, ShipmentStatusData.Remarks.Replace("\r", " ").Replace("\n", " ").TrimEnd(), Length.Remarks);
			AppendFixedLengthField(lineBuilder, ShipmentStatusData.ExceptionStatusCode, Length.ExceptionStatusCode);
			AppendFixedLengthField(lineBuilder, ShipmentStatusData.ExceptionResolutionCode, Length.ExceptionResolutionCode);
			AppendFixedLengthField(lineBuilder, "", Length.Filler);
		}

		#endregion

		#region Constants

		public class Length : BaseLength
		{
			public const int ShipmentStatus = 2;
			public const int HoldReasonCode = 2;
			public const int InspectIndicator = 1;
			public const int AddressCorrectionIndicator = 1;
			public const int ImportReleaseDate = 10;
			public const int CustomsRefNo = 13;
			public const int BrokerCode = 3;
			public const int Remarks = 70;
			public const int ExceptionStatusCode = 2;
			public const int ExceptionResolutionCode = 2;
			public const int Filler = 130;
		}

		#endregion
	}
}
