using System.Globalization;
using System.Linq;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public enum NewsAnnouncementSectionListMode
	{
		All,
		CustomerOnly,
		WiseTechOnly
	}

	public static class NewsAnnouncementSectionSectionCategory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static class NewsAnnouncementSectionSectionCategoryTypeList
		{
			public const string WiseTechGlobalCommunication = "WiseTech Global Communication";
			public const string NewsAnnouncements = "News & Announcements";
		}

		public static bool IsWiseTechGlobalCommunicationDefaultCode(string code)
		{
			string[] newsSectionTypeListDefaultCodes_WiseTechGlobalCommunication =  {
				NewsSectionTypeList.Codes.ProductUpdates, NewsSectionTypeList.Codes.WiseLearningUpdates,
				NewsSectionTypeList.Codes.WiseNews, NewsSectionTypeList.Codes.TechnicalAdvisoryNotes,
				NewsSectionTypeList.Codes.BorderWise, NewsSectionTypeList.Codes.WiseTechAcademy,
			};
			return newsSectionTypeListDefaultCodes_WiseTechGlobalCommunication.Contains(code);
		}

		public static bool IsNewsAnnouncementsDefaultCode(string code)
		{
			string[] newsSectionTypeListDefaultCodes_NewsAnnouncements =  {
				NewsSectionTypeList.Codes.ClientAnnouncements,
				NewsSectionTypeList.Codes.ClientNews,
				NewsSectionTypeList.Codes.ClientStaffNews,
			};
			return newsSectionTypeListDefaultCodes_NewsAnnouncements.Contains(code);
		}
	}

	public static class NewsAnnouncementSectionListRetriever
	{
		public static CodeDescriptionPairList GetList(NewsAnnouncementSectionListMode mode = NewsAnnouncementSectionListMode.All)
		{
			var companyName = GetCompanyName();
			var result = new CodeDescriptionPairList();
			foreach (var item in SystemDataRegistry.Instance.NewsSectionTypes.Value)
			{
				if (mode == NewsAnnouncementSectionListMode.CustomerOnly && NewsAnnouncementSectionSectionCategory.IsWiseTechGlobalCommunicationDefaultCode(item.Code)
					||
					mode == NewsAnnouncementSectionListMode.WiseTechOnly && NewsAnnouncementSectionSectionCategory.IsNewsAnnouncementsDefaultCode(item.Code))
				{
					continue;
				}

				result.AddPair(item.Code, string.Format(CultureInfo.InvariantCulture, item.Description, companyName));
			}

			return result;
		}

		public static string GetCompanyName()
		{
			var result = string.Empty;
			if (Env.CurrentCompany != null)
			{
				var shortName = SystemDataRegistry.Instance.CompanyShortNameForNews.Value;
				result = !string.IsNullOrEmpty(shortName) ? shortName : Env.CurrentCompany.Name;
			}
			else
			{
				result = Res.GetString("e48d7d2c-8c64-4e32-9956-abd07836dd91", "Your");
			}

			return result;
		}
	}
}
