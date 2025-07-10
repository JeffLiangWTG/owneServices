using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class PaymentBatchModule : ZFilterGridModule
	{
		public PaymentBatchModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PaymentBatch; }
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.PaymentBatch;

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override bool AllowNew => false;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.PaymentBatch);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccPaymentBatchCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl() => new PaymentBatchFilterStripControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new PaymentBatchFilterStripBusinessObject();

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.PrintMenuItemText, new EventHandler(PrintPaymentBatch)));
			return menuItems.ToArray();
		}

		#region Print

		protected void PrintPaymentBatch(object sender, EventArgs e)
		{
			AccPaymentBatch batch;
			var printCheckpoint = Env.Security.PrintPaymentBatch;

			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length != 1)
			{
				Globals.Message.Show(Res.GetString("6dd06647-7e50-4ddb-b8b0-262d3389db96", "Please select a payment batch to print."));
			}
			else if (!printCheckpoint.IsAllowed)
			{
				printCheckpoint.ShowError();
			}
			else if ((batch = SelectedBusinessObjects[0] as AccPaymentBatch).PaymentApprovalCollection.Count == 0)
			{
				Globals.Message.Show(Res.GetString("6B6E3C0E-A809-45E6-9C62-F5FF37E8D09C", "The selected payment batch does not include any approval request."));
			}
			else if (batch.PaymentApprovalCollection.Any())
			{
				var printer = new PaymentDocumentsPrinter(batch.PaymentApprovalCollection[0], Factory);
				(printer as IPaymentPrint).PrintPaymentBatchListing();
			}
		}

		#endregion
	}
}
