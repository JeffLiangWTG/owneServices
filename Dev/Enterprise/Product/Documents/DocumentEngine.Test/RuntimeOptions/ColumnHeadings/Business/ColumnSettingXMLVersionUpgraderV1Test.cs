using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class ColumnSettingXMLVersionUpgraderV1Test : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpgradeWorks()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZGuid reportID = ColumnSettingWithUpgraderTestHelper.GetReportIDThruFullTemplatePivotMenuBuild("MultipleTemplatesFirstRenderableAfterNonRenderables.xls");

			ColumnHeadingCollection columnHeadings = new ColumnHeadingCollection();
			columnHeadings.Add(new ColumnHeading("Label 1", "Text 1", 0, 0, 12, false));
			columnHeadings.Add(new ColumnHeading("Label 2", "Text 2", 1, 2, 10, true));
			columnHeadings.Add(new ColumnHeading("Label 3", "Text 3", 2, 1, 11, true));

			string xMLin = "";
			StringWriter writer = new StringWriter();
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(ColumnHeadingCollection));
			serialiser.Serialize(writer, columnHeadings);
			xMLin = writer.ToString();

			ColumnConfigurationsManager manager = new ColumnConfigurationsManager(reportID, false);
			ColumnSettingXMLVersionUpgraderCurrentVersionParameters parameters = new ColumnSettingXMLVersionUpgraderCurrentVersionParameters(manager);
			ColumnSettingXMLVersionUpgraderCurrentVersion upgrader = new ColumnSettingXMLVersionUpgraderCurrentVersion(parameters);
			string xMLout = upgrader.Upgrade(xMLin);

			ReportColumnSettings columnSettings;
			serialiser = ZXmlSerializer.New(typeof(ReportColumnSettings));
			columnSettings = (ReportColumnSettings)serialiser.Deserialize(new StringReader(xMLout));

			AssertEquals("ColumnHeading version is 1", 1, columnSettings.Version);
			AssertEquals("There is only one worksheet", 1, columnSettings.Worksheets.Count);
			AssertEquals("the worksheet is called Sheet2", "Sheet2", columnSettings.Worksheets["Sheet2"].Name);
			AssertEquals("Sheet2 contains 3 column settings", 3, columnSettings.Worksheets["Sheet2"].ColumnHeadings.Count);
			AssertEquals("Sheet2 column 0 contains 3 column settings", 3, columnSettings.Worksheets["Sheet2"].ColumnHeadings.Count);
			AssertColumnHeading(columnSettings.Worksheets["Sheet2"].ColumnHeadings[0], columnHeadings[0]);
			AssertColumnHeading(columnSettings.Worksheets["Sheet2"].ColumnHeadings[1], columnHeadings[1]);
			AssertColumnHeading(columnSettings.Worksheets["Sheet2"].ColumnHeadings[2], columnHeadings[2]);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpgradeWorks_PivotMissed()
		{
			var factory = new BusinessObjectFactory();
			var reportID = ColumnSettingWithUpgraderTestHelper.GetReportIDThruFullTemplatePivotMenuBuild("MultipleTemplatesFirstRenderableAfterNonRenderables.xls");

			var columnHeadings = new ColumnHeadingCollection();
			columnHeadings.Add(new ColumnHeading("Label 1", "Text 1", 0, 0, 12, false));
			columnHeadings.Add(new ColumnHeading("Label 2", "Text 2", 1, 2, 10, true));
			columnHeadings.Add(new ColumnHeading("Label 3", "Text 3", 2, 1, 11, true));

			var xMLin = "";
			var writer = new StringWriter();
			var serialiser = ZXmlSerializer.New(typeof(ColumnHeadingCollection));
			serialiser.Serialize(writer, columnHeadings);
			xMLin = writer.ToString();

			var pivot = factory.LoadTop1<StmMenuTemplatePivotBase>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, reportID));
			pivot.Delete();
			factory.Save();

			var manager = new ColumnConfigurationsManager(reportID, false);
			var parameters = new ColumnSettingXMLVersionUpgraderCurrentVersionParameters(manager);
			var upgrader = new ColumnSettingXMLVersionUpgraderCurrentVersion(parameters);
			AssertExceptionThrown<ColumnSettingXMLInvalidException>(() => upgrader.Upgrade(xMLin));
		}

		void AssertColumnHeading(Enterprise.DocumentEngine.RuntimeOptions.ColumnHeading @new, ColumnHeading old)
		{
			AssertEquals("labels is wrong", @new.DisplayLabel, old.DisplayLabel);
			AssertEquals("Text is wrong", @new.HeadingText, old.HeadingText);
			AssertEquals("Original pos wrong", @new.OriginalColumnNumber, old.OriginalColumnNumber);
			AssertEquals("Current pos wrong", @new.CurrentPosition, old.CurrentPosition);
			AssertEquals("width wrong", @new.WidthInPixels, old.WidthInPixels);
			AssertEquals("hidden wrong", @new.Hidden, old.Hidden);
		}

		ColumnSettingWithUpgraderTestHelper ColumnSettingWithUpgraderTestHelper
		{
			get
			{
				if (fColumnSettingWithUpgraderTestHelper == null)
				{
					fColumnSettingWithUpgraderTestHelper = new ColumnSettingWithUpgraderTestHelper();
				}
				return fColumnSettingWithUpgraderTestHelper;
			}
		}
		ColumnSettingWithUpgraderTestHelper fColumnSettingWithUpgraderTestHelper;
	}
}
