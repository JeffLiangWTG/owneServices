using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CargoWise.BrandManager;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using ZClientEDI.Business;

namespace Enterprise.Client.EDI
{
	/// <summary>
	/// NOTE: If making changes to the table schema, you must manually bump up the version in SchemaVersion.cs.
	/// If making changes to the views, procedures, functions & triggers, bump up the version in ScriptVersion.cs.
	/// Else, your changes will be ignored until the next schema/script upgrade.
	/// </summary>
	public class EDIClientDbSchemaUpgradeInfo : IExtensionObjects
	{
		ImmutableArray<DatabaseObjectCreateScript> IExtensionObjects.TableCreationScripts => TableCreationScripts.Value;
		ImmutableArray<DatabaseViewAndRoutineCreateScript> IExtensionObjects.ViewAndRoutineCreationScripts => ViewAndRoutinesCreationScripts.Value;

		#region TableCreationScripts

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Lazy<ImmutableArray<DatabaseObjectCreateScript>> TableCreationScripts
		{
			get
			{
				return new Lazy<ImmutableArray<DatabaseObjectCreateScript>>
				(() => ImmutableArray.Create(
					// TABLES
					ReleaseBuild,
					LicenceCompany,
					EdiTrustedMessagingConfig,
					EdiTrustedSystem,
					FeatureControlSet,
					LicenceDatabase,
					EdiLicenceDatabaseConsolidationHistory,
					LicenceHeader,
					LicenceModules,
					LicenceConnection,
					Licence3rdPartySoftware,

					EdiIdentityTenant,
					EdiIdentityApplication,
					EdiIdentityApplicationPermission,
					EdiIdentityCertificate,
					EdiIdentityRedirectUrl,

					UpgradesToClient,

					ClientCompany,
					ClientCompanyCodeHistory,
					ClientCompanyActiveStatusHistory,
					EdiClientCompanyMergeHistory,

					HelpErrorLogOccurrence,
					HelpErrorLogKey,
					HelpErrorStackLineCount,

					IncidentTriage,
					IncidentTriageChecklistItem,
					IncidentTriageChecklistItemPivot,
					IncidentMain,
					IncidentDiagnosticCriteria,
					IncidentDiagnosticCriteriaPivot,
					IncidentTriageDiagnosticCriteriaPivot,
					InvestigationItem,
					InvestigationItemResponseOption,
					DiagnosticCriteriaInvestigationItemLink,
					DiagnosticCriteriaInvestigationResult,
					IncidentSchema.EdiIncidentConversationMessageQueue,
					IncidentSchema.EdiLegacyConversationMessage,
					ClientIncidentEstimate,
					ClientIncidentQuote,
					IncidentManagementGroup,
					IncidentManagementGroupMessage,
					IncidentManagementLink,
					IncidentMetrics,

					GlbTrainingCourse,
					GlbClassroomSubject,
					GlbClassroomSession,
					GlbTime,
					GlbClassroomAttendee,

					AccAmbiguousCommission,

					ClientMailDBRecipients,
					ClientFeatureRequestValueAndContribution,
					ClientProductConsultantSurveyRecipient,
					ClientProductConsultantSurveyRecipientSession,
					ClientLicenceBilling,
					ClientLicenceBillingDiscount,
					ClientLicencePriceHeader,
					EdiPriceHeaderDiscount,
					EdiPriceDiscountGroupMember,
					ClientLicencePriceItem,
					EdiPriceItemRate,
					EdiPriceUsageMapping,
					EdiPriceHeaderLink,
					EdiLicenceSetting,
					EdiUsageInvoice,
					EdiBilledUsage,
					EdiBilledDiscount,
					EdiPriceHeaderExchangeRate,
					ClientLicenceFee,
					ClientChargeableUsage,
					ClientInvoiceDelivery,
					ClientInvoiceDelivery_L9_OH_InvoiceTo,
					ClientLicenceHeaderEx,
					ClientStaff,
					ClientBranch,

					ClientLicenceUsage,
					ClientLicenceUsage_LX_UsageTime_LX_PK,
					ClientLicenceUsageIndexDetailedUsage,
					EdiLicenceUsage,
					EdiExternalChargeableUsage,
					ClientFaxPrice,
					ClientPremiumService,
					ClientLicenceBillingExcludeOrg,
					DepositSchema.EdiDepositBalance,
					DepositSchema.EdiDepositAdjust,
					DepositSchema.EdiDepositBalanceChanges,
					ClientOrgImportHistory,
					ClientOrgImportHistoryIndex_O2_ImportedDate,
					ClientOrgImportHistoryIndex_O2_TargetOrgPK,
					ClientStatisticsXML,
					ClientStatisticsXMLIndex_IM_ClientID_IM_InsertUTC,
					ClientStatisticsXMLIndex_IM_InsertUTC,
					ClientOrgConsol,
					ClientOrgConsolClusteredIndex_O7_ExportUTC,
					ClientWorkProject,
					ClientStatisticsXMLArchive,
					ClientStatisticsXMLArchiveIndex_IMA_InsertUTC,

					ClientTelRimRegistration,

					EdiCommissionAgreementCustomization,
					EdiCommissionAgreementDatabasePivot,
					EdiCommissionAgreementCompanyPivot,
					EdiCommissionAgreementCompanyAutoAddDatabase,
					EdiCommissionAgreementCompanyAutoAddCountry,
					EdiCommissionHeaderAdditionalInfo,

					EdiOrgOpportunityEx,
					EdiOrgOpportunityValueAnalysis,

					EdiUsageReportQueue,
					EdiPersonMergeQueue,

					EdiCustomerUserAccount,
					EdiUserAgreement,
					EdiUserAgreementAssignment,
					EdiUserAgreementAcceptanceLog,
					EdiPromptSkip,
					TrustedMessagingSchema.EdiAccessToken,

					BillingUsageSchema.EdiOrgMembership,
					HrSchema.EdiStaffChange,
					HrSchema.EdiGlbStaffEx,
					HrSchema.GlbStaffHoliday_NR_RX__GA_GS_GA_RecordType_GA_LeaveComment,

					EdiERequestDocumentQueue,

					// NOTE - Run the ClientOverride.Test if you add a new table

					Client_eRouterEdiEnterpriseCommunication,

					IncidentSimilarityToken,
					IncidentSimilarityTfIdf,
					IncidentSimilarityMatrix,

					EdiLicenceDatabaseOrgSuggestion,

					ELearningDocumentDescription,
					ELearningDocumentTfIdf,

					IncidentSimilarityExclusion,

					EdiTokenAuthOnBoardingData,
					LicenceEnterpriseDomains,

					FeatureControlHeader,
					FeatureControlRule,
					FeatureControlRuleLicenceDatabasePivot,
					GenPivot_Constraint_XX_RelationType_XX_Relation1TableCode_XX_Relation2TableCode,

					ApplicationLogger,
					ApplicationActiveLogger
				));
			}
		}

		#region IncidentSimilarityExclusion
		static DatabaseObjectCreateScript IncidentSimilarityExclusion
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentSimilarityExclusion",
					@"
CREATE TABLE dbo.IncidentSimilarityExclusion
(
	[ISE_PK] [UNIQUEIDENTIFIER] NOT NULL DEFAULT (newid()),
    [ISE_IM] [UNIQUEIDENTIFIER] NULL,
    [ISE_LastUpdatedUtc] [datetime] NOT NULL,
);
ALTER TABLE [IncidentSimilarityExclusion]
    SET (LOCK_ESCALATION = DISABLE);
ALTER TABLE  [IncidentSimilarityExclusion]
ADD CONSTRAINT [PK_UX__ISE_PK] PRIMARY KEY CLUSTERED ([ISE_PK] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
;
ALTER TABLE [IncidentSimilarityExclusion] WITH NOCHECK
	  ADD CONSTRAINT [IncidentSimilarityExclusion_ISE_IM_FK2_IncidentMain_ID] FOREIGN KEY
		  ( [ISE_IM] )
		  REFERENCES [IncidentMain]
		  ( [IM_PK] );

CREATE NONCLUSTERED INDEX FK_RX__ISE_IM ON IncidentSimilarityExclusion(ISE_IM ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
",
					"DROP TABLE IncidentSimilarityExclusion");
			}
		}
		#endregion

		#region ELearningDocumentDescription
		static DatabaseObjectCreateScript ELearningDocumentDescription
		{
			get
			{
				return new DatabaseObjectCreateScript("ELearningDocumentDescription",
					@"
CREATE TABLE dbo.ELearningDocumentDescription
(
	[ELD_PK] [uniqueidentifier] NOT NULL,
    [ELD_MyAccountDocumentId] [UNIQUEIDENTIFIER] NOT NULL,
    [ELD_Title] NVARCHAR(255) NOT NULL DEFAULT '',
    [ELD_Url] NVARCHAR(max) NOT NULL DEFAULT '',
    [ELD_DocumentType] NVARCHAR(255) NOT NULL DEFAULT '',
    [ELD_DocumentLastModified] [datetime] NOT NULL,
);

ALTER TABLE [ELearningDocumentDescription]
    SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [ELearningDocumentDescription]
ADD CONSTRAINT [PK_UX__ELD_PK] PRIMARY KEY CLUSTERED ([ELD_PK] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
;

CREATE NONCLUSTERED INDEX [NR_RX__ELD_DocumentLastModified] ON [ELearningDocumentDescription] ([ELD_DocumentLastModified] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [NR_RX__ELD_MyAccountDocumentId] ON [ELearningDocumentDescription] ([ELD_MyAccountDocumentId] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

",
					"DROP TABLE ELearningDocumentDescription");
			}
		}

		#endregion

		#region ELearningDocumentTfIdf
		static DatabaseObjectCreateScript ELearningDocumentTfIdf
		{
			get
			{
				return new DatabaseObjectCreateScript("ELearningDocumentTfIdf",
					@"
CREATE TABLE dbo.ELearningDocumentTfIdf	
(
	[EDT_PK] [uniqueidentifier] NOT NULL,		
	[EDT_TF] [varbinary](max) NULL,
	[EDT_TFIDF] [varbinary](max) NULL,		
    [EDT_ELD] [UNIQUEIDENTIFIER] NOT NULL,
    [EDT_DocumentLastModified] [datetime] NOT NULL
);

ALTER TABLE [ELearningDocumentTfIdf]
    SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [ELearningDocumentTfIdf]
ADD CONSTRAINT [PK_UX__EDT_PK] PRIMARY KEY CLUSTERED ([EDT_PK] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
;
ALTER TABLE [ELearningDocumentTfIdf] WITH NOCHECK
	  ADD CONSTRAINT [ELearningDocumentTfIdf_EDT_ELD_ELearningDocumentDescription_ID] FOREIGN KEY
		  ( [EDT_ELD] )
		  REFERENCES [ELearningDocumentDescription]
		  ( [ELD_PK] )
;
CREATE NONCLUSTERED INDEX [NR_RX__EDT_DocumentLastModified] ON [ELearningDocumentTfIdf] ([EDT_DocumentLastModified] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [NR_RX__EDT_ELD] ON [ELearningDocumentTfIdf] ([EDT_ELD] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
",
					"DROP TABLE ELearningDocumentTfIdf");
			}
		}

		#endregion

		#region IncidentSimilarityToken

		static DatabaseObjectCreateScript IncidentSimilarityToken
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentSimilarityToken", @"
CREATE TABLE dbo.IncidentSimilarityToken
(
	[IST_PK] [uniqueidentifier] NOT NULL,
	[IST_Version] [int] NOT NULL,
	[IST_InputToken] [nvarchar](max) NOT NULL,
	[IST_OutputToken] [int] NOT NULL,

	CONSTRAINT [PK_UX__IST_PK] PRIMARY KEY CLUSTERED ([IST_PK] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];",
					"DROP TABLE IncidentSimilarityToken");
			}
		}

		#endregion

		#region IncidentSimilarityTfIdf

		static DatabaseObjectCreateScript IncidentSimilarityTfIdf
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentSimilarityTfIdf",
					@"
CREATE TABLE dbo.IncidentSimilarityTfIdf
(
	[ISV_PK] [uniqueidentifier] NOT NULL,
	[ISV_IM_Incident] [uniqueidentifier] NOT NULL,
	[ISV_Version] [int] NOT NULL,
	[ISV_TF] [varbinary](max) NULL,
	[ISV_TFIDF] [varbinary](max) NULL,
	[ISV_Status] [char](3) NOT NULL,
	[ISV_IncidentLastModified] [datetime] NOT NULL,

	CONSTRAINT [PK_UX__ISV_PK] PRIMARY KEY CLUSTERED ([ISV_PK] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE NONCLUSTERED INDEX [NR_RX__ISV_IM_Incident__ISV_Version] ON [dbo].[IncidentSimilarityTfIdf] ([ISV_IM_Incident] ASC, [ISV_Version] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY];

CREATE NONCLUSTERED INDEX [NR_RX__ISV_IncidentLastModified] ON [dbo].[IncidentSimilarityTfIdf] ([ISV_IncidentLastModified] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY];",
					"DROP TABLE IncidentSimilarityTfIdf");
			}
		}

		#endregion

		#region IncidentSimilarityMatrix

		static DatabaseObjectCreateScript IncidentSimilarityMatrix
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentSimilarityMatrix",
					@"
CREATE TABLE dbo.IncidentSimilarityMatrix
(
	[ISM_PK] [uniqueidentifier] NOT NULL,
	[ISM_IM_Incident1] [uniqueidentifier] NOT NULL,
	[ISM_IM_Incident2] [uniqueidentifier] NOT NULL,
	[ISM_Version] [int] NOT NULL,
	[ISM_Similarity] [decimal](9, 8) NOT NULL,

	CONSTRAINT [PK_UX__ISM_PK] PRIMARY KEY CLUSTERED ([ISM_PK] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY];

CREATE NONCLUSTERED INDEX [NR_RX__ISM_IM_Incident1__ISM_IM_Incident2__ISM_Version] ON [dbo].[IncidentSimilarityMatrix] ([ISM_IM_Incident1] ASC, [ISM_IM_Incident2] ASC, [ISM_Version] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY];

CREATE NONCLUSTERED INDEX [NR_RX__ISM_IM_Incident2__ISM_PK__ISM_Incident1__ISM_Version] ON [dbo].[IncidentSimilarityMatrix] ([ISM_IM_Incident2] ASC)
INCLUDE ([ISM_PK], [ISM_IM_Incident1], [ISM_Version], [ISM_Similarity])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY];",
					"DROP TABLE IncidentSimilarityMatrix");
			}
		}

		#endregion

		#region IncidentMain

		static DatabaseObjectCreateScript IncidentMain
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentMain", @"
CREATE TABLE dbo.IncidentMain
(
   [IM_PK] UNIQUEIDENTIFIER NOT NULL,
   [IM_IncidentType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_WorkItemType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_Product] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_SourceModuleId] VARCHAR(50) NOT NULL DEFAULT '' ,
   [IM_ProgramArea] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_RN_NKCountry] VARCHAR(2) NOT NULL DEFAULT '' ,
   [IM_PatchTo] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_Category] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_IncidentNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
   [IM_ClientIncidentReference] VARCHAR(20) NOT NULL DEFAULT '' ,
   [IM_Language] CHAR(7) NOT NULL DEFAULT 'EN',
   [IM_OH_Client] UNIQUEIDENTIFIER NULL,
   [IM_OC_Contact] UNIQUEIDENTIFIER NULL,
   [IM_OA_BranchAddress] UNIQUEIDENTIFIER NULL,
   [IM_Module] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_ChargableWork] CHAR(1) NOT NULL DEFAULT 'N' ,
   [IM_QuoteAmount] MONEY NOT NULL DEFAULT 0 ,
   [IM_RX_NKQuoteCurrency] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_DepositAmountRequired] MONEY NOT NULL DEFAULT 0 ,
   [IM_UpgradeAssuranceAccepted] SMALLDATETIME NULL,
   [IM_Description] NVARCHAR(80) NOT NULL DEFAULT '' ,
   [IM_Priority] CHAR(3) NOT NULL DEFAULT '' ,
   [IM_ResolutionCode] CHAR(3) NOT NULL DEFAULT '' ,
   [IM_ClosureResolution] CHAR(3) NOT NULL DEFAULT '' ,
   [IM_Status] CHAR(3) NOT NULL DEFAULT '' ,
   [IM_IncidentOpenedWorkHours] DECIMAL(8,2) NOT NULL DEFAULT 0 ,
   [IM_CloseTimeUtc] DATETIME NULL,
   [IM_HL_ClientReportedOnVersion] UNIQUEIDENTIFIER NULL,
   [IM_HL_FixedOnThisVersion] UNIQUEIDENTIFIER NULL,
   [IM_ClientBugSeverity] VARCHAR(3) NOT NULL DEFAULT 'LOW' ,
   [IM_BugFixDeployed] SMALLDATETIME NULL,
   [IM_FeatureRequestType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_FeatureRequestStatus] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_FeatureRequestDisposition] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_FeatureRequestCost] MONEY NOT NULL DEFAULT 0 ,
   [IM_FeatureRequestIndustryValue] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_FeatureRequestPublishForVote] CHAR(1) NOT NULL DEFAULT 'N' ,
   [IM_DefectStatus] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_DefectDisposition] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_Source] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_ServiceStatus] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_IsADefectPerDefinition] CHAR(1) NOT NULL DEFAULT 'N' ,
   [IM_ReproducedInPatchRelease] CHAR(1) NOT NULL DEFAULT 'N' ,
   [IM_ReproducedInAlphaRelease] CHAR(1) NOT NULL DEFAULT 'N' ,
   [IM_DefectNonCompliantReason] VARCHAR(80) NOT NULL DEFAULT '' ,
   [IM_OC_TechnicalContact] UNIQUEIDENTIFIER NULL,
   [IM_IM_ProjectHeader] UNIQUEIDENTIFIER NULL,
   [IM_OrderReceived] SMALLDATETIME NULL,
   [IM_PlannedInstall] SMALLDATETIME NULL,
   [IM_InstallDate] SMALLDATETIME NULL,
   [IM_LA] UNIQUEIDENTIFIER NULL,
   [IM_LCC] UNIQUEIDENTIFIER NULL,
   [IM_LD] UNIQUEIDENTIFIER NULL,
   [IM_GG_Team] UNIQUEIDENTIFIER NULL,
   [IM_GC] UNIQUEIDENTIFIER NULL,
   [IM_IMT_Triage] UNIQUEIDENTIFIER NULL,
   [IM_SubCategory] VARCHAR(20) NOT NULL DEFAULT '' ,
   [IM_Details] VARBINARY(MAX) NULL,
   [IM_RequiredBy] SMALLDATETIME NULL,
   [IM_CallbackBy] SMALLDATETIME NULL,
   [IM_EstimatedHours] SMALLINT NOT NULL DEFAULT 0 ,
   [IM_ActualHoursWorked] DECIMAL(8,2) NOT NULL DEFAULT 0 ,
   [IM_ReasonableHours] DECIMAL(8,1) NOT NULL DEFAULT 0 ,
   [IM_DefectCausedPhase] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_GS_NKCustDefectCausedBy] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_GS_NKCustServiceContact] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_GS_NKAssignedToCurrent] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_GS_NKSpecifiedBy] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [IM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_SystemCreateBranch] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_SystemCreateDepartment] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_SystemLastEditTimeUtc] DATETIME NULL,
   [IM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '' ,
   [IM_INC_Request] uniqueidentifier NULL,
   [IM_RequestStatus] varchar(3) not null default '',
   [IM_ServiceType] VARCHAR(3) NOT NULL DEFAULT '',
   [IM_ResolveTimeUtc] DATETIME NULL,

   CONSTRAINT IncidentMain_IM_INC_Request_FK2_IncidentRequest_RRR_120N FOREIGN KEY (IM_INC_Request) REFERENCES IncidentRequest (INC_PK),
);

ALTER TABLE [IncidentMain]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [IncidentMain]
ADD CONSTRAINT [PK_UX__IM_PK] PRIMARY KEY NONCLUSTERED  ([IM_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_GG_Team] ON [IncidentMain] ([IM_GG_Team] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IM_IncidentNumber] ON [IncidentMain] ([IM_IncidentNumber] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_CloseTimeUtc] ON [IncidentMain] ([IM_CloseTimeUtc] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_OH_Client] ON [IncidentMain] ([IM_OH_Client] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_OC_Contact] ON [IncidentMain] ([IM_OC_Contact] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_OA_BranchAddress] ON [IncidentMain] ([IM_OA_BranchAddress] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_HL_ClientReportedOnVersion] ON [IncidentMain] ([IM_HL_ClientReportedOnVersion] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_HL_FixedOnThisVersion] ON [IncidentMain] ([IM_HL_FixedOnThisVersion] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_IM_ProjectHeader] ON [IncidentMain] ([IM_IM_ProjectHeader] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_SystemCreateTimeUtc] ON [IncidentMain] ([IM_SystemCreateTimeUtc] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_SystemCreateUser] ON [IncidentMain] ([IM_SystemCreateUser] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_SystemLastEditTimeUtc] ON [IncidentMain] ([IM_SystemLastEditTimeUtc] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_SystemLastEditUser] ON [IncidentMain] ([IM_SystemLastEditUser] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_LA] ON [IncidentMain] ([IM_LA] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_OC_TechnicalContact] ON [IncidentMain] ([IM_OC_TechnicalContact] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_Module] ON [IncidentMain] ([IM_Module] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_GC] ON [IncidentMain] ([IM_GC] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_IncidentType] ON [IncidentMain] ([IM_IncidentType] ASC)
INCLUDE ( [IM_PK] )
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_LCC] ON [IncidentMain] ([IM_LCC] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__IM_LD] ON [IncidentMain] ([IM_LD] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_ProgramArea] ON [IncidentMain] ([IM_ProgramArea] ASC)
WHERE IM_IncidentType = 'INC'
WITH (ALLOW_PAGE_LOCKS = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [FK_UX__IM_INC_Request] ON [IncidentMain] ([IM_INC_Request] ASC)
WHERE IM_INC_Request is not null
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IM_ClientIncidentReference__IM_OH_Client__IM_Status__IM_SystemCreateTimeUtc] ON [IncidentMain] ([IM_ClientIncidentReference] ASC, [IM_OH_Client] ASC, [IM_Status] ASC, [IM_SystemCreateTimeUtc] DESC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE CLUSTERED INDEX [FK_RC__IM_OH_Client__IM_LD] ON [IncidentMain] ([IM_OH_Client] ASC, [IM_LD] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_GG_Team_FK2_GlbGroup_RRR_120N] FOREIGN KEY
		  ( [IM_GG_Team] )
		  REFERENCES [GlbGroup]
		  ( [GG_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_OH_Client_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [IM_OH_Client] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_OC_Contact_FK2_OrgContact_RRR_120N] FOREIGN KEY
		  ( [IM_OC_Contact] )
		  REFERENCES [OrgContact]
		  ( [OC_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_OA_BranchAddress_FK2_OrgAddress_RRR_120N] FOREIGN KEY
		  ( [IM_OA_BranchAddress] )
		  REFERENCES [OrgAddress]
		  ( [OA_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_HL_ClientReportedOnVersion_FK2_ReleaseBuild_RRR_120N] FOREIGN KEY
		  ( [IM_HL_ClientReportedOnVersion] )
		  REFERENCES [ReleaseBuild]
		  ( [HL_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_HL_FixedOnThisVersion_FK2_ReleaseBuild_RRR_120N] FOREIGN KEY
		  ( [IM_HL_FixedOnThisVersion] )
		  REFERENCES [ReleaseBuild]
		  ( [HL_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_IM_ProjectHeader_FK2_IncidentMain_RRR_120N] FOREIGN KEY
		  ( [IM_IM_ProjectHeader] )
		  REFERENCES [IncidentMain]
		  ( [IM_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_LA_FK2_LicenceHeader_RRR_120N] FOREIGN KEY
		  ( [IM_LA] )
		  REFERENCES [LicenceHeader]
		  ( [LA_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_LCC_FK2_ClientCompany_RRR_120N] FOREIGN KEY
		  ( [IM_LCC] ) 
		  REFERENCES [ClientCompany]
		  ( [LCC_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		  ( [IM_LD] ) 
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_OC_TechnicalContact_FK2_OrgContact_RRR_120N] FOREIGN KEY
		  ( [IM_OC_TechnicalContact] )
		  REFERENCES [OrgContact]
		  ( [OC_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_GC_FK2_GlbCompany_RRR_120N] FOREIGN KEY
		  ( [IM_GC] )
		  REFERENCES [GlbCompany]
		  ( [GC_PK] )

ALTER TABLE [IncidentMain] WITH NOCHECK
	  ADD CONSTRAINT [IncidentMain_IM_IMT_Triage_FK2_IncidentTriage_RRR_120N] FOREIGN KEY
		  ( [IM_IMT_Triage] )
		  REFERENCES [IncidentTriage]
		  ( [IMT_PK] )
;

ALTER TABLE [IncidentMain] WITH NOCHECK 
	  ADD CONSTRAINT [IncidentMain_IM_Language] CHECK (IM_Language <> '')
;
				", "DROP TABLE IncidentMain");
			}
		}

		#endregion

		#region IncidentManagementGroup

		static DatabaseObjectCreateScript IncidentManagementGroup
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentManagementGroup", @"
CREATE TABLE dbo.IncidentManagementGroup
(
   [ING_PK] UNIQUEIDENTIFIER NOT NULL,
   [ING_IncidentGroupNumber] VARCHAR(20) NOT NULL DEFAULT '',
   [ING_Type] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_Description] NVARCHAR(80) NOT NULL DEFAULT '',
   [ING_ServiceOutage] VARCHAR(3) NOT NULL DEFAULT 'INV',
   [ING_BusinessImpact] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_Urgency] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_GS_NKGroupOwner] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_Status] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_Product] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_ProductArea] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_Priority] CHAR(3) NOT NULL DEFAULT '',
   [ING_SourceModuleId] VARCHAR(50) NOT NULL DEFAULT '',
   [ING_Category] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_Module] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_RN_NKCountry] VARCHAR(2) NOT NULL DEFAULT '',
   [ING_GS_NKDefaultResponder] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_IsBroadcastToControlledOnly] BIT NOT NULL DEFAULT 1,
   [ING_IsAutoReply] BIT NOT NULL DEFAULT 0,
   [ING_IsKnownIssue] BIT NOT NULL DEFAULT 1,
   [ING_IMT_Triage] UNIQUEIDENTIFIER NULL,
   [ING_SystemCreateTimeUtc] DATETIME NULL,
   [ING_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_SystemCreateBranch] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_SystemCreateDepartment] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_SystemLastEditTimeUtc] DATETIME NULL,
   [ING_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_ServiceType] VARCHAR(3) NOT NULL DEFAULT '',
   [ING_IsAutoReplyUnflagsCommunication] BIT NOT NULL DEFAULT 1,
   [ING_IsInterimBroadcastUnflagsCommunication] BIT NOT NULL DEFAULT 1,
);

ALTER TABLE [IncidentManagementGroup]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentManagementGroup]
ADD CONSTRAINT [PK_UX__ING_PK] PRIMARY KEY NONCLUSTERED ([ING_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

DROP INDEX IF EXISTS [NR_UX__ING_IncidentGroupNumber] ON [IncidentManagementGroup]
;

CREATE UNIQUE CLUSTERED INDEX [NR_UC__ING_IncidentGroupNumber] ON [IncidentManagementGroup] ([ING_IncidentGroupNumber] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__ING_Type] ON [IncidentManagementGroup] ([ING_Type] ASC)
INCLUDE ([ING_PK])
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentManagementGroup] WITH NOCHECK
	  ADD CONSTRAINT [IncidentManagementGroup_ING_IMT_Triage_FK2_IncidentTriage_RRR_120N] FOREIGN KEY
		  ( [ING_IMT_Triage] )
		  REFERENCES [IncidentTriage]
		  ( [IMT_PK] )
;

				", "DROP TABLE IncidentManagementGroup");
			}
		}

		#endregion

		#region IncidentManagementGroupMessage

		static DatabaseObjectCreateScript IncidentManagementGroupMessage
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentManagementGroupMessage", @"
CREATE TABLE dbo.IncidentManagementGroupMessage
(
	[IGM_PK] UNIQUEIDENTIFIER NOT NULL,
	[IGM_ING_Group] UNIQUEIDENTIFIER NOT NULL,
	[IGM_IsPublished] BIT NOT NULL DEFAULT 0,
	[IGM_Type] VARCHAR(3) NOT NULL DEFAULT '',
	[IGM_Message] NVARCHAR(MAX)	NOT NULL DEFAULT '',
	[IGM_BroadcastDateUtc] DATETIME NULL,
	[IGM_SystemCreateTimeUtc] DATETIME NULL,
	[IGM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
	[IGM_SystemLastEditTimeUtc] DATETIME NULL,
	[IGM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);

ALTER TABLE [IncidentManagementGroupMessage]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentManagementGroupMessage]
ADD CONSTRAINT [IncidentManagementGroupMessage_IGM_ING_Group_FK2_IncidentManagementGroup] FOREIGN KEY ([IGM_ING_Group]) REFERENCES [dbo].[IncidentManagementGroup]([ING_PK]);

CREATE CLUSTERED INDEX [NR_RC__IncidentManagementGroupMessage_ING_Group_Type_IsPublished] ON [IncidentManagementGroupMessage] (IGM_ING_Group, IGM_Type, IGM_IsPublished)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentManagementGroupMessage]
ADD CONSTRAINT [PK_UX__IGM_PK] PRIMARY KEY NONCLUSTERED ([IGM_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
", "DROP TABLE IncidentManagementGroupMessage");
			}
		}

		#endregion

		#region IncidentManagementLink

		static DatabaseObjectCreateScript IncidentManagementLink
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentManagementLink", @"
CREATE TABLE dbo.IncidentManagementLink
(
	[INL_PK] UNIQUEIDENTIFIER NOT NULL,
	[INL_IsGroupControlled] BIT NOT NULL DEFAULT 1,
	[INL_GS_NKResponder] VARCHAR(3) NOT NULL DEFAULT '',
	[INL_ING_Group] UNIQUEIDENTIFIER NOT NULL,
	[INL_IM_Incident] UNIQUEIDENTIFIER NOT NULL,
	[INL_SystemCreateTimeUtc] [smalldatetime] NOT NULL DEFAULT GetUtcDate(),
	[INL_SystemCreateUser] [varchar](3) NOT NULL DEFAULT (''),
);

ALTER TABLE [IncidentManagementLink]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentManagementLink]
ADD CONSTRAINT [IncidentManagementLink_INL_IM_Incident_FK2_IncidentMain] FOREIGN KEY ([INL_IM_Incident]) REFERENCES [dbo].[IncidentMain] ([IM_PK]);

ALTER TABLE [IncidentManagementLink]
ADD CONSTRAINT [IncidentManagementLink_INL_ING_Group_FK2_IncidentManagementGroup] FOREIGN KEY ([INL_ING_Group]) REFERENCES [dbo].[IncidentManagementGroup] ([ING_PK]);

CREATE CLUSTERED INDEX [NR_RC__IncidentManagementLink_ING_Group_IM_Incident] ON [IncidentManagementLink] (INL_ING_Group, INL_IM_Incident)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [FK_UX__INL_IM_Incident] ON [IncidentManagementLink] ([INL_IM_Incident] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentManagementLink]
ADD CONSTRAINT [PK_UX__INL_PK] PRIMARY KEY NONCLUSTERED ([INL_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
", "DROP TABLE IncidentManagementLink");
			}
		}

		#endregion

		#region IncidentTriage

		static DatabaseObjectCreateScript IncidentTriage
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentTriage", @"
CREATE TABLE dbo.IncidentTriage
(
   [IMT_PK] UNIQUEIDENTIFIER NOT NULL,
   [IMT_TriageNumber] VARCHAR(20) NOT NULL DEFAULT '',
   [IMT_SupportDescription] NVARCHAR(160) NOT NULL DEFAULT '',
   [IMT_IsActive] BIT NOT NULL DEFAULT 1,
   [IMT_IsInternal] BIT NOT NULL DEFAULT 0,
   [IMT_IsPublished] BIT NOT NULL DEFAULT 0,
   [IMT_IsPublishedToAssist] BIT NOT NULL DEFAULT 0,
   [IMT_Type] VARCHAR(3) NOT NULL DEFAULT 'SPT',
   [IMT_Level] CHAR(1) NOT NULL DEFAULT '2',
   [IMT_Product] VARCHAR(3) NOT NULL DEFAULT '',
   [IMT_ProductArea] VARCHAR(3) NOT NULL DEFAULT '',
   [IMT_SetProductAreaByMenuItem] BIT NOT NULL DEFAULT 0,
   [IMT_Module] VARCHAR(3) NOT NULL DEFAULT '',
   [IMT_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMT_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IMT_SystemCreateBranch] VARCHAR(3) NOT NULL DEFAULT '',
   [IMT_SystemCreateDepartment] VARCHAR(3) NOT NULL DEFAULT '',
   [IMT_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMT_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [IncidentTriage]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentTriage]
ADD CONSTRAINT [PK_UX__IMT_PK] PRIMARY KEY NONCLUSTERED ([IMT_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

DROP INDEX IF EXISTS [NR_UX__IMT_TriageNumber] ON [IncidentTriage]
;

CREATE UNIQUE CLUSTERED INDEX [NR_UC__IMT_TriageNumber] ON [IncidentTriage] ([IMT_TriageNumber] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IMT_Product__IMT_ProductArea__IMT_Module] ON [IncidentTriage] ([IMT_Product] ASC, [IMT_ProductArea] ASC, [IMT_Module] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentTriage] WITH NOCHECK 
ADD CONSTRAINT [Constraint_IMT_Level] CHECK (IMT_Level in ('1', '2'))
;
				", "DROP TABLE IncidentTriage");
			}
		}

		#endregion

		#region IncidentTriageChecklistItem

		static DatabaseObjectCreateScript IncidentTriageChecklistItem
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentTriageChecklistItem", @"
CREATE TABLE dbo.IncidentTriageChecklistItem
(
   [IMC_PK] UNIQUEIDENTIFIER NOT NULL,
   [IMC_ChecklistNumber] VARCHAR(20) NOT NULL DEFAULT '',
   [IMC_SupportDescription] NVARCHAR(160) NOT NULL DEFAULT '',
   [IMC_IsPublished] BIT NOT NULL DEFAULT 0,
   [IMC_Category] VARCHAR(3) NOT NULL DEFAULT '',
   [IMC_ResponseType] VARCHAR(3) NOT NULL DEFAULT '',
   [IMC_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMC_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IMC_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMC_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [IncidentTriageChecklistItem]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentTriageChecklistItem]
ADD CONSTRAINT [PK_UX__IMC_PK] PRIMARY KEY NONCLUSTERED ([IMC_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

DROP INDEX IF EXISTS [NR_UX__IMC_ChecklistNumber] ON [IncidentTriageChecklistItem]
;

CREATE UNIQUE CLUSTERED INDEX [NR_UC__IMC_ChecklistNumber] ON [IncidentTriageChecklistItem] ([IMC_ChecklistNumber] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
				", "DROP TABLE IncidentTriageChecklistItem");
			}
		}

		#endregion

		#region IncidentTriageChecklistItemPivot

		static DatabaseObjectCreateScript IncidentTriageChecklistItemPivot
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentTriageChecklistItemPivot", @"
CREATE TABLE dbo.IncidentTriageChecklistItemPivot
(
   [IMP_PK] UNIQUEIDENTIFIER NOT NULL,
   [IMP_IMT_Triage] UNIQUEIDENTIFIER NOT NULL,
   [IMP_IMC_ChecklistItem] UNIQUEIDENTIFIER NOT NULL,
   [IMP_Sequence] SMALLINT NOT NULL DEFAULT 1,
   [IMP_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMP_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IMP_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMP_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [IncidentTriageChecklistItemPivot]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentTriageChecklistItemPivot]
ADD CONSTRAINT [PK_UX__IMP_PK] PRIMARY KEY CLUSTERED ([IMP_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [NR_RX__IMP_IMT_Triage__IMP_IMC_ChecklistItem] ON [IncidentTriageChecklistItemPivot] ([IMP_IMT_Triage] ASC, [IMP_IMC_ChecklistItem] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentTriageChecklistItemPivot] WITH NOCHECK
	  ADD CONSTRAINT [IncidentTriageChecklistItemPivot_IMP_IMT_Triage_FK2_IncidentTriage_RRR_120N] FOREIGN KEY
		  ( [IMP_IMT_Triage] )
		  REFERENCES [IncidentTriage]
		  ( [IMT_PK] )

ALTER TABLE [IncidentTriageChecklistItemPivot] WITH NOCHECK
	  ADD CONSTRAINT [IncidentTriageChecklistItemPivot_IMP_IMC_ChecklistItem_FK2_IncidentTriageChecklistItem_RRR_120N] FOREIGN KEY
		  ( [IMP_IMC_ChecklistItem] )
		  REFERENCES [IncidentTriageChecklistItem]
		  ( [IMC_PK] )

				", "DROP TABLE IncidentTriageChecklistItemPivot");
			}
		}

		#endregion

		#region IncidentDiagnosticCriteria

		static DatabaseObjectCreateScript IncidentDiagnosticCriteria
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentDiagnosticCriteria", @"
CREATE TABLE dbo.IncidentDiagnosticCriteria
(
   [IMD_PK] UNIQUEIDENTIFIER NOT NULL,
   [IMD_Type] CHAR(3) NOT NULL DEFAULT '',
   [IMD_FocusRelatedTriageNodesOnly] BIT NOT NULL DEFAULT 0,
   [IMD_Description] NVARCHAR(256) NOT NULL DEFAULT '',
   [IMD_IsActive] BIT NOT NULL DEFAULT 1,
   [IMD_Keywords] NVARCHAR(256) NOT NULL DEFAULT '',
   [IMD_Question] NVARCHAR(MAX) NOT NULL DEFAULT '',
   [IMD_InternalSupportNote] NVARCHAR(MAX) NOT NULL DEFAULT '',
   [IMD_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IMD_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [IncidentDiagnosticCriteria]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentDiagnosticCriteria]
ADD CONSTRAINT [PK_UX__IMD_PK] PRIMARY KEY NONCLUSTERED ([IMD_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentDiagnosticCriteria] WITH NOCHECK 
ADD CONSTRAINT [Constraint_IMD_Type] CHECK (IMD_Type IN ('', 'SMP', 'DIA'));

CREATE CLUSTERED INDEX [NR_RC__IMD_Type_IMD_Keywords] ON [IncidentDiagnosticCriteria] ([IMD_Type] ASC, [IMD_Keywords] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF) 
;

CREATE UNIQUE INDEX NR_UX__IMD_Type_IMD_Description ON [IncidentDiagnosticCriteria] ([IMD_Type], [IMD_Description])
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
; 
				", "DROP TABLE IncidentDiagnosticCriteria");
			}
		}

		#endregion

		#region IncidentDiagnosticCriteriaPivot

		static DatabaseObjectCreateScript IncidentDiagnosticCriteriaPivot
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentDiagnosticCriteriaPivot", @"
CREATE TABLE dbo.IncidentDiagnosticCriteriaPivot
(
   [IMV_PK] UNIQUEIDENTIFIER NOT NULL,
   [IMV_IMD_DiagnosticCriteria] UNIQUEIDENTIFIER NOT NULL,
   [IMV_ParentID] UNIQUEIDENTIFIER NOT NULL,
   [IMV_ParentTableCode] VARCHAR(3) NOT NULL DEFAULT '',
   [IMV_Status] VARCHAR(3) NOT NULL DEFAULT '',
   [IMV_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMV_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IMV_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMV_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [IncidentDiagnosticCriteriaPivot]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentDiagnosticCriteriaPivot]
ADD CONSTRAINT [PK_UX__IMV_PK] PRIMARY KEY NONCLUSTERED ([IMV_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentDiagnosticCriteriaPivot] WITH NOCHECK
	ADD CONSTRAINT [IncidentDiagnosticCriteriaPivot_IMV_IMD_DiagnosticCriteria_FK2_IncidentDiagnosticCriteria_PK] FOREIGN KEY
		( [IMV_IMD_DiagnosticCriteria] )
		REFERENCES [IncidentDiagnosticCriteria]
		( [IMD_PK] );

ALTER TABLE [IncidentDiagnosticCriteriaPivot] WITH NOCHECK 
ADD CONSTRAINT [Constraint_IMV_Status] CHECK (IMV_Status IN ('', 'VER', 'UNV', 'NEG', 'INV'));

ALTER TABLE [IncidentDiagnosticCriteriaPivot] WITH NOCHECK 
ADD CONSTRAINT [Constraint_IMV_ParentTableCode] CHECK (IMV_ParentTableCode IN ('IM', 'ING'));

CREATE UNIQUE CLUSTERED INDEX NR_UC__IMV_IMD_DiagnosticCriteria_IMV_ParentID ON [IncidentDiagnosticCriteriaPivot] ([IMV_IMD_DiagnosticCriteria], [IMV_ParentID])
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
				", "DROP TABLE IncidentDiagnosticCriteriaPivot");
			}
		}

		#endregion

		#region IncidentTriageDiagnosticCriteriaPivot

		static DatabaseObjectCreateScript IncidentTriageDiagnosticCriteriaPivot
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentTriageDiagnosticCriteriaPivot", @"
CREATE TABLE dbo.IncidentTriageDiagnosticCriteriaPivot
(
   [IMO_PK] UNIQUEIDENTIFIER NOT NULL,
   [IMO_IMD_DiagnosticCriteria] UNIQUEIDENTIFIER NOT NULL,
   [IMO_IMT_Triage] UNIQUEIDENTIFIER NOT NULL,
   [IMO_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMO_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IMO_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IMO_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [IncidentTriageDiagnosticCriteriaPivot]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentTriageDiagnosticCriteriaPivot]
ADD CONSTRAINT [PK_UX__IMO_PK] PRIMARY KEY NONCLUSTERED ([IMO_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [IncidentTriageDiagnosticCriteriaPivot] WITH NOCHECK
	ADD CONSTRAINT [IncidentTriageDiagnosticCriteriaPivot_IMO_IMD_DiagnosticCriteria_FK2_IncidentDiagnosticCriteria_PK] FOREIGN KEY
		( [IMO_IMD_DiagnosticCriteria] )
		REFERENCES [IncidentDiagnosticCriteria]
		( [IMD_PK] );


ALTER TABLE [IncidentTriageDiagnosticCriteriaPivot] WITH NOCHECK
	ADD CONSTRAINT [IncidentTriageDiagnosticCriteriaPivot_IMO_IMT_Triage_FK2_IncidentTriage_PK] FOREIGN KEY
		( [IMO_IMT_Triage] )
		REFERENCES [IncidentTriage]
		( [IMT_PK] );


CREATE UNIQUE CLUSTERED INDEX NR_UC__IMO_IMD_DiagnosticCriteria_IMO_IMT_Triage ON [IncidentTriageDiagnosticCriteriaPivot] ([IMO_IMD_DiagnosticCriteria], [IMO_IMT_Triage])
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
				", "DROP TABLE IncidentTriageDiagnosticCriteriaPivot");
			}
		}

		#endregion

		#region InvestigationItem

		static DatabaseObjectCreateScript InvestigationItem
		{
			get
			{
				return new DatabaseObjectCreateScript("InvestigationItem", @"
CREATE TABLE dbo.InvestigationItem
(
   [INV_PK] UNIQUEIDENTIFIER NOT NULL,
   [INV_ItemNumber] VARCHAR(20) NOT NULL DEFAULT '',
   [INV_Type] VARCHAR(3) NOT NULL DEFAULT '',
   [INV_Description] NVARCHAR(256) NOT NULL DEFAULT '',
   [INV_IsActive] BIT NOT NULL DEFAULT 1,
   [INV_ItemText] NVARCHAR(MAX) NOT NULL DEFAULT '',
   [INV_AskClient] BIT NOT NULL DEFAULT 1,
);
 
ALTER TABLE [InvestigationItem]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [InvestigationItem]
ADD CONSTRAINT [PK_UX__INV_PK] PRIMARY KEY NONCLUSTERED ([INV_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

DROP INDEX IF EXISTS [NR_UX__INV_ItemNumber] ON [InvestigationItem]
;

CREATE UNIQUE CLUSTERED INDEX [NR_UC__INV_ItemNumber] ON [InvestigationItem] ([INV_ItemNumber] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

				", "DROP TABLE InvestigationItem");
			}
		}

		#endregion

		#region InvestigationItemResponseOption

		static DatabaseObjectCreateScript InvestigationItemResponseOption
		{
			get
			{
				return new DatabaseObjectCreateScript("InvestigationItemResponseOption", @"
CREATE TABLE dbo.InvestigationItemResponseOption
(
   [INR_PK] UNIQUEIDENTIFIER NOT NULL,
   [INR_INV_InvestigationItem] UNIQUEIDENTIFIER NOT NULL,
   [INR_ResponseOption] VARCHAR(40) NOT NULL DEFAULT '',
   [INR_Sequence] SMALLINT NOT NULL DEFAULT 1,
);
 
ALTER TABLE [InvestigationItemResponseOption]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [InvestigationItemResponseOption]
ADD CONSTRAINT [PK_UX__INR_PK] PRIMARY KEY NONCLUSTERED ([INR_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [InvestigationItemResponseOption] WITH NOCHECK
	ADD CONSTRAINT [InvestigationItemResponseOption_INR_INV_InvestigationItem_FK2_InvestigationItem_PK] FOREIGN KEY
		( [INR_INV_InvestigationItem] )
		REFERENCES [InvestigationItem]
		( [INV_PK] );

CREATE CLUSTERED INDEX [NR_RC__INR_Sequence_INR_ResponseOption] ON [InvestigationItemResponseOption] ([INR_Sequence] ASC, [INR_ResponseOption] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF) 
;
				", "DROP TABLE InvestigationItemResponseOption");
			}
		}

		#endregion

		#region DiagnosticCriteriaInvestigationItemLink

		static DatabaseObjectCreateScript DiagnosticCriteriaInvestigationItemLink
		{
			get
			{
				return new DatabaseObjectCreateScript("DiagnosticCriteriaInvestigationItemLink", @"
CREATE TABLE dbo.DiagnosticCriteriaInvestigationItemLink
(
	[DIL_PK] UNIQUEIDENTIFIER NOT NULL,
	[DIL_IMD_DiagnosticCriteria] UNIQUEIDENTIFIER NOT NULL,
	[DIL_INV_InvestigationItem] UNIQUEIDENTIFIER NOT NULL,
	[DIL_Order] INT NOT NULL DEFAULT 1,
);

ALTER TABLE [DiagnosticCriteriaInvestigationItemLink]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [DiagnosticCriteriaInvestigationItemLink]
ADD CONSTRAINT [DiagnosticCriteriaInvestigationItemLink_DIL_IMD_DiagnosticCriteria_FK2_DiagnosticCriteria_PK] FOREIGN KEY ([DIL_IMD_DiagnosticCriteria]) REFERENCES [dbo].[IncidentDiagnosticCriteria] ([IMD_PK]);

ALTER TABLE [DiagnosticCriteriaInvestigationItemLink]
ADD CONSTRAINT [IncidentManagementLink_INL_INV_InvestigationItem_FK2_InvestigationItem_PK] FOREIGN KEY ([DIL_INV_InvestigationItem]) REFERENCES [dbo].[InvestigationItem] ([INV_PK]);

CREATE CLUSTERED INDEX [NR_RC__DiagnosticCriteriaInvestigationItemLink_IMD_DiagnosticCriteria_INV_InvestigationItem] ON [DiagnosticCriteriaInvestigationItemLink] (DIL_IMD_DiagnosticCriteria, DIL_INV_InvestigationItem)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [DiagnosticCriteriaInvestigationItemLink]
ADD CONSTRAINT [PK_UX__DIL_PK] PRIMARY KEY NONCLUSTERED ([DIL_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;


				", "DROP TABLE DiagnosticCriteriaInvestigationItemLink");
			}
		}

		#endregion

		#region DiagnosticCriteriaInvestigationResult

		static DatabaseObjectCreateScript DiagnosticCriteriaInvestigationResult
		{
			get
			{
				return new DatabaseObjectCreateScript("DiagnosticCriteriaInvestigationResult", @"
CREATE TABLE dbo.DiagnosticCriteriaInvestigationResult
(
	[DCR_PK] UNIQUEIDENTIFIER NOT NULL,
	[DCR_DIL_ParentLink] UNIQUEIDENTIFIER NOT NULL,
	[DCR_INR_ResponseOption] UNIQUEIDENTIFIER NOT NULL,
	[DCR_ResponseResult] VARCHAR(3) NULL DEFAULT NULL,
);

ALTER TABLE [DiagnosticCriteriaInvestigationResult]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [DiagnosticCriteriaInvestigationResult]
ADD CONSTRAINT [DiagnosticCriteriaInvestigationResult_DCR_DIL_ParentLink_FK2_DiagnosticCriteriaInvestigationItemLink_PK] FOREIGN KEY ([DCR_DIL_ParentLink]) REFERENCES [dbo].[DiagnosticCriteriaInvestigationItemLink] ([DIL_PK]);

ALTER TABLE [DiagnosticCriteriaInvestigationResult]
ADD CONSTRAINT [DiagnosticCriteriaInvestigationResult_DCR_INR_ResponseOption_FK2_ResponseOption_PK] FOREIGN KEY ([DCR_INR_ResponseOption]) REFERENCES [dbo].[InvestigationItemResponseOption] ([INR_PK]);

CREATE CLUSTERED INDEX [NR_RC__DiagnosticCriteriaInvestigationItemLink_DIL_ParentLink_INR_ResponseOption] ON [DiagnosticCriteriaInvestigationResult] (DCR_DIL_ParentLink, DCR_INR_ResponseOption)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [DiagnosticCriteriaInvestigationResult]
ADD CONSTRAINT [PK_UX__DCR_PK] PRIMARY KEY NONCLUSTERED ([DCR_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
				", "DROP TABLE DiagnosticCriteriaInvestigationResult");
			}
		}

		#endregion

		#region IncidentMetrics

		static DatabaseObjectCreateScript IncidentMetrics
		{
			get
			{
				return new DatabaseObjectCreateScript("IncidentMetrics", @"
CREATE TABLE dbo.IncidentMetrics
(
   [IME_PK] UNIQUEIDENTIFIER NOT NULL,
   [IME_MetricCode] VARCHAR(3) NOT NULL,
   [IME_IncidentNumber] VARCHAR(20) NOT NULL,
   [IME_StartTimeUtc] DATETIME NULL,
   [IME_EndTimeUtc] DATETIME NULL,
   [IME_CalculatedMetric] INT NOT NULL DEFAULT 0,
   [IME_MetricCount] INT NOT NULL DEFAULT 0,
   [IME_SystemCreateTimeUtc] SMALLDATETIME NOT NULL DEFAULT GETUTCDATE(),
   [IME_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [IME_SystemLastEditTimeUtc] DATETIME NULL,
   [IME_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [IncidentMetrics]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [IncidentMetrics]
ADD CONSTRAINT [PK_UX__IME_PK] PRIMARY KEY NONCLUSTERED ([IME_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE CLUSTERED INDEX [NR_UC__IME_IncidentNumber_IME_PK] 
ON IncidentMetrics ([IME_IncidentNumber] ASC, [IME_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);
;
				", "DROP TABLE IncidentMetrics");
			}
		}

		#endregion

		#region Incident Estimate

		static DatabaseObjectCreateScript ClientIncidentEstimate
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientIncidentEstimate", @"
CREATE TABLE dbo.ClientIncidentEstimate
(
	[CIE_PK] [uniqueidentifier] NOT NULL,
	[CIE_IM] [uniqueidentifier] NOT NULL,
	[CIE_MinDevelopmentHours] [decimal](8,2) NOT NULL,
	[CIE_MaxDevelopmentHours] [decimal](8,2) NOT NULL,
	[CIE_EstimateSentDateUTC] [smalldatetime] NULL,
	[CIE_EstimateExpiryDateUTC] [smalldatetime] NULL,
	[CIE_MinEstimateMonthly] [money] NOT NULL,
	[CIE_MaxEstimateMonthly] [money] NOT NULL,
	[CIE_PaymentTerms] [varchar](3) NOT NULL,
	[CIE_MinEstimateOneoff] [money] NOT NULL,
	[CIE_MaxEstimateOneoff] [money] NOT NULL,
	[CIE_CancellationFee] [money] NOT NULL,
	[CIE_RX_NKCurrency] [varchar](3) NOT NULL,
	[CIE_ExpressDeliveryOptionCutOffDateUTC] [smalldatetime] NULL,
	[CIE_QuoteRequestedUTC] [smalldatetime] NULL,
);

ALTER TABLE [ClientIncidentEstimate]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_MinDevelopmentHours]  DEFAULT ((0)) FOR [CIE_MinDevelopmentHours];

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_MaxDevelopmentHours]  DEFAULT ((0)) FOR [CIE_MaxDevelopmentHours];

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_MinEstimateMonthly]  DEFAULT ((0)) FOR [CIE_MinEstimateMonthly];

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_MaxEstimateMonthly]  DEFAULT ((0)) FOR [CIE_MaxEstimateMonthly];

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_PaymentTerms]  DEFAULT ('') FOR [CIE_PaymentTerms];

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_MinEstimateOneoff]  DEFAULT ((0)) FOR [CIE_MinEstimateOneoff];

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_MaxEstimateOneoff]  DEFAULT ((0)) FOR [CIE_MaxEstimateOneoff];

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_CancellationFee]  DEFAULT ((0)) FOR [CIE_CancellationFee];

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [DF_CIE_RX_NKCurrency]  DEFAULT ('') FOR [CIE_RX_NKCurrency];

ALTER TABLE [ClientIncidentEstimate] WITH CHECK ADD  CONSTRAINT [ClientIncidentEstimate_CIE_IM_FK2_IncidentMain] FOREIGN KEY([CIE_IM]) REFERENCES [dbo].[IncidentMain] ([IM_PK]);

ALTER TABLE [ClientIncidentEstimate] ADD  CONSTRAINT [PK_UX__CIE_PK] PRIMARY KEY NONCLUSTERED 
(
	[CIE_PK] ASC
)WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY];

CREATE UNIQUE CLUSTERED INDEX [FK_UC__CIE_IM] ON [ClientIncidentEstimate]
(
	[CIE_IM] ASC
)WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY];


", "DROP TABLE ClientIncidentEstimate");
			}
		}

		#endregion

		#region Incident Quote

		static DatabaseObjectCreateScript ClientIncidentQuote
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientIncidentQuote", @"
CREATE TABLE dbo.ClientIncidentQuote
(
	[CIQ_PK] [uniqueidentifier] NOT NULL,
	[CIQ_IM] [uniqueidentifier] NOT NULL,
	[CIQ_MinDevelopmentHours] [decimal](8,2) NOT NULL,
	[CIQ_MaxDevelopmentHours] [decimal](8,2) NOT NULL,
	[CIQ_QuoteSentDateUTC] [smalldatetime] NULL,
	[CIQ_QuoteExpiryDateUTC] [smalldatetime] NULL,
	[CIQ_QuoteAcceptedDateUTC] [smalldatetime] NULL,
	[CIQ_DeliveredDateUTC] [smalldatetime] NULL,
	[CIQ_QuoteAmount] [money] NOT NULL,
	[CIQ_CancellationFee] [money] NOT NULL,
	[CIQ_Type] [varchar](3) NOT NULL,
	[CIQ_OneoffUpfront] [money] NOT NULL,
	[CIQ_PaymentTerms] [varchar](3) NOT NULL,
	[CIQ_RX_NKCurrency] [varchar](3) NOT NULL,
	[CIQ_HeadStartOptionIncluded] [bit] NOT NULL,
	[CIQ_HeadStartSurcharge] [money] NOT NULL,
	[CIQ_ExpressDeliveryOptionIncluded] [bit] NOT NULL,
	[CIQ_ExpressDeliverySurcharge] [money] NOT NULL,
);

ALTER TABLE [ClientIncidentQuote]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_MinDevelopmentHours]  DEFAULT ((0)) FOR [CIQ_MinDevelopmentHours];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_MaxDevelopmentHours]  DEFAULT ((0)) FOR [CIQ_MaxDevelopmentHours];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_QuoteAmount]  DEFAULT ((0)) FOR [CIQ_QuoteAmount];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_CancellationFee]  DEFAULT ((0)) FOR [CIQ_CancellationFee];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_Type]  DEFAULT ('') FOR [CIQ_Type];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_OneoffUpfront]  DEFAULT ((0)) FOR [CIQ_OneoffUpfront];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_PaymentTerms]  DEFAULT ('') FOR [CIQ_PaymentTerms];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_RX_NKCurrency]  DEFAULT ('') FOR [CIQ_RX_NKCurrency];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_HeadStartOptionIncluded]  DEFAULT ((0)) FOR [CIQ_HeadStartOptionIncluded];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_HeadStartSurcharge]  DEFAULT ((0)) FOR [CIQ_HeadStartSurcharge];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_ExpressDeliveryOptionIncluded]  DEFAULT ((0)) FOR [CIQ_ExpressDeliveryOptionIncluded];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [DF_CIQ_ExpressDeliverySurcharge]  DEFAULT ((0)) FOR [CIQ_ExpressDeliverySurcharge];

ALTER TABLE [ClientIncidentQuote] WITH CHECK ADD  CONSTRAINT [ClientIncidentQuote_CIQ_IM_FK2_IncidentMain] FOREIGN KEY([CIQ_IM]) REFERENCES [dbo].[IncidentMain] ([IM_PK]);

CREATE UNIQUE CLUSTERED INDEX [FK_UC__CIQ_IM] ON [ClientIncidentQuote]
(
	[CIQ_IM] ASC
)WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY];

ALTER TABLE [ClientIncidentQuote] ADD  CONSTRAINT [PK_UX__CIQ_PK] PRIMARY KEY NONCLUSTERED 
(
	[CIQ_PK] ASC
)WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
;


", "DROP TABLE ClientIncidentQuote");
			}
		}

		#endregion

		#region HelpErrorLogOccurrence

		static DatabaseObjectCreateScript HelpErrorLogOccurrence
		{
			get
			{
				return new DatabaseObjectCreateScript("HelpErrorLogOccurrence", @"
CREATE TABLE dbo.HelpErrorLogOccurrence
(
	[HO_PK] UNIQUEIDENTIFIER NOT NULL,
	[HO_HE] UNIQUEIDENTIFIER NOT NULL,
	[HO_ServerName] VARCHAR(64) NOT NULL DEFAULT '' ,
	[HO_Company] VARCHAR(50) NOT NULL DEFAULT '' ,
	[HO_ExceptionDateTime] SMALLDATETIME NULL,
	[HO_VersionNumber] VARCHAR(32) NOT NULL DEFAULT '' ,
	[HO_EXEDateTime] SMALLDATETIME NULL,
	[HO_CompressedXmlData] VARBINARY(MAX) NOT NULL DEFAULT 0x,
	[HO_ExceptionID] VARCHAR(19) NOT NULL DEFAULT '' ,
	[HO_LD] UNIQUEIDENTIFIER NULL,
	[HO_LCC] UNIQUEIDENTIFIER NULL,
	[HO_HL] UNIQUEIDENTIFIER NULL,
	[HO_SessionID] UNIQUEIDENTIFIER NULL,
	[HO_Sequence] int NOT NULL DEFAULT(-1),
);

ALTER TABLE [HelpErrorLogOccurrence]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [HelpErrorLogOccurrence]
ADD CONSTRAINT [PK_UX__HO_PK] PRIMARY KEY NONCLUSTERED  ([HO_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

CREATE NONCLUSTERED INDEX [FK_RX__HO_HE_HO_EXEDateTime] ON [HelpErrorLogOccurrence] ([HO_HE] ASC,[HO_EXEDateTime] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE NONCLUSTERED INDEX [FK_RX__HO_HL] ON [HelpErrorLogOccurrence] ([HO_HL] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE NONCLUSTERED INDEX [FK_RX__HO_LD] ON [HelpErrorLogOccurrence] ([HO_LD] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE NONCLUSTERED INDEX [FK_RX__HO_LCC] ON [HelpErrorLogOccurrence] ([HO_LCC] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE CLUSTERED INDEX [NR_RC__HO_ExceptionDateTime] ON [HelpErrorLogOccurrence] ([HO_ExceptionDateTime] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE NONCLUSTERED INDEX [NR_RX__HO_ExceptionID] on [HelpErrorLogOccurrence] (HO_ExceptionID) include (HO_HE) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE NONCLUSTERED INDEX [NR_RX__HO_SessionID] ON [HelpErrorLogOccurrence] ([HO_SessionID] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [HelpErrorLogOccurrence] WITH NOCHECK
	  ADD CONSTRAINT [HelpErrorLogOccurrence_HO_HE_FK2_HelpErrorLog_RRR_120N] FOREIGN KEY
		  ( [HO_HE] )
		  REFERENCES [HelpErrorLog]
		  ( [HE_PK] )
;

ALTER TABLE [HelpErrorLogOccurrence] WITH NOCHECK
	  ADD CONSTRAINT [HelpErrorLogOccurrence_HO_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		  ( [HO_LD] )
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] )
;

ALTER TABLE [HelpErrorLogOccurrence] WITH NOCHECK
	  ADD CONSTRAINT [HelpErrorLogOccurrence_HO_LCC_FK2_ClientCompany_RRR_120N] FOREIGN KEY
		  ( [HO_LCC] )
		  REFERENCES [ClientCompany]
		  ( [LCC_PK] )
;

ALTER TABLE [HelpErrorLogOccurrence] WITH NOCHECK
	  ADD CONSTRAINT [HelpErrorLogOccurrence_HO_HL_FK2_ReleaseBuild_RRR_120N] FOREIGN KEY
		  ( [HO_HL] )
		  REFERENCES [ReleaseBuild]
		  ( [HL_PK] )
;

", "DROP TABLE HelpErrorLogOccurrence");
			}
		}

		#endregion

		#region LicenceModules

		static DatabaseObjectCreateScript LicenceModules
		{
			get
			{
				return new DatabaseObjectCreateScript("LicenceModules", @"
CREATE TABLE dbo.LicenceModules
(
   [LM_PK] UNIQUEIDENTIFIER NOT NULL,
   [LM_GroupModuleCode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LM_UserCount] SMALLINT NOT NULL DEFAULT 0 ,
   [LM_PartPurchasedCount] SMALLINT NOT NULL DEFAULT 0 ,
   [LM_LicenceType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LM_ExpiryDate] SMALLDATETIME NULL,
   [LM_Checksum] DECIMAL(38,0) NOT NULL DEFAULT 0 ,
   [LM_RenewalUserCount] INT NOT NULL DEFAULT 0 ,
   [LM_LA] UNIQUEIDENTIFIER NULL,
);

ALTER TABLE [LicenceModules]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [LicenceModules]
ADD CONSTRAINT [PK_UX__LM_PK] PRIMARY KEY NONCLUSTERED  ([LM_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__LM_LA] ON [LicenceModules] ([LM_LA] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [LicenceModules] WITH NOCHECK
	  ADD CONSTRAINT [LicenceModules_LM_LA_FK2_LicenceHeader_RRR_120N] FOREIGN KEY
		  ( [LM_LA] )
		  REFERENCES [LicenceHeader]
		  ( [LA_PK] )
;

CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LM_LA_LM_GroupModuleCode ON LicenceModules (LM_LA, LM_GroupModuleCode) WITH (ALLOW_PAGE_LOCKS = OFF);

", "DROP TABLE LicenceModules");
			}
		}

		#endregion

		#region LicenceCompany

		static DatabaseObjectCreateScript LicenceCompany
		{
			get
			{
				return new DatabaseObjectCreateScript("LicenceCompany", @"
CREATE TABLE dbo.LicenceCompany
(
   [LC_PK] UNIQUEIDENTIFIER NOT NULL,
   [LC_CompanyCode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LC_CompanyCountry] VARCHAR(2) NOT NULL DEFAULT '' ,
   [LC_IsReciprocal] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LC_IsGSTRegistered] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LC_IsGSTCashBasis] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LC_IsWHTRegistered] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LC_IsWHTCashBasis] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LC_RX_NKCurrency] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LC_OH] UNIQUEIDENTIFIER NOT NULL,
   [LC_LE] UNIQUEIDENTIFIER NOT NULL,
	[LC_CompanyNumber] INT NOT NULL DEFAULT 0,
);

ALTER TABLE [LicenceCompany]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [LicenceCompany]
ADD CONSTRAINT [PK_UC__LC_PK] PRIMARY KEY CLUSTERED  ([LC_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE UNIQUE NONCLUSTERED INDEX [FK_UX__LC_OH] ON [LicenceCompany] ([LC_OH] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__LC_LE] ON [LicenceCompany] ([LC_LE] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LC_CompanyCode_LC_LE] ON [LicenceCompany] ([LC_CompanyCode] ASC,[LC_LE] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LC_CompanyNumber] ON [LicenceCompany] ([LC_CompanyNumber] ASC)
WHERE LC_CompanyNumber != 0
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [LicenceCompany] WITH NOCHECK
	  ADD CONSTRAINT [LicenceCompany_LC_OH_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [LC_OH] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

ALTER TABLE [LicenceCompany] WITH NOCHECK
	  ADD CONSTRAINT [LicenceCompany_LC_LE_FK2_LicenceEnterprise_RRR_120N] FOREIGN KEY
		  ( [LC_LE] )
		  REFERENCES [LicenceEnterprise]
		  ( [LE_PK] )
;

ALTER TABLE [LicenceCompany] WITH CHECK ADD CONSTRAINT [Constraint_LC_CompanyCode] CHECK (LC_CompanyCode <> '');

", "DROP TABLE LicenceCompany");
			}
		}

		#endregion

		#region HelpErrorLogKey

		static DatabaseObjectCreateScript HelpErrorLogKey
		{
			get
			{
				return new DatabaseObjectCreateScript("HelpErrorLogKey", @"
CREATE TABLE dbo.HelpErrorLogKey
(
   [HK_PK] UNIQUEIDENTIFIER NOT NULL,
   [HK_HashCode] INT NOT NULL DEFAULT 0 ,
   [HK_Key] VARCHAR(MAX) NOT NULL DEFAULT '' ,
   [HK_HE] UNIQUEIDENTIFIER NOT NULL,
);

ALTER TABLE [HelpErrorLogKey]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [HelpErrorLogKey]
ADD CONSTRAINT [PK_UX__HK_PK] PRIMARY KEY NONCLUSTERED  ([HK_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__HK_HE] ON [HelpErrorLogKey] ([HK_HE] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [NR_RX__HK_HashCode] ON [HelpErrorLogKey] ([HK_HashCode] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [HelpErrorLogKey] WITH NOCHECK
	  ADD CONSTRAINT [HelpErrorLogKey_HK_HE_FK2_HelpErrorLog_RRR_120N] FOREIGN KEY
		  ( [HK_HE] )
		  REFERENCES [HelpErrorLog]
		  ( [HE_PK] )
;

", "DROP TABLE HelpErrorLogKey");
			}
		}

		#endregion

		#region Licence3rdPartySoftware

		static DatabaseObjectCreateScript Licence3rdPartySoftware
		{
			get
			{
				return new DatabaseObjectCreateScript("Licence3rdPartySoftware", @"
CREATE TABLE dbo.Licence3rdPartySoftware
(
   [L3_PK] UNIQUEIDENTIFIER NOT NULL,
   [L3_OP_ProductSKU] UNIQUEIDENTIFIER NULL,
   [L3_OH_Supplier] UNIQUEIDENTIFIER NULL,
   [L3_OSType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [L3_LicenceType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [L3_LicenceIssued] SMALLDATETIME NULL,
   [L3_LicenceCount] SMALLINT NOT NULL DEFAULT 0 ,
   [L3_UpgradeAssuranceStartsOn] SMALLDATETIME NULL,
   [L3_UpgradeAssuranceEndsOn] SMALLDATETIME NULL,
   [L3_LC_LicenceCompany] UNIQUEIDENTIFIER NOT NULL,
);

ALTER TABLE [Licence3rdPartySoftware]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [Licence3rdPartySoftware]
ADD CONSTRAINT [PK_UX__L3_PK] PRIMARY KEY NONCLUSTERED  ([L3_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__L3_LC_LicenceCompany] ON [Licence3rdPartySoftware] ([L3_LC_LicenceCompany] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__L3_OP_ProductSKU] ON [Licence3rdPartySoftware] ([L3_OP_ProductSKU] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__L3_OH_Supplier] ON [Licence3rdPartySoftware] ([L3_OH_Supplier] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [Licence3rdPartySoftware] WITH NOCHECK
	  ADD CONSTRAINT [Licence3rdPartySoftware_L3_LC_LicenceCompany_FK2_LicenceCompany_RRR_120N] FOREIGN KEY
		  ( [L3_LC_LicenceCompany] )
		  REFERENCES [LicenceCompany]
		  ( [LC_PK] )
;

ALTER TABLE [Licence3rdPartySoftware] WITH NOCHECK
	  ADD CONSTRAINT [Licence3rdPartySoftware_L3_OP_ProductSKU_FK2_OrgSupplierPart_RRR_120N] FOREIGN KEY
		  ( [L3_OP_ProductSKU] )
		  REFERENCES [OrgSupplierPart]
		  ( [OP_PK] )
;

ALTER TABLE [Licence3rdPartySoftware] WITH NOCHECK
	  ADD CONSTRAINT [Licence3rdPartySoftware_L3_OH_Supplier_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [L3_OH_Supplier] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

", "DROP TABLE Licence3rdPartySoftware");
			}
		}

		#endregion

		#region LicenceConnection

		static DatabaseObjectCreateScript LicenceConnection
		{
			get
			{
				return new DatabaseObjectCreateScript("LicenceConnection", @"
CREATE TABLE dbo.LicenceConnection
(
   [LK_PK] UNIQUEIDENTIFIER NOT NULL,
   [LK_ConnectionOrder] TINYINT NOT NULL DEFAULT 0 ,
   [LK_Description] VARCHAR(80) NOT NULL DEFAULT '' ,
   [LK_RemoteAccessMethod] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LK_RemoteAccessAddress] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LK_RemoteAccessUserName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LK_RemoteAccessPassWord] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LK_LD] UNIQUEIDENTIFIER NULL,
   [LK_LC_Company] UNIQUEIDENTIFIER NULL,
);

ALTER TABLE [LicenceConnection]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [LicenceConnection]
ADD CONSTRAINT [PK_UX__LK_PK] PRIMARY KEY NONCLUSTERED  ([LK_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__LK_LD] ON [LicenceConnection] ([LK_LD] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__LK_LC_Company] ON [LicenceConnection] ([LK_LC_Company] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [LicenceConnection] WITH NOCHECK
	  ADD CONSTRAINT [LicenceConnection_LK_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		  ( [LK_LD] )
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] )
;

ALTER TABLE [LicenceConnection] WITH NOCHECK
	  ADD CONSTRAINT [LicenceConnection_LK_LC_Company_FK2_LicenceCompany_RRR_120N] FOREIGN KEY
		  ( [LK_LC_Company] )
		  REFERENCES [LicenceCompany]
		  ( [LC_PK] )
;

", "DROP TABLE LicenceConnection");
			}
		}

		#endregion

		#region ReleaseBuild

		internal static DatabaseObjectCreateScript ReleaseBuild
		{
			get
			{
				return new DatabaseObjectCreateScript("ReleaseBuild", @"
CREATE TABLE dbo.ReleaseBuild
(
	[HL_PK] UNIQUEIDENTIFIER NOT NULL,
	[HL_Product] VARCHAR(3) NOT NULL DEFAULT 'ENT',
	[HL_ReleaseStatus] VARCHAR(3) NOT NULL DEFAULT '',
	[HL_ExeVersionDate] SMALLDATETIME NULL,
	[HL_IsRolledOut] BIT NOT NULL DEFAULT 0,
	[HL_MajorVersion] INT NOT NULL DEFAULT 0,
	[HL_MinorVersion] INT NOT NULL DEFAULT 0,
	[HL_Release] INT NOT NULL DEFAULT 0,
	[HL_Patch] INT NOT NULL DEFAULT 0,
	[HL_PackagePath] VARCHAR(256) NULL DEFAULT '',
	[HL_IsActive] BIT NOT NULL DEFAULT 1,
	[HL_Superceded] CHAR(1) NOT NULL DEFAULT 'Y',
	[HL_CompressedDLLs] VARBINARY(MAX) NULL,
	[HL_Comment] VARCHAR(256) NOT NULL DEFAULT '',
	[HL_IsTestPassed] BIT NOT NULL DEFAULT 0,
	[HL_TestDateUtc] SMALLDATETIME NULL,
	[HL_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[HL_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
	[HL_SystemLastEditTimeUtc] SMALLDATETIME NULL,
	[HL_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	CONSTRAINT [PK_UX__HL_PK] PRIMARY KEY NONCLUSTERED ([HL_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ReleaseBuild]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX NR_RC__ReleaseBuild_Product_Version ON ReleaseBuild (HL_Product,HL_MajorVersion,HL_MinorVersion,HL_Release,HL_Patch)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;
",
"DROP TABLE ReleaseBuild");
			}
		}

		#endregion

		#region GlbTime

		static DatabaseObjectCreateScript GlbTime
		{
			get
			{
				return new DatabaseObjectCreateScript("GlbTime", @"
CREATE TABLE dbo.GlbTime
(
   [GT_PK] UNIQUEIDENTIFIER NOT NULL,
   [GT_IsValid] BIT NOT NULL DEFAULT 0 ,
   [GT_BookingActual] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GT_StartTime] SMALLDATETIME NOT NULL,
   [GT_EndTime] SMALLDATETIME NOT NULL,
   [GT_Comment] VARCHAR(50) NOT NULL DEFAULT '' ,
   [GT_GS_NKStaffAssigned] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GT_OA_OverrideWorkLocation] UNIQUEIDENTIFIER NULL,
   [GT_GZ_ClassroomSession] UNIQUEIDENTIFIER NULL,
);

ALTER TABLE [GlbTime]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [GlbTime]
ADD CONSTRAINT [PK_UX__GT_PK] PRIMARY KEY NONCLUSTERED  ([GT_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__GT_OA_OverrideWorkLocation] ON [GlbTime] ([GT_OA_OverrideWorkLocation] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__GT_GZ_ClassroomSession] ON [GlbTime] ([GT_GZ_ClassroomSession] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [GlbTime] WITH NOCHECK
	  ADD CONSTRAINT [GlbTime_GT_OA_OverrideWorkLocation_FK2_OrgAddress_RRR_120N] FOREIGN KEY
		  ( [GT_OA_OverrideWorkLocation] )
		  REFERENCES [OrgAddress]
		  ( [OA_PK] )
;

ALTER TABLE [GlbTime] WITH NOCHECK
	  ADD CONSTRAINT [GlbTime_GT_GZ_ClassroomSession_FK2_GlbClassroomSession_RRR_1210] FOREIGN KEY
		  ( [GT_GZ_ClassroomSession] )
		  REFERENCES [GlbClassroomSession]
		  ( [GZ_PK] )
;

", "DROP TABLE GlbTime");
			}
		}

		#endregion

		#region GlbClassroomSession

		static DatabaseObjectCreateScript GlbClassroomSession
		{
			get
			{
				return new DatabaseObjectCreateScript("GlbClassroomSession", @"
CREATE TABLE dbo.GlbClassroomSession
(
   [GZ_PK] UNIQUEIDENTIFIER NOT NULL,
   [GZ_InvoiceRateType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GZ_IsCancelled] CHAR(1) NOT NULL DEFAULT 'N' ,
   [GZ_IsConfirmed] CHAR(1) NOT NULL DEFAULT 'N' ,
   [GZ_BookedFrom] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GZ_G3_TrainingSubject] UNIQUEIDENTIFIER NULL,
   [GZ_ExperienceLevel] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GZ_SessionStartTime] SMALLDATETIME NOT NULL,
   [GZ_SessionEndTime] SMALLDATETIME NOT NULL,
   [GZ_ActualStartTime] SMALLDATETIME NULL,
   [GZ_ActualEndTime] SMALLDATETIME NULL,
   [GZ_SessionMaxAttendees] SMALLINT NOT NULL DEFAULT 0 ,
   [GZ_SessionCost] MONEY NOT NULL DEFAULT 0 ,
   [GZ_RX_NK_SessionCostCurrency] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GZ_DiscountsApplyGreaterThan] SMALLINT NOT NULL DEFAULT 0 ,
   [GZ_DiscountPercent] SMALLINT NOT NULL DEFAULT 0 ,
   [GZ_Diem] CHAR(1) NOT NULL DEFAULT 'N' ,
   [GZ_GS_NKCoordinatorOfSession] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GZ_OA_SessionDeliveryLocation] UNIQUEIDENTIFIER NOT NULL,
   [GZ_G2_Course] UNIQUEIDENTIFIER NULL,
);

ALTER TABLE [GlbClassroomSession]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [GlbClassroomSession]
ADD CONSTRAINT [PK_UX__GZ_PK] PRIMARY KEY NONCLUSTERED  ([GZ_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__GZ_OA_SessionDeliveryLocation] ON [GlbClassroomSession] ([GZ_OA_SessionDeliveryLocation] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__GZ_G3_TrainingSubject] ON [GlbClassroomSession] ([GZ_G3_TrainingSubject] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__GZ_G2_Course] ON [GlbClassroomSession] ([GZ_G2_Course] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [GlbClassroomSession] WITH NOCHECK
	  ADD CONSTRAINT [GlbClassroomSession_GZ_OA_SessionDeliveryLocation_FK2_OrgAddress_RRR_120N] FOREIGN KEY
		  ( [GZ_OA_SessionDeliveryLocation] )
		  REFERENCES [OrgAddress]
		  ( [OA_PK] )
;

ALTER TABLE [GlbClassroomSession] WITH NOCHECK
	  ADD CONSTRAINT [GlbClassroomSession_GZ_G3_TrainingSubject_FK2_GlbClassroomSubject_RRR_120N] FOREIGN KEY
		  ( [GZ_G3_TrainingSubject] )
		  REFERENCES [GlbClassroomSubject]
		  ( [G3_PK] )
;

ALTER TABLE [GlbClassroomSession] WITH NOCHECK
	  ADD CONSTRAINT [GlbClassroomSession_GZ_G2_Course_FK2_GlbTrainingCourse_RRR_120N] FOREIGN KEY
		  ( [GZ_G2_Course] )
		  REFERENCES [GlbTrainingCourse]
		  ( [G2_PK] )
;

", "DROP TABLE GlbClassroomSession");
			}
		}

		#endregion

		#region GlbClassroomAttendee

		static DatabaseObjectCreateScript GlbClassroomAttendee
		{
			get
			{
				return new DatabaseObjectCreateScript("GlbClassroomAttendee", @"
CREATE TABLE dbo.GlbClassroomAttendee
(
   [GX_PK] UNIQUEIDENTIFIER NOT NULL,
   [GX_Type] VARCHAR(3) NOT NULL DEFAULT 'CLS' ,
   [GX_Name] VARCHAR(256) NOT NULL DEFAULT '' ,
   [GX_Company] VARCHAR(50) NOT NULL DEFAULT '' ,
   [GX_Branch] VARCHAR(35) NOT NULL DEFAULT '' ,
   [GX_WorkPhone] VARCHAR(20) NOT NULL DEFAULT '' ,
   [GX_Mobile] VARCHAR(20) NOT NULL DEFAULT '' ,
   [GX_HomePhone] VARCHAR(20) NOT NULL DEFAULT '' ,
   [GX_EmailAddress] NVARCHAR(254) NOT NULL DEFAULT '' ,
   [GX_DateOfBirth] SMALLDATETIME NULL,
   [GX_BillingType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GX_CreditCard] VARCHAR(20) NOT NULL DEFAULT '' ,
   [GX_Expiry] VARCHAR(4) NOT NULL DEFAULT '' ,
   [GX_IsBookingConfirmed] CHAR(1) NOT NULL DEFAULT 'N' ,
   [GX_IsBillingComplete] CHAR(1) NOT NULL DEFAULT 'N' ,
   [GX_IsCancelled] CHAR(1) NOT NULL DEFAULT 'N' ,
   [GX_AttendedSession] CHAR(1) NOT NULL DEFAULT 'N' ,
   [GX_OH_ClientOrgNotForWebPublish] UNIQUEIDENTIFIER NULL,
   [GX_OA_ClientAddressNoForWebPublish] UNIQUEIDENTIFIER NULL,
   [GX_GS_NKStaffMember] VARCHAR(3) NOT NULL DEFAULT '' ,
   [GX_G8] UNIQUEIDENTIFIER NULL,
   [GX_GZ_ClassroomSession] UNIQUEIDENTIFIER NOT NULL,
);

ALTER TABLE [GlbClassroomAttendee]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [GlbClassroomAttendee]
ADD CONSTRAINT [PK_UX__GX_PK] PRIMARY KEY NONCLUSTERED  ([GX_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__GX_GZ_ClassroomSession] ON [GlbClassroomAttendee] ([GX_GZ_ClassroomSession] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__GX_OH_ClientOrgNotForWebPublish] ON [GlbClassroomAttendee] ([GX_OH_ClientOrgNotForWebPublish] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__GX_OA_ClientAddressNoForWebPublish] ON [GlbClassroomAttendee] ([GX_OA_ClientAddressNoForWebPublish] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__GX_G8] ON [GlbClassroomAttendee] ([GX_G8] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [GlbClassroomAttendee] WITH NOCHECK
	  ADD CONSTRAINT [GlbClassroomAttendee_GX_GZ_ClassroomSession_FK2_GlbClassroomSession_RRR_120N] FOREIGN KEY
		  ( [GX_GZ_ClassroomSession] )
		  REFERENCES [GlbClassroomSession]
		  ( [GZ_PK] )
;

ALTER TABLE [GlbClassroomAttendee] WITH NOCHECK
	  ADD CONSTRAINT [GlbClassroomAttendee_GX_OH_ClientOrgNotForWebPublish_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [GX_OH_ClientOrgNotForWebPublish] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

ALTER TABLE [GlbClassroomAttendee] WITH NOCHECK
	  ADD CONSTRAINT [GlbClassroomAttendee_GX_OA_ClientAddressNoForWebPublish_FK2_OrgAddress_RRR_120N] FOREIGN KEY
		  ( [GX_OA_ClientAddressNoForWebPublish] )
		  REFERENCES [OrgAddress]
		  ( [OA_PK] )
;

ALTER TABLE [GlbClassroomAttendee] WITH NOCHECK
	  ADD CONSTRAINT [GlbClassroomAttendee_GX_G8_FK2_GlbCompanyCampaignItem_RRR_120N] FOREIGN KEY
		  ( [GX_G8] )
		  REFERENCES [GlbCompanyCampaignItem]
		  ( [G8_PK] )
;

", "DROP TABLE GlbClassroomAttendee");
			}
		}

		#endregion

		#region GlbClassroomSubject

		static DatabaseObjectCreateScript GlbClassroomSubject
		{
			get
			{
				return new DatabaseObjectCreateScript("GlbClassroomSubject", @"
CREATE TABLE dbo.GlbClassroomSubject
(
   [G3_PK] UNIQUEIDENTIFIER NOT NULL,
   [G3_IsActive] BIT NOT NULL DEFAULT 1 ,
   [G3_Type] VARCHAR(3) NOT NULL DEFAULT 'CLS' ,
   [G3_SubjectName] VARCHAR(50) NOT NULL DEFAULT '' ,
   [G3_SubjectContent] VARBINARY(MAX) NULL,
   [G3_SubjectRider] VARBINARY(MAX) NULL,
   [G3_SessionStandardDuration] SMALLDATETIME NULL,
   [G3_ExperienceLevel] VARCHAR(3) NOT NULL DEFAULT '' ,
   [G3_MaxAttendees] TINYINT NOT NULL DEFAULT 0 ,
   [G3_TrainingContentURL] VARCHAR(250) NOT NULL DEFAULT '' ,
);

ALTER TABLE [GlbClassroomSubject]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [GlbClassroomSubject]
ADD CONSTRAINT [PK_UX__G3_PK] PRIMARY KEY NONCLUSTERED  ([G3_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE UNIQUE CLUSTERED INDEX [NR_UC__G3_SubjectName] ON [GlbClassroomSubject] ([G3_SubjectName] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

				", "DROP TABLE GlbClassroomSubject");
			}
		}

		#endregion

		#region GlbTrainingCourse

		static DatabaseObjectCreateScript GlbTrainingCourse
		{
			get
			{
				return new DatabaseObjectCreateScript("GlbTrainingCourse", @"
CREATE TABLE dbo.GlbTrainingCourse
(
   [G2_PK] UNIQUEIDENTIFIER NOT NULL,
   [G2_BookingNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
   [G2_Type] VARCHAR(3) NOT NULL DEFAULT 'CLS' ,
   [G2_Locale] VARCHAR(3) NOT NULL DEFAULT '' ,
   [G2_TemplateName] VARCHAR(50) NOT NULL DEFAULT '' ,
   [G2_GoLive] SMALLDATETIME NULL,
   [G2_RX_NKRateCurrency] VARCHAR(3) NOT NULL DEFAULT '' ,
   [G2_HourlyRate] MONEY NOT NULL DEFAULT 0 ,
   [G2_DailyRate] MONEY NOT NULL DEFAULT 0 ,
   [G2_WeeklyRate] MONEY NOT NULL DEFAULT 0 ,
   [G2_InitialStartDate] SMALLDATETIME NOT NULL,
   [G2_GS_NKPrimeTrainer] VARCHAR(3) NOT NULL DEFAULT '' ,
   [G2_OA_TrainingAddress] UNIQUEIDENTIFIER NOT NULL,
   [G2_OC_TrainingContact] UNIQUEIDENTIFIER NULL,
   [G2_LA] UNIQUEIDENTIFIER NULL,
);

ALTER TABLE [GlbTrainingCourse]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [GlbTrainingCourse]
ADD CONSTRAINT [PK_UX__G2_PK] PRIMARY KEY NONCLUSTERED  ([G2_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__G2_OA_TrainingAddress] ON [GlbTrainingCourse] ([G2_OA_TrainingAddress] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__G2_LA] ON [GlbTrainingCourse] ([G2_LA] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__G2_OC_TrainingContact] ON [GlbTrainingCourse] ([G2_OC_TrainingContact] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [NR_RX__G2_BookingNumber] ON [GlbTrainingCourse] ([G2_BookingNumber] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [GlbTrainingCourse] WITH NOCHECK
	  ADD CONSTRAINT [GlbTrainingCourse_G2_OA_TrainingAddress_FK2_OrgAddress_RRR_120N] FOREIGN KEY
		  ( [G2_OA_TrainingAddress] )
		  REFERENCES [OrgAddress]
		  ( [OA_PK] )
;

ALTER TABLE [GlbTrainingCourse] WITH NOCHECK
	  ADD CONSTRAINT [GlbTrainingCourse_G2_LA_FK2_LicenceHeader_RRR_120N] FOREIGN KEY
		  ( [G2_LA] )
		  REFERENCES [LicenceHeader]
		  ( [LA_PK] )
;

ALTER TABLE [GlbTrainingCourse] WITH NOCHECK
	  ADD CONSTRAINT [GlbTrainingCourse_G2_OC_TrainingContact_FK2_OrgContact_RRR_120N] FOREIGN KEY
		  ( [G2_OC_TrainingContact] )
		  REFERENCES [OrgContact]
		  ( [OC_PK] )
;


				", "DROP TABLE GlbTrainingCourse");
			}
		}

		#endregion

		#region LicenceDatabase

		static DatabaseObjectCreateScript LicenceDatabase
		{
			get
			{
				return new DatabaseObjectCreateScript("LicenceDatabase", @"
CREATE TABLE dbo.LicenceDatabase
(
   [LD_PK] UNIQUEIDENTIFIER NOT NULL,
   [LD_IsActive] BIT NOT NULL DEFAULT 1 ,
   [LD_ServerCode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_LicenceType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_LicenceExpiry] SMALLDATETIME NULL,
   [LD_PurchasedLicenceUnits] INT NOT NULL DEFAULT 0 ,
   [LD_LastHeartbeat] SMALLDATETIME NULL,
   [LD_ReleaseRing] VARCHAR(3) NOT NULL DEFAULT 'GPR' ,
   [LD_AvailableUpgradeMethod] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_DBServerSecurityMode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_PublicEmailAddressForUpdate] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_InternalPop3EmailAddress] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_InternalPop3UserName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_InternalPop3Port] INT NOT NULL DEFAULT 0 ,
   [LD_InternalSmtpEmailAddress] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_InternalSmtpPort] INT NOT NULL DEFAULT 0 ,
   [LD_HL_CurrentRunningVersion] UNIQUEIDENTIFIER NULL,
   [LD_HL_CurrentSentVersion] UNIQUEIDENTIFIER NULL,
   [LD_HostServerSID] UNIQUEIDENTIFIER NULL,
   [LD_HostServerName] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_HostDBName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_HostDBInstance] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_ReportedHostServerName] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_ReportedHostDBName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_ReportedHostDBInstance] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_ScheduleStateUPG] VARCHAR(4096) DEFAULT '',
   [LD_ScheduleStateMUG] VARCHAR(4096) DEFAULT '',
   [LD_NextRunTimeUtcUPG] SMALLDATETIME NULL,
   [LD_NextRunTimeUtcMUG] SMALLDATETIME NULL,
   [LD_RetryTimeoutInMinutes] TINYINT NOT NULL DEFAULT 0 ,
   [LD_MaxDataInMegBeforeAck] DECIMAL(9,1) NOT NULL DEFAULT 0 ,
   [LD_OC_ContractInstallerOrInternalTechContact] UNIQUEIDENTIFIER NULL,
   [LD_OA_SoftwareInstallAddressDetails] UNIQUEIDENTIFIER NULL,
   [LD_OC_LicenseeAdminContact] UNIQUEIDENTIFIER NULL,
   [LD_NoOfActivePrintQueues] SMALLINT NOT NULL DEFAULT 0 ,
   [LD_LogFileOnDifferentPhysicalVolume] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LD_DatabaseFilePathDetail] VARCHAR(MAX) NOT NULL DEFAULT '' ,
   [LD_SQLEdition] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_SQLVersion] VARCHAR(10) NOT NULL DEFAULT '' ,
   [LD_SQLVerString] VARCHAR(MAX) NOT NULL DEFAULT '' ,
   [LD_OSName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_OSVersion] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_SystemManufacturer] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_BIOSDate] SMALLDATETIME NULL,
   [LD_TotalPhysicalMemoryMB] INT NOT NULL DEFAULT 0 ,
   [LD_AvailablePhysicalMemoryMB] INT NOT NULL DEFAULT 0 ,
   [LD_ProcessorType] VARCHAR(256) NOT NULL DEFAULT '' ,
   [LD_ProcessorReleaseDate] SMALLDATETIME NULL,
   [LD_ProcessorSpeedMHz] DECIMAL(9,3) NOT NULL DEFAULT 0 ,
   [LD_NoOfProcessorCores] INT NOT NULL DEFAULT 0 ,
   [LD_VirtualMachineDetected] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LD_HostedLocation] CHAR(3) NOT NULL DEFAULT 'NCW' ,
   [LD_LegacyInterfaceSupport] CHAR(1) NOT NULL DEFAULT 'Y' ,
   [LD_OH_BillingParty] UNIQUEIDENTIFIER NULL,
   [LD_LE] UNIQUEIDENTIFIER NOT NULL,
   [LD_DatabaseNumber] INT NOT NULL DEFAULT 0,
   [LD_Status] varchar(3) NOT NULL default '',
   [LD_HostDBCreateDate] datetime NULL,
   [LD_HostGroupId] UNIQUEIDENTIFIER NULL,
   [LD_Password] VARCHAR(200) NOT NULL DEFAULT '',
   [LD_Product] VARCHAR(3) NOT NULL DEFAULT '',
   [LD_EnablePackageDownloadOptimization] BIT NOT NULL DEFAULT 0 ,
   [LD_HostConnectionServerName] varchar(255) NOT NULL DEFAULT '',
   [LD_CanReregisterToSameServer] bit not null default (0),
   [LD_CurrentVersionFirstReportUtc] smalldatetime null,
   [LD_CurrentVersionLastReportUtc] smalldatetime null,
   [LD_LD_ParentDatabase] uniqueidentifier NULL,
   [LD_IsBilledPerCompany] bit not null default(0),
   [LD_ManualLicenceExpiry] SMALLDATETIME NULL,
   [LD_GS_NKOwner] VARCHAR(3) NOT NULL DEFAULT '',
   [LD_Billable] CHAR(1) NOT NULL DEFAULT 'X',
   [LD_DatabaseConfig] VARCHAR(8000) NOT NULL DEFAULT '',
   [LD_AllowAutoLogin] BIT NOT NULL DEFAULT 1,
   [LD_StaffFirstReportUtc] SMALLDATETIME NULL,
   [LD_OH_WebAccessOrg] uniqueidentifier NULL,
   [LD_TenantID] VARCHAR(50) NOT NULL DEFAULT '',
   [LD_PreRegistrationExpiryDateUTC] SMALLDATETIME NULL,
   [LD_ETS_TrustedSystem] UNIQUEIDENTIFIER NULL,
   [LD_MasterOrgSuggestedUTC] SMALLDATETIME NULL,
   [LD_OutboundEAdaptorUrl] VARCHAR(2048) NULL,
   [LD_TokenAuthenticationEnabled] BIT NOT NULL DEFAULT 0,
   [LD_FCS_FeatureSet] UNIQUEIDENTIFIER NULL,
   [LD_FeatureControlRuleLastSyncUtc] SMALLDATETIME NULL,
   [LD_FeatureControlRuleLastSyncContent] NVARCHAR(MAX) NULL,
   [LD_FeatureSetConfigDateUtc] SMALLDATETIME NULL,
   [LD_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [LD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [LD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [LD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
   CONSTRAINT Constraint_LD_LD_ParentDatabase CHECK (LD_LD_ParentDatabase != LD_PK),
   CONSTRAINT Constraint_LD_LicenceType CHECK (LD_LicenceType != '')
);

ALTER TABLE [LicenceDatabase]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [LicenceDatabase]
ADD CONSTRAINT [PK_UC__LD_PK] PRIMARY KEY CLUSTERED  ([LD_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_HL_CurrentRunningVersion] ON [LicenceDatabase] ([LD_HL_CurrentRunningVersion] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_HL_CurrentSentVersion] ON [LicenceDatabase] ([LD_HL_CurrentSentVersion] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OC_ContractInstallerOrInternalTechContact] ON [LicenceDatabase] ([LD_OC_ContractInstallerOrInternalTechContact] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OA_SoftwareInstallAddressDetails] ON [LicenceDatabase] ([LD_OA_SoftwareInstallAddressDetails] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OC_LicenseeAdminContact] ON [LicenceDatabase] ([LD_OC_LicenseeAdminContact] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_LE] ON [LicenceDatabase] ([LD_LE] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OH_BillingParty] ON [LicenceDatabase] ([LD_OH_BillingParty] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_LD_ParentDatabase] ON [LicenceDatabase] ([LD_LD_ParentDatabase] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [NR_RX__LD_GS_NKOwner] ON [LicenceDatabase] ([LD_GS_NKOwner] ASC)
WHERE LD_GS_NKOwner != ''
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LD_DatabaseNumber] ON [LicenceDatabase] ([LD_DatabaseNumber] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LD_ServerCode_LD_LE] ON [LicenceDatabase] ([LD_ServerCode] ASC, LD_LE)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LD_Product_LD_TenantID] ON [LicenceDatabase] ([LD_Product] ASC, [LD_TenantID] ASC)
WHERE LD_Product <> '' AND LD_TenantID <> ''
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OH_WebAccessOrg] ON [LicenceDatabase] ([LD_OH_WebAccessOrg] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_HL_CurrentRunningVersion_FK2_ReleaseBuild_RRR_120N] FOREIGN KEY
		  ( [LD_HL_CurrentRunningVersion] )
		  REFERENCES [ReleaseBuild]
		  ( [HL_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_HL_CurrentSentVersion_FK2_ReleaseBuild_RRR_120N] FOREIGN KEY
		  ( [LD_HL_CurrentSentVersion] )
		  REFERENCES [ReleaseBuild]
		  ( [HL_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OC_ContractInstallerOrInternalTechContact_FK2_OrgContact_RRR_120N] FOREIGN KEY
		  ( [LD_OC_ContractInstallerOrInternalTechContact] )
		  REFERENCES [OrgContact]
		  ( [OC_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OA_SoftwareInstallAddressDetails_FK2_OrgAddress_RRR_120N] FOREIGN KEY
		  ( [LD_OA_SoftwareInstallAddressDetails] )
		  REFERENCES [OrgAddress]
		  ( [OA_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OC_LicenseeAdminContact_FK2_OrgContact_RRR_120N] FOREIGN KEY
		  ( [LD_OC_LicenseeAdminContact] )
		  REFERENCES [OrgContact]
		  ( [OC_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_LE_FK2_LicenceEnterprise_RRR_120N] FOREIGN KEY
		  ( [LD_LE] )
		  REFERENCES [LicenceEnterprise]
		  ( [LE_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OH_BillingParty_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [LD_OH_BillingParty] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OH_WebAccessOrg_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [LD_OH_WebAccessOrg] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_ETS_TrustedSystem_FK2_EdiTrustedSystem_RRR_120N] FOREIGN KEY
		  ( [LD_ETS_TrustedSystem] )
		  REFERENCES [EdiTrustedSystem]
		  ( [ETS_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_FCS_FeatureSet_FK2_FeatureControlSet_RRR_120N] FOREIGN KEY
		  ( [LD_FCS_FeatureSet] )
		  REFERENCES [FeatureControlSet]
		  ( [FCS_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK 
ADD CONSTRAINT [LicenceDatabase_LD_LD_ParentDatabase_FK2_LicenceDatabase] FOREIGN KEY ([LD_LD_ParentDatabase]) REFERENCES [LicenceDatabase] ([LD_PK])
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK 
ADD CONSTRAINT [Constraint_LD_Billable] CHECK (LD_Billable in ('X', 'Y', 'N', 'P'))
;

				", "DROP TABLE LicenceDatabase");
			}
		}

		static DatabaseObjectCreateScript EdiLicenceDatabaseConsolidationHistory
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiLicenceDatabaseConsolidationHistory", @"
				
CREATE TABLE dbo.EdiLicenceDatabaseConsolidationHistory
(
	[EDH_Period]  [INT] NOT NULL,
	[EDH_LD] [UNIQUEIDENTIFIER] NOT NULL,
	[EDH_LD_ConsolidatedDatabase] [UNIQUEIDENTIFIER] NOT NULL
);

ALTER TABLE [EdiLicenceDatabaseConsolidationHistory] WITH NOCHECK
	  ADD CONSTRAINT [EdiLicenceDatabaseConsolidationHistory_EDH_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		  ( [EDH_LD] )
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] ) ON DELETE CASCADE;

ALTER TABLE [EdiLicenceDatabaseConsolidationHistory] WITH NOCHECK
	  ADD CONSTRAINT [EdiLicenceDatabaseConsolidationHistory_EDH_LD_ConsolidatedDatabase_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		  ( [EDH_LD_ConsolidatedDatabase] )
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] );

ALTER TABLE [EdiLicenceDatabaseConsolidationHistory]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__EDH_Period_EDH_LD] ON [EdiLicenceDatabaseConsolidationHistory]
(
	[EDH_Period] ASC,
	[EDH_LD] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);

", "DROP TABLE EdiLicenceDatabaseConsolidationHistory");
			}
		}

		#endregion

		#region EdiIdentityCertificate

		static DatabaseObjectCreateScript EdiIdentityCertificate
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiIdentityCertificate", @"

CREATE TABLE dbo.EdiIdentityCertificate
(
ICE_PK uniqueidentifier NOT NULL,
ICE_IDA uniqueidentifier NOT NULL,
ICE_SequenceNumber bigint NOT NULL Default 0,
ICE_CARoot varchar(20) NOT NULL DEFAULT '',
ICE_CertificateData varbinary(MAX) NULL,
ICE_CertificateThumbprint varchar(50) NULL,
ICE_CertificateValidDate smalldatetime NULL,
ICE_CertificateExpiryDate smalldatetime NULL,
ICE_CertificateIssuedTo varchar(256) NOT NULL DEFAULT '',
ICE_CertificateIssuedBy varchar(256) NOT NULL DEFAULT '',
ICE_ProcessingStatus varchar(3) NOT NULL DEFAULT 'QUE',
ICE_IsActive bit NOT NULL Default 1,
ICE_IsCertificateRevoked bit NOT NULL Default 0,
ICE_CertificateSigningRequest varchar(max) NOT NULL,
ICE_CertificateSigningRequestHash AS HASHBYTES('SHA2_256', ICE_CertificateSigningRequest),
ICE_SystemCreateTimeUtc smalldatetime NULL,
ICE_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
ICE_SystemLastEditTimeUtc datetime NULL,
ICE_SystemLastEditUser varchar(3) NOT NULL DEFAULT ''
);

ALTER TABLE [EdiIdentityCertificate]
ADD CONSTRAINT [PK_UX__ICE_PK] PRIMARY KEY NONCLUSTERED ([ICE_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;

ALTER TABLE [EdiIdentityCertificate] ADD CONSTRAINT [Constraint_ICE_ProcessingStatus] CHECK (ICE_ProcessingStatus in ('QUE', 'PRC', 'COM', 'CAN', 'FAL'))

ALTER TABLE [EdiIdentityCertificate] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityCertificate_ICE_IDA_FK2_EdiIdentityApplication_PK] FOREIGN KEY
		  ( [ICE_IDA] )
		  REFERENCES [EdiIdentityApplication]
		  ( [IDA_PK] )
;

CREATE CLUSTERED INDEX [FK_RC__ICE_IDA] ON [EdiIdentityCertificate] ([ICE_IDA] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__ICE_CertificateSigningRequestHash] 
ON [EdiIdentityCertificate] ([ICE_CertificateSigningRequestHash])
WHERE [ICE_IsActive] = 1 AND [ICE_CertificateData] IS Null WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__ICE_SequenceNumber] ON [EdiIdentityCertificate] ([ICE_SequenceNumber] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
CREATE NONCLUSTERED INDEX [NR_RX__ICE_ProcessingStatus] ON [EdiIdentityCertificate] ([ICE_ProcessingStatus] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
",
"DROP TABLE EdiIdentityCertificate");
			}
		}

		#endregion

		#region EdiIdentityApplication

		static DatabaseObjectCreateScript EdiIdentityApplication
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiIdentityApplication", @"

Create Table dbo.EdiIdentityApplication
(
IDA_PK uniqueidentifier NOT NULL,
IDA_LD uniqueidentifier NULL,
IDA_IDT uniqueidentifier NULL,
IDA_OH_ParentOrg uniqueidentifier NULL,
IDA_IDA_ParentApplication uniqueidentifier NULL,
IDA_ClientID varchar(36) NOT NULL DEFAULT '',
IDA_IsRollback bit NOT NULL Default 0,
IDA_ApplicationName varchar(256) NOT NULL DEFAULT '',
IDA_ApplicationModule nvarchar(15) NOT NULL DEFAULT '',
IDA_ProcessingStatus varchar(3) NOT NULL DEFAULT '',
IDA_RedirectUrlStatus varchar(3) NOT NULL DEFAULT 'NON',
IDA_RedirectUrlLastSyncTimeUtc datetime NULL,
IDA_Product varchar(3) NOT NULL Default '',
IDA_ApplicationType varchar(3) NOT NULL Default '',
IDA_IsActive bit NOT NULL Default 1,
IDA_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemCreateTimeUtc smalldatetime NULL,
IDA_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemLastEditTimeUtc datetime NULL
);

ALTER TABLE [EdiIdentityApplication]
ADD CONSTRAINT [PK_UX__IDA_PK] PRIMARY KEY CLUSTERED ([IDA_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;

ALTER TABLE [EdiIdentityApplication] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityApplication_IDA_LD_FK2_LicenceDatabase_PK] FOREIGN KEY
		  ( [IDA_LD] )
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] );

ALTER TABLE [EdiIdentityApplication] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityApplication_IDA_IDT_FK2_EdiIdentityTenant_PK] FOREIGN KEY
		  ( [IDA_IDT] )
		  REFERENCES [EdiIdentityTenant]
		  ( [IDT_PK] );

ALTER TABLE [EdiIdentityApplication] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityApplication_IDA_OH_ParentOrg_FK2_OrgHeader_PK] FOREIGN KEY
		  ( [IDA_OH_ParentOrg] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] );

ALTER TABLE [EdiIdentityApplication] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityApplication_IDA_IDA_ParentApplication_FK2_IDA_PK] FOREIGN KEY
		  ( [IDA_IDA_ParentApplication] )
		  REFERENCES [EdiIdentityApplication]
		  ( [IDA_PK] );

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDA_LD_IDA_IDT] ON [EdiIdentityApplication] ([IDA_LD] ASC, [IDA_IDT] ASC) WHERE IDA_LD IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDA_ClientID_IDA_IDT] ON [EdiIdentityApplication] ([IDA_ClientID] ASC, [IDA_IDT] ASC) WHERE IDA_ClientID != '' WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDA_ApplicationName] ON [EdiIdentityApplication] ([IDA_ApplicationName]) WHERE IDA_ApplicationName != '' WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [NR_RX__IDA_RedirectUrlStatus] ON [EdiIdentityApplication] ([IDA_RedirectUrlStatus]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [NR_RX__IDA_IsRollback_IDA_IsActive] ON [EdiIdentityApplication] ([IDA_IsRollback] ASC, [IDA_IsActive] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [EdiIdentityApplication] ADD CONSTRAINT [Constraint_IDA_RedirectUrlStatus] CHECK (IDA_RedirectUrlStatus in ('NON', 'SCH', 'NUD', 'ERR'));

ALTER TABLE [EdiIdentityApplication] ADD CONSTRAINT [Constraint_IDA_ProcessingStatus] CHECK (IDA_ProcessingStatus in ('', 'FAL'));
",
					"DROP TABLE EdiIdentityApplication");
			}
		}

		#endregion

		#region EdiIdentityApplicationPermission

		static DatabaseObjectCreateScript EdiIdentityApplicationPermission
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiIdentityApplicationPermission", @"

Create Table dbo.EdiIdentityApplicationPermission
(
IAP_PK uniqueidentifier NOT NULL,
IAP_IDA uniqueidentifier NOT NULL,
IAP_Scope varchar(64) NOT NULL DEFAULT '',
IAP_IsActive bit NOT NULL Default 1,
IAP_SystemCreateUser varchar(3) NOT NULL,
IAP_SystemCreateTimeUtc smalldatetime NULL,
IAP_SystemLastEditUser varchar(3) NOT NULL,
IAP_SystemLastEditTimeUtc datetime NULL
);

ALTER TABLE [EdiIdentityApplicationPermission]
	ADD CONSTRAINT [PK_UX__IAP_PK] PRIMARY KEY CLUSTERED ([IAP_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;

ALTER TABLE [EdiIdentityApplicationPermission] WITH NOCHECK
	ADD CONSTRAINT [EdiIdentityApplicationPermission_IAP_IDA_FK2_IDA_PK] FOREIGN KEY
		( [IAP_IDA] )
		REFERENCES [EdiIdentityApplication]
		( [IDA_PK] );
",
					"DROP TABLE EdiIdentityApplicationPermission");
			}
		}

		#endregion

		#region EdiIdentityRedirectUrl

		static DatabaseObjectCreateScript EdiIdentityRedirectUrl
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiIdentityRedirectUrl", @"
Create Table dbo.EdiIdentityRedirectUrl
(
IAR_PK uniqueidentifier NOT NULL,
IAR_IDA uniqueidentifier NOT NULL,
IAR_ApplicationName varchar(256) NOT NULL,
IAR_RedirectType varchar(3) NOT NULL DEFAULT 'SPA',
IAR_RedirectUrl varchar(200) NOT NULL,
IAR_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IAR_SystemCreateTimeUtc smalldatetime NULL,
IAR_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IAR_SystemLastEditTimeUtc datetime NULL
);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IAR_IDA_IAR_RedirectUrl] ON [EdiIdentityRedirectUrl] ([IAR_IDA] ASC, [IAR_RedirectUrl] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);

ALTER TABLE [EdiIdentityRedirectUrl]
ADD CONSTRAINT [PK_UX__IAR_PK] PRIMARY KEY CLUSTERED ([IAR_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;

ALTER TABLE [EdiIdentityRedirectUrl] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityRedirectUrl_IAR_IDA_FK2_EdiIdentityApplication_PK] FOREIGN KEY
		  ( [IAR_IDA] )
		  REFERENCES [EdiIdentityApplication]
		  ( [IDA_PK] );

ALTER TABLE [EdiIdentityRedirectUrl] ADD CONSTRAINT [Constraint_IAR_RedirectType] CHECK (IAR_RedirectType  in ('WEB', 'SPA', 'ICL'));
",
					"DROP TABLE EdiIdentityRedirectUrl");
			}
		}

		#endregion

		#region EdiIdentityTenant

		static DatabaseObjectCreateScript EdiIdentityTenant
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiIdentityTenant", @"
CREATE TABLE dbo.EdiIdentityTenant
(
IDT_PK uniqueidentifier NOT NULL,
IDT_TenantId varchar(36) NOT NULL,
IDT_OidcClientId varchar(36) NOT NULL,
IDT_Name varchar(64) NOT NULL,
IDT_AuthorityUrl varchar(128) NOT NULL DEFAULT '',
IDT_GraphClientId varchar(36) NOT NULL,
IDT_Onboarding bit NOT NULL DEFAULT 0,
IDT_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDT_SystemCreateTimeUtc smalldatetime NULL,
IDT_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDT_SystemLastEditTimeUtc datetime NULL
);

ALTER TABLE [EdiIdentityTenant]
ADD CONSTRAINT [PK_UX__IDT_PK] PRIMARY KEY CLUSTERED ([IDT_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [EdiIdentityTenant]
ADD CONSTRAINT [NR_UX__IDT_TenantId] UNIQUE NONCLUSTERED ([IDT_TenantId]) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDT_Onboarding] ON [EdiIdentityTenant] ([IDT_Onboarding]) WHERE [IDT_Onboarding] = 1 WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDT_Name] ON [EdiIdentityTenant] ([IDT_Name]) WHERE IDT_Name != '' WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDT_AuthorityUrl] ON [EdiIdentityTenant] ([IDT_AuthorityUrl]) WITH (ALLOW_PAGE_LOCKS = OFF);

",
					"DROP TABLE EdiIdentityTenant");
			}
		}

		#endregion

		#region HelpErrorStackLineCount

		static DatabaseObjectCreateScript HelpErrorStackLineCount
			=> new DatabaseObjectCreateScript(nameof(HelpErrorStackLineCount),
				@"
CREATE TABLE dbo.HelpErrorStackLineCount
(
	[HSL_PK] UNIQUEIDENTIFIER NOT NULL,
	[HSL_Assembly]	VARCHAR(260) NULL,
	[HSL_Type]	VARCHAR(1024) NULL,
	[HSL_Method] VARCHAR(1024) NULL,
	[HSL_Parameters] VARCHAR(1024) NULL,
	[HSL_StackLine] VARCHAR(900) NOT NULL,
	[HSL_Count] INT NOT NULL,
	CONSTRAINT [PK_UX__HSL_PK] PRIMARY KEY NONCLUSTERED ([HSL_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [HelpErrorStackLineCount]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX NR_UC__HSL_StackLine ON HelpErrorStackLineCount ([HSL_StackLine]) WITH (ALLOW_PAGE_LOCKS = OFF);
",
				@"DROP TABLE HelpErrorStackLineCount"
			);

		#endregion

		#region LicenceHeader

		static DatabaseObjectCreateScript LicenceHeader
		{
			get
			{
				return new DatabaseObjectCreateScript("LicenceHeader", @"
CREATE TABLE dbo.LicenceHeader
(
   [LA_PK] UNIQUEIDENTIFIER NOT NULL,
   [LA_ProductType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LA_IsActive] BIT NOT NULL DEFAULT 1 ,
   [LA_LicenceAdvStdOth] VARCHAR(3) NOT NULL DEFAULT 'ADV' ,
   [LA_SupportMode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LA_SpecialSupportConditions] VARCHAR(250) NOT NULL DEFAULT '' ,
   [LA_EstimatedLiveDate] SMALLDATETIME NULL,
   [LA_SiteLiveDate] SMALLDATETIME NULL,
   [LA_SupportStartDate] SMALLDATETIME NULL,
   [LA_ContractExpiryDate] SMALLDATETIME NULL,
   [LA_ContractRenewalIssued] SMALLDATETIME NULL,
   [LA_AMS_USMode] VARCHAR(3) NOT NULL DEFAULT 'OFF' ,
   [LA_InstallationStartDate] SMALLDATETIME NULL,
   [LA_InstallationCompleteDate] SMALLDATETIME NULL,
   [LA_LastFaxReport] SMALLDATETIME NULL,
   [LA_LastLicenceSyncCheck] SMALLDATETIME NULL,
   [LA_LastLicenceCheckInSync] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LA_LC] UNIQUEIDENTIFIER NOT NULL,
   [LA_LD] UNIQUEIDENTIFIER NOT NULL,
	[LA_RX_NKPriceCurrency] [varchar](3) NOT NULL DEFAULT (''),
	[LA_AgreedLiveDate] SMALLDATETIME NULL,
);

ALTER TABLE [LicenceHeader]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [LicenceHeader]
ADD CONSTRAINT [PK_UC__LA_PK] PRIMARY KEY CLUSTERED  ([LA_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__LA_LC] ON [LicenceHeader] ([LA_LC] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LA_LD_LA_LC] ON [LicenceHeader] ([LA_LD] ASC, [LA_LC] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [LicenceHeader] WITH NOCHECK
	  ADD CONSTRAINT [LicenceHeader_LA_LC_FK2_LicenceCompany_RRR_120N] FOREIGN KEY
		  ( [LA_LC] )
		  REFERENCES [LicenceCompany]
		  ( [LC_PK] )
;

ALTER TABLE [LicenceHeader] WITH NOCHECK
	  ADD CONSTRAINT [LicenceHeader_LA_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		  ( [LA_LD] )
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] )
;

", "DROP TABLE LicenceHeader");
			}
		}

		#endregion

		#region UpgradesToClient

		static DatabaseObjectCreateScript UpgradesToClient
		{
			get
			{
				return new DatabaseObjectCreateScript("UpgradesToClient", @"
CREATE TABLE dbo.UpgradesToClient
(
   [L1_PK] UNIQUEIDENTIFIER NOT NULL,
   [L1_RequestedDateTime] SMALLDATETIME NULL,
   [L1_ActualDateTime] SMALLDATETIME NULL,
   [L1_LastEmailSent] SMALLDATETIME NULL,
   [L1_RequestedUpgradeMethod] VARCHAR(3) NOT NULL DEFAULT '' ,
   [L1_ActualUpgradeMethod] VARCHAR(3) NOT NULL DEFAULT '' ,
   [L1_CurrentStatus] VARCHAR(3) NOT NULL DEFAULT '' ,
   [L1_UpdateNotificationMessageText] VARCHAR(MAX) NOT NULL DEFAULT '' ,
   [L1_GS_NKStaffCode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [L1_LD] UNIQUEIDENTIFIER NULL,
   [L1_HL] UNIQUEIDENTIFIER NULL,
   [L1_OC] UNIQUEIDENTIFIER NULL,
   [L1_OH] UNIQUEIDENTIFIER NULL,
   [L1_NotifyUser] BIT NOT NULL DEFAULT 1
);

ALTER TABLE [UpgradesToClient]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [UpgradesToClient]
ADD CONSTRAINT [PK_UX__L1_PK] PRIMARY KEY NONCLUSTERED  ([L1_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE CLUSTERED INDEX [FK_RC__L1_LD] ON [UpgradesToClient] ([L1_LD] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__L1_HL] ON [UpgradesToClient] ([L1_HL] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__L1_OC] ON [UpgradesToClient] ([L1_OC] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__L1_OH] ON [UpgradesToClient] ([L1_OH] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [UpgradesToClient] WITH NOCHECK
	  ADD CONSTRAINT [UpgradesToClient_L1_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		  ( [L1_LD] )
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] )
;

ALTER TABLE [UpgradesToClient] WITH NOCHECK
	  ADD CONSTRAINT [UpgradesToClient_L1_HL_FK2_ReleaseBuild_RRR_120N] FOREIGN KEY
		  ( [L1_HL] )
		  REFERENCES [ReleaseBuild]
		  ( [HL_PK] )
;

ALTER TABLE [UpgradesToClient] WITH NOCHECK
	  ADD CONSTRAINT [UpgradesToClient_L1_OC_FK2_OrgContact_RRR_120N] FOREIGN KEY
		  ( [L1_OC] )
		  REFERENCES [OrgContact]
		  ( [OC_PK] )
;

ALTER TABLE [UpgradesToClient] WITH NOCHECK
	  ADD CONSTRAINT [UpgradesToClient_L1_OH_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [L1_OH] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

", "DROP TABLE UpgradesToClient");
			}
		}

		#endregion

		#region ClientMailDBRecipients

		static DatabaseObjectCreateScript ClientMailDBRecipients
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientMailDBRecipients", @"
CREATE TABLE dbo.ClientMailDBRecipients(
	MRX_PK uniqueidentifier NOT NULL,
	MRX_MR uniqueidentifier NOT NULL,
	MRX_L1 uniqueidentifier NOT NULL,
 CONSTRAINT [PK_UX__MRX_PK] PRIMARY KEY NONCLUSTERED 
(
	MRX_PK ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientMailDBRecipients]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE dbo.ClientMailDBRecipients WITH CHECK ADD CONSTRAINT ClientMailDBRecipients_MRX_MR_FK2_MailDBRecipients_CRR_120N FOREIGN KEY(MRX_MR) REFERENCES MailDBRecipients (MR_PK) ON DELETE CASCADE
ALTER TABLE dbo.ClientMailDBRecipients WITH CHECK ADD CONSTRAINT ClientMailDBRecipients_MRX_L1_FK2_UpgradesToClient_RRR_120N FOREIGN KEY(MRX_L1) REFERENCES UpgradesToClient (L1_PK)

CREATE CLUSTERED INDEX [NR_RC__MRX_MR] ON [ClientMailDBRecipients] ([MRX_MR] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

", "DROP TABLE ClientMailDBRecipients");
			}
		}

		#endregion

		#region ClientFeatureRequestValueAndContribution

		static DatabaseObjectCreateScript ClientFeatureRequestValueAndContribution
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientFeatureRequestValueAndContribution", @"
CREATE TABLE dbo.ClientFeatureRequestValueAndContribution
(
	T9_PK uniqueidentifier NOT NULL CONSTRAINT DF_T9_PK DEFAULT (newid()),
	T9_Contribution money NOT NULL DEFAULT (0),
	T9_EBV money NOT NULL DEFAULT (0),
	T9_CostReduction	 money NOT NULL DEFAULT (0),
	T9_RiskReduction	 money NOT NULL DEFAULT (0),
	T9_ReductionInErrorRates	 money NOT NULL DEFAULT (0),
	T9_ProcessSimplification	 money NOT NULL DEFAULT (0),
	T9_Satisfaction  money NOT NULL DEFAULT (0),
	T9_SalesImprovement  money NOT NULL DEFAULT (0),	
	T9_PreventionOfLossOfCustomerOrBusiness  money NOT NULL DEFAULT (0),
	T9_CompetitiveAdvantage  money NOT NULL DEFAULT (0),
	T9_AnyEffectThatLowersCostsOrGrowsRevenue  money NOT NULL DEFAULT (0),
	T9_ParentID uniqueidentifier NOT NULL,
	T9_ParentTableCode char(2) NOT NULL DEFAULT('')
						
	CONSTRAINT PK_UX__T9_PK PRIMARY KEY NONCLUSTERED 
	( T9_PK )
	WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientFeatureRequestValueAndContribution]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX NR_UC__T9_ParentID ON ClientFeatureRequestValueAndContribution (T9_ParentID) WITH (ALLOW_PAGE_LOCKS = OFF)
					
", "DROP TABLE ClientFeatureRequestValueAndContribution");
			}
		}

		#endregion

		#region ClientProductConsultantSurveyRecipient

		static DatabaseObjectCreateScript ClientProductConsultantSurveyRecipient
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientProductConsultantSurveyRecipient",
@"CREATE TABLE dbo.ClientProductConsultantSurveyRecipient
(
	T8_PK uniqueidentifier NOT NULL CONSTRAINT DF_T8_PK DEFAULT (newid()),
	T8_G2 uniqueidentifier NOT NULL,
	T8_OC uniqueidentifier NOT NULL

	CONSTRAINT PK_UX__T8_PK PRIMARY KEY  NONCLUSTERED ( T8_PK ) WITH (ALLOW_PAGE_LOCKS = OFF),

	CONSTRAINT ClientProductConsultantSurveyRecipient_T8_G2_FK2_GlbTrainingCourse_RRR_120N FOREIGN KEY 
		( T8_G2 ) REFERENCES GlbTrainingCourse ( G2_PK ),

	CONSTRAINT ClientProductConsultantSurveyRecipient_T8_OC_FK2_OrgContact_RRR_120N FOREIGN KEY 
		( T8_OC ) REFERENCES OrgContact ( OC_PK )
);

ALTER TABLE [ClientProductConsultantSurveyRecipient]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX FK_RC__T8_G2 ON ClientProductConsultantSurveyRecipient (T8_G2) WITH (ALLOW_PAGE_LOCKS = OFF)
CREATE INDEX FK_RX__T8_OC ON ClientProductConsultantSurveyRecipient (T8_OC) WITH (ALLOW_PAGE_LOCKS = OFF)
",
"DROP TABLE ClientProductConsultantSurveyRecipient");
			}
		}

		#endregion

		#region ClientProductConsultantSurveyRecipientSession

		static DatabaseObjectCreateScript ClientProductConsultantSurveyRecipientSession
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientProductConsultantSurveyRecipientSession",
@"CREATE TABLE dbo.ClientProductConsultantSurveyRecipientSession
(
	T7_PK uniqueidentifier NOT NULL CONSTRAINT DF_T7_PK DEFAULT (newid()),
	T7_T8 uniqueidentifier NOT NULL,
	T7_GZ uniqueidentifier NOT NULL

	CONSTRAINT PK_UX__T7_PK PRIMARY KEY  NONCLUSTERED ( T7_PK ) WITH (ALLOW_PAGE_LOCKS = OFF),

	CONSTRAINT ClientProductConsultantSurveyRecipientSession_T7_T8_FK2_ClientProductConsultantSurveyRecipient_RRR_120N FOREIGN KEY 
		( T7_T8 ) REFERENCES ClientProductConsultantSurveyRecipient ( T8_PK ),

	CONSTRAINT ClientProductConsultantSurveyRecipientSession_T7_GZ_FK2_GlbClassroomSession_RRR_120N FOREIGN KEY 
		( T7_GZ ) REFERENCES GlbClassroomSession ( GZ_PK )
);

ALTER TABLE [ClientProductConsultantSurveyRecipientSession]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX FK_RC__T7_T8 ON ClientProductConsultantSurveyRecipientSession (T7_T8) WITH (ALLOW_PAGE_LOCKS = OFF)
CREATE INDEX FK_RX__T7_GZ ON ClientProductConsultantSurveyRecipientSession (T7_GZ) WITH (ALLOW_PAGE_LOCKS = OFF)
",
"DROP TABLE ClientProductConsultantSurveyRecipientSession");
			}
		}

		#endregion

		#region Client_eRouterEdiEnterpriseCommunication

		static DatabaseObjectCreateScript Client_eRouterEdiEnterpriseCommunication
		{
			get
			{
				return new DatabaseObjectCreateScript("eRouterEdiEnterpriseCommunication", @"
CREATE TABLE dbo.eRouterEdiEnterpriseCommunication(
	EC_PK uniqueidentifier NOT NULL,
	EC_EnterpriseCode char(3) NOT NULL DEFAULT(''),
	EC_CompanyCode char(3) NOT NULL DEFAULT(''),
	EC_ApplicationCode char(3) NOT NULL DEFAULT(''),
	EC_Type char(3) NOT NULL DEFAULT(''),
	EC_Direction char(3) NOT NULL DEFAULT(''),
	EC_Sender varchar(64) NOT NULL DEFAULT(''),
	EC_Recipient varchar(64) NOT NULL DEFAULT(''),
	EC_Identifier varchar(64) NOT NULL DEFAULT(''),
	EC_Data varchar(max) NOT NULL DEFAULT(''),
	EC_TransmitDate datetime NOT NULL DEFAULT (getdate()),
	CONSTRAINT PK_eRouterEdiEnterpriseCommunication PRIMARY KEY NONCLUSTERED (EC_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [eRouterEdiEnterpriseCommunication]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [NR_RC__EC_EnterpriseCode] ON [eRouterEdiEnterpriseCommunication] ([EC_EnterpriseCode] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
", "DROP TABLE eRouterEdiEnterpriseCommunication");
			}
		}

		#endregion

		#region ClientInvoiceDelivery

		static DatabaseObjectCreateScript ClientInvoiceDelivery
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientInvoiceDelivery", @"
CREATE TABLE dbo.ClientInvoiceDelivery(
	[L9_PK] [uniqueidentifier] NOT NULL,
	[L9_LC] [uniqueidentifier] NOT NULL,
	[L9_ServerCode] [varchar](3) NOT NULL DEFAULT (''),
	[L9_SystemCode] [varchar](3) NOT NULL DEFAULT (''),
	[L9_OH_InvoiceTo] [uniqueidentifier] NULL,
	[L9_IsBilled] [char](1) NOT NULL DEFAULT ('Y'),
	[L9_Note] [varchar](200) NOT NULL DEFAULT (''),
	[L9_GroupBy] char(3) NOT NULL DEFAULT ('ALL'),
	[L9_GB_InvoicingBranch] [uniqueidentifier] NULL,
	[L9_AT_TaxId] [uniqueidentifier] NULL,
	[L9_RX_NKInvoiceCurrency] [varchar](3) NOT NULL DEFAULT (''),
	[L9_AC_SalesTaxChargeCode] [uniqueidentifier] NULL,
	[L9_UseParentPrices] bit not null default(0),
CONSTRAINT [PK_ClientInvoiceDelivery] PRIMARY KEY NONCLUSTERED
(
	[L9_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientInvoiceDelivery]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [FK_RC__L9_LC] ON [ClientInvoiceDelivery] ([L9_LC] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [ClientInvoiceDelivery]  WITH CHECK ADD  CONSTRAINT [ClientInvoiceDelivery_L9_LC_FK2_LicenceCompany] FOREIGN KEY([L9_LC]) REFERENCES [LicenceCompany] ([LC_PK])
ALTER TABLE [ClientInvoiceDelivery]  WITH CHECK ADD  CONSTRAINT [ClientInvoiceDelivery_L9_OH_InvoiceTo_FK2_OrgHeader] FOREIGN KEY([L9_OH_InvoiceTo]) REFERENCES [OrgHeader] ([OH_PK])
ALTER TABLE [ClientInvoiceDelivery]  WITH CHECK ADD  CONSTRAINT [ClientInvoiceDelivery_L9_GB_InvoicingBranch_FK2_GlbBranch] FOREIGN KEY([L9_GB_InvoicingBranch]) REFERENCES [GlbBranch] ([GB_PK])
ALTER TABLE [ClientInvoiceDelivery]  WITH CHECK ADD  CONSTRAINT [ClientInvoiceDelivery_L9_AT_TaxId_FK2_AccTaxRate] FOREIGN KEY([L9_AT_TaxId]) REFERENCES [AccTaxRate] ([AT_PK])
ALTER TABLE [ClientInvoiceDelivery]  WITH CHECK ADD  CONSTRAINT [ClientInvoiceDelivery_L9_AC_SalesTaxChargeCode_FK2_AccChargeCode] FOREIGN KEY([L9_AC_SalesTaxChargeCode]) REFERENCES [AccChargeCode] ([AC_PK])
				", "DROP TABLE ClientInvoiceDelivery");
			}
		}

		static DatabaseObjectCreateScript ClientInvoiceDelivery_L9_OH_InvoiceTo
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RX__L9_OH_InvoiceTo",
					"CREATE NONCLUSTERED INDEX NR_RX__L9_OH_InvoiceTo ON ClientInvoiceDelivery (L9_OH_InvoiceTo) INCLUDE ([L9_LC]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientInvoiceDelivery.NR_RX__L9_OH_InvoiceTo");
			}
		}

		#endregion

		#region ClientLicenceBilling

		static DatabaseObjectCreateScript ClientLicenceBilling
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientLicenceBilling", @"
CREATE TABLE dbo.ClientLicenceBilling(
	[L4_PK] [uniqueidentifier] NOT NULL,
	[L4_LC] [uniqueidentifier] NOT NULL,
	[L4_RX_NKFeeCurrency] [varchar](3) NOT NULL,
	[L4_InvoiceComment] [nvarchar](1024) NOT NULL,
	[L4_ProcessingFee] [varchar](3) NOT NULL,
	[L4_ProcessingFeePercent] [decimal] (9, 3) NOT NULL,
	[L4_IsPartner] [char](1) NOT NULL,
	[L4_Note] [varchar](200) NOT NULL,
	[L4_Comment] [nvarchar](1024) NOT NULL DEFAULT(''),
	[L4_PredeterminedPrepaidBalance] [MONEY] NOT NULL DEFAULT(0),
	[L4_RX_NKPredeterminedPrepaidBalanceCurrency] [VARCHAR](3) NOT NULL DEFAULT(''),
	[L4_FuturePredeterminedPrepaidBalance] [MONEY] NOT NULL DEFAULT(0),
	[L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency] [VARCHAR](3) NOT NULL DEFAULT(''),
CONSTRAINT [PK_ClientLicenceBilling] PRIMARY KEY NONCLUSTERED
(
	[L4_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientLicenceBilling]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [FK_UC__L4_LC] ON [ClientLicenceBilling] ([L4_LC] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [ClientLicenceBilling]  WITH CHECK ADD  CONSTRAINT [ClientLicenceBilling_L4_LC_FK2_LicenceCompany] FOREIGN KEY([L4_LC]) REFERENCES [LicenceCompany] ([LC_PK])

ALTER TABLE [ClientLicenceBilling] ADD  CONSTRAINT [DF_ClientLicenceBilling_L4_RX_NKFeeCurrency]  DEFAULT ('') FOR [L4_RX_NKFeeCurrency]
ALTER TABLE [ClientLicenceBilling] ADD  CONSTRAINT [DF_ClientLicenceBilling_L4_InvoiceComment]  DEFAULT ('') FOR [L4_InvoiceComment]
ALTER TABLE [ClientLicenceBilling] ADD  CONSTRAINT [DF_ClientLicenceBilling_L4_ProcessingFee]  DEFAULT ('NON') FOR [L4_ProcessingFee]
ALTER TABLE [ClientLicenceBilling] ADD  CONSTRAINT [DF_ClientLicenceBilling_L4_ProcessingFeePercent]  DEFAULT ((0)) FOR [L4_ProcessingFeePercent]
ALTER TABLE [ClientLicenceBilling] ADD  CONSTRAINT [DF_ClientLicenceBilling_L4_IsPartner]  DEFAULT ('N') FOR [L4_IsPartner]
ALTER TABLE [ClientLicenceBilling] ADD  CONSTRAINT [DF_ClientLicenceBilling_L4_Note]  DEFAULT ('') FOR [L4_Note]

				", "DROP TABLE ClientLicenceBilling");
			}
		}

		#endregion

		#region ClientLicenceBillingDiscount

		static DatabaseObjectCreateScript ClientLicenceBillingDiscount
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientLicenceBillingDiscount", @"
CREATE TABLE dbo.ClientLicenceBillingDiscount(
	[L5_PK] [uniqueidentifier] NOT NULL,
	[L5_L4] [uniqueidentifier] NOT NULL,
	[L5_SystemCode] [varchar](3) NOT NULL DEFAULT(''),
	[L5_SubCode] [varchar](50) NOT NULL DEFAULT (''),
	[L5_Type] [varchar](3) NOT NULL DEFAULT (''),
	[L5_BreakAmount] [money] NOT NULL DEFAULT (0),
	[L5_Units] [int] NOT NULL DEFAULT (0),
	[L5_Discount] [decimal](5, 2) NOT NULL DEFAULT (0),
	[L5_Description] [varchar](80) NOT NULL DEFAULT (''),
	[L5_StartDate] [smalldatetime] NULL,
	[L5_EndDate] [smalldatetime] NULL,
	[L5_ModuleCode] [varchar](3) NOT NULL DEFAULT (''),
	[L5_Duration] [smallint] NOT NULL DEFAULT (0),
	[L5_BreakUnits] [varchar](3) NOT NULL DEFAULT('CUR'),
	[L5_DiscountCode] [varchar](20) NOT NULL DEFAULT(''),
	[L5_Comment] [nvarchar](1024) NOT NULL DEFAULT(''),
 CONSTRAINT [PK_ClientLicenceBillingDiscount] PRIMARY KEY NONCLUSTERED
(
	[L5_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

CREATE CLUSTERED INDEX [NR_RC__L5_L4] ON [ClientLicenceBillingDiscount] ([L5_L4] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [ClientLicenceBillingDiscount]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientLicenceBillingDiscount]  WITH CHECK ADD  CONSTRAINT [ClientLicenceBillingDiscount_L5_L4_FK2_ClientLicenceBilling] FOREIGN KEY([L5_L4]) REFERENCES [ClientLicenceBilling] ([L4_PK])
				", "DROP TABLE ClientLicenceBillingDiscount");
			}
		}

		#endregion

		#region ClientLicencePriceHeader

		static DatabaseObjectCreateScript ClientLicencePriceHeader
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientLicencePriceHeader", @"
CREATE TABLE dbo.ClientLicencePriceHeader(
	[L6_PK] [uniqueidentifier] NOT NULL,
	[L6_LC] [uniqueidentifier] NULL CONSTRAINT [ClientLicencePriceHeader_L6_LC_FK2_LicenceCompany] REFERENCES [LicenceCompany] ([LC_PK]),
	[L6_RX_NKCurrency] [varchar](3) NOT NULL CONSTRAINT [DF_ClientLicencePriceHeader_L6_RX_NKCurrency]  DEFAULT (''),
	[L6_RN_NKCountry] [varchar](2) NOT NULL CONSTRAINT [DF_ClientLicencePriceHeader_L6_RN_NKCountry]  DEFAULT (''),
	[L6_ValidFrom] [smalldatetime] NULL,
	[L6_ValidTo] [smalldatetime] NULL,
	[L6_SystemCreateTimeUtc] [smalldatetime] NULL,
	[L6_SystemCreateUser] [varchar](3) NOT NULL CONSTRAINT [DF_L6_SystemCreateUser] DEFAULT (''),
	[L6_LicenceEdition] [varchar](3) NOT NULL CONSTRAINT [DF_ClientLicencePriceHeader_L6_LicenceEdition] DEFAULT (''),
	[L6_SystemCode] [varchar](3) NOT NULL CONSTRAINT [DF_ClientLicencePriceHeader_L6_SystemCode]  DEFAULT (''),
	[L6_PricelistVersion] [varchar](50) NOT NULL CONSTRAINT [DF_ClientLicencePriceHeader_L6_PricelistVersion] DEFAULT (''),
	[L6_IsStandard] [char](1) NOT NULL DEFAULT('N'),
	[L6_LicenceUnitRate] [decimal](18, 9) NOT NULL DEFAULT (0),
	[L6_DiscountCode] [varchar](20) NOT NULL DEFAULT(''),
	[L6_UseStandardDiscount] bit NOT NULL DEFAULT(1),
	L6_LiveMonthsUntilTestDbBilling int NOT NULL DEFAULT (0),
	L6_TestDbPriceCode varchar(3) NOT NULL DEFAULT (''),
	L6_HasExchangeRates bit NOT NULL DEFAULT(0),
	L6_Rounding varchar(3) NOT NULL DEFAULT('V1'),
	L6_IsDisbursementBundle BIT NOT NULL DEFAULT(0),
CONSTRAINT [PK_ClientLicencePriceHeader] PRIMARY KEY NONCLUSTERED
(
	[L6_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientLicencePriceHeader]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [NR_RC__L6_LC] ON [dbo].[ClientLicencePriceHeader] ([L6_LC] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;

CREATE NONCLUSTERED INDEX NR_RX__ClientLicencePriceHeader_Std
ON dbo.ClientLicencePriceHeader (L6_SystemCode,L6_RX_NKCurrency,L6_LicenceEdition,L6_RN_NKCountry,L6_PricelistVersion) include (L6_PK)
where L6_LC = '31754C3F-4782-4504-AC75-C92B0EEB1B73'
WITH (ALLOW_PAGE_LOCKS = OFF)

				", "DROP TABLE ClientLicencePriceHeader");
			}
		}

		#endregion

		#region EdiPriceHeaderDiscount

		static DatabaseObjectCreateScript EdiPriceHeaderDiscount
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiPriceHeaderDiscount", @"
CREATE TABLE dbo.EdiPriceHeaderDiscount(
	[PHD_PK] [uniqueidentifier] NOT NULL,
	[PHD_Version] [varchar](20) NOT NULL,
	[PHD_Name] [varchar](30) NOT NULL,
	[PHD_Type] [varchar](3) NOT NULL,
	[PHD_Percent] [decimal](5, 2) NOT NULL DEFAULT(0),
	[PHD_IsDefaultEnabled] bit NOT NULL DEFAULT (1),
	[PHD_ConfigXml] varchar(max) NOT NULL DEFAULT(''),
 CONSTRAINT [PK_EdiPriceHeaderDiscount] PRIMARY KEY NONCLUSTERED ([PHD_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiPriceHeaderDiscount]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX NR_UC__PHD_Version_PHD_Name ON [dbo].[EdiPriceHeaderDiscount] (PHD_Version, PHD_Name) WITH (ALLOW_PAGE_LOCKS = OFF)
;

				", "DROP TABLE EdiPriceHeaderDiscount");
			}
		}

		#endregion

		#region EdiPriceDiscountGroupMember

		static DatabaseObjectCreateScript EdiPriceDiscountGroupMember
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiPriceDiscountGroupMember", @"
CREATE TABLE dbo.EdiPriceDiscountGroupMember(
	[PGM_PK] [uniqueidentifier] NOT NULL,
	[PGM_PHD] [uniqueidentifier] NOT NULL CONSTRAINT [EdiPriceDiscountGroupMember_PGM_PHD_FK2_EdiPriceHeaderDiscount] REFERENCES [EdiPriceHeaderDiscount] ([PHD_PK]) ON DELETE CASCADE,
	[PGM_GroupCode] [varchar](20) NOT NULL,
 CONSTRAINT [PK_EdiPriceDiscountGroupMember] PRIMARY KEY NONCLUSTERED
(
	[PGM_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiPriceDiscountGroupMember]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX NR_UC__PGM_PHD_PGM_GroupCode ON [dbo].[EdiPriceDiscountGroupMember] (PGM_PHD, PGM_GroupCode) WITH (ALLOW_PAGE_LOCKS = OFF)
;
				", "DROP TABLE EdiPriceDiscountGroupMember");
			}
		}

		#endregion

		#region ClientLicencePriceItem

		static DatabaseObjectCreateScript ClientLicencePriceItem
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientLicencePriceItem", @"
CREATE TABLE dbo.ClientLicencePriceItem(
	[L7_PK] uniqueidentifier NOT NULL,
	[L7_L6] uniqueidentifier NOT NULL CONSTRAINT [ClientLicencePriceItem_L7_L6_FK2_ClientLicencePriceHeader] REFERENCES [ClientLicencePriceHeader] ([L6_PK]),
	[L7_Category] varchar(3) NOT NULL DEFAULT(''),
	[L7_Code] varchar(3) NOT NULL DEFAULT(''),
	[L7_Order] smallint NOT NULL DEFAULT(0),
	[L7_FeeType] varchar(3) NOT NULL DEFAULT(''),
	[L7_Description] nvarchar(250) NOT NULL DEFAULT(''),
	[L7_Price] decimal(18,6) NOT NULL DEFAULT(0),
	[L7_ParentCategory] varchar(3) NOT NULL DEFAULT(''),
	[L7_ParentCode] varchar(3) NOT NULL DEFAULT(''),
	[L7_WebParentCode] varchar(3) NOT NULL DEFAULT(''),
	[L7_LicenceUnits] decimal(18, 6) NOT NULL DEFAULT(0),
	[L7_UnitBreak] int NOT NULL DEFAULT(0),
	[L7_RX_NKCurrency] varchar(3) NOT NULL DEFAULT(''),
	[L7_Ref4] varchar(50) NOT NULL DEFAULT(''),
	[L7_PGM_DiscountGroupCode] [varchar](20) NULL DEFAULT (''),
	[L7_ChargeCode] [varchar](10) NOT NULL DEFAULT (''),
	[L7_DepositChargeCode] [varchar](10) NOT NULL DEFAULT (''),
	[L7_ChargeBasis] [nvarchar](100) NOT NULL DEFAULT (''),
	[L7_DiscountChargeCode] [varchar](10) NOT NULL DEFAULT (''),
	[L7_UnitBreakParentCode] varchar(3) NOT NULL DEFAULT(''),
	[L7_Language] VARCHAR(7) NOT NULL DEFAULT(''),
	[L7_ExchangeRateGroupCode] VARCHAR(5) NOT NULL DEFAULT(''),
	[L7_IsVolumeAdjustmentEligible] BIT NOT NULL DEFAULT (1),
	[L7_ProductAvailability] VARCHAR(1) NOT NULL DEFAULT '',
	[L7_ProductDisplayCategory] VARCHAR(3) NOT NULL DEFAULT '',
	[L7_CountryTierCode] VARCHAR(3) NOT NULL DEFAULT '',
	[L7_RN_NKDisbursementCountry] VARCHAR(2) NOT NULL DEFAULT '',
	[L7_DisbursementDirection] VARCHAR(3) NOT NULL DEFAULT '',
 CONSTRAINT [PK_ClientLicencePriceItem] PRIMARY KEY NONCLUSTERED ([L7_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF),
 CONSTRAINT [Constraint_L7_ProductAvailability] CHECK (L7_ProductAvailability IN ('Y', 'N', '')),
 CONSTRAINT [Constraint_L7_DisbursementDirection] CHECK (L7_DisbursementDirection IN ('ALL', 'IMP', 'EXP', 'DOM', 'CST', 'OTH', ''))
);

ALTER TABLE [ClientLicencePriceItem]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientLicencePriceItem] WITH NOCHECK 
ADD CONSTRAINT [Constraint_L7_Category] CHECK (NOT (L7_Category <> 'CWN' AND (L7_RN_NKDisbursementCountry <> '' OR L7_DisbursementDirection <> '')));

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__L7_L6_L7_Category_L7_Code_L7_UnitBreak_L7_Ref4] ON [dbo].[ClientLicencePriceItem]
(
	[L7_L6] ASC,
	[L7_Category] ASC,
	[L7_Code] ASC,
	[L7_UnitBreak] ASC,
	[L7_Ref4] ASC
)
WHERE ([L7_Code]<>'' AND [L7_CountryTierCode]='' AND [L7_Category] <> 'CWN')
WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__L7_L6_L7_Code_L7_RN_NKDisbursementCountry_L7_DisbursementDirection] ON [dbo].[ClientLicencePriceItem]
(
	[L7_L6] ASC,
	[L7_Code] ASC,
	[L7_RN_NKDisbursementCountry] ASC,
	[L7_DisbursementDirection] ASC
)
WHERE ([L7_Code] <> '' AND [L7_Category] = 'CWN')
WITH (ALLOW_PAGE_LOCKS = OFF)
;

CREATE CLUSTERED INDEX [NR_RC__L7_L6] ON [dbo].[ClientLicencePriceItem] ([L7_L6] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;
				", "DROP TABLE ClientLicencePriceItem");
			}
		}

		#endregion

		#region EdiPriceUsageMapping

		static DatabaseObjectCreateScript EdiPriceUsageMapping
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiPriceUsageMapping", @"
CREATE TABLE dbo.EdiPriceUsageMapping(
	[PUM_PK] uniqueidentifier NOT NULL,
	[PUM_L6] uniqueidentifier NOT NULL CONSTRAINT [EdiPriceUsageMapping_PUM_L6_FK2_ClientLicencePriceHeader] REFERENCES [ClientLicencePriceHeader] ([L6_PK]) on delete cascade,
	[PUM_PriceCategory] varchar(3) NOT NULL default(''),
	[PUM_PriceCode] varchar(3) NOT NULL,
	[PUM_UsageCategory] varchar(3) NOT NULL default (''),
	[PUM_UsageCode] varchar(3) NOT NULL,

 CONSTRAINT [PK_EdiPriceUsageMapping] PRIMARY KEY NONCLUSTERED ([PUM_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiPriceUsageMapping]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__PUM_ALL] ON [dbo].[EdiPriceUsageMapping]
(
	[PUM_L6] ASC,
	[PUM_PriceCategory] ASC,
	[PUM_PriceCode] ASC,
	[PUM_UsageCategory] ASC,
	[PUM_UsageCode] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__PUM_L6_PUM_UsageCategory_PUM_UsageCode] ON [dbo].[EdiPriceUsageMapping]
(
	[PUM_L6] ASC,
	[PUM_UsageCategory] ASC,
	[PUM_UsageCode] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF);
				", "DROP TABLE EdiPriceUsageMapping");
			}
		}

		#endregion

		#region STL

		static DatabaseObjectCreateScript EdiPriceItemRate
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiPriceItemRate", @"
CREATE TABLE dbo.EdiPriceItemRate(
	[PIR_PK] [uniqueidentifier] NOT NULL,
	[PIR_L7] [uniqueidentifier] NOT NULL CONSTRAINT [EdiPriceItemRate_PIR_L7_FK2_ClientLicencePriceItem] REFERENCES [ClientLicencePriceItem] ([L7_PK]) ON DELETE CASCADE,
	[PIR_RX_NKCurrency] [varchar](3) NOT NULL,
	[PIR_Price] [money] NOT NULL DEFAULT ((0)),
CONSTRAINT [PK_EdiPriceItemRate] PRIMARY KEY NONCLUSTERED ([PIR_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF),
CONSTRAINT Constraint_PIR_RX_NKCurrency CHECK (PIR_RX_NKCurrency != '')
);

ALTER TABLE [EdiPriceItemRate]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [NR_RC__PIR_L7] ON [dbo].[EdiPriceItemRate] ([PIR_L7] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__PIR_L7_PIR_RX_NKCurrency] ON [dbo].[EdiPriceItemRate] (PIR_L7, PIR_RX_NKCurrency) WITH (ALLOW_PAGE_LOCKS = OFF)
				", "DROP TABLE EdiPriceItemRate");
			}
		}

		static DatabaseObjectCreateScript EdiPriceHeaderLink
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiPriceHeaderLink", @"
CREATE TABLE dbo.EdiPriceHeaderLink(
	[PHL_PK] [uniqueidentifier] NOT NULL,
	[PHL_L6] [uniqueidentifier] NOT NULL CONSTRAINT [EdiPriceHeaderLink_PHL_L6_FK2_ClientLicencePriceHeader] REFERENCES [ClientLicencePriceHeader] ([L6_PK]) ON DELETE CASCADE,
	[PHL_LD] [uniqueidentifier] NULL CONSTRAINT [EdiPriceHeaderLink_PHL_LD_FK2_LicenceDatabase] REFERENCES [LicenceDatabase] ([LD_PK]),

	-- price currency - can be different to invoice currency
	[PHL_RX_NKCurrency] [varchar](3) NOT NULL DEFAULT (''),
	[PHL_ValidFrom] [smalldatetime] NOT NULL,
	[PHL_ValidTo] [smalldatetime] NULL,
	[PHL_SystemCreateTimeUtc] [smalldatetime] NULL,
	[PHL_SystemCreateUser] [varchar](3) NOT NULL DEFAULT (''),
	[PHL_VolumeCode] varchar(3) NOT NULL DEFAULT('STD'),
	[PHL_VolumePercent] decimal(5, 2) NOT NULL DEFAULT(100),
	[PHL_CorePackCode] varchar(3) NOT NULL DEFAULT('INC'),
	[PHL_CoreUpliftPercent] decimal(5, 2) NOT NULL DEFAULT(0),
 CONSTRAINT [PK_EdiPriceHeaderLink] PRIMARY KEY NONCLUSTERED 
(
	PHL_PK ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiPriceHeaderLink]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__PHL_LD_PHL_ValidFrom] ON [EdiPriceHeaderLink] ([PHL_LD] ASC, [PHL_ValidFrom] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;

				", "DROP TABLE EdiPriceHeaderLink");
			}
		}

		static DatabaseObjectCreateScript EdiLicenceSetting
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiLicenceSetting", @"
create table dbo.EdiLicenceSetting
(
	LS9_PK uniqueidentifier not null,
	LS9_LD uniqueidentifier not null CONSTRAINT [EdiLicenceSetting_LS9_LD_FK2_LicenceDatabase] REFERENCES [LicenceDatabase] ([LD_PK]),
	LS9_ValidFrom smalldatetime NULL,
	LS9_ValidTo smalldatetime NULL,
	LS9_Type varchar(3) not null default('DIS'),
	LS9_Price money not null default(0),
	LS9_IsActive bit NOT NULL DEFAULT (1),
	LS9_Name varchar(30) NOT NULL DEFAULT(''),
	LS9_Percent decimal(5, 2) NOT NULL default(0),
	LS9_Comment nvarchar(1024) NOT NULL DEFAULT(''),
	LS9_GE_Department1 uniqueidentifier NULL CONSTRAINT [EdiLicenceSetting_LS9_GE_Department1_FK2_GlbDepartment] REFERENCES [GlbDepartment] ([GE_PK]),
	LS9_GE_Department2 uniqueidentifier NULL CONSTRAINT [EdiLicenceSetting_LS9_GE_Department2_FK2_GlbDepartment] REFERENCES [GlbDepartment] ([GE_PK]),
	LS9_RX_NKPriceCurrency varchar(3) NOT NULL DEFAULT (''),
	LS9_SystemCreateTimeUtc [smalldatetime] NULL,
	LS9_SystemCreateUser varchar(3) NOT NULL DEFAULT (''),
	LS9_IsManualOverride BIT NOT NULL DEFAULT (0),
	LS9_Units decimal(18, 4) NOT NULL default(0),
	LS9_ApplyDiscounts bit NOT NULL default(1)
CONSTRAINT [PK_EdiLicenceSetting] PRIMARY KEY NONCLUSTERED (LS9_PK ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiLicenceSetting]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [NR_RC__LS9_LD] ON EdiLicenceSetting ([LS9_LD] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE NONCLUSTERED INDEX [NR_RX__LS9_Type_LS9_ValidTo] ON EdiLicenceSetting ([LS9_Type] ASC, [LS9_ValidTo]) WITH (ALLOW_PAGE_LOCKS = OFF)
;

				", "DROP TABLE EdiLicenceSetting");
			}
		}

		static DatabaseObjectCreateScript EdiUsageInvoice
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiUsageInvoice", @"
create table dbo.EdiUsageInvoice(
	EUI_PK uniqueidentifier not null,
	EUI_PeriodStart date not null,
	EUI_PrepayAmountIncTax money not null default(0),
	EUI_AccountBalanceIncTax money not null default(0),
	EUI_AH_Invoice uniqueidentifier NOT NULL CONSTRAINT [EdiUsageInvoice_EUI_AH_FK2_AccTransactionHeader] REFERENCES [AccTransactionHeader] ([AH_PK]),
CONSTRAINT [PK_EdiUsageInvoice] PRIMARY KEY NONCLUSTERED
(
	EUI_PK ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiUsageInvoice]
	SET (LOCK_ESCALATION = DISABLE);

create unique clustered index NR_UC__EUI_PeriodStart_EUI_AH_Invoice on EdiUsageInvoice (EUI_PeriodStart, EUI_AH_Invoice) WITH (ALLOW_PAGE_LOCKS = OFF)
;
create nonclustered index NR_RX__EUI_AH_Invoice on EdiUsageInvoice (EUI_AH_Invoice) WITH (ALLOW_PAGE_LOCKS = OFF)
				", "DROP TABLE EdiUsageInvoice");
			}
		}

		static DatabaseObjectCreateScript EdiBilledUsage
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiBilledUsage", @"
create table dbo.EdiBilledUsage(
	BU9_PK uniqueidentifier not null,
	BU9_PeriodStart date not null,
	BU9_UsageCode varchar(3) not null default (''),
	BU9_UsageSubCode varchar(50) not null default(''),
	BU9_UnitCount decimal(14, 4) not null default(0),
	BU9_L7 uniqueidentifier null CONSTRAINT [EdiBilledUsage_BU9_L7_FK2_ClientLicencePriceItem] REFERENCES [ClientLicencePriceItem] ([L7_PK]),
	BU9_PriceCode varchar(3) not null default (''),
	BU9_UnitPrice money not null default (0),
	BU9_PriceCurrency varchar(3) not null default (''),
	BU9_TransactionAmountPreDiscount money not null default(0),
	BU9_TransactionAmountPostDiscount money not null default(0),
	BU9_TransactionProcessingAmount money not null default(0),
	BU9_LocalAmountPreDiscount money not null default(0),
	BU9_LocalAmountPostDiscount money not null default(0),
	BU9_LocalProcessingAmount money not null default(0),
	BU9_LCC uniqueidentifier NULL CONSTRAINT [EdiBilledUsage_BU9_LCC_FK2_ClientCompany] REFERENCES [ClientCompany] ([LCC_PK]),
	BU9_LD uniqueidentifier NULL CONSTRAINT [EdiBilledUsage_BU9_LD_FK2_LicenceDatabase] REFERENCES [LicenceDatabase] ([LD_PK]),
	BU9_LC uniqueidentifier NULL CONSTRAINT [EdiBilledUsage_BU9_LC_FK2_LicenceCompany] REFERENCES [LicenceCompany] ([LC_PK]),
	BU9_AH_Invoice uniqueidentifier NOT NULL CONSTRAINT [EdiBilledUsage_BU9_AH_FK2_AccTransactionHeader] REFERENCES [AccTransactionHeader] ([AH_PK]),
	BU9_AC_AmountChargeCode uniqueidentifier NULL CONSTRAINT [EdiBilledUsage_BU9_AC_AmountChargeCode_FK2_AccChargeCode] REFERENCES [AccChargeCode] ([AC_PK]),
	BU9_AC_DiscountChargeCode uniqueidentifier NULL CONSTRAINT [EdiBilledUsage_BU9_AC_DiscountChargeCode_FK2_AccChargeCode] REFERENCES [AccChargeCode] ([AC_PK]),
	BU9_BillingModel varchar(3) NOT NULL default('STL'),
	BU9_CommitmentAdjustTransactionPostDiscount money not null default(0),
	BU9_TotalDiscountUnits decimal(18, 4) NOT NULL DEFAULT(0)
CONSTRAINT [PK_EdiBilledUsage] PRIMARY KEY NONCLUSTERED 
(
	BU9_PK ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiBilledUsage]
	SET (LOCK_ESCALATION = DISABLE);

create clustered index NR_RC__BU9_PeriodStart on EdiBilledUsage (BU9_PeriodStart) WITH (ALLOW_PAGE_LOCKS = OFF);

create nonclustered index NR_RX__BU9_PeriodStart_BU9_PriceCode on EdiBilledUsage (BU9_PeriodStart, BU9_PriceCode) WITH (ALLOW_PAGE_LOCKS = OFF)
;
create nonclustered index NR_RX__BU9_LCC on EdiBilledUsage (BU9_LCC) WITH (ALLOW_PAGE_LOCKS = OFF)
;
create nonclustered index NR_RX__BU9_AH_Invoice on EdiBilledUsage (BU9_AH_Invoice) WITH (ALLOW_PAGE_LOCKS = OFF)
;
create nonclustered index NR_RX__BU9_LD on EdiBilledUsage (BU9_LD) WITH (ALLOW_PAGE_LOCKS = OFF)
;
create nonclustered index NR_RX__BU9_L7 on EdiBilledUsage (BU9_L7) WITH (ALLOW_PAGE_LOCKS = OFF)
;
create nonclustered index NR_RX__BU9_LC on EdiBilledUsage (BU9_LC) WITH (ALLOW_PAGE_LOCKS = OFF)
				", "DROP TABLE EdiBilledUsage");
			}
		}

		static DatabaseObjectCreateScript EdiBilledDiscount
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiBilledDiscount", @"
CREATE TABLE dbo.EdiBilledDiscount
(
	BD9_PK UNIQUEIDENTIFIER NOT NULL,
	BD9_BU9_Usage  UNIQUEIDENTIFIER NULL CONSTRAINT [EdiBilledDiscount_BD9_BU9_Usage_FK2_EdiBilledUsage] REFERENCES [EdiBilledUsage] ([BU9_PK]),
	BD9_Percent DECIMAL(5, 2) NOT NULL DEFAULT (0),
	BD9_TransactionAmount MONEY NOT NULL DEFAULT (0),
	BD9_PHD_Discount  UNIQUEIDENTIFIER NULL CONSTRAINT [EdiBilledDiscount_BD9_PHD_Discount_FK2_EdiPriceHeaderDiscount] REFERENCES [EdiPriceHeaderDiscount] ([PHD_PK]),
	BD9_Type VARCHAR(3) NOT NULL DEFAULT ('')
CONSTRAINT [PK_EdiBilledDiscount] PRIMARY KEY NONCLUSTERED 
(
	BD9_PK ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiBilledDiscount]
	SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [NR_RC__BD9_BU9_Usage] ON [dbo].[EdiBilledDiscount] ([BD9_BU9_Usage]) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE NONCLUSTERED INDEX [NR_RX__BD9_PHD_Discount] ON [dbo].[EdiBilledDiscount] ([BD9_PHD_Discount]) WITH (ALLOW_PAGE_LOCKS = OFF)
;

", "DROP TABLE EdiBilledDiscount");
			}
		}

		static DatabaseObjectCreateScript EdiPriceHeaderExchangeRate
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiPriceHeaderExchangeRate", @"
				
CREATE TABLE dbo.EdiPriceHeaderExchangeRate(
	PHE_PK UNIQUEIDENTIFIER NOT NULL,
	PHE_L6 UNIQUEIDENTIFIER NOT NULL CONSTRAINT [EdiPriceHeaderExchangeRate_PHE_L6_FK2_ClientLicencePriceHeader] REFERENCES [ClientLicencePriceHeader] ([L6_PK]) ON DELETE CASCADE,
	PHE_GroupCode VARCHAR(5) NOT NULL DEFAULT (''),
	PHE_RX_NKCurrency VARCHAR(3) NOT NULL DEFAULT (''),
	PHE_Rate DECIMAL(18, 9) NOT NULL DEFAULT (0),
	PHE_UpliftPercent DECIMAL(5, 2) NOT NULL DEFAULT (0),
CONSTRAINT [PK_EdiPriceHeaderExchangeRate] PRIMARY KEY NONCLUSTERED 
(
	PHE_PK ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiPriceHeaderExchangeRate]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__PHE_L6_PHE_GroupCode_PHE_RX_NKCurrency] ON [dbo].[EdiPriceHeaderExchangeRate] (PHE_L6 ASC, PHE_GroupCode ASC, PHE_RX_NKCurrency ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

", "DROP TABLE EdiPriceHeaderExchangeRate");
			}
		}

		#endregion

		#region ClientLicenceFee

		static DatabaseObjectCreateScript ClientLicenceFee
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientLicenceFee", @"
CREATE TABLE dbo.ClientLicenceFee(
	[L8_PK] [uniqueidentifier] NOT NULL,
	[L8_Type] [varchar](3) NOT NULL,
	[L8_Description] [nvarchar](1024) NOT NULL,
	[L8_Amount] [money] NOT NULL,
	[L8_ChargeCode] [varchar](10) NOT NULL,
	[L8_StartDate] [smalldatetime] NULL,
	[L8_EndDate] [smalldatetime] NULL,
	[L8_Comment] [nvarchar](1024) NOT NULL DEFAULT(''),
	[L8_SystemCode] [varchar](3) NOT NULL DEFAULT ('ODM'),
	[L8_RX_NKCurrency] [varchar](3) NOT NULL DEFAULT (''),
	[L8_RenewalMonths] [int] NOT NULL DEFAULT(1),
	[L8_Order] [smallint] NOT NULL DEFAULT(0),
	[L8_OH_RemitToOrg] [uniqueidentifier] NULL,
	[L8_LD] [uniqueidentifier] NULL CONSTRAINT [ClientLicenceFee_L8_LD_FK2_LicenceDatabase] REFERENCES [LicenceDatabase] ([LD_PK]),
	[L8_LC] [uniqueidentifier] NULL CONSTRAINT [ClientLicenceFee_L8_LC_FK2_LicenceCompany] REFERENCES [LicenceCompany] ([LC_PK]),
	[L8_IsDiscountable] bit not null default(1),
	[L8_TaxDateCode] varchar(3) NOT NULL DEFAULT ('CUR'),
CONSTRAINT [PK_ClientLicenceFee] PRIMARY KEY NONCLUSTERED
(
	[L8_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientLicenceFee]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientLicenceFee] WITH CHECK ADD  CONSTRAINT [ClientLicenceFee_L8_OH_FK2_OrgHeader] FOREIGN KEY([L8_OH_RemitToOrg]) REFERENCES [OrgHeader] ([OH_PK])

ALTER TABLE [ClientLicenceFee] ADD  CONSTRAINT [DF_ClientLicenceFee_L8_Type]  DEFAULT ('') FOR [L8_Type]
ALTER TABLE [ClientLicenceFee] ADD  CONSTRAINT [DF_ClientLicenceFee_L8_Description]  DEFAULT ('') FOR [L8_Description]
ALTER TABLE [ClientLicenceFee] ADD  CONSTRAINT [DF_ClientLicenceFee_L8_Amount]  DEFAULT ((0)) FOR [L8_Amount]
ALTER TABLE [ClientLicenceFee] ADD  CONSTRAINT [DF_ClientLicenceFee_L8_ChargeCode]  DEFAULT ('') FOR [L8_ChargeCode]

ALTER TABLE [ClientLicenceFee] ADD CONSTRAINT [Constraint_L8_TaxDateCode] CHECK (L8_TaxDateCode in ('CUR', 'STA', 'END'))

CREATE CLUSTERED INDEX NR_RC__L8_LD on ClientLicenceFee ([L8_LD]) WITH (ALLOW_PAGE_LOCKS = OFF)
CREATE NONCLUSTERED INDEX NR_RX__L8_LC on ClientLicenceFee ([L8_LC]) WITH (ALLOW_PAGE_LOCKS = OFF)


				", "DROP TABLE ClientLicenceFee");
			}
		}

		#endregion

		#region ClientChargeableUsage

		static DatabaseObjectCreateScript ClientChargeableUsage
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientChargeableUsage", @"
CREATE TABLE dbo.ClientChargeableUsage(
	[U1_PK] [uniqueidentifier] NOT NULL,
	[U1_Code] [varchar](3) NOT NULL DEFAULT(''),
	[U1_SubCode] [varchar](50) NOT NULL DEFAULT(''),
	[U1_PeriodStart] [smalldatetime] NOT NULL,
	[U1_UnitCount] [decimal](14, 4) NOT NULL DEFAULT(0),
	[U1_AH_Invoice] [uniqueidentifier] NULL,
	[U1_UnitPrice] [money] NOT NULL DEFAULT(0),
	[U1_TotalPrice] [money] NOT NULL DEFAULT(0),
	[U1_RX_NKCurrency] [varchar](3) NOT NULL DEFAULT (''),
	[U1_Direction] [varchar](3) NOT NULL DEFAULT (''),
	[U1_LC] [uniqueidentifier] NULL,
	[U1_UpdateTime] [smalldatetime] NULL,
	[U1_InvoicedUnitCount] [decimal](14, 4) NOT NULL DEFAULT(0),
	[U1_Parent] [uniqueidentifier] NULL,
	[U1_Reference1] [varchar](50) NOT NULL DEFAULT(''),
	[U1_Reference2] [varchar](50) NOT NULL DEFAULT(''),
	[U1_Reference3] [varchar](50) NOT NULL DEFAULT(''),
	[U1_Reference4] [varchar](50) NOT NULL DEFAULT(''),
	[U1_LD] uniqueidentifier NULL,
	[U1_LCC] uniqueidentifier NULL,
	[U1_ManuallyProcessed] bit not null default(0),
	[U1_SystemCreateTimeUtc] [smalldatetime] NOT NULL DEFAULT GetUtcDate(),
 CONSTRAINT [PK_ClientChargeableUsage] PRIMARY KEY NONCLUSTERED
(
	[U1_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientChargeableUsage]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientChargeableUsage]  WITH CHECK ADD  CONSTRAINT [ClientChargeableUsage_U1_AH_FK2_AccTransactionHeader] FOREIGN KEY([U1_AH_Invoice]) REFERENCES [AccTransactionHeader] ([AH_PK])
ALTER TABLE [ClientChargeableUsage] CHECK CONSTRAINT [ClientChargeableUsage_U1_AH_FK2_AccTransactionHeader]

ALTER TABLE [ClientChargeableUsage]  WITH CHECK ADD  CONSTRAINT [ClientChargeableUsage_U1_LC_FK2_LicenceCompany] FOREIGN KEY([U1_LC]) REFERENCES [LicenceCompany] ([LC_PK])
ALTER TABLE [ClientChargeableUsage] CHECK CONSTRAINT [ClientChargeableUsage_U1_LC_FK2_LicenceCompany]

ALTER TABLE [ClientChargeableUsage]  WITH CHECK ADD  CONSTRAINT [ClientChargeableUsage_U1_LD_FK2_LicenceDatabase] FOREIGN KEY([U1_LD]) REFERENCES [LicenceDatabase] ([LD_PK])
ALTER TABLE [ClientChargeableUsage] CHECK CONSTRAINT [ClientChargeableUsage_U1_LD_FK2_LicenceDatabase]

ALTER TABLE [ClientChargeableUsage]  WITH CHECK ADD  CONSTRAINT [ClientChargeableUsage_U1_LCC_FK2_ClientCompany] FOREIGN KEY([U1_LCC]) REFERENCES [ClientCompany] ([LCC_PK])
ALTER TABLE [ClientChargeableUsage] CHECK CONSTRAINT [ClientChargeableUsage_U1_LCC_FK2_ClientCompany]

CREATE CLUSTERED INDEX [NR_RC__U1_PeriodStart] ON [ClientChargeableUsage] ([U1_PeriodStart] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [NR_RX__U1_PeriodStart_U1_Code] ON [ClientChargeableUsage] ([U1_PeriodStart] ASC, [U1_Code] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [NR_RX__U1_LCC] ON [ClientChargeableUsage] ([U1_LCC] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX NR_RX__U1_PeriodStart_U1_LD_U1_Code_U1_SubCode ON ClientChargeableUsage (U1_PeriodStart, U1_LD, U1_Code, U1_SubCode) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE NONCLUSTERED INDEX NR_UX__ClientChargeableUsage_Data on ClientChargeableUsage(U1_PeriodStart, U1_Code, U1_LC, U1_Parent, U1_Reference1, U1_Reference2, U1_Reference3, U1_Reference4, U1_SubCode, U1_LD, U1_LCC, U1_RX_NKCurrency, U1_Direction)
WHERE U1_PeriodStart >= '2016-11-1'
WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX NR_RX__U1_Code_U1_SubCode_U1_LD_U1_PeriodStart ON [dbo].[ClientChargeableUsage] (U1_Code, U1_SubCode, U1_LD, U1_PeriodStart) WITH (ALLOW_PAGE_LOCKS = OFF);

				", "DROP TABLE ClientChargeableUsage");
			}
		}
		#endregion

		#region ClientLicenceHeaderEx

		static DatabaseObjectCreateScript ClientLicenceHeaderEx
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientLicenceHeaderEx", @"
CREATE TABLE dbo.ClientLicenceHeaderEx(
	[L0_PK] [uniqueidentifier] NOT NULL,
	[L0_LA] [uniqueidentifier] NOT NULL,
	[L0_LastMaintenancePercent] [decimal](8, 5) NOT NULL DEFAULT (0),
	[L0_NextMaintenancePercent] [decimal](8, 5) NOT NULL DEFAULT (0),
	[L0_CurrentMaintenancePercent] [decimal](8, 5) NOT NULL DEFAULT (0),
	[L0_LastNewSeatMaintenancePercent] [decimal](8, 5) NOT NULL DEFAULT (0),
	[L0_NextNewSeatMaintenancePercent] [decimal](8, 5) NOT NULL DEFAULT (0),
	[L0_FixedMaintenanceAmount] [money] NOT NULL DEFAULT (0),
	[L0_RenewalMonths] [int] NOT NULL DEFAULT(12),
	[L0_LastMaintenanceAmount] [money] NOT NULL DEFAULT (0),
	[L0_Comment] nvarchar(1024) NOT NULL DEFAULT(''),
	[L0_ClientRef] nvarchar(100) NOT NULL DEFAULT(''),
	[L0_RX_NKFixedMaintenanceCurrency] [varchar](3) NOT NULL DEFAULT (''),
	[L0_Surcharge] decimal(5, 2) NOT NULL DEFAULT (0),
	[L0_SurchargeDescription] varchar(100) NOT NULL DEFAULT (''),
	[L0_OH_Owner] [uniqueidentifier] NULL,
	[L0_TagNote] varchar(15) NOT NULL DEFAULT('')
CONSTRAINT [PK_ClientLicenceHeaderEx] PRIMARY KEY NONCLUSTERED
(
	[L0_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientLicenceHeaderEx]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientLicenceHeaderEx]  WITH CHECK ADD  CONSTRAINT [ClientLicenceHeaderEx_L9_LA_FK2_LicenceHeader] FOREIGN KEY([L0_LA]) REFERENCES [LicenceHeader] ([LA_PK])
ALTER TABLE [ClientLicenceHeaderEx]  WITH CHECK ADD  CONSTRAINT [ClientLicenceHeaderEx_L0_OH_Owner_FK2_OrgHeader] FOREIGN KEY([L0_OH_Owner]) REFERENCES [OrgHeader] ([OH_PK])

CREATE CLUSTERED INDEX FK_RC__L0_LA ON ClientLicenceHeaderEx (L0_LA) WITH (ALLOW_PAGE_LOCKS = OFF)
CREATE NONCLUSTERED INDEX FK_RX__L0_OH_Owner ON ClientLicenceHeaderEx (L0_OH_Owner) WITH (ALLOW_PAGE_LOCKS = OFF)

				", "DROP TABLE ClientLicenceHeaderEx");
			}
		}

		#endregion

		#region ClientStaff

		static DatabaseObjectCreateScript ClientStaff
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientStaff", @"
CREATE TABLE dbo.ClientStaff(
	[LS_PK] [uniqueidentifier] NOT NULL,
	[LS_LD] [uniqueidentifier] NOT NULL,
	[LS_Code] [char](3) NOT NULL DEFAULT(''),
	[LS_FullName] [nvarchar](256) NOT NULL DEFAULT(''),
	[LS_Email] [varchar](254) NOT NULL DEFAULT(''),
	[LS_IsActive] [bit] NOT NULL DEFAULT 1,
CONSTRAINT [PK_ClientStaff] PRIMARY KEY NONCLUSTERED ([LS_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientStaff]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientStaff] WITH CHECK ADD CONSTRAINT [ClientStaff_LS_LD_FK_LicenceDatabase] FOREIGN KEY([LS_LD]) REFERENCES [LicenceDatabase] ([LD_PK]) ON DELETE CASCADE
;
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LS_LD_LS_FullName_LS_Code ON ClientStaff (LS_LD, LS_FullName, LS_Code) include (LS_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LS_LD_LS_Code ON ClientStaff (LS_LD, LS_Code) WHERE LS_Code != '' WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE CLUSTERED INDEX [NR_RC__LS_LD] ON [ClientStaff] ([LS_LD] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
", "DROP TABLE ClientStaff");
			}
		}

		#endregion

		#region ClientCompany

		static DatabaseObjectCreateScript ClientCompany
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientCompany", @"
CREATE TABLE dbo.ClientCompany(
	[LCC_PK] [uniqueidentifier] NOT NULL,
	[LCC_Code] [char](3) NOT NULL CONSTRAINT [DF_ClientCompany_LCC_Code] DEFAULT (''),
	[LCC_Name] [nvarchar](100) NOT NULL CONSTRAINT [DF_ClientCompany_LCC_Name] DEFAULT (''),
	[LCC_ClientPK] [uniqueidentifier] NULL,
	[LCC_LD] [uniqueidentifier] NOT NULL,
	[LCC_OH] [uniqueidentifier] NULL,
	[LCC_RN_NKCountryCode] [char](2) NOT NULL CONSTRAINT [DF_ClientCompany_LCC_RN_NKCountryCode] DEFAULT (''),
	[LCC_CreateTimeUtc] smalldatetime NOT NULL DEFAULT(GETUTCDATE()),
	[LCC_DeactivateTimeUtc] smalldatetime NULL,
	[LCC_Address1] [nvarchar](50) NOT NULL DEFAULT (''),
	[LCC_Address2] [nvarchar](50) NOT NULL DEFAULT (''),
	[LCC_City] [nvarchar](25) NOT NULL DEFAULT (''),
	[LCC_Phone] [varchar](20) NOT NULL DEFAULT (''),
	[LCC_PostCode] [varchar](10) NOT NULL DEFAULT (''),
	[LCC_State] [nvarchar](25) NOT NULL DEFAULT (''),
	[LCC_RX_NKLocalCurrency] [varchar](3) NOT NULL DEFAULT (''),
	[LCC_BusinessRegNo] [nvarchar](35) NOT NULL DEFAULT (''),
	[LCC_BusinessRegNo2] [nvarchar](35) NOT NULL DEFAULT (''),
	[LCC_CustomsRegistrationNo] [nvarchar](35) NOT NULL DEFAULT (''),
	[LCC_WebAddress] [varchar](250) NOT NULL DEFAULT (''),
	[LCC_IsGSTRegistered] [bit] NOT NULL DEFAULT ((1)),
	[LCC_IsGSTCashBasis] [bit] NOT NULL DEFAULT ((0)),
	[LCC_IsWHTRegistered] [bit] NOT NULL DEFAULT ((0)),
	[LCC_IsWHTCashBasis] [bit] NOT NULL DEFAULT ((1)),
	[LCC_IsReciprocal] [bit] NOT NULL DEFAULT ((0)),
	[LCC_CodeValidFromUtc] [smalldatetime] NOT NULL,
	CONSTRAINT [PK_ClientCompany] PRIMARY KEY CLUSTERED
	(
		[LCC_PK] ASC
	) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientCompany]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientCompany] WITH CHECK ADD CONSTRAINT [FK_ClientCompany_LicenceDatabase] FOREIGN KEY([LCC_LD]) REFERENCES [LicenceDatabase] ([LD_PK])
;
ALTER TABLE [ClientCompany] WITH CHECK ADD CONSTRAINT [Constraint_LCC_Code] CHECK (LCC_Code <> '');
;
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LCC_LD_LCC_Code ON ClientCompany (LCC_LD, LCC_Code) WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE NONCLUSTERED INDEX NR_RX__LCC_OH on ClientCompany ([LCC_OH]) WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LCC_LD_LCC_ClientPK on ClientCompany (LCC_LD, LCC_ClientPK) where LCC_ClientPK is not null WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LCC_LD_LCC_OH on ClientCompany (LCC_LD, LCC_OH) where LCC_OH is not null WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE NONCLUSTERED INDEX NR_RX__LCC_RN_NKCountryCode on ClientCompany ([LCC_RN_NKCountryCode]) WITH (ALLOW_PAGE_LOCKS = OFF)
;
", "DROP TABLE ClientCompany");
			}
		}

		static DatabaseObjectCreateScript ClientCompanyCodeHistory
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientCompanyCodeHistory", @"
CREATE TABLE dbo.ClientCompanyCodeHistory(
	[CCH_LCC] [uniqueidentifier] NOT NULL CONSTRAINT [FK_ClientCompanyCodeHistory_ClientCompany] REFERENCES [ClientCompany] ([LCC_PK]) ON DELETE CASCADE,
	[CCH_Code] [char](3) NOT NULL,
	[CCH_CodeValidFromUtc] smalldatetime NOT NULL
);

ALTER TABLE [ClientCompanyCodeHistory]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CCH_LCC_CCH_Code_CCH_CodeValidFromUtc] ON [ClientCompanyCodeHistory]
(
	[CCH_LCC] ASC,
	[CCH_Code] ASC,
	[CCH_CodeValidFromUtc] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
", "DROP TABLE ClientCompanyCodeHistory");
			}
		}

		static DatabaseObjectCreateScript ClientCompanyActiveStatusHistory
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientCompanyActiveStatusHistory", @"
CREATE TABLE dbo.ClientCompanyActiveStatusHistory
(
	[CSH_LCC] [uniqueidentifier] NOT NULL CONSTRAINT [FK_ClientCompanyActiveStatusHistory_ClientCompany] REFERENCES [ClientCompany] ([LCC_PK]) ON DELETE CASCADE,
	[CSH_Period] [INT] NOT NULL
);

ALTER TABLE [ClientCompanyActiveStatusHistory]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__CSH_Period_CSH_LCC] ON [ClientCompanyActiveStatusHistory]
(
	[CSH_Period] ASC,
	[CSH_LCC] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__CSH_LCC_CSH_Period] ON [ClientCompanyActiveStatusHistory]
(
	[CSH_LCC] ASC,
	[CSH_Period] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
", "DROP TABLE ClientCompanyActiveStatusHistory");
			}
		}

		static DatabaseObjectCreateScript EdiClientCompanyMergeHistory
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiClientCompanyMergeHistory", @"
CREATE TABLE dbo.EdiClientCompanyMergeHistory
(
	[CMH_FromPK] [uniqueidentifier] NOT NULL,
	[CMH_ToPK] [uniqueidentifier] NOT NULL,
	[CMH_FromCode] [char](3) NOT NULL,
	[CMH_MergeTimeUtc] [datetime] NOT NULL
);

ALTER TABLE [EdiClientCompanyMergeHistory]
	SET (LOCK_ESCALATION = DISABLE);

CREATE NONCLUSTERED INDEX [NR_UX__CMH_FromPK] ON [EdiClientCompanyMergeHistory]
(
	[CMH_FromPK] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
", "DROP TABLE EdiClientCompanyMergeHistory");
			}
		}

		#endregion

		#region ClientBranch

		static DatabaseObjectCreateScript ClientBranch
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientBranch", @"
CREATE TABLE dbo.ClientBranch(
	[LCB_PK] [uniqueidentifier] NOT NULL,
	[LCB_Code] [char](3) NOT NULL CONSTRAINT [DF_ClientBranch_LCB_Code] DEFAULT (''),
	[LCB_LCC_Code] [char](3) NOT NULL CONSTRAINT [DF_ClientBranch_LCB_LCC_Code] DEFAULT (''),
	[LCB_Name] [nvarchar](100) NOT NULL CONSTRAINT [DF_ClientBranch_LCB_Name] DEFAULT (''),
	[LCB_ClientPK] [uniqueidentifier] NULL,
	[LCB_LD] [uniqueidentifier] NOT NULL,
	[LCB_OA] [uniqueidentifier] NULL,
	[LCB_IsActive] [bit] NOT NULL DEFAULT 1,
	CONSTRAINT [PK_ClientBranch] PRIMARY KEY CLUSTERED
	(
		[LCB_PK] ASC
	) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientBranch]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientBranch] WITH CHECK ADD CONSTRAINT [FK_ClientBranch_LicenceDatabase] FOREIGN KEY([LCB_LD]) REFERENCES [LicenceDatabase] ([LD_PK])
;
ALTER TABLE [ClientBranch] WITH CHECK ADD CONSTRAINT [Constraint_LCB_Code] CHECK (LCB_Code <> '');
;
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LCB_LD_LCB_ClientPK on ClientBranch (LCB_LD, LCB_ClientPK) where LCB_ClientPK is not null WITH (ALLOW_PAGE_LOCKS = OFF)
;
CREATE NONCLUSTERED INDEX NR_RX__LCB_OA on ClientBranch ([LCB_OA]) WHERE LCB_OA IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF)
;
", "DROP TABLE ClientBranch");
			}
		}

		#endregion

		#region ClientLicenceUsage

		static DatabaseObjectCreateScript ClientLicenceUsage
		{
			get
			{
				// Hijack this table to replace a generic index.
				// Can't be in a script on it's own since client override unit tests don't support such changes.

				return new DatabaseObjectCreateScript("ClientLicenceUsage", @"
CREATE TABLE dbo.ClientLicenceUsage(
	[LX_PK] [uniqueidentifier] NOT NULL,
	[LX_LS] [uniqueidentifier] NOT NULL,
	[LX_UsageTime] [smalldatetime] NULL,
	[LX_LicenceMode] [char](3) NOT NULL DEFAULT (''),
	[LX_Branch] [char](3) NOT NULL DEFAULT (''),
	[LX_ModuleCode] [varchar](3) NOT NULL DEFAULT (''),
	[LX_LCC] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_ClientLicenceUsage] PRIMARY KEY NONCLUSTERED 
(
	[LX_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientLicenceUsage]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientLicenceUsage]  WITH CHECK ADD  CONSTRAINT [ClientLicenceUsage_LX_LS_FK_ClientStaff] FOREIGN KEY([LX_LS]) REFERENCES [ClientStaff] ([LS_PK]) ON DELETE CASCADE
ALTER TABLE [ClientLicenceUsage]  WITH CHECK ADD  CONSTRAINT [ClientLicenceUsage_LX_LCC_FK_ClientCompany] FOREIGN KEY([LX_LCC]) REFERENCES [ClientCompany] ([LCC_PK])

IF EXISTS(select null from sys.indexes where name = 'NR_UX__PR_OH_Parent_PR_GC_PR_PartyType_PR_FreightTransportMode_PR_FreightContainerMode_PR_FreightDirection_PR_Service_PR_Loc') 
BEGIN
	DROP INDEX [NR_UX__PR_OH_Parent_PR_GC_PR_PartyType_PR_FreightTransportMode_PR_FreightContainerMode_PR_FreightDirection_PR_Service_PR_Loc] on [OrgRelatedParty];
	CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__PR_OH_Parent_PR_GC_PR_PartyType_PR_FreightTransportMode_PR_FreightContainerMode_PR_FreightDirection_PR_Service_PR_Loc] ON [OrgRelatedParty]
	(
		[PR_OH_Parent] ASC,
		[PR_GC] ASC,
		[PR_PartyType] ASC,
		[PR_FreightTransportMode] ASC,
		[PR_FreightContainerMode] ASC,
		[PR_FreightDirection] ASC,
		[PR_Service] ASC,
		[PR_Location] ASC,
		[PR_OA] ASC,
		[PR_RN_NKImporterCountry] ASC
	)
	WHERE [PR_PartyType]<>'WRP' AND [PR_PartyType]<>'SRV'
	WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
END", "DROP TABLE ClientLicenceUsage");
			}
		}

		static DatabaseObjectCreateScript ClientLicenceUsage_LX_UsageTime_LX_PK
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_UC__LX_UsageTime_LX_PK",
					"CREATE UNIQUE CLUSTERED INDEX NR_UC__LX_UsageTime_LX_PK ON ClientLicenceUsage (LX_UsageTime ASC, LX_PK) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientLicenceUsage.NR_UC__LX_UsageTime_LX_PK");
			}
		}

		/// <summary>
		/// Index for detailed usage query
		/// </summary>
		static DatabaseObjectCreateScript ClientLicenceUsageIndexDetailedUsage
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RX__LX_LCC_LX_LicenceMode_LX_UsageTime",
					"CREATE NONCLUSTERED INDEX NR_RX__LX_LCC_LX_LicenceMode_LX_UsageTime ON ClientLicenceUsage (LX_LCC,LX_LicenceMode,LX_UsageTime) INCLUDE (LX_LS,LX_Branch) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientLicenceUsage.NR_RX__LX_LCC_LX_LicenceMode_LX_UsageTime");
			}
		}

		#endregion

		#region EdiLicenceUsage

		static DatabaseObjectCreateScript EdiLicenceUsage
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiLicenceUsage", @"
CREATE TABLE dbo.EdiLicenceUsage(
	LX2_PK uniqueidentifier NOT NULL,
	LX2_Period int NOT NULL,
	LX2_LS uniqueidentifier NOT NULL,
	LX2_FirstUsageUtc smalldatetime NOT NULL,
	LX2_LastUsageUtc smalldatetime NOT NULL,
	LX2_UsageCount int not null,
	LX2_LicenceMode char(3) NOT NULL,
	LX2_ModuleCode varchar(3) NOT NULL,
	LX2_LCC uniqueidentifier NOT NULL,
CONSTRAINT PK_EdiLicenceUsage PRIMARY KEY NONCLUSTERED (LX2_PK ASC) WITH (ALLOW_PAGE_LOCKS = OFF),
CONSTRAINT EdiLicenceUsage_LX2_LCC_FK_ClientCompany FOREIGN KEY(LX2_LCC) REFERENCES ClientCompany (LCC_PK),
CONSTRAINT EdiLicenceUsage_LX2_LS_FK_ClientStaff FOREIGN KEY(LX2_LS) REFERENCES ClientStaff (LS_PK) ON DELETE CASCADE
);

ALTER TABLE [EdiLicenceUsage]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX NR_UC__EdiLicenceUsage_LX2_FirstUsageUtc_LX2_PK ON EdiLicenceUsage
(
	LX2_FirstUsageUtc,
	LX2_PK
)
WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE INDEX NR_UX__EdiLicenceUsage_Index ON EdiLicenceUsage
(
	LX2_Period,
	LX2_LicenceMode,
	LX2_LCC,
	LX2_ModuleCode,
	LX2_LS
) include (LX2_FirstUsageUtc, LX2_LastUsageUtc)
WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [FK_RX__LX2_LCC] ON EdiLicenceUsage ([LX2_LCC] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [FK_RX__LX2_LS] ON EdiLicenceUsage ([LX2_LS] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [FK_RX__LX2_LCC_LX2_FirstUsageUtc] ON [EdiLicenceUsage] ([LX2_LCC],[LX2_FirstUsageUtc])
INCLUDE ([LX2_Period],[LX2_LS],[LX2_LastUsageUtc],[LX2_UsageCount],[LX2_LicenceMode],[LX2_ModuleCode]) WITH (ALLOW_PAGE_LOCKS = OFF)
", "DROP TABLE EdiLicenceUsage");
			}
		}

		#endregion

		#region EdiExternalChargeableUsage

		static DatabaseObjectCreateScript EdiExternalChargeableUsage
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiExternalChargeableUsage", @"
CREATE TABLE dbo.EdiExternalChargeableUsage
(
	EXU_PK UNIQUEIDENTIFIER NOT NULL,
	EXU_Period INT NOT NULL,
	EXU_Code VARCHAR(3) NOT NULL,
	EXU_EnterpriseCode VARCHAR(3) NOT NULL,
	EXU_CompanyCode VARCHAR(3) NOT NULL,
	EXU_ServerCode VARCHAR(3) NOT NULL,
	EXU_UnitCount INT NOT NULL
CONSTRAINT [PK_EdiExternalChargeableUsage] PRIMARY KEY NONCLUSTERED 
(
	EXU_PK
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiExternalChargeableUsage] SET (LOCK_ESCALATION = DISABLE);

CREATE NONCLUSTERED INDEX [NR_RX__EXU_Period_EXU_Code] ON [EdiExternalChargeableUsage] ([EXU_Period] ASC, [EXU_Code] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE CLUSTERED INDEX NR_UC__EXU_Period_EXU_Code_EXU_EnterpriseCode_EXU_CompanyCode_EXU_ServerCode ON EdiExternalChargeableUsage
	(EXU_Period, EXU_Code, EXU_EnterpriseCode, EXU_CompanyCode, EXU_ServerCode) WITH (ALLOW_PAGE_LOCKS = OFF);

", "DROP TABLE EdiExternalChargeableUsage");
			}
		}

		#endregion

		#region ClientFaxPrice

		static DatabaseObjectCreateScript ClientFaxPrice
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientFaxPrice", @"
CREATE TABLE dbo.ClientFaxPrice(
	[CFP_PK] [uniqueidentifier] NOT NULL,
	[CFP_RX_NKCurrencyCode] [char](3) NOT NULL DEFAULT(''),
	[CFP_Year] [smallint] NOT NULL DEFAULT(0),
	[CFP_Month] [tinyint] NOT NULL DEFAULT(1),
	[CFP_PageRate] [money] NOT NULL DEFAULT (0)
CONSTRAINT [PK_ClientFaxPrice] PRIMARY KEY NONCLUSTERED
(
	[CFP_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

CREATE CLUSTERED INDEX [NR_RC__CFP_RX_NKCurrencyCode] ON [ClientFaxPrice] ([CFP_RX_NKCurrencyCode] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [ClientFaxPrice]
	SET (LOCK_ESCALATION = DISABLE);

", "DROP TABLE ClientFaxPrice");
			}
		}

		#endregion

		#region ClientPremiumService

		static DatabaseObjectCreateScript ClientPremiumService
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientPremiumService", @"
CREATE TABLE dbo.ClientPremiumService(
	[CPS_PK] [uniqueidentifier] NOT NULL,
	[CPS_LD] [uniqueidentifier] NOT NULL,
	[CPS_Type] [varchar](3) NOT NULL,
	[CPS_Units] [smallint] NOT NULL DEFAULT(1),
	[CPS_StartDate] [smalldatetime] NOT NULL,
	[CPS_EndDate] [smalldatetime] NULL,
	[CPS_ClientRef] [nvarchar](1024) NOT NULL DEFAULT(''),
	[CPS_Comment] [nvarchar](1024) NOT NULL DEFAULT(''),
	[CPS_DisplayOrder] [smallint] NOT NULL DEFAULT(0),
	[CPS_PriceHeaderCode] [varchar](3) NOT NULL DEFAULT(''),
	[CPS_LCC] [uniqueidentifier] NULL,
CONSTRAINT [PK_ClientPremiumService] PRIMARY KEY NONCLUSTERED
(
	[CPS_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientPremiumService]
	SET (LOCK_ESCALATION = DISABLE);

CREATE NONCLUSTERED INDEX [FK_RX__CPS_LCC] ON [ClientPremiumService] ([CPS_LCC] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
CREATE CLUSTERED INDEX [FK_RC__CPS_LD] ON [ClientPremiumService] ([CPS_LD] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [ClientPremiumService] WITH CHECK ADD  CONSTRAINT [ClientPremiumService_CPS_LD_FK2_LicenceDatabase] FOREIGN KEY([CPS_LD]) REFERENCES [LicenceDatabase] ([LD_PK])
ALTER TABLE [ClientPremiumService] WITH CHECK ADD  CONSTRAINT [ClientPremiumService_CPS_LCC_FK2_ClientCompany] FOREIGN KEY([CPS_LCC]) REFERENCES [ClientCompany] ([LCC_PK])
				", "DROP TABLE ClientPremiumService");
			}
		}

		#endregion

		#region ClientLicenceBillingExcludeOrg

		static DatabaseObjectCreateScript ClientLicenceBillingExcludeOrg
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientLicenceBillingExcludeOrg", @"
CREATE TABLE dbo.ClientLicenceBillingExcludeOrg(
	[CEX_PK] [UNIQUEIDENTIFIER] NOT NULL,
	[CEX_BillingSystem] [VARCHAR](3) NOT NULL DEFAULT(''),
	[CEX_LicenceCode] [VARCHAR](9) NOT NULL DEFAULT(''),
CONSTRAINT [PK_ClientLicenceBillingExcludeOrg] PRIMARY KEY NONCLUSTERED
(
	[CEX_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

CREATE CLUSTERED INDEX [NR_RC__CEX_BillingSystem] ON [ClientLicenceBillingExcludeOrg] ([CEX_BillingSystem] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [ClientLicenceBillingExcludeOrg]
	SET (LOCK_ESCALATION = DISABLE);

", "DROP TABLE ClientLicenceBillingExcludeOrg");
			}
		}

		#endregion

		#region ClientOrgImportHistory

		static DatabaseObjectCreateScript ClientOrgImportHistory
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientOrgImportHistory", @"
CREATE TABLE dbo.ClientOrgImportHistory(
	[O2_PK] [uniqueidentifier] NOT NULL,
	[O2_ClientKey] [varchar](36) NOT NULL,
	[O2_OrgCode] [nvarchar](12) NOT NULL,
	[O2_TargetOrgPK] [uniqueidentifier] NOT NULL,
	[O2_ImportedDate] [datetime] NOT NULL,
	[O2_ActionType] [nchar](20) NOT NULL,
	[O2_MatchCount] [int] NOT NULL
CONSTRAINT PK_UX__O2_PK PRIMARY KEY NONCLUSTERED ( [O2_PK] ) WITH (ALLOW_PAGE_LOCKS = OFF)
)

ALTER TABLE [ClientOrgImportHistory]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientOrgImportHistory]  WITH CHECK ADD  CONSTRAINT [ClientOrgImportHistory_O2_OH_FK_OrgHeader] FOREIGN KEY([O2_TargetOrgPK]) REFERENCES [OrgHeader] ([OH_PK]) ON DELETE CASCADE

ALTER TABLE dbo.ClientOrgImportHistory ADD [O2_PartitionBy] AS
	CASE WHEN O2_ActionType = 'CreateNew' THEN null
	ELSE [O2_ImportedDate]
	END PERSISTED", "DROP TABLE ClientOrgImportHistory");
			}
		}

		static DatabaseObjectCreateScript ClientOrgImportHistoryIndex_O2_ImportedDate
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RC__O2_ImportedDate",
					"CREATE CLUSTERED INDEX NR_RC__O2_ImportedDate ON ClientOrgImportHistory (O2_ImportedDate ASC) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientOrgImportHistory.NR_RC__O2_ImportedDate");
			}
		}

		static DatabaseObjectCreateScript ClientOrgImportHistoryIndex_O2_TargetOrgPK
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RX__O2_TargetOrgPK",
					"CREATE NONCLUSTERED INDEX NR_RX__O2_TargetOrgPK ON ClientOrgImportHistory (O2_TargetOrgPK ASC) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientOrgImportHistory.NR_RX__O2_TargetOrgPK");
			}
		}

		#endregion

		#region ClientStatisticsXML

		static DatabaseObjectCreateScript ClientStatisticsXML
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientStatisticsXML", @"
CREATE TABLE dbo.ClientStatisticsXML(
	[IM_PK] [uniqueidentifier] NOT NULL,
	[IM_ClientID] [varchar](36) NOT NULL,
	[IM_MessageTrackingID] [varchar](36) NOT NULL,
	[IM_MessageType] [varchar](200) NOT NULL,
	[IM_ApplicationCode] [varchar](3) NOT NULL,
	[IM_InsertUTC] [datetime] NOT NULL,
	[IM_Content] [nvarchar](max) NULL,
CONSTRAINT [PK_UX_IM_PK] PRIMARY KEY NONCLUSTERED ([IM_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientStatisticsXML]
	SET (LOCK_ESCALATION = DISABLE);
", "DROP TABLE ClientStatisticsXML");
			}
		}

		static DatabaseObjectCreateScript ClientStatisticsXMLIndex_IM_ClientID_IM_InsertUTC
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RX__IM_ClientID_IM_InsertUTC",
					"CREATE NONCLUSTERED INDEX NR_RX__IM_ClientID_IM_InsertUTC ON ClientStatisticsXML (IM_ClientID, IM_InsertUTC) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientStatisticsXML.NR_RX__IM_ClientID_IM_InsertUTC");
			}
		}

		static DatabaseObjectCreateScript ClientStatisticsXMLIndex_IM_InsertUTC
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RX__IM_InsertUTC",
					"CREATE NONCLUSTERED INDEX NR_RX__IM_InsertUTC ON ClientStatisticsXML (IM_InsertUTC ASC) INCLUDE (IM_ClientID) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientStatisticsXML.NR_RX__IM_InsertUTC");
			}
		}

		#endregion

		#region ClientStatisticsXMLArchive

		static DatabaseObjectCreateScript ClientStatisticsXMLArchive
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientStatisticsXMLArchive", @"
CREATE TABLE dbo.ClientStatisticsXMLArchive(
	[IMA_PK] [uniqueidentifier] NOT NULL,
	[IMA_ClientID] [varchar](36) NOT NULL,
	[IMA_MessageTrackingID] [varchar](36) NOT NULL,
	[IMA_MessageType] [varchar](200) NOT NULL,
	[IMA_ApplicationCode] [varchar](3) NOT NULL,
	[IMA_InsertUTC] [datetime] NOT NULL,
	[IMA_Content] [nvarchar](max) NULL,
CONSTRAINT [PK_UX_IMA_PK] PRIMARY KEY NONCLUSTERED ([IMA_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientStatisticsXMLArchive]
	SET (LOCK_ESCALATION = DISABLE);

", "DROP TABLE ClientStatisticsXMLArchive");
			}
		}

		static DatabaseObjectCreateScript ClientStatisticsXMLArchiveIndex_IMA_InsertUTC
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RC__IMA_InsertUTC",
					"CREATE CLUSTERED INDEX NR_RC__IMA_InsertUTC ON ClientStatisticsXMLArchive (IMA_InsertUTC ASC) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientStatisticsXMLArchive.NR_RC__IMA_InsertUTC");
			}
		}

		#endregion

		#region ClientOrgConsol

		static DatabaseObjectCreateScript ClientOrgConsol
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientOrgConsol", @"
CREATE TABLE dbo.ClientOrgConsol(
	[O7_PK] [uniqueidentifier] NOT NULL,
	[O7_AgentType] [varchar](10) NOT NULL,
	[O7_TransMode] [char](3) NOT NULL,
	[O7_LoadPort] [char](5) NOT NULL,
	[O7_DischargePort] [char](5) NOT NULL,
	[O7_TotalWeight] [decimal](15, 3) NOT NULL,
	[O7_TotalVolume] [decimal](15, 3) NOT NULL,
	[O7_ShipCount] [smallint] NOT NULL,
	[O7_CreateUTC] [datetime] NOT NULL,
	[O7_ExportUTC] [datetime] NOT NULL,
	[O7_ClientKey] [varchar](36) NOT NULL,
	[O7_OH] [uniqueidentifier] NOT NULL
CONSTRAINT [PK_OrgConsol] PRIMARY KEY NONCLUSTERED
(
	[O7_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientOrgConsol]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientOrgConsol]  WITH CHECK ADD  CONSTRAINT [ClientOrgConsol_O7_OH_FK_OrgHeader] FOREIGN KEY([O7_OH]) REFERENCES [OrgHeader] ([OH_PK]) ON DELETE CASCADE

CREATE CLUSTERED INDEX [NR_RC__O7_OH] ON [ClientOrgConsol] ([O7_OH] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

				", "DROP TABLE ClientOrgConsol");
			}
		}

		static DatabaseObjectCreateScript ClientOrgConsolClusteredIndex_O7_ExportUTC
		{
			get
			{
				return new DatabaseObjectCreateScript("NR_RX__O7_ExportUTC",
					"CREATE NONCLUSTERED INDEX NR_RX__O7_ExportUTC ON ClientOrgConsol (O7_ExportUTC ASC) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"DROP INDEX ClientOrgConsol.NR_RX__O7_ExportUTC");
			}
		}

		#endregion

		#region ClientWorkProject

		static DatabaseObjectCreateScript ClientWorkProject
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientWorkProject", @"
CREATE TABLE dbo.ClientWorkProject(
	[CWP_PK] [uniqueidentifier] NOT NULL,
	[CWP_WKP] [uniqueidentifier] NOT NULL,
	[CWP_LA] [uniqueidentifier] NULL,
	[CWP_CallbackBy] [datetime] NULL,
	[CWP_PlannedInstall] [datetime] NULL,
	[CWP_InstallDate] [datetime] NULL
CONSTRAINT [PK_ClientWorkProject] PRIMARY KEY NONCLUSTERED
(
	[CWP_PK] ASC
)
WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [ClientWorkProject]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientWorkProject]  WITH CHECK ADD  CONSTRAINT [ClientWorkProject_CWP_WKP_FK_WorkProject] FOREIGN KEY([CWP_WKP]) REFERENCES [WorkProject] ([WKP_PK]) ON DELETE CASCADE
ALTER TABLE [ClientWorkProject]  WITH CHECK ADD  CONSTRAINT [ClientWorkProject_CWP_LA_FK_Licenceheader] FOREIGN KEY([CWP_LA]) REFERENCES [Licenceheader] ([LA_PK]) ON DELETE CASCADE

CREATE CLUSTERED INDEX [NR_RC__CWP_WKP] ON [ClientWorkProject] ([CWP_WKP] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
				", "DROP TABLE ClientWorkProject");
			}
		}

		#endregion

		#region ClientTelRimRegistration

		static DatabaseObjectCreateScript ClientTelRimRegistration
		{
			get
			{
				return new DatabaseObjectCreateScript(
					"ClientTelRimRegistration",
					@"
CREATE TABLE dbo.ClientTelRimRegistration(
	[TRR_PK] [uniqueidentifier] NOT NULL,
	[TRR_EnrolmentId] [varchar](150) NOT NULL,
	[TRR_EnrolmentScheme] [varchar](16) NOT NULL,
	[TRR_CDH_ClientDeviceHeader] [uniqueidentifier] NOT NULL,
	[TRR_LCC_ClientCompany] [uniqueidentifier] NOT NULL,
	[TRR_OK_OrgCusCode] [uniqueidentifier] NOT NULL,
	[TRR_VehicleRegistration] [varchar](16) NOT NULL,
	[TRR_VehicleRegistrationState] [varchar](3) NOT NULL,
	[TRR_VehicleIdentificationNumber] [varchar](17) NOT NULL,
	[TRR_StartTime] [DateTimeOffset](4) NOT NULL,
	[TRR_EndTime] [DateTimeOffset](4) DEFAULT NULL,
	[TRR_InstallationDateTimeOffset] [DateTimeOffset](4) NOT NULL,
	CONSTRAINT [PK_ClientTelRimRegistration] PRIMARY KEY NONCLUSTERED ([TRR_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

CREATE CLUSTERED INDEX [NR_RC__TRR_StartTime] ON [ClientTelRimRegistration] ([TRR_StartTime] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE
	[ClientTelRimRegistration]
WITH CHECK
ADD CONSTRAINT
	[ClientTelRimRegistration_TRR_CDH_FK_DmgDeviceHeader]
FOREIGN KEY
	([TRR_CDH_ClientDeviceHeader])
REFERENCES
	[DmgDeviceHeader] ([CDH_PK]);

ALTER TABLE
	[ClientTelRimRegistration]
WITH CHECK
ADD CONSTRAINT
	[ClientTelRimRegistration_TRR_LCC_FK_ClientCompany]
FOREIGN KEY
	([TRR_LCC_ClientCompany])
REFERENCES
	[ClientCompany] ([LCC_PK]);

ALTER TABLE
	[ClientTelRimRegistration]
WITH CHECK
ADD CONSTRAINT
	[ClientTelRimRegistration_TRR_OK_FK_OrgCusCode]
FOREIGN KEY
	([TRR_OK_OrgCusCode])
REFERENCES
	[OrgCusCode] ([OK_PK]);

ALTER TABLE
	[ClientTelRimRegistration]
WITH CHECK
ADD CONSTRAINT
	[Constraint_TRR_Time]
CHECK
	(
		TRR_EndTime = NULL
		OR (TRR_StartTime < TRR_EndTime)
	);

ALTER TABLE
	[ClientTelRimRegistration]
WITH CHECK
ADD CONSTRAINT
	[Constraint_TRR_EnrolmentId]
CHECK
	(LEN(TRR_EnrolmentId) > 0);
	
ALTER TABLE [ClientTelRimRegistration]
	SET (LOCK_ESCALATION = DISABLE);
;
",
					"DROP TABLE ClientTelRimRegistration");
			}
		}

		#endregion

		#region Commission

		static DatabaseObjectCreateScript AccAmbiguousCommission
		{
			get
			{
				return new DatabaseObjectCreateScript("AccAmbiguousCommission", @"
CREATE TABLE dbo.AccAmbiguousCommission(
	[AC0_PK] [uniqueidentifier] NOT NULL,
	[AC0_CommissionStream] [varchar](3) NOT NULL DEFAULT (''),
	[AC0_AH_Source] [uniqueidentifier] NOT NULL,
	[AC0_CA0_SelectedAgreement] [uniqueidentifier] NULL,

	[AC0_SystemCreateTimeUtc] [smalldatetime] NOT NULL DEFAULT GetUtcDate(),
	[AC0_SystemCreateUser] [varchar](3) NOT NULL DEFAULT (''),

	CONSTRAINT [PK_AccAmbiguousCommission] PRIMARY KEY NONCLUSTERED ([AC0_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [AccAmbiguousCommission]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [AccAmbiguousCommission]  WITH CHECK ADD  CONSTRAINT [AccAmbiguousCommission_AC0_AH_Source_FK_AccTransactionHeader] FOREIGN KEY([AC0_AH_Source]) REFERENCES [AccTransactionHeader] ([AH_PK]) ON DELETE CASCADE
ALTER TABLE [AccAmbiguousCommission]  WITH CHECK ADD  CONSTRAINT [AccAmbiguousCommission_AC0_CA0_SelectedAgreement_FK_OrgCommissionAgreement] FOREIGN KEY([AC0_CA0_SelectedAgreement]) REFERENCES [OrgCommissionAgreement] ([CA0_PK]) ON DELETE CASCADE

CREATE UNIQUE CLUSTERED INDEX [NR_UC__AC0_AH_Source_AC0_CommissionStream] ON [dbo].[AccAmbiguousCommission] ([AC0_AH_Source] ASC, [AC0_CommissionStream] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

				", "DROP TABLE AccAmbiguousCommission");
			}
		}

		static DatabaseObjectCreateScript EdiCommissionAgreementCustomization
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiCommissionAgreementCustomization", @"
CREATE TABLE dbo.EdiCommissionAgreementCustomization
(
	[EZN_PK] [uniqueidentifier] NOT NULL,
	[EZN_CA0] [uniqueidentifier] NOT NULL,

	[EZN_IsAllDatabases] [bit] NOT NULL DEFAULT (1),
	[EZN_IsAllCompanies] [bit] NOT NULL DEFAULT (1),
	
	CONSTRAINT [PK_EdiCommissionAgreementCustomization] PRIMARY KEY NONCLUSTERED ([EZN_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiCommissionAgreementCustomization]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__EZN_CA0] ON [dbo].[EdiCommissionAgreementCustomization] ([EZN_CA0] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [EdiCommissionAgreementCustomization] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionAgreementCustomization_EZN_CA0_FK2_OrgCommissionAgreement_RRR_120N] FOREIGN KEY
		( [EZN_CA0] )
		REFERENCES [OrgCommissionAgreement]
		( [CA0_PK] )
	ON DELETE CASCADE
;
				", "DROP TABLE EdiCommissionAgreementCustomization");
			}
		}

		static DatabaseObjectCreateScript EdiCommissionAgreementDatabasePivot
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiCommissionAgreementDatabasePivot", @"
CREATE TABLE dbo.EdiCommissionAgreementDatabasePivot
(
	[EZD_PK] [uniqueidentifier] NOT NULL,
	[EZD_EZN] [uniqueidentifier] NOT NULL,
	[EZD_LD] [uniqueidentifier] NULL,

	CONSTRAINT [PK_EdiCommissionAgreementDatabasePivot] PRIMARY KEY NONCLUSTERED ([EZD_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiCommissionAgreementDatabasePivot]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__EZD_EZN_EZD_LD] ON [dbo].[EdiCommissionAgreementDatabasePivot] ([EZD_EZN] ASC, [EZD_LD] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [EdiCommissionAgreementDatabasePivot] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionAgreementDatabasePivot_EZD_EZN_FK2_EdiCommissionAgreementCustomerCutomization_RRR_120N] FOREIGN KEY
		( [EZD_EZN] )
		REFERENCES [EdiCommissionAgreementCustomization]
		( [EZN_PK] )
	ON DELETE CASCADE
;

ALTER TABLE [EdiCommissionAgreementDatabasePivot] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionAgreementDatabasePivot_EZD_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		( [EZD_LD] )
		REFERENCES [LicenceDatabase]
		( [LD_PK] )
;
				", "DROP TABLE EdiCommissionAgreementDatabasePivot");
			}
		}

		static DatabaseObjectCreateScript EdiCommissionAgreementCompanyPivot
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiCommissionAgreementCompanyPivot", @"
CREATE TABLE dbo.EdiCommissionAgreementCompanyPivot
(
	[EPY_PK] [uniqueidentifier] NOT NULL,
	[EPY_EZN] [uniqueidentifier] NOT NULL,
	[EPY_LCC] [uniqueidentifier] NOT NULL,

	CONSTRAINT [PK_EdiCommissionAgreementCompanyPivot] PRIMARY KEY NONCLUSTERED ([EPY_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiCommissionAgreementCompanyPivot]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__EPY_EZN_EPY_LCC] ON [dbo].[EdiCommissionAgreementCompanyPivot] ([EPY_EZN] ASC, [EPY_LCC] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [EdiCommissionAgreementCompanyPivot] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionAgreementCompanyPivot_EPY_EZN_FK2_EdiCommissionAgreementCustomerCutomization_RRR_120N] FOREIGN KEY
		( [EPY_EZN] )
		REFERENCES [EdiCommissionAgreementCustomization]
		( [EZN_PK] )
	ON DELETE CASCADE
;

ALTER TABLE [EdiCommissionAgreementCompanyPivot] WITH NOCHECK
	ADD CONSTRAINT [EEdiCommissionAgreementCompanyPivot_EPY_LCC_FK2_LicenceCompany_RRR_120N] FOREIGN KEY
		( [EPY_LCC] )
		REFERENCES [ClientCompany]
		( [LCC_PK] )
;
				", "DROP TABLE EdiCommissionAgreementCompanyPivot");
			}
		}

		static DatabaseObjectCreateScript EdiCommissionAgreementCompanyAutoAddDatabase
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiCommissionAgreementCompanyAutoAddDatabase", @"
CREATE TABLE dbo.EdiCommissionAgreementCompanyAutoAddDatabase
(
	[EPD_PK] [uniqueidentifier] NOT NULL,
	[EPD_EZN] [uniqueidentifier] NOT NULL,
	[EPD_LD] [uniqueidentifier] NULL,

	CONSTRAINT [PK_EdiCommissionAgreementCompanyAutoAddDatabase] PRIMARY KEY NONCLUSTERED ([EPD_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiCommissionAgreementCompanyAutoAddDatabase]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__EPD_EZN_EPD_LD] ON [dbo].[EdiCommissionAgreementCompanyAutoAddDatabase] ([EPD_EZN] ASC, [EPD_LD] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [EdiCommissionAgreementCompanyAutoAddDatabase] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionAgreementCompanyAutoAddDatabase_EPD_EZN_FK2_EdiCommissionAgreementCustomerCutomization_RRR_120N] FOREIGN KEY
		( [EPD_EZN] )
		REFERENCES [EdiCommissionAgreementCustomization]
		( [EZN_PK] )
	ON DELETE CASCADE
;

ALTER TABLE [EdiCommissionAgreementCompanyAutoAddDatabase] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionAgreementCompanyAutoAddDatabase_EPD_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		( [EPD_LD] )
		REFERENCES [LicenceDatabase]
		( [LD_PK] )
;
				", "DROP TABLE EdiCommissionAgreementCompanyAutoAddDatabase");
			}
		}

		static DatabaseObjectCreateScript EdiCommissionAgreementCompanyAutoAddCountry
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiCommissionAgreementCompanyAutoAddCountry", @"
CREATE TABLE dbo.EdiCommissionAgreementCompanyAutoAddCountry
(
	[EPC_PK] [uniqueidentifier] NOT NULL,
	[EPC_EZN] [uniqueidentifier] NOT NULL,
	[EPC_LD] [uniqueidentifier] NULL,
	[EPC_RN_NKCountry] [char](2) NOT NULL DEFAULT '',

	CONSTRAINT [PK_EdiCommissionAgreementCompanyAutoAddCountry] PRIMARY KEY NONCLUSTERED ([EPC_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiCommissionAgreementCompanyAutoAddCountry]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__EPC_EZN_EPC_LD_EPC_RN_NKCountry] ON [dbo].[EdiCommissionAgreementCompanyAutoAddCountry] ([EPC_EZN] ASC, [EPC_LD] ASC, [EPC_RN_NKCountry] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [EdiCommissionAgreementCompanyAutoAddCountry] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionAgreementCustomerCountry_EPC_EZN_FK2_EdiCommissionAgreementCustomerCutomization_RRR_120N] FOREIGN KEY
		( [EPC_EZN] )
		REFERENCES [EdiCommissionAgreementCustomization]
		( [EZN_PK] )
	ON DELETE CASCADE
;
				", "DROP TABLE EdiCommissionAgreementCompanyAutoAddCountry");
			}
		}

		static DatabaseObjectCreateScript EdiCommissionHeaderAdditionalInfo
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiCommissionHeaderAdditionalInfo", @"
CREATE TABLE dbo.EdiCommissionHeaderAdditionalInfo
(
	[ECH_PK] [uniqueidentifier] NOT NULL,
	[ECH_CH0] [uniqueidentifier] NOT NULL,
	[ECH_LCC] [uniqueidentifier] NULL,
	[ECH_LD] [uniqueidentifier] NULL,
	
	CONSTRAINT [PK_EdiCommissionHeaderAdditionalInfo] PRIMARY KEY NONCLUSTERED ([ECH_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiCommissionHeaderAdditionalInfo]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__ECH_CH0] ON [dbo].[EdiCommissionHeaderAdditionalInfo] ([ECH_CH0] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

ALTER TABLE [EdiCommissionHeaderAdditionalInfo] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionHeaderAdditionalInfo_ECH_CH0_FK2_AccCommissionHeader_RRR_120N] FOREIGN KEY
		( [ECH_CH0] )
		REFERENCES [AccCommissionHeader]
		( [CH0_PK] )
	ON DELETE CASCADE
;

ALTER TABLE [EdiCommissionHeaderAdditionalInfo] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionHeaderAdditionalInfo_ECH_LCC_FK2_ClientCompany_RRR_120N] FOREIGN KEY
		( [ECH_LCC] )
		REFERENCES [ClientCompany]
		( [LCC_PK] )
;

ALTER TABLE [EdiCommissionHeaderAdditionalInfo] WITH NOCHECK
	ADD CONSTRAINT [EdiCommissionHeaderAdditionalInfo_ECH_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
		( [ECH_LD] )
		REFERENCES [LicenceDatabase]
		( [LD_PK] )
;
				", "DROP TABLE EdiCommissionHeaderAdditionalInfo");
			}
		}

		#endregion

		#region EdiOrgOpportunity

		static DatabaseObjectCreateScript EdiOrgOpportunityEx
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiOrgOpportunityEx", @"
CREATE TABLE dbo.EdiOrgOpportunityEx 
(
	[EOM_PK] UNIQUEIDENTIFIER NOT NULL
	,[EOM_P8] UNIQUEIDENTIFIER NOT NULL
	,[EOM_GlobalPotential] INT NOT NULL DEFAULT 0
	,[EOM_LifetimeValueOver3Years] MONEY NOT NULL DEFAULT 0 
	,[EOM_RX_NKLifetimeValueCurrency] [VARCHAR](3) NOT NULL DEFAULT ''
	CONSTRAINT PK_UX__EOE_PK PRIMARY KEY NONCLUSTERED (EOM_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiOrgOpportunityEx]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX NR_UC__EOM_P8 ON [EdiOrgOpportunityEx] ([EOM_P8]) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [EdiOrgOpportunityEx]
	WITH NOCHECK ADD CONSTRAINT [EdiOrgOpportunityEx_EOM_P8_FK2_OrgOpportunity_RRR_120N] 
	FOREIGN KEY ([EOM_P8]) REFERENCES [OrgOpportunity]([P8_PK]);

", "DROP TABLE EdiOrgOpportunityEx");
			}
		}

		static DatabaseObjectCreateScript EdiOrgOpportunityValueAnalysis
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiOrgOpportunityValueAnalysis", @"

CREATE TABLE dbo.EdiOrgOpportunityValueAnalysis 
(
	 EOV_PK UNIQUEIDENTIFIER NOT NULL
	,EOV_P8 UNIQUEIDENTIFIER NOT NULL
	,EOV_ModuleCode VARCHAR(3) NOT NULL DEFAULT '' 
	,EOV_UserCount INT NOT NULL DEFAULT 0 
	CONSTRAINT PK_UX__EOV_PK PRIMARY KEY NONCLUSTERED (EOV_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiOrgOpportunityValueAnalysis]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX NR_UC__EOV_P8_EOV_ModuleCode ON EdiOrgOpportunityValueAnalysis (EOV_P8, EOV_ModuleCode) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [EdiOrgOpportunityValueAnalysis]
	WITH NOCHECK ADD CONSTRAINT [EdiOrgOpportunityValueAnalysis_EOV_P8_FK2_OrgOpportunity_RRR_120N] 
	FOREIGN KEY ([EOV_P8]) REFERENCES [OrgOpportunity]([P8_PK]);

", "DROP TABLE EdiOrgOpportunityValueAnalysis");
			}
		}

		#endregion

		#region EdiUsageReportQueue

		static DatabaseObjectCreateScript EdiUsageReportQueue
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiReportingQueue", @"
CREATE TABLE dbo.EdiReportingQueue
(
	[ERQ_PK] [UNIQUEIDENTIFIER] NOT NULL,
	[ERQ_ReportType] VARCHAR(3) NOT NULL,
	[ERQ_Status] VARCHAR(3) NOT NULL,
	[ERQ_Period] INT NOT NULL DEFAULT 0,
	[ERQ_LD] [UNIQUEIDENTIFIER],
	[ERQ_OH] [UNIQUEIDENTIFIER],
	[ERQ_OC] [UNIQUEIDENTIFIER],
	[ERQ_ReportName] VARCHAR(100) NOT NULL DEFAULT '',
	[ERQ_ReportFileFullName] VARCHAR(256) NOT NULL DEFAULT '',
	[ERQ_CreateTimeUtc] [DATETIME] NOT NULL DEFAULT GETUTCDATE(),
	[ERQ_GS_NKSupportStaff] VARCHAR(3) NOT NULL DEFAULT ''
	CONSTRAINT PK_UX__ERQ_PK PRIMARY KEY NONCLUSTERED (ERQ_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiReportingQueue]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [EdiReportingQueue] WITH NOCHECK
ADD CONSTRAINT [EdiReportingQueue_ERQ_LD_FK2_LicenceDatabase_RRR_120N] FOREIGN KEY
( [ERQ_LD] ) REFERENCES [LicenceDatabase] ( [LD_PK] );

ALTER TABLE [EdiReportingQueue] WITH NOCHECK
ADD CONSTRAINT [EdiReportingQueue_ERQ_OH_FK2_OrgHeader_RRR_120N] FOREIGN KEY
( [ERQ_OH] ) REFERENCES [OrgHeader] ( [OH_PK] );

ALTER TABLE [EdiReportingQueue] WITH NOCHECK
ADD CONSTRAINT [EdiReportingQueue_ERQ_OC_FK2_OrgContact_RRR_120N] FOREIGN KEY
( [ERQ_OC] ) REFERENCES [OrgContact] ( [OC_PK] );

CREATE CLUSTERED INDEX NR_RC__EdiReportingQueue_ReportType_Status ON [EdiReportingQueue] (ERQ_ReportType, ERQ_Status, ERQ_CreateTimeUtc) WITH (ALLOW_PAGE_LOCKS = OFF);

		", "DROP TABLE EdiReportingQueue");
			}
		}

		#endregion

		#region EdiPersonMergeQueue

		static DatabaseObjectCreateScript EdiPersonMergeQueue
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiPersonMergeQueue", @"
CREATE TABLE dbo.EdiPersonMergeQueue
(
	[EMQ_PK] [UNIQUEIDENTIFIER] NOT NULL,
	[EMQ_PER_RetainPerson] [UNIQUEIDENTIFIER] NOT NULL,
	[EMQ_PER_DissolvePerson] [UNIQUEIDENTIFIER] NOT NULL,
	CONSTRAINT PK_UX__EMQ_PK PRIMARY KEY NONCLUSTERED (EMQ_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiPersonMergeQueue]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [EdiPersonMergeQueue] WITH NOCHECK
ADD CONSTRAINT [EdiPersonMergeQueue_EMQ_PER_RetainPerson_FK2_GlbPerson_RRR_120N] FOREIGN KEY
( [EMQ_PER_RetainPerson] ) REFERENCES [GlbPerson] ( [PER_PK] );

ALTER TABLE [EdiPersonMergeQueue] WITH NOCHECK
ADD CONSTRAINT [EdiPersonMergeQueue_EMQ_PER_DissolvePerson_FK2_GlbPerson_RRR_120N] FOREIGN KEY
( [EMQ_PER_DissolvePerson] ) REFERENCES [GlbPerson] ( [PER_PK] );

CREATE UNIQUE CLUSTERED INDEX [NR_UC__EMQ_PER_DissolvePerson_EMQ_PER_RetainPerson] ON [EdiPersonMergeQueue] ([EMQ_PER_DissolvePerson] ASC, [EMQ_PER_RetainPerson] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

CREATE NONCLUSTERED INDEX [NR_RX__EMQ_PER_DissolvePerson] ON [EdiPersonMergeQueue] ([EMQ_PER_DissolvePerson] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

CREATE NONCLUSTERED INDEX [NR_RX__EMQ_PER_RetainPerson] ON [EdiPersonMergeQueue] ([EMQ_PER_RetainPerson] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

", "DROP TABLE EdiPersonMergeQueue");
			}
		}

		#endregion

		#region EdiERequestDocumentQueue

		static DatabaseObjectCreateScript EdiERequestDocumentQueue
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiERequestDocumentQueue", @"
 
CREATE TABLE dbo.EdiERequestDocumentQueue
(
	[EDQ_PK] [UNIQUEIDENTIFIER] NOT NULL,
	[EDQ_INC_ReferenceID] [UNIQUEIDENTIFIER] NOT NULL,
	[EDQ_FileName] NVARCHAR(256) NOT NULL DEFAULT '',
	[EDQ_Data] VARBINARY(MAX) NOT NULL,
	[EDQ_IsPublished] BIT NOT NULL DEFAULT 0,
	[EDQ_SystemCreateTimeUtc] SMALLDATETIME NOT NULL
	
	CONSTRAINT PK_UX__EDQ_PK PRIMARY KEY NONCLUSTERED (EDQ_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiERequestDocumentQueue] SET (LOCK_ESCALATION = DISABLE);
	
CREATE CLUSTERED INDEX NR_RC__EdiERequestDocumentQueue_INC_ReferenceID_SystemCreateTimeUtc ON [EdiERequestDocumentQueue] (EDQ_INC_ReferenceID, EDQ_SystemCreateTimeUtc) WITH (ALLOW_PAGE_LOCKS = OFF);

		", "DROP TABLE EdiERequestDocumentQueue");
			}
		}

		#endregion

		#region EdiCustomerUserAccount

		static DatabaseObjectCreateScript EdiCustomerUserAccount
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiCustomerUserAccount", @"
CREATE TABLE dbo.EdiCustomerUserAccount(
	[EUA_PK] [uniqueidentifier] NOT NULL,
	[EUA_LD] [uniqueidentifier] NOT NULL,
	[EUA_UserID] [varchar](36) NOT NULL DEFAULT(''),
	[EUA_FullName] [nvarchar](256) NOT NULL DEFAULT(''),
	[EUA_Email] [varchar](254) NOT NULL DEFAULT(''),
	[EUA_PreviousEmail] [varchar](254) NOT NULL DEFAULT(''),
	[EUA_IsActive] [bit] NOT NULL DEFAULT 1,
	[EUA_OC_WebAccessContact] [uniqueidentifier] NULL,
	[EUA_LS] [uniqueidentifier] NULL,
	[EUA_IsContactRelationshipActive] [bit] NOT NULL DEFAULT 1,
	[EUA_ContactRelationshipStatus] [varchar](3) NOT NULL DEFAULT(''),
	[EUA_IsEmailVerificationRequired] [bit] NOT NULL DEFAULT 0,
	[EUA_SystemVerifiedDateUtc] [smalldatetime] NULL,
	[EUA_UserVerifiedDateUtc] [smalldatetime] NULL,
	[EUA_RN_NKCountry] [varchar](2) NOT NULL DEFAULT(''),
	[EUA_IsEmailOverridden] [bit] NOT NULL DEFAULT 0,
	[EUA_SystemCreateTimeUtc]  SMALLDATETIME NULL,
	[EUA_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
	[EUA_SystemLastEditTimeUtc]  SMALLDATETIME NULL,
	[EUA_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',

CONSTRAINT [PK_EdiCustomerUserAccount] PRIMARY KEY NONCLUSTERED ([EUA_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiCustomerUserAccount]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [EdiCustomerUserAccount_EUA_LD_FK_LicenceDatabase] FOREIGN KEY([EUA_LD]) REFERENCES [LicenceDatabase] ([LD_PK]) ON DELETE CASCADE;
ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [EdiCustomerUserAccount_EUA_OC_WebAccessContact_FK_OrgContact] FOREIGN KEY([EUA_OC_WebAccessContact]) REFERENCES [OrgContact] ([OC_PK]);
ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [EdiCustomerUserAccount_EUA_LS_FK_ClientStaff] FOREIGN KEY([EUA_LS]) REFERENCES [ClientStaff] ([LS_PK]);
ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [Constraint_EUA_UserID] CHECK (EUA_UserID != '');
CREATE UNIQUE CLUSTERED INDEX NR_UC__EUA_LD_EUA_UserID ON EdiCustomerUserAccount (EUA_LD, EUA_UserID) WITH (ALLOW_PAGE_LOCKS = OFF);
CREATE INDEX FK_RX__EUA_OC_WebAccessContact ON EdiCustomerUserAccount (EUA_OC_WebAccessContact) WHERE EUA_OC_WebAccessContact IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF);
", "DROP TABLE EdiCustomerUserAccount");
			}
		}

		#endregion

		#region EdiUserAgreement

		static DatabaseObjectCreateScript EdiUserAgreement
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiUserAgreement", @"
CREATE TABLE dbo.EdiUserAgreement(
	[ERA_PK] [uniqueidentifier] NOT NULL,
	[ERA_Title] [nvarchar](200) NOT NULL DEFAULT(''),
	[ERA_Content] [nvarchar](MAX) NOT NULL DEFAULT(''),
	[ERA_VersionNumber] [int] NOT NULL,
	[ERA_MinorVersion] [int] NOT NULL DEFAULT(0),
	[ERA_VariantCode] [varchar](3) NOT NULL DEFAULT(''),
	[ERA_VariantDescription] [nvarchar](200) NOT NULL DEFAULT(''),
	[ERA_RN_NKCountryCode] [varchar](2) NOT NULL DEFAULT(''),
	[ERA_Type] [varchar](3) NOT NULL,
	[ERA_EffectiveTimeUtc] [smalldatetime] NOT NULL,
	[ERA_IsActive] [bit] NOT NULL DEFAULT 1,
	[ERA_SystemCreateTimeUtc] [smalldatetime] NULL,
	[ERA_SystemCreateUser] [varchar](3) NOT NULL DEFAULT(''),
	[ERA_SystemLastEditTimeUtc] [smalldatetime] NULL,
	[ERA_SystemLastEditUser] [varchar](3) NOT NULL DEFAULT(''),
CONSTRAINT [PK_EdiUserAgreement] PRIMARY KEY NONCLUSTERED ([ERA_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiUserAgreement]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE NONCLUSTERED INDEX NR_UX__ERA_Type_ERA_VariantCode_ERA_RN_NKCountryCode_ERA_VersionNumber_ERA_MinorVersion ON EdiUserAgreement (ERA_Type, ERA_VariantCode, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_MinorVersion) INCLUDE (ERA_PK) WITH (ALLOW_PAGE_LOCKS = OFF);
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__ERA_Type_ERA_VariantCode_ERA_RN_NKCountryCode_ERA_EffectiveTimeUtc ON EdiUserAgreement (ERA_Type, ERA_VariantCode, ERA_RN_NKCountryCode, ERA_EffectiveTimeUtc) INCLUDE (ERA_PK) WITH (ALLOW_PAGE_LOCKS = OFF);
CREATE CLUSTERED INDEX [NR_RC__ERA_Type] ON [EdiUserAgreement] ([ERA_Type] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
", "DROP TABLE EdiUserAgreement");
			}
		}

		#endregion

		#region EdiUserAgreementAssignment

		static DatabaseObjectCreateScript EdiUserAgreementAssignment
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiUserAgreementAssignment", @"
CREATE TABLE dbo.EdiUserAgreementAssignment(
	[EAE_PK] [uniqueidentifier] NOT NULL,
	[EAE_ParentTableCode] [varchar](3) NOT NULL,
	[EAE_ParentID] [uniqueidentifier] NOT NULL,
	[EAE_OH_ClientAgreementOrg] [uniqueidentifier] NULL,
	[EAE_AgreementType] [varchar](3) NOT NULL,
	[EAE_VariantCode] [varchar](3) NOT NULL DEFAULT(''),
	[EAE_AllowOnlineAcceptance] [bit] NOT NULL DEFAULT 1,
	[EAE_SystemCreateTimeUtc] [smalldatetime] NULL,
	[EAE_SystemCreateUser] [varchar](3) NOT NULL DEFAULT(''),
	[EAE_SystemLastEditTimeUtc] [smalldatetime] NULL,
	[EAE_SystemLastEditUser] [varchar](3) NOT NULL DEFAULT(''),
CONSTRAINT [PK_EdiUserAgreementAssignment] PRIMARY KEY NONCLUSTERED ([EAE_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiUserAgreementAssignment] WITH CHECK
	  ADD CONSTRAINT [EdiUserAgreementAssignment_EAE_OH_ClientAgreementOrg_FK2_OrgHeader_PK] FOREIGN KEY
		  ( [EAE_OH_ClientAgreementOrg] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )

ALTER TABLE [EdiUserAgreementAssignment] ADD CONSTRAINT [EdiUserAgreementAssignment_EAE_ParentTableCode] CHECK ([EAE_ParentTableCode] IN ('LE', 'LD'));

CREATE CLUSTERED INDEX NR_RC__EAE_ParentTableCode_EAE_ParentID ON EdiUserAgreementAssignment (EAE_ParentTableCode ASC, EAE_ParentID ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__EAE_ParentTableCode_EAE_ParentID_EAE_AgreementType ON EdiUserAgreementAssignment (EAE_ParentTableCode, EAE_ParentID, EAE_AgreementType) INCLUDE (EAE_PK) WITH (ALLOW_PAGE_LOCKS = OFF);
", "DROP TABLE EdiUserAgreementAssignment");
			}
		}

		#endregion

		#region EdiUserAgreementAcceptanceLog

		static DatabaseObjectCreateScript EdiUserAgreementAcceptanceLog
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiUserAgreementAcceptanceLog", @"
CREATE TABLE dbo.EdiUserAgreementAcceptanceLog(
	EUL_PK uniqueidentifier NOT NULL,
	EUL_EUA uniqueidentifier NULL,
	EUL_OH uniqueidentifier NULL,
	EUL_GS uniqueidentifier NULL,
	EUL_ERA uniqueidentifier NOT NULL,
	EUL_LD uniqueidentifier NULL,
	EUL_LE uniqueidentifier NULL,
	EUL_AcceptanceTimeUtc smalldatetime NOT NULL,
	EUL_AcceptedByName nvarchar(254) NOT NULL DEFAULT '',
	EUL_AcceptedByJobTitle nvarchar(254) NOT NULL DEFAULT '',
	EUL_AcceptedByIPAddress varchar(39) NOT NULL DEFAULT '',
	EUL_AcceptedByEmail varchar(254) NOT NULL DEFAULT '',
	EUL_RN_NKCountryCode varchar(2) NOT NULL DEFAULT(''),
	EUL_MajorVersion varchar(10) NOT NULL DEFAULT '',
	EUL_MinorVersion varchar(10) NOT NULL DEFAULT '',
	EUL_VariantCode varchar(3) NOT NULL DEFAULT '',
	EUL_Type varchar(3) NOT NULL DEFAULT '',
CONSTRAINT [PK_EdiUserAgreementAcceptanceLog] PRIMARY KEY NONCLUSTERED ([EUL_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiUserAgreementAcceptanceLog] WITH CHECK ADD CONSTRAINT [EdiUserAgreementAcceptanceLog_EUL_EUA_FK_EdiCustomerUserAccount] FOREIGN KEY([EUL_EUA]) REFERENCES [EdiCustomerUserAccount] ([EUA_PK]) ON DELETE CASCADE;
ALTER TABLE [EdiUserAgreementAcceptanceLog] WITH CHECK ADD CONSTRAINT [EdiUserAgreementAcceptanceLog_EUL_ERA_FK_EdiUserAgreement] FOREIGN KEY([EUL_ERA]) REFERENCES [EdiUserAgreement] ([ERA_PK]) ON DELETE CASCADE;
ALTER TABLE [EdiUserAgreementAcceptanceLog] WITH CHECK ADD CONSTRAINT [EdiUserAgreementAcceptanceLog_EUL_EUA_FK_OrgHeader] FOREIGN KEY([EUL_OH]) REFERENCES [OrgHeader] ([OH_PK]) ON DELETE CASCADE;
ALTER TABLE [EdiUserAgreementAcceptanceLog] WITH CHECK ADD CONSTRAINT [EdiUserAgreementAcceptanceLog_EUL_EUA_FK_GlbStaff] FOREIGN KEY([EUL_GS]) REFERENCES [GlbStaff] ([GS_PK]) ON DELETE CASCADE;
ALTER TABLE [EdiUserAgreementAcceptanceLog] WITH CHECK ADD CONSTRAINT [EdiUserAgreementAcceptanceLog_EUL_LD_FK_LicenceDatabase] FOREIGN KEY([EUL_LD]) REFERENCES [LicenceDatabase] ([LD_PK]);
ALTER TABLE [EdiUserAgreementAcceptanceLog] WITH CHECK ADD CONSTRAINT [EdiUserAgreementAcceptanceLog_EUL_LE_FK2_LicenceEnterprise_RRR_120N] FOREIGN KEY ([EUL_LE]) REFERENCES [LicenceEnterprise] ([LE_PK]);

CREATE CLUSTERED INDEX [NR_RC__EUL_EUA] ON [EdiUserAgreementAcceptanceLog] ([EUL_EUA] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

", "DROP TABLE EdiUserAgreementAcceptanceLog");
			}
		}

		#endregion

		#region EdiTrustedSystem

		static DatabaseObjectCreateScript EdiTrustedSystem
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiTrustedSystem", @"
 
CREATE TABLE dbo.EdiTrustedSystem
(
	[ETS_PK] UNIQUEIDENTIFIER NOT NULL,
	[ETS_SystemNumber] VARCHAR(12) NOT NULL DEFAULT '' ,
	[ETS_Product] VARCHAR(3) NOT NULL,
	[ETS_SystemID] VARCHAR(50) NOT NULL DEFAULT(''),
	[ETS_Description] NVARCHAR(250) NOT NULL DEFAULT(''),
	[ETS_SystemEndpointUrl] VARCHAR(2048) NULL,
	[ETS_SecretKey_COMPRESSED] VARBINARY(MAX) NULL,
	[ETS_SecretKeyExpiryUtc] SMALLDATETIME NULL,
	[ETS_ETM_Certificate] UNIQUEIDENTIFIER NULL,
	[ETS_AccessTokenExpiryOverride] INT NOT NULL DEFAULT 0,
	[ETS_IssueRefreshToken] BIT NOT NULL DEFAULT 1,
	[ETS_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[ETS_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT(''),
	[ETS_SystemLastEditTimeUtc] DATETIME NULL,
	[ETS_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT(''),
	CONSTRAINT PK_UX__ETS_PK PRIMARY KEY NONCLUSTERED (ETS_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiTrustedSystem] WITH NOCHECK
	  ADD CONSTRAINT [EdiTrustedSystem_ETS_ETM_Certificate_FK2_EdiTrustedMessagingConfig_RRR_120N] FOREIGN KEY
		  ( [ETS_ETM_Certificate] )
		  REFERENCES [EdiTrustedMessagingConfig]
		  ( [ETM_PK] )
;

CREATE UNIQUE INDEX NR_UX__ETS_Product_ETS_SystemID ON [EdiTrustedSystem] ([ETS_Product], [ETS_SystemID])
WHERE ETS_Product <> '' AND ETS_SystemID <> '' WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__ETS_SystemNumber] ON [EdiTrustedSystem] ([ETS_SystemNumber] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);

ALTER TABLE [EdiTrustedSystem] WITH CHECK ADD CONSTRAINT [Constraint_ETS_Product] CHECK (ETS_Product <> '');

		", "DROP TABLE EdiTrustedSystem");
			}
		}

		#endregion

		#region EdiTrustedMessagingConfig

		static DatabaseObjectCreateScript EdiTrustedMessagingConfig
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiTrustedMessagingConfig", @"
 
CREATE TABLE dbo.EdiTrustedMessagingConfig
(
	[ETM_PK] UNIQUEIDENTIFIER NOT NULL,
	[ETM_Product] VARCHAR(3) NOT NULL,
	[ETM_CertificateThumbprint] VARCHAR(50) NULL,
	[ETM_CertificateData] VARBINARY(MAX)	NULL,
	[ETM_CertificatePassword] VARCHAR(128) NOT NULL DEFAULT '',
	[ETM_CertificateType] VARCHAR(3) NOT NULL,
	CONSTRAINT PK_UX__ETM_PK PRIMARY KEY NONCLUSTERED (ETM_PK) WITH (ALLOW_PAGE_LOCKS = OFF)
);

CREATE CLUSTERED INDEX [NR_RC__ETM_Product] ON [EdiTrustedMessagingConfig] ([ETM_Product] ASC) WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);

ALTER TABLE [EdiTrustedMessagingConfig] WITH NOCHECK 
ADD CONSTRAINT [Constraint_ETM_CertificateType] CHECK (ETM_CertificateType in ('CSC', 'TSC', 'PDC'));

CREATE UNIQUE INDEX [NR_UX__ETM_Product_ETM_CertificateType] ON [EdiTrustedMessagingConfig] ([ETM_Product] ASC, [ETM_CertificateType] ASC)
WHERE [ETM_CertificateType] <> 'TSC'
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);


ALTER TABLE [EdiTrustedMessagingConfig] WITH CHECK ADD CONSTRAINT [Constraint_ETM_Product] CHECK (ETM_Product <> '');

		", "DROP TABLE EdiTrustedMessagingConfig");
			}
		}

		#endregion

		#region EdiPromptSkip

		static DatabaseObjectCreateScript EdiPromptSkip
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiPromptSkip", @"
CREATE TABLE dbo.EdiPromptSkip(
	EPS_PK [uniqueidentifier] NOT NULL,
	EPS_Owner [uniqueidentifier] NOT NULL,
	EPS_SkipUntilDate [smalldatetime] NULL,
	EPS_Type [varchar](3) NOT NULL,
CONSTRAINT [PK_EdiPromptSkip] PRIMARY KEY NONCLUSTERED ([EPS_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

CREATE CLUSTERED INDEX [NR_RC__EPS_Owner] ON [EdiPromptSkip] ([EPS_Owner] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
", "DROP TABLE EdiPromptSkip");
			}
		}

		#endregion

		#region EdiLicenceDatabaseOrgSuggestion

		static DatabaseObjectCreateScript EdiLicenceDatabaseOrgSuggestion
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiLicenceDatabaseOrgSuggestion", @"
CREATE TABLE dbo.EdiLicenceDatabaseOrgSuggestion (
   [LDS_PK] UNIQUEIDENTIFIER NOT NULL,
   [LDS_LD] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [EdiLicenceDatabaseOrgSuggestion_LDS_LD_FK2_LicenceDatabase] REFERENCES [LicenceDatabase] ([LD_PK]) ON DELETE CASCADE,
   [LDS_OH] UNIQUEIDENTIFIER NOT NULL DEFAULT '' CONSTRAINT [EdiLicenceDatabaseOrgSuggestion_LDS_OH_FK2_OrgHeader] REFERENCES [OrgHeader] ([OH_PK]) ON DELETE CASCADE,
   [LDS_TotalScore] INT NOT NULL DEFAULT 0,
);

ALTER TABLE [EdiLicenceDatabaseOrgSuggestion]
	SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX NR_UC__LDS_LD_LDS_OH ON [EdiLicenceDatabaseOrgSuggestion] ([LDS_LD], [LDS_OH]) WITH (ALLOW_PAGE_LOCKS = OFF);

", "DROP TABLE EdiLicenceDatabaseOrgSuggestion");
			}
		}

		#endregion

		#region EdiTokenAuthOnBoardingData

		static DatabaseObjectCreateScript EdiTokenAuthOnBoardingData => new DatabaseObjectCreateScript(
			"EdiTokenAuthOnBoardingData", @"
CREATE TABLE dbo.EdiTokenAuthOnBoardingData 
(
	[TOD_PK] uniqueidentifier NOT NULL,
	[TOD_SystemUniqueIdentifier] varchar(100) NOT NULL,
	[TOD_LE] uniqueidentifier NOT NULL,
	[TOD_IDT] uniqueidentifier NOT NULL,
	[TOD_VerificationUsername] nvarchar(254) NOT NULL DEFAULT '',
	[TOD_VerificationUserPassword] nvarchar(35) NOT NULL DEFAULT '',
	[TOD_ConfigurationIdentifier] varchar(20) NOT NULL,
	[TOD_ClaimMappingName] varchar(50) NOT NULL,
	[TOD_ClaimMappingIdentifier] varchar(50) NOT NULL,
	[TOD_Status] varchar(3) NOT NULL DEFAULT 'NEW',
	[TOD_IM] uniqueidentifier NOT NULL,
	[TOD_Retry] int NOT NULL DEFAULT 0,
	[TOD_OIDCServer] varchar(3) NOT NULL DEFAULT 'AZU',
	[TOD_ValidTokenIssuerPrefix] varchar(100) NOT NULL DEFAULT '',
	[TOD_StagingPRLink] varchar(200) NOT NULL DEFAULT '',
	[TOD_ProdPRLink] varchar(200) NOT NULL DEFAULT '',
	[TOD_Enabled] bit NOT NULL DEFAULT 0,
	[TOD_WinzorOnly] bit NOT NULL DEFAULT 0,
	[TOD_SystemCreateTimeUtc] smalldatetime NOT NULL,
	[TOD_SystemCreateUser] varchar(3) NOT NULL DEFAULT '',
	[TOD_SystemLastEditTimeUtc] smalldatetime NOT NULL,
	[TOD_SystemLastEditUser] varchar(3) NOT NULL DEFAULT '',
);

ALTER TABLE  [EdiTokenAuthOnBoardingData]
	ADD CONSTRAINT [PK_UX__TOD_PK] PRIMARY KEY NONCLUSTERED ([TOD_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [EdiTokenAuthOnBoardingData]
	ADD CONSTRAINT [Constraint_TOD_Status] CHECK (TOD_Status in ('NEW','QUE','SPR','SMV','PPR','VER','CTC','COM','ERR','REV'));

ALTER TABLE [EdiTokenAuthOnBoardingData] WITH NOCHECK
	ADD CONSTRAINT [EdiTokenAuthOnBoardingData_TOD_LE_FK2_LicenceDatabase_PK] FOREIGN KEY
		( [TOD_LE] )
		REFERENCES [LicenceEnterprise]
		( [LE_PK] );

ALTER TABLE [EdiTokenAuthOnBoardingData] WITH NOCHECK
	ADD CONSTRAINT [EdiTokenAuthOnBoardingData_TOD_IM_FK2_IncidentMain_PK] FOREIGN KEY
		( [TOD_IM] )
		REFERENCES [IncidentMain]
		( [IM_PK] );

ALTER TABLE [EdiTokenAuthOnBoardingData] WITH NOCHECK
	ADD CONSTRAINT [EdiTokenAuthOnBoardingData_TOD_IDT_FK2_EdiIdentityTenant_PK] FOREIGN KEY
		( [TOD_IDT] )
		REFERENCES [EdiIdentityTenant]
		( [IDT_PK] );

CREATE UNIQUE CLUSTERED INDEX [FK_UC__TOD_LE] ON [EdiTokenAuthOnBoardingData] ([TOD_LE] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);
CREATE NONCLUSTERED INDEX [NR_RX__TOD_Status] ON [EdiTokenAuthOnBoardingData] ([TOD_Status] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE INDEX [FK_RX__TOD_IM] ON [EdiTokenAuthOnBoardingData] ([TOD_IM] ASC)
	WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF, IGNORE_DUP_KEY = OFF)

CREATE NONCLUSTERED INDEX [NR_RX__TOD_SystemCreateTimeUtc] ON [EdiTokenAuthOnBoardingData] ([TOD_SystemCreateTimeUtc] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [NR_RX__TOD_SystemLastEditTimeUtc] ON [EdiTokenAuthOnBoardingData] ([TOD_SystemLastEditTimeUtc] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

", "DROP TABLE EdiTokenAuthOnBoardingData");
		#endregion

		#region LicenceEnterpriseDomains

		static DatabaseObjectCreateScript LicenceEnterpriseDomains
		{
			get
			{
				return new DatabaseObjectCreateScript("LicenceEnterpriseDomains", @"

Create Table dbo.LicenceEnterpriseDomains
(
LED_PK uniqueidentifier NOT NULL,
LED_LE uniqueidentifier NOT NULL,
LED_Domain varchar(64) NOT NULL DEFAULT '',
LED_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
LED_SystemCreateTimeUtc smalldatetime NULL,
LED_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
LED_SystemLastEditTimeUtc datetime NULL
);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LED_LE_LED_Domain] ON [LicenceEnterpriseDomains] ([LED_LE] ASC, [LED_Domain] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);

ALTER TABLE [LicenceEnterpriseDomains]
ADD CONSTRAINT [PK_UX__LED_PK] PRIMARY KEY CLUSTERED ([LED_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [LicenceEnterpriseDomains] WITH NOCHECK
	  ADD CONSTRAINT [LicenceEnterpriseDomains_LED_LE_FK2_LicenceEnterprise_PK] FOREIGN KEY
		  ( [LED_LE] )
		  REFERENCES [LicenceEnterprise]
		  ( [LE_PK] )
;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LED_Domain] ON [LicenceEnterpriseDomains] ([LED_Domain]) WHERE LED_Domain != '' WITH (ALLOW_PAGE_LOCKS = OFF);

", "DROP TABLE LicenceEnterpriseDomains");
			}
		}

		#endregion

		#region FeatureControl Tables

		static DatabaseObjectCreateScript FeatureControlHeader
		{
			get
			{
				return new DatabaseObjectCreateScript("FeatureControlHeader", @"
CREATE TABLE dbo.FeatureControlHeader
(
   [FCM_PK] UNIQUEIDENTIFIER NOT NULL,
   [FCM_FeatureControlCode] VARCHAR(9) NOT NULL DEFAULT '',
   [FCM_Description] NVARCHAR(80) NOT NULL DEFAULT '',
   [FCM_GG_ReleaseGroup] UNIQUEIDENTIFIER NOT NULL,
   [FCM_WKI_ActiveWorkItem] UNIQUEIDENTIFIER NULL,
   [FCM_WKI_DeactivateWorkItem] UNIQUEIDENTIFIER NULL,
   [FCM_SystemCreateTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [FCM_SystemLastEditTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [FeatureControlHeader] SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [FeatureControlHeader]
ADD CONSTRAINT [PK_UX__FCM_PK] PRIMARY KEY NONCLUSTERED ([FCM_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE CLUSTERED INDEX [NR_UC__FCM_FeatureControlCode] ON [FeatureControlHeader] ([FCM_FeatureControlCode] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [FeatureControlHeader] WITH NOCHECK
	  ADD CONSTRAINT [FeatureControlHeader_FCM_GG_ReleaseGroup_FK2_GlbGroup_RRR_120N] FOREIGN KEY
		  ( [FCM_GG_ReleaseGroup] )
		  REFERENCES [GlbGroup]
		  ( [GG_PK] );

ALTER TABLE [FeatureControlHeader] WITH NOCHECK
	  ADD CONSTRAINT [FeatureControlHeader_FCM_WKI_ActiveWorkItem_FK2_WorkItem_RRR_120N] FOREIGN KEY
		  ( [FCM_WKI_ActiveWorkItem] )
		  REFERENCES [WorkItem]
		  ( [WKI_PK] )
;


ALTER TABLE [FeatureControlHeader] WITH NOCHECK
	  ADD CONSTRAINT [FeatureControlHeader_FCM_WKI_DeactivateWorkItem_FK2_WorkItem_RRR_120N] FOREIGN KEY
		  ( [FCM_WKI_DeactivateWorkItem] )
		  REFERENCES [WorkItem]
		  ( [WKI_PK] )
;
		  
				", "DROP TABLE FeatureControlHeader");
			}
		}

		static DatabaseObjectCreateScript FeatureControlRule
		{
			get
			{
				return new DatabaseObjectCreateScript("FeatureControlRule", @"
CREATE TABLE dbo.FeatureControlRule
(
   [FCR_PK] UNIQUEIDENTIFIER NOT NULL,
   [FCR_FCM_FeatureControl] UNIQUEIDENTIFIER NOT NULL,
   [FCR_Description] NVARCHAR(80) NOT NULL DEFAULT '',
   [FCR_IsActive] BIT NOT NULL DEFAULT 1,
   [FCR_RuleType] CHAR(3) NOT NULL DEFAULT 'CLI',
   [FCR_FCS_FeatureSet] UNIQUEIDENTIFIER NULL,
   [FCR_StartDateUtc] SMALLDATETIME NOT NULL,
   [FCR_EndDateUtc] SMALLDATETIME NULL,
   [FCR_Parameters] NVARCHAR(MAX) NOT NULL,
   [FCR_UseGlobalParameters] BIT NOT NULL DEFAULT 0,
   [FCR_SystemCreateTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCR_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [FCR_SystemLastEditTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCR_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);

ALTER TABLE [FeatureControlRule] SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [FeatureControlRule]
ADD CONSTRAINT [PK_UX__FCR_PK] PRIMARY KEY NONCLUSTERED ([FCR_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [FeatureControlRule] WITH NOCHECK 
ADD CONSTRAINT [Constraint_FCR_RuleType] CHECK (FCR_RuleType IN ('GLB', 'CLI', 'FCS'));

ALTER TABLE [FeatureControlRule] WITH NOCHECK 
ADD CONSTRAINT [Constraint_FCR_FCS_FeatureSet] CHECK ((FCR_RuleType IN ('GLB', 'CLI') AND FCR_FCS_FeatureSet IS NULL) OR (FCR_RuleType = 'FCS' AND FCR_FCS_FeatureSet IS NOT NULL));

CREATE UNIQUE INDEX [NR_UX__FCR_FCM_FeatureControl_FCR_RuleType] ON [FeatureControlRule] ([FCR_FCM_FeatureControl] ASC, [FCR_RuleType] ASC)
WHERE [FCR_RuleType] = 'GLB' AND [FCR_IsActive] = 1
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE INDEX [NR_UX__FCR_FCM_FeatureControl_FCR_Description] ON [FeatureControlRule] ([FCR_FCM_FeatureControl] ASC, [FCR_Description] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE INDEX [NR_UX__FCR_FCM_FeatureControl_FCR_FCS_FeatureSet] ON [FeatureControlRule] ([FCR_FCM_FeatureControl] ASC, [FCR_FCS_FeatureSet] ASC)
WHERE [FCR_FCS_FeatureSet] IS NOT NULL
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

CREATE CLUSTERED INDEX [NR_RC__FCR_FCM_FeatureControl_FCR_StartDateUtc] ON [FeatureControlRule] ([FCR_FCM_FeatureControl] ASC, [FCR_StartDateUtc] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [FeatureControlRule] WITH NOCHECK
	  ADD CONSTRAINT [FeatureControlRule_FCR_FCM_FeatureControl_FK2_FeatureControlHeader_RRR_120N] FOREIGN KEY
		  ( [FCR_FCM_FeatureControl] )
		  REFERENCES [FeatureControlHeader]
		  ( [FCM_PK] )
;

ALTER TABLE [FeatureControlRule] WITH NOCHECK
	  ADD CONSTRAINT [FeatureControlRule_FCR_FCS_FeatureSet_FK2_FeatureControlSet_RRR_120N] FOREIGN KEY
		  ( [FCR_FCS_FeatureSet] )
		  REFERENCES [FeatureControlSet]
		  ( [FCS_PK] )
;

				", "DROP TABLE FeatureControlRule");
			}
		}

		static DatabaseObjectCreateScript FeatureControlRuleLicenceDatabasePivot
		{
			get
			{
				return new DatabaseObjectCreateScript("FeatureControlRuleLicenceDatabasePivot", @"
CREATE TABLE dbo.FeatureControlRuleLicenceDatabasePivot
(
   [FCD_PK] UNIQUEIDENTIFIER NOT NULL,
   [FCD_FCR_FeatureControlRule] UNIQUEIDENTIFIER NOT NULL,
   [FCD_LD_LicenceDatabase] UNIQUEIDENTIFIER NOT NULL,
   [FCD_LD_DatabaseNumber] INT NOT NULL DEFAULT 0,
   [FCD_SystemCreateTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [FCD_SystemLastEditTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
	
ALTER TABLE [FeatureControlRuleLicenceDatabasePivot] SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [FeatureControlRuleLicenceDatabasePivot]
ADD CONSTRAINT [PK_UX__FCD_PK] PRIMARY KEY NONCLUSTERED ([FCD_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
 
CREATE UNIQUE CLUSTERED INDEX [NR_UC__FCD_FCR_FeatureControlRule_FCD_LD_LicenceDatabase] ON [FeatureControlRuleLicenceDatabasePivot] ([FCD_FCR_FeatureControlRule] ASC, [FCD_LD_LicenceDatabase] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [FeatureControlRuleLicenceDatabasePivot] WITH NOCHECK
	  ADD CONSTRAINT [FeatureControlRuleLicenceDatabasePivot_FCD_FCR_FeatureControlRule_FK2_FeatureControlRule_RRR_120N] FOREIGN KEY
		  ( [FCD_FCR_FeatureControlRule] )
		  REFERENCES [FeatureControlRule]
		  ( [FCR_PK] ) ON DELETE CASCADE
;

ALTER TABLE [FeatureControlRuleLicenceDatabasePivot] WITH NOCHECK
	  ADD CONSTRAINT [FeatureControlRuleLicenceDatabasePivot_FCD_LD_LicenceDatabase_FK2_FeatureControlRule_RRR_120N] FOREIGN KEY
		  ( [FCD_LD_LicenceDatabase] )
		  REFERENCES [LicenceDatabase]
		  ( [LD_PK] )
;

				", "DROP TABLE FeatureControlRuleLicenceDatabasePivot");
			}
		}

		static DatabaseObjectCreateScript FeatureControlSet
		{
			get
			{
				return new DatabaseObjectCreateScript("FeatureControlSet", @"
CREATE TABLE dbo.FeatureControlSet
(
   [FCS_PK] UNIQUEIDENTIFIER NOT NULL,
   [FCS_ProductName] NVARCHAR(200) NOT NULL DEFAULT '',
   [FCS_SystemCreateTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCS_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [FCS_SystemLastEditTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCS_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
	
ALTER TABLE [FeatureControlSet] SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__FCS_ProductName] ON [FeatureControlSet] ([FCS_ProductName] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)

ALTER TABLE [FeatureControlSet]
ADD CONSTRAINT [PK_UX__FCS_PK] PRIMARY KEY NONCLUSTERED ([FCS_PK] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;

				", "DROP TABLE FeatureControlSet");
			}
		}

		#endregion

		#region GenPivot

		internal static DatabaseObjectCreateScript GenPivot_Constraint_XX_RelationType_XX_Relation1TableCode_XX_Relation2TableCode
		{
			get
			{
				return new DatabaseObjectCreateScript("Constraint_XX_RelationType_XX_Relation1TableCode_XX_Relation2TableCode", @"
					ALTER TABLE [dbo].[GenPivot]
ADD CONSTRAINT[Constraint_XX_RelationType_XX_Relation1TableCode_XX_Relation2TableCode]
CHECK(NOT(XX_RelationType = 'WRK' AND XX_Relation1TableCode = 'WKI' AND XX_Relation2TableCode = 'IM'));
				", "ALTER TABLE GenPivot DROP CONSTRAINT Constraint_XX_RelationType_XX_Relation1TableCode_XX_Relation2TableCode");
			}
		}

		#endregion

		#region Logging

		static DatabaseObjectCreateScript ApplicationLogger
		{
			get
			{
				return new DatabaseObjectCreateScript(
					"ApplicationLogger",
					@"
CREATE TABLE dbo.ApplicationLogger(
  [ALG_PK]													UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UC__ALG_PK PRIMARY KEY WITH (ALLOW_PAGE_LOCKS = OFF),
  [ALG_Product]												NVARCHAR(35) DEFAULT '' NOT NULL,
  [ALG_Name]												NVARCHAR(80) DEFAULT '' NOT NULL,
  [ALG_Description]											NVARCHAR(256) DEFAULT '' NOT NULL,
  [ALG_Category]											NVARCHAR(35) DEFAULT '' NOT NULL,
  [ALG_SystemCreateTimeUtc]									DATETIME DEFAULT GETUTCDATE() NOT NULL,
  [ALG_SystemCreateUser]									VARCHAR(3) DEFAULT '' NOT NULL,
  [ALG_SystemLastEditTimeUtc]								DATETIME DEFAULT GETUTCDATE() NOT NULL,
  [ALG_SystemLastEditUser]									VARCHAR(3) DEFAULT '' NOT NULL,
  INDEX NR_UX__ALG_Product_ALG_Category_ALG_Name			UNIQUE(ALG_Product, ALG_Category, ALG_Name) WITH (ALLOW_PAGE_LOCKS = OFF),
  CONSTRAINT Constraint_ALG_Product							CHECK (ALG_Product = 'CargoWise'),
);

ALTER TABLE [ApplicationLogger] SET (LOCK_ESCALATION = DISABLE);
",
					@"DROP TABLE ApplicationLogger");
			}
		}

		static DatabaseObjectCreateScript ApplicationActiveLogger
		{
			get
			{
				return new DatabaseObjectCreateScript(
					"ApplicationActiveLogger",
					@"
CREATE TABLE dbo.ApplicationActiveLogger(
  [AAL_PK]													UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UC__AAL_PK PRIMARY KEY WITH (ALLOW_PAGE_LOCKS = OFF),
  [AAL_ALG_ApplicationLogger]								UNIQUEIDENTIFIER NOT NULL,
  [AAL_Environment]											NVARCHAR(80) DEFAULT '' NOT NULL,
  [AAL_ActiveUntil]											DATETIMEOFFSET(0),
  [AAL_Status]												CHAR(1) DEFAULT 'U' NOT NULL,
  [AAL_SystemCreateTimeUtc]									DATETIME DEFAULT GETUTCDATE() NOT NULL,
  [AAL_SystemCreateUser]									VARCHAR(3) DEFAULT '' NOT NULL,
  [AAL_SystemLastEditTimeUtc]								DATETIME DEFAULT GETUTCDATE() NOT NULL,
  [AAL_SystemLastEditUser]									VARCHAR(3)  DEFAULT '' NOT NULL,
  INDEX FK_RX__AAL_ALG_ApplicationLogger					(AAL_ALG_ApplicationLogger) WHERE [AAL_ALG_ApplicationLogger] IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF),
  INDEX NR_UX__AAL_ALG_ApplicationLogger_AAL_Environment	UNIQUE(AAL_ALG_ApplicationLogger, AAL_Environment) WITH (ALLOW_PAGE_LOCKS = OFF),
  INDEX NR_RX__AAL_ActiveUntil								(AAL_ActiveUntil) WHERE [AAL_ActiveUntil] IS NOT NULL WITH (ALLOW_PAGE_LOCKS = OFF),
  CONSTRAINT Constraint_AAL_Status							CHECK (AAL_Status IN ('U', 'D'))
);

ALTER TABLE [ApplicationActiveLogger] SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE ApplicationActiveLogger WITH NOCHECK
  ADD CONSTRAINT ApplicationActiveLogger_AAL_ALG_ApplicationLogger_FK2_ApplicationLogger_RRR_120N FOREIGN KEY (AAL_ALG_ApplicationLogger) REFERENCES [ApplicationLogger] (ALG_PK);
",
					@"DROP TABLE ApplicationActiveLogger");
			}
		}

		#endregion

		#endregion

		#region View and Routines
		[SuppressMessage("CargoWiseOne", "CW1119:DoNotUseSLEventTimeTableColumn", Justification = "Baseline")]
		static readonly Lazy<ImmutableArray<DatabaseViewAndRoutineCreateScript>> ViewAndRoutinesCreationScripts = new Lazy<ImmutableArray<DatabaseViewAndRoutineCreateScript>>(() => ImmutableArray.Create(
		#region ViewGenericClientJob

			new DatabaseViewAndRoutineCreateScript("ViewGenericClientJob",
@"CREATE VIEW ViewGenericClientJob 
	WITH SCHEMABINDING
AS
-- Incident etc
SELECT
	IM_PK AS PK,
	'IncidentMain' AS VJ_TableName,
	CASE IM_IncidentType
		WHEN 'WI' THEN 'WI'		-- Work Item
		WHEN 'INC' THEN ''		-- Incident
		WHEN 'PSQ' THEN ''		-- Prof Services Quote
		WHEN 'EI' THEN 'CSE'	-- Incident Enterprise (Old)
		WHEN 'DI' THEN 'CSD'	-- Incident Deliverance (Old)
		WHEN 'PRJ' THEN ''		-- Project
	END + IM_IncidentNumber AS VJ_JobNumber,

	CASE IM_IncidentType
		WHEN 'WI' THEN 'WIT'
		WHEN 'INC' THEN 'INC'
		WHEN 'PSQ' THEN 'PSQ'
		WHEN 'EI' THEN 'CSE'
		WHEN 'DI' THEN 'CSD'
		WHEN 'PRJ' THEN 'PRJ'
	END AS VJ_JobType,
	NULL AS VJ_ETD,
	NULL AS VJ_ETA,
	'' AS VJ_HouseBillNumber,
	'' AS VJ_MasterBillNumber,
	CAST(NULL AS uniqueidentifier) AS CompanyPK,
	CAST (0 AS BIT) AS VJ_IsInactive
	FROM dbo.IncidentMain

UNION

-- Training Course
SELECT
	G2_PK,
	'GlbTrainingCourse',
	G2_BookingNumber,
	'TRS',
	NULL,
	NULL,
	'',
	'',
	CAST(NULL AS uniqueidentifier) AS CompanyPK,
	CAST (0 AS BIT) AS VJ_IsInactive
	FROM dbo.GlbTrainingCourse
", "drop view ViewGenericClientJob", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region vw_SalesRelationNodeWithLastEdit

			new DatabaseViewAndRoutineCreateScript("vw_SalesRelationNodeWithLastEdit",
@"CREATE VIEW vw_SalesRelationNodeWithLastEdit
WITH SCHEMABINDING
AS
SELECT
	ActivityTableCode,
	ActivityID,
	ActivityType,
	CASE ActivityTableCode
		WHEN 'OQ' THEN OQ_SystemLastEditTimeUtc
		WHEN 'G0' THEN Campaign.G0_SystemLastEditTimeUtc
		WHEN 'G8' THEN CampaignItemHeader.G0_SystemLastEditTimeUtc
		WHEN 'O1' THEN O1_SystemLastEditTimeUtc
		WHEN 'P8' THEN P8_SystemLastEditTimeUtc
		WHEN 'TH' THEN TH_SystemLastEditTimeUtc
		WHEN 'VB' THEN COALESCE(JS_SystemLastEditTimeUtc, TH_SystemLastEditTimeUtc)
		WHEN 'WKP' THEN WKP_SystemLastEditTimeUtc
		WHEN 'IM' THEN IM_SystemLastEditTimeUtc
	END AS ActivitySystemLastEditTime,
	SalesRelationTreeID
FROM
	dbo.vw_SalesRelationNode
	LEFT JOIN dbo.OrgSalesCall
		ON ActivityTableCode = 'OQ'
		AND ActivityID = OQ_PK
	LEFT JOIN dbo.GlbCompanyCampaign Campaign
		ON ActivityTableCode = 'G0'
		AND ActivityID = G0_PK
	LEFT JOIN dbo.GlbCompanyCampaignItem
		ON ActivityTableCode = 'G8'
		AND ActivityID = G8_PK
	LEFT JOIN dbo.GlbCompanyCampaign CampaignItemHeader
		ON ActivityTableCode = 'G8'
		AND CampaignItemHeader.G0_PK = G8_G0
	LEFT JOIN dbo.OrgColdCallRegister
		ON ActivityTableCode = 'O1'
		AND ActivityID = O1_PK
	LEFT JOIN dbo.OrgOpportunity
		ON ActivityTableCode = 'P8'
		AND ActivityID = P8_PK
	LEFT JOIN dbo.RatingHeader
		ON ActivityTableCode in ('TH', 'VB')
		AND ActivityID = TH_PK
	LEFT JOIN dbo.JobShipment
		ON ActivityTableCode = 'VB'
		AND ActivityID = JS_PK
	LEFT JOIN dbo.WorkProject
		ON ActivityTableCode = 'WKP'
		AND ActivityID = WKP_PK
	LEFT JOIN dbo.IncidentMain
		ON ActivityTableCode = 'IM'
		AND ActivityID = IM_PK
"
, "drop view vw_SalesRelationNodeWithLastEdit", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region ViewCommissionAgreement

			new DatabaseViewAndRoutineCreateScript("ViewCommissionAgreement",
@"CREATE VIEW ViewCommissionAgreement
WITH SCHEMABINDING
AS
WITH CommissionAgreement (VCA_PK, VCA_GC, VCA_OH_OpportunityClient, VCA_OH_Customer, VCA_CommissionStream, VCA_CommissionBasis, VCA_Status, VCA_CommissionTriggerType, VCA_HasDraft, VCA_LastApprovedDateTimeUtc, VCA_ExpiredDate, VCA_ReversedDateTimeUtc) 
AS
(
	SELECT
		CA0_PK,
		P8_GC,
		P8_OH,
		CA0_OH_Customer,
		CA0_CommissionStream,
		CA0_CommissionBasis,
		P8_Status,
		CA0_CommissionTriggerType,
		CONVERT(BIT, CASE WHEN CA0_PK IN (SELECT CA0_CA0_ParentVersion FROM dbo.OrgCommissionAgreement) THEN 1 ELSE 0 END),
		CA0_LastApprovedDateUtc,
		CA0_ExpiredDate,
		CA0_ReversedDateUtc
	FROM dbo.OrgCommissionAgreement
	JOIN dbo.OrgOpportunity ON CA0_P8 = P8_PK
)
SELECT
	VCA_PK,
	VCA_GC,
	VCA_OH_OpportunityClient,
	VCA_OH_Customer,
	VCA_CommissionStream,
	VCA_CommissionBasis,
	VCA_Status,
	VCA_CommissionTriggerType,
	VCA_HasDraft,
	VCA_LastApprovedDateTimeUtc,
	VCA_ExpiredDate,
	VCA_ReversedDateTimeUtc,
	AH_PostDateMinimum [VCA_EffectiveDate]
FROM CommissionAgreement
LEFT JOIN
(
	SELECT AH_Ledger, AH_TransactionType, AH_OH, MIN(AH_PostDate) AH_PostDateMinimum 
	FROM dbo.AccTransactionHeader
	GROUP BY AH_Ledger, AH_TransactionType, AH_OH 
) a ON AH_Ledger = 'AR' AND AH_TransactionType = 'INV' AND AH_OH = VCA_OH_Customer
WHERE VCA_CommissionTriggerType = '1AR'

UNION ALL

SELECT
	VCA_PK,
	VCA_GC,
	VCA_OH_OpportunityClient,
	VCA_OH_Customer,
	VCA_CommissionStream,
	VCA_CommissionBasis,
	VCA_Status,
	VCA_CommissionTriggerType,
	VCA_HasDraft,
	VCA_LastApprovedDateTimeUtc,
	VCA_ExpiredDate,
	VCA_ReversedDateTimeUtc,
	AL_ReverseDateMinimum [VCA_EffectiveDate]
FROM CommissionAgreement
LEFT JOIN
(
	SELECT AH_Ledger, AH_TransactionType, AH_OH, MIN(AL_ReverseDate) AL_ReverseDateMinimum
	FROM dbo.AccTransactionHeader 
	LEFT JOIN dbo.AccTransactionLines on AL_AH = AH_PK
	GROUP BY AH_Ledger, AH_TransactionType, AH_OH
) a ON AH_Ledger = 'AR' AND AH_TransactionType = 'INV' AND AH_OH = VCA_OH_Customer
WHERE VCA_CommissionTriggerType = 'ERR'

UNION ALL

SELECT
	VCA_PK,
	VCA_GC,
	VCA_OH_OpportunityClient,
	VCA_OH_Customer,
	VCA_CommissionStream,
	VCA_CommissionBasis,
	VCA_Status,
	VCA_CommissionTriggerType,
	VCA_HasDraft,
	VCA_LastApprovedDateTimeUtc,
	VCA_ExpiredDate,
	VCA_ReversedDateTimeUtc,
	OM_CMClientCommenced [VCA_EffectiveDate]
FROM CommissionAgreement
LEFT JOIN dbo.OrgMiscServ ON OM_OH = VCA_OH_Customer
WHERE VCA_CommissionTriggerType = 'CCD'

UNION ALL

SELECT
	VCA_PK,
	VCA_GC,
	VCA_OH_OpportunityClient,
	VCA_OH_Customer,
	VCA_CommissionStream,
	VCA_CommissionBasis,
	VCA_Status,
	VCA_CommissionTriggerType,
	VCA_HasDraft,
	VCA_LastApprovedDateTimeUtc,
	VCA_ExpiredDate,
	VCA_ReversedDateTimeUtc,
	CA0_EffectiveDate [VCA_EffectiveDate]
FROM CommissionAgreement
INNER JOIN dbo.OrgCommissionAgreement ON CA0_PK = VCA_PK
WHERE VCA_CommissionTriggerType = 'MAN'

UNION ALL

SELECT
	VCA_PK,
	VCA_GC,
	VCA_OH_OpportunityClient,
	VCA_OH_Customer,
	VCA_CommissionStream,
	VCA_CommissionBasis,
	VCA_Status,
	VCA_CommissionTriggerType,
	VCA_HasDraft,
	VCA_LastApprovedDateTimeUtc,
	VCA_ExpiredDate,
	VCA_ReversedDateTimeUtc,
	NULL [VCA_EffectiveDate]
FROM CommissionAgreement
WHERE VCA_CommissionTriggerType = 'GLC'

UNION ALL

SELECT
	VCA_PK,
	VCA_GC,
	VCA_OH_OpportunityClient,
	VCA_OH_Customer,
	VCA_CommissionStream,
	VCA_CommissionBasis,
	VCA_Status,
	VCA_CommissionTriggerType,
	VCA_HasDraft,
	VCA_LastApprovedDateTimeUtc,
	VCA_ExpiredDate,
	VCA_ReversedDateTimeUtc,
	AYCEffectiveDate [VCA_EffectiveDate]
FROM CommissionAgreement
LEFT JOIN
(
	SELECT LC_OH,
	ROW_NUMBER() OVER (PARTITION BY LC_OH ORDER BY CASE L8_ChargeCode WHEN registry.PrimaryChargeCode THEN 1 ELSE 2 END, L8_StartDate) AS RowNumber,
	L8_StartDate as AYCEffectiveDate
	FROM dbo.ClientLicenceFee 
	INNER JOIN dbo.LicenceCompany on LC_PK = L8_LC
	CROSS JOIN
	(
		SELECT CONVERT(XML, SD_BinaryValue).value('(/AYCTriggerTypeSettings/PrimaryChargeCode)[1]', 'varchar(10)') as PrimaryChargeCode,
		CONVERT(XML, SD_BinaryValue).value('(/AYCTriggerTypeSettings/SecondaryChargeCode)[1]', 'varchar(10)') as SecondaryChargeCode
		FROM dbo.StmData
		WHERE SD_Name = 'AYCTRIGGERTYPESETTINGS' and SD_BinaryValue IS NOT NULL
	) registry
	WHERE L8_ChargeCode IN (registry.PrimaryChargeCode, registry.SecondaryChargeCode) AND L8_StartDate IS NOT NULL
) a ON a.LC_OH = VCA_OH_Customer AND a.RowNumber = 1
WHERE VCA_CommissionTriggerType = 'AYC'"
, "drop view ViewCommissionAgreement", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region ViewClientProcessHeader

			ViewClientProcessHeaderCreateScript.Create(),

		#endregion

		#region Base 27 Encoding

			new DatabaseViewAndRoutineCreateScript("Base27Encode",
@"CREATE FUNCTION Base27Encode
(
	@Num int
)
RETURNS varchar(7) with schemabinding
AS
BEGIN
	declare @EncodingCharSet char(27) = 'BCDFGHJKMNPQRSTVWXYZ2345679';
	declare @text varchar(7) = '';
	declare @done bit = 0;
	while @done = 0
	begin
		set @text = substring(@EncodingCharSet, (@Num % 27) + 1, 1) + @text;
		set @Num = @Num / 27;
		if @Num = 0
			set @done = 1;
	end;

	return @text;
END
", "DROP FUNCTION Base27Encode", DbRoutineType.SqlFunctionScalarTypeDesc),

			new DatabaseViewAndRoutineCreateScript("Base27Decode",
@"CREATE FUNCTION Base27Decode
(
	@Text varchar(7)
)
RETURNS int with schemabinding
AS
BEGIN
	declare @n int = len(@Text);
	if @n = 0
		return -1;

	declare @EncodingCharSet char(27) = 'BCDFGHJKMNPQRSTVWXYZ2345679';
	declare @i int = 1;
	declare @digit int;
	declare @num int = 0;
	while @i <= @n
	begin
		declare @ch char(1) = substring(@Text, @i, 1);
		set @i = @i + 1
		if (ASCII(@ch) >= ASCII('a') and ASCII(@ch) <= ASCII('z'))
			set @ch = CHAR(ASCII(@ch) + ASCII('A') - ASCII('a'));
		set @digit = CHARINDEX(@ch, @EncodingCharSet) - 1;
		if (@digit = -1)
		begin
			set @num = -1;
			break;
		end;

		set @num = (@num * 27) + @digit
	end;

	RETURN @num;
END
", "DROP FUNCTION Base27Decode", DbRoutineType.SqlFunctionScalarTypeDesc),

		#endregion

		#region ViewLicenceDatabaseSystemId

			new DatabaseIndexedViewCreateScript("ViewLicenceDatabaseSystemId",
@"CREATE VIEW ViewLicenceDatabaseSystemId
WITH SCHEMABINDING
AS
	SELECT LD_PK, LD_SystemId = dbo.Base27Encode(LD_DatabaseNumber)
	FROM dbo.LicenceDatabase WHERE LD_DatabaseNumber != 0;
",
@"CREATE UNIQUE CLUSTERED INDEX NR_UC__ViewLicenceDatabaseSystemId
	ON dbo.ViewLicenceDatabaseSystemId(LD_SystemId) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE INDEX NR_UX__ViewLicenceDatabaseSystemId_LD_PK
	ON dbo.ViewLicenceDatabaseSystemId(LD_PK) WITH (ALLOW_PAGE_LOCKS = OFF);
", "DROP VIEW ViewLicenceDatabaseSystemId", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region ClientStopUnsuccessfulUpgradesSendingSP

			new DatabaseViewAndRoutineCreateScript("ClientStopUnsuccessfulUpgradesSendingSP",
@"CREATE PROCEDURE ClientStopUnsuccessfulUpgradesSendingSP @AckIntervalInMinutes INT = 30, @MaxAttempts INT = 3
AS

SELECT
	SUBSTRING(MI_Subject, 1, 41) AS Package,
	MR_RecipientMailAddress,
	L1_PK,
	GS_EmailAddress,
	OH_Code,
	OH_FullName
INTO #FailedUpgrades
FROM
	dbo.MailDBRecipients
	INNER JOIN dbo.MailDBItems ON MR_MI = MI_PK
	LEFT JOIN dbo.ClientMailDBRecipients on MRX_MR = MR_PK
	LEFT JOIN dbo.UpgradesToClient ON L1_PK = MRX_L1
	LEFT JOIN dbo.GlbStaff ON GS_Code = L1_GS_NKStaffCode
	LEFT JOIN dbo.OrgHeader ON OH_PK = L1_OH
WHERE
	MR_DeliveredTime IS NULL
	AND MI_Status = 'QWA'
	AND MI_Direction = 'TRX'
	AND MI_Subject LIKE N'Package%'
GROUP BY
	SUBSTRING(MI_Subject, 1, 41),
	MR_RecipientMailAddress,
	L1_PK,
	GS_EmailAddress,
	OH_Code,
	OH_FullName
HAVING
	SUM(CASE WHEN MR_AckAttempt >= @MaxAttempts AND MR_LastAttempt < DATEADD(Minute, -@AckIntervalInMinutes, GETDATE()) THEN 1 ELSE 0 END) > 0
	AND SUM(CASE WHEN MR_AckAttempt < @MaxAttempts THEN 1 ELSE 0 END) > 0

UPDATE dbo.MailDBRecipients SET MR_AckAttempt = @MaxAttempts + 1
WHERE MR_PK IN (SELECT MR_PK 
		FROM dbo.MailDBRecipients 
			INNER JOIN dbo.MailDBItems ON MailDBRecipients.MR_MI = MailDBItems.MI_PK
			LEFT JOIN dbo.ClientMailDBRecipients on MRX_MR = MR_PK
			INNER JOIN #FailedUpgrades ON ((MRX_L1 = #FailedUpgrades.L1_PK) OR ((MRX_L1 IS NULL) AND (MailDBRecipients.MR_RecipientMailAddress = #FailedUpgrades.MR_RecipientMailAddress) AND (MailDBItems.MI_Subject LIKE (#FailedUpgrades.Package + '%'))))
		WHERE (MailDBRecipients.MR_AckAttempt <= @MaxAttempts) AND (MailDBRecipients.MR_DeliveredTime IS NULL) AND 
					  (MailDBItems.MI_Status = 'QWA') AND (MailDBItems.MI_Direction = 'TRX'))

DECLARE @UpgradesStopped INT

SELECT @UpgradesStopped = COUNT(*) FROM #FailedUpgrades

IF (@UpgradesStopped > 0)
BEGIN
	DECLARE @UpgradeAddress VARCHAR(128)
	DECLARE @UserAddress VARCHAR(128)
	DECLARE @OrgCode NVARCHAR(12)
	DECLARE @OrgName NVARCHAR(50)

	DECLARE UpgCursor CURSOR FOR SELECT DISTINCT MR_RecipientMailAddress FROM #FailedUpgrades WHERE L1_PK IS NULL
	OPEN UpgCursor
	FETCH NEXT FROM UpgCursor INTO @UpgradeAddress
	WHILE (@@FETCH_STATUS <> -1)
	BEGIN
		SELECT TOP 1 @UserAddress = GS_EmailAddress, @OrgCode = OH_Code, @OrgName = OH_FullName
		FROM dbo.LicenceDatabase 
			INNER JOIN dbo.LicenceEnterprise ON (LD_LE = LE_PK) 
			INNER JOIN dbo.OrgHeader ON (LE_OH = OH_PK) 
			INNER JOIN dbo.StmALog ON (SL_Parent = OH_PK AND SL_SE_NKEvent = 'UPG') 
			INNER JOIN dbo.GlbStaff ON (GS_Code = SL_GS_NKUser) 
			WHERE LD_PublicEmailAddressForUpdate <> '' AND
				(@UpgradeAddress like ('%<' + LD_PublicEmailAddressForUpdate + '>%') OR LD_PublicEmailAddressForUpdate = @UpgradeAddress) 
		ORDER BY SL_EventTime DESC" +  // use SL_EventTime is ok since filtered by SL_Parent
@"		
		IF (@@ROWCOUNT = 1) 
			UPDATE #FailedUpgrades SET GS_EmailAddress = @UserAddress, OH_Code = @OrgCode, OH_FullName = @OrgName
			WHERE (MR_RecipientMailAddress = @UpgradeAddress) AND (L1_PK IS NULL)

		FETCH NEXT FROM UpgCursor INTO @UpgradeAddress
	END
	CLOSE UpgCursor
	DEALLOCATE UpgCursor

	UPDATE dbo.UpgradesToClient SET L1_CurrentStatus = 'FAL' 
	WHERE L1_PK IN (SELECT DISTINCT L1_PK FROM #FailedUpgrades WHERE L1_PK IS NOT NULL)
 
END

SELECT * FROM #FailedUpgrades 

DROP TABLE #FailedUpgrades

RETURN @UpgradesStopped
", "DROP PROCEDURE ClientStopUnsuccessfulUpgradesSendingSP", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region vw_Report_3rdPartySoftwareReport

			new DatabaseViewAndRoutineCreateScript("vw_Report_3rdPartySoftwareReport",
@"CREATE VIEW vw_Report_3rdPartySoftwareReport
AS
SELECT 
OP_PartNum As ProductCode,
OP_Desc As ProductDesc,
L3_LicenceIssued As LicenceIssuedDate,
L3_OH_Supplier As SupplierPK,
SupplierOrg.OH_Code As SupplierCode,
BuyerOrg.OH_Code As BuyerCode,
L3_LicenceCount As LicenceCount

FROM dbo.Licence3rdPartySoftware
LEFT OUTER JOIN dbo.OrgHeader SupplierOrg ON SupplierOrg.OH_PK = L3_OH_Supplier
LEFT OUTER JOIN dbo.OrgSupplierPart ON OP_PK = L3_OP_ProductSKU
LEFT OUTER JOIN dbo.LicenceCompany ON LC_PK = L3_LC_LicenceCompany
LEFT OUTER JOIN dbo.OrgHeader BuyerOrg ON BuyerOrg.OH_PK = LC_OH
", "DROP VIEW vw_Report_3rdPartySoftwareReport", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region vw_Report_BookedTrainingHours

			new DatabaseViewAndRoutineCreateScript("vw_Report_BookedTrainingHours",
@"CREATE VIEW vw_Report_BookedTrainingHours AS
SELECT
	GS_Fullname AS TrainerName,
	OH_Fullname AS ClientName,
	GS_GB_HomeBranch AS HomeBranch,
	GZ_SessionStartTime AS StartTime,
	GZ_SessionEndTime AS EndTime,
	G2_RX_NKRateCurrency AS Currency,
	GS_Fullname+G2_RX_NKRateCurrency AS TrainerNameCurrency,
	DATEDIFF(Minute, GZ_SessionStartTime, GZ_SessionEndTime)/60.00 AS BookedHours,
	G2_HourlyRate*DATEDIFF(Minute, GZ_SessionStartTime, GZ_SessionEndTime)/60.00 AS EstRevenue,
	GS_PK AS TrainerPK,
	OH_PK AS OrgPK

FROM dbo.GlbClassroomSession
INNER JOIN dbo.GlbStaff ON GlbClassroomSession.GZ_GS_NKCoordinatorOfSession = GlbStaff.GS_Code 
INNER JOIN dbo.OrgAddress ON GlbClassroomSession.GZ_OA_SessionDeliveryLocation = OrgAddress.OA_PK
INNER JOIN dbo.OrgHeader ON OrgAddress.OA_OH = OrgHeader.OH_PK
INNER JOIN dbo.GlbTrainingCourse ON GlbClassroomSession.GZ_G2_Course = GlbTrainingCourse.G2_PK",
						"drop view vw_Report_BookedTrainingHours",
						DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region vw_Report_TrainingSessionEstimatedVsActualTimes

			new DatabaseViewAndRoutineCreateScript("vw_Report_TrainingSessionEstimatedVsActualTimes",
@"CREATE VIEW vw_Report_TrainingSessionEstimatedVsActualTimes
AS
SELECT
	GZ_PK,
	G2_BookingNumber AS BookingNumber,
	CoursePrimaryTrainer.GS_PK AS PrimaryTrainerPK,
	G2_InitialStartDate AS InitialStartDate,
	OH_PK AS CompanyPK,
	OH_Code AS CompanyCode, 
	OH_FullName AS CompanyFullName,
	MainTrainer.GS_GB_HomeBranch AS HomeBranch,

	MainTrainer.GS_PK as SessionCoordinatorPK,
	MainTrainer.GS_Code as SessionCoordinatorInitials,
	MainTrainer.GS_FullName as SessionCoordinator,
	MainTrainer.GS_FullName+OH_FullName as TrainerNameCompanyName,
	MainTrainer.GS_FullName+'Total' as TrainerNameTotal,
	NULL AS SessionMainTrainerPK,
	'N' as IsAdditionalTrainer,

	GZ_G3_TrainingSubject AS SubjectID,
	G3_SubjectName AS SubjectName,
	G2_TemplateName AS ScheduleDescription,
	GZ_InvoiceRateType AS RateType,
	GZ_SessionStartTime AS SessionStartTime, 
	GZ_SessionEndTime AS SessionEndTime,
	CAST(DATEDIFF(mi, GZ_SessionStartTime, GZ_SessionEndTime) AS DECIMAL)/60 AS EstimatedHours,
	ActualHours = ISNULL(
		CASE GZ_IsCancelled
			WHEN 'Y' THEN 0
			WHEN 'N' THEN CONVERT(DECIMAL(10,2), CAST(DATEDIFF(mi, GZ_ActualStartTime, GZ_ActualEndTime)AS DECIMAL(10,2))/60) 
		END, 0),
	ActualStartTime = ISNULL(
		CASE GZ_IsCancelled
			WHEN 'Y' THEN ''
			WHEN 'N' THEN GZ_ActualStartTime
		END, 0),
	ActualEndTime = ISNULL(
		CASE GZ_IsCancelled
			WHEN 'Y' THEN ''
			WHEN 'N' THEN GZ_ActualEndTime
		END, 0),
	EstimatedAndActualHoursDiff = ISNULL(
		CASE GZ_IsCancelled 
			WHEN 'Y' THEN 0
			WHEN 'N' THEN ISNULL(CAST(DATEDIFF(mi, GZ_ActualStartTime, GZ_ActualEndTime) AS DECIMAL(10,2))/60,0) - ISNULL(CAST(DATEDIFF(mi, GZ_SessionStartTime, GZ_SessionEndTime) AS DECIMAL(10,2))/60,0)
		END, 0),
	GZ_IsCancelled AS IsCancelled

FROM 
	dbo.GlbClassroomSession
	INNER JOIN dbo.GLbTrainingCourse on GZ_G2_Course = G2_PK
	INNER JOIN dbo.GlbStaff MainTrainer on GZ_GS_NKCoordinatorOfSession = MainTrainer.GS_Code
	INNER JOIN dbo.GlbStaff CoursePrimaryTrainer on G2_GS_NKPrimeTrainer = CoursePrimaryTrainer.GS_Code
	INNER JOIN dbo.GlbClassroomSubject ON GZ_G3_TrainingSubject = G3_PK
	INNER JOIN dbo.OrgAddress ON G2_OA_TrainingAddress = OA_PK
	INNER JOIN dbo.OrgHeader ON OA_OH = OH_PK


UNION ALL


SELECT
	GZ_PK,
	G2_BookingNumber AS BookingNumber, 
	CoursePrimaryTrainer.GS_PK AS PrimaryTrainer,
	G2_InitialStartDate AS InitialStartDate,
	OH_PK AS CompanyPK,
	OH_Code AS CompanyCode, 
	OH_FullName AS CompanyFullName,
	AdditionalTrainer.GS_GB_HomeBranch AS HomeBranch,

	AdditionalTrainer.GS_PK as SessionCoordinatorPK,
	AdditionalTrainer.GS_Code as SessionCoordinatorInitials,
	AdditionalTrainer.GS_FullName as SessionCoordinator,
	AdditionalTrainer.GS_FullName+OH_FullName as TrainerNameCompanyName,
	AdditionalTrainer.GS_FullName+'Total' as TrainerNameTotal,

	SessionCoordinator.GS_PK AS SessionMainTrainerPK,
	'Y' as IsAdditionalTrainer,

	GZ_G3_TrainingSubject AS SubjectID,
	G3_SubjectName AS SubjectName,
	G2_TemplateName AS ScheduleDescription,
	GZ_InvoiceRateType AS RateType,
	GZ_SessionStartTime AS SessionStartTime, 
	GZ_SessionEndTime AS SessionEndTime,
	CAST(DATEDIFF(mi, GZ_SessionStartTime, GZ_SessionEndTime) AS DECIMAL)/60 AS EstimatedHours,
	ActualHours = ISNULL(
		CASE GZ_IsCancelled
			WHEN 'Y' THEN 0
			WHEN 'N' THEN CONVERT(DECIMAL(10,2), CAST(DATEDIFF(mi, GZ_ActualStartTime, GZ_ActualEndTime)AS DECIMAL(10,2))/60) 
		END, 0),
	ActualStartTime = ISNULL(
		CASE GZ_IsCancelled
			WHEN 'Y' THEN ''
			WHEN 'N' THEN GZ_ActualStartTime
		END, 0),
	ActualEndTime = ISNULL(
		CASE GZ_IsCancelled
			WHEN 'Y' THEN ''
			WHEN 'N' THEN GZ_ActualEndTime
		END, 0),
	EstimatedAndActualHoursDiff = ISNULL(
		CASE GZ_IsCancelled 
			WHEN 'Y' THEN 0
			WHEN 'N' THEN ISNULL(CAST(DATEDIFF(mi, GZ_ActualStartTime, GZ_ActualEndTime) AS DECIMAL(10,2))/60,0) - ISNULL(CAST(DATEDIFF(mi, GZ_SessionStartTime, GZ_SessionEndTime) AS DECIMAL(10,2))/60,0)
		END, 0),
	GZ_IsCancelled AS IsCancelled

FROM 
	dbo.GlbClassroomSession
	INNER JOIN dbo.GLbTrainingCourse on GZ_G2_Course = G2_PK
	INNER JOIN dbo.GlbClassroomAttendee on GX_GZ_ClassroomSession = GZ_PK
	INNER JOIN dbo.GlbStaff AdditionalTrainer on GX_GS_NKStaffMember = AdditionalTrainer.GS_Code and AdditionalTrainer.GS_IsResource = 0
	INNER JOIN dbo.GlbStaff CoursePrimaryTrainer on G2_GS_NKPrimeTrainer = CoursePrimaryTrainer.GS_Code
	INNER JOIN dbo.GlbClassroomSubject ON GZ_G3_TrainingSubject = G3_PK
	INNER JOIN dbo.OrgAddress ON G2_OA_TrainingAddress = OA_PK
	INNER JOIN dbo.OrgHeader ON OA_OH = OH_PK
	INNER JOIN dbo.GlbStaff SessionCoordinator on GZ_GS_NKCoordinatorOfSession = SessionCoordinator.GS_Code
", "drop view vw_Report_TrainingSessionEstimatedVsActualTimes", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region GetCommissionAgreementPrimaryKeys

			new DatabaseViewAndRoutineCreateScript("GetCommissionAgreementPrimaryKeys",
@"CREATE FUNCTION GetCommissionAgreementPrimaryKeys
(
	@OpportunityClientPk UNIQUEIDENTIFIER,
	@CustomerPk UNIQUEIDENTIFIER,
	@CommissionStream VARCHAR(3),
	@CommissionDate DATE,
	@EffectiveDateCacheTable dbo.TVP_TriggerTypeEffectiveDate READONLY
)
RETURNS TABLE WITH SCHEMABINDING

AS
RETURN
	WITH CommissionAgreement (VCA_PK, VCA_CommissionTriggerType, VCA_OH_Customer)
	AS
	(
		SELECT 
			CA0_PK,
			CA0_CommissionTriggerType,
			CA0_OH_Customer
		FROM dbo.OrgCommissionAgreement
		JOIN dbo.OrgOpportunity ON CA0_P8 = P8_PK
		WHERE P8_OH = ISNULL(@OpportunityClientPk, P8_OH) AND 
			CA0_OH_Customer = ISNULL(@CustomerPk, CA0_OH_Customer) AND
			CA0_CommissionStream = ISNULL(@CommissionStream, CA0_CommissionStream) AND
			(
				CA0_ExpiredDate > @CommissionDate OR CA0_ExpiredDate IS NULL
			) AND
			CA0_ReversedDateUtc IS NULL AND
			CA0_LastApprovedDateUtc IS NOT NULL
	)
	SELECT VCA_PK
	FROM CommissionAgreement
	JOIN @EffectiveDateCacheTable ON TriggerType = VCA_CommissionTriggerType AND CustomerPk = VCA_OH_Customer
	WHERE VCA_CommissionTriggerType IN ('ERR', '1AR') AND EffectiveDate <= @CommissionDate
	UNION ALL
	SELECT VCA_PK
	FROM CommissionAgreement
	JOIN dbo.OrgMiscServ ON OM_OH = VCA_OH_Customer
	WHERE VCA_CommissionTriggerType = 'CCD' AND OM_CMClientCommenced <= @CommissionDate
	UNION ALL
	SELECT VCA_PK
	FROM CommissionAgreement
	JOIN dbo.OrgCommissionAgreement ON CA0_PK = VCA_PK
	WHERE VCA_CommissionTriggerType = 'MAN' AND CA0_EffectiveDate <= @CommissionDate
	UNION ALL
	SELECT VCA_PK
	FROM CommissionAgreement
	LEFT JOIN
	(
		SELECT LC_OH,
		ROW_NUMBER() OVER (PARTITION BY LC_OH ORDER BY CASE L8_ChargeCode WHEN registry.PrimaryChargeCode THEN 1 ELSE 2 END, L8_StartDate) AS RowNumber,
		L8_StartDate as AYCEffectiveDate
		FROM dbo.ClientLicenceFee 
		INNER JOIN dbo.LicenceCompany on LC_PK = L8_LC
		CROSS JOIN
		(
			SELECT CONVERT(XML, SD_BinaryValue).value('(/AYCTriggerTypeSettings/PrimaryChargeCode)[1]', 'varchar(10)') as PrimaryChargeCode,
			CONVERT(XML, SD_BinaryValue).value('(/AYCTriggerTypeSettings/SecondaryChargeCode)[1]', 'varchar(10)') as SecondaryChargeCode
			FROM dbo.StmData
			WHERE SD_Name = 'AYCTRIGGERTYPESETTINGS' and SD_BinaryValue IS NOT NULL
		) registry
		WHERE L8_ChargeCode IN (registry.PrimaryChargeCode, registry.SecondaryChargeCode) AND L8_StartDate IS NOT NULL
	) a ON a.LC_OH = VCA_OH_Customer AND a.RowNumber = 1
	WHERE VCA_CommissionTriggerType = 'AYC' AND AYCEffectiveDate <= @CommissionDate
",
	"DROP FUNCTION GetCommissionAgreementPrimaryKeys", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region csfn_TrainingSessionAdditionalStaff

			new DatabaseViewAndRoutineCreateScript("csfn_TrainingSessionAdditionalStaff",
@"CREATE FUNCTION csfn_TrainingSessionAdditionalStaff  
(  
	@SessionPK UNIQUEIDENTIFIER  
)  
RETURNS VARCHAR (8000)  
BEGIN  
	DECLARE @additionalStaff varchar(8000)  
	SET @additionalStaff = ''  
	SELECT  
		@additionalStaff =   
			CASE  
				WHEN @additionalStaff = '' THEN rtrim(GS_Code)  
				ELSE @additionalStaff + ', ' + rtrim(GS_Code)  
			END  
	FROM   
		dbo.GlbClassroomSession  
		INNER JOIN dbo.GlbClassroomAttendee ON GX_GZ_ClassroomSession = GZ_PK  
		INNER JOIN dbo.GlbStaff ON GX_GS_NKStaffMember = GS_Code AND GS_IsResource = 0
	WHERE  
		GZ_PK = @SessionPK  
 RETURN @additionalStaff  
END
", "drop function csfn_TrainingSessionAdditionalStaff", DbRoutineType.SqlFunctionScalarTypeDesc),

		#endregion

		#region Report_TrainingSchedules

			new DatabaseViewAndRoutineCreateScript("Report_TrainingSchedules",
@"CREATE FUNCTION Report_TrainingSchedules
(
	@AdditionalResources CHAR (1)
)
RETURNS TABLE 
AS
RETURN
	SELECT
		BookingNumber
		,PrimaryTrainer.GS_Code AS PrimaryTrainerInitials
		,ScheduleDescription
		,CompanyPK AS ClientPK
		,CompanyCode AS ClientCode
		,CompanyFullName AS ClientName
		,CASE 
			WHEN CompanyFullName LIKE 'CargoWise%' THEN 'Internal'
			ELSE 'External'
		END AS InternalExternal
		,SubjectID
		,SubjectName
		,SessionStartTime
		,SessionEndTime
		,EstimatedHours
		,SessionCoordinatorInitials AS TrainerInitials
		,SessionCoordinator AS TrainerFullName
		,CASE 
			WHEN IsAdditionalTrainer = 'Y' THEN 'AdditRes' 
			ELSE 'Trainer'	
		END AS TrainersRoleOnSession
		,CASE 
			WHEN IsAdditionalTrainer = 'Y' THEN SessionTrainer.GS_Code
			ELSE SessionCoordinatorInitials
		END AS SessionTrainerInitials
		,CASE 
			WHEN IsAdditionalTrainer = 'Y' THEN SessionTrainer.GS_FullName
			ELSE SessionCoordinator
		END AS SessionTrainerFullName
		,GB_PK AS SessionTrainersHomeBranchPK
		,GB_Code AS SessionTrainersHomeBranch
		,ActualStartTime
		,ActualEndTime
		,ActualHours
		,EstimatedAndActualHoursDiff
		,dbo.csfn_TrainingSessionAdditionalStaff(GZ_PK) AS AdditionalStaff
		,RateType
	FROM 
		dbo.vw_Report_TrainingSessionEstimatedVsActualTimes
		INNER JOIN dbo.GlbStaff AS PrimaryTrainer ON PrimaryTrainerPK = PrimaryTrainer.GS_PK
		LEFT JOIN dbo.GlbStaff AS SessionTrainer 
			ON 
			SessionTrainer.GS_PK = SessionMainTrainerPK 
			AND IsAdditionalTrainer = 'Y'
		LEFT JOIN dbo.GlbBranch ON GB_PK = CASE WHEN IsAdditionalTrainer = 'Y' THEN SessionTrainer.GS_GB_HomeBranch ELSE HomeBranch END
	WHERE
		(@AdditionalResources = 'Y' OR IsAdditionalTrainer = 'N')
		AND
		IsCancelled = 'N'
", "DROP FUNCTION Report_TrainingSchedules", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ReportExpiringLicences

			new DatabaseViewAndRoutineCreateScript("ReportExpiringLicences",
@"CREATE PROC ReportExpiringLicences
	@LicenceType varchar(3), @WhenItExpires varchar(3), @ExpiryDate datetime, @CompanyPK uniqueidentifier, @SalesRep uniqueidentifier
AS
BEGIN
	-- Base SQL
	declare @SQL varchar(2000)
	set @SQL = 
		'SELECT OH_Code as OrgCode, 
			OH_FullName as OrgName, 
			GS_FullName as StaffRep,
			LM_LicenceType as LicenceType,
			LM_GroupModuleCode as ModuleCode, 
			LM_UserCount as UserCount, 
			CASE
				WHEN LM_ExpiryDate IS NULL THEN LA_ContractExpiryDate
				ELSE LM_ExpiryDate 
			END as ExpiryDate
		FROM	dbo.LicenceModules 
			JOIN dbo.LicenceHeader on LM_LA = LA_PK 
			JOIN dbo.LicenceDatabase ON LA_LD = LD_PK
			JOIN dbo.LicenceCompany on LA_LC = LC_PK
			JOIN dbo.OrgHeader on LC_OH = OH_PK
			JOIN dbo.GlbStaff on GS_Code = dbo.csfn_GetAssignedStaff(OH_PK, ''ALL'', ''SAL'', ''' + CAST(@CompanyPK as varchar(40)) + ''') '			
	
	IF (@SalesRep IS NOT NULL)
	BEGIN
		set @SQL = @SQL + 'AND GS_PK = ''' + CAST(@SalesRep as varchar(40)) + ''' '
	END

	-- Filter By Licence Type
	set @SQL = @SQL +
	CASE @LicenceType
		WHEN 'ALL' THEN 'AND LM_LicenceType IN (''PUR'', ''TRI'', ''REN'') '
		WHEN 'PRN' THEN 'AND LM_LicenceType IN (''PUR'', ''REN'') '
		WHEN 'PUR' THEN 'AND LM_LicenceType = ''PUR'' '
		WHEN 'TRI' THEN 'AND LM_LicenceType = ''TRI'' '
		WHEN 'REN' THEN 'AND LM_LicenceType = ''REN'' '
		ELSE ' '
	END
	
	declare @ExpirySQLON varchar(2000)
	set @ExpirySQLON =
		'AND (
			(LM_LicenceType = ''TRI'' AND LM_ExpiryDate = ''' + CAST(@ExpiryDate as varchar) + ''')
			OR (LM_LicenceType = ''PUR'' AND LA_ContractExpiryDate = ''' + CAST(@ExpiryDate as varchar) + ''') 
			OR (LM_LicenceType = ''REN'' AND LM_ExpiryDate != '''' AND LM_ExpiryDate = ''' + CAST(@ExpiryDate as varchar) + ''') 
			OR (LM_LicenceType = ''REN'' AND LM_ExpiryDate = '''' AND LA_ContractExpiryDate = ''' + CAST(@ExpiryDate as varchar) + ''')
		) '

	declare @ExpirySQLBEF varchar(2000)
	set @ExpirySQLBEF =
		'AND (
			(LM_LicenceType = ''TRI''    AND LM_ExpiryDate > GETDATE() AND LM_ExpiryDate <= ''' + CAST(@ExpiryDate as varchar) + ''') 
			OR (LM_LicenceType = ''PUR'' AND LA_ContractExpiryDate > GETDATE() AND LA_ContractExpiryDate <= ''' + CAST(@ExpiryDate as varchar) + ''') 
			OR (LM_LicenceType = ''REN'' AND LM_ExpiryDate != '''' AND LM_ExpiryDate > GETDATE() AND LM_ExpiryDate <= ''' + CAST(@ExpiryDate as varchar) + ''')
			OR (LM_LicenceType = ''REN'' AND LM_ExpiryDate = '''' AND LA_ContractExpiryDate > GETDATE() AND LA_ContractExpiryDate <= ''' + CAST(@ExpiryDate as varchar) + ''')
		) '

	declare @ExpirySQLAFT varchar(2000)
	set @ExpirySQLAFT =
		'AND (
			(LM_LicenceType = ''TRI'' AND LM_ExpiryDate > ''' + CAST(@ExpiryDate as varchar) + ''') 
			OR (LM_LicenceType = ''PUR'' AND LA_ContractExpiryDate > ''' + CAST(@ExpiryDate as varchar) + ''') 
			OR (LM_LicenceType = ''REN'' AND LM_ExpiryDate != '''' AND LM_ExpiryDate > ''' + CAST(@ExpiryDate as varchar) + ''') 
			OR (LM_LicenceType = ''REN'' AND LM_ExpiryDate = '''' AND LA_ContractExpiryDate > ''' + CAST(@ExpiryDate as varchar) + ''')
		) '

	-- Filter By Expiry Date
	set @SQL = @SQL +
	CASE @WhenItExpires
		WHEN 'ON'  THEN @ExpirySQLON
		WHEN 'BEF' THEN @ExpirySQLBEF
		WHEN 'AFT' THEN @ExpirySQLAFT
		ELSE ' '
	END

	set @SQL = @SQL + 'ORDER BY OrgCode, ModuleCode, ExpiryDate'
	exec(@SQL)
END
", "DROP PROCEDURE ReportExpiringLicences", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region Clientfn_GetHoursFromDuration

			new DatabaseViewAndRoutineCreateScript("Clientfn_GetHoursFromDuration",
@"CREATE function Clientfn_GetHoursFromDuration (@dateTime datetime)" +
@" returns decimal (6, 2) as 
begin 
declare @result decimal (6, 2) 
set @result = 0
if @DateTime is not null begin
set @result = (datepart(dayofyear, @DateTime)-1) * 24 + datepart(hour, @DateTime) + (cast(datePart(minute, @DateTime) as decimal) / 60) 
end 
return(@Result) 
end",
						"drop function Clientfn_GetHoursFromDuration",
						DbRoutineType.SqlFunctionScalarTypeDesc),

		#endregion

		#region LicenceProfileReport

			new DatabaseViewAndRoutineCreateScript("LicenceProfileReport", @"
CREATE PROC LicenceProfileReport
   @LicenceType varchar(3),
   @Module varchar(3),
   @ModuleNo varchar(3),
   @ModuleTrial varchar(3),
   @Display varchar(3),
   @Enterprise varchar(3),
   @Organisations varchar(4000), 
   @Country varchar(2),
   @OrgPort varchar(5),
   @ExpiryFrom smalldatetime,
	@ExpiryEnds smalldatetime,
	@DBType varchar(3)
AS
BEGIN
SET NOCOUNT ON
IF @Enterprise IS NULL
   BEGIN
   SET @Enterprise = ''
   END
IF @Module IS NULL
   BEGIN
   SET @Module = ''
   END
IF @ModuleNo IS NULL
   BEGIN
   SET @ModuleNo = ''
   END
IF @ModuleTrial IS NULL
   BEGIN
   SET @ModuleTrial = ''
   END
DECLARE @ModuleCode_cursor CURSOR, @ModuleCode varchar(12), @SQLText nvarchar(max)
SET @SQLText = ''
SET @ModuleCode_cursor = CURSOR FOR SELECT DISTINCT LM_GroupModuleCode FROM dbo.LicenceModules ORDER BY LM_GroupModuleCode

OPEN @ModuleCode_cursor
FETCH NEXT FROM @ModuleCode_cursor INTO @ModuleCode
WHILE (@@FETCH_STATUS = 0)
BEGIN
	  IF LEN(@SQLText) > 0  SET @SQLText = @SQLText + ', '
		 SET @SQLText = @SQLText + '[' + @ModuleCode + '] varchar(10)'
		 FETCH NEXT FROM @ModuleCode_cursor INTO @ModuleCode
END

SET @SQLText = 'CREATE TABLE ##m (LA uniqueidentifier, LMode varchar(3) Collate database_default, LUsers smallint, 
				TExpiry smalldatetime, LSelect1 char(1), LSelect2 char(1), LSelect3 char(1), ' + @SQLText + ')'
EXEC sp_executesql @SQLText

CLOSE @ModuleCode_cursor

INSERT INTO ##m (LA, LMode, LUsers) SELECT Distinct
		LM_LA,
		LM_LicenceType,
		LM_UserCount
	FROM dbo.LicenceModules
	WHERE @Display = 'ALL' OR (@Display = 'LIC' AND LM_LicenceType != 'NON')

-- First Pass
OPEN @ModuleCode_cursor
FETCH NEXT FROM @ModuleCode_cursor INTO @ModuleCode

WHILE (@@FETCH_STATUS = 0)
BEGIN
	  SET @SQLText = 'UPDATE ##m SET [' + @ModuleCode + '] = '''''
	  EXEC sp_executesql @SQLText
	  SET @SQLText = 'UPDATE ##m SET [' + @ModuleCode + '] = CASE LM_LicenceType 
					WHEN ''PUR'' THEN ''P''
					WHEN ''REN'' THEN ''R''
					WHEN ''TRI'' THEN ''T''
					WHEN ''NON'' THEN ''N''
					ELSE '''' END,
			 TExpiry = LM_ExpiryDate,
			 LSelect1 = ''N'',
			 LSelect2 = ''N'',
			 LSelect3 = ''N''
			FROM ##m 
			JOIN dbo.LicenceModules ON LA = LM_LA AND LMode = LM_LicenceType AND LUsers = LM_UserCount
			WHERE LM_GroupModuleCode = ''' + @ModuleCode + ''''
	  EXEC sp_executesql @SQLText
	
  FETCH NEXT FROM @ModuleCode_cursor INTO @ModuleCode
END
					  
CLOSE @ModuleCode_cursor 

OPEN @ModuleCode_cursor
FETCH NEXT FROM @ModuleCode_cursor INTO @ModuleCode

WHILE (@@FETCH_STATUS = 0)
BEGIN
	   SET @SQLText = 'UPDATE ##m SET 
			 LSelect1 = CASE WHEN (''' + @ModuleCode + ''' = ''' + @Module + ''' AND
					(LM_LicenceType = ''PUR'' OR LM_LicenceType = ''REN'')) THEN ''Y'' ELSE LSelect1 END,
			 LSelect2 = CASE WHEN (''' + @ModuleCode + ''' = ''' + @ModuleNo + ''' AND
					LM_LicenceType = ''NON'') THEN ''Y'' ELSE LSelect2  END,
			 LSelect3 = CASE WHEN (''' + @ModuleCode + ''' = ''' + @ModuleTrial + ''' AND
					LM_LicenceType = ''TRI'') THEN ''Y'' ELSE LSelect3  END
			FROM ##m 
			JOIN dbo.LicenceModules ON LA = LM_LA 
			WHERE LM_GroupModuleCode = ''' + @ModuleCode + ''''
	  EXEC sp_executesql @SQLText
	
  FETCH NEXT FROM @ModuleCode_cursor INTO @ModuleCode
END 
					  
CLOSE @ModuleCode_cursor 

DEALLOCATE @ModuleCode_cursor

SELECT 
  LE_EnterpriseCode As EntCode,
  LC_CompanyCode AS Company,
  LC_CompanyCountry As Country,
  OH_PK As OrgPK,
  OH_Code As OrgCode,
  OH_FullName As OrgName,
  OH_RL_NKClosestPort As OrgPort,
  LD_ServerCode As Server, 
  LD_DBServerSecurityMode As DBMode, 
  LA_SiteLiveDate As LiveDate,
  LA_SupportStartDate As SupportExpiry,
  @OrgPort As SelectedPort,
  OM_CMOverallClientRelation As OverallClientRelation,
  GS_FullName As SalesReptName,
  GS_Code As SalesReptCode,
  ##m.*

FROM
  dbo.LicenceHeader
  LEFT JOIN dbo.LicenceCompany On LC_PK = LA_LC
  LEFT JOIN dbo.LicenceEnterprise On LE_PK = LC_LE
  LEFT JOIN dbo.OrgHeader On OH_PK = LC_OH 
  LEFT JOIN dbo.LicenceDatabase On LD_PK = LA_LD
  LEFT JOIN dbo.OrgMiscServ On OH_PK = OM_OH
  LEFT JOIN (select * from dbo.OrgStaffAssignments where O8_Role = 'SAL') a On O8_OH = OM_OH
  LEFT JOIN dbo.GlbStaff On GS_Code = O8_GS_NKPersonResponsible
  JOIN ##m On LA = LA_PK

WHERE
  ((@Module = '' OR ##m.LSelect1 = 'Y')) AND  
  ((@ModuleNo = '' OR ##m.LSelect2 = 'Y')) AND --@LicenceType != 'TRI') AND
  (@ModuleTrial = '' OR ##m.LSelect3 = 'Y') AND
  (@LicenceType = 'ALL' OR @LicenceType = ##m.LMode OR 
		@LicenceType = 'P&R' AND (##m.LMode='PUR' OR ##m.LMode='REN')) AND
  (@Enterprise = '' OR @Enterprise = LE_EnterpriseCode) AND 
  (@Organisations = '' OR charindex(CAST(OrgHeader.OH_Code AS VARCHAR(50)),@Organisations) > 0) AND
  (OrgHeader.OH_IsActive = 1) AND
  (@Country is null OR @Country = '' OR @Country = LC_CompanyCountry) AND
  (@OrgPort is null OR @OrgPort = '' OR @OrgPort = OH_RL_NKClosestPort) AND
  (@ExpiryFrom = '' OR TExpiry = '' OR TExpiry between @ExpiryFrom and @ExpiryEnds) AND
  (@DBType = 'ALL' OR @DBType = LD_ServerCode)
  
 
ORDER BY
  OH_Code, LC_CompanyCode, LD_ServerCode, LD_DBServerSecurityMode, LMode, LUsers

DROP TABLE ##m

SET NOCOUNT OFF
END
", "drop procedure LicenceProfileReport", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region Clientfn_WorkingDaysBetween

			new DatabaseViewAndRoutineCreateScript("Clientfn_WorkingDaysBetween", @"CREATE FUNCTION Clientfn_WorkingDaysBetween 
	(
			@StartDay datetime, 
			@EndDay datetime,
			@GH_ParentID UNIQUEIDENTIFIER,
			@GA_GS UNIQUEIDENTIFIER			
	)
RETURNS int
AS
BEGIN 
	DECLARE 	
		@date1 datetime, 	
		@date2 datetime, 	
		@tempdate datetime, 	
		@day int, 	
		@count int 	
		
	SET @date1 = CONVERT(datetime ,(CONVERT(char(10),@StartDay,102)),102 ) 	        
	SET @date2 = CONVERT(datetime ,(CONVERT(char(10),@EndDay,102)),102 )         	
	SET @tempdate = @date1 
	SET @count = 0 
	WHILE ( datediff(dd,@tempdate,@date2) >= 0) 	
	BEGIN 		
		SET @day = Datepart(dw,@tempdate) 		        
		IF (@day != 1 AND @day != 7) 		        
			IF NOT EXISTS
			(
				SELECT * 
				FROM dbo.GLBholiday 
				WHERE 
					GH_Date = @tempdate 
					AND GH_ParentTableCode = 'GB'
					AND GH_ParentID = @GH_ParentID
			) 
			AND NOT EXISTS
			(
				SELECT * 
				FROM dbo.GLBStaffHoliday 
				WHERE 
					GA_GS = @GA_GS 
					AND GA_WorkHolidayType <> 'TRN'		/* Ignore Training Session */
					AND (@TempDate BETWEEN GA_StartTime AND GA_EndTime OR @TempDate = CONVERT(datetime,(CONVERT(char(10),GA_StartTime,102)),102))
			)		/* If it is not a Public Holiday or a staff members holiday*/			
			SELECT @count = @count + 1 		 			        					
		SET @tempdate = Dateadd(dd,1,@tempdate) 		
	END 
	RETURN @count
END", "drop FUNCTION Clientfn_WorkingDaysBetween", DbRoutineType.SqlFunctionScalarTypeDesc),

		#endregion

		#region Clientvw_GetDate

			new DatabaseViewAndRoutineCreateScript("Clientvw_GetDate", @"create view Clientvw_GetDate as select Getdate() as Today", "drop view Clientvw_GetDate", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region ClientReport_GetAllProductsFromRegistry

			new DatabaseViewAndRoutineCreateScript("ClientReport_GetAllProductsFromRegistry",
@"CREATE FUNCTION ClientReport_GetAllProductsFromRegistry()
RETURNS @ProductsTable TABLE 
(
	ProductCode VARCHAR(3),
	ProductDescription VARCHAR(100)
)
AS
BEGIN

DECLARE @ProductsXml XML = (SELECT CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'SystemProductMappings');

INSERT INTO @ProductsTable
SELECT 
	ProductNode.Item.value('Code[1]', 'char(3)') AS ProductCode,
	ProductNode.Item.value('Description[1]', 'varchar(100)') AS ProductDescription
FROM
	@ProductsXml.nodes('ArrayOfSystemProduct/SystemProduct') as ProductNode(Item)

IF NOT EXISTS (SELECT 1 FROM @ProductsTable WHERE ProductCode = 'HUB')
BEGIN
	INSERT INTO @ProductsTable VALUES ('HUB', NULL)
END

IF NOT EXISTS (SELECT 1 FROM @ProductsTable WHERE ProductCode = 'ENT')
BEGIN
	INSERT INTO @ProductsTable VALUES ('ENT', '" + BrandingFactory.Instance.ProductName + @"')
END

RETURN
END", "DROP FUNCTION ClientReport_GetAllProductsFromRegistry", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region ClientReport_GetAllEnterpriseProductAreasFromRegistry

			new DatabaseViewAndRoutineCreateScript("ClientReport_GetAllEnterpriseProductAreasFromRegistry",
@"CREATE FUNCTION ClientReport_GetAllEnterpriseProductAreasFromRegistry()
RETURNS @ProductAreaTable TABLE 
(
	ProductAreaCode VARCHAR(3),
	ProductAreaDescription VARCHAR(100)
)
AS
BEGIN

DECLARE @ProductAreasXml XML = (SELECT CONVERT(VARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'PRODUCTAREAS');

INSERT INTO @ProductAreaTable
SELECT 
	AreaNodesTable.Item.value('Code[1]', 'varchar(3)'),
	AreaNodesTable.Item.value('Description[1]', 'varchar(100)')
FROM  
	@ProductAreasXml.nodes('/NewDataSet/Table1') AS AreaNodesTable(Item)

RETURN
END", "DROP FUNCTION ClientReport_GetAllEnterpriseProductAreasFromRegistry", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region ClientReport_GetAllProductsAndModulesFromRegistry

			new DatabaseViewAndRoutineCreateScript("ClientReport_GetAllProductsAndModulesFromRegistry",
@"CREATE FUNCTION ClientReport_GetAllProductsAndModulesFromRegistry()
RETURNS @Mapping TABLE 
(
	ProductCode VARCHAR(3),
	ProductDescription VARCHAR(100),
	ModuleCode VARCHAR(3),
	ModuleDescription VARCHAR(100)
)
AS
BEGIN

--------------------------------------------------
-- Add All " + BrandingFactory.Instance.ProductName + @" Modules
--------------------------------------------------

DECLARE @ProductsXml XML = (SELECT CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'SystemProductMappings');

INSERT INTO @Mapping
SELECT 
	ProductNode.Item.value('Code[1]', 'char(3)') AS ProductCode,
	ProductNode.Item.value('Description[1]', 'varchar(100)') AS ProductDescription,
	ISNULL(ModuldNode.Item.value('ModuleCode[1]', 'char(3)'), '') AS ModuleCode, 
	ISNULL(ModuldNode.Item.value('ModuleDescription[1]', 'varchar(100)'), '') AS ModuleDescription
FROM
	@ProductsXml.nodes('ArrayOfSystemProduct/SystemProduct') as ProductNode(Item)
	OUTER APPLY	ProductNode.Item.nodes('ArrayOfProductAreaModuleMapping/ProductAreaModuleMapping') as ModuldNode(Item)

--------------------------------------------------
-- Duplicate Modules From " + BrandingFactory.Instance.ProductName + @" to eHub
--------------------------------------------------

DECLARE @EHubProductDescription VARCHAR(100) = (SELECT TOP 1 ProductDescription FROM @Mapping WHERE ProductCode = 'HUB');

INSERT INTO @Mapping
SELECT 
	'HUB', 
	@EHubProductDescription, 
	ModuleCode,
	ModuleDescription
FROM
	@Mapping
WHERE
	ProductCode = 'ENT'

--------------------------------------------------
-- Add 'ALL' Module Description
--------------------------------------------------

INSERT INTO @Mapping
SELECT DISTINCT 
	ProductCode, 
	ProductDescription,
	'ALL',
	'All - ' + ProductDescription
FROM
	@Mapping

RETURN
END", "DROP FUNCTION ClientReport_GetAllProductsAndModulesFromRegistry", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region ClientReport_GetAllCr8ModulesFromRegistry

			new DatabaseViewAndRoutineCreateScript("ClientReport_GetAllCr8ModulesFromRegistry",
@"CREATE FUNCTION ClientReport_GetAllCr8ModulesFromRegistry()
RETURNS @Mapping TABLE 
(
	ModuleCode VARCHAR(3),
	ModuleDescription VARCHAR(100),
	ProductCode VARCHAR(3),
	ProductDescription VARCHAR(100)
)
AS
BEGIN

DECLARE @IncidentCr8ModulesXml XML = (SELECT CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'SystemProductAreaIncidentCr8Mappings');

INSERT INTO @Mapping
SELECT
	ISNULL(ModuldNode.Item.value('ModuleCode[1]', 'char(3)'), '') AS ModuleCode, 
	ISNULL(ModuldNode.Item.value('ModuleDescription[1]', 'varchar(100)'), '') AS ModuleDescription,
	ProductNode.Item.value('Code[1]', 'char(3)') AS ProductCode,
	ProductNode.Item.value('Description[1]', 'varchar(100)') AS ProductDescription
FROM
	@IncidentCr8ModulesXml.nodes('ArrayOfSystemProduct/SystemProduct') as ProductNode(Item)
	OUTER APPLY	ProductNode.Item.nodes('ArrayOfProductAreaModuleMapping/ProductAreaModuleMapping') as ModuldNode(Item)
RETURN
END", "DROP FUNCTION ClientReport_GetAllCr8ModulesFromRegistry", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region ClientReport_GetAllCr9ModulesFromRegistry

			new DatabaseViewAndRoutineCreateScript("ClientReport_GetAllCr9ModulesFromRegistry",
@"CREATE FUNCTION ClientReport_GetAllCr9ModulesFromRegistry()
RETURNS @Mapping TABLE 
(
	ModuleCode VARCHAR(3),
	ModuleDescription VARCHAR(100),
	ProductCode VARCHAR(3),
	ProductDescription VARCHAR(100)
)
AS
BEGIN

DECLARE @IncidentCr9ModulesXml XML = (SELECT CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'SystemProductAreaIncidentCr9Mappings');

INSERT INTO @Mapping
SELECT
	ISNULL(ModuldNode.Item.value('ModuleCode[1]', 'char(3)'), '') AS ModuleCode, 
	ISNULL(ModuldNode.Item.value('ModuleDescription[1]', 'varchar(100)'), '') AS ModuleDescription,
	ProductNode.Item.value('Code[1]', 'char(3)') AS ProductCode,
	ProductNode.Item.value('Description[1]', 'varchar(100)') AS ProductDescription
FROM
	@IncidentCr9ModulesXml.nodes('ArrayOfSystemProduct/SystemProduct') as ProductNode(Item)
	OUTER APPLY	ProductNode.Item.nodes('ArrayOfProductAreaModuleMapping/ProductAreaModuleMapping') as ModuldNode(Item)
RETURN
END", "DROP FUNCTION ClientReport_GetAllCr9ModulesFromRegistry", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region ClientReport_GetAllSourceModuleProductAreasFromRegistry

			new DatabaseViewAndRoutineCreateScript("ClientReport_GetAllSourceModuleProductAreasFromRegistry",
@"CREATE FUNCTION ClientReport_GetAllSourceModuleProductAreasFromRegistry()
RETURNS @Mapping TABLE 
(
	SourceModuleCode VARCHAR(50),
	ModuleCode VARCHAR(3),
	ProductAreaCode VARCHAR(3),
	ProductAreaDescription VARCHAR(100)
)
AS
BEGIN

--------------------------------------------------
-- " + BrandingFactory.Instance.ProductName + @" Product Areas
--------------------------------------------------

DECLARE @ProductAreasXml XML
SET @ProductAreasXml = 
	(SELECT CONVERT(VARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'PRODUCTAREAS')

DECLARE @ProductAreaTable TABLE
(
	ProductAreaCode VARCHAR(3),
	ProductAreaDescription VARCHAR(100)
)

INSERT INTO @ProductAreaTable
SELECT 
	AreaNodesTable.Item.value('Code[1]', 'varchar(3)'),
	AreaNodesTable.Item.value('Description[1]', 'varchar(100)')
FROM  
	@ProductAreasXml.nodes('/NewDataSet/Table1') AS AreaNodesTable(Item)

--------------------------------------------------
-- Add " + BrandingFactory.Instance.ProductName + @" Incident Source Modules
--------------------------------------------------

DECLARE @IncidentMenuSectionsXml XML
SET @IncidentMenuSectionsXml = 
	(SELECT CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'SystemProductMappings')

DECLARE @IncidentCr8ModulesXml XML
SET @IncidentCr8ModulesXml = 
	(SELECT CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'SystemProductAreaIncidentCr8Mappings')

DECLARE @IncidentCr9ModulesXml XML
SET @IncidentCr9ModulesXml = 
	(SELECT CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), SD_BinaryValue)) AS XML FROM dbo.StmData WHERE SD_Name = 'SystemProductAreaIncidentCr9Mappings')

INSERT INTO @Mapping
SELECT
	SourceModuleCode,
	ModuleCode,
	ISNULL(ProductAreaCode, ''),
	ISNULL(ProductAreaDescription, '')
FROM  
	(
		SELECT
			IncidentCategorySubmappingsTable.Item.value('Code[1]', 'char(50)') AS SourceModuleCode,
			IncidentCategorySubmappingsTable.Item.value('../../ModuleCode[1]', 'char(3)') AS ModuleCode,
			IncidentCategorySubmappingsTable.Item.value('ProductArea[1]', 'char(3)') AS ProductArea
		FROM @IncidentMenuSectionsXml.nodes('ArrayOfSystemProduct/SystemProduct/ArrayOfProductAreaModuleMapping/ProductAreaModuleMapping/ArrayOfProductAreaSourceModuleMapping/ProductAreaSourceModuleMapping') AS IncidentCategorySubmappingsTable(Item)
		UNION ALL
		SELECT
			IncidentCr8SubmappingsTable.Item.value('Code[1]', 'char(50)') AS SourceModuleCode,
			IncidentCr8SubmappingsTable.Item.value('../../ModuleCode[1]', 'char(3)') AS ModuleCode,
			IncidentCr8SubmappingsTable.Item.value('ProductArea[1]', 'char(3)') AS ProductArea
		FROM @IncidentCr8ModulesXml.nodes('ArrayOfSystemProduct/SystemProduct/ArrayOfProductAreaModuleMapping/ProductAreaModuleMapping/ArrayOfProductAreaSourceModuleMapping/ProductAreaSourceModuleMapping') AS IncidentCr8SubmappingsTable(Item)
		UNION ALL
		SELECT
			IncidentCr9SubmappingsTable.Item.value('Code[1]', 'char(50)') AS SourceModuleCode,
			IncidentCr9SubmappingsTable.Item.value('../../ModuleCode[1]', 'char(3)') AS ModuleCode,
			IncidentCr9SubmappingsTable.Item.value('ProductArea[1]', 'char(3)') AS ProductArea
		FROM @IncidentCr9ModulesXml.nodes('ArrayOfSystemProduct/SystemProduct/ArrayOfProductAreaModuleMapping/ProductAreaModuleMapping/ArrayOfProductAreaSourceModuleMapping/ProductAreaSourceModuleMapping') AS IncidentCr9SubmappingsTable(Item)
	) IncidentMenuSectionMappings
	LEFT JOIN @ProductAreaTable ON ProductArea = ProductAreaCode

RETURN
END", "DROP FUNCTION ClientReport_GetAllSourceModuleProductAreasFromRegistry", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region Clientcsfn__IncidentMainOpenAnalysis

			new DatabaseViewAndRoutineCreateScript("Clientcsfn__IncidentMainOpenAnalysis", @"CREATE FUNCTION Clientcsfn__IncidentMainOpenAnalysis  
(  
 @IM_DateAddedFrom DATETIME,  
 @IM_DateAddedTo DATETIME,  
 @IM_GG_TeamList VARCHAR(4000),   
 @IM_GS_CustomerServiceContactList VARCHAR(4000),   
 @IM_Product CHAR(3),
 @IM_ProductArea CHAR(3),
 @IM_OH_ClientList VARCHAR(4000),
 @CountryPKs VARCHAR(4000)  
)  
RETURNS TABLE   
AS   
RETURN  
SELECT  
	GLBStaff.GS_PK AS IM_GS_CustomerServiceContact,  
	IM_OH_Client,  
	IM_Product,  
	IncidentProduct.ProductDescription AS IM_ProductDescription,
	IM_Module,  
	IM_Status,  
	IM_IncidentNumber, 
	IM_ResolutionCode,   
	1 AS IM_JobCount,  
	IM_SystemCreateTimeUtc,  
	dbo.Clientfn_WorkingDaysBetween(IM_SystemCreateTimeUtc, Today, GS_GB_HomeBranch, GLBStaff.GS_PK) as IM_WorkingDaysOld,
	DATEDIFF(dd, IM_SystemCreateTimeUtc, Today) as IM_RoughWorkingDaysOld,
	CASE WHEN dbo.Clientfn_WorkingDaysBetween(IM_SystemCreateTimeUtc, Today, GS_GB_HomeBranch, GLBStaff.GS_PK) <= 1 THEN 1 END AS IM_UpTo1DayOld,  
	CASE WHEN dbo.Clientfn_WorkingDaysBetween(IM_SystemCreateTimeUtc, Today, GS_GB_HomeBranch, GLBStaff.GS_PK) BETWEEN 2 AND 5 THEN 1 END AS IM_UpTo1WeekOld,  
	CASE WHEN dbo.Clientfn_WorkingDaysBetween(IM_SystemCreateTimeUtc, Today, GS_GB_HomeBranch, GLBStaff.GS_PK) BETWEEN 6 AND 10 THEN 1 END AS IM_UpTo1MonthOld,  
	CASE WHEN dbo.Clientfn_WorkingDaysBetween(IM_SystemCreateTimeUtc, Today, GS_GB_HomeBranch, GLBStaff.GS_PK) > 10 THEN 1 END AS IM_Old,  
	CASE WHEN IM_Priority = 'CR1' THEN 1 END AS IM_CR1,
	CASE WHEN IM_Priority = 'CR2' THEN 1 END AS IM_CR2,
	CASE WHEN IM_Priority = 'CR3' THEN 1 END AS IM_CR3,
	CASE WHEN IM_Priority = 'CR4' THEN 1 END AS IM_CR4,
	CASE WHEN IM_Priority = 'CR5' THEN 1 END AS IM_CR5,
	CASE WHEN IM_Priority = 'CR6' THEN 1 END AS IM_CR6,
	CASE WHEN IM_ResolutionCode = 'AUC' AND IM_Status = 'OPN' THEN 1 END AS IM_OPNAUCJobsOpen,  
	CASE WHEN IM_ResolutionCode = 'ADD' AND IM_Status = 'OPN' THEN 1 END AS IM_OPNADDJobsOpen,  
	CASE WHEN IM_ResolutionCode = 'AUC' AND IM_Status = 'WRK' THEN 1 END AS IM_WRKAUCJobsOpen,  
	CASE WHEN IM_ResolutionCode = 'REQ' THEN 1 END AS IM_REQJobsOpen,  
	CASE WHEN IM_ResolutionCode = 'STE' THEN 1 END AS IM_STEJobsOpen,  
	CASE WHEN IM_ResolutionCode = 'WRK' THEN 1 END AS IM_WRKJobsOpen,  
	CASE WHEN IM_ResolutionCode = 'CBK' THEN 1 END AS IM_CBKJobsOpen,  
	CASE WHEN IM_ResolutionCode = 'UPO' THEN 1 END AS IM_UPOJobsOpen,  
	CASE IM_Product WHEN 'ENT' THEN 1 END AS IM_EnterpriseIncident,  
	CASE IM_Product WHEN 'DLV' THEN 1 END AS IM_DeliveranceIncident,  
	CASE IM_Product WHEN 'CAR' THEN 1 END AS IM_CargoWiseIncident,
	CASE WHEN IM_Product NOT IN ('ENT','DLV','CAR') THEN 1 END AS IM_OtherProductIncident
FROM  
	dbo.incidentmain  
	LEFT JOIN dbo.GLBStaff ON IM_GS_NKCustServiceContact = GLBStaff.GS_Code
	LEFT JOIN 
	(
		OrgHeader
		LEFT JOIN dbo.RefUNLOCO ON RL_Code = OH_RL_NKClosestPort
	)
		ON
			OH_PK = IM_OH_Client
			AND ISNULL(@CountryPKs, '') != ''
	CROSS JOIN dbo.Clientvw_GetDate
	LEFT JOIN dbo.RefCountry ON RN_Code = RL_RN_NKCountryCode
	LEFT JOIN ClientReport_GetAllProductsFromRegistry() IncidentProduct ON IM_Product = IncidentProduct.ProductCode
WHERE  
 IM_CloseTimeUtc is NULL AND
 IM_IncidentType = 'INC' AND  
 (IM_Status != 'CLS' AND IM_Category = 'SUP') AND  
 IM_SystemCreateTimeUtc BETWEEN @IM_DateAddedFrom AND @IM_DateAddedTo AND   
 (charindex(CAST(IM_GG_Team AS CHAR(36)), isnull(@IM_GG_TeamList,'')) > 0 OR isnull(@IM_GG_TeamList,'') = '') AND  
 (charindex(CAST(GLBStaff.GS_PK AS VARCHAR(36)), isnull(@IM_GS_CustomerServiceContactList,'')) > 0 OR isnull(@IM_GS_CustomerServiceContactList,'') = '') AND  
 (charindex(CAST(IM_OH_Client AS CHAR(36)), isnull(@IM_OH_ClientList,'')) > 0 OR isnull(@IM_OH_ClientList,'') = '') AND  
 (IM_Product = @IM_Product OR ISNULL(@IM_Product,'')='')  AND
 (IM_ProgramArea = @IM_ProductArea OR ISNULL(@IM_ProductArea,'') = '') AND 
 (isnull(@CountryPKs,'') = '' OR charindex(CAST(RefCountry.RN_PK AS CHAR(36)), isnull(@CountryPKs,'')) > 0)", "drop FUNCTION Clientcsfn__IncidentMainOpenAnalysis", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Clientcsfn__IncidentMainClosedAnalysis

			new DatabaseViewAndRoutineCreateScript("Clientcsfn__IncidentMainClosedAnalysis", @"CREATE FUNCTION Clientcsfn__IncidentMainClosedAnalysis
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_CloseTimeUtcFrom DATETIME,
	@IM_CloseTimeUtcTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@ProductArea CHAR(3),
	@IM_ResolutionCode CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000)
)
RETURNS @Result TABLE 
(
	IM_PK UNIQUEIDENTIFIER,
	IM_GS_CustomerServiceContact UNIQUEIDENTIFIER,
	IM_OH_Client UNIQUEIDENTIFIER,
	IM_ResolutionCode VARCHAR(3),	
	IM_Product VARCHAR(3),
	IM_ProductDescription VARCHAR(100),
	IM_Priority CHAR(3),
	IM_Module VARCHAR(3),
	IM_SystemCreateTimeUtc SMALLDATETIME,
	IM_CloseTimeUtc SMALLDATETIME,
	IM_IncidentOpenedWorkHours DECIMAL(5),
	IM_JobCount INT,
	IM_EnterpriseIncident INT,
	IM_DeliveranceIncident INT,
	IM_CargoWiseIncident INT,
	IM_OtherProductIncident INT,
	IM_PDMJobsClosed INT,
	IM_PFRJobsClosed INT,
	IM_PassedJobsClosed INT,
	IM_CWRJobsClosed INT,
	IM_SRSJobsClosed INT,
	IM_SYSJobsClosed INT,
	IM_TRNJobsClosed INT,
	IM_TRMJobsClosed INT,
	IM_UPDJobsClosed INT,
	IM_DTFJobsClosed INT,
	IM_URPJobsClosed INT,
	IM_TSPJobsClosed INT,
	IM_NRCJobsClosed INT,
	IM_OTHJobsClosed INT
)
AS 
BEGIN

INSERT INTO @Result
SELECT
	IM_PK,
	GlbStaff.GS_PK AS IM_GS_CustomerServiceContact,
	IM_OH_Client,
	IM_ResolutionCode,	
	IM_Product,
	IncidentProduct.ProductDescription AS IM_ProductDescription,
	IM_Priority,
	IM_Module,
	IM_SystemCreateTimeUtc,
	IM_CloseTimeUtc,
	IM_IncidentOpenedWorkHours,
	1 AS IM_JobCount,
	CASE IM_Product WHEN 'ENT' THEN	1 END AS IM_EnterpriseIncident,
	CASE IM_Product	WHEN 'DLV' THEN	1 END AS IM_DeliveranceIncident,
	CASE IM_Product	WHEN 'CAR' THEN	1 END AS IM_CargoWiseIncident,
	CASE WHEN IM_Product NOT IN ('ENT', 'DLV', 'CAR') THEN 1 END AS IM_OtherProductIncident,
	CASE IM_Category WHEN 'DEF' THEN 1 END AS IM_PDMJobsClosed,		-- Escalated As Defect
	CASE IM_Category WHEN 'FTR' THEN 1 END AS IM_PFRJobsClosed,		-- Escalated As Feature Request
	CASE WHEN IM_ResolutionCode IN ('PSL','PTI','PTT') THEN 1 END AS IM_PassedJobsClosed,	-- Passed To Sales / Installation / Training
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'CWR' THEN 1 END AS IM_CWRJobsClosed,	-- Awaiting Response
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'SRS' THEN 1 END AS IM_SRSJobsClosed,	-- Self Resolved
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'SYS' THEN 1 END AS IM_SYSJobsClosed,	-- System Hardware / Network
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'TRN' THEN 1 END AS IM_TRNJobsClosed,	-- Training Referred To Learning Materials
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'TRM' THEN 1 END AS IM_TRMJobsClosed,	-- Training No Learning Materials
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'UPD' THEN 1 END AS IM_UPDJobsClosed,	-- Upgrade Delivered
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'DTF' THEN 1 END AS IM_DTFJobsClosed,	-- CS Data Fix
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'URP' THEN 1 END AS IM_URPJobsClosed,	-- Unreproducible
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'TSP' THEN 1 END AS IM_TSPJobsClosed,	-- Third Party System Problem
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode = 'NRC' THEN 1 END AS IM_NRCJobsClosed,	-- No Response From Client
	CASE WHEN IM_Category = 'SUP' AND IM_ResolutionCode NOT IN ('PDM','PFR','CWR','PSL','PTI','PTT','SRS','SYS','TRN','TRM','UPD','DTF','URP','TSP','NRC') THEN 1 END AS IM_OTHJobsClosed
FROM
	dbo.IncidentMain
	LEFT JOIN dbo.GlbStaff ON IM_GS_NKCustServiceContact = GlbStaff.GS_Code
	LEFT JOIN 
	(
		dbo.OrgHeader
		LEFT JOIN dbo.RefUNLOCO ON RL_Code = OH_RL_NKClosestPort
	) ON OH_PK = IM_OH_Client AND ISNULL(@CountryPKs, '') != ''
	LEFT JOIN dbo.RefCountry ON RN_Code = RL_RN_NKCountryCode
	LEFT JOIN dbo.ClientReport_GetAllProductsFromRegistry() IncidentProduct ON IM_Product = IncidentProduct.ProductCode
WHERE
	IM_CloseTimeUtc IS NOT NULL AND 
	IM_IncidentType = 'INC' AND
	(IM_Status = 'CLS' OR IM_Category <> 'SUP') AND
	IM_SystemCreateTimeUtc BETWEEN @IM_DateAddedFrom AND @IM_DateAddedTo AND 
	IM_CloseTimeUtc BETWEEN @IM_CloseTimeUtcFrom AND @IM_CloseTimeUtcTo AND
	IM_GS_NKCustServiceContact NOT IN ('~BP', 'ZZ') AND
	(CHARINDEX(CAST(IM_GG_Team AS CHAR(36)), ISNULL(@IM_GG_TeamList,'')) > 0 OR ISNULL(@IM_GG_TeamList,'') = '') AND
	(CHARINDEX(CAST(IM_OH_Client AS CHAR(36)), ISNULL(@IM_OH_ClientList,'')) > 0 OR ISNULL(@IM_OH_ClientList,'') = '') AND
	(IM_Product = @IM_Product OR ISNULL(@IM_Product,'') = '') AND
	(IM_ProgramArea = @ProductArea OR ISNULL(@ProductArea,'') = '') AND
	(ISNULL(@CountryPKs,'') = '' OR CHARINDEX(CAST(RefCountry.RN_PK AS CHAR(36)), ISNULL(@CountryPKs,'')) > 0) AND
	(ISNULL(@IM_ResolutionCode, '') = ''
		OR (@IM_ResolutionCode = IM_ResolutionCode)
		OR (@IM_ResolutionCode = 'PDM' AND IM_Category = 'DEF')
		OR (@IM_ResolutionCode = 'PFR' AND IM_Category = 'FTR')
		OR (@IM_ResolutionCode = 'PCR' AND IM_Category = 'COM')
		OR (@IM_ResolutionCode = 'PSR' AND IM_Category = 'CSR'))


UPDATE 
	@Result
SET 
	IM_GS_CustomerServiceContact = LastCloseOrEscalateStaffByTask.GS_PK
FROM
	(
		SELECT GS_PK, P9_ParentId
		FROM
			(
				SELECT P9_GS_NKAssignedStaffMember, P9_ParentId, ROW_NUMBER() OVER (PARTITION BY P9_ParentId ORDER BY P9_Sequence DESC) AS TaskSeq 
				FROM dbo.ProcessTasks JOIN @Result ON P9_ParentId = IM_PK
				WHERE P9_Status = 'CLS' AND P9_GS_NKAssignedStaffMember != '' AND P9_GS_NKAssignedStaffMember NOT IN ('~BP', 'ZZ')
			) CloseTask 
			LEFT JOIN dbo.GlbStaff ON P9_GS_NKAssignedStaffMember = GS_Code
		WHERE
			TaskSeq = 1
	) LastCloseOrEscalateStaffByTask JOIN @Result ON P9_ParentId = IM_PK
WHERE
	IM_GS_CustomerServiceContact IS NULL

UPDATE 
	@Result
SET 
	IM_GS_CustomerServiceContact = LastCloseOrEscalateStaffByLog.GS_PK
FROM
	(
		SELECT GS_PK,SL_Parent
		FROM
			(
				SELECT SL_GS_NKUser, SL_Parent, ROW_NUMBER() OVER (PARTITION BY SL_Parent ORDER BY SL_PostedTimeUtc DESC) AS EventSeq 
				FROM dbo.StmALog JOIN @Result ON SL_Parent = IM_PK
				WHERE SL_Reference LIKE 'Support - % to CLS' OR SL_Reference LIKE 'Status - % to CLS' OR SL_Reference LIKE 'Stage - SUP to %'
			) EventLog 
			LEFT JOIN dbo.GlbStaff ON SL_GS_NKUser = GS_Code AND SL_GS_NKUser NOT IN ('~BP', 'ZZ')
		WHERE
			EventSeq = 1
	) LastCloseOrEscalateStaffByLog JOIN @Result ON SL_Parent = IM_PK
WHERE
	IM_GS_CustomerServiceContact IS NULL

DELETE FROM
	@Result
WHERE
	ISNULL(@IM_GS_CustomerServiceContactList,'') != ''
	AND CHARINDEX(CAST(IM_GS_CustomerServiceContact AS VARCHAR(36)), @IM_GS_CustomerServiceContactList) <= 0 

RETURN
END", "drop FUNCTION Clientcsfn__IncidentMainClosedAnalysis", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region Clientcsfn__CSIClosuresEventStaffSummary

			new DatabaseViewAndRoutineCreateScript("Clientcsfn__CSIClosuresEventStaffSummary", @"CREATE FUNCTION Clientcsfn__CSIClosuresEventStaffSummary
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_CloseTimeUtcFrom DATETIME,
	@IM_CloseTimeUtcTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@IM_ProductArea CHAR(3),
	@IM_ResolutionCode CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000)
)
RETURNS TABLE 
AS 
RETURN
SELECT
	GS_PK,
	COUNT(*) as IM_SL_EventCount
FROM
	Clientcsfn__IncidentMainClosedAnalysis
		(
			@IM_DateAddedFrom ,
			@IM_DateAddedTo ,
			@IM_CloseTimeUtcFrom ,
			@IM_CloseTimeUtcTo ,
			@IM_GG_TeamList ,
			@IM_GS_CustomerServiceContactList , 
			@IM_Product ,
			@IM_ProductArea,
			@IM_ResolutionCode ,
			@IM_OH_ClientList,
			@CountryPKs 
		)
	INNER JOIN dbo.STMALog
		on 
		IM_PK = SL_Parent
		AND DATEADD(mi, DATEDIFF(mi, 0, SL_PostedTimeUtc), 0) BETWEEN IM_SystemCreateTimeUtc AND IM_CloseTimeUtc
	INNER JOIN dbo.GLBStaff ON SL_GS_NKUser = GS_Code
	GROUP BY GS_PK", "drop FUNCTION Clientcsfn__CSIClosuresEventStaffSummary", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Clientcsfn__CSIClosuresClientDispositionSummary

			new DatabaseViewAndRoutineCreateScript("Clientcsfn__CSIClosuresClientDispositionSummary", @"CREATE FUNCTION Clientcsfn__CSIClosuresClientDispositionSummary
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_CloseTimeUtcFrom DATETIME,
	@IM_CloseTimeUtcTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@IM_ProductArea CHAR(3),
	@IM_ResolutionCode CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000)
)
RETURNS @Result TABLE 
(
	IM_OH_Client UNIQUEIDENTIFIER,
	IM_JobCount INT,
	IM_OTHJobsClosed INT,
	IM_PDMJobsClosed INT,
	IM_PFRJobsClosed INT,
	IM_PassedJobsClosed INT,
	IM_SRSJobsClosed INT,
	IM_SYSJobsClosed INT,
	IM_TRNJobsClosed INT,
	IM_TRMJobsClosed INT,
	IM_UPDJobsClosed INT,
	IM_URPJobsClosed INT,
	IM_TSPJobsClosed INT,
	IM_NRCJobsClosed INT,
	IM_CWRJobsClosed INT,
	IM_DTFJobsClosed INT,
	IM_EnterpriseIncident INT,
	IM_DeliveranceIncident INT,
	IM_CargoWiseIncident INT,
	IM_OtherProductIncident INT,
	IM_OtherProductIncidentDetails VARCHAR(500)
) 
AS
BEGIN

-------------------------------------
-- Main Analysis Result
-------------------------------------

DECLARE @MainAnalysisResult TABLE
(
	IM_OH_Client UNIQUEIDENTIFIER,
	IM_Product VARCHAR(3),
	IM_ProductDescription VARCHAR(100),
	IM_JobCount INT,
	IM_OTHJobsClosed INT,
	IM_PDMJobsClosed INT,
	IM_PFRJobsClosed INT,
	IM_PassedJobsClosed INT,
	IM_SRSJobsClosed INT,
	IM_SYSJobsClosed INT,
	IM_TRNJobsClosed INT,
	IM_TRMJobsClosed INT,
	IM_UPDJobsClosed INT,
	IM_URPJobsClosed INT,
	IM_TSPJobsClosed INT,
	IM_NRCJobsClosed INT,
	IM_CWRJobsClosed INT,
	IM_DTFJobsClosed INT,
	IM_EnterpriseIncident INT,
	IM_DeliveranceIncident INT,
	IM_CargoWiseIncident INT,
	IM_OtherProductIncident INT
)

INSERT INTO @MainAnalysisResult
SELECT
	IM_OH_Client,
	IM_Product,
	IM_ProductDescription,
	IM_JobCount,
	IM_OTHJobsClosed,
	IM_PDMJobsClosed,
	IM_PFRJobsClosed,
	IM_PassedJobsClosed,
	IM_SRSJobsClosed,
	IM_SYSJobsClosed,
	IM_TRNJobsClosed,
	IM_TRMJobsClosed,
	IM_UPDJobsClosed,
	IM_URPJobsClosed,
	IM_TSPJobsClosed,
	IM_NRCJobsClosed,
	IM_CWRJobsClosed,
	IM_DTFJobsClosed,
	IM_EnterpriseIncident, 
	IM_DeliveranceIncident, 
	IM_CargoWiseIncident,
	IM_OtherProductIncident
FROM
	dbo.Clientcsfn__IncidentMainClosedAnalysis
	(
		@IM_DateAddedFrom,
		@IM_DateAddedTo,
		@IM_CloseTimeUtcFrom,
		@IM_CloseTimeUtcTo,
		@IM_GG_TeamList, 
		@IM_GS_CustomerServiceContactList, 
		@IM_Product,
		@IM_ProductArea,
		@IM_ResolutionCode,
		@IM_OH_ClientList,
		@CountryPKs
	)

-------------------------------------
-- Other Product Incident Summary
-------------------------------------

DECLARE @OtherProductAnalysisResult TABLE
(
    IM_OH_Client UNIQUEIDENTIFIER,
    IM_OtherProductIncidentDetails VARCHAR(500)
)

INSERT INTO @OtherProductAnalysisResult (IM_OH_Client, IM_OtherProductIncidentDetails)
SELECT DISTINCT t1.IM_OH_Client, 
    (
        SELECT STRING_AGG(CONCAT(IM_ProductDescription, '(', CONVERT(VARCHAR(100), ProductCount), ')'), ', ')
        FROM 
        (
            SELECT IM_OH_Client, IM_ProductDescription, COUNT(*) as ProductCount
            FROM @MainAnalysisResult t2
            WHERE 
            t2.IM_OH_Client = t1.IM_OH_Client
            AND t2.IM_Product NOT IN ('ENT', 'DLV', 'CAR')
            GROUP BY IM_OH_Client, IM_ProductDescription
        ) AS subquery
    )
FROM @MainAnalysisResult t1;

-------------------------------------
-- Result Table
-------------------------------------

INSERT INTO @Result
SELECT
	IM_OH_Client,
	SUM(IM_JobCount) as IM_JobCount,
	SUM(IM_OTHJobsClosed) AS IM_OTHJobsClosed,
	SUM(IM_PDMJobsClosed) AS IM_PDMJobsClosed,
	SUM(IM_PFRJobsClosed) AS IM_PFRJobsClosed,
	SUM(IM_PassedJobsClosed) AS IM_PassedJobsClosed,
	SUM(IM_SRSJobsClosed) AS IM_SRSJobsClosed,
	SUM(IM_SYSJobsClosed) AS IM_SYSJobsClosed,
	SUM(IM_TRNJobsClosed) AS IM_TRNJobsClosed,
	SUM(IM_TRMJobsClosed) AS IM_TRMJobsClosed,
	SUM(IM_UPDJobsClosed) AS IM_UPDJobsClosed,
	SUM(IM_URPJobsClosed) AS IM_URPJobsClosed,
	SUM(IM_TSPJobsClosed) AS IM_TSPJobsClosed,
	SUM(IM_NRCJobsClosed) AS IM_NRCJobsClosed,
	SUM(IM_CWRJobsClosed) AS IM_CWRJobsClosed,
	SUM(IM_DTFJobsClosed) AS IM_DTFJobsClosed,
	SUM(IM_EnterpriseIncident) AS IM_EnterpriseIncident, 
	SUM(IM_DeliveranceIncident) AS IM_DeliveranceIncident, 
	SUM(IM_CargoWiseIncident) AS IM_CargoWiseIncident,
	SUM(IM_OtherProductIncident) AS IM_OtherProductIncident,
	''
FROM
	@MainAnalysisResult
GROUP BY
	IM_OH_Client

UPDATE @Result
SET result.IM_OtherProductIncidentDetails = other.IM_OtherProductIncidentDetails
FROM
	@Result result
	JOIN @OtherProductAnalysisResult other ON result.IM_OH_Client = other.IM_OH_Client

RETURN
END",
	"DROP FUNCTION Clientcsfn__CSIClosuresClientDispositionSummary", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region Clientcsfn__CSIOpenItemsStaffSummary

			new DatabaseViewAndRoutineCreateScript("Clientcsfn__CSIOpenItemsStaffSummary", @"CREATE FUNCTION Clientcsfn__CSIOpenItemsStaffSummary
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@IM_ProductArea CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000)
)
RETURNS @Result TABLE 
(
	IM_GS_CustomerServiceContact UNIQUEIDENTIFIER,
	IM_Old INT,
	IM_UpTo1DayOld INT,
	IM_UpTo1WeekOld INT,
	IM_UpTo1MonthOld INT,
	IM_JobCount INT,
	IM_CR1 INT,
	IM_CR2 INT,
	IM_CR3 INT,
	IM_CR4 INT,
	IM_CR5 INT,
	IM_CR6 INT,
	IM_OPNAUCJobsOpen INT,
	IM_OPNADDJobsOpen INT,
	IM_WRKAUCJobsOpen INT,
	IM_REQJobsOpen INT,
	IM_STEJobsOpen INT,
	IM_WRKJobsOpen INT,
	IM_CBKJobsOpen INT,
	IM_UPOJobsOpen INT,
	IM_EnterpriseIncident INT,
	IM_DeliveranceIncident INT,
	IM_CargoWiseIncident INT,
	IM_OtherProductIncident INT,
	IM_OtherProductIncidentDetails VARCHAR(500)
) 
AS
BEGIN

-------------------------------------
-- Main Analysis Result
-------------------------------------

DECLARE @MainAnalysisResult TABLE
(
	IM_GS_CustomerServiceContact UNIQUEIDENTIFIER,
	IM_Product VARCHAR(3),
	IM_ProductDescription VARCHAR(100),
	IM_Old INT,
	IM_UpTo1DayOld INT,
	IM_UpTo1WeekOld INT,
	IM_UpTo1MonthOld INT,
	IM_JobCount INT,
	IM_CR1 INT,
	IM_CR2 INT,
	IM_CR3 INT,
	IM_CR4 INT,
	IM_CR5 INT,
	IM_CR6 INT,
	IM_OPNAUCJobsOpen INT,
	IM_OPNADDJobsOpen INT,
	IM_WRKAUCJobsOpen INT,
	IM_REQJobsOpen INT,
	IM_STEJobsOpen INT,
	IM_WRKJobsOpen INT,
	IM_CBKJobsOpen INT,
	IM_UPOJobsOpen INT,
	IM_EnterpriseIncident INT,
	IM_DeliveranceIncident INT,
	IM_CargoWiseIncident INT,
	IM_OtherProductIncident INT
)

INSERT INTO @MainAnalysisResult
SELECT
	IM_GS_CustomerServiceContact,
	IM_Product,
	IM_ProductDescription,
	IM_Old,
	IM_UpTo1DayOld,
	IM_UpTo1WeekOld,
	IM_UpTo1MonthOld,
	IM_JobCount,
	IM_CR1,
	IM_CR2,
	IM_CR3,
	IM_CR4,
	IM_CR5,
	IM_CR6,
	IM_OPNAUCJobsOpen,
	IM_OPNADDJobsOpen,
	IM_WRKAUCJobsOpen,
	IM_REQJobsOpen,
	IM_STEJobsOpen,
	IM_WRKJobsOpen,
	IM_CBKJobsOpen,
	IM_UPOJobsOpen,
	IM_EnterpriseIncident,
	IM_DeliveranceIncident,
	IM_CargoWiseIncident,
	IM_OtherProductIncident
FROM
	dbo.Clientcsfn__IncidentMainOpenAnalysis
	(
		@IM_DateAddedFrom,
		@IM_DateAddedTo,
		@IM_GG_TeamList, 
		@IM_GS_CustomerServiceContactList, 
		@IM_Product,
		@IM_ProductArea,
		@IM_OH_ClientList,
		@CountryPKs
	)

-------------------------------------
-- Other Product Incident Summary
-------------------------------------

DECLARE @OtherProductAnalysisResult TABLE
(
	IM_GS_CustomerServiceContact UNIQUEIDENTIFIER,
	IM_OtherProductIncidentDetails VARCHAR(500)
)

INSERT INTO @OtherProductAnalysisResult (IM_GS_CustomerServiceContact, IM_OtherProductIncidentDetails)
SELECT DISTINCT t1.IM_GS_CustomerServiceContact, 
    (
        SELECT STRING_AGG(CONCAT(IM_ProductDescription, '(', CONVERT(VARCHAR(100), ProductCount), ')'), ', ')
        FROM 
        (
            SELECT IM_GS_CustomerServiceContact, IM_ProductDescription, COUNT(*) as ProductCount
            FROM @MainAnalysisResult t2
            WHERE 
            t2.IM_GS_CustomerServiceContact = t1.IM_GS_CustomerServiceContact
            AND t2.IM_Product NOT IN ('ENT', 'DLV', 'CAR')
            GROUP BY IM_GS_CustomerServiceContact, IM_ProductDescription
        ) AS subquery
    )
FROM @MainAnalysisResult t1

-------------------------------------
-- Result Table
-------------------------------------

INSERT INTO @Result
SELECT
	IM_GS_CustomerServiceContact,
	SUM(IM_Old) AS IM_Old,
	SUM(IM_UpTo1DayOld) AS IM_UpTo1DayOld,
	SUM(IM_UpTo1WeekOld) AS IM_UpTo1WeekOld,
	SUM(IM_UpTo1MonthOld) AS IM_UpTo1MonthOld,
	SUM(IM_JobCount) AS IM_JobCount,
	SUM(IM_CR1) AS IM_CR1,
	SUM(IM_CR2) AS IM_CR2,
	SUM(IM_CR3) AS IM_CR3,
	SUM(IM_CR4) AS IM_CR4,
	SUM(IM_CR5) AS IM_CR5,
	SUM(IM_CR6) AS IM_CR6,
	SUM(IM_OPNAUCJobsOpen) AS IM_OPNAUCJobsOpen,
	SUM(IM_OPNADDJobsOpen) AS IM_OPNADDJobsOpen,
	SUM(IM_WRKAUCJobsOpen) AS IM_WRKAUCJobsOpen,
	SUM(IM_REQJobsOpen) AS IM_REQJobsOpen,
	SUM(IM_STEJobsOpen) AS IM_STEJobsOpen,
	SUM(IM_WRKJobsOpen) AS IM_WRKJobsOpen,
	SUM(IM_CBKJobsOpen) AS IM_CBKJobsOpen,
	SUM(IM_UPOJobsOpen) AS IM_UPOJobsOpen,
	SUM(IM_EnterpriseIncident) AS IM_EnterpriseIncident,
	SUM(IM_DeliveranceIncident) AS IM_DeliveranceIncident,
	SUM(IM_CargoWiseIncident) AS IM_CargoWiseIncident,
	SUM(IM_OtherProductIncident) AS IM_OtherProductIncident,
	''
FROM
	@MainAnalysisResult
GROUP BY
	IM_GS_CustomerServiceContact

UPDATE @Result
SET result.IM_OtherProductIncidentDetails = other.IM_OtherProductIncidentDetails
FROM
	@Result result
	JOIN @OtherProductAnalysisResult other ON result.IM_GS_CustomerServiceContact = other.IM_GS_CustomerServiceContact

RETURN
END",
	"DROP FUNCTION Clientcsfn__CSIOpenItemsStaffSummary", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region Clientcsfn__CSIClosuresStaffSummary

			new DatabaseViewAndRoutineCreateScript("Clientcsfn__CSIClosuresStaffSummary", @"CREATE FUNCTION Clientcsfn__CSIClosuresStaffSummary
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_CloseTimeUtcFrom DATETIME,
	@IM_CloseTimeUtcTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@IM_ProductArea CHAR(3),
	@IM_ResolutionCode CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000)
)
RETURNS @Result TABLE 
(
	IM_GS_CustomerServiceContact UNIQUEIDENTIFIER,
	IM_JobCount INT,
	IM_OTHJobsClosed INT,
	IM_PDMJobsClosed INT,
	IM_PFRJobsClosed INT,
	IM_PassedJobsClosed INT,
	IM_SRSJobsClosed INT,
	IM_SYSJobsClosed INT,
	IM_TRNJobsClosed INT,
	IM_TRMJobsClosed INT,
	IM_UPDJobsClosed INT,
	IM_URPJobsClosed INT,
	IM_TSPJobsClosed INT,
	IM_NRCJobsClosed INT,
	IM_CWRJobsClosed INT,
	IM_DTFJobsClosed INT,
	IM_EnterpriseIncident INT,
	IM_DeliveranceIncident INT,
	IM_CargoWiseIncident INT,
	IM_OtherProductIncident INT,
	IM_OtherProductIncidentDetails VARCHAR(500)
)
AS
BEGIN

-------------------------------------
-- Main Analysis Result
-------------------------------------

DECLARE @MainAnalysisResult TABLE
(
	IM_GS_CustomerServiceContact UNIQUEIDENTIFIER,
	IM_Product VARCHAR(3),
	IM_ProductDescription VARCHAR(100),
	IM_JobCount INT,
	IM_OTHJobsClosed INT,
	IM_PDMJobsClosed INT,
	IM_PFRJobsClosed INT,
	IM_PassedJobsClosed INT,
	IM_SRSJobsClosed INT,
	IM_SYSJobsClosed INT,
	IM_TRNJobsClosed INT,
	IM_TRMJobsClosed INT,
	IM_UPDJobsClosed INT,
	IM_URPJobsClosed INT,
	IM_TSPJobsClosed INT,
	IM_NRCJobsClosed INT,
	IM_CWRJobsClosed INT,
	IM_DTFJobsClosed INT,
	IM_EnterpriseIncident INT,
	IM_DeliveranceIncident INT,
	IM_CargoWiseIncident INT,
	IM_OtherProductIncident INT
)

INSERT INTO @MainAnalysisResult
SELECT
	IM_GS_CustomerServiceContact,
	IM_Product,
	IM_ProductDescription,
	IM_JobCount,
	IM_OTHJobsClosed,
	IM_PDMJobsClosed,
	IM_PFRJobsClosed,
	IM_PassedJobsClosed,
	IM_SRSJobsClosed,
	IM_SYSJobsClosed,
	IM_TRNJobsClosed,
	IM_TRMJobsClosed,
	IM_UPDJobsClosed,
	IM_URPJobsClosed,
	IM_TSPJobsClosed,
	IM_NRCJobsClosed,
	IM_CWRJobsClosed,
	IM_DTFJobsClosed,
	IM_EnterpriseIncident, 
	IM_DeliveranceIncident, 
	IM_CargoWiseIncident,
	IM_OtherProductIncident
FROM
	dbo.Clientcsfn__IncidentMainClosedAnalysis
	(
		@IM_DateAddedFrom,
		@IM_DateAddedTo,
		@IM_CloseTimeUtcFrom,
		@IM_CloseTimeUtcTo,
		@IM_GG_TeamList, 
		@IM_GS_CustomerServiceContactList, 
		@IM_Product,
		@IM_ProductArea,
		@IM_ResolutionCode,
		@IM_OH_ClientList,
		@CountryPKs
	)

-------------------------------------
-- Other Product Incident Summary
-------------------------------------

DECLARE @OtherProductAnalysisResult TABLE
(
	IM_GS_CustomerServiceContact UNIQUEIDENTIFIER,
	IM_OtherProductIncidentDetails VARCHAR(500)
)

INSERT INTO @OtherProductAnalysisResult (IM_GS_CustomerServiceContact, IM_OtherProductIncidentDetails)
SELECT DISTINCT t1.IM_GS_CustomerServiceContact, 
    (
        SELECT STRING_AGG(CONCAT(IM_ProductDescription, '(', CONVERT(VARCHAR(100), ProductCount), ')'), ', ')
        FROM 
        (
            SELECT IM_GS_CustomerServiceContact, IM_ProductDescription, COUNT(*) as ProductCount
            FROM @MainAnalysisResult t2
            WHERE 
            t2.IM_GS_CustomerServiceContact = t1.IM_GS_CustomerServiceContact
            AND t2.IM_Product NOT IN ('ENT', 'DLV', 'CAR')
            GROUP BY IM_GS_CustomerServiceContact, IM_ProductDescription
        ) AS subquery
    )
FROM @MainAnalysisResult t1

-------------------------------------
-- Result Table
-------------------------------------

INSERT INTO @Result
SELECT
	IM_GS_CustomerServiceContact,
	SUM(IM_JobCount) as IM_JobCount,
	SUM(IM_OTHJobsClosed) AS IM_OTHJobsClosed,
	SUM(IM_PDMJobsClosed) AS IM_PDMJobsClosed,
	SUM(IM_PFRJobsClosed) AS IM_PFRJobsClosed,
	SUM(IM_PassedJobsClosed) AS IM_PassedJobsClosed,
	SUM(IM_SRSJobsClosed) AS IM_SRSJobsClosed,
	SUM(IM_SYSJobsClosed) AS IM_SYSJobsClosed,
	SUM(IM_TRNJobsClosed) AS IM_TRNJobsClosed,
	SUM(IM_TRMJobsClosed) AS IM_TRMJobsClosed,
	SUM(IM_UPDJobsClosed) AS IM_UPDJobsClosed,
	SUM(IM_URPJobsClosed) AS IM_URPJobsClosed,
	SUM(IM_TSPJobsClosed) AS IM_TSPJobsClosed,
	SUM(IM_NRCJobsClosed) AS IM_NRCJobsClosed,
	SUM(IM_CWRJobsClosed) AS IM_CWRJobsClosed,
	SUM(IM_DTFJobsClosed) AS IM_DTFJobsClosed,
	SUM(IM_EnterpriseIncident) AS IM_EnterpriseIncident, 
	SUM(IM_DeliveranceIncident) AS IM_DeliveranceIncident, 
	SUM(IM_CargoWiseIncident) AS IM_CargoWiseIncident,
	SUM(IM_OtherProductIncident) AS IM_OtherProductIncident,
	''
FROM
	@MainAnalysisResult
GROUP BY
	IM_GS_CustomerServiceContact

UPDATE @Result
SET result.IM_OtherProductIncidentDetails = other.IM_OtherProductIncidentDetails
FROM
	@Result result
	JOIN @OtherProductAnalysisResult other ON result.IM_GS_CustomerServiceContact = other.IM_GS_CustomerServiceContact

RETURN
END",
	"DROP FUNCTION Clientcsfn__CSIClosuresStaffSummary", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region ClientReport_CSIClosuresEventStaffSummary

			new DatabaseViewAndRoutineCreateScript("ClientReport_CSIClosuresEventStaffSummary", @"CREATE FUNCTION ClientReport_CSIClosuresEventStaffSummary
	(
		@IM_DateAddedFrom DATETIME,
		@IM_DateAddedTo DATETIME,
		@IM_CloseTimeUtcFrom DATETIME,
		@IM_CloseTimeUtcTo DATETIME,
		@IM_GG_TeamList VARCHAR(4000), 
		@IM_GS_CustomerServiceContactList VARCHAR(4000), 
		@IM_Product CHAR(3),
		@IM_ProductArea CHAR(3),
		@IM_ResolutionCode CHAR(3),
		@IM_OH_ClientList VARCHAR(4000),
		@CountryPKs VARCHAR(4000)
	)
	RETURNS TABLE 
	AS
	RETURN
	SELECT
		GLBStaff.GS_Code, 
		GS_FullName, 
		IM_JobCount, 
		IM_SL_EventCount,
		dbo.Clientfn_WorkingDaysBetween(@IM_CloseTimeUtcFrom, dateadd(ss, -1, @IM_CloseTimeUtcTo), GS_GB_HomeBranch, GLBStaff.GS_PK) as IM_GS_WorkingDaysInPeriod
	FROM
		dbo.Clientcsfn__CSIClosuresStaffSummary
				(		
						@IM_DateAddedFrom ,
						@IM_DateAddedTo ,
						@IM_CloseTimeUtcFrom ,
						@IM_CloseTimeUtcTo ,
						@IM_GG_TeamList ,
						@IM_GS_CustomerServiceContactList , 
						@IM_Product ,
						@IM_ProductArea,
						@IM_ResolutionCode ,
						@IM_OH_ClientList ,
						@CountryPKs 
				)
		FULL JOIN dbo.Clientcsfn__CSIClosuresEventStaffSummary
				(
						@IM_DateAddedFrom ,
						@IM_DateAddedTo ,
						@IM_CloseTimeUtcFrom ,
						@IM_CloseTimeUtcTo ,
						@IM_GG_TeamList ,
						@IM_GS_CustomerServiceContactList , 
						@IM_Product ,
						@IM_ProductArea,
						@IM_ResolutionCode ,
						@IM_OH_ClientList ,
						@CountryPKs
				)
		on
		Clientcsfn__CSIClosuresEventStaffSummary.GS_PK = Clientcsfn__CSIClosuresStaffSummary.IM_GS_CustomerServiceContact
		INNER JOIN dbo.GLBStaff ON Clientcsfn__CSIClosuresEventStaffSummary.GS_PK = GLBStaff.GS_PK
	WHERE
		dbo.Clientfn_WorkingDaysBetween(@IM_CloseTimeUtcFrom, dateadd(ss, -1, @IM_CloseTimeUtcTo), GS_GB_HomeBranch, GLBStaff.GS_PK) > 0
", "drop FUNCTION ClientReport_CSIClosuresEventStaffSummary", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_CSIClosuresClientDispositionSummary

			new DatabaseViewAndRoutineCreateScript("ClientReport_CSIClosuresClientDispositionSummary", @"CREATE FUNCTION ClientReport_CSIClosuresClientDispositionSummary
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_CloseTimeUtcFrom DATETIME,
	@IM_CloseTimeUtcTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@IM_ProductArea CHAR(3),
	@IM_ResolutionCode CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000),
	@CompanyPK UNIQUEIDENTIFIER
)
RETURNS TABLE 
AS
RETURN
SELECT
	OrgHeader.OH_Code,
	OH_FullName,
	LA_SiteLiveDate,
	CoreUsers,
	GS_Code, 
	GS_FullName,
	IM_JobCount,
	IM_OTHJobsClosed,
	IM_PDMJobsClosed,
	IM_PFRJobsClosed,
	IM_PassedJobsClosed,
	IM_SRSJobsClosed,
	IM_SYSJobsClosed,
	IM_TRNJobsClosed,
	IM_TRMJobsClosed,
	IM_UPDJobsClosed,
	IM_URPJobsClosed,
	IM_TSPJobsClosed,
	IM_NRCJobsClosed,
	IM_CWRJobsClosed,
	IM_DTFJobsClosed,
	IM_EnterpriseIncident,
	IM_DeliveranceIncident,
	IM_CargoWiseIncident,
	IM_OtherProductIncident,
	IM_OtherProductIncidentDetails
FROM
	Clientcsfn__CSIClosuresClientDispositionSummary
	(
		@IM_DateAddedFrom, 
		@IM_DateAddedTo,
		@IM_CloseTimeUtcFrom, 
		@IM_CloseTimeUtcTo,
		@IM_GG_TeamList, 
		@IM_GS_CustomerServiceContactList, 
		@IM_Product,
		@IM_ProductArea,
		@IM_ResolutionCode,
		@IM_OH_ClientList,
		@CountryPKs	
	)
	INNER JOIN dbo.OrgHeader ON IM_OH_Client = OH_PK
	LEFT JOIN 
	(
		SELECT OH_PK, ISNULL(CompanyRM1.O8_GS_NKPersonResponsible, ISNULL(CompanyRM2.O8_GS_NKPersonResponsible, ISNULL(GlobalRM1.O8_GS_NKPersonResponsible, GlobalRM2.O8_GS_NKPersonResponsible))) AS RMCode
		FROM 
			dbo.OrgHeader
			LEFT JOIN dbo.OrgStaffAssignments AS CompanyRM1 ON CompanyRM1.O8_OH = OH_PK AND CompanyRM1.O8_Role = 'RM1' AND CompanyRM1.O8_GC = @CompanyPK
			LEFT JOIN dbo.OrgStaffAssignments AS CompanyRM2 ON CompanyRM2.O8_OH = OH_PK AND CompanyRM2.O8_Role = 'RM1' AND CompanyRM2.O8_GC = @CompanyPK
			LEFT JOIN dbo.OrgStaffAssignments AS GlobalRM1 ON GlobalRM1.O8_OH = OH_PK AND GlobalRM1.O8_Role = 'RM1' AND GlobalRM1.O8_GC IS NULL
			LEFT JOIN dbo.OrgStaffAssignments AS GlobalRM2 ON GlobalRM2.O8_OH = OH_PK AND GlobalRM2.O8_Role = 'RM1' AND GlobalRM2.O8_GC IS NULL
	) RMAssignment ON OrgHeader.OH_PK = RMAssignment.OH_PK
	LEFT JOIN dbo.GlbStaff ON RMAssignment.RMCode = GS_Code
	LEFT JOIN 
	(
		SELECT OH_PK, MAX(LA_SiteLiveDate) AS LA_SiteLiveDate
		FROM 
			dbo.OrgHeader
			JOIN dbo.LicenceCompany ON LC_OH = OH_PK
			JOIN dbo.licenceheader ON LA_LC = LC_PK
			JOIN dbo.LicenceDatabase ON LA_LD = LD_PK
		WHERE
			OH_IsActive = 1 
			AND LA_IsActive = 1
			AND LD_IsActive = 1
			AND LD_LicenceType = 'PRD'
		GROUP BY OH_PK
	) MostRecentGoLiveDate ON OrgHeader.OH_PK = MostRecentGoLiveDate.OH_PK
	LEFT JOIN 
	(
		SELECT OH_PK = LC_OH, CoreUsers = MAX(CoreUsers)
		FROM
			(
				SELECT
					LCC_LD,
					LCC_OH,
					CoreUsers = COUNT(DISTINCT LX2_LS)
				FROM
					dbo.EdiLicenceUsage
					JOIN dbo.ClientCompany ON LX2_LCC = LCC_PK
				WHERE
					LX2_ModuleCode = 'COR'
					AND LX2_Period >= (YEAR(@IM_CloseTimeUtcFrom) * 100 + MONTH(@IM_CloseTimeUtcFrom)) and LX2_Period <= (YEAR(@IM_CloseTimeUtcTo) * 100 + MONTH(@IM_CloseTimeUtcTo))
				GROUP BY 
					LCC_LD,
					LCC_OH
			) CoreUsage
			JOIN dbo.LicenceDatabase ON LCC_LD = LD_PK
			JOIN dbo.LicenceCompany ON LCC_OH = LC_OH
			JOIN dbo.LicenceHeader ON LA_LD = LD_PK AND LA_LC = LC_PK
		WHERE 
			LA_IsActive = 1 
			AND LD_IsActive = 1
		GROUP BY LC_OH
	) ProductionCoreUserSum ON OrgHeader.OH_PK = ProductionCoreUserSum.OH_PK
", "DROP FUNCTION ClientReport_CSIClosuresClientDispositionSummary", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_CSIOpenItemsStaffSummary

			new DatabaseViewAndRoutineCreateScript("ClientReport_CSIOpenItemsStaffSummary", @"CREATE FUNCTION ClientReport_CSIOpenItemsStaffSummary
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@IM_ProductArea CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000)
)
RETURNS TABLE 
AS
RETURN
SELECT
	GS_Code,
	ISNULL(GS_FullName, '**Unassigned**') as GS_FullName, 
	Clientcsfn__CSIOpenItemsStaffSummary.IM_JobCount,
	IM_UpTo1DayOld,
	IM_UpTo1WeekOld,
	IM_UpTo1MonthOld,
	IM_Old,
	IM_CR1,
	IM_CR2,
	IM_CR3,
	IM_CR4,
	IM_CR5,
	IM_CR6, 
	IM_OPNAUCJobsOpen,
	IM_OPNADDJobsOpen,
	IM_WRKAUCJobsOpen,
	IM_REQJobsOpen,
	IM_STEJobsOpen,
	IM_WRKJobsOpen,
	IM_CBKJobsOpen,
	IM_UPOJobsOpen,
	IM_EnterpriseIncident,
	IM_DeliveranceIncident,
	IM_CargoWiseIncident,
	IM_OtherProductIncident,
	IM_OtherProductIncidentDetails,
	1 As AccumulativeTotalHack	
FROM
	dbo.Clientcsfn__CSIOpenItemsStaffSummary
		(
			@IM_DateAddedFrom,
			@IM_DateAddedTo,
			@IM_GG_TeamList, 
			@IM_GS_CustomerServiceContactList, 
			@IM_Product,
			@IM_ProductArea,
			@IM_OH_ClientList,
			@CountryPKs
		)
	LEFT JOIN dbo.GLBStaff ON IM_GS_CustomerServiceContact = GS_PK", "DROP FUNCTION ClientReport_CSIOpenItemsStaffSummary", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_CSIClosuresStaffSummary

			new DatabaseViewAndRoutineCreateScript("ClientReport_CSIClosuresStaffSummary", @"CREATE FUNCTION ClientReport_CSIClosuresStaffSummary
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_CloseTimeUtcFrom DATETIME,
	@IM_CloseTimeUtcTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@IM_ProductArea CHAR(3),
	@IM_ResolutionCode CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000)
)
RETURNS TABLE 
AS
RETURN
SELECT
	GS_Code,
	GS_FullName, 
	Clientcsfn__CSIClosuresStaffSummary.IM_JobCount,
	IM_OTHJobsClosed,
	IM_PDMJobsClosed,
	IM_PFRJobsClosed,
	IM_PassedJobsClosed,
	IM_SRSJobsClosed,
	IM_SYSJobsClosed,
	IM_TRNJobsClosed,
	IM_TRMJobsClosed,
	IM_UPDJobsClosed,
	IM_URPJobsClosed,
	IM_TSPJobsClosed,
	IM_NRCJobsClosed,
	IM_CWRJobsClosed,
	IM_DTFJobsClosed,
	IM_EnterpriseIncident,
	IM_DeliveranceIncident,
	IM_CargoWiseIncident,
	IM_OtherProductIncident,
	IM_OtherProductIncidentDetails,
	1 As AccumulativeTotalHack
FROM
	Clientcsfn__CSIClosuresStaffSummary
		(
			@IM_DateAddedFrom,
			@IM_DateAddedTo,
			@IM_CloseTimeUtcFrom,
			@IM_CloseTimeUtcTo,
			@IM_GG_TeamList, 
			@IM_GS_CustomerServiceContactList, 
			@IM_Product,
			@IM_ProductArea,
			@IM_ResolutionCode,
			@IM_OH_ClientList,
			@CountryPKs
		)
	LEFT JOIN dbo.GLBStaff ON IM_GS_CustomerServiceContact = GS_PK", "DROP FUNCTION ClientReport_CSIClosuresStaffSummary", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_CSIClosuresProductModuleSummary

			new DatabaseViewAndRoutineCreateScript("ClientReport_CSIClosuresProductModuleSummary", @"CREATE FUNCTION ClientReport_CSIClosuresProductModuleSummary
(
	@IM_DateAddedFrom DATETIME,
	@IM_DateAddedTo DATETIME,
	@IM_CloseTimeUtcFrom DATETIME,
	@IM_CloseTimeUtcTo DATETIME,
	@IM_GG_TeamList VARCHAR(4000), 
	@IM_GS_CustomerServiceContactList VARCHAR(4000), 
	@IM_Product CHAR(3),
	@IM_ProductArea CHAR(3),
	@IM_ResolutionCode CHAR(3),
	@IM_OH_ClientList VARCHAR(4000),
	@CountryPKs VARCHAR(4000)
)
RETURNS TABLE 
AS
RETURN
SELECT
	IM_Product AS IM_ProductCode,
	IncidentProduct.ProductDescription AS IM_ProductDescription,
	IM_Module as IM_ModuleCode,
	COALESCE(IncidentProductAndModules.ModuleDescription, IncidentCr8Modules.ModuleDescription, IncidentCr9Modules.ModuleDescription) AS IM_ModuleDescription,
	SUM(IM_JobCount) AS IM_JobCount,
	SUM(IM_OTHJobsClosed) AS IM_OTHJobsClosed,
	SUM(IM_PDMJobsClosed) AS IM_PDMJobsClosed,
	SUM(IM_PFRJobsClosed) AS IM_PFRJobsClosed,
	SUM(IM_PassedJobsClosed) AS IM_PassedJobsClosed,
	SUM(IM_SRSJobsClosed) AS IM_SRSJobsClosed,
	SUM(IM_SYSJobsClosed) AS IM_SYSJobsClosed,
	SUM(IM_TRNJobsClosed) AS IM_TRNJobsClosed,
	SUM(IM_TRMJobsClosed) AS IM_TRMJobsClosed,
	SUM(IM_UPDJobsClosed) AS IM_UPDJobsClosed,
	SUM(IM_URPJobsClosed) AS IM_URPJobsClosed,
	SUM(IM_TSPJobsClosed) AS IM_TSPJobsClosed,
	SUM(IM_NRCJobsClosed) AS IM_NRCJobsClosed,
	SUM(IM_CWRJobsClosed) AS IM_CWRJobsClosed,
	SUM(IM_DTFJobsClosed) AS IM_DTFJobsClosed,
	SUM(IM_IncidentOpenedWorkHours) AS IM_IncidentOpenedWorkHours
FROM
	dbo.Clientcsfn__IncidentMainClosedAnalysis
		(
			@IM_DateAddedFrom,
			@IM_DateAddedTo,
			@IM_CloseTimeUtcFrom,
			@IM_CloseTimeUtcTo,
			@IM_GG_TeamList, 
			@IM_GS_CustomerServiceContactList, 
			@IM_Product,
			@IM_ProductArea,
			@IM_ResolutionCode,
			@IM_OH_ClientList,
			@CountryPKs	
		)
	JOIN dbo.ClientReport_GetAllProductsFromRegistry() IncidentProduct ON IM_Product = IncidentProduct.ProductCode
	LEFT JOIN dbo.ClientReport_GetAllProductsAndModulesFromRegistry() IncidentProductAndModules
		ON IM_Priority NOT IN ('CR8', 'CR9')
		AND IM_PRODUCT = IncidentProductAndModules.ProductCode
		AND IM_Module = IncidentProductAndModules.ModuleCode
	LEFT JOIN dbo.ClientReport_GetAllCr8ModulesFromRegistry() IncidentCr8Modules
		ON IM_Priority = 'CR8'
		AND IM_Module = IncidentCr8Modules.ModuleCode
		AND IM_Product = IncidentCr8Modules.ProductCode
	LEFT JOIN dbo.ClientReport_GetAllCr9ModulesFromRegistry() IncidentCr9Modules
		ON IM_Priority = 'CR9'
		AND IM_Module = IncidentCr9Modules.ModuleCode
		AND IM_Product = IncidentCr9Modules.ProductCode
GROUP BY
	IM_Product,
	IncidentProduct.ProductDescription,
	IM_Module,
	COALESCE(IncidentProductAndModules.ModuleDescription, IncidentCr8Modules.ModuleDescription, IncidentCr9Modules.ModuleDescription)", "DROP FUNCTION ClientReport_CSIClosuresProductModuleSummary", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Clientvw_Report_IncidentBilling

			new DatabaseViewAndRoutineCreateScript("Clientvw_Report_IncidentBilling", @"CREATE VIEW	Clientvw_Report_IncidentBilling
AS
SELECT
	IM_IncidentNumber AS IncidentNumber,
	OH_Code AS ClientCode,
	OH_FullNAme AS ClientName,
	IM_Description AS Description,
	IM_ResolutionCode AS Resolution,	
	IM_CloseTimeUtc AS CloseDate,	
	IM_ReasonableHours AS ChargeableTime,
	95 AS Rate,
	95*IM_ReasonableHours AS Total,
	GS_FullName AS CustomerServiceContact,
	GS_PK AS CustomerServiceContactPK,
	RN_PK AS Country,
	RN_Desc AS CountryName
FROM 
	dbo.IncidentMain	
	LEFT JOIN dbo.OrgHeader ON OrgHeader.OH_PK = IncidentMain.IM_OH_Client
	LEFT JOIN dbo.RefUNLOCO ON RefUNLOCO.RL_Code = OrgHeader.OH_RL_NKClosestPort
	LEFT JOIN dbo.RefCountry ON RefCountry.RN_Code = RefUNLOCO.RL_RN_NKCountryCode
	LEFT JOIN dbo.GlbStaff ON GlbStaff.GS_Code = IncidentMain.IM_GS_NKCustServiceContact
WHERE
	IncidentMain.IM_ChargableWork = 'Y'", "DROP VIEW Clientvw_Report_IncidentBilling", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region Clientvw_Report_LicenceExpiry
			new DatabaseViewAndRoutineCreateScript("Clientvw_Report_LicenceExpiry", @"CREATE VIEW Clientvw_Report_LicenceExpiry
AS
SELECT 
	OH_Code
	,OH_FullName
	,LD_ServerCode
	,LD_LicenceExpiry 
FROM 
	dbo.LicenceDatabase 
	join dbo.LicenceHeader on LA_LD = LD_PK 
	join dbo.LicenceCompany on LA_LC = LC_PK
	join dbo.OrgHeader on LC_OH = OH_PK
WHERE 
	LD_LicenceType = 'PRD'
	AND LD_Product in ('ENT', 'CW1', 'PRW') and OH_IsActive = 1", "DROP VIEW Clientvw_Report_LicenceExpiry", DbRoutineType.SqlViewTypeDesc),
		#endregion

		#region ClientReport_InstallationProjects

			new DatabaseViewAndRoutineCreateScript("ClientReport_InstallationProjects", @"CREATE FUNCTION ClientReport_InstallationProjects
(	
	@Status NVARCHAR(3),
	@Location NVARCHAR(2)
)
RETURNS TABLE
AS
RETURN
SELECT
	WKP_ProjectNumber
	,OA_OH as ClientPK
	,OH_Code
	,OH_FullName
	,OA_RL_NKRelatedPortCode
	,CompanyResponsiblePerson.GS_PK as SalStaffPK
	,CompanyResponsiblePerson.GS_Code as SalStaff
	,WKP_Type
	,WKP_SubType
	,WKP_GS_NKProjectManager as ProjMan
	,WKP_Summary
	,WKP_Status
	,WKP_SystemCreateTimeUtc
	,CWP_PlannedInstall
	,CWP_InstallDate
	,CWP_CallbackBy

	,G2_InitialStartDate
	,LA_EstimatedLiveDate
	,LA_SiteLiveDate
	,CASE WHEN LA_SiteLiveDate >= WKP_SystemCreateTimeUtc THEN
			LA_SiteLiveDate - WKP_SystemCreateTimeUtc
		ELSE
			NULL
	END as NumOfDay
FROM
	dbo.WorkProject 
	LEFT JOIN dbo.OrgAddress on WKP_OA_ClientAddress = OA_PK
	LEFT JOIN dbo.OrgHeader on OA_OH = OH_PK
	LEFT JOIN 
	(
		OrgStaffAssignments AS CompanyStaffAssignments
		INNER JOIN dbo.GlbStaff AS CompanyResponsiblePerson ON CompanyStaffAssignments.O8_GS_NKPersonResponsible = CompanyResponsiblePerson.GS_Code
	)
		ON	CompanyStaffAssignments.O8_OH = OH_PK 
			AND CompanyStaffAssignments.O8_Role = 'SAL'
			AND CompanyStaffAssignments.O8_Department = 'ALL'			
	LEFT JOIN dbo.ClientWorkProject on CWP_WKP = WKP_PK
	LEFT JOIN dbo.LicenceHeader on CWP_LA=LA_PK
	LEFT JOIN 
	(
		SELECT 
			G2_LA
			,MIN(G2_InitialStartDate) AS G2_InitialStartDate
		FROM 
			dbo.GlbTrainingCourse
		WHERE
			 G2_InitialStartDate IS NOT NULL
		GROUP BY 
			G2_LA
	) AS TrainingCourse 
		ON G2_LA=CWP_LA
WHERE (@Status = '' OR (@Status = 'NCM' AND (WKP_Status = 'OPN' OR WKP_Status = 'WRK')) OR WKP_Status = @Status)
	AND (@Location = '' or @Location = LEFT(OA_RL_NKRelatedPortCode,2))", "DROP FUNCTION ClientReport_InstallationProjects", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_InstallationProjectsV2

			new DatabaseViewAndRoutineCreateScript("ClientReport_InstallationProjectsV2", @"CREATE FUNCTION ClientReport_InstallationProjectsV2
(	
	@Status NVARCHAR(3),
	@Location NVARCHAR(2),
	@RelatedActivityType AS varchar(3)
)
RETURNS TABLE
AS
RETURN
SELECT
	WKP_ProjectNumber
	,OA_OH as ClientPK
	,OH_Code
	,OH_FullName
	,OA_RL_NKRelatedPortCode
	,CompanyResponsiblePerson.GS_PK as SalStaffPK
	,CompanyResponsiblePerson.GS_Code as SalStaff
	,WKP_Type
	,WKP_SubType
	,WKP_GS_NKProjectManager as ProjMan
	,WKP_Summary
	,WKP_Status
	,WKP_SystemCreateTimeUtc
	,CWP_PlannedInstall
	,CWP_InstallDate
	,CWP_CallbackBy

	,G2_InitialStartDate
	,LA_EstimatedLiveDate
	,LA_SiteLiveDate
	,CASE WHEN LA_SiteLiveDate >= WKP_SystemCreateTimeUtc THEN
			LA_SiteLiveDate - WKP_SystemCreateTimeUtc
		ELSE
			NULL
	END as NumOfDay,
	CASE WHEN RelatedActivityCount > 0 THEN 'Y' ELSE 'N' END AS HasSalesRelation
FROM
	dbo.WorkProject 
	LEFT JOIN dbo.OrgAddress on WKP_OA_ClientAddress = OA_PK
	LEFT JOIN dbo.OrgHeader on OA_OH = OH_PK
	LEFT JOIN 
	(
		dbo.OrgStaffAssignments AS CompanyStaffAssignments
		INNER JOIN dbo.GlbStaff AS CompanyResponsiblePerson ON CompanyStaffAssignments.O8_GS_NKPersonResponsible = CompanyResponsiblePerson.GS_Code
	)
		ON	CompanyStaffAssignments.O8_OH = OH_PK 
			AND CompanyStaffAssignments.O8_Role = 'SAL'
			AND CompanyStaffAssignments.O8_Department = 'ALL'			
	LEFT JOIN dbo.ClientWorkProject on CWP_WKP = WKP_PK
LEFT JOIN dbo.GetSalesRelationActivityData(NULLIF(@RelatedActivityType, 'ANY')) RelatedActivityData
		ON RelatedActivityData.ActivityID = WKP_PK
	LEFT JOIN dbo.LicenceHeader on CWP_LA=LA_PK
	LEFT JOIN 
	(
		SELECT 
			G2_LA
			,MIN(G2_InitialStartDate) AS G2_InitialStartDate
		FROM 
			dbo.GlbTrainingCourse
		WHERE
			 G2_InitialStartDate IS NOT NULL
		GROUP BY 
			G2_LA
	) AS TrainingCourse 
		ON G2_LA=CWP_LA
WHERE (@Status = '' OR (@Status = 'NCM' AND (WKP_Status = 'OPN' OR WKP_Status = 'WRK')) OR WKP_Status = @Status)
	AND (@Location = '' or @Location = LEFT(OA_RL_NKRelatedPortCode,2))
	AND ((@RelatedActivityType IS NULL) OR (RelatedActivityCount > 0))", "DROP FUNCTION ClientReport_InstallationProjectsV2", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Clientvw_FeatureRequestInvoices

	new DatabaseViewAndRoutineCreateScript("Clientvw_FeatureRequestInvoices", @"
CREATE VIEW Clientvw_FeatureRequestInvoices
AS
SELECT
	IM_IncidentNumber		IncidentNumber
	,IM_Description			IncidentDescription
	,IM_Module				ModuleCode
	,OH_Code				ClientCode
	,AH_TransactionNum		TransactionNumber
	,AH_OutstandingAmount	OutstandingAmount
	,RX_Code				CurrencyCode
	,AH_ExchangeRate		ExchangeRate
	,AH_FullyPaidDate		FullyPaidDate
	,AH_DueDate				DueDate
	,GB_BranchName			BranchName
	,GC_Name				CompanyName
	,CASE 
		WHEN AH_OutstandingAmount>0 AND AH_FullyPaidDate IS NULL THEN 'Y'
		ELSE 'N'
	END AS IsOutstanding
FROM
	dbo.AccTransactionHeader
	INNER JOIN dbo.JobHeader ON JH_PK = AH_JH
	INNER JOIN dbo.IncidentMain 
		ON 
		IM_PK = JH_ParentID
		AND JH_ParentTableCode = 'IM'
	LEFT JOIN dbo.OrgHeader ON OH_PK = IM_OH_Client	
	LEFT JOIN dbo.RefCurrency ON RX_Code = AH_RX_NKTransactionCurrency		
	LEFT JOIN dbo.GlbBranch ON GB_PK = AH_GB
	LEFT JOIN dbo.GlbCompany ON GC_PK = GB_GC
WHERE
	IM_IncidentType = 'INC'
	AND AH_TransactionType = 'INV'", "DROP VIEW Clientvw_FeatureRequestInvoices", DbRoutineType.SqlViewTypeDesc),

		#endregion

		BillingUsageSchema.ViewLicenceDatabaseOwnerScript(),
		BillingUsageSchema.ViewClientCompanyLicenceScript(),

		#region Clientvw_OnDemandLicenceUsage

	new DatabaseViewAndRoutineCreateScript("Clientvw_OnDemandLicenceUsage", @"
CREATE VIEW Clientvw_OnDemandLicenceUsage
AS
SELECT
	LicenceModule = LX2_ModuleCode,
	OrgPK = OrgHeader.OH_PK,
	OrgCode = OrgHeader.OH_Code,
	OrgName = OrgHeader.OH_FullName,
	LicenceEnterprisePK = LE_PK,
	LicenceEnterpriseCode = LE_EnterpriseCode,
	LicenceServerCode = LD_ServerCode,
	LicenceServerType = LD_LicenceType,
	UsageTime = LX2_FirstUsageUtc,
	LastUsageTime = LX2_LastUsageUtc,
	UsageCount = LX2_UsageCount,
	StaffCode = LS_Code,
	StaffName = LS_FullName,
	LicenceType = LX2_LicenceMode,
	Period = LX2_Period,
	PeriodAsDate = DATEFROMPARTS(LX2_Period / 100, LX2_Period % 100, 1),
	GoLiveFlag = CASE WHEN LA_AgreedLiveDate <= LX2_LastUsageUtc THEN 'POS' ELSE 'PRE' END,
	ClientCompanyCode = LCC_Code
FROM
	dbo.EdiLicenceUsage
	JOIN dbo.ClientCompany ON LX2_LCC = LCC_PK
	JOIN dbo.ClientStaff ON LX2_LS = LS_PK AND LS_LD = LCC_LD
	JOIN dbo.LicenceDatabase ON LCC_LD = LD_PK
	JOIN dbo.LicenceEnterprise ON LD_LE = LE_PK
	JOIN dbo.EdiViewClientCompanyLicence vw on vw.LCC_PK = ClientCompany.LCC_PK
	JOIN dbo.LicenceHeader on vw.LA_PK = LicenceHeader.LA_PK
	JOIN dbo.OrgHeader ON vw.LC_OH = OrgHeader.OH_PK
",
									 "DROP VIEW Clientvw_OnDemandLicenceUsage",
									 DbRoutineType.SqlViewTypeDesc),

		#endregion

		BillingUsageSchema.ViewClientCompanyUniqueCodesScript(),
		BillingUsageSchema.ViewClientCompanyCodeHistoryScript(),
		BillingUsageSchema.ViewDatabaseCompanyUniqueCodesScript(),
		BillingUsageSchema.CompanyCodeToClientCompanyScript(),
		BillingUsageSchema.EdiClientCompanyMergeScript(),
		BillingUsageSchema.ViewBillableUsageScript(),
		BillingUsageSchema.EdiGetOdmPriceHeadersForDateScript(),
		DepositSchema.EdiViewDepositBalanceScript(),
		DepositSchema.TriggerAccTransactionHeaderToDepositScript(),
		DepositSchema.TriggerEdiDepositAdjustScript(),
		IncidentSchema.TriggerJobConversationMessageScript(),
		IncidentSchema.EdiViewIncidentStatusChangeScript(),

		#region Clienttg_INS_HelpErrorLogKey

			new DatabaseViewAndRoutineCreateScript("Clienttg_INS_HelpErrorLogKey", @"
CREATE TRIGGER Clienttg_INS_HelpErrorLogKey ON HelpErrorLogKey FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @existingIssueNumber VARCHAR(20)
	DECLARE @duplicateIssueNumber VARCHAR(20)
	SELECT TOP 1 @existingIssueNumber = existingLog.HE_IssueNumber, @duplicateIssueNumber = insertedLog.HE_IssueNumber
	FROM inserted
	JOIN dbo.HelpErrorLog insertedLog on inserted.HK_HE = insertedLog.HE_PK
	JOIN dbo.HelpErrorLogKey existingKey on inserted.HK_HashCode = existingKey.HK_HashCode AND inserted.HK_Key = existingKey.HK_Key AND inserted.HK_PK != existingKey.HK_PK
	JOIN dbo.HelpErrorLog existingLog ON existingKey.HK_HE = existingLog.HE_PK
	IF @existingIssueNumber IS NOT NULL
	BEGIN
		DECLARE @error VARCHAR(255)
		SET @error = 'Duplicate HK_Key NOT allowed on HelpErrorLogKey. Existing issue number: ' + @existingIssueNumber + '. Duplicate issue number: ' + @duplicateIssueNumber
		RAISERROR(@error, 16, 1)
		ROLLBACK TRANSACTION
		RETURN
	END
END",
				"DROP TRIGGER Clienttg_INS_HelpErrorLogKey",
				DbRoutineType.SqlTriggerTypeDesc),

		#endregion

		#region ClientReport_ConsultantSurveyReportGetComments

			new DatabaseViewAndRoutineCreateScript("ClientReport_ConsultantSurveyReportGetComments", @"CREATE FUNCTION ClientReport_ConsultantSurveyReportGetComments()
RETURNS TABLE
AS
RETURN
SELECT
	HZ_G8,
	REPLACE(VoteExamSurveyQuestion.HY_Question, '<b>', '') + CHAR(10) +
	HZ_AnswerComment + CHAR(10) + CHAR(10)
	AS Comment
	FROM dbo.VoteExamSurveyQuestion
	INNER JOIN dbo.VoteExamSurveyAnswer ON VoteExamSurveyQuestion.HY_PK = VoteExamSurveyAnswer.HZ_HY
	WHERE
	VoteExamSurveyQuestion.HY_G0 IN ('7D3BBAD1-8CCF-43D5-AC7C-224AB1FEE7CD')
	AND HY_AnswerType = 'FRT' ", "DROP FUNCTION ClientReport_ConsultantSurveyReportGetComments", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_CustomerServiceStaffActivity

			new DatabaseViewAndRoutineCreateScript("ClientReport_CustomerServiceStaffActivity",
@"CREATE FUNCTION ClientReport_CustomerServiceStaffActivity(@LowerBoundDate datetime, @UpperBoundDate datetime, @Product char(3))
RETURNS @result TABLE
(
	GS_FullName nvarchar(256),
	GS_Code char(3),
	Taken int,
	TakenFromBacklog int,
	TakenOnSteQueue int,
	Total int
)
AS
BEGIN
SET @LowerBoundDate = ISNULL(@LowerBoundDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))
SET @UpperBoundDate = ISNULL(@UpperBoundDate, DATEADD(dd, 1, @LowerBoundDate))


DECLARE @temp TABLE
(
	IM_PK UNIQUEIDENTIFIER,
	IM_GS_NKCustServiceContact VARCHAR(3),
	IM_SystemCreateTimeUtc SMALLDATETIME,
	IM_OrderReceived SMALLDATETIME,
	IM_ResolutionCode VARCHAR(3)
)

INSERT INTO @temp
SELECT
	IM_PK,
	IM_GS_NKCustServiceContact,
	IM_SystemCreateTimeUtc,
	IM_OrderReceived,
	IM_ResolutionCode
FROM
	dbo.IncidentMain
WHERE
	IM_IncidentType = 'INC'
	AND (@Product IS NULL OR @Product = '' OR IM_Product = @Product)
	AND IM_OrderReceived >= @LowerBoundDate
	AND IM_OrderReceived < @UpperBoundDate

UPDATE
	@temp
SET
	IM_GS_NKCustServiceContact = LastCloseOrEscalateStaff.SL_GS_NKUser
FROM
	(
		SELECT SL_GS_NKUser, SL_Parent
		FROM
			(
				SELECT SL_GS_NKUser, SL_Parent, ROW_NUMBER() OVER (PARTITION BY SL_Parent ORDER BY SL_PostedTimeUtc DESC) AS EventSeq 
				FROM dbo.StmALog JOIN @temp ON SL_Parent = IM_PK
				WHERE SL_Reference LIKE 'Support - % to CLS' OR SL_Reference LIKE 'Status - % to CLS' OR SL_Reference LIKE 'Stage - SUP to %'
			) EventLog
		WHERE
			SL_GS_NKUser NOT IN ('~BP', 'ZZ') AND EventSeq = 1
	) LastCloseOrEscalateStaff JOIN @temp ON SL_Parent = IM_PK
WHERE
	IM_GS_NKCustServiceContact = ''


INSERT INTO @result	
SELECT
	*,
	Taken + TakenFromBacklog + TakenOnSteQueue AS Total
FROM
(
	SELECT
		GS_FullName,
		GS_Code,
		SUM(
			CASE
				WHEN IM_SystemCreateTimeUtc >= @LowerBoundDate
					AND IM_SystemCreateTimeUtc < @UpperBoundDate
					AND IM_ResolutionCode != 'STE' 					
						THEN 1
				ELSE 0
			END) AS Taken,
		SUM(
			CASE
				WHEN IM_SystemCreateTimeUtc < @LowerBoundDate
					AND IM_ResolutionCode != 'STE' 
						THEN 1
				ELSE 0
			END) AS TakenFromBacklog,
		SUM(
			CASE 
				WHEN IM_SystemCreateTimeUtc >= @LowerBoundDate 
					AND IM_SystemCreateTimeUtc < @UpperBoundDate
					AND IM_ResolutionCode = 'STE' 
						THEN 1
				ELSE 0
			END) AS TakenOnSteQueue
		
	FROM 
		@temp
		INNER JOIN dbo.GlbStaff  on GS_Code = IM_GS_NKCustServiceContact	
		
	GROUP BY 
		GS_Code, GS_FullName
) ReportData
WHERE Taken + TakenFromBacklog + TakenOnSteQueue > 0

RETURN

END;", "DROP FUNCTION ClientReport_CustomerServiceStaffActivity", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region Clientvw_Report_ClassroomRevenue

			new DatabaseViewAndRoutineCreateScript("Clientvw_Report_ClassroomRevenue", @"
CREATE VIEW Clientvw_Report_ClassroomRevenue AS
SELECT 
	GZ_PK AS PK,
	G3_SubjectName AS Course,
	GZ_GS_NKCoordinatorOfSession AS Trainer,
	GS_FullName AS TrainerName,
	GZ_OA_SessionDeliveryLocation AS Location,
	RTRIM(LTRIM(ISNULL(OA_Address1, ''))) AS Address1,
	RTRIM(LTRIM(ISNULL(OA_Address2, ''))) AS Address2,
	RTRIM(LTRIM(ISNULL(OA_City, ''))) AS City,
	RTRIM(LTRIM(ISNULL(OA_State, ''))) AS State,
	GZ_SessionStartTime AS StartDate,
	GZ_SessionEndTime AS EndDate,
	GZ_SessionCost AS Cost,
	GZ_SessionMaxAttendees,
	(SELECT COUNT(*) FROM dbo.GlbClassroomAttendee WHERE GX_GZ_ClassroomSession = GZ_PK) AS AttendeeCount,
		(SELECT COUNT(*) FROM dbo.GlbClassroomAttendee WHERE GX_GZ_ClassroomSession = GZ_PK AND GX_IsCancelled = 'Y') AS CancelledAttendeeCount,
	(GZ_SessionMaxAttendees - (SELECT COUNT(*) FROM dbo.GlbClassroomAttendee WHERE GX_GZ_ClassroomSession = GZ_PK AND GX_IsCancelled = 'N')) AS AvailablePlacesCount,
	(GZ_SessionCost * (SELECT COUNT(*) FROM dbo.GlbClassroomAttendee WHERE GX_GZ_ClassroomSession = GZ_PK AND GX_IsCancelled = 'N' AND UPPER(GX_Company) != 'EDI')) AS EstimatedRevenue

FROM dbo.GlbClassroomSession
	LEFT OUTER JOIN dbo.GlbStaff ON GS_Code = GZ_GS_NKCoordinatorOfSession
	LEFT OUTER JOIN dbo.OrgAddress ON OA_PK = GZ_OA_SessionDeliveryLocation
	INNER JOIN dbo.GlbClassroomSubject ON G3_PK = GZ_G3_TrainingSubject
WHERE GZ_G2_Course is NULL", "DROP VIEW Clientvw_Report_ClassroomRevenue", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region Clientvw_Report_ClassroomAttendees

			new DatabaseViewAndRoutineCreateScript("Clientvw_Report_ClassroomAttendees", @"
CREATE view Clientvw_Report_ClassroomAttendees as

SELECT 
	GZ_PK AS PK,
	G3_SubjectName AS Course,
	GZ_GS_NKCoordinatorOfSession AS Trainer,
	GS_FullName AS TrainerName,
	GlbStaff.GS_PK AS TrainerPK, 
	GZ_OA_SessionDeliveryLocation AS Location,
	RTRIM(LTRIM(ISNULL(OA_Address1, ''))) AS Address1,
	RTRIM(LTRIM(ISNULL(OA_Address2, ''))) AS Address2,
	RTRIM(LTRIM(ISNULL(OA_City, ''))) AS City,
	RTRIM(LTRIM(ISNULL(OA_State, ''))) AS State,
	GZ_SessionStartTime AS StartDate,
	GZ_SessionEndTime AS EndDate,
	GX_Name AS FullName,
	GX_Company AS Company,
	GX_IsBillingComplete AS IsOnWaitingList,
	GZ_SessionCost AS Cost,
	GZ_SessionMaxAttendees,
	locationCompany.OH_PK AS LocationOrgPK,
	locationCompany.OH_FullName AS LocationOrgName,
	filterCompany.OH_PK as CompanyPK,
	filterCompany.OH_FullName as CompanyName,
	(SELECT COUNT(*) FROM dbo.GlbClassroomAttendee WHERE GX_GZ_ClassroomSession = GZ_PK) AS AttendeeCount,
	(SELECT COUNT(*) FROM dbo.GlbClassroomAttendee WHERE GX_IsCancelled = 'Y') AS CancelledAttendeeCount,
	(GZ_SessionMaxAttendees - (SELECT COUNT(*) FROM dbo.GlbClassroomAttendee WHERE GX_GZ_ClassroomSession = GZ_PK)) AS AvailablePlacesCount,
	(GZ_SessionCost * (SELECT COUNT(*) FROM dbo.GlbClassroomAttendee WHERE GX_GZ_ClassroomSession = GZ_PK AND UPPER(GX_Company) != 'EDI')) AS EstimatedRevenue,
	convert(varchar, GlbClassroomSession.GZ_SessionStartTime, 112) + GlbClassroomSubject.G3_SubjectName + convert(varchar(36),GlbClassroomSession.GZ_PK) as GroupKey,
	LE_PK AS LicenceEnterprisePK
FROM 	dbo.GlbClassroomSession
	LEFT OUTER JOIN dbo.GlbClassroomAttendee ON GX_GZ_ClassroomSession = GZ_PK
	LEFT OUTER JOIN dbo.GlbStaff ON GS_Code = GZ_GS_NKCoordinatorOfSession
	LEFT OUTER JOIN dbo.OrgAddress ON OA_PK = GZ_OA_SessionDeliveryLocation
	INNER JOIN dbo.GlbClassroomSubject ON G3_PK = GZ_G3_TrainingSubject
	INNER JOIN dbo.OrgHeader AS locationCompany ON locationCompany.OH_PK = OrgAddress.OA_OH
	LEFT OUTER JOIN dbo.OrgHeader AS filterCompany ON filterCompany.OH_PK = GlbClassroomAttendee.GX_OH_ClientOrgNotForWebPublish
	LEFT OUTER JOIN dbo.LicenceCompany AS licenceCompany ON LC_OH= filterCompany.OH_PK
	LEFT OUTER JOIN dbo.LicenceEnterprise ON LE_PK = licenceCompany.LC_LE
WHERE 
	GZ_G2_Course is NULL AND
	((GX_IsCancelled IS NULL) OR (GX_IsCancelled = 'N'))", "DROP VIEW Clientvw_Report_ClassroomAttendees", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region ClientReport_CustomerServiceSurveyResult

			new DatabaseViewAndRoutineCreateScript("ClientReport_CustomerServiceSurveyResult",
@"CREATE FUNCTION ClientReport_CustomerServiceSurveyResult
(
	@CampaignPks VARCHAR(8000)
)
RETURNS TABLE
AS
RETURN
SELECT
	CampaignName,
	Received,
	TotalSent,
	Question,
	Average
FROM
(
	SELECT
		HY_G0,
		Question,
		SUM(Score * TotalCount) / CAST(SUM(TotalCount) AS SMALLMONEY) AS Average
	FROM
	(
		SELECT
			Parent.HY_G0,
			REPLACE(CAST(Parent.HY_Question AS VARCHAR(200)), '<b>', '') AS Question,
			COUNT(*) AS TotalCount,
			CASE (CAST(Choice.HY_Question AS VARCHAR(200)))
				WHEN 'Poor' THEN 1
				WHEN 'Average' THEN 2
				WHEN 'Good' THEN 3
				WHEN 'Excellent' THEN 4 
			END AS Score,
			CASE (CAST(Choice.HY_Question AS VARCHAR(200)))
				WHEN 'Poor' THEN 1
				WHEN 'Average' THEN 2
				WHEN 'Good' THEN 3
				WHEN 'Excellent' THEN 4
			END * COUNT(*) AS TotalScore
		FROM 
			dbo.VoteExamSurveyAnswer 
			INNER JOIN dbo.VoteExamSurveyQuestion Choice ON Choice.HY_PK = HZ_HY
			INNER JOIN dbo.VoteExamSurveyQuestion Parent ON 
				Parent.HY_QuestionOrder = Choice.HY_QuestionOrder 
				AND Choice.HY_G0 = Parent.HY_G0 
				AND Parent.HY_SubQuestionOrder = 0
		WHERE 
			CAST(Choice.HY_Question AS VARCHAR(200)) != '<b>Other Comments'
			AND HZ_G8 IN
			(
				SELECT 
					G8_PK 
				FROM 
					dbo.GlbCompanyCampaignItem
				WHERE 
					CHARINDEX(CONVERT(VARCHAR(36),G8_G0), @CampaignPks) > 0
					AND G8_ClosedDateUtc IS NOT NULL
			)
		GROUP BY
			CAST(Parent.HY_Question AS VARCHAR(200)), 
			CAST(Choice.HY_Question AS VARCHAR(200)),
			Parent.HY_G0
	) RawData
	GROUP BY
		Question,
		HY_G0
)QuestionAverage
INNER JOIN
(
	SELECT
		G0_PK,
		G0_CampaignName AS CampaignName,
		COUNT(G8_ClosedDateUtc) AS Received, 
		COUNT(*) AS TotalSent
	FROM 
		dbo.GlbCompanyCampaignItem 
		INNER JOIN dbo.Glbcompanycampaign ON G8_G0 = G0_PK
	WHERE
		CHARINDEX(CONVERT(VARCHAR(36),G0_PK), @CampaignPks) > 0
		AND G0_BroadcastVoteSurveyExam = 'SVY'
	GROUP BY
		G0_PK,
		G0_CampaignName
)SurveyCount
ON QuestionAverage.HY_G0 = SurveyCount.G0_PK", "DROP FUNCTION ClientReport_CustomerServiceSurveyResult", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_StaffLongServiceYears

			new DatabaseViewAndRoutineCreateScript("ClientReport_StaffLongServiceYears",
@"CREATE FUNCTION ClientReport_StaffLongServiceYears
(
	@PeriodStart DATETIME,
	@PeriodEnd	DATETIME
)
RETURNS TABLE
AS
RETURN
SELECT 
	GS_Code,
	GS_FullName,
	GB_BranchName,
	GS_EmploymentDate,
	ServiceYear,
	LongServicePeriod
FROM
	(
		SELECT
			GS_Code,
			GS_FullName,
			GB_BranchName,
			GS_EmploymentDate,
			ServiceYear = CASE
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -50, @PeriodStart) AND DATEADD(YEAR, -50, @PeriodEnd) THEN 50
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -45, @PeriodStart) AND DATEADD(YEAR, -45, @PeriodEnd) THEN 45
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -40, @PeriodStart) AND DATEADD(YEAR, -40, @PeriodEnd) THEN 40
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -35, @PeriodStart) AND DATEADD(YEAR, -35, @PeriodEnd) THEN 35
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -30, @PeriodStart) AND DATEADD(YEAR, -30, @PeriodEnd) THEN 30
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -25, @PeriodStart) AND DATEADD(YEAR, -25, @PeriodEnd) THEN 25
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -20, @PeriodStart) AND DATEADD(YEAR, -20, @PeriodEnd) THEN 20
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -15, @PeriodStart) AND DATEADD(YEAR, -15, @PeriodEnd) THEN 15
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -10, @PeriodStart) AND DATEADD(YEAR, -10, @PeriodEnd) THEN 10
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -5, @PeriodStart) AND DATEADD(YEAR, -5, @PeriodEnd) THEN 5
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -2, @PeriodStart) AND DATEADD(YEAR, -2, @PeriodEnd) THEN 2
				ELSE -1
			END,
			LongServicePeriod = CASE
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -50, @PeriodStart) AND DATEADD(YEAR, -50, @PeriodEnd) THEN '50 Years'
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -45, @PeriodStart) AND DATEADD(YEAR, -45, @PeriodEnd) THEN '45 Years'
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -40, @PeriodStart) AND DATEADD(YEAR, -40, @PeriodEnd) THEN '40 Years'				
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -35, @PeriodStart) AND DATEADD(YEAR, -35, @PeriodEnd) THEN '35 Years'
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -30, @PeriodStart) AND DATEADD(YEAR, -30, @PeriodEnd) THEN '30 Years'				
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -25, @PeriodStart) AND DATEADD(YEAR, -25, @PeriodEnd) THEN '25 Years'
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -20, @PeriodStart) AND DATEADD(YEAR, -20, @PeriodEnd) THEN '20 Years'
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -15, @PeriodStart) AND DATEADD(YEAR, -15, @PeriodEnd) THEN '15 Years'
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -10, @PeriodStart) AND DATEADD(YEAR, -10, @PeriodEnd) THEN '10 Years'
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -5, @PeriodStart) AND DATEADD(YEAR, -5, @PeriodEnd) THEN '5 Years'
				WHEN GS_EmploymentDate BETWEEN DATEADD(YEAR, -2, @PeriodStart) AND DATEADD(YEAR, -2, @PeriodEnd) THEN '2 Years'
				ELSE ''
			END
		FROM
			dbo.GlbStaff
			INNER JOIN dbo.GlbBranch ON GS_GB_HomeBranch = GB_PK
		WHERE
			GS_IsActive = 1
			AND GS_IsSystemAccount = 0
			AND GS_IsResource = 0
			AND GS_EmploymentDate <= DATEADD(YEAR, -2, @PeriodEnd)
	) a
WHERE
	ServiceYear != -1

", "DROP FUNCTION ClientReport_StaffLongServiceYears", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_ClientIncidentSummary

			new DatabaseViewAndRoutineCreateScript("ClientReport_ClientIncidentSummary",
@"CREATE FUNCTION ClientReport_ClientIncidentSummary
(
	@CompanyPK UNIQUEIDENTIFIER,
	@Status NVARCHAR(3),
	@ClientPK UNIQUEIDENTIFIER,
	@IncludeManagementGroup nvarchar
)
RETURNS TABLE
AS
RETURN
SELECT 
 GlbStaffAssignedTo.GS_PK as AssignedToStaffPK,
 GlbStaffAssignedTo.GS_FullName as AssignedToStaffFullName,
 GlbStaffCreatedUser.GS_PK as CreateUserPK,
 GlbStaffCreatedUser.GS_FullName as CreateUserCode,
 IM_ReasonableHours,
 IM_Category,
 OH_Code,
 OC_ContactName,
 OH_FullName,
 OH_PK,
 RN_Desc,
 RN_PK, 
 IM_Priority, 
 IM_SystemCreateTimeUtc,
 IM_Description,
 IM_IncidentNumber,
 IM_RequiredBy,
 OH_RL_NKClosestPort,
 IM_Module,
 IM_SourceModuleId,
 IM_ProgramArea as ProductAreaCode,
 productAreas.ProductAreaDescription as ProductAreaDescription,
 COALESCE(IncidentProductAndModules.ModuleDescription, IncidentCr8Modules.ModuleDescription, IncidentCr9Modules.ModuleDescription) as ModuleDescription,
 IM_ClientIncidentReference,
 IM_Status as IncidentStatus,
 StmNoteComment.ST_NoteText as ST_NoteTextComment,
 StmNoteResolution.ST_NoteText as ST_NoteTextResolution,
 IM_CloseTimeUtc,
 LE_EnterpriseCode,
 GlbStaffRM1.GS_PK as RM1PK,
 GlbStaffRM2.GS_PK as RM2PK,
 GlbStaffRM1.GS_FullName as RelManagerPrimaryName,
 GlbStaffRM2.GS_FullName as RelManagerSecondaryName,
 IM_ResolutionCode as DispositionCode,
 IM_Category + '_' + IM_Status + '_' + IM_Priority + '_' + IM_Product + '_' + IM_ResolutionCode as DispositionDescriptionKey,
 IM_Product,
 LastGS.GS_Code as LastTaskClosedByCode,
 LastGS.GS_FullName as LastTaskClosedByFullName
FROM 
 dbo.IncidentMain IM1
 LEFT JOIN dbo.ClientReport_GetAllProductsAndModulesFromRegistry() IncidentProductAndModules ON IM_Priority NOT IN ('CR8', 'CR9') AND IM_PRODUCT = IncidentProductAndModules.ProductCode AND IM_Module = IncidentProductAndModules.ModuleCode
 LEFT JOIN dbo.ClientReport_GetAllCr8ModulesFromRegistry() IncidentCr8Modules ON IM_Priority = 'CR8' AND IM_Module = IncidentCr8Modules.ModuleCode AND IM_Product = IncidentCr8Modules.ProductCode
 LEFT JOIN dbo.ClientReport_GetAllCr9ModulesFromRegistry() IncidentCr9Modules ON IM_Priority = 'CR9' AND IM_Module = IncidentCr9Modules.ModuleCode AND IM_Product = IncidentCr9Modules.ProductCode
 LEFT join dbo.ClientReport_GetAllEnterpriseProductAreasFromRegistry() as productAreas ON productAreas.ProductAreaCode = IM_ProgramArea
 inner join dbo.OrgHeader on IM_OH_Client = OH_PK
 LEFT JOIN dbo.RefUNLOCO on OH_RL_NKClosestPort = RL_Code
 LEFT JOIN dbo.RefCountry on RL_RN_NKCountryCode = RN_Code
 LEFT JOIN dbo.LicenceCompany on LC_OH = OH_PK
 LEFT JOIN dbo.LicenceEnterprise on LC_LE = LE_PK
 LEFT JOIN dbo.OrgContact on IM_OC_Contact = OC_PK
 LEFT JOIN dbo.GlbStaff as GlbStaffAssignedTo on IM_GS_NKAssignedToCurrent = GlbStaffAssignedTo.GS_Code
 LEFT JOIN dbo.GlbStaff as GlbStaffCreatedUser on IM_SystemCreateUser = GlbStaffCreatedUser.GS_Code
 LEFT JOIN dbo.StmNote as StmNoteComment on StmNoteComment.ST_ParentID = IM_PK and StmNoteComment.ST_Table = 'IncidentMain' and StmNoteComment.ST_Description = 'Incident Comment'
 LEFT JOIN dbo.StmNote as StmNoteResolution on StmNoteResolution.ST_ParentID = IM_PK and StmNoteResolution.ST_Table = 'IncidentMain' and StmNoteResolution.ST_Description = 'Incident Resolution Detail'
 LEFT JOIN dbo.OrgStaffAssignments as GlbStaffOrgStaffAssignmentsRM1 ON GlbStaffOrgStaffAssignmentsRM1.O8_OH = OH_PK 
 AND GlbStaffOrgStaffAssignmentsRM1.O8_Role = 'RM1' AND GlbStaffOrgStaffAssignmentsRM1.O8_GC = @CompanyPK
 LEFT JOIN dbo.GlbStaff as GlbStaffRM1 ON GlbStaffOrgStaffAssignmentsRM1.O8_GS_NKPersonResponsible = GlbStaffRM1.GS_Code
 LEFT JOIN dbo.OrgStaffAssignments as GlbStaffOrgStaffAssignmentsRM2 ON GlbStaffOrgStaffAssignmentsRM2.O8_OH = OH_PK AND 
 GlbStaffOrgStaffAssignmentsRM2.O8_Role = 'RM2' AND GlbStaffOrgStaffAssignmentsRM2.O8_GC = @CompanyPK
 LEFT JOIN dbo.GlbStaff as GlbStaffRM2 ON GlbStaffOrgStaffAssignmentsRM2.O8_GS_NKPersonResponsible = GlbStaffRM2.GS_Code
 LEFT JOIN
	(
		  SELECT IM, P9_GS_NKAssignedStaffMember, 
		  ROW_NUMBER() OVER (PARTITION BY IM ORDER BY P9_CompletedTimeUtc DESC) RN
		  FROM
		  (
				 SELECT XX_Relation1ID IM, XX_Relation2ID WKI FROM dbo.GenPivot where XX_Relation1TableCode = 'IM' AND XX_Relation2TableCode = 'WKI'
		  ) IM_WKI
		  INNER JOIN dbo.ProcessTasks ON P9_ParentID = WKI
		  WHERE P9_ParentTableCode = 'WKI'
		  AND P9_Status = 'CLS'
		  AND P9_GS_NKAssignedStaffMember <> ''
	) IM_P9 ON IM_P9.IM = IM_PK AND RN = 1
LEFT JOIN dbo.GlbStaff LastGS ON IM_P9.P9_GS_NKAssignedStaffMember = LastGS.GS_Code  
WHERE
	IM_IncidentType = 'inc'
AND (
	@Status = '' OR 
		@Status = IM_Status OR
		(@Status = 'NCL' AND (IM_Status = 'OPN' OR IM_Status = 'WRK' or IM_Status = 'SUS'))
	)
AND 
	(@ClientPK is null or
	(@ClientPK is not null and 
	((@IncludeManagementGroup = '' and OH_PK = @ClientPK) or
	(@IncludeManagementGroup = 'Y' and OH_PK in (select PR_OH_Parent from dbo.OrgRelatedParty 
	 where PR_OH_RelatedParty = @ClientPK and PR_PartyType = 'MNG' and PR_IsValid = 1 )))))",
	"DROP FUNCTION ClientReport_ClientIncidentSummary", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientReport_WARPNominatedAgentsReport

			new DatabaseViewAndRoutineCreateScript("ClientReport_WARPNominatedAgentsReport",
				@"CREATE FUNCTION ClientReport_WARPNominatedAgentsReport()
RETURNS @result TABLE
(
	NominatedAgent nvarchar(256),
	NAFullName nvarchar(256),
	NAPK uniqueidentifier,
	NAContactName nvarchar(256),
	NAContactPhone nvarchar(256),
	NAContactMobile nvarchar(256),
	NAContactEmail nvarchar(256),
	SalesRepCode varchar(3),
	SalesRep nvarchar(256),
	SalesPK uniqueidentifier,
	AchieveableBusiness money,
	EmployeeNumber int,
	ClientSize varchar(5),
	UNLOCO varchar(5),
	Country varchar(5),
	ReferringCustomer nvarchar(256),
	RCFullName nvarchar(256),
	RCPK uniqueidentifier,
	RCContactName nvarchar(256),
	RCContactPhone nvarchar(256),
	RCContactMobile nvarchar(256),
	RCContactEmail nvarchar(256)
)
AS
BEGIN
INSERT INTO @result	

SELECT NA.OH_Code AS 'Nominated Agent',
	NA.OH_FullName AS NAFullName,
	NA.OH_PK as NAPK, 
	NAContact.OC_ContactName AS NAContactName, 
	NAContact.OC_Phone AS NAContactPhone,	
	NAContact.OC_Mobile AS NAContactMobile, 
	NAContact.OC_Email AS NAContactEmail,
	GlbStaff.GS_Code as SalesRepCode,
	GlbStaff.GS_FullName as SalesRep,
	GlbStaff.GS_PK as SalesPK, 
	OM_CMAcheivableClientRevenue, 
	OM_CMNoOfEmployees, 
	OM_CMClientSize, 
	NA.OH_RL_NKClosestPort, 
	LEFT(NA.OH_RL_NKClosestPort, 2) as Country,
	RC.OH_Code AS 'Referring Customer',
	RC.OH_FullName AS RCFullName,
	RC.OH_PK as RCPK,  
	RCContact.OC_ContactName AS RCContactName,
	RCContact.OC_Phone AS RCContactPhone,
	RCContact.OC_Mobile AS RCContactMobile,
	RCContact.OC_Email AS RCContactEmail
FROM dbo.OrgRelatedParty
	join dbo.OrgHeader NA on NA.OH_PK = PR_OH_Parent
	join dbo.OrgHeader RC on RC.OH_PK = PR_OH_RelatedParty
	join dbo.OrgMiscServ on PR_OH_Parent = OrgMiscServ.OM_OH
	left join dbo.OrgStaffAssignments on O8_OH = NA.OH_PK AND O8_Role = 'SAL'
	left join dbo.GlbStaff on GlbStaff.GS_Code = OrgStaffAssignments.O8_GS_NKPersonResponsible
	left join
	(
		SELECT ROW_NUMBER() OVER(PARTITION BY OC_OH ORDER BY OC_ContactName) AS RowNumber, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Mobile
		FROM dbo.OrgContact join dbo.OrgContactAttribute on PC_OC = OC_PK AND PC_Type = 'WPC'
	) NAContact ON NAContact.OC_OH = NA.OH_PK AND NAContact.RowNumber = 1
	left join
	(
		SELECT ROW_NUMBER() OVER(PARTITION BY OC_OH ORDER BY OC_ContactName) AS RowNumber, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Mobile
		FROM dbo.OrgContact join dbo.OrgContactAttribute on PC_OC = OC_PK AND PC_Type = 'WPC'
	) RCContact ON RCContact.OC_OH = RC.OH_PK AND RCContact.RowNumber = 1
WHERE 
	PR_PartyType = 'WRP' 

	
RETURN
END", "DROP FUNCTION ClientReport_WARPNominatedAgentsReport", DbRoutineType.SqlFunctionTableTypeDesc),

		#endregion

		#region ClientReport_CloudServicesClientSummary

			new DatabaseViewAndRoutineCreateScript("ClientReport_CloudServicesClientSummary",
@"CREATE FUNCTION ClientReport_CloudServicesClientSummary
(
	@EnterpriseCode NVARCHAR(3)
)
RETURNS TABLE
AS
RETURN

SELECT
 LE_EnterpriseCode AS EnterpriseCode,
 LCC_Code AS CompanyCode,
 LCB_Code AS BranchCode,
 LCC_Name AS CompanyName,
 COALESCE (OA_Address1, LCC_Address1) AS Address1,
 COALESCE (OA_Address2, LCC_Address2) AS Address2,
 COALESCE (OA_City, LCC_City) AS City,
 COALESCE (OA_PostCode, LCC_PostCode) AS PostCode,
 COALESCE (OA_State, LCC_State) AS State,
 COALESCE (OA_RN_NKCountryCode, LCC_RN_NKCountryCode) AS CountryCode,
 COALESCE (OA_Phone, LCC_Phone) AS Phone,
 OA_Email AS Email,
 LD_IsActive AS IsLicenceDatabaseActive,
 CAST (
	CASE
		WHEN LCC_DeactivateTimeUtc IS NULL OR LCC_DeactivateTimeUtc > GETUTCDATE()
			THEN 1
		ELSE 0
	END AS bit) as IsClientCompanyActive,
 LCB_IsActive AS IsClientBranchActive
FROM
 dbo.LicenceEnterprise
 JOIN dbo.LicenceDatabase ON LD_LE = LE_PK
 JOIN dbo.ClientCompany ON LCC_LD = LD_PK
 JOIN dbo.ClientBranch ON LCB_LD = LD_PK AND LCB_LCC_Code = LCC_Code
 LEFT JOIN dbo.OrgAddress ON LCB_OA = OA_PK
WHERE
 LE_EnterpriseCode = @EnterpriseCode
",
	"DROP FUNCTION ClientReport_CloudServicesClientSummary", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Billing Usage

			BillingUsageSchema.NonBilledEnterpriseCodesScript(),
			BillingUsageSchema.GetChargeableUsageUpdateScript(),
			BillingUsageSchema.GetOdmMonthlyModuleBundleUsersScript(),
			BillingUsageSchema.GetBillingCountryGroupsScript(),
			BillingUsageSchema.GetGenericUsageScript(),
			BillingUsageSchema.GetODMScript(),
			BillingUsageSchema.GetCPTScript(),
			BillingUsageSchema.GetFaxLegacyScript(),
			BillingUsageSchema.GetFaxScript(),
			//BillingUsageSchema.GetIQMScript(),
			//BillingUsageSchema.GetEXDScript(),
			BillingUsageSchema.GetDPS_BillingScript(),
			BillingUsageSchema.GetDPSScript(),
			BillingUsageSchema.GetDCGBillingScript(),
			BillingUsageSchema.GetDCGScript(),
			BillingUsageSchema.GetDCPBillingScript(),
			BillingUsageSchema.GetDCPScript(),
			BillingUsageSchema.GetHOSScript(),
			BillingUsageSchema.GetAirlineMessagingScript(),
			BillingUsageSchema.GetNZC_BillingScript(),
			BillingUsageSchema.GetNZCScript(),
			BillingUsageSchema.GetJapanCustomsAFR_Billing(),
			BillingUsageSchema.GetJapanCustomsAFRScript(),
			BillingUsageSchema.GetCMP_BillingScript(),
			BillingUsageSchema.GetCMPScript(),
			BillingUsageSchema.GetClientMappingNameSyncScript(),
			BillingUsageSchema.GetEAD_BillingScript(),
			BillingUsageSchema.GetEADScript(),
			BillingUsageSchema.GetEAD_DetailedUsageScript(),
			BillingUsageSchema.GetE2E_BillingScript(),
			BillingUsageSchema.GetE2EScript(),
			BillingUsageSchema.GetUSCScript(),
			BillingUsageSchema.GetUSC_BillingScript(),
			BillingUsageSchema.GetOCTScript(),
			BillingUsageSchema.GetOCT_BillingScript(),
			BillingUsageSchema.GetRICScript(),
			BillingUsageSchema.GetRIC_BillingScript(),
			BillingUsageSchema.GetBillingUnitCountAdjustmentsScript(),
			BillingUsageSchema.GetCountryTierPriceCodeMappingsScript(),
			BillingUsageSchema.GetBillingDisbursementUsageMappingsScript(),
			BillingUsageSchema.GetChargeableUsageFromBillingSTLDisbursementScript(),
			BillingUsageSchema.GetStlScript(),
			BillingUsageSchema.GetStl_BillingScript(),
			BillingUsageSchema.GetFWAScript(),
			BillingUsageSchema.GetFWA_BillingScript(),
			BillingUsageSchema.GetSPMScript(),
			BillingUsageSchema.GetSPM_BillingScript(),
			BillingUsageSchema.GetGBCScript(),
			BillingUsageSchema.GetGBC_BillingScript(),
			BillingUsageSchema.GetISFScript(),
			BillingUsageSchema.GetISF_BillingScript(),
			BillingUsageSchema.GetASCScript(),
			BillingUsageSchema.GetASC_BillingScript(),
			BillingUsageSchema.GetHDA_BillingScript(),
			BillingUsageSchema.GetHDAScript(),
			BillingUsageSchema.GetZACScript(),
			BillingUsageSchema.GetZAC_BillingScript(),
			BillingUsageSchema.GetBorderWiseUsersScript(),
			BillingUsageSchema.GetBorderWiseUsersForPeriodRange(),
			BillingUsageSchema.GetBorderWiseLicencesScript(),
			BillingUsageSchema.GetBorderWiseScript(),
			BillingUsageSchema.GetFlightStatsScript(),

			BillingUsageSchema.GetBillingGroupsScript(),
			BillingUsageSchema.GetViewBillingClientCompanyScript(),
			BillingUsageSchema.GetSystemUsageScript(),
			BillingUsageSchema.GetStlUsageSummaryScript(),
			BillingUsageSchema.GetStlGenericUsageSummaryScript(),
			BillingUsageSchema.GetBillingDbDetailedUsageWTUScript(),
			BillingUsageSchema.GetBillingDbDetailedUsageUSRScript(),
			BillingUsageSchema.GetBillingDbDetailedUsageCOTScript(),
			BillingUsageSchema.GetBillingDbDetailedConsolidationUsageScript(),
			BillingUsageSchema.GetBillingDbDetailedUsageDisbursementScript(),
			BillingUsageSchema.GetBillingDbDetailedUsageScript(),
			BillingUsageSchema.LoadAllStlChargeableUsageScript(),
			BillingUsageSchema.LoadBilledUsageScript(),
			BillingUsageSchema.UpdateContactCountryScript(),
			BillingUsageSchema.LoadAllBorderWiseChargeableUsageScript(),
			BillingUsageSchema.LoadAllGenericChargeableUsageScript(),
			BillingUsageSchema.GetDatabaseBillingHostedLocationsScript(),
			BillingUsageSchema.GetHandheldDevicePremiumTypesScript(),
			BillingUsageSchema.EdiGetBillingDbUsageScript(),
			BillingUsageSchema.UpdateLicenceDatabaseConsolidationScript(),

		#endregion

		#region Deposits

			DepositSchema.GetDepositChargeCodesScript(),
			DepositSchema.EdiDepositBalanceUpdateScript(),

		#endregion

		#region Clientvw_DevWorkItem

			new DatabaseViewAndRoutineCreateScript("Clientvw_DevWorkItem", @"
CREATE view Clientvw_DevWorkItem as
SELECT 
	DWI_PK = WKI_PK,
	DWI_Number = WKI_WorkItemNumber,
	DWI_Status = WKI_Status
FROM dbo.Workitem", "DROP VIEW Clientvw_DevWorkItem", DbRoutineType.SqlViewTypeDesc),

		#endregion

		#region Product Registration

		#region ProductRegistrationGetStatus

		new DatabaseViewAndRoutineCreateScript("ProductRegistrationGetStatus",
@"CREATE FUNCTION ProductRegistrationGetStatus
(
	@EnterpriseCode varchar(3),
	@ServerCode varchar(3)
)
RETURNS TABLE
AS
RETURN
(
	select LD_PK, LD_DatabaseNumber,
		-- Map PRE to NON for compatibility with the older web service.
		LD_Status = case LD_Status when 'PRE' then 'NON' else LD_Status end
		, LD_Product
	from dbo.LicenceDatabase
	join dbo.LicenceEnterprise on LD_LE = LE_PK
	where LE_EnterpriseCode = @EnterpriseCode
		and LD_ServerCode = @ServerCode
		and LD_IsActive = 1
)

", "drop function ProductRegistrationGetStatus", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ProductRegistrationGetDatabaseUniqueKey

		new DatabaseViewAndRoutineCreateScript("ProductRegistrationGetDatabaseUniqueKey",
@"CREATE FUNCTION ProductRegistrationGetDatabaseUniqueKey
(
	@DatabaseNumber int
)
RETURNS TABLE
AS
RETURN
(
	select
		LD_HostDBName,
		LD_HostServerName,
		LD_HostGroupId = ISNULL(LD_HostGroupId, 0x),
		LD_HostDBCreateDate = ISNULL(LD_HostDBCreateDate, '1900-1-1'),
		LD_HostConnectionServerName
	from
		dbo.LicenceDatabase
	where
		LD_DatabaseNumber = @DatabaseNumber
)
", "drop function ProductRegistrationGetDatabaseUniqueKey", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ProductRegistrationAdd

		new DatabaseViewAndRoutineCreateScript("ProductRegistrationAdd",
@"CREATE PROCEDURE ProductRegistrationAdd
	@EnterpriseCode varchar(3),
	@ServerCode varchar(3),
	@ServerName varchar(max),
	@DatabaseName varchar(max),
	@DatabaseCreateDate datetime,
	@GroupId uniqueidentifier,
	@Password varchar(200),
	@LockTimeoutMs int = 5000,
	@ConnectionServerName varchar(max) = '',
	@VersionMajor int = NULL,
	@VersionMinor int = NULL,
	@VersionRelease int = NULL,
	@VersionPatch int = NULL
-- Returns:
--   0 = ok (number must equal RegisterStatus.Success)
--   1 = code not found (number must equal RegisterStatus.ProductKeyNotFound)
--   2 = code unavailable (number must equal RegisterStatus.ProductKeyUnavailable)
--   < 0 = error
AS
BEGIN
SET NOCOUNT ON;

declare @ldpk uniqueidentifier
select @ldpk = LD_PK
from dbo.LicenceDatabase
join dbo.LicenceEnterprise on LD_LE = LE_PK
where LE_EnterpriseCode = @EnterpriseCode
	and LD_ServerCode = @ServerCode
	and LD_IsActive = 1

if @ldpk is null
	return 1;

declare @LockName varchar(50) = @ldpk
BEGIN TRY
	DECLARE @rc int = 0

	DECLARE @initialTranCount int
	SET @initialTranCount = @@TRANCOUNT
	IF @initialTranCount = 0
		BEGIN TRAN

	declare @gotLock int;
	EXEC @gotLock = sp_getapplock @Resource = @LockName,
				@LockMode = 'Exclusive',
				@LockTimeout = @LockTimeoutMs

	IF (@gotLock < 0)
	BEGIN
		declare @Error varchar(200) = 'Could not obtain lock ' + CAST(@gotLock as varchar(10));
		THROW 50000, @Error, 1
	END

	declare @InstanceIndex int = CHARINDEX('/', @ServerName);
	declare @InstanceName varchar(max) = case when @InstanceIndex > 0 then substring(@ServerName, @InstanceIndex + 1, len(@ServerName) - @InstanceIndex) else '' end;
	
	update dbo.LicenceDatabase
	set LD_HostDBName = @DatabaseName,
		LD_HostServerName = @ServerName, -- NB includes instance name
		LD_HostDBInstance = @InstanceName,
		LD_Status = 'REG',
		LD_HostDBCreateDate = @DatabaseCreateDate,
		LD_HostGroupId = @GroupId,
		LD_Password = @Password,
		LD_HostConnectionServerName = @ConnectionServerName
	where LD_PK = @ldpk and
		(
			LD_Status in ('', 'NON')
			or
			(LD_Status = 'PRE' and LD_HostServerName = @ServerName and LD_HostDBName = @DatabaseName)
			or
			(LD_CanReregisterToSameServer = 1 and LD_HostServerName = @ServerName and @ConnectionServerName != '' and LD_HostConnectionServerName = @ConnectionServerName)
		)

	set @rc = case @@ROWCOUNT when 0 then 2 else 0 end;

	if (@rc = 0)
	begin
		select 
			UtcNow = SYSUTCDATETIME(), 
			db.LD_PK, 
			LD_HostedLocation = case db.LD_HostedLocation when 'TRA' then 'NCW' else db.LD_HostedLocation end, 
			LD_LicenceExpiry = db.LD_ManualLicenceExpiry, 
			db.LD_DBServerSecurityMode, 
			db.LD_DatabaseNumber, 
			db.LD_LicenceType,
			IsInternalSystem = LE_IsInternal,
			BillingModel = case when PHL_L6 is null then 'ODM' else 'STL' end
		from 
			dbo.LicenceDatabase db
			join dbo.LicenceEnterprise on db.LD_LE = LE_PK
			left join dbo.LicenceDatabase parentdb on db.LD_LD_ParentDatabase = parentdb.LD_PK
			outer apply
			(
				select top 1 PHL_L6 
				from dbo.EdiPriceHeaderLink 
				where PHL_ValidFrom <= getdate() and (PHL_ValidTo >= getdate() or PHL_ValidTo is null) and PHL_LD = db.LD_PK and db.LD_LicenceType = 'PRD'
				
				union all

				select top 1 PHL_L6 
				from dbo.EdiPriceHeaderLink 
				where PHL_ValidFrom <= getdate() and (PHL_ValidTo >= getdate() or PHL_ValidTo is null) and PHL_LD = db.LD_LD_ParentDatabase and db.LD_LicenceType != 'PRD'
			) a

		where 
			db.LD_PK = @ldpk
	end

	EXEC sp_releaseapplock @Resource = @LockName
	SET @gotLock = -1

	IF (@initialTranCount = 0 AND @@TRANCOUNT > 0)
		COMMIT

	return @rc

END TRY
BEGIN CATCH
	IF (@initialTranCount = 0 AND @@TRANCOUNT > 0)
		ROLLBACK

	IF (@initialTranCount > 0 AND @gotLock >= 0)
		EXEC sp_releaseapplock @Resource = @LockName;

	THROW
END CATCH
END
", "DROP PROCEDURE ProductRegistrationAdd", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region ProductRegistrationVerify

		new DatabaseViewAndRoutineCreateScript("ProductRegistrationVerify",
@"CREATE PROCEDURE ProductRegistrationVerify
	@DatabaseNumber int,
	@ServerName varchar(max),
	@DatabaseName varchar(max),
	@DatabaseCreateDate datetime,
	@GroupId uniqueidentifier,
	@Password varchar(max) = null,
	@ConnectionServerName varchar(max) = '',
	@VersionMajor int = NULL,
	@VersionMinor int = NULL,
	@VersionRelease int = NULL,
	@VersionPatch int = NULL
-- Returns:
--   0 = ok, no change to DbUniqueKey (number must equal RegisterStatus.Success)
--   1 = database not found (number must equal RegisterStatus.ProductKeyNotFound)
--   3 = ok, DbUniqueKey updated (number must equal RegisterStatus.UniqueKeyUpdated)
--   4 = key does not match (number must equal RegisterStatus.UniqueKeyNotMatched)
--   5 = database is not registered (number must equal RegisterStatus.Unregistered)
--   < 0 = error
AS
begin
set NOCOUNT ON;

declare @ldpk uniqueidentifier;
declare @LD_Password varchar(200);
declare @LD_IsActive bit;
declare @LD_Status varchar(3);
declare @LD_LicenceType varchar(3);
declare @LD_HostedLocation varchar(3);
declare @LD_ServerCode varchar(3);
declare @LD_DBServerSecurityMode varchar(3);
declare @LD_HostServerName varchar(max);
declare @LD_HostDBName varchar(max);
declare @LD_HostDBCreateDate datetime;
declare @LD_HostGroupId uniqueidentifier;
declare @LD_LE uniqueidentifier;
declare @LD_ManualLicenceExpiry smalldatetime;
declare @LD_HostConnectionServerName varchar(max);
declare @IsInternalSystem bit;
declare @BillingModel varchar(3);

declare @initialTranCount int = -1;
declare @gotLock int = -1;
declare @rc int = -1;
declare @loopCount int = 0;
declare @LockName varchar(50);

-- May need a second loop to obtain a lock and update the LicenceDatabase if AlwaysOn changes
begin try
while @loopCount < 2
begin
	set @loopCount = @loopCount + 1;

	select @ldpk = db.LD_PK,
		@LD_Password = db.LD_Password,
		@LD_HostServerName = db.LD_HostServerName,
		@LD_HostDBName = db.LD_HostDBName,
		@LD_HostDBCreateDate = db.LD_HostDBCreateDate,
		@LD_HostGroupId = db.LD_HostGroupId,
		@LD_IsActive = db.LD_IsActive,
		@LD_Status = db.LD_Status,
		@LD_LicenceType = db.LD_LicenceType,
		@LD_HostedLocation = case db.LD_HostedLocation when 'TRA' then 'NCW' else db.LD_HostedLocation end,
		@LD_DBServerSecurityMode = db.LD_DBServerSecurityMode,
		@LD_ManualLicenceExpiry = db.LD_ManualLicenceExpiry,
		@LD_ServerCode = db.LD_ServerCode,
		@LD_LE = db.LD_LE,
		@LD_HostConnectionServerName = db.LD_HostConnectionServerName,
		@IsInternalSystem = LE_IsInternal,
		@BillingModel = case when PHL_L6 is null then 'ODM' else 'STL' end
	from 
		dbo.LicenceDatabase db
		join dbo.LicenceEnterprise on db.LD_LE = LE_PK
		left join dbo.LicenceDatabase parentdb on db.LD_LD_ParentDatabase = parentdb.LD_PK
		outer apply
		(
			select top 1 PHL_L6 
			from dbo.EdiPriceHeaderLink 
			where PHL_ValidFrom <= getdate() and (PHL_ValidTo >= getdate() or PHL_ValidTo is null) and PHL_LD = db.LD_PK and db.LD_LicenceType = 'PRD'
				
			union all

			select top 1 PHL_L6 
			from dbo.EdiPriceHeaderLink 
			where PHL_ValidFrom <= getdate() and (PHL_ValidTo >= getdate() or PHL_ValidTo is null) and PHL_LD = db.LD_LD_ParentDatabase and db.LD_LicenceType != 'PRD'
		) a
	where 
		db.LD_DatabaseNumber = @DatabaseNumber;


	if @ldpk is null
		or ISNULL(@ServerName, '') = ''
		or ISNULL(@DatabaseName, '') = ''
		or @LD_IsActive = 0
	begin
		set @rc = 1 -- Not Found
	end
	else if (@LD_Status = '' or @LD_Status = 'NON')
	begin
		set @rc = 5 -- unregistered
	end
	else if (@LD_Status != 'REG')
		or (@Password is not null and @Password != @LD_Password)
		or @LD_Password = '-'
		or @LD_Password = ''
		or @LD_HostDBName != @DatabaseName
		or (
			(@LD_HostServerName != @ServerName or @LD_HostDBCreateDate != @DatabaseCreateDate)
			and
			(@LD_HostGroupId is null or @GroupId is null or @LD_HostGroupId != @GroupId)
			and
			(@LD_HostedLocation in ('', 'NCW') or @LD_HostConnectionServerName = '' or @LD_HostConnectionServerName != @ConnectionServerName)
		)
	begin
		set @rc = 4; -- same as Not Matched
	end
	else
	begin
		declare @isKeyEqual bit = (case when 
				ISNULL(@LD_HostGroupId, 0x) = ISNULL(@GroupId, 0x)
				and @LD_HostServerName = @ServerName
				and @LD_HostDBCreateDate = @DatabaseCreateDate
				and @LD_HostConnectionServerName = @ConnectionServerName
			then 1 else 0 end);

		if (@isKeyEqual = 1)
			or (1 = (select top 1 is_read_only from sys.databases where name = DB_NAME()))
		begin
			-- everything matches, and no need for update or the DB is read-only
			set @rc = 0
		end
		else
		begin
			-- Need to update AlwaysOn or Hosted server move
			-- On first loop we just obtain a lock
			if (@loopCount = 1)
			begin
				set @LockName = @ldpk;
				set @initialTranCount = @@TRANCOUNT
				if @initialTranCount = 0
					begin tran

				EXEC @gotLock = sp_getapplock @Resource = @LockName,
						@LockMode = 'Exclusive',
						@LockOwner =  'Transaction',
						@LockTimeout = '5000',
						@DbPrincipal = 'public'

				if (@gotLock < 0)
				begin
					-- could not obtain lock
					set @rc = 0
					rollback
				end
			end
			else
			begin
				-- second loop does the update
				set @rc = 0
				begin try
					update dbo.LicenceDatabase 
					set LD_HostGroupId = @GroupId,
						LD_HostServerName = @ServerName,
						LD_HostDBCreateDate = @DatabaseCreateDate,
						LD_HostConnectionServerName = @ConnectionServerName
					where LD_PK = @ldpk;
					set @rc = 3;
				end try
				begin catch
				end catch
			end
		end
	end

	if @rc = 0 or @rc = 3
	begin
		select UtcNow = SYSUTCDATETIME(), LE_EnterpriseCode,
			LD_LicenceType = @LD_LicenceType,
			LD_HostedLocation = @LD_HostedLocation,
			LD_DBServerSecurityMode = @LD_DBServerSecurityMode,
			LD_LicenceExpiry = @LD_ManualLicenceExpiry,
			LD_ServerCode = @LD_ServerCode,
			IsInternalSystem = @IsInternalSystem,
			BillingModel = @BillingModel,
			CustomExpiredMessage, CustomExpiryWeekMessage, CustomExpiryMonthMessage
		from dbo.LicenceEnterprise
		outer apply
		(
			select
				CustomExpiredMessage = max(case ST_Description when 'CustomExpiredMessage' then ST_NoteText else '' end),
				CustomExpiryWeekMessage = max(case ST_Description when 'CustomExpiryWeekMessage' then ST_NoteText else '' end),
				CustomExpiryMonthMessage = max(case ST_Description when 'CustomExpiryMonthMessage' then ST_NoteText else '' end)
			from dbo.StmNote
			where ST_ParentID = @ldpk
				and ST_Description in ('CustomExpiredMessage', 'CustomExpiryWeekMessage', 'CustomExpiryMonthMessage')
				and ST_NoteType = 'DOC'
			group by ST_ParentID
		) a
		where @LD_LE = LE_PK;
	end

	if @rc >= 0 or @loopCount = 2
	begin
		if (@gotLock >= 0)
		begin
			EXEC sp_releaseapplock @Resource = @LockName
			set @gotLock = -1

			if (@initialTranCount = 0 AND @@TRANCOUNT > 0)
				COMMIT
		end

		return @rc;
	end
end
end try
begin catch
	if (@initialTranCount = 0 AND @@TRANCOUNT > 0)
		ROLLBACK

	if (@initialTranCount > 0 AND @gotLock >= 0)
		EXEC sp_releaseapplock @Resource = @LockName;

	throw
end catch
end
", "DROP PROCEDURE ProductRegistrationVerify", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region ProductRegistrationRemove

			new DatabaseViewAndRoutineCreateScript("ProductRegistrationRemove",
@"CREATE PROCEDURE ProductRegistrationRemove
	@DatabaseNumber int,
	@Password VARCHAR(200)
-- Returns:
--   0 = ok
--   1 = database not found
--   2 = database not registered or not active or wrong password
--   < 0 = error
AS
BEGIN
SET NOCOUNT ON;

declare @ldpk uniqueidentifier
select @ldpk = LD_PK from dbo.LicenceDatabase where LD_DatabaseNumber = @DatabaseNumber

if @ldpk is null
	return 1;

declare @LockName varchar(50) = @ldpk
BEGIN TRY
	DECLARE @rc int = 0
	DECLARE @initialTranCount int
	SET @initialTranCount = @@TRANCOUNT

	IF @initialTranCount = 0
		BEGIN TRAN

	declare @gotLock int;
	EXEC @gotLock = sp_getapplock @Resource = @LockName,
				@LockMode = 'Exclusive',
				@LockOwner =  'Transaction',
				@LockTimeout = '5000',
				@DbPrincipal = 'public'

	IF (@gotLock < 0)
	BEGIN
		declare @Error varchar(200) = 'Could not obtain lock ' + CAST(@gotLock as varchar(10));
		THROW 50000, @Error, 1
	END

	update dbo.LicenceDatabase set LD_Status = 'NON', LD_Password = '-' where LD_PK = @ldpk and LD_Status = 'REG' and LD_IsActive = 1 and LD_Password = @Password
	set @rc = case @@ROWCOUNT when 0 then 2 else 0 end;

	EXEC sp_releaseapplock @Resource = @LockName
	SET @gotLock = -1

	IF (@initialTranCount = 0 AND @@TRANCOUNT > 0)
		COMMIT

	return @rc

END TRY
BEGIN CATCH
	IF (@initialTranCount = 0 AND @@TRANCOUNT > 0)
		ROLLBACK

	IF (@initialTranCount > 0 AND @gotLock >= 0)
		EXEC sp_releaseapplock @Resource = @LockName;

	THROW
END CATCH
END
", "DROP PROCEDURE ProductRegistrationRemove", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#endregion

		#region Report_ApplicantExamResults

			new DatabaseViewAndRoutineCreateScript("Report_ApplicantExamResults",
@"CREATE FUNCTION Report_ApplicantExamResults
(
	@Organisation uniqueidentifier,
	@IncludeManagedOrgs char(1),
	@HS_PK UNIQUEIDENTIFIER, -- Job Skill
	@PER_PK UNIQUEIDENTIFIER = NULL,  -- Person
	@G0_PK UNIQUEIDENTIFIER = NULL,  -- Certificate Exam
	@TestCommencedFrom DATETIME = NULL,
	@TestCommencedTo DATETIME = NULL,
	@TestCompletedFrom DATETIME = NULL,
	@TestCompletedTo DATETIME = NULL
)
RETURNS TABLE
WITH SCHEMABINDING
AS 
RETURN

WITH Exam AS
(
	SELECT
		PER_PK,
		PER_FullName,
		HA_EmailAddress,
		HT_TestName,
		HT_PassMark,
		H3_PK,
		H3_SkillTestRating,
		H3_TestCommencedUtc,
		H3_TestCompletedUtc,
		G8_PK,
		G8_ClosedDateUtc,
		HS_Code,
		HS_SkillDescription,
		G0_PK,
		G0_CampaignName,
		Contact.OH_PK,
		Contact.OH_FullName,
		Contact.OH_Code,
		EXA_PK,
		EXA_Score,
		EXA_TestCommencedUtc,
		EXA_TestCompletedUtc,
		OC_PrimaryWorkplace = 
			case 
				when PrimaryContact.OC_PK is not null 
				then COALESCE(ContactAddress.OA_CompanyNameOverride, PrimaryContactOrg.OH_FullName)
			else StaffCompany.GC_Name end,

		OC_WorkingLocation = 
			case 
				when PrimaryContact.OC_PK is not null 
				then COALESCE(ContactAddress.OA_RL_NKRelatedPortCode, ContactOrgMainAddress.OA_RL_NKRelatedPortCode)
			else StaffBranch.GB_RL_NKHomePort end,

		OC_Branch = 
			case 
				when PrimaryContact.OC_PK is not null 
				then COALESCE(ContactAddress.OA_Code, ContactOrgMainAddress.OA_Code)
			else StaffBranch.GB_Code end,
		Ranking = ROW_NUMBER() OVER (PARTITION BY H3_PK, Contact.OH_PK ORDER BY EXA_TestCompletedUtc DESC)
	FROM
		(
			SELECT DISTINCT
				OC_PK, 
				OC_Email,
				OH_PK = ContactOrg.OH_PK,
				OH_Code = ContactOrg.OH_Code,
				OH_FullName = ContactOrg.OH_FullName,
				OC_PER
			FROM 
				dbo.OrgContact
				INNER JOIN dbo.OrgHeader ContactOrg ON OH_PK = OC_OH 
			WHERE
				OC_Email IS NOT NULL
				AND OC_Email <> ''
				AND OC_IsActive = 1
				AND (ContactOrg.OH_FullName LIKE 'CargoWise%' OR OC_Email NOT LIKE '%@cargowise.com')
				AND
				(
					@Organisation IS NULL
					OR @Organisation = OC_OH
					OR 
					(
						@IncludeManagedOrgs = 'Y'
						AND OC_OH IN 
						(
							SELECT PR_OH_Parent 
							FROM dbo.OrgRelatedParty   
							WHERE
								PR_OH_RelatedParty = @Organisation 
								AND PR_PartyType = 'MNG'
						)
					)
				)
		) Contact
		JOIN dbo.GlbPerson on Contact.OC_PER = PER_PK
		JOIN dbo.HRJobApplicant ON OC_PER = HA_PER
		JOIN dbo.HRJobApplicantSkillRating ON HR_HA = HA_PK
		JOIN dbo.HRJobSkill ON HR_HS = HS_PK
		JOIN dbo.HRJobSkillTest ON HT_HS = HS_PK
		JOIN dbo.ExamSetting on HT_EXS = EXS_PK
		JOIN dbo.GlbCompanyCampaign ON EXS_G0 = G0_PK
		JOIN dbo.GlbCompanyCampaignItem ON G8_G0 = G0_PK AND G8_RecipientID = HA_PK
		JOIN dbo.HRJobApplicantSkillRatingTest ON H3_HR = HR_PK AND H3_HT = HT_PK
		JOIN dbo.GlbPersonPrimaryRelationship ON PPR_PER = PER_PK
		LEFT JOIN dbo.OrgContact PrimaryContact ON PPR_PrimaryTableCode = 'OC' AND PPR_PrimaryId = PrimaryContact.OC_PK
		LEFT JOIN dbo.GlbStaff PrimaryStaff ON PPR_PrimaryTableCode = 'GS' AND PPR_PrimaryId = GS_PK
		LEFT JOIN dbo.ExamAttempt ON EXA_G8 = G8_PK
		LEFT JOIN dbo.OrgHeader PrimaryContactOrg ON PrimaryContactOrg.OH_PK = PrimaryContact.OC_OH 
		LEFT JOIN dbo.OrgAddress ContactAddress on ContactAddress.OA_PK = PrimaryContact.OC_OA_OrgAddress
		LEFT JOIN dbo.OrgAddress ContactOrgMainAddress on ContactOrgMainAddress.OA_OH = PrimaryContactOrg.OH_PK
		LEFT JOIN dbo.GlbBranch StaffBranch on GS_GB_HomeBranch = StaffBranch.GB_PK
		LEFT JOIN dbo.GlbCompany StaffCompany on GC_PK = GB_GC
	WHERE
		(
			H3_TestCompletedUtc IS NOT NULL
			OR EXA_TestCompletedUtc IS NOT NULL
			OR G8_ClosedDateUtc IS NOT NULL
			OR (H3_TestCommencedUtc IS NOT NULL AND DATEADD(mi, EXS_ExamExpiryTimeInMinutes, H3_TestCommencedUtc) < GETUTCDATE())
		)
		AND (@HS_PK IS NULL OR HS_PK = @HS_PK)
		AND (@PER_PK IS NULL OR PER_PK = @PER_PK)
		AND (@G0_PK IS NULL OR G0_PK = @G0_PK)
		AND (H3_TestCommencedUtc IS NULL OR @TestCommencedFrom IS NULL OR H3_TestCommencedUtc >= @TestCommencedFrom)
		AND (H3_TestCommencedUtc IS NULL OR @TestCommencedTo IS NULL OR H3_TestCommencedUtc < @TestCommencedTo)
		AND (H3_TestCompletedUtc IS NULL OR @TestCompletedFrom IS NULL OR H3_TestCompletedUtc >= @TestCompletedFrom)
		AND (H3_TestCompletedUtc IS NULL OR @TestCompletedTo IS NULL OR H3_TestCompletedUtc < @TestCompletedTo)
		AND (PrimaryStaff.GS_PK is not null OR PrimaryContact.OC_OH = @Organisation OR @Organisation is null)
)
SELECT
	PersonName = PER_FullName,
	PersonEmailAddress = HA_EmailAddress,
	PersonNameAndEmailAddress = PER_FullName + ' (' + HA_EmailAddress + ')',
	ExamName = HT_TestName,
	PassMark = HT_PassMark,
	Correct,
	Incorrect,
	Result = ISNULL(CASE WHEN EXA_PK IS NOT NULL AND H3_TestCompletedUtc IS NULL THEN EXA_Score ELSE H3_SkillTestRating END, 0),
	[Status] = CASE WHEN (H3_TestCompletedUtc IS NOT NULL AND H3_SkillTestRating >= HT_PassMark) OR (H3_TestCompletedUtc IS NULL AND EXA_Score >= HT_PassMark) THEN 'Passed' ELSE 'Failed' END,
	ExamCommenceDate = CASE WHEN EXA_PK IS NOT NULL AND H3_TestCompletedUtc IS NULL THEN EXA_TestCommencedUtc ELSE H3_TestCommencedUtc END,
	ExamCompletionDate = CASE WHEN EXA_PK IS NOT NULL AND H3_TestCompletedUtc IS NULL THEN EXA_TestCompletedUtc ELSE COALESCE(H3_TestCompletedUtc, G8_ClosedDateUtc) END,
	CompaignName = G0_CampaignName,
	JobSkillCode = dbo.CLRCssvAgg(HS_Code + CASE WHEN (H3_TestCompletedUtc IS NOT NULL AND H3_SkillTestRating >= HT_PassMark) OR (H3_TestCompletedUtc IS NULL AND EXA_Score >= HT_PassMark)  THEN '*' ELSE '' END),
	JobSkill = dbo.CLRCssvAgg(HS_SkillDescription + CASE WHEN (H3_TestCompletedUtc IS NOT NULL AND H3_SkillTestRating >= HT_PassMark) OR (H3_TestCompletedUtc IS NULL AND EXA_Score >= HT_PassMark)  THEN '*' ELSE '' END), 
	OrganisationName = OH_FullName,
	OrganisationCode = OH_Code,
	PrimaryWorkplace = OC_PrimaryWorkplace,
	WorkingLocation = OC_WorkingLocation,
	Branch = OC_Branch,
	PER_PK,
	G0_PK,
	OH_PK
FROM
	(
		SELECT
			PER_PK,
			PER_FullName,
			HA_EmailAddress,
			HT_TestName,
			HT_PassMark,
			H3_PK,
			H3_SkillTestRating,
			H3_TestCommencedUtc,
			H3_TestCompletedUtc,
			G8_PK,
			G8_ClosedDateUtc,
			HS_Code,
			HS_SkillDescription,
			G0_PK,
			G0_CampaignName,
			OH_PK,
			OH_FullName,
			OH_Code,
			OC_PrimaryWorkplace,
			OC_WorkingLocation,
			OC_Branch,
			EXA_PK,
			EXA_Score,
			EXA_TestCommencedUtc,
			EXA_TestCompletedUtc,
			Correct = SUM(CASE HY_ExamCorrectAnswer WHEN 'Y' THEN 1 ELSE 0 END),
			Incorrect = SUM(CASE HY_ExamCorrectAnswer WHEN 'Y' THEN 0 ELSE 1 END)
		FROM
			EXAM
			LEFT JOIN dbo.VoteExamSurveyAnswer ON HZ_G8 = G8_PK
			LEFT JOIN dbo.VoteExamSurveyQuestion ON HZ_HY = HY_PK
		WHERE
			Ranking = 1
		GROUP BY
			PER_PK,
			PER_FullName,
			HA_EmailAddress,
			HT_TestName,
			HT_PassMark,
			H3_PK,
			H3_SkillTestRating,
			H3_TestCommencedUtc,
			H3_TestCompletedUtc,
			G8_PK,
			G8_ClosedDateUtc,
			HS_Code,
			HS_SkillDescription,
			G0_PK,
			G0_CampaignName,
			OH_PK,
			OH_FullName,
			OH_Code,
			OC_PrimaryWorkplace,
			OC_WorkingLocation,
			OC_Branch,
			EXA_PK,
			EXA_Score,
			EXA_TestCommencedUtc,
			EXA_TestCompletedUtc
	) a
GROUP BY
	PER_PK,
	PER_FullName,
	HA_EmailAddress,
	HT_TestName,
	HT_PassMark,
	H3_SkillTestRating,
	H3_TestCommencedUtc,
	H3_TestCompletedUtc,
	G8_ClosedDateUtc,
	G0_PK,
	G0_CampaignName,
	OH_PK,
	OH_FullName,
	OH_Code,
	OC_PrimaryWorkplace,
	OC_WorkingLocation,
	OC_Branch,
	EXA_PK,
	EXA_Score,
	EXA_TestCommencedUtc,
	EXA_TestCompletedUtc,
	Correct,
	Incorrect
", "DROP FUNCTION Report_ApplicantExamResults", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Report_OrgExamResults

			new DatabaseViewAndRoutineCreateScript("Report_OrgExamResults",
@"create FUNCTION Report_OrgExamResults
(
	@Organisation uniqueidentifier,
	@HS_PK UNIQUEIDENTIFIER, -- Job Skill
	@PER_PK UNIQUEIDENTIFIER = NULL,  -- Person
	@G0_PK UNIQUEIDENTIFIER = NULL,  -- Certificate Exam
	@WorkingCountryPortCodes dbo.TVP_VARCHAR_250 READONLY,
	@WorkingCountryPortCodesIsEmpty BIT,
	@TestCommencedFrom DATETIME = NULL,
	@TestCommencedTo DATETIME = NULL,
	@TestCompletedFrom DATETIME = NULL,
	@TestCompletedTo DATETIME = NULL
)
RETURNS TABLE
WITH SCHEMABINDING
AS 
RETURN

WITH Exam AS
(
	SELECT
		PER_PK,
		PER_FullName,
		OC_Email,
		HT_TestName,
		HT_PassMark,
		H3_PK,
		H3_SkillTestRating,
		H3_TestCommencedUtc,
		H3_TestCompletedUtc,
		G8_PK,
		G8_ClosedDateUtc,
		HS_Code,
		HS_SkillDescription,
		G0_PK,
		G0_CampaignName,
		OH_PK,
		OH_FullName,
		OH_Code,
		EXA_PK,
		EXA_Score,
		EXA_TestCommencedUtc,
		EXA_TestCompletedUtc,
		OC_PrimaryWorkplace,
		OC_WorkingLocation,
		OC_Branch,
		Ranking = ROW_NUMBER() OVER (PARTITION BY H3_PK, OH_PK ORDER BY EXA_TestCompletedUtc DESC)
	FROM
		(
			SELECT DISTINCT
				OC_PK, 
				OC_Email,
				OH_PK = ContactOrg.OH_PK,
				OH_Code = ContactOrg.OH_Code,
				OH_FullName = ContactOrg.OH_FullName,
				OC_PrimaryWorkplace = COALESCE(ContactAddress.OA_CompanyNameOverride, ContactOrg.OH_FullName),
				OC_WorkingLocation = COALESCE(ContactAddress.OA_RL_NKRelatedPortCode, ContactOrgMainAddress.OA_RL_NKRelatedPortCode),
				OC_Branch = COALESCE(ContactAddress.OA_Code, ContactOrgMainAddress.OA_Code),
				OC_PER
			FROM 
				dbo.OrgContact
				INNER JOIN dbo.OrgHeader ContactOrg ON OH_PK = OC_OH 
				LEFT JOIN dbo.OrgAddress ContactAddress on ContactAddress.OA_PK = OC_OA_OrgAddress
				LEFT JOIN dbo.OrgAddress ContactOrgMainAddress on ContactOrgMainAddress.OA_OH = ContactOrg.OH_PK
			WHERE
				OC_Email IS NOT NULL
				AND OC_Email <> ''
				AND OC_IsActive = 1
				AND (ContactOrg.OH_FullName LIKE 'CargoWise%' OR OC_Email NOT LIKE '%@cargowise.com')
				AND	@Organisation = OC_OH
				AND
				(
					ContactAddress.OA_PK is null OR
					ContactAddress.OA_OH = @Organisation
				)
		) Contact
		JOIN dbo.GlbPerson on Contact.OC_PER = PER_PK
		JOIN dbo.HRJobApplicant ON OC_PER = HA_PER
		JOIN dbo.GlbPersonPrimaryRelationship on PER_PK = PPR_PER
		JOIN dbo.HRJobApplicantSkillRating ON HR_HA = HA_PK
		JOIN dbo.HRJobSkill ON HR_HS = HS_PK
		JOIN dbo.HRJobSkillTest ON HT_HS = HS_PK
		JOIN dbo.ExamSetting on HT_EXS = EXS_PK
		JOIN dbo.GlbCompanyCampaign ON EXS_G0 = G0_PK
		JOIN dbo.GlbCompanyCampaignItem ON G8_G0 = G0_PK AND G8_RecipientID = HA_PK
		JOIN dbo.HRJobApplicantSkillRatingTest ON H3_HR = HR_PK AND H3_HT = HT_PK
		LEFT JOIN dbo.ExamAttempt ON EXA_G8 = G8_PK
		LEFT JOIN dbo.GlbStaff PrimaryStaff on GS_PK = PPR_PrimaryId
		LEFT JOIN dbo.GlbBranch StaffBranch on GB_PK = PrimaryStaff.GS_GB_HomeBranch
	WHERE
		(
			H3_TestCompletedUtc IS NOT NULL
			OR EXA_TestCompletedUtc IS NOT NULL
			OR G8_ClosedDateUtc IS NOT NULL
			OR (H3_TestCommencedUtc IS NOT NULL AND DATEADD(mi, EXS_ExamExpiryTimeInMinutes, H3_TestCommencedUtc) < GETUTCDATE())
		)
		AND (@HS_PK IS NULL OR HS_PK = @HS_PK)
		AND (@PER_PK IS NULL OR PER_PK = @PER_PK)
		AND (@G0_PK IS NULL OR G0_PK = @G0_PK)
		AND (H3_TestCommencedUtc IS NULL OR @TestCommencedFrom IS NULL OR H3_TestCommencedUtc >= @TestCommencedFrom)
		AND (H3_TestCommencedUtc IS NULL OR @TestCommencedTo IS NULL OR H3_TestCommencedUtc < @TestCommencedTo)
		AND (H3_TestCompletedUtc IS NULL OR @TestCompletedFrom IS NULL OR H3_TestCompletedUtc >= @TestCompletedFrom)
		AND (H3_TestCompletedUtc IS NULL OR @TestCompletedTo IS NULL OR H3_TestCompletedUtc < @TestCompletedTo)
		AND 
		(
			PPR_PrimaryId = OC_PK
			OR 
			PPR_PrimaryId = GS_PK
		)
)
SELECT
	PersonName = PER_FullName,
	PersonEmailAddress = OC_Email,
	PersonNameAndEmailAddress = PER_FullName + ' (' + OC_Email + ')',
	ExamName = HT_TestName,
	PassMark = HT_PassMark,
	Correct,
	Incorrect,
	Result = ISNULL(CASE WHEN EXA_PK IS NOT NULL AND H3_TestCompletedUtc IS NULL THEN EXA_Score ELSE H3_SkillTestRating END, 0),
	[Status] = CASE WHEN (H3_TestCompletedUtc IS NOT NULL AND H3_SkillTestRating >= HT_PassMark) OR (H3_TestCompletedUtc IS NULL AND EXA_Score >= HT_PassMark) THEN 'Passed' ELSE 'Failed' END,
	ExamCommenceDate = CASE WHEN EXA_PK IS NOT NULL AND H3_TestCompletedUtc IS NULL THEN EXA_TestCommencedUtc ELSE H3_TestCommencedUtc END,
	ExamCompletionDate = CASE WHEN EXA_PK IS NOT NULL AND H3_TestCompletedUtc IS NULL THEN EXA_TestCompletedUtc ELSE COALESCE(H3_TestCompletedUtc, G8_ClosedDateUtc) END,
	CompaignName = G0_CampaignName,
	JobSkillCode = dbo.CLRCssvAgg(HS_Code + CASE WHEN (H3_TestCompletedUtc IS NOT NULL AND H3_SkillTestRating >= HT_PassMark) OR (H3_TestCompletedUtc IS NULL AND EXA_Score >= HT_PassMark)  THEN '*' ELSE '' END),
	JobSkill = dbo.CLRCssvAgg(HS_SkillDescription + CASE WHEN (H3_TestCompletedUtc IS NOT NULL AND H3_SkillTestRating >= HT_PassMark) OR (H3_TestCompletedUtc IS NULL AND EXA_Score >= HT_PassMark)  THEN '*' ELSE '' END), 
	OrganisationName = OH_FullName,
	OrganisationCode = OH_Code,
	PrimaryWorkplace = OC_PrimaryWorkplace,
	WorkingLocation = OC_WorkingLocation,
	Branch = OC_Branch,
	PER_PK,
	G0_PK,
	OH_PK
FROM
	(
		SELECT
			PER_PK,
			PER_FullName,
			OC_Email,
			HT_TestName,
			HT_PassMark,
			H3_PK,
			H3_SkillTestRating,
			H3_TestCommencedUtc,
			H3_TestCompletedUtc,
			G8_PK,
			G8_ClosedDateUtc,
			HS_Code,
			HS_SkillDescription,
			G0_PK,
			G0_CampaignName,
			OH_PK,
			OH_FullName,
			OH_Code,
			OC_PrimaryWorkplace,
			OC_WorkingLocation,
			OC_Branch,
			EXA_PK,
			EXA_Score,
			EXA_TestCommencedUtc,
			EXA_TestCompletedUtc,
			Correct = SUM(CASE HY_ExamCorrectAnswer WHEN 'Y' THEN 1 ELSE 0 END),
			Incorrect = SUM(CASE HY_ExamCorrectAnswer WHEN 'Y' THEN 0 ELSE 1 END)
		FROM
			EXAM
			LEFT JOIN dbo.VoteExamSurveyAnswer ON HZ_G8 = G8_PK
			LEFT JOIN dbo.VoteExamSurveyQuestion ON HZ_HY = HY_PK
		WHERE
			Ranking = 1
		GROUP BY
			PER_PK,
			PER_FullName,
			OC_Email,
			HT_TestName,
			HT_PassMark,
			H3_PK,
			H3_SkillTestRating,
			H3_TestCommencedUtc,
			H3_TestCompletedUtc,
			G8_PK,
			G8_ClosedDateUtc,
			HS_Code,
			HS_SkillDescription,
			G0_PK,
			G0_CampaignName,
			OH_PK,
			OH_FullName,
			OH_Code,
			OC_PrimaryWorkplace,
			OC_WorkingLocation,
			OC_Branch,
			EXA_PK,
			EXA_Score,
			EXA_TestCommencedUtc,
			EXA_TestCompletedUtc
	) a
WHERE
	(
		@WorkingCountryPortCodesIsEmpty = 1
		OR 
		(OC_WorkingLocation IN (SELECT VALUE FROM @WorkingCountryPortCodes WHERE LEN(VALUE) = 5))
		OR 
		(LEFT(OC_WorkingLocation, 2) IN (SELECT VALUE FROM @WorkingCountryPortCodes WHERE LEN(VALUE) = 2))
	)
GROUP BY
	PER_PK,
	PER_FullName,
	OC_Email,
	HT_TestName,
	HT_PassMark,
	H3_SkillTestRating,
	H3_TestCommencedUtc,
	H3_TestCompletedUtc,
	G8_ClosedDateUtc,
	G0_PK,
	G0_CampaignName,
	OH_PK,
	OH_FullName,
	OH_Code,
	OC_PrimaryWorkplace,
	OC_WorkingLocation,
	OC_Branch,
	EXA_PK,
	EXA_Score,
	EXA_TestCommencedUtc,
	EXA_TestCompletedUtc,
	Correct,
	Incorrect
", "DROP FUNCTION Report_OrgExamResults", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Report_CertificationStatus

			new DatabaseViewAndRoutineCreateScript("Report_CertificationStatus",
@"CREATE FUNCTION Report_CertificationStatus
(
	@Organisation uniqueidentifier,
	@IncludeManagedOrgs char(1),
	@HA_PK UNIQUEIDENTIFIER = NULL,  --Job Applicant
	@EnterpriseCode UNIQUEIDENTIFIER = NULL,
	@CertificateType char(3) = NULL,
	@CertificateIssueDateFrom DATETIME = NULL,
	@CertificateIssueDateTo DATETIME = NULL,
	@CertificateExpiryOrDueDateFrom DATETIME = NULL,
	@CertificateExpiryOrDueDateTo DATETIME = NULL
)
RETURNS TABLE
WITH SCHEMABINDING
AS 
RETURN

WITH Applicant AS
(
	SELECT
		HA_PK,
		PER_FullName,
		HA_EmailAddress,
		XZ_Type,
		XZ_IssueDate,
		XZ_ExpiryOrDueDate,
		LE_PK,
		LE_EnterpriseCode,
		OH_PK,
		OH_FullName,
		Ranking = ROW_NUMBER() OVER (PARTITION BY HA_PK, OH_PK, XZ_Type ORDER BY CASE WHEN XZ_RefNumber <> '' THEN 0 ELSE 1 END, XZ_IssueDate DESC)
	FROM 
		(
			SELECT DISTINCT
				OC_PK,
				OC_Email,
				OH_PK = COALESCE(EnterpriseOrg.OH_PK, ContactOrg.OH_PK),
				OH_FullName = COALESCE(EnterpriseOrg.OH_FullName, ContactOrg.OH_FullName),
				LE_PK,
				LE_EnterpriseCode
			FROM
				dbo.OrgContact
				INNER JOIN dbo.OrgHeader ContactOrg ON OH_PK = OC_OH
				LEFT JOIN dbo.LicenceCompany ON LC_OH = ContactOrg.OH_PK
				LEFT JOIN dbo.LicenceEnterprise ON LC_LE = LE_PK
				LEFT JOIN dbo.OrgHeader EnterpriseOrg ON EnterpriseOrg.OH_PK = LE_OH
			WHERE
				OC_Email IS NOT NULL
				AND OC_Email <> ''
				AND OC_IsActive = 1
				AND (ContactOrg.OH_FullName LIKE 'CargoWise%' OR OC_Email NOT LIKE '%@cargowise.com')
				AND
				(
					@Organisation IS NULL
					OR @Organisation = OC_OH
					OR 
					(
						@IncludeManagedOrgs = 'Y'
						AND OC_OH IN 
						(
							SELECT PR_OH_Parent 
							FROM dbo.OrgRelatedParty   
							WHERE
								PR_OH_RelatedParty = @Organisation
								AND PR_PartyType = 'MNG'
						)
					)
				)
		) Contact
		JOIN dbo.HRJobApplicant ON OC_Email = HA_EmailAddress
		JOIN dbo.GlbPerson on HA_PER = PER_PK
		JOIN dbo.GenRegCertAccredMaintList ON XZ_ParentID = HA_PK
	WHERE
		(@HA_PK IS NULL OR HA_PK = @HA_PK)
		AND (@EnterpriseCode IS NULL OR LE_PK = @EnterpriseCode)
		AND (@CertificateType IS NULL OR XZ_Type = @CertificateType)
		AND (@CertificateIssueDateFrom IS NULL OR XZ_IssueDate >= @CertificateIssueDateFrom)
		AND (@CertificateIssueDateTo IS NULL OR XZ_IssueDate <= @CertificateIssueDateTo)
		AND (@CertificateExpiryOrDueDateFrom IS NULL OR XZ_ExpiryOrDueDate >= @CertificateExpiryOrDueDateFrom)
		AND (@CertificateExpiryOrDueDateTo IS NULL OR XZ_ExpiryOrDueDate <= @CertificateExpiryOrDueDateTo)

)
SELECT
	ApplicantName = PER_FullName,
	ApplicantEmailAddress = HA_EmailAddress,
	ApplicantNameAndEmailAddress = PER_FullName + ' (' + HA_EmailAddress + ')',
	CertificateType = XZ_Type,
	CertificateIssueDate = XZ_IssueDate,
	CertificateExpiryOrDueDate = XZ_ExpiryOrDueDate,
	EnterpriseCode = LE_EnterpriseCode,
	OrganisationName = OH_FullName,
	OrganisationNameAndEnterpriseCode = LE_EnterpriseCode + ', ' + OH_FullName,
	HA_PK,
	LE_PK,
	OH_PK
FROM
(
	SELECT
		HA_PK,
		PER_FullName,
		HA_EmailAddress,
		XZ_Type,
		XZ_IssueDate,
		XZ_ExpiryOrDueDate,
		LE_PK,
		LE_EnterpriseCode,
		OH_PK,
		OH_FullName
	FROM Applicant
	WHERE Ranking = 1
) a
GROUP BY
	PER_FullName,
	XZ_Type,
	XZ_IssueDate,
	XZ_ExpiryOrDueDate,
	LE_EnterpriseCode,
	OH_FullName,
	HA_EmailAddress,
	HA_PK,
	LE_PK,
	OH_PK

", "DROP FUNCTION Report_CertificationStatus", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Report_CompetencyCompletionResults

			new DatabaseViewAndRoutineCreateScript("Report_CompetencyCompletionResults",
@"CREATE FUNCTION Report_CompetencyCompletionResults
(
	@Organisation uniqueidentifier,
	@IncludeManagedOrgs char(1),
	@HS_PK UNIQUEIDENTIFIER, --Job Skill
	@HA_PK UNIQUEIDENTIFIER = NULL, --Job Applicant
	@TestCommencedFrom DATETIME = NULL,
	@TestCommencedTo DATETIME = NULL,
	@TestCompletedFrom DATETIME = NULL,
	@TestCompletedTo DATETIME = NULL
)
RETURNS TABLE
WITH SCHEMABINDING
AS 
RETURN

WITH Exam AS
(
	SELECT
		HA_PK,
		PER_FullName,
		HA_EmailAddress,
		PER_RN_NKCountry,
		RN_PK,
		HT_TestName,
		HT_PassMark,
		H3_PK,
		H3_SkillTestRating,
		H3_TestCommencedUtc,
		H3_TestCompletedUtc,
		G8_PK,
		G8_ClosedDateUtc,
		HS_Code,
		HS_SkillDescription,
		G0_PK,
		G0_CampaignName,
		OH_PK,
		OH_FullName,
		OH_Code,
		LE_PK, 
		LE_EnterpriseCode, 
		GS_PK,
		GS_Code,
		EXA_PK,
		EXA_Score,
		EXA_TestCommencedUtc,
		EXA_TestCompletedUtc,
		Ranking = ROW_NUMBER() OVER (PARTITION BY H3_PK, OH_PK ORDER BY EXA_TestCompletedUtc DESC)
	FROM
		(
			SELECT  DISTINCT
				OC_PK, 
				OC_Email,
				OH_PK = COALESCE(EnterpriseOrg.OH_PK, ContactOrg.OH_PK),
				OH_Code = COALESCE(EnterpriseOrg.OH_Code, ContactOrg.OH_Code),
				OH_FullName = COALESCE(EnterpriseOrg.OH_FullName, ContactOrg.OH_FullName),
				LE_PK, 
				LE_EnterpriseCode, 
				GS_PK,
				GS_Code
			FROM 
				dbo.OrgContact
				INNER JOIN dbo.OrgHeader ContactOrg ON OH_PK = OC_OH 
				LEFT JOIN dbo.LicenceCompany ON LC_OH = ContactOrg.OH_PK
				LEFT JOIN dbo.LicenceEnterprise ON LC_LE = LE_PK
				LEFT JOIN dbo.OrgHeader EnterpriseOrg ON EnterpriseOrg.OH_PK = LE_OH
				LEFT JOIN dbo.OrgStaffAssignments ON O8_OH = EnterpriseOrg.OH_PK AND O8_Role = 'RM1'
				LEFT JOIN dbo.GlbStaff ON GS_Code = O8_GS_NKPersonResponsible
			WHERE
				OC_Email IS NOT NULL
				AND OC_Email <> ''
				AND OC_IsActive = 1
				AND (ContactOrg.OH_FullName LIKE 'CargoWise%' OR OC_Email NOT LIKE '%@cargowise.com')
				AND
				(
					@Organisation IS NULL
					OR @Organisation = OC_OH
					OR 
					(
						@IncludeManagedOrgs = 'Y'
						AND OC_OH IN 
						(
							SELECT PR_OH_Parent 
							FROM dbo.OrgRelatedParty   
							WHERE
								PR_OH_RelatedParty = @Organisation
								AND PR_PartyType = 'MNG'
						)
					)
				)
		) Contact
		JOIN dbo.HRJobApplicant ON OC_Email = HA_EmailAddress
		JOIN dbo.GlbPerson on HA_PER = PER_PK
		JOIN dbo.HRJobApplicantSkillRating ON HR_HA = HA_PK
		JOIN dbo.HRJobSkill ON HR_HS = HS_PK
		JOIN dbo.HRJobSkillTest ON HT_HS = HS_PK
		JOIN dbo.ExamSetting on HT_EXS = EXS_PK
		JOIN dbo.GlbCompanyCampaign ON EXS_G0 = G0_PK
		LEFT JOIN dbo.HRJobApplicantSkillRatingTest ON H3_HR = HR_PK AND H3_HT = HT_PK
		LEFT JOIN dbo.GlbCompanyCampaignItem ON G8_G0 = G0_PK AND G8_RecipientID = HA_PK
		LEFT JOIN dbo.ExamAttempt ON EXA_G8 = G8_PK
		LEFT JOIN dbo.RefCountry ON RN_Code = PER_RN_NKCountry
	WHERE
		(@HS_PK IS NULL OR HS_PK = @HS_PK)
		AND (@HA_PK IS NULL OR HA_PK = @HA_PK)
		AND (H3_TestCommencedUtc IS NULL OR @TestCommencedFrom IS NULL OR H3_TestCommencedUtc >= @TestCommencedFrom)
		AND (H3_TestCommencedUtc IS NULL OR @TestCommencedTo IS NULL OR H3_TestCommencedUtc < @TestCommencedTo)
		AND (H3_TestCompletedUtc IS NULL OR @TestCompletedFrom IS NULL OR H3_TestCompletedUtc >= @TestCompletedFrom)
		AND (H3_TestCompletedUtc IS NULL OR @TestCompletedTo IS NULL OR H3_TestCompletedUtc < @TestCompletedTo)
)
SELECT 
	ApplicantName = PER_FullName,
	ApplicantEmailAddress = HA_EmailAddress,
	ApplicantNameAndEmailAddress = PER_FullName + ' (' + HA_EmailAddress + ')',
	ApplicantCountry = PER_RN_NKCountry,
	EnterpriseCode = LE_EnterpriseCode,
	OrganisationName = OH_FullName,
	OrganisationCode = OH_Code,
	RelationshipManager = GS_Code,
	JobSkillCode = HS_Code,
	JobSkill = HS_SkillDescription,
	CompetencyCommenceDate = MIN(ExamCommenceDate),
	CompetencyCompletionDate =
		CASE 
			WHEN MAX(CASE WHEN ExamCompletionDate IS NULL THEN 1 ELSE 0 END) = 0 AND MIN(Passed) = 1 THEN MAX(ExamCompletionDate)
		END,
	IsCompleted = 
		CASE 
			WHEN MAX(CASE WHEN ExamCompletionDate IS NULL THEN 1 ELSE 0 END) = 0 AND MIN(Passed) = 1 THEN 'Completed'
			ELSE 'Not Completed'
		END,
	HA_PK,
	LE_PK,
	OH_PK,
	RN_PK
from
(
	select 
		HA_PK,
		PER_FullName,
		HA_EmailAddress,
		PER_RN_NKCountry,
		RN_PK,
		HT_TestName,
		HT_PassMark,
		H3_PK,
		H3_SkillTestRating,
		H3_TestCommencedUtc,
		H3_TestCompletedUtc,
		G8_PK,
		G8_ClosedDateUtc,
		HS_Code,
		HS_SkillDescription,
		G0_PK,
		G0_CampaignName,
		OH_PK,
		OH_FullName,
		OH_Code,
		LE_PK, 
		LE_EnterpriseCode, 
		GS_PK,
		GS_Code,
		EXA_PK,
		EXA_Score,
		EXA_TestCommencedUtc,
		EXA_TestCompletedUtc,
		ExamCommenceDate = CASE WHEN EXA_PK IS NOT NULL AND H3_TestCompletedUtc IS NULL THEN EXA_TestCommencedUtc ELSE H3_TestCommencedUtc END,
		ExamCompletionDate = CASE WHEN EXA_PK IS NOT NULL AND H3_TestCompletedUtc IS NULL THEN EXA_TestCompletedUtc ELSE COALESCE(H3_TestCompletedUtc, G8_ClosedDateUtc) END,
		Passed = CASE WHEN (H3_TestCompletedUtc IS NOT NULL AND H3_SkillTestRating >= HT_PassMark) OR (H3_TestCompletedUtc IS NULL AND EXA_Score >= HT_PassMark) THEN 1 ELSE 0 END
	from Exam
	where Ranking = 1
) a
GROUP BY 
	PER_FullName,
	HA_EmailAddress,
	PER_RN_NKCountry,
	LE_EnterpriseCode,
	OH_FullName,
	OH_Code,
	GS_Code,
	HS_Code,
	HS_SkillDescription,
	HA_PK,
	LE_PK,
	OH_PK,
	RN_PK

", "DROP FUNCTION Report_CompetencyCompletionResults", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Report_StlPriceListDiscounts

			new DatabaseViewAndRoutineCreateScript("Report_StlPriceListDiscounts",
				@"CREATE FUNCTION Report_StlPriceListDiscounts
(
	@DiscountName varchar(20),
	@ReportDate datetime,
	@DiscountDescriptions TVP_CodeDescriptionMapping readonly
)
RETURNS TABLE
AS 
RETURN
SELECT
	ph.L6_PricelistVersion as StlPriceListVersion,
	ph.L6_PK as StlPriceListPK,
	phd.PHD_Percent as DiscountPercent,
	phd.PHD_Name as DiscountCode,
	dd.Description As DiscountDescription
FROM
	dbo.EdiPriceHeaderDiscount phd 
	INNER JOIN dbo.ClientLicencePriceHeader ph on phd.PHD_Version = ph.L6_DiscountCode 
	LEFT JOIN @DiscountDescriptions dd on dd.Code = phd.PHD_Name
WHERE phd.PHD_Name = @DiscountName
	AND ph.L6_PK IN
	(
		SELECT distinct StlPriceListPK
		FROM
		(
			SELECT ph.L6_PK as StlPriceListPK,
				row_number() over (partition by OH_Code, LD_ServerCode, LE_EnterpriseCode order by PHL_ValidFrom desc) as RowNumber
			from dbo.LicenceCompany lc
			join dbo.LicenceHeader la on LA_LC = LC_PK
			join dbo.LicenceDatabase ld on LA_LD = LD_PK
			join dbo.EdiPriceHeaderLink phl on PHL_LD = LD_PK
			join dbo.ClientLicencePriceHeader ph on PHL_L6 = L6_PK and PHL_LD = LD_PK
			join dbo.LicenceEnterprise le on LC_LE = LE_PK
			cross apply
			(
			 select top 1 ClientInvoiceDelivery.*
			 from dbo.ClientInvoiceDelivery
			 where L9_LC = lc.LC_PK and L9_SystemCode in ((case when phl.PHL_PK is null then 'ODM' else 'STL' end), 'ALL') and L9_ServerCode in ('', LD_ServerCode)
			 ORDER BY L9_SystemCode desc, L9_ServerCode asc
			) delivery
			join dbo.OrgHeader oh on ISNULL(L9_OH_InvoiceTo, lc.LC_OH) = oh.OH_PK
			WHERE @ReportDate >= L6_ValidFrom AND (@ReportDate <= L6_ValidTo OR L6_ValidTo is null)
			and @ReportDate >= PHL_ValidFrom AND (@ReportDate <= PHL_ValidTo OR PHL_ValidTo is null)
			and OH_IsActive = 1
			and LD_IsActive = 1
			and LA_IsActive = 1
			and LD_LicenceType = 'PRD'
			and L6_SystemCode = 'STL'
			and L6_RX_NkCurrency = 'USD'
			and L6_HasExchangeRates = 1
		) as ResultTable
		WHERE RowNumber = 1
	)
ORDER BY StlPriceListPK
OFFSET 0 ROWS
", "DROP FUNCTION Report_StlPriceListDiscounts", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region Report_EdiPricelistMaster

			new DatabaseViewAndRoutineCreateScript("Report_EdiPricelistMaster",
@"CREATE PROC Report_EdiPricelistMaster
(
	@ReportType VARCHAR(100),
	@ReportingDate DATETIME
)
AS
BEGIN

	DECLARE @AllStlPricelist TABLE
	(
		STL_L6 UNIQUEIDENTIFIER PRIMARY KEY
	);

	INSERT INTO @AllStlPricelist(STL_L6)
	SELECT L6_PK
	FROM dbo.ClientLicencePriceHeader
	WHERE L6_SystemCode NOt IN ('LDS', 'MSC', 'ODM', 'PUR');

	DECLARE @StlDiscountNameXml XML = ( SELECT cast(SD_BinaryValue AS NVARCHAR(MAX)) FROM dbo.StmData WHERE SD_Name = 'StlDiscountTypes' );
	DECLARE @StlDiscountName TABLE 
	(
		Name VARCHAR(50) NOT NULL
		,Description VARCHAR(100) NOT NULL
	);

	INSERT INTO @StlDiscountName ( Name ,Description )
	SELECT Code = NodeTable.Item.value('Code[1]', 'varchar(50)')
		,Description = NodeTable.Item.value('Description[1]', 'varchar(100)')
	FROM @StlDiscountNameXml.nodes('/ArrayOfCodeDescriptionBool/CodeDescriptionBool') AS NodeTable(Item);

	IF @ReportType = 'Global Settings'
	BEGIN
		SELECT ph.L6_PricelistVersion AS StlPriceListVersion
			,ph.L6_PK AS StlPriceListPK
			,er.PHE_GroupCode AS GroupCode
			,er.PHE_RX_NKCurrency AS Currency
			,er.PHE_Rate AS USDBaseRate
			,er.PHE_UpliftPercent AS Uplift
		FROM dbo.EdiPriceHeaderExchangeRate er
		JOIN dbo.ClientLicencePriceHeader ph ON er.PHE_L6 = ph.L6_PK
		JOIN @AllStlPricelist ON STL_L6 = L6_PK
		ORDER BY StlPriceListVersion, StlPriceListPK, GroupCode ,Currency

		RETURN;
	END
	ELSE IF @ReportType = 'Client Specific Settings'
	BEGIN
		SELECT OrgCode
			,OrgName
			,ServerCode
			,EnterpriseCode
			,StlPriceListVersion
			,StlPriceListPK
			,Currency
			,Volume
			,VolumePercent
			,VolumeBasedPriceItems
			,CorePack
			,CoreUplift
		FROM (
			SELECT oh.OH_Code AS OrgCode
				,oh.OH_FullName AS OrgName
				,ld.LD_ServerCode AS ServerCode
				,le.LE_EnterpriseCode AS EnterpriseCode
				,ph.L6_PricelistVersion AS StlPriceListVersion
				,ph.L6_PK AS StlPriceListPK
				,phl.PHL_RX_NKCurrency AS Currency
				,phl.PHL_VolumeCode AS Volume
				,phl.PHL_VolumePercent AS VolumePercent
				,customerSettings.VolumeBasedPriceItems AS VolumeBasedPriceItems
				,phl.PHL_CorePackCode AS CorePack
				,phl.PHL_CoreUpliftPercent AS CoreUplift
				,row_number() OVER ( PARTITION BY OH_Code ,LD_ServerCode ,LE_EnterpriseCode ORDER BY PHL_ValidFrom DESC ) AS RowNumber
			FROM dbo.LicenceCompany lc
			JOIN dbo.LicenceHeader la ON LA_LC = LC_PK
			JOIN dbo.LicenceDatabase ld ON LA_LD = LD_PK
			JOIN dbo.EdiPriceHeaderLink phl ON PHL_LD = LD_PK
			JOIN dbo.ClientLicencePriceHeader ph ON PHL_L6 = L6_PK AND PHL_LD = LD_PK
			JOIN dbo.LicenceEnterprise le ON LC_LE = LE_PK
			CROSS APPLY (
				SELECT TOP 1 ClientInvoiceDelivery.*
				FROM dbo.ClientInvoiceDelivery
				WHERE L9_LC = lc.LC_PK
					AND L9_SystemCode IN (
						(
							CASE 
								WHEN phl.PHL_PK IS NULL
									THEN 'ODM'
								ELSE 'STL'
								END
							)
						,'ALL'
						)
					AND L9_ServerCode IN ( '', LD_ServerCode )
				ORDER BY L9_SystemCode DESC, L9_ServerCode ASC
				) delivery
			JOIN dbo.OrgHeader oh ON ISNULL(L9_OH_InvoiceTo, lc.LC_OH) = oh.OH_PK
			CROSS APPLY (
				SELECT STRING_AGG(LS9_Name, ',') WITHIN GROUP ( ORDER BY LS9_Name ) AS VolumeBasedPriceItems
				FROM dbo.EdiLicenceSetting
				WHERE LS9_LD = LD_PK
					AND LS9_Type = 'HVF'
					AND LS9_IsActive = 1
					AND @ReportingDate >= LS9_ValidFrom
					AND ( @ReportingDate <= LS9_ValidTo OR LS9_ValidTo IS NULL )
				) customerSettings
			WHERE @ReportingDate >= L6_ValidFrom
				AND ( @ReportingDate <= L6_ValidTo OR L6_ValidTo IS NULL )
				AND @ReportingDate >= PHL_ValidFrom
				AND ( @ReportingDate <= PHL_ValidTo OR PHL_ValidTo IS NULL )
				AND OH_IsActive = 1
				AND LD_IsActive = 1
				AND LA_IsActive = 1
				AND LD_LicenceType = 'PRD'
				AND L6_SystemCode NOt IN ('LDS', 'MSC', 'ODM', 'PUR')
				AND L6_RX_NkCurrency = 'USD'
				AND L6_HasExchangeRates = 1
			) AS ResultTable
		WHERE RowNumber = 1
		ORDER BY OrgCode

		RETURN;
	END
	ELSE IF @ReportType = 'Pricelist Master'
	BEGIN
		SELECT ph.L6_SystemCode AS [System]
		    ,ph.L6_RX_NKCurrency AS ParentCurrency
			,ph.L6_ValidFrom AS [ValidFrom]
			,ph.L6_DiscountCode AS [DiscountVersion]
			,ph.L6_PricelistVersion AS PricelistVersion
			,ph.L6_PK AS PricelistPK
			,L7_Order
			,L7_Category
			,L7_Code
			,L7_Description
			,L7_FeeType
			,L7_Price
			,L7_LicenceUnits
			,L7_ParentCategory
			,L7_ParentCode
			,L7_UnitBreak
			,L7_UnitBreakParentCode
			,L7_WebParentCode
			,L7_RX_NKCurrency
			,L7_Ref4
			,L7_PGM_DiscountGroupCode
			,L7_ChargeCode
			,L7_DepositChargeCode
			,L7_DiscountChargeCode
			,L7_ChargeBasis
			,L7_Language
			,L7_ExchangeRateGroupCode
			,L7_IsVolumeAdjustmentEligible
			,L7_ProductAvailability
			,L7_ProductDisplayCategory
			,L7_CountryTierCode
		FROM dbo.ClientLicencePriceHeader ph
		JOIN dbo.ClientLicencePriceItem item ON item.L7_L6 = ph.L6_PK
		JOIN @AllStlPricelist ON STL_L6 = L6_PK
		WHERE L6_PricelistVersion <> ''
		ORDER BY PriceListVersion, PriceListPK, L7_Order

		RETURN;
	END
	ELSE IF @ReportType = 'STL-DiscountVolume'
	BEGIN
		SELECT L6_PricelistVersion AS StlPriceListVersion
			,L6_PK AS StlPriceListPK
			,lines.item.value('UnitCount[1]', 'bigint') AS UnitCount
			,lines.item.value('Percent[1]', 'decimal(5,2)') AS Discount
		FROM (
			SELECT CONVERT(XML, CONVERT(NVARCHAR(MAX), PHD_ConfigXml)) PHD_ConfigXmlAsXML
				,ph.L6_PricelistVersion
				,ph.L6_PK
			FROM dbo.EdiPriceHeaderDiscount phd
			JOIN dbo.ClientLicencePriceHeader ph ON phd.PHD_Version = ph.L6_DiscountCode
			JOIN @AllStlPricelist ON STL_L6 = L6_PK
			WHERE phd.PHD_Type = 'VOL'
			) Discount
		CROSS APPLY Discount.PHD_ConfigXmlAsXML.nodes('/VolumeDiscount/Lines/Line') lines(item)
		WHERE L6_PricelistVersion <> ''
		ORDER BY StlPriceListVersion, StlPriceListPK, UnitCount, Discount

		RETURN;
	END
	ELSE IF @ReportType = 'STL Discounts'
	BEGIN
		DECLARE @DiscountType TABLE 
		(
			Type CHAR(3)
			,Description VARCHAR(100)
		);

		INSERT INTO @DiscountType
		VALUES
		('PER', 'Percentage'),
		('VOL', 'Volume'),
		('PRE', 'Prepayment'),
		('DOM', 'Domestic Entity (No Overseas Office)'),
		('DCO', 'Developing Country'),
		('WIS', 'For WiseCloud Customers Only'),
		('COU', 'Country'),
		('MEM', 'Membership'),
		('MDC', 'Master Org Developing Country'),
		('BUN', 'Product Bundle');

		SELECT DISTINCT L6_PricelistVersion AS PricelistVersion
			,PHD_Name AS Name
			,T1.Description AS Description
			,PHD_Type AS [Type]
			,T2.Description AS TypeDescription
			,PHD_Percent AS DiscountPercent
			,IIF(PHD_IsDefaultEnabled = 1, 'Y', 'N') AS Active
		FROM dbo.EdiPriceHeaderDiscount phd
		JOIN dbo.ClientLicencePriceHeader ph ON phd.PHD_Version = ph.L6_DiscountCode
		LEFT JOIN @StlDiscountName T1 ON T1.Name = PHD_Name
		LEFT JOIN @DiscountType T2 ON T2.Type = PHD_Type
		WHERE L6_PricelistVersion <> ''
		ORDER BY 1, 2, 3, 4, 5, 6, 7;

		RETURN;
	END
	ELSE IF @ReportType = 'Discount Structure'
	BEGIN
		SELECT DISTINCT L6_PricelistVersion AS PricelistVersion
			,PGM_GroupCode AS Structure
			,PHD_Name AS Discount
			,T1.Description AS DiscountDescription
		FROM dbo.EdiPriceHeaderDiscount phd
		JOIN dbo.ClientLicencePriceHeader ph ON phd.PHD_Version = ph.L6_DiscountCode
		JOIN dbo.EdiPriceDiscountGroupMember ON PGM_PHD = PHD_PK
		LEFT JOIN @StlDiscountName T1 ON T1.Name = PHD_Name
		LEFT JOIN @DiscountType T2 ON T2.Type = PHD_Type
		WHERE L6_PricelistVersion <> ''
		ORDER BY 1, 2, 3, 4;

		RETURN;
	END
	ELSE IF @ReportType = 'Usage Mapping'
	BEGIN
		SELECT DISTINCT L6_PricelistVersion AS PricelistVersion
			,PUM_PriceCategory AS PriceCategory
			,PUM_PriceCode AS PriceCode
			,PUM_UsageCategory AS UsageCategory
			,PUM_UsageCode AS UsageCode
			,ISNULL(L7_Description, '') AS PriceDescription
		FROM dbo.EdiPriceUsageMapping
		JOIN dbo.ClientLicencePriceHeader ON L6_PK = PUM_L6
		LEFT JOIN (
			SELECT L7_L6, L7_Category, L7_Code, L7_Description
			FROM (
				SELECT L7_L6, L7_Category, L7_Code, L7_Description = LTRIM(L7_Description)
					,ROW_NUMBER() OVER ( PARTITION BY L7_L6 ,L7_Category ,L7_Code ORDER BY L7_Order ) AS RowNumber
				FROM dbo.ClientLicencePriceItem
				WHERE L7_Category <> '' AND L7_Code <> '' AND L7_Description <> ''
				) T
			WHERE RowNumber = 1
			) L7 ON L7.L7_L6 = L6_PK
			AND PUM_PriceCategory = L7_Category
			AND PUM_PriceCode = L7_Code
		WHERE L6_PricelistVersion <> ''
		ORDER BY 1, 2, 3, 4, 5, 6;

		RETURN;
	END
	ELSE IF @ReportType = 'STL-DiscountDeveloping Country'
	BEGIN
		SELECT L6_PricelistVersion AS PricelistVersion
			,lines.item.value('Country[1]', 'char(3)') AS Country
			,lines.item.value('Percent[1]', 'decimal(5,2)') AS [Percent]
			,lines.item.value('RequiresDomesticDiscount[1]', 'char(1)') AS RequiresDomesticDiscount
		FROM (
			SELECT CONVERT(XML, CONVERT(NVARCHAR(MAX), PHD_ConfigXml)) PHD_ConfigXmlAsXML
				,ph.L6_PricelistVersion
				,ph.L6_PK
			FROM dbo.EdiPriceHeaderDiscount phd
			INNER JOIN dbo.ClientLicencePriceHeader ph ON phd.PHD_Version = ph.L6_DiscountCode
			JOIN @AllStlPricelist ON STL_L6 = L6_PK
			WHERE phd.PHD_Type = 'DCO'
			) Discount
		CROSS APPLY Discount.PHD_ConfigXmlAsXML.nodes('/CountryDiscount/Lines/Line') lines(item)
		WHERE L6_PricelistVersion <> ''
		ORDER BY 1, 2, 3, 4;

		RETURN;
	END
	ELSE
	BEGIN
		RAISERROR ('%s Not Implemented!', 16, 1, @ReportType);
	END
END
", "DROP PROCEDURE Report_EdiPricelistMaster", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region Report_EdiStlBilling

			new DatabaseViewAndRoutineCreateScript("Report_EdiStlBilling",
@"CREATE PROC Report_EdiStlBilling
(
	@OrgPK UNIQUEIDENTIFIER,
	@DateTo DATETIME,
	@Product VARCHAR(3)
)
AS
BEGIN

	DECLARE @PeriodStart DATETIME = DATEADD(MONTH, DATEDIFF(MONTH, 0, @DateTo), 0);
	DECLARE @PeriodEnd DATETIME =  DATEADD(SECOND, -1, DATEADD(DAY, 1, DATEDIFF(DAY, 0, @DateTo)));

	SELECT PayingOrg = payingParty.OH_Code
		,LD_DatabaseNumber = ISNULL(LD_DatabaseNumber, '')
		,AG_AccountNum
		,AG_Description
		,GB_Code
		,AH_PostDate
		,BU9_PeriodStart
		,BU9_UnitCount
		,BU9_PriceCode
		,BU9_UnitPrice
		,BU9_PriceCurrency
		,BU9_LocalAmountPreDiscount
		,BU9_LocalAmountPostDiscount
		,BU9_TransactionAmountPreDiscount
		,BU9_TransactionAmountPostDiscount
		,BU9_BillingModel
		,L7_Order
		,L7_FeeType
		,L7_Price
		,L7_ParentCode
		,L7_WebParentCode
		,L7_LicenceUnits
		,L7_Description
		,L7_DiscountChargeCode
		,L6_PricelistVersion
		,LE_EnterpriseCode
		,UsingOrg = usingParty.OH_Code
		,AH_IsCancelled
		,AH_TransactionNum
		,AH_RX_NKTransactionCurrency
		,BU9_UsageCode
		,BU9_UsageSubCode
		,Membership = ISNULL(EOR_MembershipType, '')
		,AC_Code
		,PayEnt
		,CompanyCountry = ISNULL(LCC_RN_NKCountryCode, '')
		,HostedLocation = ISNULL(LD_HostedLocation, '')
	FROM (
		-- things with a price item
		SELECT EdiBilledUsage.*
			,L7_Order
			,L7_FeeType
			,L7_Price
			,L7_ParentCode
			,L7_WebParentCode
			,L7_LicenceUnits
			,L7_Description
			,L7_DiscountChargeCode
			,OH_PK = ISNULL(LicenceCompany.LC_OH, v.LC_OH)
			,LE_PK = ISNULL(LicenceCompany.LC_LE, LD_LE)
			,L6_PricelistVersion
		FROM dbo.EdiBilledUsage
		JOIN dbo.ClientLicencePriceItem ON BU9_L7 = L7_PK
		JOIN dbo.ClientLicencePriceHeader ON L7_L6 = L6_PK
		JOIN dbo.AccTransactionHeader ON BU9_AH_Invoice = AH_PK
		JOIN dbo.LicenceDatabase ON BU9_LD = LD_PK
		JOIN dbo.EdiViewLicenceDatabaseOwner v ON v.LD_PK = LicenceDatabase.LD_PK
		LEFT JOIN dbo.LicenceCompany ON LicenceCompany.LC_PK = BU9_LC
		WHERE AH_PostDate >= @PeriodStart
			AND AH_PostDate < @PeriodEnd
			AND (LD_Product = @Product
				OR (@Product IN ('ENT', 'CW1', 'CWN') AND LD_Product IN ('ENT', 'CW1', 'CWN', 'CGW')))
		UNION ALL
	
		-- things with a database and no price item such as database fees
		SELECT EdiBilledUsage.*
			,L7_Order = 0
			,L7_FeeType = ''
			,L7_Price = BU9_UnitPrice
			,L7_ParentCode = ''
			,L7_WebParentCode = ''
			,L7_LicenceUnits = 0
			,L7_Description = ''
			,L7_DiscountChargeCode = ''
			,OH_PK = LC_OH
			,LE_PK = LD_LE
			,L6_PricelistVersion = 'Non Pricelist Item'
		FROM dbo.EdiBilledUsage
		JOIN dbo.LicenceDatabase ON BU9_LD = LD_PK
		JOIN dbo.AccTransactionHeader ON BU9_AH_Invoice = AH_PK
		JOIN dbo.EdiViewLicenceDatabaseOwner v ON v.LD_PK = LicenceDatabase.LD_PK
		WHERE AH_PostDate >= @PeriodStart
			AND AH_PostDate < @PeriodEnd
			AND BU9_L7 IS NULL
			AND ( LD_Product = @Product 
				OR (@Product IN ('ENT', 'CW1', 'CWN') AND LD_Product IN ('ENT', 'CW1', 'CWN', 'CGW')))

		UNION ALL
	
		-- things with no database and no price item such as old-style org fees
		SELECT EdiBilledUsage.*
			,L7_Order = 0
			,L7_FeeType = ''
			,L7_Price = BU9_UnitPrice
			,L7_ParentCode = ''
			,L7_WebParentCode = ''
			,L7_LicenceUnits = 0
			,L7_Description = ''
			,L7_DiscountChargeCode = ''
			,OH_PK = AH_OH
			,LE_PK = LC_LE
			,L6_PricelistVersion = 'Non Pricelist Item'
		FROM dbo.EdiBilledUsage
		JOIN dbo.AccTransactionHeader ON AH_PK = BU9_AH_Invoice
		LEFT JOIN dbo.LicenceCompany ON LC_OH = AH_OH
		WHERE AH_PostDate >= @PeriodStart
			AND AH_PostDate < @PeriodEnd
			AND BU9_L7 IS NULL
			AND BU9_LD IS NULL
		) a
	JOIN dbo.OrgHeader usingParty ON usingParty.OH_PK = a.OH_PK
	LEFT JOIN dbo.LicenceDatabase ON BU9_LD = LD_PK
	LEFT JOIN dbo.LicenceEnterprise ON LicenceEnterprise.LE_PK = a.LE_PK
	JOIN dbo.AccTransactionHeader ON AH_PK = BU9_AH_Invoice
	JOIN dbo.OrgHeader payingParty ON payingParty.OH_PK = AH_OH
	LEFT JOIN (
		SELECT PayOrgPk = payCo.LC_OH
			,PayEnt = payLicEnt.LE_EnterpriseCode
		FROM dbo.LicenceCompany payCo
		JOIN dbo.LicenceEnterprise payLicEnt ON payCo.LC_LE = payLicEnt.LE_PK
		) PayingEnt ON AH_OH = PayingEnt.PayOrgPk
	JOIN dbo.GlbBranch ON AH_GB = GB_PK
	LEFT JOIN dbo.AccChargeCode ON BU9_AC_AmountChargeCode = AC_PK
	LEFT JOIN dbo.AccGLHeader ON AC_AG_RevenueAccount = AG_PK
	LEFT JOIN dbo.ClientCompany ON bu9_lcc = lcc_pk
	OUTER APPLY (
		SELECT TOP 1 EOR_MembershipType
		FROM dbo.EdiOrgMembership
		WHERE EOR_OH = payingParty.OH_PK
			AND EOR_MembershipType IN ( 'FTA', 'CBAFF', 'IFCBAA', 'SAL' )
			AND EOR_ValidFrom <= AH_PostDate
			AND ( EOR_ValidTo IS NULL OR EOR_ValidTo >= AH_PostDate )
		ORDER BY EOR_ValidFrom
		) EOR
	WHERE @OrgPK IS NULL OR @OrgPK = usingParty.OH_PK
	ORDER BY LE_EnterpriseCode, BU9_PeriodStart, usingParty.OH_Code, L7_Order;
END

", "DROP PROCEDURE Report_EdiStlBilling", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region Report_IncidentFeatureRequestDetails

			new DatabaseViewAndRoutineCreateScript("Report_IncidentFeatureRequestDetails",
@"CREATE PROC [dbo].[Report_IncidentFeatureRequestDetails]
(
	@Product VARCHAR(3),
	@Criticality VARCHAR(3),
	@Section VARCHAR(3),
	@CreateDateFromUTC DATETIME,
	@CreateDateToNextDateUTC DATETIME,
	@QuoteDeliveredDateFromUTC DATETIME,
	@QuoteDeliveredDateToNextDateUTC DATETIME
)
AS
BEGIN
	
	SELECT 
	IM_Product,
	IM_Priority,
	IM_Module,
	IM_RN_NKCountry,
	IM_SystemCreateTimeUtc,
	IM_IncidentNumber,
	OH_Code,
	OH_FullName = ISNULL(OH_FullName, ''),
	OC_ContactName = ISNULL(OC_ContactName, ''),

	--Details
	IM_Description,
	IM_RequiredBy,
	WKP_ProjectNumber,
	WKP_Summary,
	IM_ResolutionCode,
	CurrentTask = CurrentTask.P9_Description,
	CurrentTaskEstimatedDate = CurrentTask.P9_ScheduledDate,
	OM_CMClientSize,
	IM_Status,
	LastTaskClosedBy = COALESCE(CurrentTask.P9_GS_NKAssignedStaffMember, LastClosedTask.P9_GS_NKAssignedStaffMember, IIF(IM_Category = 'SUP', IM_GS_NKCustServiceContact, IM_GS_NKAssignedToCurrent)),

	--Estimate
	Estimate.CIE_MinDevelopmentHours,
	Estimate.CIE_MaxDevelopmentHours,
	Estimate.CIE_EstimateSentDateUTC,
	Estimate.CIE_EstimateExpiryDateUTC,
	Estimate.CIE_QuoteRequestedUTC,
	Estimate.CIE_PaymentTerms,
	Estimate.CIE_RX_NKCurrency,
	Estimate.CIE_MinEstimateMonthly,
	Estimate.CIE_MaxEstimateMonthly,
	Estimate.CIE_MinEstimateOneoff,
	Estimate.CIE_MaxEstimateOneoff,
	Estimate.CIE_CancellationFee,
	Estimate.CIE_ExpressDeliveryOptionCutOffDateUTC,

	-- Quote
	Quote.CIQ_MinDevelopmentHours,
	Quote.CIQ_MaxDevelopmentHours,
	Quote.CIQ_QuoteSentDateUTC,
	Quote.CIQ_QuoteExpiryDateUTC,
	Quote.CIQ_QuoteAcceptedDateUTC,
	Quote.CIQ_DeliveredDateUTC,
	Quote.CIQ_Type,
	Quote.CIQ_PaymentTerms,
	Quote.CIQ_RX_NKCurrency,
	Quote.CIQ_QuoteAmount,
	Quote.CIQ_OneoffUpfront,
	Quote.CIQ_CancellationFee,
	Quote.CIQ_HeadStartOptionIncluded,
	Quote.CIQ_HeadStartSurcharge,
	Quote.CIQ_ExpressDeliveryOptionIncluded,
	Quote.CIQ_ExpressDeliverySurcharge,

	--BusinessRequirement
	BusinessRequirement = ST_NoteData

	FROM dbo.IncidentMain

	LEFT JOIN dbo.OrgMiscServ ON OM_OH = IM_OH_Client

	--OrgHeader
	LEFT JOIN dbo.GenPivot FOA ON FOA.XX_Relation1ID = IM_PK AND FOA.XX_RelationType = 'FOA' 
								AND FOA.XX_Relation1TableCode = 'IM' AND FOA.XX_Relation2TableCode = 'OA'
	LEFT JOIN dbo.OrgAddress ON OA_PK = FOA.XX_Relation2ID
	LEFT JOIN dbo.OrgHeader ON OH_PK = OA_OH

	--OrgContact
	LEFT JOIN dbo.GenPivot FOC ON FOC.XX_Relation1ID = IM_PK AND FOC.XX_RelationType = 'FOC'
							AND FOC.XX_Relation1TableCode = 'IM' AND FOC.XX_Relation2TableCode = 'OC'
	LEFT JOIN dbo.OrgContact ON OC_PK = FOC.XX_Relation2ID

	OUTER APPLY
	(
		SELECT TOP 1 WKP_ProjectNumber, WKP_Summary FROM 
		dbo.GenPivot WKP
		JOIN WorkProject ON WKP.XX_Relation1ID = IM_PK AND WKP.XX_RelationType = 'WRK' AND WKP.XX_Relation1TableCode = 'IM' 
						AND WKP.XX_Relation2TableCode = 'WKP'
		ORDER BY WKP_SystemCreateTimeUtc
	) WKP
	
	OUTER APPLY
	(
		SELECT TOP 1 P9_Description, P9_ScheduledDate, P9_GS_NKAssignedStaffMember, P9_G4_RequiredCapability FROM
		dbo.ProcessTasks
		WHERE P9_ParentID = IM_PK
		AND P9_ParentTableCode = 'IM' AND P9_Status NOT IN ('CLS', 'CAN')
		ORDER BY P9_Sequence, P9_TaskID
	) CurrentTask

	OUTER APPLY
	(
		SELECT TOP 1 P9_GS_NKAssignedStaffMember FROM
		dbo.ProcessTasks
		WHERE P9_ParentID = IM_PK
		AND P9_ParentTableCode = 'IM' AND P9_Status  = 'CLS' AND P9_GS_NKAssignedStaffMember <> ''
		AND IM_Status = 'CLS'
		ORDER BY P9_CompletedTimeUtc DESC
	) LastClosedTask

	LEFT JOIN ClientIncidentEstimate Estimate ON CIE_IM = IM_PK
	LEFT JOIN ClientIncidentQuote Quote ON CIQ_IM = IM_PK

	OUTER APPLY
	(
		SELECT TOP 1 ST_NoteData FROM
		dbo.StmNote
		WHERE ST_ParentID = IM_PK AND ST_Table = 'IncidentMain' AND ST_Description = 'Business Requirements'
	) BizReq

	WHERE 
	IM_Product = @Product
	AND IM_Priority = @Criticality
	AND (ISNULL(@Section ,'') = '' OR IM_Module = @Section)
	AND (@CreateDateFromUTC IS NULL OR @CreateDateFromUTC = '' OR IM_SystemCreateTimeUtc >= @CreateDateFromUTC)
	AND (@CreateDateToNextDateUTC IS NULL OR @CreateDateToNextDateUTC = '' OR IM_SystemCreateTimeUtc < @CreateDateToNextDateUTC)
	AND (@QuoteDeliveredDateFromUTC IS NULL OR @QuoteDeliveredDateFromUTC = '' OR Quote.CIQ_DeliveredDateUTC >= @QuoteDeliveredDateFromUTC)
	AND (@QuoteDeliveredDateToNextDateUTC IS NULL OR @QuoteDeliveredDateToNextDateUTC = '' OR Quote.CIQ_DeliveredDateUTC < @QuoteDeliveredDateToNextDateUTC)
	ORDER BY IM_SystemCreateTimeUtc;
END
", "DROP PROCEDURE Report_IncidentFeatureRequestDetails", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region ClientPrepayTransactions

			new DatabaseViewAndRoutineCreateScript("ClientPrepayTransactions",
@"CREATE PROCEDURE ClientPrepayTransactions
	@Company uniqueidentifier,
	@OrgList varchar(8000),
	@TransactionTypeList varchar(8000),
	@PostDateFrom smalldatetime,
	@PostDateTo smalldatetime,
	@ChargeCodeList varchar(8000),
	@DepositCodeList varchar(8000)
AS

SET NOCOUNT ON

exec dbo.EdiDepositBalanceUpdate 0

DECLARE @IsReciprocal char(1)
DECLARE @LocalCurrency char(3)
SELECT @IsReciprocal = GC_IsReciprocal, @LocalCurrency = GC_RX_NKLocalCurrency FROM dbo.GlbCompany WHERE GC_PK = @Company
SET @PostDateTo = DateAdd(dd, 1, Convert(varchar, @PostDateTo, 101))
DECLARE @ReportDate datetime = GetDate()


select 
	AccountPK = Invoice.AH_OH,
	AccountCode = OH_Code,
	AccountName = OH_FullName,	
	BranchCode = GB_Code,
	OSDepositExTax = isnull(OSDeposit, 0),
	OSDepositTax = isnull(OSDepositTax, 0),
	OSDeposit = isnull(OSDeposit + OSDepositTax, 0),
	OSLdaasDepositExTax = isnull(OSLdaasDeposit, 0),
	OSLdaasDepositTax = isnull(OSLdaasDepositTax, 0),
	OSLdaasDeposit = isnull(OSLdaasDeposit + OSLdaasDepositTax, 0),
	OSExLdaasDepositExTax = isnull(OSExLdaasDeposit, 0),
	OSExLdaasDepositTax = isnull(OSExLdaasDepositTax, 0),
	OSExLdaasDeposit = isnull(OSExLdaasDeposit + OSExLdaasDepositTax, 0),
	BranchName = GB_BranchName,
	TransactionType = AH_TransactionType,
	InvoiceRef = AH_TransactionNum,
	Description = AH_Desc,
	InvoiceTotal = -(AH_InvoiceAmount + AH_GSTAmount),
	DueDate = AH_DueDate,
	PostDate = AH_PostDate,
	InvoiceDate = AH_InvoiceDate,
	CurrencyCode = AH_RX_NKTransactionCurrency,
	ExchangeRate = AH_ExchangeRate,
	OSTotal = AH_OSTotal,
	InvoiceExTax = -AH_InvoiceAmount,
	TaxAmount = -AH_GSTAmount,
	LineOSExTaxAmount,
	LineOSTaxAmount,
	LineOSIncTaxAmount,
	LineExTaxAmount,
	LineTaxAmount,
	LineIncTaxAmount,
	OutstandingAmount = isnull(OutstandingAmount, 0),
	OSOutstandingAmount = isnull(OSOutstandingAmount, 0),
	OutstandingCurrencyCount = isnull(CurrencyCount, 0),
	AccountBalanceOnInvoiceDate = isnull(EUI_AccountBalanceIncTax, 0),
	PrepaymentAmountOnInvoiceDate = isnull(EUI_PrepayAmountIncTax, 0)	
from
	(
		select
			AH_PK,
			AH_OH,
			AH_GB,
			AH_GC,
			AH_TransactionType,
			AH_TransactionNum, 
			AH_Desc, 
			AH_InvoiceAmount,
			AH_GSTAmount, 
			AH_DueDate, 
			AH_InvoiceDate, 
			OH_Code, 
			OH_FullName,
			AH_RX_NKTransactionCurrency,
			AH_ExchangeRate,
			AH_OSTotal, 
			AH_PostDate, 
			AH_FullyPaidDate,
			LineOSExTaxAmount = sum(OSExTaxAmount),
			LineOSTaxAmount = sum(AL_OSAmount - OSExTaxAmount),
			LineOSIncTaxAmount = sum(AL_OSAmount),
			LineExTaxAmount = sum(AL_LineAmount),
			LineTaxAmount = sum(AL_GSTVAT),
			LineIncTaxAmount = sum(AL_LineAmount + AL_GSTVAT)
		from 
			dbo.AccTransactionHeader   
			join dbo.AccTransactionLines on AccTransactionLines.AL_AH = AccTransactionHeader.AH_PK 
			join dbo.OrgHeader on OrgHeader.OH_PK = AccTransactionHeader.AH_OH 
			join dbo.AccChargeCode on AccChargeCode.AC_PK = AccTransactionLines.AL_AC 
			join dbo.GlbCompany on AH_GC = GC_PK
			join dbo.GlbBranch on AH_GB = GB_PK
			join dbo.RefCurrency on AL_RX_NKTransactionCurrency = RX_Code
			cross apply
			(
				select OSExTaxAmount = 
					case
						when AL_ExchangeRate = 1 then AL_LineAmount 
						when AL_GSTVAT = 0 then AL_OSAmount
						else round(case when GC_IsReciprocal = 0 then AL_LineAmount * AL_ExchangeRate else AL_LineAmount / AL_ExchangeRate end
							, case when RX_SubUnitRatio <= 1 then 0 else cast(log10(RX_SubUnitRatio) as int) end)
						end
			) OSExTaxAmountCalculation
		where 
			AH_Ledger = 'AR'
			AND AH_GC = @Company
			AND ((AH_PostDate >= @PostDateFrom or @PostDateFrom = '' or @PostDateFrom is null) and (AH_PostDate < @PostDateTo or @PostDateTo = '' or @PostDateTo is null))
			AND (AH_TransactionType in (select value from dbo.SplitStringToTable(@TransactionTypeList, DEFAULT)))
			AND (OrgHeader.OH_Code in (select value from dbo.SplitStringToTable(@OrgList, DEFAULT)) or @OrgList = '' or @OrgList is null)
			AND (AccChargeCode.AC_Code in (select value from dbo.SplitStringToTable(@ChargeCodeList, DEFAULT)) or @ChargeCodeList = '' or @ChargeCodeList is null)
			AND ((AC_ChargeType <> 'CMT') or (AccTransactionLines.AL_AG is not null))
		group by
			AH_PK,
			AH_OH,
			AH_GB,
			AH_GC,
			AH_TransactionType,
			AH_TransactionNum, 
			AH_Desc, 
			AH_InvoiceAmount,
			AH_GSTAmount, 
			AH_DueDate, 
			AH_InvoiceDate, 
			OH_Code, 
			OH_FullName,
			AH_RX_NKTransactionCurrency,
			AH_ExchangeRate,
			AH_OSTotal, 
			AH_PostDate, 
			AH_FullyPaidDate
	) Invoice
	join dbo.GlbBranch on AH_GB = GB_PK	
	left join dbo.EdiUsageInvoice ON AH_PK = EUI_AH_Invoice
	outer apply
		(
			select 
				OSDeposit = sum(OSDeposit), 
				OSDepositTax = sum(OSDepositTax),
				OSLdaasDeposit = sum(OSLdaasDeposit), 
				OSLdaasDepositTax = sum(OSLdaasDepositTax), 
				OSExLdaasDeposit = sum(OSDepositExLDaaS), 
				OSExLdaasDepositTax = sum(OSDepositExLDaaSTax)
			from 
				dbo.EdiDepositBalance
				left join dbo.RefExchangeRate LocalToDeposit
					on LocalToDeposit.RE_GC = AH_GC
					and LocalToDeposit.RE_StartDate <= Invoice.AH_InvoiceDate and Invoice.AH_InvoiceDate <= LocalToDeposit.RE_ExpiryDate
					and LocalToDeposit.RE_RX_NKExCurrency = DEB_RX_NKCurrency
					and LocalToDeposit.RE_ExRateType = 'SEL'
				left join dbo.RefExchangeRate LocalToCurrent
					on  LocalToCurrent.RE_GC = AH_GC
					and LocalToCurrent.RE_StartDate <= Invoice.AH_InvoiceDate and Invoice.AH_InvoiceDate <= LocalToCurrent.RE_ExpiryDate
					and LocalToCurrent.RE_RX_NKExCurrency = Invoice.AH_RX_NKTransactionCurrency
					and LocalToCurrent.RE_ExRateType = 'SEL'
				join dbo.RefCurrency on @LocalCurrency = RX_Code
				left join (select value from dbo.SplitStringToTable(@DepositCodeList, DEFAULT)) d on DEB_ChargeCode = value
				cross apply
				(
					select 
						invoiceRate = round(
							case when @IsReciprocal = 1 
								then (isnull(LocalToDeposit.RE_SellRate, 1.0) / isnull(LocalToCurrent.RE_SellRate, 1.0)) 
								else (isnull(LocalToCurrent.RE_SellRate, 1.0) / isnull(LocalToDeposit.RE_SellRate, 1.0))
							end, 5),
						isLdaasDeposit = case when DEB_ChargeCode in ('LDAASDEP', 'LDASINIPAY') then 1 else 0 end
				) xrate
				cross apply
				(
					select 
						OSDeposit = case when value is not null or isnull(@DepositCodeList, '') = '' then round(DEB_Amount * invoiceRate, 2) else 0 end,
						OSDepositTax = case when value is not null or isnull(@DepositCodeList, '') = '' then round(DEB_Tax * invoiceRate, 2) else 0 end,
						OSLdaasDeposit = case when isLdaasDeposit = 1 then ROUND(DEB_Amount * invoiceRate, 2) else 0 end,
						OSLdaasDepositTax = case when isLdaasDeposit = 1 then ROUND(DEB_Tax * invoiceRate, 2) else 0 end,
						OSDepositExLDaaS = case when (value is not null or isnull(@DepositCodeList, '') = '') and isLdaasDeposit = 0 then round(DEB_Amount * invoiceRate, 2) else 0 end,
						OSDepositExLDaaSTax = case when (value is not null or isnull(@DepositCodeList, '') = '') and isLdaasDeposit = 0 then round(DEB_Tax * invoiceRate, 2) else 0 end
				) util
			where 
				DEB_OH = AH_OH and DEB_Amount != 0
			group by 
				DEB_OH
		) Deposit
	outer apply
		(
			-- Outstanding balance in outer transaction currency
			-- Use the date of the old transaction for the rate
			select
				OSOutstandingAmount = -sum(round(OldHeader.AH_OutstandingAmount * RateAsMultiplier, 2)),
				OutstandingAmount = -sum(OldHeader.AH_OutstandingAmount),
				CurrencyCount = count(distinct OldHeader.AH_RX_NKTransactionCurrency)
			FROM 
				dbo.AccTransactionHeader OldHeader
				left join dbo.RefExchangeRate InvoiceExRate
					on  InvoiceExRate.RE_GC = OldHeader.AH_GC
					and InvoiceExRate.RE_StartDate <= OldHeader.AH_InvoiceDate and OldHeader.AH_InvoiceDate < InvoiceExRate.RE_ExpiryDate
					and InvoiceExRate.RE_RX_NKExCurrency = Invoice.AH_RX_NKTransactionCurrency
					and InvoiceExRate.RE_ExRateType = 'SEL'
				left join dbo.RefExchangeRate CurrentExRate
					on  CurrentExRate.RE_GC = OldHeader.AH_GC
					and CurrentExRate.RE_StartDate <= @ReportDate and @ReportDate < CurrentExRate.RE_ExpiryDate
					and CurrentExRate.RE_RX_NKExCurrency = Invoice.AH_RX_NKTransactionCurrency
					and CurrentExRate.RE_ExRateType = 'SEL'
				cross apply
					(
						select InvoiceRate = 
							case 
								when OldHeader.AH_RX_NKTransactionCurrency = Invoice.AH_RX_NKTransactionCurrency then AH_ExchangeRate 
								else isnull(InvoiceExRate.RE_SellRate, 1.0) 
							end
					) InvoiceRateCalculation
				cross apply
					(
						select CurrentRate = isnull(CurrentExRate.RE_SellRate, 1.0)
					) CurrentRateCalculation
				cross apply
					(
						select Rate = 
							case 
								when @IsReciprocal = 0 then 
									case when InvoiceRate > CurrentRate then InvoiceRate else CurrentRate end
								else
									case when InvoiceRate > CurrentRate then CurrentRate else InvoiceRate end
							end
					) RateCalculation
				cross apply
					(
						select RateAsMultiplier = case when @IsReciprocal = 0 then Rate else 1.0 / Rate end
					) MultiplierCalculation
			where 
				OldHeader.AH_IsCancelled <> 1
				and OldHeader.AH_OutstandingAmount != 0
				and OldHeader.AH_Ledger = 'AR'
				and OldHeader.AH_GC = @Company
				and not OldHeader.AH_TransactionType = 'INB'
				and OldHeader.AH_OH = Invoice.AH_OH
			group by 
				AH_OH
		) Balance

order by 
	oh_code

", "DROP PROCEDURE ClientPrepayTransactions", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region GetDatesInRange

			new DatabaseViewAndRoutineCreateScript("AllDatesInRange",
			@"CREATE FUNCTION AllDatesInRange(@startDate datetime, @endDate datetime)
			returns table as
			return (
			with 
			 N0 as (SELECT 1 as n UNION ALL SELECT 1)
			,N1 as (SELECT 1 as n FROM N0 t1, N0 t2)
			,N2 as (SELECT 1 as n FROM N1 t1, N1 t2)
			,N3 as (SELECT 1 as n FROM N2 t1, N2 t2)
			,N4 as (SELECT 1 as n FROM N3 t1, N3 t2)
			,N5 as (SELECT 1 as n FROM N4 t1, N4 t2)
			,N6 as (SELECT 1 as n FROM N5 t1, N5 t2)
			,nums as (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT 1)) as num FROM N6)
			SELECT DATEADD(day,num-1,@startDate) as Dates
			FROM nums
			WHERE num <= DATEDIFF(day,@startDate,@endDate) + 1
			)
			
			", "drop function AllDatesInRange", DbRoutineType.SqlFunctionInlineTypeDesc),

		#endregion

		#region ClientEDIArap

			new DatabaseViewAndRoutineCreateScript("ClientEDIArap",
@"CREATE PROCEDURE ClientEDIArap
	@PostDateFrom smalldatetime = NULL,
	@PostDateTo smalldatetime = NULL,
	@EntCode varchar(3) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @Period int
	DECLARE @Company uniqueidentifier
	DECLARE @OrgList varchar(8000)
	DECLARE @OrgGroupList varchar(8000)
	DECLARE @AccHeaderBranchList varchar(8000)
	DECLARE @AccHeaderDepartmentList varchar(8000)
	DECLARE @AccLineBranchList varchar(8000)
	DECLARE @AccLineDepartmentList varchar(8000)
	DECLARE @SalesRep uniqueidentifier
	DECLARE @AccountsRelationShip varchar(3)
	DECLARE @ConsolidatedCategory varchar(3)
	DECLARE @SalesRepRoll varchar(8000)
	DECLARE @SummaryOnly char(1)
	DECLARE @LedgerType char(2)
	DECLARE @SettlementGroupList varchar(8000)
	DECLARE @CreditRating char(3)
	DECLARE @TransactionTypeList varchar(8000)
	DECLARE @ChargeGroupList varchar(8000)
	DECLARE @ChargeCodeList varchar(8000)
	DECLARE @IncludeOrgCountryList varchar(8000) = ''
	DECLARE @ExcludeOrgCountryList varchar(8000) = ''
	DECLARE @OrderBy char(1)
	
	if @PostDateFrom is null
	BEGIN
		set @PostDateFrom = CONVERT (date, SYSDATETIME());
		set @PostDateFrom = DATEADD(DAY, 1 - DAY(@PostDateFrom), @PostDateFrom)
	END
	
	if @PostDateTo is null
	BEGIN
		set @PostDateTo = DATEADD(DAY, -1, DATEADD(MONTH, 1, @PostDateFrom))
	END

	SET @Period = ''
	SET @OrgList = ''
	SET @OrgGroupList = ''
	SET @AccHeaderBranchList = ''
	SET @AccHeaderDepartmentList = ''
	SET @AccLineBranchList = ''
	SET @AccLineDepartmentList = ''
	SET @SalesRep = NULL
	SET @AccountsRelationShip = ''
	SET @ConsolidatedCategory = ''
	SET @SalesRepRoll = ''
	SET @SummaryOnly = 'Y'
	SET @LedgerType = 'AR'
	SET @SettlementGroupList = ''
	SET @CreditRating = ''
	SET @TransactionTypeList = 'INV, CRD, ADJ'
	SET @ChargeGroupList = ''
	SET @ChargeCodeList = ''
	SET @OrderBy = ''

	declare @Summary TABLE
	(
		AccountPK UNIQUEIDENTIFIER,
		AccountCode varchar(24) COLLATE database_default,
		AccountName varchar(100) COLLATE database_default,	
		ChargeCode varchar(10) COLLATE database_default,
		ChargeDesc varchar(80) COLLATE database_default,
		BRK money,
		BON money,
		CDS money,
		FRT money,
		INS money,
		ORG money,
		DST money,
		LOD money,
		UNL money,
		CLL money,
		CSH money,
		TRN money,
		WIN money,
		WOU money,
		WST money,
		NJR money,
		NGC money,
		CST money,
		OBR money,
		OBO money,
		SDS money,
		TotalIncTax money,
		TaxAmount money,
		TotalExTax money,
		SalesRep char(3) COLLATE database_default,
		CreditController char(3) COLLATE database_default,
		CustomerService char(3) COLLATE database_default,
		AccountManager char(3) COLLATE database_default, 
		SalesRepName nvarchar(256) COLLATE database_default,
		CreditControllerName nvarchar(256) COLLATE database_default,
		CustomerServiceName nvarchar(256) COLLATE database_default,
		AccountManagerName nvarchar(256) COLLATE database_default
	)

	declare @Final TABLE
	(
		AccountPK UNIQUEIDENTIFIER,
		AccountCode varchar(24) COLLATE database_default,
		AccountName varchar(100) COLLATE database_default,	
		ChargeCode varchar(10) COLLATE database_default,
		ChargeDesc varchar(80) COLLATE database_default,
		BRK money,
		BON money,
		CDS money,
		FRT money,
		INS money,
		ORG money,
		DST money,
		LOD money,
		UNL money,
		CLL money,
		CSH money,
		TRN money,
		WIN money,
		WOU money,
		WST money,
		NJR money,
		NGC money,
		CST money,
		OBR money,
		OBO money,
		SDS money,
		TotalIncTax money,
		TaxAmount money,
		TotalExTax money,
		SalesRep char(3) COLLATE database_default,
		CreditController char(3) COLLATE database_default,
		CustomerService char(3) COLLATE database_default,
		AccountManager char(3) COLLATE database_default, 
		SalesRepName nvarchar(256) COLLATE database_default,
		CreditControllerName nvarchar(256) COLLATE database_default,
		CustomerServiceName nvarchar(256) COLLATE database_default,
		AccountManagerName nvarchar(256) COLLATE database_default,
		CompanyPK uniqueidentifier,
		PostDateFrom smalldatetime,
		PostDateTo smalldatetime
	)


	DECLARE CompanyCursor CURSOR FOR
		SELECT GC_PK FROM dbo.GlbCompany;
	OPEN CompanyCursor;
	FETCH NEXT FROM CompanyCursor INTO @Company;

	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		declare @EndOfMonth smalldatetime
		declare @StartOfMonth smalldatetime
		set @StartOfMonth = @PostDateFrom
		
		while (@StartOfMonth < @PostDateTo)
		BEGIN
			set @EndOfMonth = DATEADD(DAY, -1, DATEADD(MONTH, 1, @StartOfMonth))
			
			insert @Summary
			EXECUTE dbo.ARAPTransactionLinesSP
			   @Period
			  ,@Company
			  ,@OrgList
			  ,@OrgGroupList
			  ,@AccHeaderBranchList
			  ,@AccHeaderDepartmentList
			  ,@AccLineBranchList
			  ,@AccLineDepartmentList
			  ,@SalesRep
			  ,@AccountsRelationShip
			  ,@ConsolidatedCategory
			  ,@SalesRepRoll
			  ,@SummaryOnly
			  ,@LedgerType
			  ,@SettlementGroupList
			  ,@CreditRating
			  ,@TransactionTypeList
			  ,@StartOfMonth
			  ,@EndOfMonth
			  ,@ChargeGroupList
			  ,@ChargeCodeList
			  ,@IncludeOrgCountryList
			  ,@ExcludeOrgCountryList
			  ,@OrderBy
			  
			insert @Final
			select *, @Company, @StartOfMonth, @EndOfMonth from @Summary
			delete from @Summary
			
			set @StartOfMonth = DATEADD(MONTH, 1, @StartOfMonth)
		END
		
		FETCH NEXT FROM CompanyCursor INTO @Company;
	END

	CLOSE CompanyCursor;
	DEALLOCATE CompanyCursor;

	select 
		PostDateFrom,
		PostDateTo,
		GC_Code as CompanyCode, 
		GC_Name as CompanyName, 
		GC_RX_NKLocalCurrency as Currency,
		ISNULL(LE_EnterpriseCode, '') as EnterpriseCode,
		AccountCode,
		AccountName,
		substring(OH_RL_NKClosestPort, 1, 2) as CountryCode,
		ISNULL(OM_CMClientSize, '') as Segment,
		ChargeCode,
		ChargeDesc,
		BRK ,
		BON ,
		CDS ,
		FRT ,
		INS ,
		ORG ,
		DST ,
		LOD ,
		UNL ,
		CLL ,
		CSH ,
		TRN ,
		WIN ,
		WOU ,
		WST ,
		NJR ,
		NGC ,
		CST ,
		OBR ,
		OBO ,
		SDS ,
		TotalIncTax ,
		TaxAmount ,
		TotalExTax,
		SalesRep,
		CreditController,
		CustomerService,
		AccountManager, 
		SalesRepName,
		CreditControllerName,
		CustomerServiceName,
		AccountManagerName
	from @Final as Final
	join dbo.GlbCompany on GC_PK = CompanyPK
	join dbo.OrgHeader on OH_Code = AccountCode
	left join dbo.LicenceCompany on LC_OH = OH_PK
	left join dbo.LicenceEnterprise on LC_LE = LE_PK
	left join dbo.OrgMiscServ on OM_OH = OH_PK
	where @EntCode is null or @EntCode = LE_EnterpriseCode
	order by PostDateFrom, AccountCode
	
END
", "DROP PROCEDURE ClientEDIArap", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region ClientEDIArapDetail

			new DatabaseViewAndRoutineCreateScript("ClientEDIArapDetail",
@"CREATE PROCEDURE [dbo].[ClientEDIArapDetail]
	@PostDateFrom smalldatetime = NULL,
	@PostDateTo smalldatetime = NULL,
	@EntCode varchar(3) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @Period int
	DECLARE @Company uniqueidentifier
	DECLARE @OrgList varchar(8000)
	DECLARE @OrgGroupList varchar(8000)
	DECLARE @AccHeaderBranchList varchar(8000)
	DECLARE @AccHeaderDepartmentList varchar(8000)
	DECLARE @AccLineBranchList varchar(8000)
	DECLARE @AccLineDepartmentList varchar(8000)
	DECLARE @SalesRep uniqueidentifier
	DECLARE @AccountsRelationShip varchar(3)
	DECLARE @ConsolidatedCategory varchar(3)
	DECLARE @SalesRepRoll varchar(8000)
	DECLARE @SummaryOnly char(1)
	DECLARE @LedgerType char(2)
	DECLARE @SettlementGroupList varchar(8000)
	DECLARE @CreditRating char(3)
	DECLARE @TransactionTypeList varchar(8000)
	DECLARE @ChargeGroupList varchar(8000)
	DECLARE @ChargeCodeList varchar(8000)
	DECLARE @IncludeOrgCountryList varchar(8000)
	DECLARE @ExcludeOrgCountryList varchar(8000)
	DECLARE @OrderBy char(1)
	
	if @PostDateFrom is null
	BEGIN
		set @PostDateFrom = CONVERT (date, SYSDATETIME());
		set @PostDateFrom = DATEADD(DAY, 1 - DAY(@PostDateFrom), @PostDateFrom)
	END
	
	if @PostDateTo is null
	BEGIN
		set @PostDateTo = DATEADD(DAY, -1, DATEADD(MONTH, 1, @PostDateFrom))
	END

	SET @Period = ''
	SET @OrgList = ''
	SET @OrgGroupList = ''
	SET @AccHeaderBranchList = ''
	SET @AccHeaderDepartmentList = ''
	SET @AccLineBranchList = ''
	SET @AccLineDepartmentList = ''
	SET @SalesRep = NULL
	SET @AccountsRelationShip = ''
	SET @ConsolidatedCategory = ''
	SET @SalesRepRoll = ''
	SET @SummaryOnly = 'N'
	SET @LedgerType = 'AR'
	SET @SettlementGroupList = ''
	SET @CreditRating = ''
	SET @TransactionTypeList = 'INV, CRD, ADJ'
	SET @ChargeGroupList = ''
	SET @ChargeCodeList = ''
	SET @OrderBy = ''

	declare @TRANSACTIONS TABLE -- @TRANSACTIONS from dbo.ARAPTransactionLinesSP
	(
		AccountPK UNIQUEIDENTIFIER,
		AccountCode varchar(24) COLLATE database_default,
		AccountName varchar(100) COLLATE database_default,	
		BranchCode CHAR(3) COLLATE database_default,
		CountryCode CHAR(2) COLLATE database_default,
		CountryName nvarchar(80) COLLATE database_default,
		TransactionLineBranchCode CHAR(3) COLLATE database_default,
		TransactionType Char(3) COLLATE database_default,
		InvoiceRef varchar(38) COLLATE database_default,
		Description nvarchar(160) COLLATE database_default,
		InvoiceRef2 nvarchar(160) COLLATE database_default,
		ContactInfo varchar(282) COLLATE database_default,
		ContactName nvarchar(256) COLLATE database_default,
		ContactPhoneNo varchar(20) COLLATE database_default,
		OC_OH_AddressOverride UNIQUEIDENTIFIER,
		InvoiceTotal money DEFAULT 0,
		DueDate smalldatetime,
		InvoiceDate smalldatetime,
		CurrencyCode char(3) COLLATE database_default,
		LineCurrencyCode char(3) COLLATE database_default,
		SalesRep char(3) COLLATE database_default,
		CreditController char(3) COLLATE database_default,
		CustomerService char(3) COLLATE database_default,
		CreditLimit money DEFAULT 0,
		AccountGroup nvarchar(6) COLLATE database_default,
		OrgBranchCode char(3) COLLATE database_default,
		IsOverLimit char(1) COLLATE database_default,
		ExchangeRate float,
		LineExchangeRate float,
		IsDSBInvoice char(3) COLLATE database_default,
		DSBCharge money,
		OSTotal money,
		ARCategory char(3) COLLATE database_default,
		ConsolidationCategory char(3) COLLATE database_default,
		SettlementCode varchar(24) COLLATE database_default,
		AccountCode2 varchar(24) COLLATE database_default,
		OrgBranchName varchar(100) COLLATE database_default,
		TranBranchName varchar(100) COLLATE database_default,
		SalesRepName nvarchar(256) COLLATE database_default,
		CreditControllerName nvarchar(256) COLLATE database_default,
		CustomerServiceName nvarchar(256) COLLATE database_default,
		SettlementName varchar(100) COLLATE database_default,	
		RXSubUnitRatio int,
		PostDate smalldatetime,
		DepartmentCode char(3) COLLATE database_default,
		PostingHeaderDepartmentCode char(3) COLLATE database_default,
		FullyPaidDate smalldatetime,
		InvoiceTerm char(15) COLLATE database_default,
		InvoiceExTax money,
		TaxAmount money,
		JobNumber varchar(35) COLLATE database_default,
		TransactionTypeDesc varchar(20) COLLATE database_default,
		ChargeCode varchar(10) COLLATE database_default,
		ChargeDesc varchar(80) COLLATE database_default,
		ChargeCodeORGLAccount varchar(10) COLLATE database_default,
		ChargeGroup char(3) COLLATE database_default,
		LineDescription nvarchar(2048) COLLATE database_default,
		LineOSTotal money,
		LineExTaxAmount money,
		LineTaxAmount money,
		LineIncTaxAmount money,
		OrderReferences VARCHAR(1000),
		OrderContacts VARCHAR(1000),
		OrderHeaderAttrib VARCHAR(1000),
		TaxBranch char(3) COLLATE database_default,
		SupplyType varchar(3) COLLATE database_default 
	)

	declare @Final TABLE
	(
		-- same columns as transactions, plus 3 at the end
		AccountPK UNIQUEIDENTIFIER,
		AccountCode varchar(24) COLLATE database_default,
		AccountName varchar(100) COLLATE database_default,	
		BranchCode CHAR(3) COLLATE database_default,
		CountryCode CHAR(2) COLLATE database_default,
		CountryName nvarchar(80) COLLATE database_default,
		TransactionLineBranchCode CHAR(3) COLLATE database_default,
		TransactionType Char(3) COLLATE database_default,
		InvoiceRef varchar(38) COLLATE database_default,
		Description nvarchar(160) COLLATE database_default,
		InvoiceRef2 nvarchar(160) COLLATE database_default,
		ContactInfo varchar(282) COLLATE database_default,
		ContactName nvarchar(256) COLLATE database_default,
		ContactPhoneNo varchar(20) COLLATE database_default,
		OC_OH_AddressOverride UNIQUEIDENTIFIER,
		InvoiceTotal money DEFAULT 0,
		DueDate smalldatetime,
		InvoiceDate smalldatetime,
		CurrencyCode char(3) COLLATE database_default,
		LineCurrencyCode char(3) COLLATE database_default,
		SalesRep char(3) COLLATE database_default,
		CreditController char(3) COLLATE database_default,
		CustomerService char(3) COLLATE database_default,
		CreditLimit money DEFAULT 0,
		AccountGroup nvarchar(6) COLLATE database_default,
		OrgBranchCode char(3) COLLATE database_default,
		IsOverLimit char(1) COLLATE database_default,
		ExchangeRate float,
		LineExchangeRate float,
		IsDSBInvoice char(3) COLLATE database_default,
		DSBCharge money,
		OSTotal money,
		ARCategory char(3) COLLATE database_default,
		ConsolidationCategory char(3) COLLATE database_default,
		SettlementCode varchar(24) COLLATE database_default,
		AccountCode2 varchar(24) COLLATE database_default,
		OrgBranchName varchar(100) COLLATE database_default,
		TranBranchName varchar(100) COLLATE database_default,
		SalesRepName nvarchar(256) COLLATE database_default,
		CreditControllerName nvarchar(256) COLLATE database_default,
		CustomerServiceName nvarchar(256) COLLATE database_default,
		SettlementName varchar(100) COLLATE database_default,	
		RXSubUnitRatio int,
		PostDate smalldatetime,
		DepartmentCode char(3) COLLATE database_default,
		PostingHeaderDepartmentCode char(3) COLLATE database_default,
		FullyPaidDate smalldatetime,
		InvoiceTerm char(15) COLLATE database_default,
		InvoiceExTax money,
		TaxAmount money,
		JobNumber varchar(35) COLLATE database_default,
		TransactionTypeDesc varchar(20) COLLATE database_default,
		ChargeCode varchar(10) COLLATE database_default,
		ChargeDesc varchar(80) COLLATE database_default,
		ChargeCodeORGLAccount varchar(10) COLLATE database_default,
		ChargeGroup char(3) COLLATE database_default,
		LineDescription nvarchar(2048) COLLATE database_default,
		LineOSTotal money,
		LineExTaxAmount money,
		LineTaxAmount money,
		LineIncTaxAmount money,
		OrderReferences VARCHAR(1000),
		OrderContacts VARCHAR(1000),
		OrderHeaderAttrib VARCHAR(1000),
		-- end of transactions columns
		CompanyPK uniqueidentifier,
		PostDateFrom smalldatetime,
		PostDateTo smalldatetime
	)


	DECLARE CompanyCursor CURSOR FOR
		SELECT GC_PK FROM dbo.GlbCompany;
	OPEN CompanyCursor;
	FETCH NEXT FROM CompanyCursor INTO @Company;

	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		declare @EndOfMonth smalldatetime
		declare @StartOfMonth smalldatetime
		set @StartOfMonth = @PostDateFrom
		
		while (@StartOfMonth < @PostDateTo)
		BEGIN
			set @EndOfMonth = DATEADD(DAY, -1, DATEADD(MONTH, 1, @StartOfMonth))
			
			insert @TRANSACTIONS
				EXECUTE dbo.ARAPTransactionLinesSP
				   @Period
				  ,@Company
				  ,@OrgList
				  ,@OrgGroupList
				  ,@AccHeaderBranchList
				  ,@AccHeaderDepartmentList
				  ,@AccLineBranchList
				  ,@AccLineDepartmentList
				  ,@SalesRep
				  ,@AccountsRelationShip
				  ,@ConsolidatedCategory
				  ,@SalesRepRoll
				  ,@SummaryOnly
				  ,@LedgerType
				  ,@SettlementGroupList
				  ,@CreditRating
				  ,@TransactionTypeList
				  ,@StartOfMonth
				  ,@EndOfMonth
				  ,@ChargeGroupList
				  ,@ChargeCodeList
				  ,@IncludeOrgCountryList
				  ,@ExcludeOrgCountryList
				  ,@OrderBy
			  
			insert @Final
			select
				AccountPK,
				AccountCode,
				AccountName,
				BranchCode,
				CountryCode,
				CountryName,
				TransactionLineBranchCode,
				TransactionType,
				InvoiceRef,
				Description,
				InvoiceRef2,
				ContactInfo,
				ContactName,
				ContactPhoneNo,
				OC_OH_AddressOverride,
				InvoiceTotal,
				DueDate,
				InvoiceDate,
				CurrencyCode,
				LineCurrencyCode,
				SalesRep,
				CreditController,
				CustomerService,
				CreditLimit,
				AccountGroup,
				OrgBranchCode,
				IsOverLimit,
				ExchangeRate,
				LineExchangeRate,
				IsDSBInvoice,
				DSBCharge,
				OSTotal,
				ARCategory,
				ConsolidationCategory,
				SettlementCode,
				AccountCode2,
				OrgBranchName,
				TranBranchName,
				SalesRepName,
				CreditControllerName,
				CustomerServiceName,
				SettlementName,
				RXSubUnitRatio,
				PostDate,
				DepartmentCode,
				PostingHeaderDepartmentCode,
				FullyPaidDate,
				InvoiceTerm,
				InvoiceExTax,
				TaxAmount,
				JobNumber,
				TransactionTypeDesc,
				ChargeCode,
				ChargeDesc,
				ChargeCodeORGLAccount,
				ChargeGroup,
				LineDescription,
				LineOSTotal,
				LineExTaxAmount,
				LineTaxAmount,
				LineIncTaxAmount,
				OrderReferences,
				OrderContacts,
				OrderHeaderAttrib,
				@Company,
				@StartOfMonth,
				@EndOfMonth
			from @TRANSACTIONS
			delete from @TRANSACTIONS
			
			set @StartOfMonth = DATEADD(MONTH, 1, @StartOfMonth)
		END
		
		FETCH NEXT FROM CompanyCursor INTO @Company;
	END

	CLOSE CompanyCursor;
	DEALLOCATE CompanyCursor;

	select 
		PostDateFrom,
		PostDateTo,
		GC_Code as CompanyCode, 
		GC_Name as CompanyName, 
		GC_RX_NKLocalCurrency as Currency,
		ISNULL(LE_EnterpriseCode, '') as EnterpriseCode,
		AccountCode,
		AccountName,
		substring(OH_RL_NKClosestPort, 1, 2) as CountryCode,
		ISNULL(OM_CMClientSize, '') as Segment,

		-- @final columns here
		BranchCode,
		TransactionLineBranchCode,
		TransactionType,
		InvoiceRef,
		[Description],
		InvoiceRef2,
		ContactInfo,
		ContactName,
		ContactPhoneNo,
		OC_OH_AddressOverride,
		InvoiceTotal,
		DueDate,
		InvoiceDate,
		CurrencyCode,
		LineCurrencyCode,
		SalesRep,
		CreditController,
		CustomerService,
		CreditLimit,
		AccountGroup,
		OrgBranchCode,
		IsOverLimit,
		ExchangeRate,
		LineExchangeRate,
		IsDSBInvoice,
		DSBCharge,
		OSTotal,
		ARCategory,
		ConsolidationCategory,
		SettlementCode,
		AccountCode2,
		OrgBranchName,
		TranBranchName,
		SalesRepName,
		CreditControllerName,
		CustomerServiceName,
		SettlementName,
		RXSubUnitRatio,
		PostDate,
		DepartmentCode,
		PostingHeaderDepartmentCode,
		FullyPaidDate,
		InvoiceTerm,
		InvoiceExTax,
		TaxAmount,
		JobNumber,
		TransactionTypeDesc,
		ChargeCode,
		ChargeDesc,
		ChargeCodeORGLAccount,
		ChargeGroup,
		LineDescription,
		LineOSTotal,
		LineExTaxAmount,
		LineTaxAmount,
		LineIncTaxAmount,
		OrderReferences,
		OrderContacts,
		OrderHeaderAttrib
	from @Final as Final
	join dbo.GlbCompany on GC_PK = CompanyPK
	join dbo.OrgHeader on OH_Code = AccountCode
	left join dbo.LicenceCompany on LC_OH = OH_PK
	left join dbo.LicenceEnterprise on LC_LE = LE_PK
	left join dbo.OrgMiscServ on OM_OH = OH_PK
	where @EntCode is null or @EntCode = LE_EnterpriseCode
	order by PostDateFrom, AccountCode
	
END
", "DROP PROCEDURE ClientEDIArapDetail", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region EdiAddContactToPersonMergeQueue

			new DatabaseViewAndRoutineCreateScript("EdiAddContactToPersonMergeQueue",
@"create procedure EdiAddContactToPersonMergeQueue
	@DatabasePk uniqueidentifier
as

declare @ContactForMerge table
(
	OC_PK uniqueidentifier,
	OC_PER uniqueidentifier,
	OC_OH uniqueidentifier,
	OC_Email nvarchar(254),
	ContactNameNoSuffix nvarchar(512),
	ContactRanking int,
	DatabasePk uniqueidentifier,
	DatabaseRanking int
)

insert into @ContactForMerge
select 
	OC_PK,
	OC_PER,
	OC_OH,
	OC_Email,
	ContactNameNoSuffix =
		case
			when PATINDEX('% (%)', OC_ContactName) > 0 then RTRIM(SUBSTRING(OC_ContactName, 1, PATINDEX('% (%)', OC_ContactName) - 1))
			else OC_ContactName
		end,
	ContactRanking = ROW_NUMBER() over (partition by OC_OH, OC_Email order by OC_IsActive desc, OC_WebAccessEnabled desc, OC_ContactName),
	DatabasePk = LD_PK,
	DatabaseRanking
from 
	dbo.OrgContact
	left join
	(
		select 
			LD_PK, LD_OH_WebAccessOrg, DatabaseRanking = 1
		from 
			dbo.LicenceDatabase
		where 
			LD_PK = @DatabasePk
			and LD_OH_WebAccessOrg is not null

	) d	 
	on OC_OH = LD_OH_WebAccessOrg
where 
	OC_Email != ''
	and
	OC_OH in
	(
		select LC_OH
		from 
			dbo.LicenceCompany
			join dbo.LicenceDatabase on LC_LE = LD_LE
		where
			LD_PK = @DatabasePk
	)

insert into dbo.EdiPersonMergeQueue 
	(EMQ_PK, EMQ_PER_RetainPerson, EMQ_PER_DissolvePerson)
select 
	NEWID(), RetainPerson, DissolvePerson
from
	(
		select distinct
			RetainPerson = tc.OC_PER,
			DissolvePerson = sc.OC_PER
		from
			@ContactForMerge tc
			join dbo.LicenceDatabase on LD_PK = DatabasePK
			join dbo.LicenceCompany on LC_LE = LD_LE
			join @ContactForMerge sc on 
				sc.OC_OH = LC_OH 
				and tc.OC_PK != sc.OC_PK 
				and tc.OC_Email = sc.OC_Email 
				and tc.ContactNameNoSuffix = sc.ContactNameNoSuffix 
				and tc.OC_PER != sc.OC_PER
			left join dbo.EdiPersonMergeQueue on sc.OC_PER = EMQ_PER_DissolvePerson and tc.OC_PER = EMQ_PER_RetainPerson
		where 
			tc.ContactRanking = 1
			and tc.DatabaseRanking = 1
			and EMQ_PK is null
	) a
", "drop procedure EdiAddContactToPersonMergeQueue", DbRoutineType.SqlProcedureTypeDesc),

		#endregion

		#region EdiUserAgreement

			new DatabaseViewAndRoutineCreateScript("TG_DEL_EdiUserAgreement", @"
CREATE TRIGGER TG_DEL_EdiUserAgreement
	ON dbo.EdiUserAgreement
	AFTER DELETE
AS
BEGIN
	IF (@@rowcount = 0) RETURN;
	SET NOCOUNT ON

	IF (EXISTS(SELECT NULL FROM DELETED WHERE ERA_EffectiveTimeUtc <= GETUTCDATE()))
	BEGIN
		RAISERROR('Delete operation NOT allowed on EdiUserAgreement for records which are already effective.', 16, 1)
		ROLLBACK TRANSACTION
		RETURN
	END
END
",
					"DROP TRIGGER TG_DEL_EdiUserAgreement",
				DbRoutineType.SqlTriggerTypeDesc),

			new DatabaseViewAndRoutineCreateScript("TG_UPD_EdiUserAgreement", @"
CREATE TRIGGER TG_UPD_EdiUserAgreement
	ON dbo.EdiUserAgreement
	FOR UPDATE
AS
BEGIN 
	IF EXISTS (SELECT NULL FROM DELETED WHERE ERA_EffectiveTimeUtc <= GETUTCDATE())
	BEGIN
		IF EXISTS
		(
			SELECT NULL
			FROM INSERTED i
			JOIN DELETED d ON i.ERA_PK = d.ERA_PK
			WHERE
				i.ERA_Title != d.ERA_Title
				OR i.ERA_Content != d.ERA_Content
				OR i.ERA_EffectiveTimeUtc != d.ERA_EffectiveTimeUtc
				OR i.ERA_VersionNumber != d.ERA_VersionNumber
				OR i.ERA_RN_NKCountryCode != d.ERA_RN_NKCountryCode
				OR i.ERA_Type != d.ERA_Type
		)
		BEGIN
			RAISERROR('Update operation NOT allowed on EdiUserAgreement for this/these column(s) if already effective.', 16, 1)
			ROLLBACK TRANSACTION
			RETURN
		END
	END

	IF EXISTS
	(
		SELECT NULL
		FROM INSERTED i
		JOIN DELETED d ON i.ERA_PK = d.ERA_PK
		WHERE
			i.ERA_EffectiveTimeUtc != d.ERA_EffectiveTimeUtc
			AND i.ERA_EffectiveTimeUtc <= GETUTCDATE()
	)
	BEGIN
		RAISERROR('Update operation NOT allowed to set Effective Time of EdiUserAgreement to past time.', 16, 1)
		ROLLBACK TRANSACTION
		RETURN
	END
END
",
				"DROP TRIGGER TG_UPD_EdiUserAgreement",
				DbRoutineType.SqlTriggerTypeDesc),

			new DatabaseViewAndRoutineCreateScript("TG_INS_EdiUserAgreement", @"
CREATE TRIGGER TG_INS_EdiUserAgreement
	ON dbo.EdiUserAgreement
	FOR INSERT
AS
BEGIN
	IF EXISTS (SELECT NULL FROM INSERTED i WHERE i.ERA_EffectiveTimeUtc <= GETUTCDATE())
	BEGIN
		RAISERROR('Insert operation NOT allowed to set Effective Time of EdiUserAgreement to past time.', 16, 1)
		ROLLBACK TRANSACTION
		RETURN
	END
END
",
				"DROP TRIGGER TG_INS_EdiUserAgreement",
				DbRoutineType.SqlTriggerTypeDesc),

		#endregion

		#region EdiAccessToken

			TrustedMessagingSchema.CreateTokenScript(),
			TrustedMessagingSchema.ConsumeTokenScript(),
			TrustedMessagingSchema.UpdateSecretKeyScript(),

		#endregion

		#region EdiLoadFeatureControlRule

			new DatabaseViewAndRoutineCreateScript("EdiLoadFeatureControlRule",
@"CREATE PROCEDURE [dbo].[EdiLoadFeatureControlRule]
(
	@ClientRuleTimestampUtc DATETIME,
	@ClientDatabasePK UNIQUEIDENTIFIER
)
AS
BEGIN
	DECLARE @RuleTimestampUtc DATETIME;

	-- timestamp
	-- the user may delete FeatureControlRule / FeatureControlRuleLicenceDatabasePivot
	-- deletion will update rule / header's timestamp.
	SELECT TOP 1 @RuleTimestampUtc = RuleTimestampUtc
	FROM
	(
		SELECT RuleTimestampUtc = FCM_SystemLastEditTimeUtc FROM dbo.FeatureControlHeader
		UNION
		SELECT RuleTimestampUtc = FCR_SystemLastEditTimeUtc FROM dbo.FeatureControlRule
		UNION
		SELECT RuleTimestampUtc = FCD_SystemLastEditTimeUtc FROM dbo.FeatureControlRuleLicenceDatabasePivot
		UNION
		SELECT RuleTimestampUtc = FCS_SystemLastEditTimeUtc FROM dbo.FeatureControlSet
	) T ORDER BY RuleTimestampUtc DESC;

	SELECT RuleTimestampUtc = @RuleTimestampUtc;

	IF @RuleTimestampUtc IS NULL OR @RuleTimestampUtc = @ClientRuleTimestampUtc
	BEGIN
		RETURN
	END

	SELECT FCM_FeatureControlCode, FCR_RuleType, 
		   FCR_StartDateUtc, FCR_EndDateUtc, FCR_Parameters
	FROM
	(
		-- Global Rules
		SELECT FCM_FeatureControlCode, FCR_RuleType, FCR_StartDateUtc, FCR_EndDateUtc, FCR_Parameters
		FROM dbo.FeatureControlRule
		JOIN dbo.FeatureControlHeader ON FCR_FCM_FeatureControl = FCM_PK
		WHERE FCR_IsActive = 1 AND FCR_RuleType = 'GLB'

		UNION

		-- Client Rules
		SELECT FCM_FeatureControlCode, FCR_RuleType, FCR_StartDateUtc, FCR_EndDateUtc, 
			   FCR_Parameters = IIF(FCR_UseGlobalParameters = 0, CLI.FCR_Parameters, GlobalRule.FCR_Parameters)
		FROM dbo.FeatureControlRule CLI
		JOIN dbo.FeatureControlHeader ON FCR_FCM_FeatureControl = FCM_PK
		JOIN dbo.FeatureControlRuleLicenceDatabasePivot ON FCD_FCR_FeatureControlRule = FCR_PK
		OUTER APPLY
		(
			SELECT TOP 1 FCR_Parameters FROM dbo.FeatureControlRule GLB 
			WHERE GLB.FCR_FCM_FeatureControl = CLI.FCR_FCM_FeatureControl AND GLB.FCR_RuleType = 'GLB'
		) GlobalRule
		WHERE FCR_IsActive = 1 AND FCR_RuleType = 'CLI' AND FCD_LD_LicenceDatabase = @ClientDatabasePK
		AND
		(
			FCR_UseGlobalParameters = 0 OR GlobalRule.FCR_Parameters IS NOT NULL
		)

		UNION

		-- Feature Set Rules
		-- Feature set rules are returned as global rules type
		SELECT FCM_FeatureControlCode, 'GLB', FCR_StartDateUtc, FCR_EndDateUtc, 
			   FCR_Parameters = IIF(FCR_UseGlobalParameters = 0, FCS.FCR_Parameters, GlobalRule.FCR_Parameters)
		FROM dbo.FeatureControlRule FCS
		JOIN dbo.FeatureControlHeader ON FCR_FCM_FeatureControl = FCM_PK
		JOIN dbo.FeatureControlSet ON FCR_FCS_FeatureSet = FCS_PK
		JOIN dbo.LicenceDatabase ON LD_FCS_FeatureSet = FCS_PK
		OUTER APPLY
		(
			SELECT TOP 1 FCR_Parameters FROM dbo.FeatureControlRule GLB 
			WHERE GLB.FCR_FCM_FeatureControl = FCS.FCR_FCM_FeatureControl AND GLB.FCR_RuleType = 'GLB'
		) GlobalRule
		WHERE FCR_IsActive = 1 AND FCR_RuleType = 'FCS' AND LD_PK = @ClientDatabasePK
		AND
		(
			FCR_UseGlobalParameters = 0 OR GlobalRule.FCR_Parameters IS NOT NULL
		)
	)T 
	ORDER BY 1, 2, 3, 4;
END
", "DROP PROCEDURE EdiLoadFeatureControlRule", DbRoutineType.SqlProcedureTypeDesc)

		#endregion
		));

		#endregion
	}
}
