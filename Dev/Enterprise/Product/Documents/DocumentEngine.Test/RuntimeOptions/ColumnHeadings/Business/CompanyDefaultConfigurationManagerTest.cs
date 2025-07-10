using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class CompanyDefaultConfigurationManagerTest : ColumnConfigurationManagerAbstractTest<CompanyDefaultConfigurationManager>
	{
		public void TestLoadWhenEmpty()
		{
			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("1"));
			Setting.Load();
			AssertEquals("HeadingManager.Headings.Count", 1, HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Count);
			AssertEquals("HeadingManager.Headings[0].DisplayLabel", "1", HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].DisplayLabel);
		}

		public override void TestNotEquals()
		{
			CompanyDefaultConfigurationManager a = GetNewSetting(HeadingManager);
			CompanyDefaultConfigurationManager b = GetNewSetting(HeadingManager);
			AssertEquals("They are always equal becuase there can be only one", a, b);
		}

		protected override string ExpectedToString => string.Format("{0}({1}) - Login Company Default Configuration", GlbCompany.CurrentCompany.GC_Name, GlbCompany.CurrentCompany.GC_Code);

		protected override CompanyDefaultConfigurationManager GetNewSetting(ColumnConfigurationsManager headingManager) => new CompanyDefaultConfigurationManager(headingManager);
	}
}
