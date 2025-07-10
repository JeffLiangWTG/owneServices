using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

public class ManifestLineTest : FlatFileDataRowTest
{
	public void TestSetFields()
	{
		MockManifestLine row = new MockManifestLine(5);
		AssertEquals("Row 0 should be 'MOCK'", "MOCK", row[0]);
		FlatFileFieldProperty one = new FlatFileFieldProperty(1, 5);
		FlatFileFieldProperty two = new FlatFileFieldProperty(2, 8);
		FlatFileFieldProperty three = new FlatFileFieldProperty(3, 10);
		FlatFileFieldProperty four = new FlatFileFieldProperty(4, 1);
		row.SetField(one, new ZInt(98765412));
		row.SetField(two, new ZDateTime(2008, 6, 25, 14, 53, 32));
		row.SetField(three, "this is a string");
		row.SetField(four, 98454.848m);
		AssertEquals("Field one should be 98765", "98765", row[1]);
		AssertEquals("Field two should be 25-Jun-2008", "25-Jun-2008", row[2]);
		AssertEquals("Field three should be 'this is a '", "this is a ", row[3]);
		AssertEquals("Field four should be '98454.8'", "98454.8", row[4]);
	}

	public void TestToString()
	{
		MockManifestLine line = new MockManifestLine(1);
		line.Children.Add(new MockManifestLine(2));
		line.Children[0].Children.Add(new MockManifestLine(3));
		AssertEquals("ToString", "\"MOCK\"\r\n\"MOCK\",\"\"\r\n\"MOCK\",\"\",\"\"\r\n", line.ToString());
	}

	#region Implementation
	public abstract class ManifestLineTesting : FlatFileDataRowTest
	{
		public void TestManifestLineType()
		{
			AssertEquals("MnifestLine Type", ExpectedType, GetNewManifestLine().GetType());
		}

		public void TestFieldCount()
		{
			AssertEquals("FieldCount", FieldCount, GetNewManifestLine().FieldCount);
		}

		protected abstract int FieldCount
		{
			get;
		}

		protected abstract Type ExpectedType
		{
			get;
		}

		protected virtual ManifestLine GetNewManifestLine()
		{
			return new MockManifestLine(1);
		}

		protected ZString Pad(ZString value)
		{
			return value.PadRight(40, ' ');
		}
	}

	public class MockManifestLine : ManifestLine
	{
		public MockManifestLine(int fieldCount) : base(fieldCount)
		{
		}

		public MockManifestLine(FlatFileDataRow row) : base(row)
		{
		}

		protected override void SetPreambleData()
		{
			if (FieldCount > 0)
			{
				this[0] = RecordIdentifier;
			}
		}

		public override string RecordIdentifier
		{
			get
			{
				return "MOCK";
			}
		}

		public new void SetField(FlatFileFieldProperty fieldProperty, ZInt value)
		{
			base.SetField(fieldProperty, value);
		}

		public new void SetField(FlatFileFieldProperty fieldProperty, ZString value)
		{
			base.SetField(fieldProperty, value);
		}

		public new void SetField(FlatFileFieldProperty fieldProperty, ZDecimal value)
		{
			base.SetField(fieldProperty, value);
		}

		public new void SetField(FlatFileFieldProperty fieldProperty, ZDateTime value)
		{
			base.SetField(fieldProperty, value);
		}

		public new ZDateTime GetFieldAsZDateTime(FlatFileFieldProperty fieldProperty)
		{
			return base.GetFieldAsZDateTime(fieldProperty);
		}

		public bool SetFixedFieldWasCalledInTheConstructor;
	}

	protected override ZString TestingCountry
	{
		get
		{
			return Core.Constants.CountryCodes.UnitedArabEmirates;
		}
	}
	#endregion
}
