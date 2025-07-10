using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.KR;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.KR
{
	[TestedType(typeof(FixKRJobDeclarationUCTemplate))]
	class FixKRJobDeclarationUCTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixKRJobDeclarationUCTemplate();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var krGC = testDataCreator.CreateCompany("GC1", "KR");
			var auGC = testDataCreator.CreateCompany("GC2", "AU");

			var filterData = Encoding.Unicode.GetBytes(OriginalXMLData);
			krPK = testDataCreator.CreateModuleFilter(FixKRJobDeclarationUCTemplate.ModuleID, "Test KR UC Template", filterData, krGC, compressFilterData: true);
			auPK = testDataCreator.CreateModuleFilter(FixKRJobDeclarationUCTemplate.ModuleID, "Test AU UC Template", filterData, auGC, compressFilterData: true);
		}

		protected override void AssertPreConditions()
		{
			AssertFilterValue(krPK, OriginalXMLData);
			AssertFilterValue(auPK, OriginalXMLData);
		}

		protected override void AssertTransformationResults()
		{
			AssertFilterValue(krPK, UpdatedXMLData);
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

		Guid krPK;
		Guid auPK;

		const string OriginalXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobDeclaration"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobDeclaration"">
    <P N=""KR_AuditorJobTitle"" Do=""Default"" />
    <P N=""KR_AuthorName"" Do=""Property"">
      <Value xsi:type=""xsd:string"">KR_AuditorName</Value>
    </P>
    <P N=""KR_CarnetUseCode"" Do=""Value"" />
    <P N=""KR_IsBlanketDeclaration"" Do=""Copy"" />
    <P N=""KR_AuditorName"" Do=""Macro"">
      <Value xsi:type=""xsd:string"">&lt;KR_AuditorJobTitle&gt;</Value>
    </P>
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";

		const string UpdatedXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobDeclaration"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobDeclaration"">
    <P N=""JE_AuditorJobTitle"" Do=""Default"" />
    <P N=""JE_AuthorName"" Do=""Property"">
      <Value xsi:type=""xsd:string"">JE_AuditorName</Value>
    </P>
    <P N=""JE_CarnetUseCode"" Do=""Value"" />
    <P N=""JE_IsBlanketDeclaration"" Do=""Copy"" />
    <P N=""JE_AuditorName"" Do=""Macro"">
      <Value xsi:type=""xsd:string"">&lt;JE_AuditorJobTitle&gt;</Value>
    </P>
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";
	}
}
