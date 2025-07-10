using System;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class APPaymentForm : PaymentForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public APPaymentForm()
		{
		}

		public APPaymentForm(APPayment payment)
			: base(payment)
		{
			payment.DisplayHotCheques += new APPayment.HotChequeSelectedHandler(Payment_DisplayHotCheques);
			payment.NotifyUserPaymentUneditable += new APPayment.PaymentFieldsUneditableHandler(Payment_NotifyUserPaymentUneditable);
			PlugIns.Add(ControllerIDs.LinkedeNettEDIMessage);
		}

		public APPayment APPayment
		{
			get { return BusinessEntity as APPayment; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation

		IAsyncResult asyncResult;

		void Payment_DisplayHotCheques(object sender, HotChequeLink link)
		{
			asyncResult = BeginInvoke(new ShowHotChequeHandler(DisplayHotCheques), new object[] { this, link });
		}

		#region DEBUG
		public void EndInvoke_ForTest()
		{
			EndInvoke(asyncResult);
		}
		#endregion

		delegate void ShowHotChequeHandler(object sender, HotChequeLink link);

		void DisplayHotCheques(object sender, HotChequeLink chequeLink)
		{
			HotChequeLinkForm chequeLinkForm = GetHotChequeLinkForm(chequeLink);
			chequeLinkForm.Closed += new EventHandler(ChequeLinkForm_Closed);
			ZFormModaliser.Show(chequeLinkForm, this);
		}

		protected virtual HotChequeLinkForm GetHotChequeLinkForm(HotChequeLink chequeLink)
		{
			return new HotChequeLinkForm(chequeLink);
		}

		void ChequeLinkForm_Closed(object sender, EventArgs e)
		{
			HotChequeLinkForm linkForm = sender as HotChequeLinkForm;
			if (linkForm != null && linkForm.SelectedHotCheque != null)
			{
				APPayment.ImportSelectedHotCheque(linkForm.SelectedHotCheque);
			}
		}

		void Payment_NotifyUserPaymentUneditable(object sender, string message)
		{
			Globals.Message.ShowInformation(message, Res.GetString("77d33ae8-9e15-4a1d-959f-a239a072bb0a", "Hot Check Imported"));
		}

		#endregion
	}
}

