using System.Threading;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDateEditTest_ControlTest : ZControlBaseTestCase<ZDateEdit>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new[] { "DateTimeValue", "ReadOnly" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

		protected override void BindControl()
		{
			base.BindControl();
			IDataBoundControl bindableControl = Control;
			Control.SetBindingMember(DummyBizoSchema.Z0_Date.Name);
			bindableControl.SetDataBinding(Dummy, "");
		}

#if !WINZOR

		[DeveloperOnlyTest]
		public void TestZDateEditPaste()
		{
			using (var testControl = new ZDateEdit())
			{
				AssertPaste("20-JUL-18 14:38", testControl, true);
				AssertPaste("20-JUL-18", testControl, true);
				AssertPaste("ABC", testControl, false);
			}
		}

		void AssertPaste(string dateString, ZDateEdit control, bool expected)
		{
			var retry = 10;
			control.Text = "";
			IDataObject dataObject = null;
			SafeClipboard.SetDataObject(dateString);
			while (retry > 0 && dataObject == null)
			{
				Thread.Sleep(50);
				dataObject = SafeClipboard.GetDataObject();
				retry--;
			}

			AssertEquals($"TryPaste should return {expected}", ((IPastableControl)control).TryPaste(), expected);
			AssertEquals(expected ? dateString : "", control.Text);
		}

#endif
	}
}
