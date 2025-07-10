using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARTransferController : TransferController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARTransfer; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARTransfer); }
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Transfer.New(typeof(ARTransfer), Factory);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewReceivablesTransfer; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesTransfer; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesTransfer; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesTransfer; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		#endregion
	}
}
