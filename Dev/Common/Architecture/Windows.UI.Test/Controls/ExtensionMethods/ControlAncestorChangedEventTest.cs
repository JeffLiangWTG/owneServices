using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlAncestorChangedEventTest : TestCase
	{
		public void TestAncestorChanged()
		{
			bool ancestorChangedFired = false;
			EventHandler handler = delegate
			{ ancestorChangedFired = true; };
			TextBox.AddAncestorChanged(handler);

			UserControl1.Controls.Add(TextBox);
			AssertEquals("When ParentChanged is fired", true, ancestorChangedFired);
			ancestorChangedFired = false;

			UserControl2.Controls.Add(UserControl1);
			AssertEquals("When Parent.ParentChanged is fired", true, ancestorChangedFired);
			ancestorChangedFired = false;
		}

		public void TestNoMemoryLeak()
		{
			TextBox.AddAncestorChanged(delegate
			{ });
			textBox = null;
			WeakReference textBoxRef = new WeakReference(null);

			GC.Collect();
			GC.WaitForPendingFinalizers(); // required to test for memory leak
			GC.Collect();
			AssertEquals("TextBox collected", false, textBoxRef.IsAlive);
		}

		#region Implementation

		TextBox TextBox
		{
			get { return textBox ?? (textBox = new TextBox()); }
		}
		TextBox textBox;

		UserControl UserControl1
		{
			get { return userControl1 ?? (userControl1 = new UserControl()); }
		}
		UserControl userControl1;

		UserControl UserControl2
		{
			get { return userControl2 ?? (userControl2 = new UserControl()); }
		}
		UserControl userControl2;

		#endregion
	}
}
