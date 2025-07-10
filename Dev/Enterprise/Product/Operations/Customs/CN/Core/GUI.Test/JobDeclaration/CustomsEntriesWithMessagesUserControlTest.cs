using System;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CustomsEntriesWithMessagesUserControlTest : TestCase
	{
		public void TestMessageUserControl()
		{
			using var userControl = new CustomsEntriesWithMessagesUserControl();
			using var control = (Control)typeof(CustomsEntriesWithMessagesUserControl).GetMethod("GetMessageUserControl", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(userControl, Array.Empty<object>());
			AssertEquals(typeof(CustomsEntriesAndEntryLinesUserControl), control.GetType());
		}
	}
}
