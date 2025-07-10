using System.Collections.Generic;
using System.Data;
using BorderWise.Sync;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public class ContactSecurityBorderWiseSubscriber : OrgBorderWiseSubscriberBase<OrgContactSecurityDataObjectWithBorderWisePK>
	{
		public override string Code => "BCS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "BorderWise Contact Security Changes Subscriber";

		public override ITableSchema Table => OrgSecurityContactsSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			OrgSecurityContactsSchema.OZ_OC,
			OrgSecurityContactsSchema.OZ_Granted
		};

		protected override string MessageSource => MessageSources.EdiProdContactSecurityChange;

		protected override string PkMapRecordName => OrgContactPkMapStmDataName;

		protected override string PKColumnName => OrgSecurityContactsSchema.Constants.OZ_OC;

		public const string OrgContactPkMapStmDataName = MapStmDataName.SecurityContactPkMapStmDataName;

		protected override OrgContactSecurityDataObjectWithBorderWisePK GetDataObject(DataRow changeRow, DataRowVersion rowVersion, ChangeType changeType)
		{
			var orgContactPk = new ZGuid(changeRow[OrgSecurityContactsSchema.Constants.OZ_OC, rowVersion]).ToGuid();
			return new OrgContactSecurityDataObjectWithBorderWisePK
			{
				OrgContactPK = orgContactPk,
				SecurityRightGranted = new ZBool(changeRow[OrgSecurityContactsSchema.Constants.OZ_Granted, rowVersion]),
				WebAccessEnabled = IsWebAccessEnabled(orgContactPk)
			};
		}

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			var changeType = GetChangeType(logger, changeRow);
			var rowVersion = changeType == ChangeType.Delete ? DataRowVersion.Original : DataRowVersion.Current;
			var orgSecurityPk = new ZGuid(changeRow[OrgSecurityContactsSchema.Constants.OZ_OX, rowVersion]).ToGuid();

			var orgRightQuery = new ZQuery(OrgSecuritySchema.PK, orgSecurityPk);
			orgRightQuery.AddToFilter(OrgSecuritySchema.OX_SecurityItemName, "BorderWise");

			var orgBorderWiseRight = DataFactory.LoadTop1<OrgSecurity>(orgRightQuery);
			if (orgBorderWiseRight != null)
			{
				base.PublishChange(logger, changeRow, publisher);
			}
		}

		bool IsWebAccessEnabled(ZGuid orgContactPk)
		{
			var contactQuery = new ZQuery(OrgContactSchema.PK, orgContactPk);
			var contact = DataFactory.LoadTop1<OrgContact>(contactQuery);

			return contact.OC_WebAccessEnabled;
		}
	}
}
