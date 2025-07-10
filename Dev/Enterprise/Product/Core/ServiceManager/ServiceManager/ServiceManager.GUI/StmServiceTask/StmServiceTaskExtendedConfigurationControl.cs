using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	public partial class StmServiceTaskExtendedConfigurationControl : ZUserControl, IBindTo
	{
		public StmServiceTaskExtendedConfigurationControl()
		{
			InitializeComponent();
		}

		[Browsable(true)]
		[Category(ZGUIConstants.DesignerCategory)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("")]
		public string BindTo { get; set; }

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (!string.IsNullOrEmpty(BindTo))
			{
				BindingHelper.UpdateControlBindTos(this, BindTo);
			}
		}

		void errorLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch(warningLink.Text);
		}
	}
}
