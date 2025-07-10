using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BorderWise.Sync;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public class OrgHeaderBorderWiseSubscriber : OrgBorderWiseSubscriberBase<OrgHeaderDataObjectWithBorderWisePK>
	{
		public override string Code => "BOH";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "BorderWise Organization Header Changes Subscriber";

		public override ITableSchema Table => OrgHeaderSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		protected override string MessageSource => MessageSources.EdiProdOrgChange;

		protected override string PkMapRecordName => OrgPkMapStmDataName;

		protected override string PKColumnName => OrgHeaderSchema.Constants.PK;

		public const string OrgPkMapStmDataName = MapStmDataName.OrgPkMapStmDataName;

		protected override OrgHeaderDataObjectWithBorderWisePK GetDataObject(DataRow changeRow, DataRowVersion rowVersion, ChangeType changeType)
		{
			var orgPk = new ZGuid(changeRow[OrgHeaderSchema.Constants.PK, rowVersion]);

			var orgDataObject = new OrgHeaderDataObjectWithBorderWisePK
			{
				PK = orgPk.ToGuid(),
				Code = changeRow[OrgHeaderSchema.Constants.OH_Code, rowVersion].ToStringSafe(),
				FullName = changeRow[OrgHeaderSchema.Constants.OH_FullName, rowVersion].ToStringSafe(),
				IsActive = new ZBool(changeRow[OrgHeaderSchema.Constants.OH_IsActive, rowVersion]),
				Language = changeRow[OrgHeaderSchema.Constants.OH_Language, rowVersion].ToStringSafe(),
				SecurityRightGranted = IsRightGranted(orgPk),
			};

			if (rowVersion == DataRowVersion.Current)
			{
				orgDataObject.EdiProdLicences = GetOrgLicences(orgPk);
			}

			if (changeType == ChangeType.Add && rowVersion == DataRowVersion.Current)
			{
				orgDataObject.OrgAddresses = GetOrgAddresses(orgPk);
			}

			return orgDataObject;
		}

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			if (GetNewOrgMergeMessageHelper().IsMergeTransactionForOrgChange(changeRow, out var targetOrgPk, out var targetOrg))
			{
				PublishMergeChange(logger, changeRow, targetOrgPk, targetOrg, publisher);
			}
			else
			{
				base.PublishChange(logger, changeRow, publisher);
			}
		}

		void PublishMergeChange(ILogger logger, DataRow changeRow, Guid targetOrgPk, OrgHeader targetOrg, IBorderWiseChangesPublisher publisher)
		{
			var rowVersion = changeRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;

			var orgChangeData = new OrgChangeDataObject<OrgHeaderDataObject>();
			orgChangeData.Source = MessageSource;
			orgChangeData.ChangeType = ChangeType.Merge;
			orgChangeData.ChangeSequence = GetChangeSequence(changeRow, rowVersion);

			orgChangeData.OriginalVersion = GetDataObject(changeRow, rowVersion, ChangeType.Merge);

			orgChangeData.CurrentVersion = targetOrg != null
				? GetDataObject(((INeedRow)targetOrg).Row, DataRowVersion.Current, ChangeType.Merge)
				: new OrgHeaderDataObject { PK = targetOrgPk };

			if (targetOrg != null)
			{
				orgChangeData.CurrentVersion.EdiProdLicences = GetOrgLicences(targetOrgPk);
			}

			var message = SerializeObject(orgChangeData);
			publisher.Publish(MessageKey, message);
			logger.Information($"BorderWise Subscriber Published Message: {message}"); // Log message
		}

		ICollection<EdiProdLicenceDataObject> GetOrgLicences(ZGuid orgPk)
		{
			var orgLicences = ObjectFactory.Get<IOrgLicencesHelper>().GetOrgLicences(orgPk);
			return orgLicences.Select(l => new EdiProdLicenceDataObject { CompanyNumber = l.CompanyNumber, DatabaseNumber = l.DatabaseNumber, Product = l.Product, LicenseType = l.LicenseType }).ToList();
		}

		ICollection<OrgAddressDataObject> GetOrgAddresses(ZGuid orgPk)
		{
			var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPk);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_Language, "EN");

			var addressCapabilitySubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			addressCapabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, "OFC");
			addressCapabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, true);

			orgAddressQuery.AddSubQuery(addressCapabilitySubQuery, JoinCondition.And);
			var orgAddresses = DataFactory.Load<OrgAddress>(orgAddressQuery);

			return orgAddresses.Select(oa => new OrgAddressDataObject
			{
				PK = oa.PK.ToGuid(),
				Code = oa.OA_Code,
				Address1 = oa.OA_Address1,
				Address2 = oa.OA_Address2,
				City = oa.OA_City,
				State = oa.OA_State,
				PostCode = oa.OA_PostCode,
				CountryCode = oa.OA_RN_NKCountryCode,
				Phone = oa.OA_Phone,
				Email = oa.OA_Email,
				Language = oa.OA_Language,
				IsActive = oa.OA_IsActive,
				OrgPk = orgPk.ToGuid(),
			}).ToList();
		}

		protected virtual OrgMergeMessageHelper GetNewOrgMergeMessageHelper()
		{
			return new OrgMergeMessageHelper();
		}

		bool IsRightGranted(ZGuid contactOrgPk)
		{
			var orgRight = GetOrgSecurityRight(contactOrgPk);
			return orgRight?.OX_Granted ?? true;
		}
	}
}
