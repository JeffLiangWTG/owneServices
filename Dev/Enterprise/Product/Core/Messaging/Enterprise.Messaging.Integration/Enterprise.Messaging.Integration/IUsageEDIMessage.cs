using Newtonsoft.Json.Linq;

namespace Enterprise.Messaging.Integration
{
	public interface IUsageEDIMessage : IEDIMessage
	{
		JObject UsageProperties { get; set; }
	}
}
