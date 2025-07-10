using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FieldPseudoApplicator : PseudoApplicator
	{
		public FieldPseudoApplicator(OperationalActionRunner runner)
			: base(runner, Res.GetString("OperationalActionRunner|Name", "Fields")) { }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var pairs = new List<IOperationalActionFieldValuePair>(
				Runner.Fields.OfType<RunnerField>().Where(field => field.ShouldApply));

			IEnumerable<BusinessObject> affectedTargets = targets;

			if (pairs.Count > 0)
			{
				var root = new RootUpdateNode(Context.Supporter.RootType, log);
				root.AddRange(pairs);
				affectedTargets = root.Apply(targets);
			}

			var hasErrorsReported = false;
			foreach (var businessObject in affectedTargets)
			{
				var errorMessage = string.Empty;
				if (businessObject.Notifications.HasErrors()) // Use BusinessObject.Notifications to skip children notifications
				{
					errorMessage = businessObject.Notifications.GetErrors().ToMessageListString() + "\r\n"; // Use BusinessObject.Notifications to skip children notifications
					hasErrorsReported = true;
				}

				var customBizo = (businessObject as ICustomFieldProvider)?.GetCustomBusinessObject();
				if (customBizo != null && customBizo.Notifications.HasErrors())
				{
					errorMessage += ToErrorListString(customBizo.Notifications.GetErrors()) + "\r\n";
					hasErrorsReported = true;
				}

				if (!string.IsNullOrEmpty(errorMessage))
				{
					log.Notify(OperationalActionLogErrorLevel.Error,
						Res.GetString("FieldPseudoApplicator|ValidationErrorsFound", "{0} has the following errors:\r\n{1}",
							businessObject.HumanReadableName, errorMessage));
				}
			}

			string ToErrorListString(IEnumerable<INotification> errors)
			{
				var result = string.Empty;
				foreach (var error in errors)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += "\r\n";
					}

					var message = error.Message;
					if (error is PropertyNotification notification && !string.IsNullOrEmpty(notification.PropertyName))
					{
						var customFieldName = notification.PropertyName.Split(new string[] { "__" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
						message = message.Replace(notification.PropertyName, $"Custom field '{customFieldName}'");
					}
					result += message;
				}
				return result;
			}

			if (!hasErrorsReported)
			{
				// Double-check for errors on roots and child elements (they might have not been changed but affected)
				foreach (var businessObject in targets.Where(bizo => bizo.HasErrors()))
				{
					log.Notify(OperationalActionLogErrorLevel.Error,
						Res.GetString("FieldPseudoApplicator|ValidationErrorsFound", "{0} has the following errors:\r\n{1}",
							businessObject.HumanReadableName, businessObject.GetErrors().ToMessageListString()));
				}
			}
		}
	}
}
