#if DEBUG

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Subscription.Subscribers
{
	public class DummyNotRequiredSubscriber : ActualDataChangesAuditSubscriber
	{
		public override string Code => "~**";

		public override string Description => "Dummy Not Required Subscriber";

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

		public override Action<DataRow> CustomFilter => null;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override ITableSchema Table => GlbStaffSchema.Instance;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			throw new InvalidOperationException();
		}

		public override bool IsRequired() => false;
	}
}

#endif
