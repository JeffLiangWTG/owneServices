using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerErrorField : RunnerField
	{
		public RunnerErrorField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, ZString errorText)
			: base(factory, descriptor)
		{
			this.errorText = errorText;
		}

		public ZString ErrorText
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return errorText; }
		}

		public override bool ShouldApply
		{
			get { return false; }
		}

		#region IOperationalActionFieldValuePair Members

		protected override OperationalActionFieldSupporter FieldCore
		{
			get { throw new InvalidOperationException("RunnerErrorField's should not be 'applied'"); }
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ZString errorText;
	}
}
