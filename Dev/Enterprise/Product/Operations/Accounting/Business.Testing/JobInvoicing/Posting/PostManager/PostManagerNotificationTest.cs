using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class PostManagerNotificationTest : TestCase
	{
		public void TestConstructorSetsTheProperties()
		{
			var notification = new PostManagerNotification(CargoWise.EntityFramework.NotificationType.Error, "Message", PostManagerValidationType.GSTApplicabilityValidation);

			AssertEquals("ValidationType property set correctly", notification.ValidationType, PostManagerValidationType.GSTApplicabilityValidation);
			AssertEquals("Message property set correctly", notification.Message, "Message");
			AssertEquals("Type property set correctly", notification.Type, CargoWise.EntityFramework.NotificationType.Error);
		}
	}
}