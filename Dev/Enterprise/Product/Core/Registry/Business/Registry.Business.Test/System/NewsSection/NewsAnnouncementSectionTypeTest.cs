using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Registry.Business.NewsAnnouncementSectionSectionCategory;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NewsAnnouncementSectionType))]
	sealed class NewsAnnouncementSectionTypeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestSectionCategory_AutoFillForSystemDefined()
		{
			var sectionSystemDefined = new NewsAnnouncementSectionType();
			sectionSystemDefined.SystemDefined = true;
			sectionSystemDefined.Code = NewsSectionTypeList.Codes.WiseTechAcademy;
			AssertEquals(NewsAnnouncementSectionSectionCategoryTypeList.WiseTechGlobalCommunication, sectionSystemDefined.SectionCategory);

			var sectionUserDefined = new NewsAnnouncementSectionType();
			sectionSystemDefined.SystemDefined = false;
			sectionUserDefined.Code = NewsSectionTypeList.Codes.WiseTechAcademy;
			AssertEquals(NewsAnnouncementSectionSectionCategoryTypeList.NewsAnnouncements, sectionUserDefined.SectionCategory);
		}

		public void TestSystemDefinedORDReadOnlyMembers()
		{
			var sectionType = new NewsAnnouncementSectionType();
			sectionType.Code = "TST";
			sectionType.Description = (NoResString)"Some Description";
			sectionType.OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime;
			sectionType.SystemDefined = true;

			Assert(sectionType.CodeInfo.ReadOnly);
			Assert(sectionType.EnglishDescriptionToShowInfo.ReadOnly);
			Assert(sectionType.OrderItemsByInfo.ReadOnly);
			Assert(sectionType.SectionCategoryInfo.ReadOnly);
		}

		public void TestEnglishDescriptionToShow()
		{
			var transportReferenceNumberType = new NewsAnnouncementSectionType();
			transportReferenceNumberType.Code = "TST";
			transportReferenceNumberType.Description = (NoResString)"Some Description {0}";
			transportReferenceNumberType.SystemDefined = true;

			AssertEquals("SystemDefined {0} will be replaced with company name", "Some Description " + Env.CurrentCompany.Name, transportReferenceNumberType.EnglishDescriptionToShow);

			transportReferenceNumberType.SystemDefined = false;

			AssertEquals("Non-SystemDefined {0} will keep", "Some Description {0}", transportReferenceNumberType.EnglishDescriptionToShow);
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

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new NewsAnnouncementSectionType();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NewsAnnouncementSectionType();
		}

		#endregion
	}
}
