using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.StabilityChecker
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class StabilityChecker
	{
		public static StabilityResults CalculateStabilityResults()
		{
			return StabilityChecker.CalculateStabilityResults(AssemblyMetaDataReader.GetAttributes<StabilityCheckerAttribute>());
		}

		public static StabilityResults CalculateStabilityResults(IEnumerable<StabilityCheckerAttribute> stabilityAttributes)
		{
			var results = new StabilityResults();
			results.Results.AddRange(stabilityAttributes.SelectMany(ResultsForThisAttribute));
			results.DateTimeCalculated = ZDateTime.UtcNow;

			return results;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static IEnumerable<StabilityResult> ResultsForThisAttribute(StabilityCheckerAttribute attribute)
		{
			try
			{
				var checker = (IStabilityChecker)Activator.CreateInstance(attribute.Type);
				return checker.Check() ?? Enumerable.Empty<StabilityResult>();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("The stability checker failed with an exception.", ex);
				var exceptionStabilityResult = new StabilityResult(StabilityResultLevel.Exception, (NoResString)"The stability checker failed with an exception.\r\n\r\n" + new ExceptionDetails(ex).GetStackTraceAndMessage());
				return new[] { exceptionStabilityResult };
			}
		}

		public static void StoreStabilityResults(StabilityResults results)
		{
			StabilityCheckerRegistry.Instance.StabilityCheckerResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, results);
		}

		public static StabilityResults GetStoredStabilityResults()
		{
			return StabilityCheckerRegistry.Instance.StabilityCheckerResults.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public static void NotifyUsersOfStabilityResultsIfRequired(StabilityResults results)
		{
			NotifyUsersOfStabilityResultsIfRequired(results, isUserNotification: false);
			NotifyUsersOfStabilityResultsIfRequired(results, isUserNotification: true);
		}

		static void NotifyUsersOfStabilityResultsIfRequired(StabilityResults results, bool isUserNotification)
		{
			if (isUserNotification && results.Results.HasUserNotificationIssues || !isUserNotification && results.Results.HasNonUserRelatedIssues)
			{
				var groupPK = isUserNotification ? Constants.Groups.PostMastersGroupPK : NotificationDataRegistry.Instance.SystemServiceTasksNotificationGroup.Value;

				var emailUtility = new EmailGroupUtility();
				var groupEmails = emailUtility.GetGroupEmailCollection(groupPK, false);

				var systemServiceTasksNotificationGroupHasEmails = groupEmails.Count > 0;
				if (!systemServiceTasksNotificationGroupHasEmails && !isUserNotification)
				{
					groupEmails = emailUtility.GetGroupEmailCollection(Constants.Groups.PostMastersGroupPK, false);
				}

				if (groupEmails.Count > 0)
				{
					var emailDef = new EmailDef();
					emailDef.AddRecipientForUserCommunication(groupEmails);

					emailDef.FromDisplayName = Core.Constants.ProductName + (NoResString)" System Stability Checker";
					emailDef.Subject = Core.Constants.ProductName + (NoResString)" System Stability Check";

					var body = new StringBuilder();

					if (!systemServiceTasksNotificationGroupHasEmails)
					{
						body.AppendLine((NoResString)"This email was intended for System Service Tasks Notification Group which is not set up in the registry item 'Notification > System Service Tasks Notification Group' or is empty.");
					}

					body.AppendLine((NoResString)"System Stability Check");
					body.AppendLine();
					body.AppendLine(string.Format(CultureInfo.InvariantCulture,
						(NoResString)"\tLicense Code:\t\t" + new EnterpriseInformationRetriever().LicenceCode));
					body.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"\tDB Server Name:\t" + Db.ServerName));
					body.AppendLine(string.Format(CultureInfo.InvariantCulture,
						(NoResString)"\tDatabase Name:\t" + Db.DatabaseName));
					body.AppendLine();
					body.AppendLine(string.Format(CultureInfo.InvariantCulture,
						(NoResString)"\tReporting Computer:\t" + System.Environment.MachineName));
					body.AppendLine(string.Format(CultureInfo.InvariantCulture,
						(NoResString)"\tReporting Time:\t" + results.DateTimeCalculated));
					body.AppendLine().AppendLine();

					foreach (StabilityResult result in results.Results)
					{
						if (!result.StabilityLevel.Equals(StabilityResultLevel.Exception) &&
							result.IsUserRelatedNotification == isUserNotification)
						{
							body.Append(result.StabilityLevelText).Append(": ").Append(result.Description).AppendLine()
								.AppendLine();
						}
					}

					emailDef.Body = body.ToString();

					Env.OutgoingMailManager.CreateAndSave(emailDef);
				}
			}
		}
	}
}
