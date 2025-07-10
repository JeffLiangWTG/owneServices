using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitNotificationMessageSendingActionParent))]
	class ExitNotificationMessageSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingOjectCollection()
		{
			exitHeader.CusExitDetails.AddNew();
			exitHeader.CusExitDetails.AddNew();

			var parent = (ExitNotificationMessageSendingActionParent)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertType<ExitNotificationMessageSendingActionCollection>("Type", parent.SendingObjectsCollection);
				AssertEquals("Count", 2, parent.SendingObjectsCollection.Count);

				parent.SendingObjectsCollection[0].ShouldSend = true;
				parent.SendingObjectsCollection[1].ShouldSend = true;
				Assert("Allow multilpe objects", !parent.HasRowErrors);
			});
		}

		public void TestMessageSendingObjectProperties()
		{
			var parent = (ExitNotificationMessageSendingActionParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;
			CombineAssertions(() =>
			{
				AssertEquals("MRN caption", "MRN", properties.ElementAt(0).ResourceString.Caption);
				AssertEquals("Reference Number caption", "Reference Number", properties.ElementAt(1).ResourceString.Caption);
				AssertEquals("Status caption", "Status", properties.ElementAt(2).ResourceString.Caption);
				AssertEquals("Status Description caption", "Status Description", properties.ElementAt(3).ResourceString.Caption);
				AssertEquals("Message Type caption", "Message Type", properties.ElementAt(4).ResourceString.Caption);

				AssertEquals("MRN width", 119, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Reference Number width", 114, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Status width", 53, properties.ElementAt(2).ColumnWidth);
				AssertEquals("Status Description width", 140, properties.ElementAt(3).ColumnWidth);
				AssertEquals("Message Type width", 92, properties.ElementAt(4).ColumnWidth);

				Assert("All properties mandatory", properties.All(x => x.IsMandatory));
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new ExitNotificationMessageSendingActionParent(exitHeader);

		protected override void SetUp()
		{
			base.SetUp();
			exitHeader = Factory.New<CusExitControlHeader>();
		}
		CusExitControlHeader exitHeader;
	}
}
