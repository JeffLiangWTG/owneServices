using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public interface ILVXJobsConsolidateRunnerLog
	{
		void BumpSectionProgress();
		void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args);
		void SetSectionProgressMax(int max);
	}
}
