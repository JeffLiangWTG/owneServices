using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ABCAnalysisCategory))]
	sealed class ABCAnalysisCategoryTest : RegistryBusinessObjectTemplateTestCase<ABCAnalysisCategory>
	{
		#region Related Entities

		#region TestParent

		public void TestParent()
		{
			var collection = new ABCAnalysisCategoryCollection();
			var category = new ABCAnalysisCategory();
			collection.Add(category);

			AssertEquals(collection, category.Parent);
		}

		#endregion

		#endregion

		#region Properties

		#region TestCode

		public void TestCode()
		{
			var abcCategory = new ABCAnalysisCategory();
			abcCategory.CategoryName = "A";

			AssertEquals("A", abcCategory.Code);
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			var abcCategory = new ABCAnalysisCategory();
			abcCategory.Operator = ">";
			abcCategory.PercentageOfTotal = 10;

			AssertEquals("> 10%", abcCategory.Description);

			abcCategory.CategoryName = "ABC";
			var previousValue = Globals.IsWeb;
			Globals.IsWeb = true;

			try
			{
				AssertEquals("ABC - > 10%", abcCategory.Description);
			}
			finally
			{
				Globals.IsWeb = previousValue;
			}
		}

		#endregion

		#region TestCategoryName

		public void TestCategoryName()
		{
			BizObj.CategoryName = "A";
			AssertEquals("A", BizObj.CategoryName);
		}

		#endregion

		#region TestOperator

		public void TestOperator()
		{
			BizObj.Operator = "<";
			AssertEquals("<", BizObj.Operator);
		}

		#endregion

		#region TestPercentageOfTotal

		public void TestPercentageOfTotal()
		{
			BizObj.PercentageOfTotal = 89;
			AssertEquals(89, BizObj.PercentageOfTotal);
		}

		#endregion

		#endregion

		#region Validation

		#region TestValidateCategoryName

		public void TestValidateCategoryName()
		{
			AssertNoErrors("Precondition", BizObj.CategoryNameInfo);

			BizObj.CategoryName = "";
			AssertHasError(BizObj.CategoryNameInfo, "Please enter a value.");

			BizObj.CategoryName = "A";
			AssertNoErrors(BizObj.CategoryNameInfo);

			var collection = new ABCAnalysisCategoryCollection();
			collection.Add(BizObj);

			var abcCategory = collection.AddNew();
			abcCategory.CategoryName = "B";
			AssertNoErrors(BizObj.CategoryNameInfo);
			AssertNoErrors(abcCategory.CategoryNameInfo);

			BizObj.CategoryName = "B";
			AssertHasError(BizObj.CategoryNameInfo, "Category Name must be unique");
		}

		#endregion

		#region TestValidatePercentageOfTotal

		public void TestValidatePercentageOfTotal()
		{
			AssertNoErrors("Precondition", BizObj.PercentageOfTotalInfo);

			BizObj.PercentageOfTotal = -1;
			AssertHasError(BizObj.PercentageOfTotalInfo, "value cannot be negative.");

			BizObj.PercentageOfTotal = 1;
			AssertNoErrors(BizObj.PercentageOfTotalInfo);

			BizObj.PercentageOfTotal = 0;
			AssertHasError(BizObj.PercentageOfTotalInfo, "Please enter a value.");

			BizObj.PercentageOfTotal = 20;
			AssertNoErrors(BizObj.PercentageOfTotalInfo);

			BizObj.PercentageOfTotal = 100;
			AssertHasError(BizObj.PercentageOfTotalInfo, "Percentage of Total can only be up to two digits long.");

			var collection = new ABCAnalysisCategoryCollection();
			collection.Add(BizObj);
			BizObj.PercentageOfTotal = 10;
			AssertNoErrors(BizObj.PercentageOfTotalInfo);

			var category2 = collection.AddNew();
			category2.PercentageOfTotal = 5;
			AssertHasError(category2.PercentageOfTotalInfo, "The lowest category must have its Percentage of Total equal to the value immediately above it.");

			category2.PercentageOfTotal = 10;
			AssertNoErrors(category2.PercentageOfTotalInfo);

			var category3 = collection.AddNew();
			BizObj.PercentageOfTotal = 80;
			category2.PercentageOfTotal = 40;
			category3.PercentageOfTotal = 40;
			AssertNoErrors(BizObj.PercentageOfTotalInfo);
			AssertNoErrors(category2.PercentageOfTotalInfo);
			AssertNoErrors(category3.PercentageOfTotalInfo);

			BizObj.PercentageOfTotal = 20;
			AssertHasError(category2.PercentageOfTotalInfo, "The Percentage Totals must be in descending order.");

			BizObj.PercentageOfTotal = 50;
			AssertNoErrors(category2.PercentageOfTotalInfo);

			BizObj.PercentageOfTotal = 40;
			AssertHasError(category2.PercentageOfTotalInfo, "The Percentage Totals must be in descending order.");

			category2.PercentageOfTotal = 30;
			AssertNoErrors(category2.PercentageOfTotalInfo);
			AssertHasError(category3.PercentageOfTotalInfo, "The lowest category must have its Percentage of Total equal to the value immediately above it.");

			category3.PercentageOfTotal = 30;
			AssertNoErrors(category3.PercentageOfTotalInfo);
		}

		#endregion

		#endregion

		#region Implementation

		protected override ABCAnalysisCategory GetBusinessObjectToClone()
		{
			return new ABCAnalysisCategory();
		}

		protected override ABCAnalysisCategory GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
