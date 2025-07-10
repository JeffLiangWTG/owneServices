#if DEBUG
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Startup
{
	public partial class DatForm : Form
	{
		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "Exposing instance to DAT test startup")]
		[SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static DatForm Instance;

		public static readonly ManualResetEvent InitializedEvent = new ManualResetEvent(false);

		public DatForm()
		{
			InitializeComponent();
		}

		[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public DatForm(string enterprisePath)
		{
			TestCase.BaseSourcePath = enterprisePath;
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Instance = this;
			WindowState = FormWindowState.Minimized;
			ShowInTaskbar = false;
			BeginInvoke(new MethodInvoker(() => InitializedEvent.Set()));
		}

		[Obsolete("Invoke does not preserve exceptions properly, use InvokeEx instead.", true)]
		public new object Invoke(Delegate method) => throw new NotImplementedException();

		[Obsolete("Invoke does not preserve exceptions properly, use InvokeEx instead.", true)]
		public new object Invoke(Delegate method, params object[] args) => throw new NotImplementedException();

		public void InvokeEx(Action action)
		{
			InvokeEx(() =>
			{
				action();
				return null;
			});
		}

		public object InvokeEx(Func<object> func)
		{
			if (!this.InvokeRequired)
			{
				return func();
			}
			if (!this.IsHandleCreated)
			{
				this.CreateHandle();
			}
			Exception exception = null;
			var result = base.Invoke(new Func<object>(() =>
			{
				try
				{
					return func();
				}
				catch (Exception ex)
				{
					exception = ex;
					return null;
				}
			}));

			if (exception != null)
			{
				throw new AggregateException(exception);
			}

			return result;
		}
	}
}
#endif
