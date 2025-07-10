using System;
using System.Linq;
using Enterprise.Messaging.Integration;
using Newtonsoft.Json.Linq;

namespace Enterprise.Billing.Business.Testing
{
	public static class UsageCollectorExtensions
	{
		public static T GetProperty<T>(this IUsageEDIMessage message, string name) => GetProperty(message, name).ToObject<T>();

		public static JToken GetProperty(this IUsageEDIMessage message, string name)
		{
			var property = message.UsageProperties.Properties().First(kp => kp.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Value;
			return property;
		}
	}
}
