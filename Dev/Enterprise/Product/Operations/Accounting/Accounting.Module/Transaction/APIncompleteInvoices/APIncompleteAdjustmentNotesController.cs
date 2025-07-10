using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Transaction
{
	public class APIncompleteAdjustmentNotesController : APIncompleteTransactionsController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APIncompleteInvoices; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesAjdustmentNote; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.APIncompleteAdjustmentNote; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APAdjustmentNote); }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.APIncompleteInvoicesEdit; }
		}
	}
}
