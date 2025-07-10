using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.BR
{
	[TestedType(typeof(FixBRJobDeclarationUCTemplate))]
	class FixBRJobDeclarationUCTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixBRJobDeclarationUCTemplate();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var brGC = testDataCreator.CreateCompany("GC1", "BR");
			var auGC = testDataCreator.CreateCompany("GC2", "AU");

			var filterData = Encoding.Unicode.GetBytes(OriginalXMLData);
			brPK = testDataCreator.CreateModuleFilter(FixBRJobDeclarationUCTemplate.ModuleID, "Test BR UC Template", filterData, brGC, compressFilterData: true);
			auPK = testDataCreator.CreateModuleFilter(FixBRJobDeclarationUCTemplate.ModuleID, "Test AU UC Template", filterData, auGC, compressFilterData: true);
		}

		protected override void AssertPreConditions()
		{
			AssertFilterValue(brPK, OriginalXMLData);
			AssertFilterValue(auPK, OriginalXMLData);
		}

		protected override void AssertTransformationResults()
		{
			AssertFilterValue(brPK, UpdatedXMLData);
			AssertFilterValue(auPK, OriginalXMLData);
		}

		void AssertFilterValue(Guid pk, string expectedValue)
		{
			var schemeSql = @"
SELECT CAST(dbo.CLRUncompressAsBytes(S9_FilterData) AS NVARCHAR(MAX)) AS FilterData
FROM dbo.StmModuleFilter
WHERE S9_PK = @pk";

			using var cmd = Db.Connection.Command(schemeSql);
			cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			AssertEquals(expectedValue, (string)cmd.ExecuteScalar());
		}

		Guid brPK;
		Guid auPK;

		const string OriginalXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobDeclaration"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobDeclaration"">
	<P N=""BR_SpecialTransport"" Do=""6"" />
	<P N=""BR_DispatchModality"" Do=""5"" />
	<P N=""BR_CargoArrivalDocumentNumber"" Do=""4"" />
	<P N=""BR_CargoArrivalDocumentType"" Do=""3"" />
	<P N=""BR_CargoArrivalDocumentUtilization"" Do=""2"" />
	<P N=""BR_IsMultimodal"" Do=""1"" />
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";

		const string UpdatedXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobDeclaration"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobDeclaration"">
	<P N=""JE_SpecialTransport"" Do=""6"" />
	<P N=""JE_DispatchModality"" Do=""5"" />
	<P N=""JE_CargoArrivalDocumentNumber"" Do=""4"" />
	<P N=""JE_CargoArrivalDocumentType"" Do=""3"" />
	<P N=""JE_CargoArrivalDocumentUtilization"" Do=""2"" />
	<P N=""JE_IsMultimodal"" Do=""1"" />
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";
	}
}
