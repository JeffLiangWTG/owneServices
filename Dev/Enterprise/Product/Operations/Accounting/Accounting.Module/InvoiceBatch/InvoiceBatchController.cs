using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class InvoiceBatchController : AccountingTransactionController
	{
		public InvoiceBatchController()
		{
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new InvoiceBatchForm(businessEntity as InvoiceBatchHeader);
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.InvoiceBatch; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(InvoiceBatchHeader); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.InvoiceBatch; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewInvoiceBatch; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CancelInvoiceBatch; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.InvoiceBatch;
			}
		}

		protected override ZString CantReverseMessageBoxCaption
		{
			get { return Res.GetString("15e8cffc-51a3-4630-86c0-322e15f9b742", "Invoice Batch Statement"); }
		}
	}
}
