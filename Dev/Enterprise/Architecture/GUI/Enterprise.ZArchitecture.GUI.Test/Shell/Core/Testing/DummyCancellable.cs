using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[PreventDelete(true)]
	sealed class DummyCancellable : DummyBusinessObject, ICancellable
	{
		public DummyCancellable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => base.HumanReadableNameCore + "Cancellable";

		#region ICancellable Members

		string canCancelMessage = "This Dummy BizO can't be cancelled.";
		public string CanCancelMessage
		{
			get => canCancelMessage;
			set => canCancelMessage = value;
		}

		public string CanCancel() => canCancelMessage;

		string canReactivateMessage;
		public string CanReactivateMessage
		{
			get => canReactivateMessage;
			set => canReactivateMessage = value;
		}

		public string CanReactivate() => canReactivateMessage;

		public bool IsCancelled
		{
			get => isCancelled;
			set => isCancelled = value;
		}
		bool isCancelled;

		public bool IsCancelledHasChanged => false;

		#endregion
	}
}
