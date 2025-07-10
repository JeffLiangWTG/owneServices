using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BatchNotification))]
	sealed class InheritedTest2 : NotificationTest<BatchNotification>
	{
		protected override BatchNotification NewTestNotification()
		{
			return new BatchNotification(ErrorType.EmailNotifyGroupNotExist, "Message");
		}
	}
}
