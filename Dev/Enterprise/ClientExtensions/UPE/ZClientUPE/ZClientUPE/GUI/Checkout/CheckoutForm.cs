using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CheckoutForm : ZChildForm
	{
		public CheckoutForm()
		{
		}

		public CheckoutForm(Checkout businessObject)
			: base(businessObject)
		{
		}

		public Checkout Checkout
		{
			get { return (Checkout)BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Operations"; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Checkout.SetFormState();
		}

		protected override void OnClosed(EventArgs e)
		{
			Checkout.SaveFormState();
			base.OnClosed(e);
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			Print();
		}

		void Print()
		{
			Checkout.RunPreSaveValidation();

			if (Checkout.HasErrors)
			{
				Globals.Message.ShowError("Please fix the errors before printing.");
			}
			else
			{
				try
				{
					Cursor.Current = Cursors.WaitCursor;
					Checkout.Print();
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}

			TrackingNumberTextBox.Focus();
			TrackingNumberTextBox.SelectAll();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
