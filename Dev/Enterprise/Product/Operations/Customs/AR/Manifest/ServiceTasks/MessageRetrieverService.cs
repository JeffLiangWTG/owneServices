using System.Collections.Generic;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.AR.Manifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.ARR,
	ServiceTaskApplicationCodeList.Descriptions.ARR,
	ARMessageConstants.MessageServiceTaskCategory,
	typeof(MessageRetrieverService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Argentina,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.ARR,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.ARCustoms },
		ServiceTaskApplicationCodeList.Descriptions.ARR)]

namespace Enterprise.Customs.AR.Manifest.ServiceTasks
{
	public class MessageRetrieverService : Customs.ServiceTasks.BranchInterchangeProcessorService
	{
		protected override IEnumerable<string> ApplicationCodes => new string[] { ApplicationCodeList.Codes.ARCustoms };

		protected override BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor() => new ARInboundInterchangeProcessor(ApplicationCodes);

		[HostedServiceRequirement]
		public static string CheckARCompanyHasCredentialsExist() => (MessageHostedServiceRequirement.CheckARCompanyHasCertificate());
	}
}
