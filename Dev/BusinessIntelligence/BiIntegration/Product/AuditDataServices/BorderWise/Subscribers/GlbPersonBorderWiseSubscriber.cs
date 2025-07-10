using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BorderWise.Sync;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public class GlbPersonBorderWiseSubscriber : BorderWiseSubscriberBase
	{
		public override string Code => "BOP";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "BorderWise Person Changes Subscriber";

		public override ITableSchema Table => GlbPersonSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			GlbPersonSchema.PER_PasswordHash,
			GlbPersonSchema.PER_PasswordHashIterations,
			GlbPersonSchema.PER_PasswordSalt,
		};

		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		protected override string MessageSource => MessageSources.EdiProdPersonChange;

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			var changeType = GetChangeType(logger, changeRow);
			if (changeType != ChangeType.Update)
			{
				return;
			}

			var orgChangeData = new OrgChangeDataObject<GlbPersonDataObject>
			{
				Source = MessageSource,
				ChangeType = changeType,
				ChangeSequence = GetChangeSequence(changeRow, DataRowVersion.Current)
			};

			var key = MessageKey;
			orgChangeData.OriginalVersion = GetDataObject(changeRow, DataRowVersion.Original);
			orgChangeData.CurrentVersion = GetDataObject(changeRow, DataRowVersion.Current);

			var message = SerializeObject(orgChangeData);
			publisher.Publish(key.ToStringSafe().Trim(), message);
			logger.Information($"BorderWise Subscriber Published Message: {message}"); // Log message
		}

		protected GlbPersonDataObject GetDataObject(DataRow changeRow, DataRowVersion rowVersion)
		{
			var personPK = new ZGuid(changeRow[GlbPersonSchema.Constants.PK, rowVersion]).ToGuid();

			return new GlbPersonDataObject
			{
				OC_PKs = GetContactPKs(personPK).ToList(),
				PasswordHash = new ZBlob(changeRow[GlbPersonSchema.Constants.PER_PasswordHash, rowVersion]),
				PasswordSalt = new ZBlob(changeRow[GlbPersonSchema.Constants.PER_PasswordSalt, rowVersion]),
				PasswordHashIterations = new ZInt(changeRow[GlbPersonSchema.Constants.PER_PasswordHashIterations, rowVersion]),
			};
		}

		IEnumerable<Guid> GetContactPKs(Guid personPK)
		{
			var contactsQuery = new ZQuery(OrgContactSchema.OC_PER, personPK);

			return DataFactory.Load<OrgContact>(contactsQuery).Select(x => x.PK.ToGuid());
		}
	}
}
