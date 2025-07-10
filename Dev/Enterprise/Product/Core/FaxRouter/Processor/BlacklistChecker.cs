using System.Collections;
using System.Text.RegularExpressions;

namespace Enterprise.FaxRouter.Processor
{
	public class BlacklistChecker
	{
		#region Singleton interface

		public static BlacklistResult Check(MailDBItemDataLine mailDBItem)
		{
			return checker.CheckInternal(mailDBItem);
		}

		static readonly BlacklistChecker checker = new BlacklistChecker(Constants.BLACKLIST);

		#endregion

		internal BlacklistChecker(string blacklistConfigString)
		{
			if (blacklistConfigString.Length > 0)
			{
				foreach (string blacklistedAddress in blacklistConfigString.Split('/'))
				{
					string address, regexPattern;
					int separatorIndex = blacklistedAddress.IndexOf(':');
					address = blacklistedAddress.Substring(0, separatorIndex);
					regexPattern = blacklistedAddress.Substring(separatorIndex + 1);

					blacklistRegexes[address] = new Regex("^" + regexPattern, RegexOptions.Compiled);
				}
			}
		}

		readonly Hashtable blacklistRegexes = new Hashtable();
		static readonly Regex insignificantCharactersInFaxNumber = new Regex("[^+0-9]");

		internal BlacklistResult CheckInternal(MailDBItemDataLine mailDBItem)
		{
			BlacklistResult result = new BlacklistResult();
			result.Destination = insignificantCharactersInFaxNumber.Replace(mailDBItem.FaxRecipientNumber, "");
			foreach (string emailAddress in blacklistRegexes.Keys)
			{
				if (mailDBItem.From.ToLower().IndexOf(emailAddress.ToLower()) >= 0)
				{
					Regex allowedRegex = blacklistRegexes[emailAddress] as Regex;
					result.Sender = emailAddress;
					result.IsBlacklisted = !allowedRegex.IsMatch(result.Destination);
					break;
				}
			}
			return result;
		}
	}

	public class BlacklistResult
	{
		public bool IsBlacklisted;
		public string Sender;
		public string Destination;
	}
}
