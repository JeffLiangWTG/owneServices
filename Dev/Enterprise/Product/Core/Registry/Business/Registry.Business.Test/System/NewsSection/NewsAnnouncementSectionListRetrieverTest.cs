using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class NewsAnnouncementSectionListRetrieverTest : TransactionedTestCase
	{
		public void TestGetListIncludesAllNewsSectionTypeListCodes()
		{
			var wtgOnlySections = NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.WiseTechOnly).GetAllCodes();
			var customerOnlySections = NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.CustomerOnly).GetAllCodes();

			var commonSections = wtgOnlySections.Intersect(customerOnlySections);
			Assert($@"NewsAnnouncementSectionListRetriever.GetList() for WiseTechOnly and CustomerOnly both contain the following section code(s): {string.Join(", ", commonSections)}
Please add them to the respective parts in GetList()", !commonSections.Any());
		}

		public void TestGetList()
		{
			var list = NewsAnnouncementSectionListRetriever.GetList();
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ProductUpdates));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseLearningUpdates));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientAnnouncements));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientStaffNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.BorderWise));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseTechAcademy));
		}

		public void TestGetList_CustomerOnly()
		{
			var list = NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.CustomerOnly);
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.ProductUpdates));
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.WiseLearningUpdates));
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.WiseNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientAnnouncements));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientStaffNews));
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes));
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.BorderWise));
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.WiseTechAcademy));
		}

		public void TestGetList_WiseTechOnly()
		{
			var list = NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.WiseTechOnly);
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ProductUpdates));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseLearningUpdates));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseNews));
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.ClientAnnouncements));
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.ClientNews));
			AssertEquals(false, list.ContainsCode(NewsSectionTypeList.Codes.ClientStaffNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.BorderWise));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseTechAcademy));
		}

		public void TestRegistrySectionsIncluded()
		{
			var extraItems = SystemDataRegistry.Instance.NewsSectionTypes.Value;

			extraItems.Add(new NewsAnnouncementSectionType() { Code = "ZAY", Description = (NoResString)"Zayden", OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime, });
			extraItems.Add(new NewsAnnouncementSectionType() { Code = "RYL", Description = (NoResString)"Rylan", OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime, });
			extraItems.Add(new NewsAnnouncementSectionType() { Code = "LOL", Description = (NoResString)"Lola", OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime, });
			extraItems.Add(new NewsAnnouncementSectionType() { Code = "BA3", Description = (NoResString)"WHo Knows", OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime, });

			SystemDataRegistry.Instance.NewsSectionTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, extraItems);

			var list = NewsAnnouncementSectionListRetriever.GetList();
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ProductUpdates));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseLearningUpdates));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientAnnouncements));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.ClientStaffNews));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.BorderWise));
			AssertEquals(true, list.ContainsCode(NewsSectionTypeList.Codes.WiseTechAcademy));
			AssertEquals(true, list.ContainsCode("ZAY"));
			AssertEquals(true, list.ContainsCode("RYL"));
			AssertEquals(true, list.ContainsCode("BA3"));
			AssertEquals(true, list.ContainsCode("LOL"));
		}
	}
}
