using System.Collections.Generic;
using CargoWise.Schema;
using WTG.Serialization.DataScience.Audit.ObjectModel;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers
{
	public interface ISubscriberDataSchema
	{
		int DataSchemaVersion { get; }
		IEnumerable<SchemaColumn> BizObjColumns { get; }
		IEnumerable<SchemaColumn> LegacyColumns { get; }
		IEnumerable<ColumnInfo> NonBizObjColumns { get; }
	}
}
