using System.Collections.Generic;
using System.Data;
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
	public class OrgContactBorderWiseSubscriber : OrgBorderWiseSubscriberBase<OrgContactDataObjectWithBorderWisePK>
	{
		public override string Code => "BOC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "BorderWise Organization Contact Changes Subscriber";

		public override ITableSchema Table => OrgContactSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[]
		{
			OrgContactSchema.OC_ContactName,
			OrgContactSchema.OC_Title,
			OrgContactSchema.OC_IsActive,
			OrgContactSchema.OC_Language,
			OrgContactSchema.OC_Phone,
			OrgContactSchema.OC_PhoneExtension,
			OrgContactSchema.OC_Mobile,
			OrgContactSchema.OC_Email,
			OrgContactSchema.OC_Birthday,
			OrgContactSchema.OC_Gender,
			OrgContactSchema.OC_RN_NKNationality,
			OrgContactSchema.OC_OH,
			OrgContactSchema.OC_PasswordHash,
			OrgContactSchema.OC_PasswordHashIterations,
			OrgContactSchema.OC_PasswordSalt,
			OrgContactSchema.OC_WebAccessEnabled,
			OrgContactSchema.OC_PER
		};

		protected override string MessageSource => MessageSources.EdiProdContactChange;

		protected override string PkMapRecordName => ContactPkMapStmDataName;

		protected override string PKColumnName => OrgContactSchema.Constants.PK;

		public const string ContactPkMapStmDataName = MapStmDataName.ContactPkMapStmDataName;

		protected override OrgContactDataObjectWithBorderWisePK GetDataObject(DataRow changeRow, DataRowVersion rowVersion, ChangeType changeType)
		{
			var pk = new ZGuid(changeRow[OrgContactSchema.Constants.PK, rowVersion]).ToGuid();
			var orgFk = new ZGuid(changeRow[OrgContactSchema.Constants.OC_OH, rowVersion]).ToGuid();

			var (passwordHash, passwordSalt, hashIterations) = GetPasswordDetails();

			return new OrgContactDataObjectWithBorderWisePK
			{
				PK = pk,
				ContactName = GetRowStringValue(OrgContactSchema.Constants.OC_ContactName),
				Title = GetRowStringValue(OrgContactSchema.Constants.OC_Title),
				IsActive = GetRowBoolValue(OrgContactSchema.Constants.OC_IsActive),
				Language = GetRowStringValue(OrgContactSchema.Constants.OC_Language),
				Phone = GetRowStringValue(OrgContactSchema.Constants.OC_Phone),
				PhoneExtension = GetRowStringValue(OrgContactSchema.Constants.OC_PhoneExtension),
				Mobile = GetRowStringValue(OrgContactSchema.Constants.OC_Mobile),
				Email = GetRowStringValue(OrgContactSchema.Constants.OC_Email),
				Birthday = GetDateTimeValue(new ZDateTime(changeRow[OrgContactSchema.Constants.OC_Birthday, rowVersion])),
				Gender = GetRowStringValue(OrgContactSchema.Constants.OC_Gender),
				Nationality = GetRowStringValue(OrgContactSchema.Constants.OC_RN_NKNationality),
				OrgFk = orgFk,
				PasswordHash = passwordHash,
				PasswordSalt = passwordSalt,
				PasswordHashIterations = hashIterations,
				WebAccessEnabled = GetRowBoolValue(OrgContactSchema.Constants.OC_WebAccessEnabled),
				SecurityRightGranted = IsRightGrantedWithoutCache(DataFactory, pk, orgFk),
			};

			object GetRowValue(string columnName) => changeRow[columnName, rowVersion];
			string GetRowStringValue(string columnName) => GetRowValue(columnName).ToStringSafe();
			bool GetRowBoolValue(string columnName) => new ZBool(GetRowValue(columnName) ?? false);
			ZBlob GetRowBlobValue(string columnName) => new ZBlob(GetRowValue(columnName));

			(byte[] PasswordHash, byte[] PasswordSalt, int HashIterations) GetPasswordDetails()
			{
				var zdbOnlyQuery = new ZDBOnlyQuery(typeof(OrgContact));
				zdbOnlyQuery.AddToFilter(OrgContactSchema.PK, pk);
				var person = changeType == ChangeType.Delete ? null : DataFactory.LoadTop1<OrgContact>(zdbOnlyQuery)?.Person;
				person?.ReloadSafe();
				var hasPersonPassword = person?.HasPassword ?? false;

				if (hasPersonPassword)
				{
					return (person.PER_PasswordHash, person.PER_PasswordSalt, person.PER_PasswordHashIterations);
				}
				else
				{
					return (
						GetRowBlobValue(OrgContactSchema.Constants.OC_PasswordHash),
						GetRowBlobValue(OrgContactSchema.Constants.OC_PasswordSalt),
						new ZInt(GetRowValue(OrgContactSchema.Constants.OC_PasswordHashIterations))
						);
				}
			}
		}

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			if (!GetNewOrgMergeMessageHelper().IsMergeTransactionForContactChange(changeRow))
			{
				base.PublishChange(logger, changeRow, publisher);
			}
		}

		protected virtual OrgMergeMessageHelper GetNewOrgMergeMessageHelper()
		{
			return new OrgMergeMessageHelper();
		}

		//TODO: The web authorization logic could be moved to a shared project
		bool IsRightGrantedWithoutCache(BusinessObjectFactory factory, ZGuid contactPK, ZGuid contactOrgPK)
		{
			var orgRight = GetOrgSecurityRight(contactOrgPK);
			if (orgRight == null)
			{
				return true;
			}

			var contactRightQuery = new ZQuery(OrgSecurityContactsSchema.OZ_OC, contactPK)
				.AddToFilter(OrgSecurityContactsSchema.OZ_OX, orgRight.PK);
			var contactRight = factory.LoadTop1<OrgSecurityContacts>(contactRightQuery);

			return contactRight?.OZ_Granted ?? orgRight.OX_Granted;
		}
	}
}
