using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class FixedWidthFaltFileDataRowTest : TestCase
	{
		public void TestNewWaySameAsOldWayFixedWidthFields()
		{
			OriginalWayFWDataRowForTest oldRow1 = new OriginalWayFWDataRowForTest("004938.636A short note   20090710");

			OriginalWayFWDataRowForTest oldRow2 = new OriginalWayFWDataRowForTest(4);
			oldRow2.SetField(OriginalWayFWDataRowForTest.Schema.Weight, 4938.61m, 1);
			oldRow2.SetField(OriginalWayFWDataRowForTest.Schema.Age, 36);
			oldRow2.Note = "A short note";
			oldRow2.SetField(OriginalWayFWDataRowForTest.Schema.Created, new ZDateTime(2009, 7, 10, 10, 32, 0), "yyyyMMdd");

			NewWayFWDataRowForTest newRow1 = new NewWayFWDataRowForTest("004938.636A short note   20090710");

			NewWayFWDataRowForTest newRow2 = new NewWayFWDataRowForTest();
			newRow2.Weight = 4938.61m;
			newRow2.Age = 36;
			newRow2.Note = "A short note";
			newRow2.Created = new ZDateTime(2009, 7, 10, 10, 32, 0);
			newRow2.PopulateFields();

			AssertEquals("Rows equal", oldRow1.ToString(), oldRow2.ToString());
			AssertEquals("Rows equal", newRow1.ToString(), newRow2.ToString());
			AssertEquals("Rows equal", oldRow1.ToString(), newRow1.ToString());
			AssertEquals("Rows equal", oldRow2.ToString(), newRow2.ToString());

			string expectedOutput = "004938.636A short note   20090710";
			string actualOutput = newRow2.ToString();
			AssertEquals("DataLine is formatted", expectedOutput, actualOutput);

			AssertEquals("Field Weight", "004938.6", newRow1["Weight"]);
			AssertEquals("Field Age", "36", newRow1["Age"]);
			AssertEquals("Field Note", "A short note   ", newRow1["Note"]);
			AssertEquals("Field Created", "20090710", newRow1["Created"]);
			AssertEquals("Length", 33, newRow1.Length);
			AssertEquals("Count", 4, newRow1.FieldCount);
		}

		public class OriginalWayFWDataRowForTest : FixedWidthFlatFileDataRow
		{
			public OriginalWayFWDataRowForTest(int fieldCount) : base(fieldCount) { }

			public OriginalWayFWDataRowForTest(ZString lineData) : base(4, lineData) { }

			public abstract class Schema
			{
				public static readonly FlatFileFieldProperty Weight = new FlatFileFieldProperty(0, 8);
				public static readonly FlatFileFieldProperty Age = new FlatFileFieldProperty(1, 2);
				public static readonly FlatFileFieldProperty Note = new FlatFileFieldProperty(2, 15);
				public static readonly FlatFileFieldProperty Created = new FlatFileFieldProperty(3, 8);
			}

			protected override void AddFieldProperties()
			{
				FieldProperties.Add(Schema.Weight);
				FieldProperties.Add(Schema.Age);
				FieldProperties.Add(Schema.Note);
				FieldProperties.Add(Schema.Created);
			}

			public ZDecimal Weight
			{
				get { return GetIntFieldAsZDecimal(Schema.Weight, 1); }
				set { SetZDecimalFieldAsInt(Schema.Weight, value, 1); }
			}

			public ZInt Age
			{
				get { return GetFieldAsZInt(Schema.Age); }
				set { SetField(Schema.Age, value); }
			}

			public ZString Note
			{
				get { return GetField(Schema.Note); }
				set { SetField(Schema.Note, value); }
			}

			public ZDateTime Created
			{
				get { return GetFieldAsZDateTime(Schema.Created); }
				set { SetField(Schema.Created, value, "yyyyMMdd"); }
			}

			protected override bool ZeroFillAllNumericFields
			{
				get { return true; }
			}
		}

		internal class NewWayFWDataRowForTest : FixedWidthFlatFileDataRow
		{
			public NewWayFWDataRowForTest() : base() { }

			public NewWayFWDataRowForTest(ZString lineData) : base(lineData) { }

			[DecimalField(0, 8, 1, AlignTypes.Right, '0')]
			public ZDecimal Weight { get; set; }

			[IntField(1, 2, AlignTypes.Right, '0')]
			public ZInt Age { get; set; }

			[Field(2, 15, AlignTypes.Left, ' ')]
			public ZString Note { get; set; }

			[DateTimeField(3, "yyyyMMdd")]
			public ZDateTime Created { get; set; }
		}
	}
}
