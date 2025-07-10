using System.Collections.Generic;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Security.ActiveDirectory
{
	public interface IActivationReportBuilder
	{
		IExcelInterface BuildReport(IEnumerable<EntitySynchronisedEventArgs> syncHistories);
	}
}
