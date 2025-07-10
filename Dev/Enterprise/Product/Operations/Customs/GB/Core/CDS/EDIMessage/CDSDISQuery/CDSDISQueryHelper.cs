using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public static class CDSDISQueryHelper
	{
		public static ZString ComposeQueryString(CDSDISQueryMessage message)
		{
			var query = new ZStringBuilder();
			_ = query.Append($"{Constants.QueryStringParameters.DeclarationCategory}{message.DeclarationCategory}");
			_ = query.Append($"{Constants.QueryStringParameters.DeclarationStatus}{message.DeclarationStatus}");
			_ = query.Append($"{Constants.QueryStringParameters.DateFrom}{FormatDate(message.DateFrom)}");
			_ = query.Append($"{Constants.QueryStringParameters.DateTo}{FormatDate(message.DateTo)}");
			_ = query.Append($"{Constants.QueryStringParameters.PageNumber}{message.PageNumber}");
			return query.ToString();
		}

		public static ZString FormatQueryString(ZString reference)
		{
			var result = reference.ReplaceIgnoringCase(Constants.QueryStringParameters.DeclarationCategory, Constants.QueryStringParameters.DeclarationCategoryName);
			result = result.ReplaceIgnoringCase(Constants.QueryStringParameters.DeclarationStatus, Constants.QueryStringParameters.DeclarationStatusName);
			result = result.ReplaceIgnoringCase(Constants.QueryStringParameters.DateFrom, Constants.QueryStringParameters.DateFromName);
			result = result.ReplaceIgnoringCase(Constants.QueryStringParameters.DateTo, Constants.QueryStringParameters.DateToName);
			result = result.ReplaceIgnoringCase(Constants.QueryStringParameters.PageNumber, Constants.QueryStringParameters.PageNumberName);
			result = $"{Constants.QueryStringParameters.PartyRole}{result}";
			return result;
		}

		public static string FormatDate(ZDate date)
		{
			return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		}

		public static class Constants
		{
			public static class NotificationTypes
			{
				public const string Full = "full";
				public const string Status = "status";
				public const string Id = "id";
				public const string List = "list";
			}

			public static class EntryNumberTypes
			{
				public const string MRN = "MRN";
				public const string DUCR = "DUCR";
				public const string UCR = "UCR";
				public const string Inventory = "INVENTORY-REFERENCE";
			}

			public static class ContextTypes
			{
				public const string EntryNumberType = "EntryNumberType";
				public const string EntryNumber = "EntryNumber";
				public const string NotificationType = "NotificationType";
				public const string QueryString = "QueryString";
			}

			public static class QueryStringParameters
			{
				public const string PartyRole = "partyRole=submitter";
				public const string DeclarationCategory = "c=";
				public const string DeclarationCategoryName = "&declarationCategory=";
				public const string DeclarationStatus = "&s=";
				public const string DeclarationStatusName = "&declarationStatus=";
				public const string DateFrom = "&f=";
				public const string DateFromName = "&dateFrom=";
				public const string DateTo = "&t=";
				public const string DateToName = "&dateTo=";
				public const string PageNumber = "&p=";
				public const string PageNumberName = "&pageNumber=";
			}

			public static class DeclarationCategories
			{
				public const string IM = "IM";
				public const string EX = "EX";
				public const string CO = "CO";
				public const string ALL = "ALL";
			}

			public static class DeclarationStatuses
			{
				public const string Cleared = "Cleared";
				public const string Uncleared = "Uncleared";
				public const string Rejected = "Rejected";
				public const string All = "All";
			}
		}
	}
}
