using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class BusinessObjectMultipleDeleterGUITest : TestCaseWithFactory
	{
		public void TestObjectsCorrectlyCaclulated()
		{
			var obj1 = Factory.New<DummyCancellable>();
			var obj2 = Factory.New<DummyBusinessObject>();
			var obj3 = Factory.New<DummyCancellable>();
			var obj4 = Factory.New<DummyBusinessObject>();
			var obj5 = Factory.New<DummyBusinessObject>();
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			var deleter = new BusinessObjectMultipleDeleterGUI(new BusinessObject[] { obj1, obj2, obj3, obj4, obj5 });
			deleter.Process(BusinessObjectMultipleDeleterAction.Delete);
			AssertMultilineASCIIEquals("There should be 2 cancellable and 3 deletable objects", "The selected records will be deleted:\r\nDummyBizo\r\nDummyBizo\r\nDummyBizo", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			deleter.Process(BusinessObjectMultipleDeleterAction.Deactivate);
			AssertMultilineASCIIEquals("There should be 2 cancellable and 3 deletable objects", "If some objects are already inactive, no action will be performed on them." + "\r\nThese objects will be deactivated:\r\nDummyBizoCancellable\r\nDummyBizoCancellable", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}