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
	[TestedType(typeof(FixCNJobDeclarationUCTemplate))]
	class FixCNJobDeclarationUCTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixCNJobDeclarationUCTemplate();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var cnGC = testDataCreator.CreateCompany("GC1", "CN");
			var auGC = testDataCreator.CreateCompany("GC2", "AU");

			var filterData = Encoding.Unicode.GetBytes(OriginalXMLData);
			cnPK = testDataCreator.CreateModuleFilter(FixCNJobDeclarationUCTemplate.ModuleID, "Test CN UC Template", filterData, cnGC, compressFilterData: true);
			auPK = testDataCreator.CreateModuleFilter(FixCNJobDeclarationUCTemplate.ModuleID, "Test AU UC Template", filterData, auGC, compressFilterData: true);
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
			var schemeSql = @"
SELECT CAST(dbo.CLRUncompressAsBytes(S9_FilterData) AS NVARCHAR(MAX)) AS FilterData
FROM dbo.StmModuleFilter
WHERE S9_PK = @pk";

			using var cmd = Db.Connection.Command(schemeSql);
			cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			AssertEquals(expectedValue, (string)cmd.ExecuteScalar());
		}

		Guid cnPK;
		Guid auPK;

		const string OriginalXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobDeclaration"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobDeclaration"">
	<P N=""XC_CNTransportMode"" Do=""Default"" />
	<P N=""XC_CNPortOfDestination"" Do=""Property"">
	  <Value xsi:type=""xsd:string"">XC_CNPortOfDestination</Value>
	</P>
	<P N=""XC_CNPortOfOrigin"" Do=""Property"">
	  <Value xsi:type=""xsd:string"">XC_CNPortOfOrigin</Value>
	</P>
	<P N=""XC_DateOfUnloadComplete"" Do=""Default"" />
	<P N=""XC_RN_NKCountryOfTrade"" Do=""Value"" />
	<P N=""XC_ClearanceMode"" Do=""Value"" />
	<P N=""XC_LicenseInvolved"" Do=""Value"" />
	<P N=""XC_InspectionInvolved"" Do=""Value"" />
	<P N=""XC_TaxInvolved"" Do=""Value"" />
	<P N=""XC_TransitMode"" Do=""Default"" />
	<P N=""XC_VesselInland"" Do=""Default"" />
	<P N=""XC_VoyageInland"" Do=""Default"" />
	<P N=""XC_OfficeOfEntryExit"" Do=""Default"" />
	<P N=""XC_CIQOfficeOfEntryExit"" Do=""Default"" />
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";

		const string UpdatedXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobDeclaration"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobDeclaration"">
	<P N=""JE_CNTransportMode"" Do=""Default"" />
	<P N=""JE_CNPortOfDestination"" Do=""Property"">
	  <Value xsi:type=""xsd:string"">JE_CNPortOfDestination</Value>
	</P>
	<P N=""JE_CNPortOfOrigin"" Do=""Property"">
	  <Value xsi:type=""xsd:string"">JE_CNPortOfOrigin</Value>
	</P>
	<P N=""JE_DateOfUnloadComplete"" Do=""Default"" />
	<P N=""JE_RN_NKCountryOfTrade"" Do=""Value"" />
	<P N=""JE_ClearanceMode"" Do=""Value"" />
	<P N=""JE_LicenseInvolved"" Do=""Value"" />
	<P N=""JE_InspectionInvolved"" Do=""Value"" />
	<P N=""JE_TaxInvolved"" Do=""Value"" />
	<P N=""JE_TransitMode"" Do=""Default"" />
	<P N=""JE_VesselInland"" Do=""Default"" />
	<P N=""JE_VoyageInland"" Do=""Default"" />
	<P N=""JE_OfficeOfEntryExit"" Do=""Default"" />
	<P N=""JE_CIQOfficeOfEntryExit"" Do=""Default"" />
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";
	}
}
