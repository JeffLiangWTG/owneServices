using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers
{
	public sealed class DocConstraintWrapper : DocumentWrapper
	{
		public static DocConstraintWrapper New(IFilterConstraint constraint)
		{
			return new DocConstraintWrapper(constraint);
		}

		DocConstraintWrapper(IFilterConstraint constraint)
			: base(constraint, null) { }

		public ZString Name
		{
			get { return Constraint.Name; }
		}

		public ZString Description
		{
			get { return Constraint.Description; }
		}

		#region Implementation

		IFilterConstraint Constraint
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (IFilterConstraint)WrappedObject; }
		}

		#endregion
	}
}
