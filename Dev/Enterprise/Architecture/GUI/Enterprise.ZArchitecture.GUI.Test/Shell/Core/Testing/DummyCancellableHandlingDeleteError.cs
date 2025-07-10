using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class DummyCancellableHandlingDeleteError : DummyBusinessObject, ICancellable, IHandleDeleteError
	{
		public DummyCancellableHandlingDeleteError(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			RollbackAfterDeleteError = true;
			RebindAfterDeleteError = true;
		}

		public bool ThrowSaveConcurrencyError;
		public bool ForceDeleted;

		public override bool IsDeleted => base.IsDeleted || ForceDeleted;

		public override void OnSaving()
		{
			base.OnSaving();
			ThrowSaveConcurrencyErrorIfEnabled();
		}

		protected override void OnSavingForDelete()
		{
			base.OnSavingForDelete();
			ThrowSaveConcurrencyErrorIfEnabled();
		}

		void ThrowSaveConcurrencyErrorIfEnabled()
		{
			if (ThrowSaveConcurrencyError)
			{
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(null, ((IBusinessObjectInternals)this).Row, ((CargoWise.Data.IDbConnected)Factory).Connection), Factory);
			}
		}

		#region ICancellable

		public bool IsCancelled { get; set; }
		public bool IsCancelledHasChanged { get; set; }

		public string CanCancel() => null;

		public string CanReactivate() => throw new NotImplementedException();

		#endregion

		#region IHandleDeleteError

		public bool RollbackAfterDeleteError { get; set; }
		public bool RebindAfterDeleteError { get; set; }
		public bool DisableFormOnDeleteConcurrencyError { get; set; }

		#endregion
	}
}
