using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.AR.Manifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.ARP,
	ServiceTaskApplicationCodeList.Descriptions.ARP,
	ARMessageConstants.MessageServiceTaskCategory,
	typeof(MessageProcessorService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Argentina,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.ARP,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ARCustoms
	},
	ServiceTaskApplicationCodeList.Descriptions.ARP)]

namespace Enterprise.Customs.AR.Manifest.ServiceTasks
{
	public class MessageProcessorService : Customs.ServiceTasks.BranchMessageProcessorService
	{
		protected override IEnumerable<ZString> MessageTypes => new ZString[] { Business.MessageTypes.Codes.ARB, Business.MessageTypes.Codes.ARE };

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.ARCustoms };

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new ARBranchMessageProcessor();

		[HostedServiceRequirement]
		public static string CheckARCompanyHasCredentialsExist() => (MessageHostedServiceRequirement.CheckARCompanyHasCertificate());
	}
}
