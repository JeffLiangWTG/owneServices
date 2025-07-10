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
	public class OrgAddressBorderWiseSubscriber : OrgBorderWiseSubscriberBase<OrgAddressDataObjectWithBorderWisePK>
	{
		public override string Code => "BOA";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "BorderWise Organization Address Changes Subscriber";

		public override ITableSchema Table => OrgAddressSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			OrgAddressSchema.OA_Code,
			OrgAddressSchema.OA_Address1,
			OrgAddressSchema.OA_Address2,
			OrgAddressSchema.OA_City,
			OrgAddressSchema.OA_State,
			OrgAddressSchema.OA_PostCode,
			OrgAddressSchema.OA_RN_NKCountryCode,
			OrgAddressSchema.OA_Phone,
			OrgAddressSchema.OA_Email,
			OrgAddressSchema.OA_Language,
			OrgAddressSchema.OA_IsActive,
			OrgAddressSchema.OA_OH,
		};

		protected override string MessageSource => MessageSources.EdiProdAddressChange;

		protected override string PkMapRecordName => AddressPkMapStmDataName;

		protected override string PKColumnName => OrgAddressSchema.Constants.PK;

		public const string AddressPkMapStmDataName = MapStmDataName.AddressPkMapStmDataName;

		protected override OrgAddressDataObjectWithBorderWisePK GetDataObject(DataRow changeRow, DataRowVersion rowVersion, ChangeType changeType)
		{
			return new OrgAddressDataObjectWithBorderWisePK
			{
				PK = new ZGuid(changeRow[OrgAddressSchema.Constants.PK, rowVersion]).ToGuid(),
				Code = changeRow[OrgAddressSchema.Constants.OA_Code, rowVersion].ToStringSafe(),
				Address1 = changeRow[OrgAddressSchema.Constants.OA_Address1, rowVersion].ToStringSafe(),
				Address2 = changeRow[OrgAddressSchema.Constants.OA_Address2, rowVersion].ToStringSafe(),
				City = changeRow[OrgAddressSchema.Constants.OA_City, rowVersion].ToStringSafe(),
				State = changeRow[OrgAddressSchema.Constants.OA_State, rowVersion].ToStringSafe(),
				PostCode = changeRow[OrgAddressSchema.Constants.OA_PostCode, rowVersion].ToStringSafe(),
				CountryCode = changeRow[OrgAddressSchema.Constants.OA_RN_NKCountryCode, rowVersion].ToStringSafe(),
				Phone = changeRow[OrgAddressSchema.Constants.OA_Phone, rowVersion].ToStringSafe(),
				Email = changeRow[OrgAddressSchema.Constants.OA_Email, rowVersion].ToStringSafe(),
				Language = changeRow[OrgAddressSchema.Constants.OA_Language, rowVersion].ToStringSafe(),
				IsActive = new ZBool(changeRow[OrgAddressSchema.Constants.OA_IsActive, rowVersion]),
				OrgPk = new ZGuid(changeRow[OrgAddressSchema.Constants.OA_OH, rowVersion]).ToGuid(),
			};
		}

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			if (!GetNewOrgMergeMessageHelper().IsMergeTransactionForAddressChange(changeRow))
			{
				base.PublishChange(logger, changeRow, publisher);
			}
		}

		protected virtual OrgMergeMessageHelper GetNewOrgMergeMessageHelper()
		{
			return new OrgMergeMessageHelper();
		}
	}
}
