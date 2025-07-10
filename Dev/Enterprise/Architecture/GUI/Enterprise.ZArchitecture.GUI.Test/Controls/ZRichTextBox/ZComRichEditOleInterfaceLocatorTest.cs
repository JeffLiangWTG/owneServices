using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	sealed class ZComRichEditOleInterfaceLocatorTest : TestCaseWithFactory
	{
#if !WINZOR
		public void TestRichEditOleLocated()
		{
			using (var box = new RichTextBox())
			{
				box.CreateControl();
				UnsafeNativeMethods.IRichEditOle richEditOle;
				using (var locator = ZComRichEditOleInterfaceLocator.GetInstance(box))
				{
					richEditOle = locator.RichEditOle;
					AssertNotNull("Should locate the interface successfully", richEditOle);
				}
			}
		}

		public void TestReferenceCounting()
		{
			using (var box = new RichTextBox())
			{
				box.CreateControl();

				TestComRichEditOleInterfaceLocator locator;
				using (locator = TestComRichEditOleInterfaceLocator.GetInstance(box))
				{
					AssertEquals("Should have a reference count of 1 initially", 1, locator.fReferenceCount);
					using (locator = TestComRichEditOleInterfaceLocator.GetInstance(box))
					{
						AssertEquals("Should have a reference count of 2 after second get", 2, locator.fReferenceCount);
					}
					AssertEquals("Should have a reference count of 1 after first dispose", 1, locator.fReferenceCount);
				}
				AssertEquals("Should have a reference count of 0 after second dispose", 0, locator.fReferenceCount);
			}
		}

		public void TestDoesntCauseMemoryLeak()
		{
			WeakReference richEditRef, locatorRef;
			DoTestDoesntCauseMemoryLeak(out richEditRef, out locatorRef);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertEquals("RichTextBox should be collected", false, richEditRef.IsAlive);
			AssertEquals("ZComRichEditOleInterfaceLocator should be collected", false, locatorRef.IsAlive);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		void DoTestDoesntCauseMemoryLeak(out WeakReference richEditRef, out WeakReference locatorRef)
		{
			var richEdit = new RichTextBox();
			ZComRichEditOleInterfaceLocator locator;
			using (locator = ZComRichEditOleInterfaceLocator.GetInstance(richEdit))
			{
			}

			richEditRef = new WeakReference(richEdit);
			locatorRef = new WeakReference(locator);
			richEdit = null;
			locator = null;
		}

		class TestComRichEditOleInterfaceLocator : ZComRichEditOleInterfaceLocator
		{
			protected TestComRichEditOleInterfaceLocator(RichTextBox richEdit)
				: base(richEdit)
			{
			}

			public new static TestComRichEditOleInterfaceLocator GetInstance(RichTextBox box)
			{
				return (TestComRichEditOleInterfaceLocator)GetInstance(box, typeof(TestComRichEditOleInterfaceLocator));
			}

			public new int fReferenceCount
			{
				get { return base.fReferenceCount; }
			}
		}
#endif
	}
}
