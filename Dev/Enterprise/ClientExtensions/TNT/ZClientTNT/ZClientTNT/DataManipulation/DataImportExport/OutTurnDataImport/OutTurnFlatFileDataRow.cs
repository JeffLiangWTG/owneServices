using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TNT.OutTurnDataImport
{
	public class OutTurnFlatFileDataRow : FlatFileDataRow
	{
		public OutTurnFlatFileDataRow()
			: base(Schema.FieldCount)
		{
		}

		public OutTurnFlatFileDataRow(ZString line)
			: base(Schema.FieldCount)
		{
			PopulateData(line);
		}

		public static class Schema
		{
			public const int FieldCount = 13;
			public const int CarrierCode = 0;
			public const int CarrierNumber = 1;
			public const int MAWB = 2;
			public const int LoadPort = 3;
			public const int DiscPort = 4;
			public const int ArrivalDate = 5;
			public const int Mode = 6;
			public const int HAWB = 7;
			public const int ConsignmentOrigin = 8;
			public const int ConsignmentDestination = 9;
			public const int DocumentIndicator = 10;
			public const int ManifestPieces = 11;
			public const int LandedPieces = 12;
		}

		public ZString SectorInformation
		{
			get
			{
				return CarrierCode.Left(2).PadRight(2) +
					CarrierNumber.Left(4).PadRight(4) +
					MAWB.Left(12).PadRight(12) +
					LoadPort.Left(3).PadRight(3) +
					DiscPort.Left(3).PadRight(3) +
					GetField(Schema.ArrivalDate).Left(6).PadRight(6) +
					Mode.Left(1).PadRight(1);
			}
		}

		public ZString FlightNo
		{
			get { return CarrierCode + CarrierNumber; }
		}

		public ZString CarrierCode
		{
			get { return GetField(Schema.CarrierCode); }
		}

		public ZString CarrierNumber
		{
			get { return GetField(Schema.CarrierNumber); }
		}

		public ZString MAWB
		{
			get { return GetField(Schema.MAWB); }
		}

		public ZString LoadPort
		{
			get { return GetField(Schema.LoadPort); }
		}

		public ZString DiscPort
		{
			get { return GetField(Schema.DiscPort); }
		}

		public ZDateTime ArrivalDate
		{
			get { return GetFieldAsZDateTime(Schema.ArrivalDate, TNTConstants.DateFormat); }
		}

		public ZString Mode
		{
			get { return GetField(Schema.Mode); }
		}

		public ZString HAWB
		{
			get { return GetField(Schema.HAWB).Trim(); }
		}

		public ZString ConsignmentOrigin
		{
			get { return GetField(Schema.ConsignmentOrigin); }
		}

		public ZString ConsignmentDestination
		{
			get { return GetField(Schema.ConsignmentDestination); }
		}

		public ZString DocumentIndicator
		{
			get { return GetField(Schema.DocumentIndicator); }
		}

		public ZInt ManifestPieces
		{
			get { return GetFieldAsZInt(Schema.ManifestPieces); }
		}

		public ZInt LandedPieces
		{
			get { return GetFieldAsZInt(Schema.LandedPieces); }
		}

		void PopulateData(ZString line)
		{
			var fields = new OCsvLine(line, '|');
			for (int i = 0; i < Math.Min(fields.FieldValues.Length, FieldCount); i++)
			{
				this[i] = fields.FieldValues[i];
			}
		}
	}
}
