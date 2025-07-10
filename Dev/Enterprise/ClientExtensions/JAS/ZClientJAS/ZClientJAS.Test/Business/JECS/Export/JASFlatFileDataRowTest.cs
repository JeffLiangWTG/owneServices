using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	public class JASFlatFileDataRowTest : TestCase
	{
		public void TestFieldCountAssignedInConstructor()
		{
			JXCFlatFileDataRow row = new JXCFlatFileDataRow(3);
			AssertEquals(3, row.FieldCount);
			row = new JXCFlatFileDataRow(17);
			AssertEquals(17, row.FieldCount);
		}

		public void TestConversionFromZDecimal()
		{
			JXCFlatFileDataRow row = new JXCFlatFileDataRow(1);
			row.SetField(0, (ZDecimal)0.05M);
			AssertEquals("0.05", row[0]);
			row.SetField(0, (ZDecimal)0M);
			AssertEquals("0", row[0]);
			row.SetField(0, (ZDecimal)1.2837M);
			AssertEquals("1.284", row[0]);
			row.SetField(0, (ZDecimal)0.2332M);
			AssertEquals("0.233", row[0]);
		}

		public void TestConversionFromZDateTime()
		{
			JXCFlatFileDataRow row = new JXCFlatFileDataRow(1);
			row.SetField(0, new ZDateTime(2005, 4, 23));
			AssertEquals("23/04/2005", row[0]);
			row.SetField(0, new ZDateTime(2002, 1, 1));
			AssertEquals("01/01/2002", row[0]);
		}

		public void TestConversionFromOtherIZTypes()
		{
			JXCFlatFileDataRow row = new JXCFlatFileDataRow(1);
			row.SetField(0, (ZInt)88);
			AssertEquals("88", row[0]);
			row.SetField(0, ZBool.True);
			AssertEquals(ZBool.True.ToString(), row[0]);
			row.SetField(0, "blah");
			AssertEquals("blah", row[0]);
		}

		public void TestSetFreeTextField()
		{
			JXCFlatFileDataRow row = new JXCFlatFileDataRow(1);
			row.SetField(0, "123456789012345678901234567890", 20);
			AssertEquals("12345678901234567890", row[0]);
			row.SetField(0, "                 123456789012345678901234567890                      ", 20);
			AssertEquals("12345678901234567890", row[0]);
		}

		public void TestSetOrganisationNameField()
		{
			JXCFlatFileDataRow row = new JXCFlatFileDataRow(1);
			row.SetOrganisationNameField(0, "	Eagle Datamation   International Pty Ltd.", 50);
			AssertEquals("Eagle Datamation   International Pty Ltd.", row[0]);
			row.SetOrganisationNameField(0, "	Eagle Datamation   International Pty Ltd.", 30);
			AssertEquals("EAGLE DATAMATION INTERNATIONAL", row[0]);
			row.SetOrganisationNameField(0, "	Eagle Datamation   International Pty Ltd. Test", 35);
			AssertEquals("EAGLE DATAMATION INTERNATIONAL TEST", row[0]);
			row.SetOrganisationNameField(0, "	Eagle Datamation   International Pty Ltd. Test", 29);
			AssertEquals("EAGLE DATAMATION INTERNATIONA", row[0]);
		}

		public void TestSetCharField()
		{
			JXCFlatFileDataRow row = new JXCFlatFileDataRow(1);
			row.SetField(0, 'C');
			AssertEquals("C", row[0]);
		}
	}
}
