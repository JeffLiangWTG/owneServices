#if DEBUG

namespace Enterprise.AuditDataServices.Subscription.Subscribers
{
	using System.Collections.Generic;
	using System.Globalization;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Schema;

	public class DummyChangedTableListSubscriber : ChangedTableListOnlyAuditSubscriber
	{
		public override bool IsRequired()
		{
			return true;
		}

		public override IEnumerable<ITableSchema> SubscribedTables
		{
			get
			{
				yield return GlbStaffSchema.Instance;
				yield return JobShipmentSchema.Instance;
				yield return OrgAddressSchema.Instance;
			}
		}

		public override string Code => "^CT";

		public override string Description => "Dummy Changed Table List Subscriber";

		public override void ProcessChanges(ILogger logger, IEnumerable<ITableSchema> changedTables)
		{
			foreach (var changedTable in changedTables)
			{
				logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", changedTable.SqlSchemaName, changedTable.TableName));
			}
		}
	}
}

#endif
