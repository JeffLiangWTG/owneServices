using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformations;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Test.HelperClasses
{
	sealed class UCTemplateUpdateHelperTest : TransactionedTestCase
	{
		public void TestUpdateTemplateMappingPrefix()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var gc = testDataCreator.CreateCompany("GC1", "ZZ");
			var otherGC = testDataCreator.CreateCompany("GC2", "AU");

			var moduleIDMaxLength = StmModuleFilterSchema.S9_ModuleID.MaxLength;
			AssertEquals("Currently we use dbo.TVP_varchar_200; please ensure that it match S9_ModuleID length", 200, moduleIDMaxLength);
			var filterData = Encoding.Unicode.GetBytes(OriginalXMLData);
			var uc1PK = testDataCreator.CreateModuleFilter(UCModuleID1, "Test UC 1", filterData, gc);
			var uc2PK = testDataCreator.CreateModuleFilter(UCModuleID2, "Test UC 2", filterData, gc);
			var otherUc1PK = testDataCreator.CreateModuleFilter(UCModuleID1, "Test Other UC 1", filterData, otherGC);
			var otherUc2PK = testDataCreator.CreateModuleFilter(UCModuleID2, "Test Other UC 2", filterData, otherGC);

			UCTemplateUpdateHelper.UpdateTemplateMappingPrefix("Z0", "Z1", "ZZ", UCModuleID1, UCModuleID2);

			AssertStmModuleFilterData(uc1PK, UpdatedXMLData);
			AssertStmModuleFilterData(uc2PK, UpdatedXMLData);
			AssertStmModuleFilterData(otherUc1PK, OriginalXMLData);
			AssertStmModuleFilterData(otherUc2PK, OriginalXMLData);
		}

		public static void AssertStmModuleFilterData(Guid pk, string expectedValue)
		{
			var schemeSql = $@"
SELECT CAST(dbo.CLRUncompressAsBytes(S9_FilterData) AS NVARCHAR(MAX)) AS FilterData
FROM dbo.StmModuleFilter
WHERE S9_PK = @pk";

			using (var cmd = Db.Connection.Command(schemeSql))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				AssertEquals(expectedValue, (string)cmd.ExecuteScalar());
			}
		}

		const string UCModuleID1 = "ABC1234567890_UC";
		const string UCModuleID2 = "DEF1234567890_UC";

		const string OriginalXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""DummyBizo"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""DummyBizo"">
    <P N=""Z0_Bool"" Do=""Default"" />
    <P N=""Z0_AnotherNumber"" Do=""Property"">
      <Value xsi:type=""xsd:string"">Z0_Number</Value>
    </P>
    <P N=""Z0_Guid"" Do=""Value"" />
    <P N=""Z0_Decimal"" Do=""Copy"" />
    <P N=""Z0_NVarCharMax"" Do=""Macro"">
      <Value xsi:type=""xsd:string"">&lt;Z0_NVarChar&gt;</Value>
    </P>
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";

		const string UpdatedXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""DummyBizo"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""DummyBizo"">
    <P N=""Z1_Bool"" Do=""Default"" />
    <P N=""Z1_AnotherNumber"" Do=""Property"">
      <Value xsi:type=""xsd:string"">Z1_Number</Value>
    </P>
    <P N=""Z1_Guid"" Do=""Value"" />
    <P N=""Z1_Decimal"" Do=""Copy"" />
    <P N=""Z1_NVarCharMax"" Do=""Macro"">
      <Value xsi:type=""xsd:string"">&lt;Z1_NVarChar&gt;</Value>
    </P>
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";
	}
}
