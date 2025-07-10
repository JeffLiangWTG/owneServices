using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BatchNotification))]
	sealed class InheritedTest1 : NotificationTest<BatchNotification>
	{
		protected override BatchNotification NewTestNotification()
		{
			return new BatchNotification("Message");
		}
	}
}
