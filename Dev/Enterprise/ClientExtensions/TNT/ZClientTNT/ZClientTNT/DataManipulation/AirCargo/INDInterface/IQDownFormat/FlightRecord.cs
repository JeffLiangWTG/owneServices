
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TNT
{
	public class FlightRecord : IQDownBaseRecord
	{
		public FlightRecord(ZString line)
			: base(line, 15)
		{
		}

		#region Schema

		public static class Schema
		{
			public static readonly FixedWidthFlatFileFieldProperty RecordType = new FixedWidthFlatFileFieldProperty(0, 0, 2);       // 1 2 Record Type Specifier must be '01'
			public static readonly FixedWidthFlatFileFieldProperty Carrier = new FixedWidthFlatFileFieldProperty(1, 2, 2);      // 3 4 Carrier
			public static readonly FixedWidthFlatFileFieldProperty CarrierNumber = new FixedWidthFlatFileFieldProperty(2, 4, 4);        // 5 8 Carrier Number
			public static readonly FixedWidthFlatFileFieldProperty PortOfLoading = new FixedWidthFlatFileFieldProperty(3, 8, 3);        // 9 11 PortOfLoading
			public static readonly FixedWidthFlatFileFieldProperty PortOfDischarge = new FixedWidthFlatFileFieldProperty(4, 11, 3); // 12 14 PortOfDischarge
			public static readonly FixedWidthFlatFileFieldProperty FlightDate = new FixedWidthFlatFileFieldProperty(5, 14, 6);  // 15 20 Flight Date
			public static readonly FixedWidthFlatFileFieldProperty Mode = new FixedWidthFlatFileFieldProperty(6, 20, 1);    // 21 21 Mode
			public static readonly FixedWidthFlatFileFieldProperty MasterBill = new FixedWidthFlatFileFieldProperty(7, 21, 12); // 22 33 MasterBill
			public static readonly FixedWidthFlatFileFieldProperty MBagNo = new FixedWidthFlatFileFieldProperty(8, 33, 10); // 34 43 MBag Number
			public static readonly FixedWidthFlatFileFieldProperty MBagOrigin = new FixedWidthFlatFileFieldProperty(9, 43, 3);  // 4 46 MBag Origin
			public static readonly FixedWidthFlatFileFieldProperty MBagDestination = new FixedWidthFlatFileFieldProperty(10, 46, 3);    // 7 49 MBag Destination
			public static readonly FixedWidthFlatFileFieldProperty MBagType = new FixedWidthFlatFileFieldProperty(11, 49, 2);   // 50 51 MBag Type
			public static readonly FixedWidthFlatFileFieldProperty MBagWeight = new FixedWidthFlatFileFieldProperty(12, 51, 10);    // 52 61 MBag Weight
			public static readonly FixedWidthFlatFileFieldProperty spaces = new FixedWidthFlatFileFieldProperty(13, 61, 427);   // 62 489 spaces
			public static readonly FixedWidthFlatFileFieldProperty RecordDelimiter = new FixedWidthFlatFileFieldProperty(14, 489, 1);   // 490 490 Record delimiter must be '.'
		}

		#endregion

		#region Field Property

		public ZString RecordType
		{
			get { return this[Schema.RecordType.Name]; }
		}

		public virtual ZString FlightNumber
		{
			get { return Carrier.Trim() + CarrierNumber.Trim(); }
			set
			{
				Carrier = value.Left(2);
				CarrierNumber = value.SubstringSafe(2);
			}
		}

		public virtual ZString Carrier
		{
			get { return this[Schema.Carrier.Name]; }
			set { this[Schema.Carrier.Name] = value; }
		}

		public virtual ZString CarrierNumber
		{
			get { return this[Schema.CarrierNumber.Name]; }
			set { this[Schema.CarrierNumber.Name] = value; }
		}

		public virtual ZString PortOfLoading
		{
			get { return this[Schema.PortOfLoading.Name]; }
			set { this[Schema.PortOfLoading.Name] = value; }
		}

		public virtual ZString PortOfDischarge
		{
			get { return this[Schema.PortOfDischarge.Name]; }
			set { this[Schema.PortOfDischarge.Name] = value; }
		}

		public virtual ZDateTime FlightDate
		{
			get { return GetFieldAsZDateTime(Schema.FlightDate.Name, TNTConstants.DateFormat); }
			set { this[Schema.FlightDate.Name] = value.ToString(TNTConstants.DateFormat); }
		}

		public virtual ZString Mode
		{
			get { return this[Schema.Mode.Name]; }
			set { this[Schema.Mode.Name] = value; }
		}

		public virtual ZString MasterBill
		{
			get { return this[Schema.MasterBill.Name]; }
			set { this[Schema.MasterBill.Name] = value; }
		}

		public virtual ZString MBagNo
		{
			get { return this[Schema.MBagNo.Name]; }
			set { this[Schema.MBagNo.Name] = value; }
		}

		public virtual ZString MBagOrigin
		{
			get { return this[Schema.MBagOrigin.Name]; }
			set { this[Schema.MBagOrigin.Name] = value; }
		}

		public virtual ZString MBagDestination
		{
			get { return this[Schema.MBagDestination.Name]; }
			set { this[Schema.MBagDestination.Name] = value; }
		}

		public virtual ZString MBagType
		{
			get { return this[Schema.MBagType.Name]; }
			set { this[Schema.MBagType.Name] = value; }
		}

		public virtual ZDecimal MBagWeight
		{
			get { return GetFieldAsZDecimal(Schema.MBagWeight.Name, 3); }
			set { this[Schema.MBagWeight.Name] = value.ToString("######.###"); }
		}

		public virtual ZString spaces
		{
			get { return this[Schema.spaces.Name]; }
		}

		public override ZString RecordDelimiter
		{
			get { return this[Schema.RecordDelimiter.Name]; }
		}

		public override ZString HumanReadable
		{
			get { return ZString.Format("Record {0} (MasterBill={1})", RecordType, MasterBill); }
		}

		public ZString RecordKey
		{
			get
			{
				return FixedPadRight(FlightNumber, 6) + FixedPadRight(MasterBill, 12) + FixedPadRight(PortOfLoading, 3) +
					FixedPadRight(PortOfDischarge, 3) + FixedPadRight(this[Schema.FlightDate.Name], 6) + FixedPadRight(Mode, 1);
			}
		}

		#endregion

		#region Implementation

		protected override void FillFromRawData(ZString rawData)
		{
			SetProperty(Schema.RecordType, rawData);
			SetProperty(Schema.Carrier, rawData);
			SetProperty(Schema.CarrierNumber, rawData);
			SetProperty(Schema.PortOfLoading, rawData);
			SetProperty(Schema.PortOfDischarge, rawData);
			SetProperty(Schema.FlightDate, rawData);
			SetProperty(Schema.Mode, rawData);
			SetProperty(Schema.MasterBill, rawData);
			SetProperty(Schema.MBagNo, rawData);
			SetProperty(Schema.MBagOrigin, rawData);
			SetProperty(Schema.MBagDestination, rawData);
			SetProperty(Schema.MBagType, rawData);
			SetProperty(Schema.MBagWeight, rawData);
			SetProperty(Schema.spaces, rawData);
			SetProperty(Schema.RecordDelimiter, rawData);
		}

		ZString FixedPadRight(ZString value, int length)
		{
			return value.Left(length).PadRight(length);
		}

		#endregion
		#region TestCase
		#endregion

	}
}
