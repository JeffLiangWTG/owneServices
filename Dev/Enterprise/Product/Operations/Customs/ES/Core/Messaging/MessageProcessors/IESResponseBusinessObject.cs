using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public interface IESResponseBusinessObject : IESMessageBusinessObject
	{
		ZGuid BranchPK { get; }
		EDIMessageCollection MessageCollection { get; }
	}

	public interface IESResponseBOMessageStatus : IESResponseBusinessObject
	{
		ZString MessageStatus { set; }
	}
}
