using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.FR.ServiceTasks.FRAutomationDeltaServiceTask.Code,
	Enterprise.Customs.FR.ServiceTasks.FRAutomationDeltaServiceTask.Description,
	"FRC",
	typeof(Enterprise.Customs.FR.ServiceTasks.FRAutomationDeltaServiceTask),
	MinimumPeriod = "15Minutes",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.France
	+ "," + Enterprise.Core.Constants.CountryCodes.FrenchGuyana
	+ "," + Enterprise.Core.Constants.CountryCodes.Guadeloupe
	+ "," + Enterprise.Core.Constants.CountryCodes.Martinique
	+ "," + Enterprise.Core.Constants.CountryCodes.Mayotte
	+ "," + Enterprise.Core.Constants.CountryCodes.Reunion
	+ "," + Enterprise.Core.Constants.CountryCodes.SaintMartin
	+ "," + Enterprise.Core.Constants.CountryCodes.SaintBarthelemy,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15Minutes"
	)]

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class FRAutomationDeltaServiceTask : CustomsServiceTask
	{
		public const string Code = "FRA";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public const string Description = "FR Customs Message Delta Automation";

		[HostedServiceRequirement]
		public static string CheckRecipientIDRegistrySetting() => HostedServiceRequirementAttribute.CheckValueIsNotNullOrEmptyString(FRCustomsDataRegistry.Instance.RecipientID);

		protected override void RunTaskCore(CancellationToken token)
		{
			var processors = GetSenders().ToArray();
			foreach (var processor in processors)
			{
				foreach (var company in FrenchCompanies)
				{
					token.ThrowIfCancellationRequested();

					RunTaskHandleEmailSendFailure(() =>
					{
						try
						{
							processor.Process(company);
						}
						catch (OnSavingCriticalCheckException ex)
						{
							ServiceLogger.GetTaskNotificationSubscriber().AddError(ex.Message);
						}
					});
				}
			}
		}

		public static GlbCompany[] FrenchCompanies
		{
			get
			{
				var frenchDepartmentsAndTerritories = Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction.ToHashSet();
				return GlbCompany.GetActiveCompanies(x => frenchDepartmentsAndTerritories.Contains(x.GC_RN_NKCountryCode));
			}
		}

		IEnumerable<FRAutoMessageSender> GetSenders()
		{
			yield return new FRAutoD2MMessageSender(new LoggerWrapper(ServiceLogger));
			yield return new DeltaIEAutoValidationMessageSender(new LoggerWrapper(ServiceLogger));
			yield return new DeltaGAutoValidationMessageSender(new LoggerWrapper(ServiceLogger));
			yield return new DeltaIEAutoDeadLineExtensionMessageSender(new LoggerWrapper(ServiceLogger));
			yield return new DeltaGAutoDeadLineExtensionMessageSender(new LoggerWrapper(ServiceLogger), OrgCusAccountDeltaGTypeList.Codes.G1);
			yield return new DeltaGAutoDeadLineExtensionMessageSender(new LoggerWrapper(ServiceLogger), OrgCusAccountDeltaGTypeList.Codes.G2);
		}
	}
}
