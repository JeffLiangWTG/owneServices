using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridContextMenuHelperTest : TestCaseWithFactory
	{
		public void TestGetDeleteMenuItemText()
		{
			var dummyBizObj = Factory.New<DummyCancellable>();

			dummyBizObj.IsCancelled = false;
			AssertEquals(
				ZFilterGridModule.DeleteButtonCaptions.Deactivate,
				ZGridContextMenuHelper.Instance.GetDeleteMenuItemTextForCancellableBizo(dummyBizObj)
			);

			dummyBizObj.IsCancelled = true;
			AssertEquals(
				ZFilterGridModule.DeleteButtonCaptions.Activate,
				ZGridContextMenuHelper.Instance.GetDeleteMenuItemTextForCancellableBizo(dummyBizObj)
			);
		}

		public void TestGetDeleteMenuItemText_TemplateRecord()
		{
			var templateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
			var templateRecord = Factory.New<DummyTemplateRecord>();
			templateRecordProvider.TemplateRecord = templateRecord;

			Assert(
				"If there is a template record, it should use the template record cancelled value, not provider",
				!templateRecordProvider.IsCancelled
			);

			templateRecord.IsCancelled = false;
			AssertEquals(
				ZFilterGridModule.DeleteButtonCaptions.Deactivate,
				ZGridContextMenuHelper.Instance.GetDeleteMenuItemTextForCancellableBizo(templateRecordProvider)
			);

			templateRecord.IsCancelled = true;
			AssertEquals(
				ZFilterGridModule.DeleteButtonCaptions.Activate,
				ZGridContextMenuHelper.Instance.GetDeleteMenuItemTextForCancellableBizo(templateRecordProvider)
			);
		}
	}
}
