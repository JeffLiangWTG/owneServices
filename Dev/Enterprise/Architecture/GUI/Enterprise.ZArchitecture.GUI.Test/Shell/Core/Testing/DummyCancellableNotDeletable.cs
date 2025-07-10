using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[PreventDelete(false)]
	sealed class DummyCancellableNotDeletable : DummyBusinessObject, ICancellable
	{
		public DummyCancellableNotDeletable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => base.HumanReadableNameCore + "CancellableNotDeletable";

		#region ICanDelete

		public override bool CanDelete => false;

		public override MultilingualString ReasonForNotAbleToDelete => (NoResString)"Just because";

		#endregion

		#region ICancellable Members

		public string CanCancel() => "This Dummy BizO can't be cancelled.";

		public string CanReactivate() => null;

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
