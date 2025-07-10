using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.Integration.MasterFiles
{
	public interface IWorkflowMacroEvaluator
	{
		(IZType result, IEnumerable<IReportError> errors) EvaluateMacros(IBusiness[] businessObjects, ZString macrosValuePath);
	}
}
