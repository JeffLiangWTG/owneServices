using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZFormActivityLoggingStrategy
	{
		public static void AddAdornments(Form form)
		{
			if (!DesignModeFinder.IsDesigning &&
				!Db.DatabaseUpgradedExceptionHasBeenThrownInConnection &&
				(Env.CurrentUser?.ActivityTrackingStatus == ActivityTrackingStatus.Yes ||
				(Env.CurrentUser?.ActivityTrackingStatus == ActivityTrackingStatus.BasedOnCompany && EnvProxy.Instance.CurrentCompany != null && EnvProxy.Instance.Registry.UserEventTrackingEnterprise)))
			{
				var formStats = UserEventTracker.Instance.GetFormStatsForForm(form);
				ZFormActivityLogger.Instance.StatLogs.Add(formStats);

				var zform = form as IZForm;
				var bform = form as IBusinessForm;
				var dataForm = form as IDataBoundControl;

				form.Shown +=
					delegate
					{
						if (!form.IsDesignMode() && formStats != null)
						{
							formStats.NotifyFormShownUtc(
								bform != null && !string.IsNullOrEmpty(bform.FormCaption) ? bform.FormCaption : form.Text,
								zform != null && zform.ControllerID != null ? zform.ControllerID.Name : "",
								ZDateTime.UtcNow.ToDateTime());
						}
					};

				form.Closed +=
					delegate
					{
						NotifyFormClosed(form.IsDesignMode(), formStats, (bform != null ? dataForm.DataSource as BusinessObject : null), zform);
					};

				form.Disposed +=
					delegate
					{
						NotifyFormClosed(form.IsDesignMode(), formStats, (bform != null ? dataForm.DataSource as BusinessObject : null), zform);
					};

				form.Deactivate +=
					delegate
					{
						if (!form.IsDesignMode() && formStats != null)
						{
							formStats.NotifyFormDeactivate();
						}
					};

				form.Activated +=
					delegate
					{
						if (!form.IsDesignMode() && formStats != null)
						{
							formStats.NotifyFormActivate();
						}
					};
			}
		}

		static void NotifyFormClosed(bool isDesignMode, FormUserStatistics formStats, BusinessObject bizo, IZForm zForm)
		{
			if (!zForm.IsActivityLogFinished)
			{
				zForm.IsActivityLogFinished = true;
				if (!isDesignMode && formStats != null)
				{
					if (bizo != null && bizo.IsInDatabase)
					{
						formStats.NotifyFormClosed(bizo.PK.ToGuid(), ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(bizo.TableName));
					}
					else
					{
						formStats.NotifyFormClosed(Guid.Empty, "");
					}
				}
			}
		}
	}
}
