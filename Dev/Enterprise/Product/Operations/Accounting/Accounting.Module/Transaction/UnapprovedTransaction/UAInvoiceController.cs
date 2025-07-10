using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class UAInvoiceController : CreditNoteInvoiceController
	{
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.APUnapprovedInvoicesCancel; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APUnapprovedInvoicesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.APUnapprovedInvoices; }
		}

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity)
		{
			return Env.Security.APUnapprovedInvoicesCopy;
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.UAInvoice; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(UAInvoice); }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.UnapprovedTransaction;
			}
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			APInvoice aPInvoice = businessEntity as APInvoice;
			aPInvoice.SubmittedFromInvoicingForm = true;
			return GetNewInvoiceForm((Invoice)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var businessEntity = base.GetNewBusinessEntityInLocalFactory();
			((APInvoice)businessEntity).AllowDefaultChargeCodeLineToBeAdded = true;
			return businessEntity;
		}

		protected override InvoiceForm GetNewInvoiceForm(Invoice invoice)
		{
			return new UAInvoiceForm(invoice);
		}

		protected override bool ShouldHaveReversedBizo
		{
			get { return false; }
		}
	}
}
