
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.Rohlig.Bellin
{
	public class BellinFlatFileDataRow : FlatFileDataRow
	{
		public BellinFlatFileDataRow() : base(Constants.FieldCount)
		{
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty InvoiceNumber = new FlatFileFieldProperty(0, 50);
			public static readonly FlatFileFieldProperty CounterpartNumber = new FlatFileFieldProperty(1, 20);
			public static readonly FlatFileFieldProperty InvoiceDate = new FlatFileFieldProperty(2, 8);
			public static readonly FlatFileFieldProperty MaturityDate = new FlatFileFieldProperty(3, 8);
			public static readonly FlatFileFieldProperty Amount = new FlatFileFieldProperty(4, 20);
			public static readonly FlatFileFieldProperty Currency = new FlatFileFieldProperty(5, 3);
			public static readonly FlatFileFieldProperty InvoiceFlag = new FlatFileFieldProperty(6, 50);
			public static readonly FlatFileFieldProperty Comment = new FlatFileFieldProperty(7, 250);
			public static readonly FlatFileFieldProperty DeliveryNoteNumber = new FlatFileFieldProperty(8, 50);
			public static readonly FlatFileFieldProperty ExternalDocumentNumber = new FlatFileFieldProperty(9, 50);
			public static readonly FlatFileFieldProperty BookingDate = new FlatFileFieldProperty(10, 8);
			public static readonly FlatFileFieldProperty Remark = new FlatFileFieldProperty(11, 250);
		}

		#endregion

		#region Field Properties

		public ZString InvoiceNumber
		{
			get { return this[Schema.InvoiceNumber.Name]; }
			set { SetField(Schema.InvoiceNumber, value); }
		}

		public ZString CounterpartNumber
		{
			get { return this[Schema.CounterpartNumber.Name]; }
			set { SetField(Schema.CounterpartNumber, value); }
		}

		public ZDateTime InvoiceDate
		{
			get { return base.GetFieldAsZDateTime(Schema.InvoiceDate.Name, Constants.DateFormat); }
			set { SetField(Schema.InvoiceDate, value); }
		}

		public ZDateTime MaturityDate
		{
			get { return base.GetFieldAsZDateTime(Schema.MaturityDate.Name, Constants.DateFormat); }
			set { SetField(Schema.MaturityDate, value); }
		}

		public ZDecimal Amount
		{
			get { return base.GetFieldAsZDecimal(Schema.Amount.Name, Constants.NumberOfDecimalPlaces); }
			set { SetField(Schema.Amount, value); }
		}

		public ZString Currency
		{
			get { return this[Schema.Currency.Name]; }
			set { SetField(Schema.Currency, value); }
		}

		public ZString InvoiceFlag
		{
			get { return this[Schema.InvoiceFlag.Name]; }
			set { SetField(Schema.InvoiceFlag, value); }
		}

		public ZString Comment
		{
			get { return this[Schema.Comment.Name]; }
			set { SetField(Schema.Comment, value); }
		}

		public ZString DeliveryNoteNumber
		{
			get { return this[Schema.DeliveryNoteNumber.Name]; }
			set { SetField(Schema.DeliveryNoteNumber, value); }
		}

		public ZString ExternalDocumentNumber
		{
			get { return this[Schema.ExternalDocumentNumber.Name]; }
			set { SetField(Schema.ExternalDocumentNumber, value); }
		}

		public ZDateTime BookingDate
		{
			get { return GetFieldAsZDateTime(Schema.BookingDate.Name, Constants.DateFormat); }
			set { SetField(Schema.BookingDate, value); }
		}

		public ZString Remark
		{
			get { return this[Schema.Remark.Name]; }
			set { SetField(Schema.Remark, value); }
		}

		#endregion

		#region Implementation

		void SetField(FlatFileFieldProperty fieldProperty, ZString value)
		{
			base.SetField(fieldProperty.Name, value.Left(fieldProperty.Length));
		}

		void SetField(FlatFileFieldProperty fieldProperty, ZDecimal value)
		{
			base.SetField(fieldProperty.Name, value, Constants.NumberOfDecimalPlaces);
		}

		void SetField(FlatFileFieldProperty fieldProperty, ZDateTime date)
		{
			base.SetField(fieldProperty.Name, date, Constants.DateFormat);
		}

		#endregion
	}
}
