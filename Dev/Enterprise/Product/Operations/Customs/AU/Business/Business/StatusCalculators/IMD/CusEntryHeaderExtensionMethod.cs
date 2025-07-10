using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business;

public static class CusEntryHeaderExtensionMethod
{
	public static IEnumerable<EDIMessage> GetRelatedDSAMessages(this CusEntryHeader entry)
	{
		return ((Customs.Business.IStatusNeedsRecalculationProvider)entry)?.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == CMRMessage.CMRMessageTypes.DSA);
	}
}
