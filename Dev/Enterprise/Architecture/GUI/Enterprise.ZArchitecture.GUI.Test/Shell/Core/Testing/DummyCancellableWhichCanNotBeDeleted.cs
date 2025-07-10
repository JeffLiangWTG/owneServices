using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[PreventDelete(false)]
	sealed class DummyCancellableWhichCanNotBeDeleted : DummyBusinessObject, ICancellable, IHandleDeleteError
	{
		public DummyCancellableWhichCanNotBeDeleted(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			RollbackAfterDeleteError = true;
			RebindAfterDeleteError = true;

			var childDummy = Factory.New<DummyBusinessObject>();
			childDummy.Z0_Number = 10;
			Collection.Add(childDummy);
		}

		#region ICancellable Members

		public string CanCancel() => null;

		public string CanReactivate() => null;

		public bool IsCancelled
		{
			get => !Z0_Bool;
			set => Z0_Bool = !value;
		}

		public bool IsCancelledHasChanged => throw new NotImplementedException();

		#endregion

		#region IHandleDeleteError

		public bool RollbackAfterDeleteError { get; set; }

		public bool RebindAfterDeleteError { get; set; }

		public bool DisableFormOnDeleteConcurrencyError { get; set; }

		#endregion
	}
}
