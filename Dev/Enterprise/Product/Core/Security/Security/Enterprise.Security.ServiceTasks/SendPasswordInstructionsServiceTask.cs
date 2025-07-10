using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Security.ServiceTasks.SendPasswordInstructionsServiceTask.Code,
	Enterprise.Security.ServiceTasks.SendPasswordInstructionsServiceTask.Description,
	"MAI",
	typeof(Enterprise.Security.ServiceTasks.SendPasswordInstructionsServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "10minutes",
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Thursday },
	ActiveByDefault = true
	)]

namespace Enterprise.Security.ServiceTasks
{
	public class SendPasswordInstructionsServiceTask : ServiceProviderImpl
	{
		public const string Code = "SPI";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string Description = "Send Password Instructions";

		bool CanRunServiceTask => ((ZDateTime)WebDataRegistry.Instance.ContactCreatedStartFromDate.Value).IsValid;

		public override void RunTask(CancellationToken token)
		{
			if (!CanRunServiceTask)
			{
				ServiceLogger.Information((NoResString)"Registry item is not enabled, can not perform this serviceTask");
				return;
			}
			ServiceLogger.Information(Description + " started.");
			var contactCreatedStartFromDate = (ZDateTime)WebDataRegistry.Instance.ContactCreatedStartFromDate.Value;
			var factory = new BusinessObjectFactory();
			var zQuery = new ZQuery(OrgContactSchema.OC_IsActive, SQLComparisonOperator.Equal, true);
			zQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, SQLComparisonOperator.Equal, true);
			zQuery.AddToFilter(OrgContactSchema.OC_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, contactCreatedStartFromDate);
			var contacts = factory.Load<OrgContact>(zQuery);
			foreach (var contact in contacts)
			{
				var passwordLog = contact.GetPasswordChangedOrSentLog();
				if (passwordLog == null)
				{
					try
					{
						var success = PasswordInstructionEmailSender.SendPasswordInstructionEmail(contact, PasswordInstructionType.Set, CreatePasswordResetInfo(contact), PasswordInstructionUrlType.Default, useCurrentUserInfo: false);
						if (success)
						{
							ServiceLogger.Information($"Password instructions sent to {contact.Email} successfully");
						}
						else
						{
							ServiceLogger.Warning($"Password instructions sent to {contact.Email} failed");
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var errorMessage = FormattableString.Invariant($"Error sending Password instructions");
						ErrorReporter.ReportOnce(errorMessage, ex);
						ServiceLogger.Error(errorMessage, ex);
					}
				}
			}
			ServiceLogger.Information(Description + " finished.");
		}

		PasswordResetInfo CreatePasswordResetInfo(OrgContact contact)
		{
			return new PasswordResetInfo()
			{
				ContactEmail = contact.Email,
				OrgCode = contact.OrgCode,
				EmailTemplateCompanyPk = GlbCompany.CurrentCompany.PK.ToString()
			};
		}
	}
}
