using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public interface IExceptionCollectionParent
	{
		List<WorkflowException> ExceptionCollection { get; }
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool SetExceptionCollection(Func<List<WorkflowException>> getter);
	}
}
