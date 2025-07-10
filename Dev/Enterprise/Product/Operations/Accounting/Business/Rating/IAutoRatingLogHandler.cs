using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business
{
	interface IAutoRatingLogHandler
	{
		void QueueCARLogToCreateOnceAutoRatingFinished(IStmALogParent logsParent, ZDateTime eventTime, string reference);
	}
}
