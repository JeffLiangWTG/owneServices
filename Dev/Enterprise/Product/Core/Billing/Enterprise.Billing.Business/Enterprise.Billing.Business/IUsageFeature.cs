using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json.Linq;

namespace Enterprise.Billing.Business
{
	public struct UsageFeatureProperties
	{
		public string Code { get; set; }
		public string Module { get; set; }
		public string Description { get; set; }
		public bool ProduceDailySummaries { get; set; }

		public UsageFeatureProperties(string code, string module, string description, bool produceDailySummaries = false)
		{
			Code = code;
			Module = module;
			Description = description;
			ProduceDailySummaries = produceDailySummaries;
		}
	}

	public interface IUsageFeature
	{
		UsageFeatureProperties FeatureProperties { get; }

		void Report(JObject allProperties, GlbBranch currentBranch, BusinessObjectFactory factory);
	}

	internal abstract class BaseUsageFeature : IUsageFeature
	{
		public BaseUsageFeature(UsageFeatureProperties featureProperties)
		{
			FeatureProperties = featureProperties;
		}

		public UsageFeatureProperties FeatureProperties { get; private set; }

		public abstract void Report(JObject allProperties, GlbBranch currentBranch, BusinessObjectFactory factory);
	}
}
