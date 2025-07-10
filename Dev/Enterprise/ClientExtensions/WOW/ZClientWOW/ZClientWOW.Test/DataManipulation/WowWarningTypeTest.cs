using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowWarningType))]
	class WowWarningTypeTest : NotificationSubscriberTypeTest<WowWarningType>
	{
		protected override WowWarningType NewNotificationType(string name)
		{
			return new WowWarningType(name);
		}

		protected override WowWarningType NewNotificationType(string name, string message)
		{
			return new WowWarningType(message);
		}
	}
}
