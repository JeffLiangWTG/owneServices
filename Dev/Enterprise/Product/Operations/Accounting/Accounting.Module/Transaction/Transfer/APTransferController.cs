using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APTransferController : TransferController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APTransfer; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APTransfer); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Transfer.New(typeof(APTransfer), Factory);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PayablesTransactions; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesTransfer; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReversePayablesTransfer; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
