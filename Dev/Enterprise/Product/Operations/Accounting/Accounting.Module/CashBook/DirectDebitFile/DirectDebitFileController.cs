using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.GUI.CashBook.DirectDebitBatch;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class DirectDebitFileController : AccountingTransactionController
	{
		public DirectDebitFileController()
		{
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new DirectDebitBatchForm(businessEntity as DirectDebitBatchHeader);
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.DirectDebitFile; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DirectDebitBatchHeader); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DDRFile; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GenerateDirectDebitFile; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CancelDirectDebitFile; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.DirectDebitFile;
			}
		}

		protected override ZString CantReverseMessageBoxCaption
		{
			get { return Res.GetString("8ee8a09c-c105-4fc7-a6a7-dc5dfcd1c620", "Cancel DDR Batch"); }
		}
	}
}