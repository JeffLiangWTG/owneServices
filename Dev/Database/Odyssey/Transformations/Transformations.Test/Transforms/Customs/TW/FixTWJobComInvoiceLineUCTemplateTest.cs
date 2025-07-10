using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.TW
{
	[TestedType(typeof(FixTWJobComInvoiceLineUCTemplate))]
	class FixTWJobComInvoiceLineUCTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixTWJobComInvoiceLineUCTemplate();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var twGC = testDataCreator.CreateCompany("GC1", "TW");
			var auGC = testDataCreator.CreateCompany("GC2", "AU");

			var filterData = Encoding.Unicode.GetBytes(OriginalXMLData);
			twPK = testDataCreator.CreateModuleFilter(FixTWJobComInvoiceLineUCTemplate.ModuleID, "Test TW UC Template", filterData, twGC, compressFilterData: true);
			auPK = testDataCreator.CreateModuleFilter(FixTWJobComInvoiceLineUCTemplate.ModuleID, "Test AU UC Template", filterData, auGC, compressFilterData: true);
		}

		protected override void AssertPreConditions()
		{
			AssertFilterValue(twPK, OriginalXMLData);
			AssertFilterValue(auPK, OriginalXMLData);
		}

		protected override void AssertTransformationResults()
		{
			AssertFilterValue(twPK, UpdatedXMLData);
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

		Guid twPK;
		Guid auPK;

		const string OriginalXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobComInvoiceLine"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobComInvoiceLine"">
    <P N=""TW_AlcoholAge"" Do=""Default"" />
    <P N=""TW_AlcoholYear"" Do=""Property"">
      <Value xsi:type=""xsd:string"">TW_AnimalAgeYear</Value>
    </P>
    <P N=""TW_AlcoholCountryRegion"" Do=""Value"" />
    <P N=""TW_AdditionalDutyRate"" Do=""Copy"" />
    <P N=""TW_AlcoholPercentage"" Do=""Macro"">
      <Value xsi:type=""xsd:string"">&lt;TW_AlcoholAge&gt;</Value>
    </P>
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";

		const string UpdatedXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobComInvoiceLine"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobComInvoiceLine"">
    <P N=""JI_AlcoholAge"" Do=""Default"" />
    <P N=""JI_AlcoholYear"" Do=""Property"">
      <Value xsi:type=""xsd:string"">JI_AnimalAgeYear</Value>
    </P>
    <P N=""JI_AlcoholCountryRegion"" Do=""Value"" />
    <P N=""JI_AdditionalDutyRate"" Do=""Copy"" />
    <P N=""JI_AlcoholPercentage"" Do=""Macro"">
      <Value xsi:type=""xsd:string"">&lt;JI_AlcoholAge&gt;</Value>
    </P>
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";
	}
}
