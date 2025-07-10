using System.Diagnostics;

namespace CargoWise.Common.ErrorManagement
{
	public interface IWithRootCauseStackTrace
	{
		StackTrace RootCauseStackTrace { get; }
	}
}
