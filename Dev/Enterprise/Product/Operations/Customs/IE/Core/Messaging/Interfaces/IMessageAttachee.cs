using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public interface IMessageAttachee
	{
		ZGuid PK { get; }
		string TableName { get; }
		string TablePrefix { get; }
		GlbBranch Branch { get; }
		IRelatedJob RelatedJob { get; }
		GlbStaff CustomsAgent { get; }
		BusinessObjectFactory Factory { get; }
		ZString LogicalStatus { get; set; }
		ZString EntryStatus { get; set; }
		ZString MovementReferenceNumber { get; }
		IEnumerable<EDIMessage> Messages { get; }
	}
}
