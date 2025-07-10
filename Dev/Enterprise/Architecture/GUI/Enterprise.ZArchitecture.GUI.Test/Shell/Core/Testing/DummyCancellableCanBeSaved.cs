using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[PreventDelete(true)]
	sealed class DummyCancellableCanBeSaved : DummyBusinessObject, ICancellable
	{
		public DummyCancellableCanBeSaved(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ICancellable Members

		public string CanCancelMessage => null;

		public string CanCancel() => null;

		public string CanReactivateMessage => null;

		public string CanReactivate() => null;

		public bool IsCancelled
		{
			get => Z0_BitFalse;
			set => Z0_BitFalse = value;
		}

		public bool IsCancelledHasChanged => Z0_BitFalseInfo.HasChanges;

		#endregion
	}
}
