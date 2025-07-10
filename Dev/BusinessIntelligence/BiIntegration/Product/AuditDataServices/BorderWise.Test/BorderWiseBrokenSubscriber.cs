using System;
using System.Collections.Generic;
using System.Data;
using BorderWise.Sync;
using CargoWise.Schema;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	class BorderWiseBrokenSubscriber : BorderWiseSubscriberBase
	{
		public override string Code => "BRK";

		public override string Description => "BorderWise Broken Subscriber For Test";

		public override ITableSchema Table => null;

		public override IEnumerable<SchemaColumn> SpecificColumns => Array.Empty<SchemaColumn>();

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

		protected override string MessageSource => MessageSources.EdiProdPersonChange;

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			throw new NotImplementedException();
		}
	}
}
