using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(PermitTypeChecklistField))]
	sealed class PermitTypeChecklistFieldTest : FilterFieldTest
	{
		#region Properties

		public void TestIsEmpty()
		{
			GlbCompany.CurrentCompany.SetCountry("ZA");
			var field = GetNewField();
			AssertEquals(true, field.IsEmpty);

			var rootItem = field.RootItemsCollection.Cast<PermitTypePartItem>().First();
			rootItem.Include = true;
			AssertEquals(false, field.IsEmpty);

			rootItem.Include = false;
			AssertEquals(true, field.IsEmpty);
		}

		public void TestSuggestedUserControlType()
		{
			var field = GetNewField();
			AssertEquals(FilterFieldSuggestedUserControlType.PermitTypeChecklistUserControl, field.SuggestedUserControlType);
		}

		#endregion

		#region Items

		public void TestRootItems()
		{
			GlbCompany.CurrentCompany.SetCountry("ZA");
			var field = GetNewField();
			var allTypes = PermitTypeChecklistField.GetByCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica).GetTypeList();

			foreach (ICodeDescription type in allTypes)
			{
				var rootItem = field.RootItemsCollection.Cast<PermitTypePartItem>().FirstOrDefault(x => x.Code == type.Code);
				AssertNotNull("Should have root item " + type.Code, rootItem);
			}
		}

		#endregion

		#region Where Clause

		public void TestNonEmptyWhereClause()
		{
			GlbCompany.CurrentCompany.SetCountry("ZA");
			var field = GetNewField();
			field.FieldName = "Type";
			field.SubTypeField = "SubType";
			AssertEquals("", field.WhereClause());

			var rccItem = field.RootItemsCollection.Cast<PermitTypePartItem>().Single(x => x.Code == "RCC");
			rccItem.Include = true;
			var rccACOItem = rccItem.SubItemsCollection.Cast<PermitTypePartItem>().Single(x => x.Code == "ACO");
			rccACOItem.Include = true;
			var rccLVEItem = rccItem.SubItemsCollection.Cast<PermitTypePartItem>().Single(x => x.Code == "LVE");
			rccLVEItem.Include = true;

			var impItem = field.RootItemsCollection.Cast<PermitTypePartItem>().Single(x => x.Code == "IMP");
			impItem.Include = true;

			var whereClause = field.WhereClause();
			var rccSqlParameter = field.SqlParameters().Single(x => (string)x.Value == "RCC");
			var rccACOParameter = field.SqlParameters().Single(x => (string)x.Value == "ACO");
			var rccLVEParameter = field.SqlParameters().Single(x => (string)x.Value == "LVE");
			var impSqlParameter = field.SqlParameters().Single(x => (string)x.Value == "IMP");

			AssertEquals(4, field.SqlParameters().Count);

			AssertEquals(
				string.Format("(Type = {3}) OR (Type = {0} AND ((SubType = {1}) OR (SubType = {2})))",
					rccSqlParameter.ParameterName,
					rccACOParameter.ParameterName,
					rccLVEParameter.ParameterName,
					impSqlParameter.ParameterName),
				field.WhereClause());
		}

		#endregion

		#region JsonConverter

		public void TestJsonConverter()
		{
			GlbCompany.CurrentCompany.SetCountry("ZA");
			var field = new PermitTypeChecklistField(Factory);
			field.DisplayName = "DisplayName";
			field.FieldName = "MyTypeField";
			field.SubTypeField = "MySubTypeField";
			var rccItem = field.RootItemsCollection.Cast<PermitTypePartItem>().First(x => x.Code == "RCC");
			rccItem.Include = true;
			var rccACOItem = rccItem.SubItemsCollection.Cast<PermitTypePartItem>().First(x => x.Code == "ACO");
			rccACOItem.Include = true;

			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<PermitTypeChecklistField>(result);

			AssertEquals("DisplayName", deserialisedField.DisplayName);
			AssertEquals("MyTypeField", deserialisedField.FieldName);
			AssertEquals("MySubTypeField", deserialisedField.SubTypeField);

			AssertContainsExactElementsInAnyOrder(new ZString[] { "RCC" }, deserialisedField.RootItemsCollection.Cast<PermitTypePartItem>().Where(x => x.Include).Select(x => x.Code));
			var actualRccItem = deserialisedField.RootItemsCollection.Cast<PermitTypePartItem>().First(x => x.Code == "RCC");

			AssertContainsExactElementsInAnyOrder(new ZString[] { "ACO" }, actualRccItem.SubItemsCollection.Cast<PermitTypePartItem>().Where(x => x.Include).Select(x => x.Code));
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var field = (PermitTypeChecklistField)base.GetNewBusinessObject();
			field.FieldName = "Type";
			field.SubTypeField = "SubType";
			return field;
		}

		#endregion

		#region Implementation

		PermitTypeChecklistField GetNewField()
		{
			var field = new PermitTypeChecklistField(Factory);
			field.FieldName = "Type";
			field.SubTypeField = "SubType";

			return field;
		}

		#endregion
	}
}
