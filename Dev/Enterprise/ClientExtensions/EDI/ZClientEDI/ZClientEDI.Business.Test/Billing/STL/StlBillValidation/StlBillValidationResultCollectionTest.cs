using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlBillValidationResultCollection))]
	internal class StlBillValidationResultCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StlBillValidationResultCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		public void TestPopulateResults()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var organisation = licence.Company.Header;
			var bill1 = new StlBill(Factory, GlbBranch.CurrentBranch, organisation, "AUD", new ZDateTime(2022, 2, 1), new ZDateTime(2022, 2, 1));
			var bill2 = new StlBill(Factory, GlbBranch.CurrentBranch, organisation, "AUD", new ZDateTime(2022, 3, 1), new ZDateTime(2022, 3, 1));

			var billCollection = new StlBillCollection(Factory);
			billCollection.Add(bill1);
			billCollection.Add(bill2);

			bill1.AddRowError("some row error here");
			bill2.AddRowWarning("some row warning here");

			var resultCollection = new StlBillValidationResultCollection();
			resultCollection.PopulateResults(billCollection);

			AssertEquals(2, resultCollection.Count);
			AssertEquals(bill1.PK, resultCollection[0].StlBillPK);
			AssertEquals(organisation.OH_Code, resultCollection[0].OrgCode);
			AssertEquals("Error - STL Bill: some row error here", resultCollection[0].ValidationMessage);
			AssertEquals(bill2.PK, resultCollection[1].StlBillPK);
			AssertEquals(organisation.OH_Code, resultCollection[1].OrgCode);
			AssertEquals("Warning - STL Bill: some row warning here", resultCollection[1].ValidationMessage);
		}

		protected override StlBillValidationResultCollection GetCollectionToTest()
		{
			return new StlBillValidationResultCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlBillValidationResult();
		}
	}
}
