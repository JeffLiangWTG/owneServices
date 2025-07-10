using CargoWise.Types;
using Enterprise.ClientSharedComponents;

namespace Enterprise.Client.TGE.Business
{
	internal class CSSDataRow : SharedFlatFileDataRow
	{
		internal CSSDataRow() : base() { }

		[Field(0, 8)]
		public ZString SequenceNumber { get; set; }

		#region File Create Date and Time

		public ZDateTime FileCreateDateTime
		{
			set
			{
				FileCreateDate = value;
				FileCreateTime = value;
			}
		}

		[DateTimeField(1, DateFormat)]
		public ZDateTime FileCreateDate { get; private set; }

		[DateTimeField(2, TimeFormat)]
		public ZDateTime FileCreateTime { get; private set; }

		#endregion

		[Field(3)]
		public ZString Direction { get; set; }

		[Field(4, 20)]
		public ZString MAWBNumber { get; set; }

		[Field(5, 8)]
		public ZString FlightNumber { get; set; }

		[DateTimeField(6, DateFormat)]
		public ZDateTime DepartureOrArrivalDate { get; set; }

		[Field(7, 5)]
		public ZString OriginCode { get; set; }

		[Field(8, 5)]
		public ZString DestinationCode { get; set; }

		[Field(9, 20)]
		public ZString HAWBNumber { get; set; }

		[Field(10, 40)]
		public ZString ShipmentCodeOrDescriptionStatus { get; set; }

		[Field(11, 20)]
		public ZString ProductCodeOrDescription { get; set; }

		[DecimalField(12, 10, 2)]
		public ZDecimal GoodsValue { get; set; }

		[Field(13, 3)]
		public ZString GoodsCurrency { get; set; }

		[Field(14, 20)]
		public ZString CustomsAuthorityNumber { get; set; }

		public const string DateFormat = "yyyyMMdd";
		public const string TimeFormat = "HHmm";
	}
}
