using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestedType(typeof(CusStatementEntry))]
	class CusStatementEntryTest : CusStatementLineTest
	{
		public void TestDefaulting()
		{
			var header = Factory.NewWithValidTestData<CusStatementHeader>();
			var entry1 = header.Entries.AddNew();
			AssertEquals(ZString.Empty, entry1.B3_EntryType);

			header.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;
			var entry2 = header.Entries.AddNew();
			AssertEquals(StatementEntryTypeList.Codes.Import, entry2.B3_EntryType);
		}

		public void TestValidation()
		{
			AssertType<CusStatementEntryValidation>(entry.Validation);
		}

		public void TestLookupsType()
		{
			AssertType<CusStatementEntryLookups>(Factory.New<CusStatementEntry>().Lookups);
		}

		public void TestB3_EntryType()
		{
			AssertEquals("Entry Type", DataBoundResourceStrings.GetDataForProperty(entry.B3_EntryTypeInfo).Caption);
		}

		public void TestB3_EntryNum()
		{
			AssertEquals("Entry Number", DataBoundResourceStrings.GetDataForProperty(entry.B3_EntryNumInfo).Caption);
		}

		public void TestB3_BrokerReference()
		{
			AssertEquals("Reference Number", DataBoundResourceStrings.GetDataForProperty(entry.B3_BrokerReferenceInfo).Caption);
		}

		protected override BusinessObject GetNewBusinessObject() => entry;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => entry;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => entry;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = StatementPeriodicityList.Codes.Day;
			entry = header.Entries.AddNew();
		}

		CusStatementEntry entry;
	}
}
