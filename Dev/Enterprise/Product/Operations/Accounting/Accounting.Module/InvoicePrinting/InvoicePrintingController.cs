using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class InvoicePrintingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.InvoicePrinting; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.InvoicePrinting; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GovernmentInvoice); }
		}

		#region Implementation

		protected override bool DisallowMultiDeleteBecauseDeleteIsNotWhatIsReallyHappeningInAccounting
		{
			get
			{
				return true;
			}
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			GovernmentInvoice selectedInvoice = (GovernmentInvoice)businessEntity;

			var messageForUpdateActionPermissions = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberManualUpdateToAnyValue(selectedInvoice.AH_Ledger, selectedInvoice.Company);
			if (!messageForUpdateActionPermissions.IsEmpty)
			{
				Globals.Message.ShowInformation(messageForUpdateActionPermissions);
				return null;
			}

			var eligibilityWarningMessage = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberEligibilityWarning(selectedInvoice.Company);
			if (!eligibilityWarningMessage.IsEmpty)
			{
				Globals.Message.ShowWarning(eligibilityWarningMessage);
			}

			selectedInvoice.PrepareClassAInvoiceForEditing();
			return new ClassAInvoiceForm(selectedInvoice);
		}

		public override IZForm ShowNewForm()
		{
			ShowPrintMessage();
			return null;
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			ShowPrintMessage();
			return null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			ShowPrintMessage();
			return null;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			ShowPrintMessage();
			return null;
		}

		protected void ShowPrintMessage()
		{
			string caption = Res.GetString("01f6728f-350b-4691-a79c-87b206cd332e", "Print Invoice");
			string message = Res.GetString("905d1b87-7467-466d-88d4-852472b8c838", "Please select an invoice or invoices, and either press the print button at the top of the form or right click the mouse and choose 'Print'.");
			Globals.Message.ShowInformation(message, caption);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
