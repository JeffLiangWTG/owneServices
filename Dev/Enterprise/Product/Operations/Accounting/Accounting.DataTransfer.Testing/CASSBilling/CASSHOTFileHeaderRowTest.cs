using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSHOTFileHeaderRowTest : CASSHOTFileDataRowTest
	{
		[TestDate(2011, 07, 13)]
		public override void TestPublicFields()
		{
			base.TestPublicFields();

			HeaderRowForTest.SetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodStart, "081023");
			HeaderRowForTest.SetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodEnd, "080922");
			HeaderRowForTest.SetField(CASSHOTFileHeaderRow.Schema.HeaderDateOfBilling, "080821");
			HeaderRowForTest.SetField(CASSHOTFileHeaderRow.Schema.BillingCurrency, "JPY");

			AssertEquals(new ZDateTime(2008, 10, 23), HeaderRowForTest.HeaderDatePeriodStart);
			AssertEquals(new ZDateTime(2008, 09, 22), HeaderRowForTest.HeaderDatePeriodEnd);
			AssertEquals(new ZDateTime(2008, 08, 21), HeaderRowForTest.HeaderDateOfBilling);
			AssertEquals("JPY", HeaderRowForTest.BillingCurrency);
		}

		protected CASSHOTFileHeaderRow HeaderRowForTest
		{
			get { return (CASSHOTFileHeaderRow)DataRowForTest; }
		}

		protected override CASSHOTFileDataRow GetCASSHOTFileDataRowForTest()
		{
			return new CASSHOTFileHeaderRow();
		}
	}
}
