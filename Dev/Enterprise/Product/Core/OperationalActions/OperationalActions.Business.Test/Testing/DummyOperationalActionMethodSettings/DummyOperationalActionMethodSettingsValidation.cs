using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public sealed class DummyOperationalActionMethodSettingsValidation : AutoDummyOperationalActionMethodSettingsValidation
	{
		public DummyOperationalActionMethodSettingsValidation(AutoDummyOperationalActionMethodSettings parent) : base(parent)
		{
		}

		protected override void CheckDefaultExcuse()
		{
			base.CheckDefaultExcuse();
			MandatoryValidation.CheckEntered(Parent.DefaultExcuseInfo);
		}

		#region Implementation
		public new DummyOperationalActionMethodSettings Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return (DummyOperationalActionMethodSettings)base.Parent;
			}
		}
		#endregion
	}
}
