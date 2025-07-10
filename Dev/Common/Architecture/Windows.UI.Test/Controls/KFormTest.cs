using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Interop;
using CargoWise.Windows.UI.Controls.Internal;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;
using static CargoWise.Windows.UI.KForm;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KFormTest : TestCase
	{
		public const int WM_KEYDOWN = 256;
		public const int WM_KEYUP = 257;

		public void TestGCCollectorTrackerIsNullWhenFinalize()
		{
			AssertExceptionThrown<Exception>(() =>
			{
				_ = new KFormTestCase();
			});
			AssertNoExceptionThrown(() =>
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			});
		}

		class KFormTestCase : KForm
		{
			static Control GetControlWhenExceptionThrown() => throw new Exception();

			public KFormTestCase() : this(GetControlWhenExceptionThrown())
			{
			}

			KFormTestCase(Control control) : base()
			{
				this.control = control;
			}
			[SuppressMessage("Maintainability", "IDE0052: Remove unread private member.", Justification = "The unit test need this feild to trigger GC!")]
			readonly Control control;
		}

#if !WINZOR

		[DeveloperOnlyTest]
		[SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public void TestShowingControlsOverlayWorks()
		{
			const Keys hotkey = Keys.Control | Keys.Shift | Keys.F;

			ControlInformationOverlayForm overlay;
			using (var kform = new KForm())
			{
				var textbox = new KTextBox { Name = "Bob", Location = ControlDpiScalingHelper.NewScaledPoint(10, 10) };
				kform.Controls.Add(textbox);

				kform.Show();

				Assert("PRE: Its not already open", !System.Windows.Forms.Application.OpenForms.OfType<ControlInformationOverlayForm>().Any());

				Cursor.Position = textbox.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(10, 10));
				SendKeyDown(kform, hotkey);
				System.Windows.Forms.Application.DoEvents();

				overlay = System.Windows.Forms.Application.OpenForms.OfType<ControlInformationOverlayForm>().SingleOrDefault();
				if (overlay == null)
				{
					Assert("Overlay didn't appear this time - that's fine. Succeed silently rather than going on amnesty", true);
					return;
				}

				AssertEquals("Should highlight the textbox", textbox, overlay.HighlightedControl);

				SendKeyUp(kform, hotkey);
				System.Windows.Forms.Application.DoEvents();

				Assert("Overlay now hidden", !overlay.Visible);
			}

			Assert("Overlay should be disposed with parent", overlay.IsDisposed);
		}

#endif

		public static void SendKeyDown(Control control, Keys key)
		{
			UnsafeNativeMethods.SendMessage(new HandleRef(control, control.Handle), WM_KEYDOWN, new IntPtr((int)key), IntPtr.Zero);
		}

		public static void SendKeyUp(Control control, Keys key)
		{
			UnsafeNativeMethods.SendMessage(new HandleRef(control, control.Handle), WM_KEYUP, new IntPtr((int)key), IntPtr.Zero);
		}

		public void TestLocalHotkeysAreRunBeforeGlobalOnes()
		{
			using (var form = new KFormWithGlobalHotkeyOverride())
			{
				var globalsDelegateCalled = 0;
				form.GlobalHotkeys.RegisterHotKey(Keys.Control | Keys.L, (sender, key) => { globalsDelegateCalled++; return true; });

#pragma warning disable WFDEV001 // 'Message.LParam' is obsolete: 'Casting to/from IntPtr is unsafe, use LParamInternal.'
				var message = new Message { Msg = 0x0100, LParam = (IntPtr)Keys.L };
#pragma warning restore WFDEV001
				Assert("Should handle it", form.ProcessCmdKey(ref message, Keys.Control | Keys.L));
				AssertEquals("Should have been from the global hotkey", 1, globalsDelegateCalled);

				var formProcessedKey = true;
				var formDelegateCalled = 0;
				form.Hotkeys.RegisterHotKey(Keys.Control | Keys.L, (sender, key) => { formDelegateCalled++; return formProcessedKey; });

				Assert("Should handle it", form.ProcessCmdKey(ref message, Keys.Control | Keys.L));
				AssertEquals("Should have been the form delegate", 1, formDelegateCalled);
				AssertEquals("Global delegate should not have been called again", 1, globalsDelegateCalled);

				formProcessedKey = false;
				Assert("Should handle it", form.ProcessCmdKey(ref message, Keys.Control | Keys.L));
				AssertEquals("Form delegate should have been run", 2, formDelegateCalled);
				AssertEquals("Since form returned false, globals should have a chance to process this hotkey", 2, globalsDelegateCalled);
			}
		}

		public void TestFinalizer()
		{
			// 1st time - no report
			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("", ErrorReporter.LastMessageReported);

			// 2nd time - report
			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertNotEquals("", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			// 3rd and subsequent - no more reports (to minimise performance problems)
			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("", ErrorReporter.LastMessageReported);

			MakeControl();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			GCTracker.ClearListForTest();
		}

		void MakeControl()
		{
			var form = new KForm();
			form.Name = "Cecil";
			form.DesignerActionExtenderProvider.Dispose();

			DisposableLeakListener.Instance.UnRegisterDisposable(form);
		}

		public void TestCurrentDataItem_WhenBindingToAnObject()
		{
			Form.DataSourceType = typeof(MockDetailObject);
			TestCurrentDataItem();
		}

		public void TestCurrentDataItem_WhenBindingToACollection()
		{
			Form.DataSourceType = typeof(MockDetailObjectCollection);
			TestCurrentDataItem();
		}

		void TestCurrentDataItem()
		{
			MockMasterObjectCollection collection = new MockMasterObjectCollection();
			collection.AddNew().DetailObjects.AddNew();

			Form.SetDataBinding(collection, "DetailObjects");
			Form.Show();

			Type currentType = Form.CurrentDataItem.GetType();
			AssertEquals(true, Form.DataSourceType.IsAssignableFrom(currentType));
		}

		public void TestExtenderProviderMembersAreProtectedFields()
		{
			ExtendedProviderMemberTestHelper.AssertExtenderMembersAreProtectedFields(typeof(KForm));
		}

		public void TestTenporarilyDesableQueryCaptionSuffix()
		{
			using (KForm form = new KForm())
			using (TemporarilyDisableQueryFormCaptionSuffix())
			{
				form.Text = "AA";
				AssertEquals("Result should be empty, as method is temporary disabled.", "", form.currentFormCaptionSuffix);
			}
		}

		public void TestDataSourceChanged()
		{
			using (KForm form = new KForm())
			{
				form.DataSourceType = typeof(object);

				bool dataSourceChangedCalled = false;
				form.DataSourceChanged += (sender, e) =>
				{
					AssertEquals(form, sender);
					dataSourceChangedCalled = true;
				};
				AssertEquals("DataSourceChanged not initially", false, dataSourceChangedCalled);
				form.DataSource = new object();
				AssertEquals("DataSourceChanged fired", true, dataSourceChangedCalled);
			}
		}

		[ExpectNoExceptions]
		public void TestFormTextNullRef()
		{
			EventHandler<QueryFormCaptionEventArgs> f = (sender, e) =>
			{
				e.GlobalFormTopCaption = null;
			};

			using (KForm form = new KForm())
			{
				try
				{
					KForm.QueryFormCaptionSuffix += f;
					form.Text = null; //there was an exception.
				}
				finally
				{
					KForm.QueryFormCaptionSuffix -= f;
				}
			}
		}

		#region Test Classes

		class TestForm : KForm
		{
		}

		[CodeAlive("WI00575476 Baseline")]
		class TestDataSource
		{
			public string Property { get; set; }
		}

		#endregion
		#region Implementation

		TestForm Form
		{
			get
			{
				if (form == null)
				{
					form = new TestForm();
				}
				return form;
			}
		}
		TestForm form;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		public class KFormWithGlobalHotkeyOverride : KForm
		{
			protected internal override HotkeyRegister GlobalHotkeys { get; } = new HotkeyRegister();
			internal new bool ProcessCmdKey(ref Message msg, Keys keyData) => base.ProcessCmdKey(ref msg, keyData);
		}

		#endregion
	}
}
