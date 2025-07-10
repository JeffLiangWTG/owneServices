using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using WiseTech.Business.Core;

namespace Enterprise.BufferManagement.Service;

public static class TaskUtil
{
	internal static bool ValidateTask(ProcessTask task, out IReadOnlyCollection<string> errors, out List<BusinessMessage> secondaryBusinessMessages)
	{
		secondaryBusinessMessages = null;

		if (!ValidateTask(task, out errors))
		{
			secondaryBusinessMessages = [.. errors.Select(error => BusinessMessage.BuildError(
				englishText: error,
				Guid.NewGuid(),
				canonicalPrefix: $"{WiseTech.Business.Taxonomy.TaxonomyCommon.WiseTechCanonicalPrefix}.workflows.tasks",
				canonicalSuffix: "validation-error"
				) )];
			return false;
		}

		return true;
	}

	internal static bool ValidateTask(ProcessTask task, out IReadOnlyCollection<string> errors)
	{
		errors = null;
		task.Validation.ValidateAll();

		if (!task.HasErrors)
		{
			return true;
		}

		errors = task.PropertiesWithNotifications
			.SelectMany(propertyInfo => propertyInfo.Notifications.Where(notification => notification.Type == NotificationType.Error))
			.Distinct()
			.Select(notification => notification.Message)
			.ToArray();

		return false;
	}
}
