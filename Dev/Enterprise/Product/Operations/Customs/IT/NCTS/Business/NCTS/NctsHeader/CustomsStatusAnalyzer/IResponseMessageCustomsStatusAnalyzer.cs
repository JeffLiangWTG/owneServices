using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

interface IResponseMessageCustomsStatusAnalyzer
{
	bool TryDetermineCustomsStatusBasedOnLatestMessageSent(out ZString customsStatus);
}
