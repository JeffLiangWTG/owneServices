using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	public abstract class SagFlatFileDataRowTest : TestCase
	{
		public void TestSetField()
		{
			SagFlatFileDataRowTestClass row = new SagFlatFileDataRowTestClass(4);
			row.SetField(Field1, 123456.7890m);
			row.SetField(Field2, new ZDateTime(2006, 2, 8, 17, 12, 42));
			row.SetField(Field3, "5 Level 2 Cote Road");
			row.SetField(Field4, (ZInt)69);
			AssertEquals("Field 1", 123456.79m, row.GetFieldAsZDecimal(Field1.Name, 2));
			AssertEquals("Field 2", new ZDateTime(2006, 2, 8), row.GetFieldAsZDateTime(Field2.Name, ELGConstants.DataDateFormat));
			AssertEquals("Field 3", "5 Level 2 Co", row.GetField(Field3.Name));
			AssertEquals("Field 4", 69, row.GetFieldAsZInt(Field4.Name));
			AssertEquals("Length", 32, row.Length);
		}

		readonly static FlatFileFieldProperty Field1 = new FlatFileFieldProperty(0, 10);
		readonly static FlatFileFieldProperty Field2 = new FlatFileFieldProperty(1, 8);
		readonly static FlatFileFieldProperty Field3 = new FlatFileFieldProperty(2, 12);
		readonly static FlatFileFieldProperty Field4 = new FlatFileFieldProperty(3, 2);
		internal class SagFlatFileDataRowTestClass : SagFlatFileDataRow
		{
			public SagFlatFileDataRowTestClass(int fieldCount) : base(fieldCount)
			{
			}

			protected override void AddFieldProperties()
			{
				FieldProperties.Add(Field1);
				FieldProperties.Add(Field2);
				FieldProperties.Add(Field3);
				FieldProperties.Add(Field4);
			}
		}
	}
}
