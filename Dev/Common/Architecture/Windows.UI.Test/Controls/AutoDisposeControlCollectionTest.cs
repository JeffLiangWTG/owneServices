using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class AutoDisposeControlCollectionTest : TestCase
	{
		public void TestControlsAreDisposed()
		{
			TextBox control = new TextBox();
			using (Control owner = new Control())
			{
				AutoDisposeControlCollection collection = new AutoDisposeControlCollection(owner);

				collection.Add(control);
				collection.Remove(control);

				AssertEquals("Control in collection should not yet be disposed", false, control.IsDisposed);
			}
			AssertEquals("Control in collection should be disposed now the owner is disposed", true, control.IsDisposed);
		}

		public void TestControlNotDisposedIfAttachedToNewParent()
		{
			using (KUserControl container1 = new KUserControl())
			using (KUserControl container2 = new KUserControl())
			using (TextBox textBox = new TextBox())
			{
				container1.Controls.Add(textBox);
				container1.Controls.Remove(textBox);
				container2.Controls.Add(textBox);
				container1.Dispose();
				AssertEquals("textBox not disposed because it has new parent", false, textBox.IsDisposed);
				AssertEquals("textBox not disposed because it has new parent", container2, textBox.Parent);
			}
		}

		public void TestCollectionDoesNotCauseLeaks()
		{
			using (var container = new KUserControl())
			{
				var weakRef = AddControlToContainerThenRemoveAndDispose(container);

				GC.Collect();
				GC.WaitForFullGCComplete();

				AssertNull("Control has been removed from its parent before disposing. This shouldn't cause a memory leak.", weakRef.Target);
			}
		}

		static WeakReference AddControlToContainerThenRemoveAndDispose(Control container)
		{
			var textBox = new TextBox();
			container.Controls.Add(textBox);
			container.Controls.Remove(textBox);

			textBox.Dispose();

			return new WeakReference(textBox);
		}
	}
}
