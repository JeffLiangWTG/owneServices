using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SalesTradeLaneChecklistField))]
	sealed class SalesTradeLaneChecklistFieldTest : FilterFieldTest
	{
		#region Properties

		public void TestIsEmpty()
		{
			var field = GetNewField();
			AssertEquals(true, field.IsEmpty);

			var rootItem = field.RootItemsCollection.Cast<SalesTradeLanePartItem>().First();
			rootItem.Include = true;
			AssertEquals(false, field.IsEmpty);

			rootItem.Include = false;
			AssertEquals(true, field.IsEmpty);
		}

		public void TestSuggestedUserControlType()
		{
			var field = GetNewField();
			AssertEquals(FilterFieldSuggestedUserControlType.SalesTradeLaneChecklistUserControl, field.SuggestedUserControlType);
		}

		#endregion

		#region Items

		public void TestRootItems()
		{
			var field = GetNewField();
			var allProducts = Factory.Load<IOrgSalesProduct>(new ZQuery());

			foreach (var product in allProducts)
			{
				var rootItem = field.RootItemsCollection.Cast<SalesTradeLanePartItem>().FirstOrDefault(x => x.Code == product.MP_Code);
				AssertNotNull("Should have root item " + product.MP_Code, rootItem);

				var expectedModeItems =
					(product.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse) ?
						OrgSalesLookups.GetServiceTypes(product.MP_Code).Cast<ICodeDescription>().Select(x => x.Code) :
						OrgTradeDetailLookups.GetTradeModes(product.MP_Code).Cast<ICodeDescription>().Select(x => x.Code);

				AssertContainsExactElementsInAnyOrder(product.MP_Code + " should have the following mode items",
					expectedModeItems,
					rootItem.SubItemsCollection.Cast<SalesTradeLanePartItem>().Select(x => x.Code.ToString()));
			}

			var opportunityValueLookup = new OrgOpportunityValueLookups(null);
			foreach (ICodeDescription type in opportunityValueLookup.ActiveValueTypes)
			{
				var valueItem = field.RootItemsCollection.Cast<SalesTradeLanePartItem>().FirstOrDefault(x => x.Code == type.Code);
				AssertNotNull("Should have root item " + type.Code, valueItem);
			}
		}

		#endregion

		#region Where Clause

		public void TestNonEmptyWhereClause()
		{
			var field = GetNewField();
			field.FieldName = "Product";
			field.ModeField = "Mode";
			field.TypeField = "Type";
			AssertEquals("", field.WhereClause());

			var shpItem = field.RootItemsCollection.Cast<SalesTradeLanePartItem>().Single(x => x.Code == "SHP");
			shpItem.Include = true;
			var shpAirItem = shpItem.SubItemsCollection.Cast<SalesTradeLanePartItem>().Single(x => x.Code == "AIR");
			shpAirItem.Include = true;
			var shpAirLseItem = shpAirItem.SubItemsCollection.Cast<SalesTradeLanePartItem>().Single(x => x.Code == "LSE");
			shpAirLseItem.Include = true;
			var shpAirUldItem = shpAirItem.SubItemsCollection.Cast<SalesTradeLanePartItem>().Single(x => x.Code == "ULD");
			shpAirUldItem.Include = true;

			var brkItem = field.RootItemsCollection.Cast<SalesTradeLanePartItem>().Single(x => x.Code == "BRK");
			brkItem.Include = true;

			var whereClause = field.WhereClause();
			var shpSqlParameter = field.SqlParameters().Single(x => (string)x.Value == "SHP");
			var shpAirSqlParameter = field.SqlParameters().Single(x => (string)x.Value == "AIR");
			var shpAirLseSqlParameter = field.SqlParameters().Single(x => (string)x.Value == "LSE");
			var shpAirUldSqlParameter = field.SqlParameters().Single(x => (string)x.Value == "ULD");
			var brkSqlParameter = field.SqlParameters().Single(x => (string)x.Value == "BRK");

			AssertEquals(5, field.SqlParameters().Count);

			AssertEquals(
				string.Format("(Product = {0} AND ((Mode = {1} AND ((Type = {2}) OR (Type = {3}))))) OR (Product = {4})",
					shpSqlParameter.ParameterName,
					shpAirSqlParameter.ParameterName,
					shpAirLseSqlParameter.ParameterName,
					shpAirUldSqlParameter.ParameterName,
					brkSqlParameter.ParameterName),
				field.WhereClause());
		}

		#endregion

		#region JsonConverter

		public void TestJsonConverter()
		{
			var field = new SalesTradeLaneChecklistField(Factory);
			field.FieldName = "MyField";
			field.ModeField = "MyModeField";
			field.TypeField = "MyTypeField";
			var shpItem = field.RootItemsCollection.Cast<SalesTradeLanePartItem>().First(x => x.Code == "SHP");
			shpItem.Include = true;
			var shpAirItem = shpItem.SubItemsCollection.Cast<SalesTradeLanePartItem>().First(x => x.Code == "AIR");
			shpAirItem.Include = true;

			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<SalesTradeLaneChecklistField>(result);

			AssertEquals("MyField", deserialisedField.FieldName);
			AssertEquals("MyModeField", deserialisedField.ModeField);
			AssertEquals("MyTypeField", deserialisedField.TypeField);

			AssertContainsExactElementsInAnyOrder(new ZString[] { "SHP" }, deserialisedField.RootItemsCollection.Cast<SalesTradeLanePartItem>().Where(x => x.Include).Select(x => x.Code));
			var actualShpItem = deserialisedField.RootItemsCollection.Cast<SalesTradeLanePartItem>().First(x => x.Code == "SHP");

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AIR" }, actualShpItem.SubItemsCollection.Cast<SalesTradeLanePartItem>().Where(x => x.Include).Select(x => x.Code));
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var field = (SalesTradeLaneChecklistField)base.GetNewBusinessObject();
			field.FieldName = "Product";
			field.ModeField = "Mode";
			field.TypeField = "Type";
			return field;
		}

		#endregion

		#region Implementation

		SalesTradeLaneChecklistField GetNewField()
		{
			var field = new SalesTradeLaneChecklistField(Factory);
			field.FieldName = "Product";
			field.ModeField = "Mode";
			field.TypeField = "Type";

			return field;
		}

		#endregion
	}
}
