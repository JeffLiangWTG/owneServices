using System;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[System.CodeDom.Compiler.GeneratedCode("NonPersistentBusinessObjectGenerator", "1.0")]
	public abstract class AutoDummyOperationalActionMethodSettingsValidation : ZValidation
	{
		public AutoDummyOperationalActionMethodSettingsValidation(AutoDummyOperationalActionMethodSettings parent) : base(parent)
		{
			this.parent = parent;
			this.ZValidationInternals = this;
			this.ParentListInternals = parent;
		}

		public void Add(AutoDummyOperationalActionMethodSettingsValidation validation)
		{
			ZValidationInternals.Add(validation);
		}

		public void Remove(AutoDummyOperationalActionMethodSettingsValidation validation)
		{
			ZValidationInternals.Remove(validation);
		}

#region ValidateAll
		public override void ValidateAll()
		{
			IDisposable suspender = ParentListInternals.SuspendListChanged();
			try
			{
				ValidateAllCore();
			}
			finally
			{
				suspender.Dispose();
			}
		}

		protected virtual void ValidateAllCore()
		{
			ValidateDefaultExcuse();
			ValidateLockExcuse();
		}

#endregion
#region DefaultExcuse
		public void ValidateDefaultExcuse()
		{
			ZValidationInternals.Validate(Parent.DefaultExcuseInfo, new RunValidationInvoker(this.DefaultExcuseValidationInvoker));
		}

		void DefaultExcuseValidationInvoker()
		{
			CheckDefaultExcuseIsWesternEuropean();
			CheckDefaultExcuse();
		}

		protected virtual void CheckDefaultExcuseIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.DefaultExcuseInfo);
		}

		protected virtual void CheckDefaultExcuse()
		{
		}

#endregion
#region LockExcuse
		public void ValidateLockExcuse()
		{
			ZValidationInternals.Validate(Parent.LockExcuseInfo, new RunValidationInvoker(this.CheckLockExcuse));
		}

		protected virtual void CheckLockExcuse()
		{
		}

#endregion
#region Implementation
		public override Type AutoValidationType
		{
			get
			{
				return typeof(AutoDummyOperationalActionMethodSettingsValidation);
			}
		}

		public AutoDummyOperationalActionMethodSettings Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}

		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		AutoDummyOperationalActionMethodSettings parent;
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		IValidationInternals ZValidationInternals;
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		ISingleElementListInternal ParentListInternals;
#endregion
	}
}
