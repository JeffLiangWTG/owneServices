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
	public class ContactCertificateBorderWiseSubscriber : OrgBorderWiseSubscriberBase<OrgContactCertificateDataObject>
	{
		public override string Code => "BCC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "BorderWise Contact Certificate Changes Subscriber";

		public override ITableSchema Table => GenRegCertAccredMaintListSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			GenRegCertAccredMaintListSchema.PK,
			GenRegCertAccredMaintListSchema.XZ_ParentID,
			GenRegCertAccredMaintListSchema.XZ_ParentTableCode,
			GenRegCertAccredMaintListSchema.XZ_ExpiryOrDueDate,
			GenRegCertAccredMaintListSchema.XZ_IssueDate,
			GenRegCertAccredMaintListSchema.XZ_IsValid,
			GenRegCertAccredMaintListSchema.XZ_Comment,
			GenRegCertAccredMaintListSchema.XZ_Type
		};

		protected override string MessageSource => MessageSources.EdiProdContactCertificateChange;

		protected override string PKColumnName => GenRegCertAccredMaintListSchema.Constants.PK;

		protected override string PkMapRecordName => string.Empty;

		protected override OrgContactCertificateDataObject GetDataObject(DataRow changeRow, DataRowVersion rowVersion, ChangeType changeType)
		{
			return new OrgContactCertificateDataObject
			{
				PK = new ZGuid(changeRow[GenRegCertAccredMaintListSchema.Constants.PK, rowVersion]).ToGuid(),
				ContactFk = new ZGuid(changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_ParentID, rowVersion]).ToGuid(),
				Comment = changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_Comment, rowVersion].ToStringSafe().Trim(),
				ExpiryOrDueDateUtc = GetDateTimeValue(new ZDateTime(changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_ExpiryOrDueDate, rowVersion])),
				IssueDateUtc = GetDateTimeValue(new ZDateTime(changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_IssueDate, rowVersion])),
				Type = changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_Type, rowVersion].ToStringSafe().Trim(),
				IsValid = new ZBool(changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_IsValid, rowVersion]),
				RefNumber = changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_RefNumber, rowVersion].ToStringSafe().Trim(),
			};
		}

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			var changeType = GetChangeType(logger, changeRow);
			var rowVersion = changeType == ChangeType.Delete ? DataRowVersion.Original : DataRowVersion.Current;
			var parentTableCode = changeRow[GenRegCertAccredMaintListSchema.Constants.XZ_ParentTableCode, rowVersion].ToStringSafe().Trim();

			if (parentTableCode == OrgContactSchema.Constants.Prefix)
			{
				base.PublishChange(logger, changeRow, publisher);
			}
		}
	}
}
