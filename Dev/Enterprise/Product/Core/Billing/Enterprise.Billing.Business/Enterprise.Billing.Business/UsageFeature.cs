using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Newtonsoft.Json.Linq;

namespace Enterprise.Billing.Business
{
	internal class UsageFeature : BaseUsageFeature
	{
		public UsageFeature(UsageFeatureProperties featureProperties) : base(featureProperties)
		{
		}

		public override void Report(JObject allProperties, GlbBranch currentBranch, BusinessObjectFactory factory)
		{
			var message = factory.New<IUsageEDIMessage>();
			message.UsageProperties = allProperties;
			message.EM_ApplicationCode = FeatureProperties.ProduceDailySummaries ? ApplicationCodeList.Codes.UsageDataToSummarise : ApplicationCodeList.Codes.UsageData;
			message.EM_MessageType = FeatureProperties.ProduceDailySummaries ? EDIMessageTypeList.Codes.UsageDataToSummarise : EDIMessageTypeList.Codes.UsageData;
			message.EM_GB = (currentBranch != null) ? currentBranch.PK : ZGuid.Empty;
		}
	}
}
