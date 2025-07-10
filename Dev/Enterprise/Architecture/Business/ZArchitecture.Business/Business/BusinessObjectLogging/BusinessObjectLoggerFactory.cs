using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	class BusinessObjectLoggerFactory
	{
		internal static IEnumerable<IBusinessObjectLogger> GetLoggers(BusinessObject businessObject)
		{
			if (businessObject is IAutoAdminLogTarget logTarget && (logTarget.IsAutoAdminBusinessObjectLoggerEnabled || logTarget.IsAutoLogOnlyEnabledForACT))
			{
				yield return new AutoAdminBusinessObjectLogger();
			}
		}
	}
}
