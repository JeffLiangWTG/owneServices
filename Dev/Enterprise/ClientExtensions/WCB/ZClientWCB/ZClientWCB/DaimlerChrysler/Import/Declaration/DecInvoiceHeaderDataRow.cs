using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.WCB.DaimlerChrysler
{
	class DecInvoiceHeaderDataRow : FixedWidthFlatFileDataRow
	{
		public DecInvoiceHeaderDataRow()
			: base(DecInvoiceHeaderDataRow.Schema.FieldCount)
		{
		}

		public DecInvoiceHeaderDataRow(ZString lineData)
			: base(DecInvoiceHeaderDataRow.Schema.FieldCount, lineData)
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

		internal ZString VesselCode
		{
			get { return GetField(Schema.VesselCode); }
			set { SetField(Schema.VesselCode, value); }
		}

		internal ZString VesselName
		{
			get { return GetField(Schema.VesselName); }
			set { SetField(Schema.VesselName, value); }
		}

		internal ZString Voyage
		{
			get { return GetField(Schema.Voyage); }
			set { SetField(Schema.Voyage, value); }
		}

		internal ZString RegionalAllocation
		{
			get { return GetField(Schema.RegionalAllocation); }
			set { SetField(Schema.RegionalAllocation, value); }
		}

		internal ZInt CountOfVehicles
		{
			get { return GetFieldAsZInt(Schema.CountOfVehicles); }
			set { SetField(Schema.CountOfVehicles, value); }
		}

		internal ZDecimal FOBAmount
		{
			get { return GetFieldAsZDecimal(Schema.FOBAmount, 2) / 100; }
			set { SetField(Schema.FOBAmount, value * 100); }
		}

		internal ZString OceanBill
		{
			get { return GetField(Schema.OceanBill); }
			set { SetField(Schema.OceanBill, value); }
		}
		#endregion

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.RecordType);
			FieldProperties.Add(Schema.VesselCode);
			FieldProperties.Add(Schema.VesselName);
			FieldProperties.Add(Schema.Voyage);
			FieldProperties.Add(Schema.RegionalAllocation);
			FieldProperties.Add(Schema.CountOfVehicles);
			FieldProperties.Add(Schema.FOBAmount);
			FieldProperties.Add(Schema.OceanBill);
		}

		internal const string InvoiceHeaderCode = "D1";

		static class Schema
		{
			internal static readonly FlatFileFieldProperty RecordType = new FlatFileFieldProperty(0, 2);
			internal static readonly FlatFileFieldProperty VesselCode = new FlatFileFieldProperty(1, 6);
			internal static readonly FlatFileFieldProperty VesselName = new FlatFileFieldProperty(2, 20);
			internal static readonly FlatFileFieldProperty Voyage = new FlatFileFieldProperty(3, 6);
			internal static readonly FlatFileFieldProperty RegionalAllocation = new FlatFileFieldProperty(4, 6);
			internal static readonly FlatFileFieldProperty CountOfVehicles = new FlatFileFieldProperty(5, 4);
			internal static readonly FlatFileFieldProperty FOBAmount = new FlatFileFieldProperty(6, 11);
			internal static readonly FlatFileFieldProperty OceanBill = new FlatFileFieldProperty(7, 30);
			internal const int FieldCount = 8;
		}
	}
}
