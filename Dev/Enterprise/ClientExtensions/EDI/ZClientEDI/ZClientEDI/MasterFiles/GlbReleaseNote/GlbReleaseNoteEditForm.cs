#if DEBUG
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Gui
{
	[SuppressFormDesignerAnalysis]
	public partial class GlbReleaseNoteEditForm : GlbReleaseNoteForm
	{
		public GlbReleaseNoteEditForm(Business.GlbReleaseNoteManagerForSourceSafe businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				UpdateCheckoutMode();
			}
		}

		#region Implementation

		public new Business.GlbReleaseNoteManagerForSourceSafe BusinessEntity
		{
			get { return (Business.GlbReleaseNoteManagerForSourceSafe)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return "Edit"; }
		}

		internal ZArchitecture.GUI.ZButton InternalCloseButton => CloseButton;
		internal ZGrid InternalReleaseNotesGrid => ReleaseNotesGrid;

		#endregion

		#region Checkout

		protected override void OnClosing(CancelEventArgs e)
		{
			if (IsCheckedOut)
			{
				Globals.Message.ShowInformation("You must undo checkout or check in first before closing this form.", "Please Undo Checkout or Check In");
				e.Cancel = true;
			}
			else
			{
				if (!BusinessEntity.HasChanges)
				{
					this.DisplayMode = ODisplayMode.Browse;
				}
				base.OnClosing(e);
			}
		}

		void UpdateCheckoutMode()
		{
			ReleaseNotesGrid.RemoveAction = (IsCheckedOut) ? RemoveAction.RemoveAndDelete : RemoveAction.NoRemovePossible;
			CheckoutButton.Enabled = !IsCheckedOut;
			CheckInButton.Enabled = IsCheckedOut;
			UndoCheckoutButton.Enabled = IsCheckedOut;
			CloseButton.Enabled = !IsCheckedOut;
		}

		internal bool IsCheckedOut;

		void CheckoutButton_Click(object sender, System.EventArgs e)
		{
			Checkout();
		}

		internal void Checkout()
		{
			string errorMessage;
			bool isCheckoutSuccessful = BusinessEntity.Checkout(out errorMessage);

			if (isCheckoutSuccessful)
			{
				IsCheckedOut = true;
				UpdateCheckoutMode();
			}
			else
			{
				Globals.Message.ShowError("Update notes cannot be checked out at the moment. The error is:" +
					System.Environment.NewLine + System.Environment.NewLine + errorMessage, "Cannot Checkout");
			}
		}

		#endregion

		#region Undo Checkout

		void UndoCheckoutButton_Click(object sender, System.EventArgs e)
		{
			UndoCheckout();
		}

		void UndoCheckout()
		{
			if (Globals.Message.Show("Do you really want to undo checkout, lose all changes and close this form?", "Undo Check Out",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				BusinessEntity.UndoCheckout();
				IsCheckedOut = false;
				DisplayMode = ODisplayMode.ReadOnly; // Otherwise it will prompt for saving.
				Close();
			}
		}

		#endregion

		#region Check In

		void CheckInButton_Click(object sender, System.EventArgs e)
		{
			CheckIn();
		}

		void CheckIn()
		{
			ContinueWithSave saveResult = FireSaveButton();

			if (saveResult == ContinueWithSave.Yes)
			{
				IsCheckedOut = false;
				UpdateCheckoutMode();
				BusinessEntity.CheckIn();
			}
		}

		#endregion
	}
}
#endif
