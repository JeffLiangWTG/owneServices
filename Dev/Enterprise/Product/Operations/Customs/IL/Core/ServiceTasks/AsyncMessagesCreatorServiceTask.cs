using System;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Customs.IL.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AsyncMessagesCreatorServiceTask.Code,
	AsyncMessagesCreatorServiceTask.FriendlyName,
	MessagingServiceTask.MessageServiceTaskCategory,
	typeof(AsyncMessagesCreatorServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Israel,
	MinimumPeriod = "10minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "10minute"
	)]
namespace Enterprise.Customs.IL.ServiceTasks
{
	public class AsyncMessagesCreatorServiceTask : MessagingServiceTask
	{
		public const string Code = "ILY";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string FriendlyName = "IL Async Messages Creator";

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var company in GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Israel))
			{
				token.ThrowIfCancellationRequested();

				using (DisposableEnvironment.ForCompany(company.GC_Code))
				{
					var enablePullASyncMessage = ILCustomsDataRegistry.Instance.EnablePullASyncMessage.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (!enablePullASyncMessage)
					{
						ServiceLogger.Log(Integration.LogType.Information, $"Company {company.GC_Code} is not enabled for Pull A-Sync Message");
						continue;
					}

					IMessageBuilder messageBuilder = new ILGEN910MessageBuilder(company, new LoggerWrapper(ServiceLogger));
					messageBuilder.PopulateMessages();
					company.Factory.Save();
				}
			}
		}

		public BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;
	}
}
