using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public interface INctsMessageSendingObject
{
	NctsHeader NctsHeader { get; }

	ZString MessageIdentification { get; }
}
