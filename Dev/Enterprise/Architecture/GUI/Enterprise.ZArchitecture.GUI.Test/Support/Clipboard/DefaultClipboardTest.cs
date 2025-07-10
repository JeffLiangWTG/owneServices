using System;
using System.Windows.Forms;
using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DefaultClipboardTest : TestCase
	{
		public void TestObjectFactoryGetDefaultClipboardInstance()
		{
			AssertType<DefaultClipboard>(ObjectFactory.Get<IClipboard>());
		}

		[DeveloperOnlyTest]
		public void TestGetDataObject()
		{
			IDataObject dataObject = new DataObject("copy me");
			var defaultClipboard = new DefaultClipboard();
			var action = new Action(() => defaultClipboard.SetDataObject(dataObject));
			action.Invoke();

			var data = ClipboardTestHelper.RetryIfCopyOrCutFailed<IDataObject>(action, retry: 20);
			AssertEquals("copy me", data.GetData(typeof(string)));
		}

		[DeveloperOnlyTest]
		public void TestSetDataObject()
		{
			IDataObject dataObject = new DataObject("copy me");
			var successfulSet = false;
			var action = new Action(() =>
			{
				successfulSet = SafeClipboard.SetDataObject(dataObject);
			});
			action.Invoke();

			var i = 0;
			while (!successfulSet && i < 10)
			{
				action.Invoke();
				i++;
			}

			Assert("SetDataObject should return true on success.", successfulSet);
			AssertEquals("copy me", new DefaultClipboard().GetDataObject().GetData(typeof(string)));
		}
	}
}
