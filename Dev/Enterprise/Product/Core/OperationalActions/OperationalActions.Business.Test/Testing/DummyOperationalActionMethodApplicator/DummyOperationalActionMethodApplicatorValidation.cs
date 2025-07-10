using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public sealed class DummyOperationalActionMethodApplicatorValidation : AutoDummyOperationalActionMethodApplicatorValidation
	{
		public DummyOperationalActionMethodApplicatorValidation(AutoDummyOperationalActionMethodApplicator parent) : base(parent)
		{
		}

		protected override void CheckExcuse()
		{
			base.CheckExcuse();
			MandatoryValidation.CheckEntered(Parent.ExcuseInfo);
		}

		#region Implementation
		public new DummyOperationalActionMethodApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return (DummyOperationalActionMethodApplicator)base.Parent;
			}
		}
		#endregion
	}
}
