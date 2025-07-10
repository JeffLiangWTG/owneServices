using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IRelatedModuleFilterBusinessObject : IFilterStripBusinessObject
	{
		bool IsInFilterRuleMode { get; set; }
		ModuleFilterComparisonStrategy ModuleFilterComparisonStrategy { get; set; }
	}

	public static class IRelatedModuleFilterBusinessObjectExtensionMethods
	{
		public static ZQuery GetFilter(this IRelatedModuleFilterBusinessObject filterBusinessObject, StmModuleFilter filter, bool disableValidation = false, Action<IRelatedModuleFilterBusinessObject> actionToDoBeforeCreatingZQuery = null)
		{
			if (filterBusinessObject.LoadFilterRuleLayout(filter, disableValidation: disableValidation))
			{
				actionToDoBeforeCreatingZQuery?.Invoke(filterBusinessObject);

				return filterBusinessObject.Filter;
			}

			return null;
		}

		public static bool LoadFilterRuleLayout(this IRelatedModuleFilterBusinessObject filterBusinessObject, StmModuleFilter layout, bool disableValidation = false)
		{
			if (layout == null || layout.IsDeleted)
			{
				return false;
			}

			if (layout.S9_FilterType == StmModuleFilterTypes.Codes.FilterRule)
			{
				filterBusinessObject.LayoutsHelper = new EmptyLayoutsHelper();
			}

			filterBusinessObject.ModuleFilterComparisonStrategy = layout.ModuleFilterComparisonStrategy;

			filterBusinessObject.AddAuditFiltersIfRequired();
			return filterBusinessObject.LoadLayout(layout, disableValidation: disableValidation);
		}

		public static void ValidateFilterStrips(this IRelatedModuleFilterBusinessObject filterBusinessObject, StmModuleFilter filter)
		{
			if (filterBusinessObject.LoadFilterRuleLayout(filter))
			{
				((FilterBusinessObject)filterBusinessObject).RunPreSaveValidation();

				var startingError = Res.GetString("7956e319-48ab-4979-a064-5d93921cf2af", "Filter has errors.");

				filter.ClearRowNotificationsContaining(startingError);

				if (filterBusinessObject.ActiveModuleFiltersForValidation.Any(m => m.HasErrors))
				{
					var message = new StringBuilder(startingError);

					foreach (var errorInfo in filterBusinessObject.ActiveModuleFiltersForValidation.SelectMany(t => t.PropertiesWithNotifications))
					{
						foreach (var error in errorInfo.Notifications.Where(n => n.Type == NotificationType.Error))
						{
							message.AppendLine();
							message.Append(error.Message);
						}
					}

					filter.AddRowError(message.ToString());
				}
			}
		}
	}
}
