using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal abstract class JXCValidationTestCase : BusinessObjectValidationTestCase
	{
		protected enum WarningMessageMatch
		{
			Exact,
			StartsWith,
			Contains,
			EndsWith
		}

		protected void AssertMaxLengthLessOrEqualToForJXC(int maxLength, ZPropertyInfo info)
		{
			AssertMaxLengthLessOrEqualToForJXC(maxLength, info.MaxLength);
		}

		protected void AssertMaxLengthLessOrEqualToForJXC(int maxLength, SchemaStringColumn column)
		{
			AssertMaxLengthLessOrEqualToForJXC(maxLength, column.MaxLength);
		}

		protected void AssertMaxLengthLessOrEqualToForJXC(int maxLength, int propertyMaxLength)
		{
			string failureMessage = "MaxLength has to be less or equal to " + maxLength + " char. If MaxLength is changed, please add a JXC MaxLength boundary test here";
			Assert(failureMessage, propertyMaxLength <= maxLength);
		}

		protected void AssertHasInvalidCurrencyCodeJXCWarning(ZPropertyInfo info)
		{
			AssertHasJXCWarning(info, JXCConstants.JXCWarningPrefix + "Invalid currency code");
		}

		protected void AssertHasNumericExactLengthJXCWarning(ZPropertyInfo info, int exactLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain numeric characters \"0-9\" and must be " + exactLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasNumericMaxLengthJXCWarning(ZPropertyInfo info, int maxLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain numeric characters \"0-9\" and must be at most " + maxLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasNumericMinMaxLengthJXCWarning(ZPropertyInfo info, int minLength, int maxLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain numeric characters \"0-9\" and must be at least " + minLength + " most " + maxLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasAlphabeticExactLengthJXCWarning(ZPropertyInfo info, int exactLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain numeric characters \"a-zA-Z\" and must be " + exactLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasAlphabeticMaxLengthJXCWarning(ZPropertyInfo info, int maxLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain numeric characters \"a-zA-Z\" and must be at most " + maxLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasAlphabeticMinMaxLengthJXCWarning(ZPropertyInfo info, int minLength, int maxLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain numeric characters \"a-zA-Z\" and must be at least " + minLength + " and at most " + maxLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasAlphanumericExactLengthJXCWarning(ZPropertyInfo info, int exactLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain alphanumeric characters \"a-zA-Z0-9_\" and must be " + exactLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasAlphanumericMaxLengthJXCWarning(ZPropertyInfo info, int maxLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain alphanumeric characters \"a-zA-Z0-9_\" and must be at most " + maxLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasAlphanumericMinMaxLengthJXCWarning(ZPropertyInfo info, int minLength, int maxLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must only contain alphanumeric characters \"a-zA-Z0-9_\" and must be at least " + minLength + " and at most " + maxLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasExactLengthJXCWarning(ZPropertyInfo info, int exactLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must be " + exactLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasMaxLengthJXCWarning(ZPropertyInfo info, int maxLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must be at most " + maxLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasMinMaxLengthJXCWarning(ZPropertyInfo info, int minLength, int maxLength)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + info.HumanReadableName + " must be at least " + minLength + " and at most " + maxLength + " characters in length";
			AssertHasJXCWarning(info, expectedErrorMessage);
		}

		protected void AssertHasNoJXCWarnings(ZString additionalFailureMessage, ZPropertyInfo info)
		{
			if (!additionalFailureMessage.IsEmpty && !additionalFailureMessage.EndsWith("."))
			{
				additionalFailureMessage = additionalFailureMessage + ". ";
			}

			AssertEquals(additionalFailureMessage + "Should have no JXC warnings on property " + info.Name, 0, GetJXCWarnings(info).Length);
		}

		protected void AssertHasNoJXCWarnings(ZPropertyInfo info)
		{
			AssertHasNoJXCWarnings("", info);
		}

		protected void AssertHasNotEnteredJXCWarning(ZPropertyInfo info)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + MandatoryValidation.YouHaveNotEntered;
			AssertHasJXCWarning(info, WarningMessageMatch.StartsWith, expectedErrorMessage);
		}

		protected void AssertHasInvalidCodeJXCWarning(ZPropertyInfo info)
		{
			string expectedErrorMessage = JXCConstants.JXCWarningPrefix + ListValidation.InvalidCodeMessage;
			AssertHasJXCWarning(info, WarningMessageMatch.Exact, expectedErrorMessage);
		}

		protected void AssertHasJXCWarning(ZPropertyInfo info, params string[] warningMessages)
		{
			AssertHasJXCWarning(info, WarningMessageMatch.Exact, warningMessages);
		}

		protected void AssertHasJXCWarning(ZPropertyInfo info, WarningMessageMatch matchType, params string[] warningMessages)
		{
			List<ZString> jXCWarnings = new List<ZString>(GetJXCWarnings(info));
			AssertEquals("Should have " + warningMessages.Length + " JXC warnings on property " + info.Name, warningMessages.Length, jXCWarnings.Count);
			ZString warnings = "";
			foreach (string warningMessage in warningMessages)
			{
				if (!jXCWarnings.Exists(GetJXCWarningMatcher(warningMessage, matchType)))
				{
					warnings += warningMessage + " on property " + info.Name + "\r\n";
				}
			}

			if (!warnings.IsEmpty)
			{
				string failureMessage = GetFailureMessagePrefix(matchType) + warnings;
				if (jXCWarnings.Count > 0)
				{
					failureMessage += "These are the following JXC Warnings added to " + info.Name + ":\r\n";
					foreach (ZString warning in jXCWarnings)
					{
						failureMessage += warning + "\r\n";
					}
				}

				Fail(failureMessage);
			}
		}

		protected ZString[] GetJXCWarnings(BusinessObject bizO)
		{
			List<ZString> list = new List<ZString>();
			foreach (INotification warning in new ZNotificationCollector(bizO, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetWarnings())
			{
				if (warning.Message.IndexOf(JXCConstants.JXCWarningPrefix) > -1)
				{
					list.Add(warning.Message);
				}
			}

			return list.ToArray();
		}

		protected ZString[] GetJXCWarnings(ZPropertyInfo propertyInfo)
		{
			List<ZString> list = new List<ZString>();
			foreach (string warning in propertyInfo.GetWarnings().GetUniqueMessageList())
			{
				if (warning.StartsWith(JXCConstants.JXCWarningPrefix))
				{
					list.Add(warning);
				}
			}

			return list.ToArray();
		}

		string GetFailureMessagePrefix(WarningMessageMatch matchType)
		{
			string result = "";
			switch (matchType)
			{
				case WarningMessageMatch.Exact:
					result = "\r\nExpecting JXC warnings:\r\n";
					break;
				case WarningMessageMatch.StartsWith:
					result = "\r\nExpecting JXC warnings starting with the following text:\r\n";
					break;
				case WarningMessageMatch.EndsWith:
					result = "\r\nExpecting JXC warnings ending with the following text:\r\n";
					break;
				case WarningMessageMatch.Contains:
					result = "\r\nExpecting JXC warnings containing the following text:\r\n";
					break;
			}

			return result;
		}

		Predicate<ZString> GetJXCWarningMatcher(string warningMessage, WarningMessageMatch matchType)
		{
			return delegate(ZString stringToMatch)
			{
				bool result;
				if (matchType == WarningMessageMatch.Exact)
				{
					result = stringToMatch == warningMessage;
				}
				else if (matchType == WarningMessageMatch.StartsWith)
				{
					result = stringToMatch.StartsWith(warningMessage);
				}
				else if (matchType == WarningMessageMatch.EndsWith)
				{
					result = stringToMatch.EndsWith(warningMessage);
				}
				else
				{
					result = stringToMatch.Contains(warningMessage);
				}

				return result;
			};
		}

		protected ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}

				return fValidationHelper;
			}
		}

		ValidationHelper fValidationHelper;
	}
}
