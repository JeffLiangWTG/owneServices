using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class GEIEServicesDelivery : EServicesDelivery
	{
		protected override IEnumerable<IStmALogParent> GetInterestedLogParents(IStmALogParent triggerBOWithLogs)
		{
			var interestedLogParents = GEIMessageLinkerHelper.GetLogParentsToLinkWithGEIMessage(triggerBOWithLogs).ToList();
			interestedLogParents.Add(triggerBOWithLogs);
			return interestedLogParents;
		}
	}
}
