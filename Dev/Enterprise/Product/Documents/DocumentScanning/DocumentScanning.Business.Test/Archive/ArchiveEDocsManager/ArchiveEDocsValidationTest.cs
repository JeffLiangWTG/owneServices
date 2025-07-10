using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business.Test
{
	internal class ArchiveEDocsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSA_IncludeConsignee()
		{
			Parent.SA_IncludeConsignee = false;
			Parent.SA_IncludeConsignor = false;

			AssertEquals("Should have errors if both are false", true, Parent.SA_IncludeConsigneeInfo.HasErrors());

			Parent.SA_IncludeConsignee = true;
			AssertEquals("Should have no errors if either one is true", false, Parent.SA_IncludeConsigneeInfo.HasErrors());

			Parent.SA_IncludeConsignee = false;
			Parent.SA_IncludeConsignor = true;
			AssertEquals("Should have no errors if either one is true", false, Parent.SA_IncludeConsigneeInfo.HasErrors());
		}

		public void TestCheckSA_IncludeConsignor()
		{
			Parent.SA_IncludeConsignee = false;
			Parent.SA_IncludeConsignor = false;

			AssertEquals("Should have errors if both are false", true, Parent.SA_IncludeConsignorInfo.HasErrors());

			Parent.SA_IncludeConsignor = true;
			AssertEquals("Should have no errors if either one is true", false, Parent.SA_IncludeConsignorInfo.HasErrors());

			Parent.SA_IncludeConsignor = false;
			Parent.SA_IncludeConsignee = true;
			AssertEquals("Should have no errors if either one is true", false, Parent.SA_IncludeConsignorInfo.HasErrors());
		}

		public void TestCheckSA_ETATo()
		{
			Parent.SA_ETATo = ZDateTime.Empty;
			AssertEquals("Date can be empty without errors", false, Parent.SA_ETAToInfo.HasErrors());
		}

		public void TestCheckSA_ETAFrom()
		{
			Parent.SA_ETAFrom = ZDateTime.Empty;
			AssertEquals("Date can be empty without errors", false, Parent.SA_ETAFromInfo.HasErrors());
		}

		public void TestCheckSA_ETDTo()
		{
			Parent.SA_ETDTo = ZDateTime.Empty;
			AssertEquals("Date can be empty without errors", false, Parent.SA_ETDToInfo.HasErrors());
		}

		public void TestCheckSA_ETDFrom()
		{
			Parent.SA_ETDFrom = ZDateTime.Empty;
			AssertEquals("Date can be empty without errors", false, Parent.SA_ETDFromInfo.HasErrors());
		}

		#region TestCheckIsSearchTypeSelected

		public void TestCheckIsSearchTypeSelected()
		{
			string[] allAvailableSearchTypes = new string[] {
				"Consol",
				"Shipment",
				"Shipping Manager Shipment",
				"Declaration",
				"Transport Job",
				"Warehouse Receive Job",
				"Warehouse Order Job",
				"Warehouse Adjustment Job",
				"Warehouse Periodic Invoicing Job",
				"Warehouse Work Order Job"
			};

			// Check Off from filter all Search Types
			foreach (string searchType in allAvailableSearchTypes)
			{
				Parent.SearchTypes[searchType].IsFilterOn = false;
			}

			AssertEquals("IsSearchTypeSelected should have error since none of search types are selected", true, Parent.IsSearchTypeSelectedInfo.HasErrors());

			foreach (string searchType in allAvailableSearchTypes)
			{
				string someOtherSearchType = GetSomeOtherSearchType(searchType, allAvailableSearchTypes);

				AssertEquals(searchType + " should have error if none of the types are selected", true, Parent.SearchTypes[searchType].IsFilterOnInfo.HasErrors());

				Parent.SearchTypes[searchType].IsFilterOn = true;
				AssertEquals(searchType + " should no longer have error if one of the types is selected", false, Parent.SearchTypes[searchType].IsFilterOnInfo.HasErrors());

				Parent.SearchTypes[searchType].IsFilterOn = false;
				Parent.SearchTypes[someOtherSearchType].IsFilterOn = true;
				AssertEquals(searchType + " should no longer have error if one of the types is selected", false, Parent.SearchTypes[searchType].IsFilterOnInfo.HasErrors());

				Parent.SearchTypes[someOtherSearchType].IsFilterOn = false; // restore data for next loop.
			}
		}

		string GetSomeOtherSearchType(string currentSearchType, string[] allAvailableSearchTypes)
		{
			foreach (string searchType in allAvailableSearchTypes)
			{
				if (searchType != currentSearchType)
				{
					return searchType;
				}
			}
			return null;
		}
		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			Parent = new ArchiveEDocsManager(MasterFactory);
		}

		DocumentFactory MasterFactory;
		ArchiveEDocsManager Parent;
	}
}
