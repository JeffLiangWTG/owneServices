using System;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVXUserControl : BaseCustomsEntryUserControl
	{
		public LVXUserControl()
		{
			InitializeComponent();
			this.LVSLinesUserControl.SetLVXMode();

			var groupBoxText = Res.GetString("5d38f24b-b073-4545-bdd7-b5244068c6e0", "Courier LVS Declaration Details");
			this.LVSSubHeaderAllDetailsUserControl.SetGroupBoxText(groupBoxText);
			this.LVSLinesUserControl.SetGroupBoxText(groupBoxText);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var declaration = ((JobDeclaration)DataSource).LVXInvoiceHeader.FirstAdditionalDeclaration;
			this.EditButton.Enabled = declaration != null;
			if (declaration != null)
			{
				this.TransactionDetailsGroupBox.Text = Res.GetString("d97218f2-24ab-4192-9021-1602a729dc82", "Type F Consolidation {0} Details", declaration.JE_DeclarationReference);
			}

			ChangeTransactionNumberControlVisibility();
		}

		void ChangeTransactionNumberControlVisibility()
		{
			var declaration = this.CurrentDataItem as JobDeclaration;
			if (declaration != null && declaration.IsLVX)
			{
				var isFormattedControlVisible = !declaration.DisplaySequentialOfTransactionNumberSeparately;
				FormattedTransactionNumberTextBox.Visible = isFormattedControlVisible;
				SecurityCodeTextBox.Visible = !isFormattedControlVisible;
				SequentialNumberTextBox.Visible = !isFormattedControlVisible;
				CheckDigitTextBox.Visible = !isFormattedControlVisible;
			}
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			var declaration = ((JobDeclaration)DataSource).LVXInvoiceHeader.FirstAdditionalDeclaration;
			if (declaration != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
				controller.SetFormsModalTo(FindForm());
				controller.ShowEditForm(declaration);
			}
		}
	}
}
