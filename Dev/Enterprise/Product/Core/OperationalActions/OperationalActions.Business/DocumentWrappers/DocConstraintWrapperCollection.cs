using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Services.OperationalActions.Business.DocumentWrappers
{
	public sealed class DocConstraintWrapperCollection : DocumentWrapperCollection<DocConstraintWrapper>
	{
		public static DocConstraintWrapperCollection New()
		{
			return New(EnvironmentFilterProvider.Instance);
		}

		public static DocConstraintWrapperCollection New(IEnumerable<IFilterConstraint> constraints)
		{
			return new DocConstraintWrapperCollection(constraints);
		}

		DocConstraintWrapperCollection(IEnumerable<IFilterConstraint> constraints)
			: base(constraints, null) { }

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return DocConstraintWrapper.New((IFilterConstraint)objectToWrap);
		}
	}
}
