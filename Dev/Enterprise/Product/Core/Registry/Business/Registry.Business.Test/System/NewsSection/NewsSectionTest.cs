using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NewsSection))]
	sealed class NewsSectionTest : RegistryBusinessObjectTemplateTestCase<NewsSection>
	{
		public void IsWiseTechSection()
		{
			var section = new NewsSection();
			AssertEquals(false, section.IsWiseTechSection);

			section.SectionID = NewsSectionTypeList.Codes.WiseNews;
			AssertEquals(true, section.IsWiseTechSection);

			section.SectionID = NewsSectionTypeList.Codes.ProductUpdates;
			AssertEquals(true, section.IsWiseTechSection);

			section.SectionID = NewsSectionTypeList.Codes.WiseLearningUpdates;
			AssertEquals(true, section.IsWiseTechSection);

			section.SectionID = NewsSectionTypeList.Codes.ClientAnnouncements;
			AssertEquals(false, section.IsWiseTechSection);

			section.SectionID = NewsSectionTypeList.Codes.ClientNews;
			AssertEquals(false, section.IsWiseTechSection);

			section.SectionID = NewsSectionTypeList.Codes.ClientStaffNews;
			AssertEquals(false, section.IsWiseTechSection);

			section.SectionID = NewsSectionTypeList.Codes.TechnicalAdvisoryNotes;
			AssertEquals(true, section.IsWiseTechSection);
		}

		public void TestWiseTechSectionsReadOnly()
		{
			var section = new NewsSection();
			section.LayoutPanelID = "Top-Right";
			AssertEquals(false, section.SectionIDInfo.ReadOnly);
			AssertEquals(false, section.HideReadItemsInfo.ReadOnly);
			AssertEquals(false, section.MandatoryToReadInfo.ReadOnly);

			section.LayoutPanelID = "Bottom-Right";
			AssertEquals(true, section.SectionIDInfo.ReadOnly);
			AssertEquals(false, section.HideReadItemsInfo.ReadOnly);
			AssertEquals(false, section.MandatoryToReadInfo.ReadOnly);
		}

		public void TestSectionName()
		{
			var section = new NewsSection();
			section.SectionID = NewsSectionTypeList.Codes.WiseLearningUpdates;
			AssertEquals(NewsSectionTypeList.Descriptions.WiseLearningUpdates, section.SectionName);

			section.SectionID = string.Empty;
			AssertEquals(string.Empty, section.SectionName);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override NewsSection GetBusinessObjectToClone()
		{
			return new NewsSection();
		}

		protected override NewsSection GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
