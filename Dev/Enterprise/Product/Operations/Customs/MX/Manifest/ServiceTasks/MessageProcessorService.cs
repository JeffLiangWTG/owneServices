using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.Customs.MX.Manifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.MXP,
	ServiceTaskApplicationCodeList.Descriptions.MXP,
	MXMessageConstants.MessageServiceTaskCategory,
	typeof(MessageProcessorService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Mexico,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.MXP,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.MXCustoms,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	ServiceTaskApplicationCodeList.Descriptions.MXP)]

namespace Enterprise.Customs.MX.Manifest.ServiceTasks
{
	public class MessageProcessorService : Customs.ServiceTasks.BranchMessageProcessorService
	{
		protected override IEnumerable<ZString> MessageTypes => new ZString[] { Business.MessageTypes.Codes.MXA, Business.MessageTypes.Codes.MXD, Business.MessageTypes.Codes.MXF, Business.MessageTypes.Codes.MXG, MXMessageConstants.XER };

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.MXCustoms };

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new MXBranchMessageProcessor();

		[HostedServiceRequirement]
		public static string IsRequired() => (MessageHostedServiceRequirement.CheckMXManifestEnabled());
	}
}
