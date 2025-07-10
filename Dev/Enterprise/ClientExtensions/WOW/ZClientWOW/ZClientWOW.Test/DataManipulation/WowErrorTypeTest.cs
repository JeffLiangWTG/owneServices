using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WowErrorType))]
	class WowErrorTypeTest : NotificationSubscriberTypeTest<WowErrorType>
	{
		protected override WowErrorType NewNotificationType(string name)
		{
			return new WowErrorType(name);
		}

		protected override WowErrorType NewNotificationType(string name, string message)
		{
			return new WowErrorType(message);
		}
	}
}
