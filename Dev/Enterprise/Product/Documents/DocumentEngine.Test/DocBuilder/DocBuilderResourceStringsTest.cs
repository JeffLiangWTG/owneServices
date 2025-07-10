using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class DocBuilderResourceStringsTest : TestCase
	{
		public void TestGetKey()
		{
			Assert(DocBuilderResourceStrings.GetKey(null, "Two") == DocBuilderResourceStrings.GetKey("", DocBuilderResourceStrings.DocLabelKeyPrefix, "Two"));

			var templateSection1 = new TemplateSection("GEN:Headings, One", 0, 0);
			Assert(DocBuilderResourceStrings.GetKey(templateSection1, "{0} {1}") == DocBuilderResourceStrings.GetKey("One", DocBuilderResourceStrings.DocLabelKeyPrefix, "{0} {1}"));
		}

		public void TestGetStringFallbacks()
		{
			using (var mockData = Res.UseMockData())
			{
				string key = DocBuilderResourceStrings.GetKey(null, "One");
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "Eno", string.Empty));
				key = DocBuilderResourceStrings.GetKey(null, "Two");
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, string.Empty, "Owt"));

				Assert(DocBuilderResourceStrings.GetKey(null, "Two") == DocBuilderResourceStrings.GetKey("", DocBuilderResourceStrings.DocLabelKeyPrefix, "Two"));
				AssertEquals("Eno", DocBuilderResourceStrings.GetString(null, "One"));
				AssertEquals("Owt", DocBuilderResourceStrings.GetString(null, "Two"));
				AssertEquals("Three", DocBuilderResourceStrings.GetString(null, "Three"));
			}
		}

		public void TestGetLocalizedReportName()
		{
			using (var mockData = Res.UseMockData())
			{
				var key = DocBuilderResourceStrings.ReportNameKeyPrefix + "Test";
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "测试", string.Empty));
				AssertEquals("测试", DocBuilderResourceStrings.GetLocalizedReportName("Test"));
			}
		}

		public void TestGetLocalizedReportTitle()
		{
			using (var mockData = Res.UseMockData())
			{
				var key = DocBuilderResourceStrings.ReportTitleKeyPrefix + "Test";
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "测试", string.Empty));
				AssertEquals("测试", DocBuilderResourceStrings.GetLocalizedReportTitle("Test"));
			}
		}

		public void TestGetDocLabelStringData()
		{
			using (var mockData = Res.UseMockData())
			{
				var key = DocBuilderResourceStrings.GetKey("", DocBuilderResourceStrings.DocLabelKeyPrefix, "Test");
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "测试", string.Empty));

				AssertEquals("测试", DocBuilderResourceStrings.GetDocLabelStringData("Test").Caption);
			}
		}

		public void TestGetCoverSheetString()
		{
			using (var mockData = Res.UseMockData())
			{
				var key = DocBuilderResourceStrings.GetKey("EmailCoverSheet", DocBuilderResourceStrings.CoverSheetLabelKeyPrefix, "Test");
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "测试", string.Empty));

				AssertEquals("测试", DocBuilderResourceStrings.GetCoverSheetString("EmailCoverSheet", "Test"));
			}
		}

		public void TestStringsRequiringSectionSpecificKey()
		{
			using (var mockData = Res.UseMockData())
			{
				var templateSection1 = new TemplateSection("GEN:Headings, One", 0, 0);
				var key = DocBuilderResourceStrings.GetKey(templateSection1, "{0} {1}");
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "{0} one {1}", string.Empty));
				key = DocBuilderResourceStrings.GetKey(templateSection1, "This string does not require section specific key");
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "Yek cificeps noitces eriuqer ton seod gnirts siht", string.Empty));

				var templateSection2 = new TemplateSection("GEN:Headings, Two", 0, 0);
				key = DocBuilderResourceStrings.GetKey(templateSection2, "{0} {1}");
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "{0} {1} two", string.Empty));

				AssertEquals("{0} one {1}", DocBuilderResourceStrings.GetString(templateSection1, "{0} {1}"));
				AssertEquals("{0} {1} two", DocBuilderResourceStrings.GetString(templateSection2, "{0} {1}"));
				AssertEquals("Yek cificeps noitces eriuqer ton seod gnirts siht", DocBuilderResourceStrings.GetString(templateSection1, "This string does not require section specific key"));
				AssertEquals("Yek cificeps noitces eriuqer ton seod gnirts siht", DocBuilderResourceStrings.GetString(templateSection2, "This string does not require section specific key"));
			}
		}

		public void TestRightToLeftFlipData()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Arabic))
			{
				AssertEquals("{1} {0}", DocBuilderResourceStrings.GetString(null, "{0} {1}"));
				AssertEquals("{0} {1}", DocBuilderResourceStrings.GetString(null, "{1} {0}"));
				AssertEquals("{1} & {0}", DocBuilderResourceStrings.GetString(null, "{0} & {1}"));
				AssertEquals("{2} {1} {0}", DocBuilderResourceStrings.GetString(null, "{0} {1} {2}"));
				AssertEquals("{0} + {1} - {2}", DocBuilderResourceStrings.GetString(null, "{2} - {1} + {0}"));
				AssertEquals("{3} {2} {1} {0}", DocBuilderResourceStrings.GetString(null, "{0} {1} {2} {3}"));
				AssertEquals("Something {0} {1}", DocBuilderResourceStrings.GetString(null, "Something {0} {1}"));
			}
		}
	}
}
