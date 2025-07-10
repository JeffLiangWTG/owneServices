using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARReceiptBatchPostingController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ReceiptBatchForm((ARReceiptBatchPoster)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesReceipt; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ARReceiptBatchPosting; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARReceiptBatchPoster); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ARReceiptBatchPoster(Factory);
		}
	}
}
