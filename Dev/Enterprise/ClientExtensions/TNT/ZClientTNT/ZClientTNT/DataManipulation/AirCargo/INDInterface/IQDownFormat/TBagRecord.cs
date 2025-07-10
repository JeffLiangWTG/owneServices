using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TNT
{
	public class TBagRecord : IQDownBaseRecord
	{
		#region Schema

		public static class Schema
		{
			public static readonly FixedWidthFlatFileFieldProperty RecordType = new FixedWidthFlatFileFieldProperty(0, 0, 2);       // 1 2 Record Type Specifier must be '02'
			public static readonly FixedWidthFlatFileFieldProperty MBagNo = new FixedWidthFlatFileFieldProperty(1, 2, 10);  // 3 12 MBag Number
			public static readonly FixedWidthFlatFileFieldProperty TBagNo = new FixedWidthFlatFileFieldProperty(2, 12, 10); // 13 22 TBag Number
			public static readonly FixedWidthFlatFileFieldProperty TBagOrigin = new FixedWidthFlatFileFieldProperty(3, 22, 3);  // 23 25 TBag Origin
			public static readonly FixedWidthFlatFileFieldProperty TBagDestination = new FixedWidthFlatFileFieldProperty(4, 25, 3); // 26 28 TBag Destination
			public static readonly FixedWidthFlatFileFieldProperty spaces = new FixedWidthFlatFileFieldProperty(5, 28, 461);    // 29 489 spaces
			public static readonly FixedWidthFlatFileFieldProperty RecordDelimiter = new FixedWidthFlatFileFieldProperty(6, 489, 1);    // 490 490 Record delimiter must be '.'
		}

		#endregion

		public TBagRecord(ZString line)
			: base(line, 7)
		{
		}

		#region Field Property

		public ZString RecordType
		{
			get { return this[Schema.RecordType.Name]; }
		}

		public virtual ZString MBagNo
		{
			get { return this[Schema.MBagNo.Name]; }
			set { this[Schema.MBagNo.Name] = value; }
		}

		public virtual ZString TBagNo
		{
			get { return this[Schema.TBagNo.Name]; }
			set { this[Schema.TBagNo.Name] = value; }
		}

		public virtual ZString TBagOrigin
		{
			get { return this[Schema.TBagOrigin.Name]; }
			set { this[Schema.TBagOrigin.Name] = value; }
		}

		public virtual ZString TBagDestination
		{
			get { return this[Schema.TBagDestination.Name]; }
			set { this[Schema.TBagDestination.Name] = value; }
		}

		public ZString spaces
		{
			get { return this[Schema.spaces.Name]; }
		}

		public override ZString RecordDelimiter
		{
			get { return this[Schema.RecordDelimiter.Name]; }
		}

		public override ZString HumanReadable
		{
			get { return ZString.Format("Record {0} (MBagNo={1}, TBagNo={2})", RecordType, MBagNo, TBagNo); }
		}

		#endregion

		#region Implementation

		protected override void FillFromRawData(ZString rawData)
		{
			SetProperty(Schema.RecordType, rawData);
			SetProperty(Schema.MBagNo, rawData);
			SetProperty(Schema.TBagNo, rawData);
			SetProperty(Schema.TBagOrigin, rawData);
			SetProperty(Schema.TBagDestination, rawData);
			SetProperty(Schema.spaces, rawData);
			SetProperty(Schema.RecordDelimiter, rawData);
		}

		#endregion
		#region TestCase
		#endregion

	}
}
