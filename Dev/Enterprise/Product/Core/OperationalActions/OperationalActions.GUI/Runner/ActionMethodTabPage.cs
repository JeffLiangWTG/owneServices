using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	internal sealed class ActionMethodTabPage : ZBindingTabPage
	{
		public ActionMethodTabPage(OperationalActionMethod method)
		{
			if (method == null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			if (!method.HasControl)
			{
				throw new ArgumentException("method does not provide a control", nameof(method));
			}

			this.Text = method.Name;
			this.method = method;

			methodControl = (Control)method.NewGuiControl();
			methodControl.Dock = DockStyle.Fill;
			Controls.Add(methodControl);
		}

		public Size MinimumContentSize
		{
			get { return methodControl.MinimumSize; }
		}

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember is not supported for this control", nameof(dataMember));
			}

			if (dataSource != null)
			{
				OperationalActionRunner runner = (OperationalActionRunner)dataSource;
				OperationalActionMethodApplicator applicator = runner.MethodApplicators[method];
				base.SetDataBindingCore(applicator, "");
			}
			else
			{
				base.SetDataBindingCore(null, "");
			}
		}

		readonly Control methodControl;
		readonly OperationalActionMethod method;
	}
}
