using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CN;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.CN
{
	[TestedType(typeof(FixCNJobComInvoiceLineUCTemplate))]
	class FixCNJobComInvoiceLineUCTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixCNJobComInvoiceLineUCTemplate();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var cnGC = testDataCreator.CreateCompany("GC1", "CN");
			var auGC = testDataCreator.CreateCompany("GC2", "AU");

			var filterData = Encoding.Unicode.GetBytes(OriginalXMLData);
			cnPK = testDataCreator.CreateModuleFilter(FixCNJobComInvoiceLineUCTemplate.ModuleID, "Test CN UC Template", filterData, cnGC, compressFilterData: true);
			auPK = testDataCreator.CreateModuleFilter(FixCNJobComInvoiceLineUCTemplate.ModuleID, "Test AU UC Template", filterData, auGC, compressFilterData: true);
		}

		protected override void AssertPreConditions()
		{
			AssertFilterValue(cnPK, OriginalXMLData);
			AssertFilterValue(auPK, OriginalXMLData);
		}

		protected override void AssertTransformationResults()
		{
			AssertFilterValue(cnPK, UpdatedXMLData);
			AssertFilterValue(auPK, OriginalXMLData);
		}

		void AssertFilterValue(Guid pk, string expectedValue)
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

		Guid cnPK;
		Guid auPK;

		const string OriginalXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobComInvoiceLine"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobComInvoiceLine"">
    <P N=""XC_DutyMode"" Do=""Default"" />
    <P N=""XC_NameOfGoods"" Do=""Value"" />
    <P N=""XC_ProductManualNo"" Do=""None"" />
    <P N=""XC_ProductVersion"" Do=""Copy"" />
    <P N=""XC_TradeQuantity"" Do=""Copy"" />
    <P N=""XC_TradeUnitQty"" Do=""Copy"" />
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";

		const string UpdatedXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobComInvoiceLine"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobComInvoiceLine"">
    <P N=""JI_DutyMode"" Do=""Default"" />
    <P N=""JI_NameOfGoods"" Do=""Value"" />
    <P N=""JI_ProductManualNo"" Do=""None"" />
    <P N=""JI_ProductVersion"" Do=""Copy"" />
    <P N=""JI_TradeQuantity"" Do=""Copy"" />
    <P N=""JI_TradeUnitQty"" Do=""Copy"" />
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";
	}
}
