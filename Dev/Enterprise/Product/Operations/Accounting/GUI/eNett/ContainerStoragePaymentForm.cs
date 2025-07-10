using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.eNett
{
	public partial class ContainerStoragePaymentForm : ZForm, IButtonPostTextOverride
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ContainerStoragePaymentForm()
		{
			InitializeComponent();
		}

		public ContainerStoragePaymentForm(StorageFeeInvoicePayment bo)
			: base(bo)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostButton, CloseButton);
		}

		string IButtonPostTextOverride.PostButtonText
		{
			get { return Res.GetString("48004cff-6fe1-493a-82c5-c4f270b2b154", "Post && Close"); }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ZGlobalMutex comPayMutex = null;
			EventHandler acquireCOMpayMutex = (sender, e) =>
				{
					comPayMutex = StorageFeeInvoicePayment.GetCOMPayMutex(DataSource.Invoice.ReceiptPayment.PK);
					comPayMutex.Lock();
				};
			var objectToHookEvent = DataSource.Invoice;
			//Based on CS01697499, under very unusual scenario, the InvoiceDate became empty,
			//We haven't found how but on the safe side, we set it here before we post it.
			//This appears to be the best place to apply this workaround 
			if (objectToHookEvent.InvoiceDate.IsEmpty)
			{
				objectToHookEvent.AH_InvoiceDate = ZDateTime.Now;
			}

			if (objectToHookEvent.ReceiptPaymentAH_InvoiceDate.IsEmpty)
			{
				objectToHookEvent.ReceiptPaymentAH_InvoiceDate = ZDateTime.Now;
			}

			objectToHookEvent.ReceiptPaymentCreated += acquireCOMpayMutex;
			try
			{
				var continueWithSave = base.ValidateAndSave();
				if (continueWithSave == ContinueWithSave.Yes)
				{
					var webServiceWrapper = new eNettWebServiceWrapper();
					webServiceWrapper.ProcessDirectDebit(DataSource.Invoice.ReceiptPayment);
				}

				return continueWithSave;
			}
			finally
			{
				var mutexToDispose = ((IDisposable)comPayMutex);
				if (mutexToDispose != null)
				{
					mutexToDispose.Dispose();
				}
				objectToHookEvent.ReceiptPaymentCreated -= acquireCOMpayMutex;
			}
		}

		public new StorageFeeInvoicePayment DataSource
		{
			get { return base.DataSource as StorageFeeInvoicePayment; }
		}
	}
}
