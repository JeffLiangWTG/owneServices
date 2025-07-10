using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer
{
	public abstract class CASSHOTFileLineRow : CASSHOTFileDataRow
	{
		protected CASSHOTFileLineRow(int fieldCount)
			: base(fieldCount)
		{
		}

		#region Properties

		public ZString AirlinePrefix
		{
			get { return GetField(Schema.AirlinePrefix); }
		}

		public ZString AWBSerialNumber
		{
			get { return GetField(Schema.AWBSerialNumber); }
		}

		public ZString AgentCode
		{
			get { return GetField(Schema.AgentCode); }
		}

		public ZDateTime DateAWBExecution
		{
			get { return GetFieldAsZDateTime(Schema.DateAWBExecution, "yyMMdd"); }
		}

		public ZDateTime DateOfArrival
		{
			get { return GetFieldAsZDateTime(Schema.DateOfArrival, "yyMMdd"); }
		}

		public ZDateTime DateOfDelivery
		{
			get { return GetFieldAsZDateTime(Schema.DateOfDelivery, "yyMMdd"); }
		}

		public ZString Origin
		{
			get { return GetField(Schema.Origin); }
		}

		public ZString Destination
		{
			get { return GetField(Schema.Destination); }
		}

		#region Weight

		public ZDecimal Weight
		{
			get { return GetFieldAsZDecimal(Schema.Weight) / (WeightUnit == "KG" ? 10 : 1); }
		}

		public ZString WeightUnit
		{
			get
			{
				ZString unit = ZString.Empty;
				switch (GetField(Schema.WeightIndicator))
				{
					case "K":
						unit = "KG";
						break;
					case "L":
						unit = "LB";
						break;
				}
				return unit;
			}
		}

		#endregion

		public ZString Currency
		{
			get { return GetField(Schema.CurrencyCode); }
		}

		#region VAT

		public ZString VATIndicator
		{
			get { return VATIndicatorCore; }
		}

		protected abstract ZString VATIndicatorCore { get; }

		#endregion

		#endregion

		protected new const int NextSchemaFieldNumber = CASSHOTFileDataRow.NextSchemaFieldNumber + 11;

		public new abstract class Schema : CASSHOTFileDataRow.Schema
		{
			public const int AirlinePrefix = CASSHOTFileDataRow.NextSchemaFieldNumber;
			public const int AWBSerialNumber = CASSHOTFileDataRow.NextSchemaFieldNumber + 1;
			public const int AgentCode = CASSHOTFileDataRow.NextSchemaFieldNumber + 2;
			public const int DateAWBExecution = CASSHOTFileDataRow.NextSchemaFieldNumber + 3;
			public const int Origin = CASSHOTFileDataRow.NextSchemaFieldNumber + 4;
			public const int Destination = CASSHOTFileDataRow.NextSchemaFieldNumber + 5;
			public const int Weight = CASSHOTFileDataRow.NextSchemaFieldNumber + 6;
			public const int WeightIndicator = CASSHOTFileDataRow.NextSchemaFieldNumber + 7;
			public const int CurrencyCode = CASSHOTFileDataRow.NextSchemaFieldNumber + 8;
			public const int DateOfArrival = CASSHOTFileDataRow.NextSchemaFieldNumber + 9;
			public const int DateOfDelivery = CASSHOTFileDataRow.NextSchemaFieldNumber + 10;
		}
	}
}
