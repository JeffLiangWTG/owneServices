using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MX.Business.Testing
{
	class IdentifierLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			var lookups = identifier.Lookups;
			AssertEquals("CodeList Count", 3, lookups.CodeList.Count);
		}

		public void TestComplement1List()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			var lookups = identifier.Lookups;
			AssertEquals("Should be", 0, lookups.Complement1List.Count);

			identifier.CSI_Code = "AI";
			AssertEquals("Should be", 5, lookups.Complement1List.Count);
			AssertContainsExactElementsInAnyOrder("1, 2, 3, 4, 5", lookups.Complement1List.CodesAsString);

			identifier.CSI_Code = "AC";
			AssertEquals("Should be", 0, lookups.Complement1List.Count);
		}

		public void TestComplement2List()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			var lookups = identifier.Lookups;
			AssertEquals("Should be", 0, lookups.Complement2List.Count);

			identifier.CSI_Code = "AI";
			AssertEquals("Should be", 5, lookups.Complement2List.Count);
			AssertContainsExactElementsInAnyOrder("1, 2, 3, 4, 5", lookups.Complement2List.CodesAsString);
		}

		public void TestComplement3List()
		{
			ReferenceTestDataHelper.CreateIdentifiersRefCusProfileQuestions(Factory);
			var invLine = Factory.New<JobComInvoiceLine>();
			var identifier = invLine.Identifiers.AddNew();
			var lookups = identifier.Lookups;
			AssertEquals("Should be", 0, lookups.Complement3List.Count);

			identifier.CSI_Code = "AI";
			AssertEquals("Should be", 5, lookups.Complement3List.Count);
			AssertContainsExactElementsInAnyOrder("1, 2, 3, 4, 5", lookups.Complement3List.CodesAsString);
		}
	}
}
