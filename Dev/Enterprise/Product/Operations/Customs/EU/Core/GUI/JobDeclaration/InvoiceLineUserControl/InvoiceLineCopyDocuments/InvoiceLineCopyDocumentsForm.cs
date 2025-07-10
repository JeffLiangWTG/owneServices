using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class InvoiceLineCopyDocumentsForm : ZChildForm
	{
		public InvoiceLineCopyDocumentsForm(CopyDocumentsSelectionHeader header) : base(header)
		{
			this.header = header;
			InitializeComponent();
			UpdateGridColumnLayout();
			UpdateDynamicPanelLayout();
			CloseButton.Click += CloseButtonOnClick;
			OkButton.Click += OkButtonOnClick;
			SelectAllButton.Click += SelectAllOnClick;
		}

		readonly CopyDocumentsSelectionHeader header;
		protected override void OnClosing(CancelEventArgs e)
		{
			CloseButton.Focus();

			if (DialogResult != DialogResult.Cancel)
			{
				header.RunPreSaveValidation();
				if (header.NotificationsIncludingChildren.GetErrors().Any())
				{
					Globals.Message.ShowError(Res.GetString("63b87f94-620b-49cb-954e-653f64d55405", "The form has errors. Please fix them before continuing."));
					e.Cancel = true;
				}
			}

			base.OnClosing(e);
		}

		void SelectAllOnClick(object sender, EventArgs e)
		{
			header.SelectAll();
		}

		void OkButtonOnClick(object sender, EventArgs e)
		{
			header.Copy();
			Close();
		}

		void CloseButtonOnClick(object sender, EventArgs e)
		{
			Close();
		}

		void UpdateGridColumnLayout()
		{
			CopyDocumentsLinesGridControl.ApplyGridColumnLayout(new CopyDocumentsLineGridColumnLayout());
		}

		void UpdateDynamicPanelLayout()
		{
			DynamicLayoutPanel.UpdateLayout(new InvoiceLineCopyDocumentLayout());
		}

		public override string FormHeading => FormCaption;
	}
}
