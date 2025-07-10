#if DEBUG
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentEngine.SDF;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.SDF
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis] // Has no Resx to stop the Compiler Warning about release builds.
	partial class StmSystemDefinedFieldForm : ZChildForm
	{
		public StmSystemDefinedFieldForm(StmSystemDefinedFieldManager manager)
			: base(manager)
		{
			InitializeComponent();
			UpdateCheckoutMode();
			UpdateEditingMode(false);
		}

		public override string FormVerb
		{
			get { return FormVerbs.Edit; }
		}

		public new StmSystemDefinedFieldManager BusinessEntity
		{
			get { return (StmSystemDefinedFieldManager)base.BusinessEntity; }
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(new ITransactionParticipant[] { BusinessEntity.FactoryForSaving });
		}

		void UpdateCheckoutMode()
		{
			AllowRemovingInGrids(IsCheckedOut);
			CheckoutButton.Enabled = !IsCheckedOut;
			SaveButton.Enabled = IsCheckedOut;
			UndoCheckoutButton.Enabled = IsCheckedOut;
			CloseButton.Enabled = !IsCheckedOut;
			EditWithoutCheckoutButton.Enabled = !IsCheckedOut;
		}

		void UpdateEditingMode(bool isEditing)
		{
			AllowRemovingInGrids(isEditing);
			CheckoutButton.Enabled = !isEditing;
			SaveWithoutCheckInButton.Enabled = isEditing;
			EditWithoutCheckoutButton.Enabled = !isEditing;
		}

		void AllowRemovingInGrids(bool isAllowed)
		{
			FieldsGrid.RemoveAction = (isAllowed) ? RemoveAction.RemoveAndDelete : RemoveAction.NoRemovePossible;
			SpecificCountriesGrid.RemoveAction = (isAllowed) ? RemoveAction.RemoveAndDelete : RemoveAction.NoRemovePossible;
			FieldColumnsGrid.RemoveAction = (isAllowed) ? RemoveAction.RemoveAndDelete : RemoveAction.NoRemovePossible;
		}

		internal bool IsCheckedOut;

		#region Close

		protected override void OnClosing(CancelEventArgs e)
		{
			if (IsCheckedOut)
			{
				Globals.Message.ShowInformation("You must undo checkout or check in first before closing this form.", "Please Undo Checkout or Check In");
				e.Cancel = true;
			}
			else
			{
				base.OnClosing(e);
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#endregion

		#region SourceControl

		void UndoCheckoutButton_Click(object sender, System.EventArgs e)
		{
			UndoCheckout();
		}

		void CheckoutButton_Click(object sender, System.EventArgs e)
		{
			Checkout();
		}

		void SaveButton_Click(object sender, System.EventArgs e)
		{
			SaveForCheckIn();
		}

		void UndoCheckout()
		{
			if (Globals.Message.Show("Do you really want to undo checkout and lose all changes?", "Undo Checkout",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				BusinessEntity.UndoCheckout();
				IsCheckedOut = false;
				UpdateCheckoutMode();
			}
		}

		void Checkout()
		{
			if (Globals.Message.Show("Checking out will override all data in your database with data from SourceControl. Do you want to continue?", "Checkout", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
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
					Globals.Message.ShowError("System Defined Fields cannot be checked out at the moment. The error is:" +
						System.Environment.NewLine + System.Environment.NewLine + errorMessage, "Cannot Checkout");
				}
			}
		}

		void SaveForCheckIn()
		{
			ContinueWithSave saveResult = FireSaveButton();

			if (saveResult == ContinueWithSave.Yes)
			{
				IsCheckedOut = false;
				UpdateCheckoutMode();
				BusinessEntity.SaveForCheckIn();
			}
		}

		#endregion

		#region For Testing

		void EditWithoutCheckoutButton_Click(object sender, System.EventArgs e)
		{
			EditWithoutCheckout();
		}

		void SaveWithoutCheckInButton_Click(object sender, System.EventArgs e)
		{
			SaveWithoutCheckIn();
		}

		void EditWithoutCheckout()
		{
			BusinessEntity.EditWithoutCheckout();
			UpdateEditingMode(true);
		}

		void SaveWithoutCheckIn()
		{
			ContinueWithSave saveResult = FireSaveButton();

			if (saveResult == ContinueWithSave.Yes)
			{
				BusinessEntity.SaveWithoutCheckIn();
				UpdateEditingMode(false);
			}
		}

		#endregion

		#region class FixedSizeZDropEdit

		internal class FixedSizeZDropEdit : ZDropEdit
		{
			protected override void SetControlSize(int maxLength)
			{
				base.SetControlSize(16);
			}
		}

		#endregion
	}
}

#endif
