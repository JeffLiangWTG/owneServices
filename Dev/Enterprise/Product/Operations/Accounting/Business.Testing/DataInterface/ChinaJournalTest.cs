using CargoWise.EntityFramework;
using Enterprise.Accounting.Utility.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	[TestedType(typeof(ChinaJournal))]
	public class ChinaJournalTest : VoucherTest
	{
		public void TestProperties()
		{
			var chinaJournal = new ChinaJournal();
			chinaJournal.Currency = "CNY";

			AssertEquals(0m, chinaJournal.OriginalAmount);
			AssertEquals("CNY", chinaJournal.Currency);
			AssertEquals("", chinaJournal.ApprovedBy);
			AssertEquals("", chinaJournal.HandlingStaff);
			AssertEquals("", chinaJournal.Annotation);
			AssertEquals("", chinaJournal.PaymentNumber);
			AssertEquals("", chinaJournal.BusinessContacts);
			AssertEquals(0, chinaJournal.SerialNumber);
			AssertEquals("", chinaJournal.SystemModule);
			AssertEquals("", chinaJournal.BusinessDesc);
			AssertEquals("", chinaJournal.ReferenceInformation);
			AssertEquals("", chinaJournal.GLAccountNumAndDescription);
			AssertEquals("", chinaJournal.VoucherNumber);
			AssertEquals("", chinaJournal.VoucherTypeNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChinaJournal();
		}
	}
}
