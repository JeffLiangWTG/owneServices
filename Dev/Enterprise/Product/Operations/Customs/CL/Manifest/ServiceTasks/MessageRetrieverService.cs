using System.Collections.Generic;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Customs.CL.Manifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.CCR,
	ServiceTaskApplicationCodeList.Descriptions.CCR,
	CLMessageConstants.MessageServiceTaskCategory,
	typeof(MessageRetrieverService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Chile,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.CCR,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CLCustoms },
	ServiceTaskApplicationCodeList.Descriptions.CCR)]

namespace Enterprise.Customs.CL.Manifest.ServiceTasks
{
	public class MessageRetrieverService : Customs.ServiceTasks.BranchInterchangeProcessorService
	{
		protected override IEnumerable<string> ApplicationCodes => new string[] { ApplicationCodeList.Codes.CLCustoms };

		protected override BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor() => new CLInboundInterchangeProcessor(ApplicationCodes);

		[HostedServiceRequirement]
		public static string CheckCLCompanyHasSMSSetupExist() => (MessageHostedServiceRequirement.CheckCLSetupSMSMessageSendingConfig());
	}
}
