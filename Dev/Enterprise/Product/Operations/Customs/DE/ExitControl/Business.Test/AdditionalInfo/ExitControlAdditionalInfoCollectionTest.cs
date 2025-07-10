using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlAdditionalInfoCollection))]
	sealed class ExitControlAdditionalInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCountValidation()
		{
			var collection = (ISupportMaxCountValidation)GetCollectionToTest();
			CombineAssertions(() =>
			{
				var validator = collection.MaxCountValidator;
				var notification = validator.Notification;
				AssertEquals("MaxCount", 99, validator.MaxCount);
				AssertEquals("WarnAtHalfway", false, validator.WarnAtHalfway);
				AssertEquals("Notification Type", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
				AssertEquals("Notification Message", "Maximum number of Additional Information is 99.", notification.Message);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var consignment = exitHeader.CusExitConsignments.AddNew();
			return new ExitControlAdditionalInfoCollection(consignment);
		}
	}
}
