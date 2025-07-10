using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[SuppressMessage("CargoWiseOne", "CW1046:DoNotSpecifyTooltipsManuallyRule", Justification = "Testing")]
	sealed class ToolTipServiceTest : TestCase
	{
		[SuppressMessage("Naming", "CW1046:CW1046 Do Not Specify Tooltips Manually Rule", Justification = "Control is not a ZButton")]
		public void TestSetTooltipAndDisposeControl_ShouldNotRetainInstance()
		{
			ToolTipService.ToolTipImpl tooltip;

			using (var control = new Label())
			{
				ToolTipService.SetToolTip(control, "Something");
				AssertEquals(1, ToolTipService.Cache.Count);

				tooltip = ToolTipService.Cache[control.GetHashCode()];
				AssertEquals("Something", tooltip.GetToolTip(control));
				AssertEquals(false, tooltip.IsDisposed);

				ToolTipService.SetToolTip(control, "Else");
				AssertEquals(1, ToolTipService.Cache.Count);

				tooltip = ToolTipService.Cache[control.GetHashCode()];
				AssertEquals("Else", tooltip.GetToolTip(control));
			}

			AssertEquals(0, ToolTipService.Cache.Count);
			AssertEquals(true, tooltip.IsDisposed);
		}

		public void TestShowToolTipAndDisposeControl_ShouldNotRetainInstance()
		{
			ToolTipService.ToolTipImpl tooltip;

			using (var form = new KForm())
			using (var label = new Label())
			{
				label.Text = "Test";
				form.Controls.Add(label);
				form.Show();
				System.Windows.Forms.Application.DoEvents();
				ToolTipService.ShowToolTip(label, "Something", label.Location);
				AssertEquals(1, ToolTipService.Cache.Count);

				AssertEquals("Something", ToolTipService.GetToolTip(label));

				tooltip = ToolTipService.Cache[label.GetHashCode()];
				AssertEquals(false, tooltip.IsDisposed);

				ToolTipService.ShowToolTip(label, "Else", label.Location);
				AssertEquals(1, ToolTipService.Cache.Count);
				AssertEquals("Else", ToolTipService.GetToolTip(label));
			}
			AssertEquals(0, ToolTipService.Cache.Count);
			AssertEquals(true, tooltip.IsDisposed);
		}

		public void TestHideToolTip()
		{
			using (var label = new Label())
			{
				ToolTipService.HideToolTip(label);
				AssertEquals(0, ToolTipService.Cache.Count);
			}
		}

		[SuppressMessage("Naming", "CW1046:CW1046 Do Not Specify Tooltips Manually Rule", Justification = "Control is not a ZButton")]
		public void TestSetTooltipOnMultipleControls()
		{
			using (var control1 = new Label())
			using (var control2 = new Label())
			{
				ToolTipService.SetToolTip(control1, "Something");
				AssertEquals(1, ToolTipService.Cache.Count);

				ToolTipService.SetToolTip(control2, "Else");
				AssertEquals(2, ToolTipService.Cache.Count);
			}

			AssertEquals(0, ToolTipService.Cache.Count);
		}

		[SuppressMessage("Naming", "CW1046:CW1046 Do Not Specify Tooltips Manually Rule", Justification = "Control is not a ZButton")]
		public void TestSetAndClearTooltip()
		{
			using (var control = new Label())
			{
				ToolTipService.SetToolTip(control, "Something");
				AssertEquals(1, ToolTipService.Cache.Count);

				ToolTipService.ClearTooltip(control);
				AssertEquals(0, ToolTipService.Cache.Count);
			}
		}

		[SuppressMessage("Naming", "CW1046:CW1046 Do Not Specify Tooltips Manually Rule", Justification = "Control is not a ZButton")]
		public void TestRegisterOnBackgroundThread_CanMarshallToForeground()
		{
			using (var form = new Form())
			using (var control = new Label())
			{
				form.Controls.Add(control);
				form.Show();

				IAsyncResult invokeResult = null;
				Task.Factory.StartNew(() =>
				{
					invokeResult = control.BeginInvoke(new Action(() => ToolTipService.SetToolTip(control, "Something")));
					AssertEquals(0, ToolTipService.Cache.Count);
				}).GetAwaiter().GetResult();
				control.EndInvoke(invokeResult);

				AssertEquals(1, ToolTipService.Cache.Count);
			}

			AssertEquals(0, ToolTipService.Cache.Count);
		}

		[SuppressMessage("Naming", "CW1046:CW1046 Do Not Specify Tooltips Manually Rule", Justification = "Control is not a ZButton")]
		[RequiresSTA]
		public void TestRegisterOnBackgroundThread_TellUsersWhatFor()
		{
			using (var form = new Form())
			using (var control = new Label())
			{
				form.Controls.Add(control);
				form.Show();

				Task.Factory.StartNew(() =>
				{
					ToolTipService.SetToolTip(control, "Something");
					AssertEquals(0, ToolTipService.Cache.Count);
				}).GetAwaiter().GetResult();

				AssertEquals(0, ToolTipService.Cache.Count);
			}

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Instance.Clear();
		}

		public void TestRegisterTooltipAndDisposeControl_ShouldNotLeak()
		{
			var weakReference = CreateControlThenRegisterTooltip();

			GC.Collect();
			GC.WaitForFullGCComplete();

			AssertNull(weakReference.Target);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[SuppressMessage("Naming", "CW1046:CW1046 Do Not Specify Tooltips Manually Rule", Justification = "Control is not a ZButton")]
		static WeakReference CreateControlThenRegisterTooltip()
		{
			var control = new Control();
			ToolTipService.SetToolTip(control, "Dat Tooltip");
			var weakReference = new WeakReference(control);
			((IDisposable)weakReference.Target).Dispose();
			return weakReference;
		}
	}
}
