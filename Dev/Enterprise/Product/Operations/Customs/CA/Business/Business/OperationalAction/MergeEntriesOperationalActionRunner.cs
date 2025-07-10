using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CA.Business.OperationalAction
{
	public class MergeEntriesOperationalActionRunner
	{
		public MergeEntriesOperationalActionRunner(OperationalActionLogAndUserNotificationWrapper logandNotificationWrapper)
		{
			this.logAndNotificationWrapper = logandNotificationWrapper;
		}
		readonly OperationalActionLogAndUserNotificationWrapper logAndNotificationWrapper;

		public void PerformFunctionOperationalAction(JobDeclaration[] targets)
		{
			var result = new List<ZGuid>();
			if (targets.Length == 0)
			{
				logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Error, Constants.NoDeclarationToMerge);
			}
			else if (targets.Length > 50)
			{
				logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Error, Constants.TooManyDeclarationToMerge);
			}
			else
			{
				logAndNotificationWrapper.SetSectionProgressMax(targets.Length);
				RunOperationalActionMergeEntries(targets);
			}
		}

		void RunOperationalActionMergeEntries(JobDeclaration[] declarations)
		{
			foreach (var declaration in declarations)
			{
				logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Informational, Constants.Seperator);
				var messageCollector = new SendsMessagesToCustomsShutterUpperer(false);
				if (declaration.DoMerge(messageCollector))
				{
					logAndNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Constants.SuccessfullyMergedDeclaration + Constants.NotifyFormatParaHolder, declaration.GetDeclarationIdLink());
				}
				else
				{
					logAndNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Warning, Constants.CannotMergeDeclaration + Constants.NotifyFormatParaHolder + Constants.MergeFailedReason + messageCollector.InvalidOperationText, declaration.GetDeclarationIdLink());
				}
				logAndNotificationWrapper.BumpSectionProgress();
			}
		}
	}
}
