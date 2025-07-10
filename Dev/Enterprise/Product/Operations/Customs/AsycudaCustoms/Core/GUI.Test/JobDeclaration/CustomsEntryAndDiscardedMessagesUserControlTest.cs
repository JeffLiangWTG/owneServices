using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	public class CustomsEntryAndDiscardedMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestMessageUserControl()
		{
			using (CustomsEntryAndDiscardedMessagesUserControlTestHelper mainControl = new CustomsEntryAndDiscardedMessagesUserControlTestHelper())
			using (IDisposable messageControl = mainControl.GetMessageUserControl())
			{
				AssertEquals(typeof(MessageUserControl), messageControl.GetType());
			}
		}

		public class CustomsEntryAndDiscardedMessagesUserControlTestHelper : CustomsEntryAndDiscardedMessagesUserControl
		{
			public new Customs.GUI.BaseCustomsEntryUserControl GetMessageUserControl() => base.GetMessageUserControl();
		}
	}
}
