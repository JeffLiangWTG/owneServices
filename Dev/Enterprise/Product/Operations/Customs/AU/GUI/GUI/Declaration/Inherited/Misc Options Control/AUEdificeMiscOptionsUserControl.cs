using System;
using Enterprise.Customs.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUEdificeMiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		public AUEdificeMiscOptionsUserControl()
		{
			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Overriding setter only")]
		public override Customs.Business.BaseJobDeclaration JobDeclaration
		{
			set
			{
				base.JobDeclaration = value;
			}
		}

		#region Implementation

		protected override void ChangeControlVisibilityOnMessageTypeChanged()
		{
			base.ChangeControlVisibilityOnMessageTypeChanged();

			var declaration = (Business.JobDeclaration)JobDeclaration;
			ForcePrimeEnclosureCheckBox.Visible = declaration.IsImport;
			CompilePrintersGroupBox.Visible = declaration.IsImport;
			ManifestClientIDTextBox.Visible = declaration.IsImport;
			MergeByDropEdit.Visible = declaration.IsImport || (declaration.IsEXPDeclaration && declaration.DeclarationExportCusEntryNumber == null);
			ExcisableGoodsHiddenCheckBox.Visible = declaration.IsExport;
			PrescribedGoodsHiddenCheckBox.Visible = declaration.IsExport;

			bool isEXD = declaration.IsExport && declaration.UseEXD;
			CCANTextBox.Visible = isEXD;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ZAUCustomsDeclarationForm parentForm = FindForm() as ZAUCustomsDeclarationForm;
			if (parentForm != null)
			{
				parentForm.Activated += new EventHandler(ParentForm_Activated);
			}
		}

		private bool messageTypeFocused;
		private void ParentForm_Activated(object sender, EventArgs e)
		{
			if (!messageTypeFocused)
			{
				messageTypeFocused = true;
				if (Visible)
				{
					EntryPrinterNumberTextBox.Focus();
				}
			}
		}
		#endregion
	}
}
