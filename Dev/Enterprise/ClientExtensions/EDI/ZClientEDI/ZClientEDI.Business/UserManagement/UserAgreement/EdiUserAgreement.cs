using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	[CodeProperty("UniqueIdentifierForCodeProperty")]
	public class EdiUserAgreement : AutoEdiUserAgreement, IDocManagerSupport
	{
		public EdiUserAgreement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoEdiUserAgreement.Schema
		{
			public const string Level = "Level";
		}

		#region Properties

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var name = ERA_Type;

				if (!ERA_RN_NKCountryCode.IsEmpty)
				{
					name += " " + ERA_RN_NKCountryCode;
				}

				if (!ERA_Title.IsEmpty)
				{
					name += " - " + ERA_Title;
				}

				return name;
			}
		}

		[ReadOnlyMember(nameof(AgreementDetailsReadOnly))]
		public override ZString ERA_Title
		{
			get => base.ERA_Title;
			set => base.ERA_Title = value;
		}

		[ReadOnlyMember(nameof(AgreementDetailsReadOnly))]
		public override ZString ERA_Content
		{
			get => base.ERA_Content;
			set => base.ERA_Content = value;
		}

		[ReadOnlyMember(nameof(AgreementDetailsReadOnly))]
		public override ZDateTime ERA_EffectiveTimeUtc
		{
			get => base.ERA_EffectiveTimeUtc;
			set => base.ERA_EffectiveTimeUtc = value;
		}

		[ReadOnlyMember(nameof(AgreementDetailsReadOnly))]
		public ZDateTime EffectiveTimeLocal
		{
			get => ERA_EffectiveTimeUtc.ToLocalBranchTime(Factory);
			set => ERA_EffectiveTimeUtc = value.ToUniversalBranchTime(Factory);
		}

		public virtual ZPropertyInfo EffectiveTimeLocalInfo => GetWrappedZPropertyInfo(nameof(EffectiveTimeLocal), x => ERA_EffectiveTimeUtcInfo);

		[ReadOnly(true)]
		public override ZInt ERA_VersionNumber
		{
			get => base.ERA_VersionNumber;
			set => base.ERA_VersionNumber = value;
		}

		[ReadOnlyMember(nameof(ERA_RN_NKCountryCodeReadOnly))]
		public override ZString ERA_RN_NKCountryCode
		{
			get => base.ERA_RN_NKCountryCode;
			set
			{
				base.ERA_RN_NKCountryCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateERA_VersionNumber();
					Validation.ValidateERA_EffectiveTimeUtc();
				}
			}
		}

		public bool ERA_RN_NKCountryCodeReadOnly => AgreementDetailsReadOnly || EdiUserAgreementTypesMapper.IsVariantEnabled(ERA_Type);

		[ReadOnlyMember(nameof(AgreementDetailsReadOnly))]
		[List("Lookups.Types")]
		public override ZString ERA_Type
		{
			get => base.ERA_Type;
			set
			{
				base.ERA_Type = value;

				if (EdiUserAgreementTypesMapper.IsVariantEnabled(base.ERA_Type))
				{
					ERA_RN_NKCountryCode = ZString.Empty;
				}
				else
				{
					ERA_VariantCode = ZString.Empty;
					ERA_VariantDescription = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateERA_VersionNumber();
					Validation.ValidateERA_Type();
					Validation.ValidateERA_EffectiveTimeUtc();
				}

				LevelInfo.RefreshBinding();
			}
		}

		[List("Lookups.Variants")]
		[ReadOnlyMember(nameof(ERA_VariantCodeReadOnly))]
		[ResourceStringData("EdiUserAgreement|Variant", Caption = "Variant")]
		public override ZString ERA_VariantCode
		{
			get => base.ERA_VariantCode;
			set
			{
				var oldValue = base.ERA_VariantCode;
				base.ERA_VariantCode = value;
				if (oldValue != value && Lookups.Variants.ContainsCode(value))
				{
					ERA_VariantDescription = Lookups.Variants.GetDescriptionFromCode(value);
					CloneAgreementContentIfNew();
				}

				ERA_VariantDescriptionInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		void CloneAgreementContentIfNew()
		{
			if (!IsInDatabase)
			{
				var previousContentQuery = new ZQuery(EdiUserAgreementSchema.ERA_Type, ERA_Type);
				previousContentQuery.AddToFilter(EdiUserAgreementSchema.ERA_VariantCode, ERA_VariantCode);
				previousContentQuery.AddToFilter(EdiUserAgreementSchema.ERA_IsActive, true);
				previousContentQuery.AddToFilter(EdiUserAgreementSchema.ERA_RN_NKCountryCode, ZString.Empty);
				previousContentQuery.AddToFilter(EdiUserAgreementSchema.PK, SQLComparisonOperator.NotEqual, PK);
				previousContentQuery.AddToFilter(EdiUserAgreementSchema.ERA_EffectiveTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
				previousContentQuery.OrderBy = EdiUserAgreementSchema.Constants.ERA_VersionNumber + OrderByClause.Descending;

				var previousAgreement = Factory.LoadTop1<EdiUserAgreement>(previousContentQuery);
				if (!string.IsNullOrEmpty(previousAgreement?.ERA_Content))
				{
					ERA_Content = previousAgreement.ERA_Content;
				}
			}
		}

		[ReadOnly(true)]
		public override ZInt ERA_MinorVersion
		{
			get => base.ERA_MinorVersion;
			set
			{
				base.ERA_MinorVersion = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateERA_VersionNumber();
				}
			}
		}

		[ReadOnlyMember(nameof(ERA_VariantDescriptionReadOnly))]
		public override ZString ERA_VariantDescription { get => base.ERA_VariantDescription; set => base.ERA_VariantDescription = value; }
		public bool ERA_VariantCodeReadOnly => AgreementDetailsReadOnly || !EdiUserAgreementTypesMapper.IsVariantEnabled(ERA_Type);

		public bool ERA_VariantDescriptionReadOnly => AgreementDetailsReadOnly || ERA_VariantCodeReadOnly || Lookups.Variants.ContainsCode(ERA_VariantCode);

		[List("Lookups.Levels")]
		[ResourceStringData("EdiUserAgreement|Level", Caption = "Level", FullDescription = "Agreement Level")]
		public ZString Level => EdiUserAgreementTypesMapper.GetAgreementLevel(ERA_Type);

		public ZPropertyInfo LevelInfo => GetZPropertyInfo(Schema.Level);

		bool AgreementDetailsReadOnly => IsInDatabase && IsEffectiveTimeInDatabaseInThePast;

		bool IsEffectiveTimeInDatabaseInThePast
		{
			get
			{
				if (!useCachedEffectiveTimeDatabaseComparison)
				{
					isEffectiveTimeInDatabaseInThePast = (ZDateTime)ERA_EffectiveTimeUtcInfo.OriginalValue <= ZDateTime.UtcNow;
					useCachedEffectiveTimeDatabaseComparison = true;
				}

				return isEffectiveTimeInDatabaseInThePast;
			}
		}

		ZBool isEffectiveTimeInDatabaseInThePast;
		ZBool useCachedEffectiveTimeDatabaseComparison;

		public ZString UniqueIdentifierForCodeProperty
		{
			get => $"{ERA_Type}_{ERA_RN_NKCountryCode}_{ERA_VersionNumber}_{ERA_MinorVersion}";
		}

		public ZPropertyInfo UniqueIdentifierForCodePropertyInfo => GetZPropertyInfo(nameof(UniqueIdentifierForCodeProperty));

		#endregion

		#region Delete

		public override bool CanDelete => !IsInDatabase || !IsEffectiveTimeInDatabaseInThePast;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("4386dd9b-00ef-4e89-852f-8bb7031fad8b", "User Agreements cannot be deleted once they have taken effect.");

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			if (ERA_VersionNumber.IsEmpty || ERA_VariantCodeInfo.HasChanges)
			{
				var highestExistingVersionNumberQuery = new ZQuery(EdiUserAgreementSchema.ERA_Type, ERA_Type);
				highestExistingVersionNumberQuery.AddToFilter(EdiUserAgreementSchema.ERA_RN_NKCountryCode, ERA_RN_NKCountryCode);
				highestExistingVersionNumberQuery.AddToFilter(EdiUserAgreementSchema.ERA_VariantCode, ERA_VariantCode);
				highestExistingVersionNumberQuery.AddToFilter(EdiUserAgreementSchema.PK, SQLComparisonOperator.NotEqual, PK);
				highestExistingVersionNumberQuery.OrderBy =
					EdiUserAgreementSchema.Constants.ERA_VersionNumber + OrderByClause.Descending
						+ ", " + EdiUserAgreementSchema.Constants.ERA_MinorVersion + OrderByClause.Descending;
				var agreement = Factory.LoadTop1<EdiUserAgreement>(highestExistingVersionNumberQuery);

				ERA_VersionNumber = 1;
				ERA_MinorVersion = 0;

				if (ERA_Type == EdiUserAgreementTypes.Codes.CargoWiseNext)
				{
					if (agreement != null)
					{
						ERA_VersionNumber = agreement.ERA_VersionNumber;
						ERA_MinorVersion = agreement.ERA_MinorVersion + 1;
					}
				}
				else
				{
					if (agreement != null)
					{
						ERA_VersionNumber += agreement.ERA_VersionNumber;
					}
				}
			}

			var utcNow = ZDateTime.UtcNow;
			if (ERA_EffectiveTimeUtc > utcNow.AddMinutes(-6) && ERA_EffectiveTimeUtc < utcNow.AddMinutes(5))
			{
				ERA_EffectiveTimeUtc = utcNow.AddMinutes(5);
			}

			base.OnSaving();

			if (ERA_IsActiveInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(AutoEvents.EditedARecord, FormattableString.Invariant($"{EdiUserAgreementSchema.Constants.ERA_IsActive}|OLD={ERA_IsActiveInfo.OriginalValue}|NEW={ERA_IsActiveInfo.Value}"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				useCachedEffectiveTimeDatabaseComparison = false;
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, EdiUserAgreementSchema.Constants.Prefix));

		DocManagerInfo docManagerInfo;

		#endregion

		#region Fallback

		public bool HasFallback
		{
			get
			{
				var fallbackQuery = new ZDBOnlyQuery(typeof(EdiUserAgreement));
				fallbackQuery.AddToFilter(EdiUserAgreementSchema.ERA_Type, ERA_Type);
				fallbackQuery.AddToFilter(EdiUserAgreementSchema.ERA_RN_NKCountryCode, string.Empty);

				var dateSubQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreement), EdiUserAgreementSchema.PK);
				dateSubQuery.AddToFilter(EdiUserAgreementSchema.ERA_EffectiveTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ERA_EffectiveTimeUtc);
				dateSubQuery.AddToFilter(JoinCondition.Or, EdiUserAgreementSchema.ERA_EffectiveTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
				fallbackQuery.AddSubQuery(dateSubQuery, JoinCondition.And);

				return Factory.ExistsInDatabase(EdiUserAgreementSchema.Constants.TableName, fallbackQuery);
			}
		}

		#endregion

		#region Get Current Agreement

		public static EdiUserAgreementInfo GetCurrentAgreement(BusinessObjectFactory factory, BusinessObject parent, string agreementType, string countryCode)
		{
			var tableCodes = EdiUserAgreementTypesMapper.GetBindingTableCodes(agreementType);
			if (tableCodes.Count > 0)
			{
				if (parent == null || !tableCodes.Contains(parent.TablePrefix))
				{
					return new EdiUserAgreementInfo(false);
				}

				return new EdiUserAgreementInfo(EdiUserAgreementAssignment.GetCurrentAssignment(parent, agreementType));
			}

			var query = GetCurrentAgreementsQuery(agreementType, countryCode);
			query.OrderBy = EdiUserAgreementSchema.Constants.ERA_RN_NKCountryCode + OrderByClause.Descending + "," + EdiUserAgreementSchema.Constants.ERA_EffectiveTimeUtc + OrderByClause.Descending;

			return new EdiUserAgreementInfo(factory.LoadTop1<EdiUserAgreement>(query));
		}

		internal static ZQuery GetCurrentAgreementsQuery(string type, string countryCode)
		{
			var currentAgreementsQuery = new ZDBOnlyQuery(typeof(EdiUserAgreement));
			currentAgreementsQuery.AddToFilter(EdiUserAgreementSchema.ERA_Type, type);
			currentAgreementsQuery.AddToFilter(EdiUserAgreementSchema.ERA_IsActive, true);
			currentAgreementsQuery.AddToFilter(EdiUserAgreementSchema.ERA_EffectiveTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			if (!string.IsNullOrEmpty(countryCode))
			{
				currentAgreementsQuery.AddToFilter(EdiUserAgreementSchema.ERA_RN_NKCountryCode, new string[] { countryCode, string.Empty });
			}
			else
			{
				currentAgreementsQuery.AddToFilter(EdiUserAgreementSchema.ERA_RN_NKCountryCode, string.Empty);
			}

			return currentAgreementsQuery;
		}

		public static EdiUserAgreementInfo GetCurrentAgreementInfoForAssignment(EdiUserAgreementAssignment assignment)
		{
			return new EdiUserAgreementInfo(assignment);
		}

		#endregion

		#region Acceptance

		public bool HasAcknowledgedCorporateAgreement(LicenceDatabase database)
		{
			var userAccountSubQuery = new ZDBOnlySubQuery(typeof(EdiCustomerUserAccount), EdiCustomerUserAccountSchema.PK);
			userAccountSubQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
			var orgDatabaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.LD_OH_WebAccessOrg);
			orgDatabaseSubQuery.AddToFilter(LicenceDatabaseSchema.PK, database.PK);

			var enterpriseQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_LD, null);
			enterpriseQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_LE, database.LD_LE);

			var databaseQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_LD, database.PK);

			var agreementAcceptanceLogQuery = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
			agreementAcceptanceLogQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, PK);
			var orQuery = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
			orQuery.AddSubQuery(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, userAccountSubQuery, JoinCondition.Or);
			orQuery.AddSubQuery(EdiUserAgreementAcceptanceLogSchema.EUL_OH, orgDatabaseSubQuery, JoinCondition.Or);
			orQuery.AddToFilter(enterpriseQuery, JoinCondition.Or);
			orQuery.AddToFilter(databaseQuery, JoinCondition.Or);

			agreementAcceptanceLogQuery.AddToFilter(orQuery);
			return Factory.Exists(typeof(EdiUserAgreementAcceptanceLog), agreementAcceptanceLogQuery);
		}

		public bool HasBeenAcceptedByOrganisation(OrgHeader organisation)
		{
			if (organisation != null)
			{
				var agreementAcceptanceLogQuery = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
				agreementAcceptanceLogQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, PK);
				agreementAcceptanceLogQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_EUA, null);
				agreementAcceptanceLogQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_AcceptanceTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);

				var orQuery = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
				orQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_OH, organisation.PK);
				if (organisation is EDIOrgHeader ediOrg && ediOrg.LicEnterprise != null)
				{
					orQuery.AddToFilter(JoinCondition.Or, EdiUserAgreementAcceptanceLogSchema.EUL_LE, ediOrg.LicEnterprise.PK);
				}
				agreementAcceptanceLogQuery.AddToFilter(orQuery);

				return Factory.Exists(typeof(EdiUserAgreementAcceptanceLog), agreementAcceptanceLogQuery);
			}
			else
			{
				return false;
			}
		}

		public bool HasBeenAcceptedByEnterprise(LicenceEnterprise enterprise)
		{
			if (enterprise != null)
			{
				var agreementAcceptanceLogQuery = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
				agreementAcceptanceLogQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, PK);
				agreementAcceptanceLogQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_LE, enterprise.PK);
				agreementAcceptanceLogQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_AcceptanceTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
				return Factory.Exists(typeof(EdiUserAgreementAcceptanceLog), agreementAcceptanceLogQuery);
			}
			else
			{
				return false;
			}
		}

		public EdiUserAgreementAcceptanceLog AcceptOnBehalfOfOrganisation(OrgHeader organisation)
		{
			var log = Factory.New<EdiUserAgreementAcceptanceLog>();
			log.EUL_ERA = PK;
			log.EUL_OH = organisation.PK;
			log.EUL_GS = EnvProxy.Instance.CurrentUser.PK;
			log.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			return log;
		}

		#endregion

#if DEBUG
		#region FillWithValidTestData

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ERA_VariantCode = string.Empty;
			ERA_VariantDescription = string.Empty;
			ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddHours(1);
		}

		#endregion
#endif
	}

	public class EdiUserAgreementInfo
	{
		internal EdiUserAgreementInfo(bool allowOnlineAcceptance)
		{
			AllowOnlineAcceptance = allowOnlineAcceptance;
		}

		internal EdiUserAgreementInfo(EdiUserAgreementAssignment assignment)
		{
			AllowOnlineAcceptance = false;
			if (assignment != null)
			{
				Agreement = assignment.CurrentAgreement;
				Assignment = assignment;
				AllowOnlineAcceptance = assignment.EAE_AllowOnlineAcceptance;
			}
		}

		public EdiUserAgreementInfo(EdiUserAgreement agreement)
		{
			Agreement = agreement;
		}

		public readonly EdiUserAgreement Agreement;
		public readonly EdiUserAgreementAssignment Assignment;

		public readonly bool AllowOnlineAcceptance = true;
	}
}
