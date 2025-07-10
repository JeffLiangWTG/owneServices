using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	[ContainerControlBaseClass]
	[TestExcludeZWinFormsAllHaveFormBashers]
	[TestExcludeZWinFormHasTypedConstructor]
	public class ZChildForm : ZForm
	{
		public ZChildForm() { }

		public ZChildForm(IBusiness businessEntity)
			: base(businessEntity)
		{ }

		protected override void AddAdornments()
		{
			ZChildFormStrategy.AddAdornments(this);
		}

		public override void ShowOtherUsersCurrentlyAccessingThisEntity() { }

		#region ForceDialogRendering

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (formBorderStyleChanged)
			{
				RestoreFormBorderStyleAfterRendering();
			}
		}

		FormBorderStyle originalBorderStyle = FormBorderStyle.Sizable;
		bool formBorderStyleChanged;

		protected void ChangeFormBorderStyleForRendering()
		{
			if (Env.Registry.ShouldForceDialogRendering)
			{
				originalBorderStyle = FormBorderStyle;
				FormBorderStyle = FormBorderStyle.FixedDialog;
				formBorderStyleChanged = true;
			}
		}

		protected void RestoreFormBorderStyleAfterRendering()
		{
			if (Env.Registry.ShouldForceDialogRendering)
			{
				FormBorderStyle = originalBorderStyle;
				formBorderStyleChanged = false;
			}
		}

		#endregion

#if !WINZOR

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.V))
			{
				ZFormPaster.PasteToActiveControl(this);
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

#endif
	}
}
