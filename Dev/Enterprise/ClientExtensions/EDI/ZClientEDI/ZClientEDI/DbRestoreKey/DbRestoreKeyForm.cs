using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.DbRestoreKey
{
	partial class DbRestoreKeyForm : ZChildForm
	{
		public DbRestoreKeyForm()
			: base()
		{
		}

		public DbRestoreKeyForm(DbRestoreKeyGenerator businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton);
		}

		#region Implementation

		public new DbRestoreKeyGenerator BusinessEntity
		{
			get { return (DbRestoreKeyGenerator)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Database Restore Release Key Generation"; }
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			// Stop the base class from asking the user if they want to save changes
			DisplayMode = ODisplayMode.Browse;
			base.OnClosing(e);
		}

		#endregion

		void ServerTextBox_TextChanged(object sender, EventArgs e)
		{
			BusinessEntity.ServerName = ServerTextBox.Text;
			BusinessEntity.ResetReleaseKey();
		}

		void DatabaseTextBox_TextChanged(object sender, EventArgs e)
		{
			BusinessEntity.DatabaseName = DatabaseTextBox.Text;
			BusinessEntity.ResetReleaseKey();
		}

		void SessionIdTextBox_TextChanged(object sender, EventArgs e)
		{
			BusinessEntity.SessionId = SessionIdTextBox.Text;
			BusinessEntity.ResetReleaseKey();
		}

		void GenerateButton_Click(object sender, EventArgs e)
		{
			try
			{
				BusinessEntity.GenerateReleaseKey();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message, "Release Key Generation");
			}
		}
	}
}
