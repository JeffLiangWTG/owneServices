using System.Collections.Generic;
using System.Data;
using BorderWise.Sync;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public class OrgSecurityBorderWiseSubscriber : OrgBorderWiseSubscriberBase<OrgSecurityDataObjectWithBorderWisePK>
	{
		public override string Code => "BOS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "BorderWise Organization Security Changes Subscriber";

		public override ITableSchema Table => OrgSecuritySchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			OrgSecuritySchema.OX_OH,
			OrgSecuritySchema.OX_Granted
		};

		protected override string MessageSource => MessageSources.EdiProdOrgSecurityChange;

		protected override string PkMapRecordName => OrgPkMapStmDataName;

		protected override string PKColumnName => OrgSecuritySchema.Constants.OX_OH;

		public const string OrgPkMapStmDataName = MapStmDataName.SecurityOrgPkMapStmDataName;

		protected override OrgSecurityDataObjectWithBorderWisePK GetDataObject(DataRow changeRow, DataRowVersion rowVersion, ChangeType changeType)
		{
			return new OrgSecurityDataObjectWithBorderWisePK
			{
				OrgHeaderPK = new ZGuid(changeRow[OrgSecuritySchema.Constants.OX_OH, rowVersion]).ToGuid(),
				SecurityRightGranted = new ZBool(changeRow[OrgSecuritySchema.Constants.OX_Granted, rowVersion]),
			};
		}

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			var changeType = GetChangeType(logger, changeRow);
			var rowVersion = changeType == ChangeType.Delete ? DataRowVersion.Original : DataRowVersion.Current;
			var securityItemName = changeRow[OrgSecuritySchema.Constants.OX_SecurityItemName, rowVersion].ToStringSafe().Trim();

			if (securityItemName == "BorderWise")
			{
				base.PublishChange(logger, changeRow, publisher);
			}
		}
	}
}
