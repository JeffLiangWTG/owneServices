using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Issues
{
	public sealed class HelpErrorLogOccurrenceSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Issues>
	{
		public override string Code => SubscriberCodes.HelpErrorLogOccurrenceSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => HelpErrorLogOccurrenceSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			HelpErrorLogOccurrenceSchema.PK,
			HelpErrorLogOccurrenceSchema.HO_Company,
			HelpErrorLogOccurrenceSchema.HO_ExceptionDateTime,
			HelpErrorLogOccurrenceSchema.HO_ExceptionID,
			HelpErrorLogOccurrenceSchema.HO_EXEDateTime,
			HelpErrorLogOccurrenceSchema.HO_HE,
			HelpErrorLogOccurrenceSchema.HO_HL,
			HelpErrorLogOccurrenceSchema.HO_LCC,
			HelpErrorLogOccurrenceSchema.HO_LD,
			HelpErrorLogOccurrenceSchema.HO_Sequence,
			HelpErrorLogOccurrenceSchema.HO_ServerName,
			HelpErrorLogOccurrenceSchema.HO_SessionID,
			HelpErrorLogOccurrenceSchema.HO_VersionNumber
		};
	}
}
