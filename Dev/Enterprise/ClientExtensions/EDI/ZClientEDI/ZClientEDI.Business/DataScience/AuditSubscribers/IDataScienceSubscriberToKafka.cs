using System;
using System.Collections.Generic;
using Enterprise.AuditDataServices.Subscription.Common;
using WTG.Serialization.DataScience.Audit.ObjectModel;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers
{
	public interface IDataScienceSubscriberToKafka : IActualDataChangesAuditSubscriber
	{
		ISubscriberDataSchema DataSchema { get; }
		IReadOnlyList<ColumnInfo> ColumnInfos { get; }
		Guid SessionId { get; set; }
	}
}
