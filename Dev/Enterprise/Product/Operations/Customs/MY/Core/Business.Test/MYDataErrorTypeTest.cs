using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(MYDataErrorType))]
	class MYDataErrorTypeTest : NotificationSubscriberTypeTest<MYDataErrorType>
	{
		protected override MYDataErrorType NewNotificationType(string name)
		{
			return new MYDataErrorType(name);
		}

		protected override MYDataErrorType NewNotificationType(string name, string message)
		{
			return new MYDataErrorType(message);
		}
	}
}
