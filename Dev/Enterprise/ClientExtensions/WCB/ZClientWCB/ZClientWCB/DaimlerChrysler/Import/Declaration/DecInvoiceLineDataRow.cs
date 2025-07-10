using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.WCB.DaimlerChrysler
{
	class DecInvoiceLineDataRow : FixedWidthFlatFileDataRow
	{
		public DecInvoiceLineDataRow()
			: base(DecInvoiceLineDataRow.Schema.FieldCount)
		{
		}

		public DecInvoiceLineDataRow(ZString lineData)
			: base(DecInvoiceLineDataRow.Schema.FieldCount, lineData)
		{
		}

		public override string DataDateFormat
		{
			get { return dataDateFormat; }
		}
		const string dataDateFormat = "yyyyMMdd";

		#region Field Property
		//	Record Type Specifier must be 'D3'
		internal ZString RecordType
		{
			get { return GetField(Schema.RecordType); }
			set { SetField(Schema.RecordType, value); }
		}

		internal ZString InvoiceOrderNo
		{
			get { return GetField(Schema.InvoiceOrderNo); }
			set { SetField(Schema.InvoiceOrderNo, value); }
		}

		internal ZString InvoiceNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (!InvoiceOrderNo.IsEmpty)
				{
					result = InvoiceOrderNo.Split(delimiter)[0];
				}
				return result;
			}
		}

		internal ZString OrderNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (!InvoiceOrderNo.IsEmpty && InvoiceOrderNo.Contains(delimiter))
				{
					result = InvoiceOrderNo.Split(delimiter)[1];
				}
				return result;
			}
		}

		internal ZString CommissionNo
		{
			get { return GetField(Schema.CommissionNo); }
			set { SetField(Schema.CommissionNo, value); }
		}

		internal ZString ChassisNo
		{
			get { return GetField(Schema.ChassisNo); }
			set { SetField(Schema.ChassisNo, value); }
		}

		internal ZString Model
		{
			get { return GetField(Schema.Model); }
			set { SetField(Schema.Model, value); }
		}

		internal ZString Colour
		{
			get { return GetField(Schema.Colour); }
			set { SetField(Schema.Colour, value); }
		}

		internal ZDecimal FOBAmount
		{
			get { return GetFieldAsZDecimal(Schema.FOBAmount, 2) / 100; }
			set { SetField(Schema.FOBAmount, value * 100); }
		}
		#endregion

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.RecordType);
			FieldProperties.Add(Schema.InvoiceOrderNo);
			FieldProperties.Add(Schema.CommissionNo);
			FieldProperties.Add(Schema.ChassisNo);
			FieldProperties.Add(Schema.Model);
			FieldProperties.Add(Schema.Colour);
			FieldProperties.Add(Schema.FOBAmount);
		}

		internal const string InvoiceLineCode = "D2";
		const char delimiter = '/';

		static class Schema
		{
			internal static readonly FlatFileFieldProperty RecordType = new FlatFileFieldProperty(0, 2);
			internal static readonly FlatFileFieldProperty InvoiceOrderNo = new FlatFileFieldProperty(1, 15);
			internal static readonly FlatFileFieldProperty CommissionNo = new FlatFileFieldProperty(2, 12);
			internal static readonly FlatFileFieldProperty ChassisNo = new FlatFileFieldProperty(3, 6);
			internal static readonly FlatFileFieldProperty Model = new FlatFileFieldProperty(4, 18);
			internal static readonly FlatFileFieldProperty Colour = new FlatFileFieldProperty(5, 5);
			internal static readonly FlatFileFieldProperty FOBAmount = new FlatFileFieldProperty(6, 11);
			internal const int FieldCount = 7;
		}
	}
}
