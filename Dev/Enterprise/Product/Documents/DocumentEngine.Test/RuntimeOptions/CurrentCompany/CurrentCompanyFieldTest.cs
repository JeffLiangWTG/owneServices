using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(CurrentCompanyField))]
	sealed class CurrentCompanyFieldTest : FilterFieldTest
	{
		public void TestJsonConverter()
		{
			var ccf = new CurrentCompanyField(Factory);
			ccf.FieldName = "MyCompanyField";
			ccf.DisplayName = "Json Test";

			var result = JsonConverterHelper.Serialize(ccf);
			var newccf = JsonConverterHelper.Deserialize<CurrentCompanyField>(result);

			var parameterList = newccf.SqlParameters();
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid().ToString(), parameterList[0].Value.ToString());
			AssertEquals("MyCompanyField", newccf.FieldName);
			AssertEquals("Json Test", newccf.DisplayName);
		}

		public void TestWhereClause()
		{
			var testField = new CurrentCompanyField(new BusinessObjectFactory());
			testField.CreateParameters();
			testField.FieldName = "XX_GC";
			Assert("Where clause: got " + testField.WhereClause(), Regex.IsMatch(testField.WhereClause(), @"^XX_GC = @p([0-9]+)$"));
			AssertEquals("Param value", GlbCompany.CurrentCompany.PK.ToGuid(), (Guid)testField.SqlParameters()[0].Value);
		}

		public void TestSingleValue()
		{
			var testField = new CurrentCompanyField(new BusinessObjectFactory());
			testField.CreateParameters();
			AssertEquals("Value as object", GlbCompany.CurrentCompany.PK.ToGuid(), testField.ValueAsObject);
		}
	}
}
