using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class DefaultTemplateConfigurationManagerTest : TestCase
	{
		DefaultTemplateConfigurationManagerForTesting setting;

		DefaultTemplateConfigurationManagerForTesting Setting
		{
			get
			{
				if (setting == null)
				{
					setting = new DefaultTemplateConfigurationManagerForTesting(new ColumnConfigurationsManager(ZGuid.NewZGuid(), false));
					setting.AddHeading("Sheet1", new ColumnHeading("1", "1d", "2", 3, 4, 5, false));
					setting.AddHeading("Sheet1", new ColumnHeading("2", "2d", "3", 4, 5, 6, true));
				}
				return setting;
			}
		}

		void AssertHeadings(ReportColumnSettings headings)
		{
			AssertEquals("Length", 2, headings.Worksheets["Sheet1"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(headings.Worksheets["Sheet1"].ColumnHeadings[0], "1", "1d", "2", 3, 4, 5, false);
			ColumnHeadingTest.AssertPropertyValues(headings.Worksheets["Sheet1"].ColumnHeadings[1], "2", "2d", "3", 4, 5, 6, true);
			Assert("Heading should not be the same instance.", Setting.headings.Worksheets["Sheet1"].ColumnHeadings[0] != headings.Worksheets["Sheet1"].ColumnHeadings[0]);
			Assert("Heading should not be the same instance.", Setting.headings.Worksheets["Sheet1"].ColumnHeadings[1] != headings.Worksheets["Sheet1"].ColumnHeadings[1]);
		}

		public void TestAddAndLoad()
		{
			Setting.Load();
			AssertHeadings(Setting.HeadingManager.CurrentConfiguration);

			using (var docPack = new DocumentPack())
			{
				docPack.Language = Core.SharedConstants.Languages.ChineseSimplified;
				Setting.Load(null, null, null, null, docPack);
				AssertEquals("Language of DefaultTemplateConfiguration should be Current ENG", DataRegistry.Instance.EnglishSpelling, docPack.Language);
			}
		}

		public void AssertSetTitle()
		{
			AssertEquals("The title should be empty", string.Empty, Setting.headings.Worksheets["Sheet1"].Title);
			Setting.SetTitle("Sheet1", "Title1");
			AssertEquals("The title should not be empty", "Title1", Setting.headings.Worksheets["Sheet1"].Title);
		}

		public void TestCanSaveAndDelete()
		{
			AssertEquals("CanSaveAndDelete", false, Setting.CanSaveAndDelete);
		}

		public void TestGetCopyOfHeadings()
		{
			AssertHeadings(Setting.GetCopyOfHeadings());
		}

		public class DefaultTemplateConfigurationManagerForTesting : DefaultTemplateConfigurationManager
		{
			public DefaultTemplateConfigurationManagerForTesting(ColumnConfigurationsManager headingManager) : base(headingManager)
			{
			}

			protected internal new ReportColumnSettings headings => base.headings;
		}
	}
}
