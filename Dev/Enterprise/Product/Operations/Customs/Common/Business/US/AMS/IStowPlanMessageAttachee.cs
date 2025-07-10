using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Common.US.AMS
{
	public interface IStowPlanMessageAttachee
	{
		ZString StowPlanMessageStatus { get; set; }
		EDIMessageCollection Messages { get; }
	}
}
