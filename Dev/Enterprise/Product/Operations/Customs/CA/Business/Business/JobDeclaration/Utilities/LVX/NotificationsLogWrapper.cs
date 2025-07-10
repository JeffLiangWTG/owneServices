using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business;

public class NotificationsLogWrapper : ILVXJobsConsolidateRunnerLog
{
	public NotificationsLogWrapper(INotifications notifications)
	{
		this.notifications = Argument.NotNull(notifications, "notifications");
	}
	readonly INotifications notifications;

	public void BumpSectionProgress()
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
	public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
	{
		var argsWithoutHyperLink = args?.Select(o => o is LogHyperlink ? ((LogHyperlink)o).Text : o)?.ToArray();
		if (errorLevel == OperationalActionLogErrorLevel.Error)
		{
			notifications.AddError(ZString.Format(format, argsWithoutHyperLink));
		}
		else if (errorLevel == OperationalActionLogErrorLevel.Informational)
		{
			notifications.Add(NotificationType.Information, ZString.Format(format, argsWithoutHyperLink));
		}
		else
		{
			notifications.AddWarning(ZString.Format(format, argsWithoutHyperLink));
		}
	}

	public void SetSectionProgressMax(int max)
	{
	}
}
