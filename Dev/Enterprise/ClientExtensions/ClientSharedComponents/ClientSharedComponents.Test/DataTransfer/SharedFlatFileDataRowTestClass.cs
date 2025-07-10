using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	internal class SharedFlatFileDataRowTestClass : SharedFlatFileDataRow
	{
		public SharedFlatFileDataRowTestClass() : base(Schema.FieldCapacity) { }

		public SharedFlatFileDataRowTestClass(int fieldCapacity) : base(fieldCapacity) { }

		public SharedFlatFileDataRowTestClass(FlatFileDataRow dataRow) : base(dataRow) { }

		public abstract class Schema
		{
			public static readonly FlatFileFieldProperty Field1 = new FlatFileFieldProperty(0, 10);
			public static readonly FlatFileFieldProperty Field2 = new FlatFileFieldProperty(1, 8);
			public static readonly FlatFileFieldProperty Field3 = new FlatFileFieldProperty(2, 12);
			public static readonly FlatFileFieldProperty Field4 = new FlatFileFieldProperty(3, 2);

			internal const int FieldCapacity = 4;
		}

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.Field1);
			FieldProperties.Add(Schema.Field2);
			FieldProperties.Add(Schema.Field3);
			FieldProperties.Add(Schema.Field4);
		}

		public ZDecimal Weight
		{
			get { return GetIntFieldAsZDecimal(Schema.Field1, 1); }
			set { SetZDecimalFieldAsInt(Schema.Field1, value, 1); }
		}

		public ZDecimal Amount
		{
			get { return GetFieldAsZDecimal(Schema.Field3, 2); }
			set { SetField(Schema.Field3, value, 2); }
		}

		public override string DataDateFormat
		{
			get { return "dd/MM/yyyy"; }
		}
	}
}
