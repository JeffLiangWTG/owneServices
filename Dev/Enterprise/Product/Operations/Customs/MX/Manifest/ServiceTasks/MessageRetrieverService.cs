using System.Collections.Generic;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.Customs.MX.Manifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.MXR,
	ServiceTaskApplicationCodeList.Descriptions.MXR,
	MXMessageConstants.MessageServiceTaskCategory,
	typeof(MessageRetrieverService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Mexico,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.MXR,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.MXCustoms },
	ServiceTaskApplicationCodeList.Descriptions.MXR)]

namespace Enterprise.Customs.MX.Manifest.ServiceTasks
{
	public class MessageRetrieverService : Customs.ServiceTasks.BranchInterchangeProcessorService
	{
		protected override IEnumerable<string> ApplicationCodes => new string[] { ApplicationCodeList.Codes.MXCustoms };

		protected override BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor() => new MXCInboundInterchangeProcessor(ApplicationCodes);

		[HostedServiceRequirement]
		public static string IsRequired() => (MessageHostedServiceRequirement.CheckMXManifestEnabled());
	}
}
