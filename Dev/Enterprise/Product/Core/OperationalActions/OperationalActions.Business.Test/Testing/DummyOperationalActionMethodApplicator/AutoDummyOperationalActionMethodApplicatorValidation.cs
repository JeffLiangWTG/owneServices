using System;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[System.CodeDom.Compiler.GeneratedCode("NonPersistentBusinessObjectGenerator", "1.0")]
	public abstract class AutoDummyOperationalActionMethodApplicatorValidation : ZValidation
	{
		public AutoDummyOperationalActionMethodApplicatorValidation(AutoDummyOperationalActionMethodApplicator parent) : base(parent)
		{
			this.parent = parent;
			this.ZValidationInternals = this;
			this.ParentListInternals = parent;
		}

		public void Add(AutoDummyOperationalActionMethodApplicatorValidation validation)
		{
			ZValidationInternals.Add(validation);
		}

		public void Remove(AutoDummyOperationalActionMethodApplicatorValidation validation)
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
			ValidateExcuse();
		}

#endregion
#region Excuse
		public void ValidateExcuse()
		{
			ZValidationInternals.Validate(Parent.ExcuseInfo, new RunValidationInvoker(this.ExcuseValidationInvoker));
		}

		void ExcuseValidationInvoker()
		{
			CheckExcuseIsWesternEuropean();
			CheckExcuse();
		}

		protected virtual void CheckExcuseIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.ExcuseInfo);
		}

		protected virtual void CheckExcuse()
		{
		}

#endregion
#region Implementation
		public override Type AutoValidationType
		{
			get
			{
				return typeof(AutoDummyOperationalActionMethodApplicatorValidation);
			}
		}

		public AutoDummyOperationalActionMethodApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}

		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		AutoDummyOperationalActionMethodApplicator parent;
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		IValidationInternals ZValidationInternals;
		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		ISingleElementListInternal ParentListInternals;
#endregion
	}
}
