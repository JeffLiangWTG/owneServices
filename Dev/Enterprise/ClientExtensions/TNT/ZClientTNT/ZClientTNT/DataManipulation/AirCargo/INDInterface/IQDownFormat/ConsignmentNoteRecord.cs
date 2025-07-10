using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TNT
{
	public class ConsignmentNoteRecord : IQDownBaseRecord
	{
		public ConsignmentNoteRecord(ZString line)
			: base(line, 12)
		{
		}

		#region Schema

		public static class Schema
		{
			public static readonly FixedWidthFlatFileFieldProperty RecordType = new FixedWidthFlatFileFieldProperty(0, 0, 2);       //	Record Type Specifier must be '04'
			public static readonly FixedWidthFlatFileFieldProperty HouseBill = new FixedWidthFlatFileFieldProperty(1, 2, 10);   //	House Bill Number
			public static readonly FixedWidthFlatFileFieldProperty Sequence = new FixedWidthFlatFileFieldProperty(2, 12, 2);    //	Sequence
			public static readonly FixedWidthFlatFileFieldProperty TariffNumber = new FixedWidthFlatFileFieldProperty(3, 14, 15);   //	Tariff Number
			public static readonly FixedWidthFlatFileFieldProperty Description1 = new FixedWidthFlatFileFieldProperty(4, 29, 78);   //	Description 1
			public static readonly FixedWidthFlatFileFieldProperty Description2 = new FixedWidthFlatFileFieldProperty(5, 107, 78);  //	Description 2
			public static readonly FixedWidthFlatFileFieldProperty Description3 = new FixedWidthFlatFileFieldProperty(6, 185, 78);  //	Description 3
			public static readonly FixedWidthFlatFileFieldProperty Origin = new FixedWidthFlatFileFieldProperty(7, 263, 3); //	Port of Origin
			public static readonly FixedWidthFlatFileFieldProperty Destination = new FixedWidthFlatFileFieldProperty(8, 266, 3);    //	Port of Destination
			public static readonly FixedWidthFlatFileFieldProperty ECN = new FixedWidthFlatFileFieldProperty(9, 269, 13);   //	ECN/EDN Number
			public static readonly FixedWidthFlatFileFieldProperty space = new FixedWidthFlatFileFieldProperty(10, 282, 207);   //	space
			public static readonly FixedWidthFlatFileFieldProperty RecordDelimiter = new FixedWidthFlatFileFieldProperty(11, 489, 1);   //	Record delimiter must be '.'
		}

		#endregion

		#region Field Property

		public ZString RecordType
		{
			get { return this[Schema.RecordType.Name]; }
		}

		public virtual ZString HouseBill
		{
			get { return this[Schema.HouseBill.Name]; }
			set { this[Schema.HouseBill.Name] = value; }
		}

		public virtual ZInt Sequence
		{
			get { return GetFieldAsZInt(Schema.Sequence.Name); }
			set { this[Schema.Sequence.Name] = value.ToString(); }
		}

		public virtual ZString TariffNumber
		{
			get { return this[Schema.TariffNumber.Name]; }
			set { this[Schema.TariffNumber.Name] = value; }
		}

		public virtual ZString NoteText
		{
			get
			{
				ZString result = Description1.Trim() + " " + Description2.Trim() + " " + Description3.Trim();
				return result.Trim();
			}
		}

		public virtual ZString Description1
		{
			get { return this[Schema.Description1.Name]; }
			set { this[Schema.Description1.Name] = value; }
		}

		public virtual ZString Description2
		{
			get { return this[Schema.Description2.Name]; }
			set { this[Schema.Description2.Name] = value; }
		}

		public virtual ZString Description3
		{
			get { return this[Schema.Description3.Name]; }
			set { this[Schema.Description3.Name] = value; }
		}

		public virtual ZString Origin
		{
			get { return this[Schema.Origin.Name]; }
			set { this[Schema.Origin.Name] = value; }
		}

		public virtual ZString Destination
		{
			get { return this[Schema.Destination.Name]; }
			set { this[Schema.Destination.Name] = value; }
		}

		public virtual ZString ECN
		{
			get { return this[Schema.ECN.Name]; }
			set { this[Schema.ECN.Name] = value; }
		}

		public ZString spaces
		{
			get { return this[Schema.space.Name]; }
		}

		public override ZString RecordDelimiter
		{
			get { return this[Schema.RecordDelimiter.Name]; }
		}

		public override ZString HumanReadable
		{
			get { return ZString.Format("Record {0} (Con Number={1}, Sequence={2})", RecordType, HouseBill, Sequence.ToString()); }
		}

		#endregion

		#region Implementation

		protected override void FillFromRawData(ZString rawData)
		{
			SetProperty(Schema.RecordType, rawData);
			SetProperty(Schema.HouseBill, rawData);
			SetProperty(Schema.Sequence, rawData);
			SetProperty(Schema.TariffNumber, rawData);
			SetProperty(Schema.Description1, rawData);
			SetProperty(Schema.Description2, rawData);
			SetProperty(Schema.Description3, rawData);
			SetProperty(Schema.Origin, rawData);
			SetProperty(Schema.Destination, rawData);
			SetProperty(Schema.ECN, rawData);
			SetProperty(Schema.space, rawData);
			SetProperty(Schema.RecordDelimiter, rawData);
		}

		public const string DateFormat = "ddMMyy";

		#endregion
		#region TestCase
		#endregion

	}
}
