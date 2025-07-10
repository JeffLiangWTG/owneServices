using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiGlbStaffEx : AutoEdiGlbStaffEx
	{
		public EdiGlbStaffEx(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool PopulateNamesFromStaffFullName()
		{
			return PopulateNamesFromFullName(Staff.GS_FullName, Staff.GS_LoginName);
		}

		public bool PopulateNamesFromFullName(ZString fullName, ZString loginName)
		{
			if (fullName.IndexOf(' ') == -1)
			{
				return false;
			}

			var name = fullName.Replace(",", "").Trim();

			// Foreign name is expected to be after English name
			var mainName = name.StripNonWesternEuropeanCharacters();
			var foreignName = ZString.Empty;
			if (mainName.Length > 0 && name.Length > mainName.Length && name.StartsWith(mainName, StringComparison.Ordinal))
			{
				foreignName = name.Substring(mainName.Length).Trim();
			}
			mainName = mainName.Trim();
			if (mainName.Length == 0)
			{
				mainName = name;
			}

			if (mainName.Length > 0)
			{
				// remove anything in brackets
				int leftBracketIndex;
				while (0 <= (leftBracketIndex = mainName.IndexOf('(')))
				{
					int rightBracketIndex = mainName.IndexOf(")", leftBracketIndex + 1, StringComparison.OrdinalIgnoreCase);
					if (rightBracketIndex >= 0)
					{
						mainName = mainName.Remove(leftBracketIndex, rightBracketIndex - leftBracketIndex + 1);
					}
					else
					{
						break;
					}
				}

				var names = mainName.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

				int lastNameWordCount = Math.Max(CalculateLastNameWordCountFromNameRules(names),
					CalculateLastNameWordCountFromLoginName(names, loginName));

				if (lastNameWordCount == 0)
				{
					lastNameWordCount = 1;
				}

				GS9_FirstName = names[0];
				GS9_LastName = string.Join(" ", names.Skip(names.Length - lastNameWordCount));
				int middleNameCount = names.Length - lastNameWordCount - 1;
				if (middleNameCount > 0)
				{
					GS9_MiddleName = string.Join(" ", names.Skip(1).Take(middleNameCount));
				}
				else
				{
					GS9_MiddleName = "";
				}

				if (foreignName.Length > 0)
				{
					GS9_DomesticName = foreignName;
				}

				return true;
			}

			return false;
		}

		int CalculateLastNameWordCountFromNameRules(ZString[] names)
		{
			for (int i = 1; i < names.Length; ++i)
			{
				var s = names[i];
				if (string.Equals(s, "van", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(s, "de", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(s, "da", StringComparison.OrdinalIgnoreCase))
				{
					return names.Length - i;
				}
			}

			return 0;
		}

		static int CalculateLastNameWordCountFromLoginName(ZString[] names, ZString loginName)
		{
			// if login name has a single period and the text after the period matches the ending characters of the full name then
			// the ending characters are the last name
			var namesNormalized = new ZString[names.Length];
			for (int i = 0; i < names.Length; ++i)
			{
				namesNormalized[i] = ToRegularLowerCaseLetters(names[i]);
			}
			var fullNameNormalized = string.Join("", namesNormalized);

			int lastNameWordCount = 0;
			int periodIndex = loginName.IndexOf('.');
			if (periodIndex > 0
				&& periodIndex < loginName.Length - 1
				&& loginName.LastIndexOf('.') == periodIndex)
			{
				var loginLastPartNormalized = ToRegularLowerCaseLetters(loginName.Substring(periodIndex + 1));
				if (loginLastPartNormalized.Length > 0 && fullNameNormalized.EndsWith(loginLastPartNormalized, StringComparison.Ordinal))
				{
					int remainingLengthToMatch = loginLastPartNormalized.Length;
					int i = namesNormalized.Length - 1;
					while (i >= 1
						&& remainingLengthToMatch >= namesNormalized[i].Length)
					{
						remainingLengthToMatch -= namesNormalized[i].Length;
						--i;
					}
					if (remainingLengthToMatch == 0)
					{
						lastNameWordCount = namesNormalized.Length - i - 1;
					}
				}
			}
			return lastNameWordCount;
		}

		static string ToRegularLowerCaseLetters(ZString name)
		{
			var result = new StringBuilder();
			foreach (var ch in name.RemoveDiacritics())
			{
				if (char.IsLetter(ch))
				{
					result.Append(char.ToLowerInvariant(ch));
				}
			}
			return result.ToString();
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new StaffExUniqueIndexFailureHandler(); }
		}

		class StaffExUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError("While you have been working, another user has made changes. Please close and reopen the form again.", "Save Error");
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return EdiGlbStaffExSchema.Constants.Indexes.NR_UC__GS9_GS; }
			}
		}
	}
}

