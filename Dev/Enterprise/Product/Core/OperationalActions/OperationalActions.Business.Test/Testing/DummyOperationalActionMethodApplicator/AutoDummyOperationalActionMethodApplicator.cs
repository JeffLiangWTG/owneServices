using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[System.CodeDom.Compiler.GeneratedCode("NonPersistentBusinessObjectGenerator", "1.0")]
	public abstract class AutoDummyOperationalActionMethodApplicator : Enterprise.Services.OperationalActions.Support.OperationalActionMethodApplicator
	{
#region Schema
		public class Schema
		{
			public const string Excuse = "Excuse";
			public const int ExcuseMaxLength = 35;
		}

#endregion
		protected AutoDummyOperationalActionMethodApplicator(string name) : base(name)
		{
		}

		protected AutoDummyOperationalActionMethodApplicator(string name, BusinessObjectFactory factory) : base(name, factory)
		{
		}

#region Excuse
		[ReadOnlyMemberAttribute("Excuse_ReadOnly")]
		[MaxLength(Schema.ExcuseMaxLength)]
		public virtual ZString Excuse
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return excuse;
			}

			set
			{
				CheckMaximumLength(ExcuseInfo, value);
				SetNonPersistentPropertyValue(ExcuseInfo, ref excuse, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					Validation.ValidateExcuse();
				}
			}
		}

		public virtual ZPropertyInfo ExcuseInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.Excuse);
			}
		}

		protected virtual bool Excuse_ReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return false;
			}
		}

		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZString excuse;
#endregion
#region Validation
		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public DummyOperationalActionMethodApplicatorValidation Validation
		{
			get
			{
				return GetNewValidation();
			}
		}

		protected virtual DummyOperationalActionMethodApplicatorValidation GetNewValidation()
		{
			return new DummyOperationalActionMethodApplicatorValidation(this);
		}
#endregion
	}
}
