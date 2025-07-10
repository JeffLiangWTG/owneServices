using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class DocumentDbManagerForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public DocumentDbManagerForm()
		{
			InitializeComponent();
		}

		public DocumentDbManagerForm(DocumentDbManager bo)
			: base(bo)
		{
			InitializeComponent();
		}

		DocumentDbManager Manager
		{
			get { return (DocumentDbManager)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		bool ConfirmSave()
		{
			return GetUserConfirmation(Res.GetString("cfa6f4a3-1c5c-4743-8600-b78c0bd551d1", "Do you want to save changes to the eDocs databases and close?"));
		}

		bool ConfirmCancel()
		{
			return GetUserConfirmation(Res.GetString("076beca0-ef59-4c5b-b749-202e2cb75a15", "Do you want to cancel and lose all read-only status changes?"));
		}

		bool GetUserConfirmation(string confirmationMessage)
		{
			return Globals.Message.Show(confirmationMessage, FormCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
		}

		bool Save()
		{
			try
			{
				Manager.Save();
				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
				return false;
			}
		}

		void okButton_Click(object sender, EventArgs e)
		{
			if (Manager.StorageDatabaseCollection.Any(storageDatabase => storageDatabase.HasErrors))
			{
				Globals.Message.ShowError(Res.GetString("DFDE3BD1-7B84-4608-920C-C5AC473FA405", "There are errors that need to be corrected before save."));
			}
			else
			{
				if (Manager.StorageDatabaseCollection.HasChanges)
				{
					if (ConfirmSave() && Save())
					{
						Close();
					}
				}
				else
				{
					Close();
				}
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (Manager.StorageDatabaseCollection.HasChanges && !ConfirmCancel())
			{
				e.Cancel = true;
			}
			else
			{
				base.OnClosing(e);
			}
		}
	}
}
