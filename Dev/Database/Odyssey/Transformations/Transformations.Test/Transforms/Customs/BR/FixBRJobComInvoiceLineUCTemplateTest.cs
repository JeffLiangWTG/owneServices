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
	[TestedType(typeof(FixBRJobComInvoiceLineUCTemplate))]
	class FixBRJobComInvoiceLineUCTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixBRJobComInvoiceLineUCTemplate();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var brGC = testDataCreator.CreateCompany("GC1", "BR");
			var auGC = testDataCreator.CreateCompany("GC2", "AU");

			var filterData = Encoding.Unicode.GetBytes(OriginalXMLData);
			brPK = testDataCreator.CreateModuleFilter(FixBRJobComInvoiceLineUCTemplate.ModuleID, "Test BR UC Template", filterData, brGC, compressFilterData: true);
			auPK = testDataCreator.CreateModuleFilter(FixBRJobComInvoiceLineUCTemplate.ModuleID, "Test AU UC Template", filterData, auGC, compressFilterData: true);
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

		Guid brPK;
		Guid auPK;

		const string OriginalXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobComInvoiceLine"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobComInvoiceLine"">
	<P N=""BR_UsedMaterialSerialNumber"" Do=""30"" />
	<P N=""BR_UsedMaterialRegime"" Do=""29"" />
	<P N=""BR_UsedMaterialOperationType"" Do=""28"" />
	<P N=""BR_UsedMaterialManufactureYear"" Do=""27"" />
	<P N=""BR_ThirdCPC"" Do=""26"" />
	<P N=""BR_TemporaryAdmissionReason"" Do=""25"" />
	<P N=""BR_SecondCPC"" Do=""24"" />
	<P N=""BR_RequiresImportLicense"" Do=""23"" />
	<P N=""BR_NFeNumber"" Do=""22"" />
	<P N=""BR_NFeLinePrice"" Do=""21"" />
	<P N=""BR_NFeItemNumber"" Do=""20"" />
	<P N=""BR_ManufacturerIndicator"" Do=""19"" />
	<P N=""BR_ManufacturerAuthorityVersion"" Do=""18"" />
	<P N=""BR_ManufacturerAuthorityIdentifier"" Do=""17"" />
	<P N=""BR_IntendedTermDays"" Do=""16"" />
	<P N=""BR_ICMSTotalAmountReductionPercentage"" Do=""15"" />
	<P N=""BR_ICMSRate"" Do=""14"" />
	<P N=""BR_ICMSFormula"" Do=""13"" />
	<P N=""BR_ExportJustificationInfo"" Do=""12"" />
	<P N=""BR_FinancedValue"" Do=""11"" />
	<P N=""BR_FourthCPC"" Do=""10"" />
	<P N=""BR_GoodsApplication"" Do=""9"" />
	<P N=""BR_GoodsCondition"" Do=""8"" />
	<P N=""BR_ICMSBaseValueReductionPercentage"" Do=""7"" />
	<P N=""BR_AgentCommissionPercentage"" Do=""6"" />
	<P N=""BR_CargoPriority"" Do=""5"" />
	<P N=""BR_CatalogAuthorityIdentifier"" Do=""4"" />
	<P N=""BR_CatalogAuthorityVersion"" Do=""3"" />
	<P N=""BR_ComplementaryNote"" Do=""2"" />
	<P N=""BR_DigitalServiceDossier"" Do=""1"" />
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";

		const string UpdatedXMLData = @"
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""JobComInvoiceLine"" ConfigurationSource=""SLT"" NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" Active=""true"">
  <E N=""JobComInvoiceLine"">
	<P N=""JI_UsedMaterialSerialNumber"" Do=""30"" />
	<P N=""JI_UsedMaterialRegime"" Do=""29"" />
	<P N=""JI_UsedMaterialOperationType"" Do=""28"" />
	<P N=""JI_UsedMaterialManufactureYear"" Do=""27"" />
	<P N=""JI_ThirdCPC"" Do=""26"" />
	<P N=""JI_TemporaryAdmissionReason"" Do=""25"" />
	<P N=""JI_SecondCPC"" Do=""24"" />
	<P N=""JI_RequiresImportLicense"" Do=""23"" />
	<P N=""JI_NFeNumber"" Do=""22"" />
	<P N=""JI_NFeLinePrice"" Do=""21"" />
	<P N=""JI_NFeItemNumber"" Do=""20"" />
	<P N=""JI_ManufacturerIndicator"" Do=""19"" />
	<P N=""JI_ManufacturerAuthorityVersion"" Do=""18"" />
	<P N=""JI_ManufacturerAuthorityIdentifier"" Do=""17"" />
	<P N=""JI_IntendedTermDays"" Do=""16"" />
	<P N=""JI_ICMSTotalAmountReductionPercentage"" Do=""15"" />
	<P N=""JI_ICMSRate"" Do=""14"" />
	<P N=""JI_ICMSFormula"" Do=""13"" />
	<P N=""JI_ExportJustificationInfo"" Do=""12"" />
	<P N=""JI_FinancedValue"" Do=""11"" />
	<P N=""JI_FourthCPC"" Do=""10"" />
	<P N=""JI_GoodsApplication"" Do=""9"" />
	<P N=""JI_GoodsCondition"" Do=""8"" />
	<P N=""JI_ICMSBaseValueReductionPercentage"" Do=""7"" />
	<P N=""JI_AgentCommissionPercentage"" Do=""6"" />
	<P N=""JI_CargoPriority"" Do=""5"" />
	<P N=""JI_CatalogAuthorityIdentifier"" Do=""4"" />
	<P N=""JI_CatalogAuthorityVersion"" Do=""3"" />
	<P N=""JI_ComplementaryNote"" Do=""2"" />
	<P N=""JI_DigitalServiceDossier"" Do=""1"" />
  </E>
  <ConfigurationName>Test UC Template</ConfigurationName>
</CopyTemplateTree>
";
	}
}
