using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Customs.CL.Manifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.CLP,
	ServiceTaskApplicationCodeList.Descriptions.CLP,
	CLMessageConstants.MessageServiceTaskCategory,
	typeof(MessageProcessorService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Chile,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.CLP,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CLCustoms,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	ServiceTaskApplicationCodeList.Descriptions.CLP)]

namespace Enterprise.Customs.CL.Manifest.ServiceTasks
{
	public class MessageProcessorService : Customs.ServiceTasks.BranchMessageProcessorService
	{
		protected override IEnumerable<ZString> MessageTypes => new ZString[] { Business.MessageTypes.Codes.CHA, Business.MessageTypes.Codes.CHB, Business.MessageTypes.Codes.CHC, Business.MessageTypes.Codes.CHD, Business.MessageTypes.Codes.CHE, Business.MessageTypes.Codes.CHF };

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.CLCustoms };

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new CLBranchMessageProcessor();

		[HostedServiceRequirement]
		public static string CheckCLCompanyHasSMSSetupExist() => (MessageHostedServiceRequirement.CheckCLSetupSMSMessageSendingConfig());
	}
}
