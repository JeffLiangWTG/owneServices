using CargoWise.Common;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CA.Business;

public class OperationalActionSectionLogWrapper : ILVXJobsConsolidateRunnerLog
{
	public OperationalActionSectionLogWrapper(IOperationalActionSectionLog log)
	{
		this.log = Argument.NotNull(log, "log");
	}
#if DEBUG
	public
#endif
		readonly IOperationalActionSectionLog log;

	public void BumpSectionProgress()
	{
		log.BumpSectionProgress();
	}

	public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
	{
		log.NotifyFormat(errorLevel, format, args);
	}

	public void SetSectionProgressMax(int max)
	{
		log.SetSectionProgressMax(max);
	}
}
