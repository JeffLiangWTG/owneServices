CREATE TABLE [EDICommunicationsMode](
	[EK_PK] [uniqueidentifier] NOT NULL,
	[EK_Module] [varchar](3) NOT NULL,
	[EK_CommsDirection] [varchar](3) NOT NULL,
	[EK_CommunicationsTransport] [varchar](3) NOT NULL,
	[EK_FtpLockingMethod] [varchar](3) NOT NULL,
	[EK_Destination] [varchar](260) NOT NULL,
	[EK_ServerAddressSubject] [varchar](256) NOT NULL,
	[EK_Filename] [varchar](64) NOT NULL,
	[EK_PortNumber] [int] NOT NULL,
	[EK_FileFormat] [varchar](3) NOT NULL,
	[EK_LoginName] [varchar](80) NOT NULL,
	[EK_Password] [varchar](80) NOT NULL,
	[EK_Certificate] [varbinary](max) NULL,
	[EK_ParentID] [uniqueidentifier] NOT NULL,
	[EK_ParentTableCode] [varchar](3) NOT NULL,
	[EK_MessagePurpose] [varchar](3) NOT NULL,
	[EK_OH_MessageVAN] [uniqueidentifier] NULL,
	[EK_LastFailed] [smalldatetime] NULL,
	[EK_LocalPartyVanID] [varchar](32) NOT NULL,
	[EK_RelatedPartyVanID] [varchar](32) NOT NULL,
	[EK_GG] [uniqueidentifier] NULL,
	[EK_PublishInternalMilestones] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [GlbBranch](
	[GB_PK] [uniqueidentifier] NOT NULL,
	[GB_Code] [char](3) NOT NULL
) ON [PRIMARY]

CREATE TABLE [GlbGroup](
	[GG_PK] [uniqueidentifier] NOT NULL,
	[GG_IsValid] [bit] NOT NULL,
	[GG_Code] [varchar](15) NOT NULL,
	[GG_Desc] [nvarchar](64) NOT NULL,
	[GG_IsSystemDefined] [bit] NOT NULL,
	[GG_IsSales] [bit] NOT NULL,
	[GG_IsActive] [bit] NOT NULL,
	[GG_ActiveDirectoryObjectGuid] [uniqueidentifier] NULL,
	[GG_SystemCreateTimeUtc] [datetime] NULL,
	[GG_SystemCreateUser] [varchar](3) NOT NULL,
	[GG_SystemLastEditTimeUtc] [datetime] NULL,
	[GG_SystemLastEditUser] [varchar](3) NOT NULL,
	[GG_GC] [uniqueidentifier] NULL,
	[GG_IsSecurityEnabled] [bit] NOT NULL,
	[GG_DomainName] [nvarchar](255) NOT NULL
) ON [PRIMARY]

CREATE TABLE [GlbStaff](
	[GS_PK] [uniqueidentifier] NOT NULL,
	[GS_IsValid] [bit] NOT NULL,
	[GS_Code] [varchar](3) NOT NULL,
	[GS_IsActive] [bit] NOT NULL,
	[GS_IsResource] [bit] NOT NULL,
	[GS_ResourceType] [varchar](3) NOT NULL,
	[GS_LoginName] [nvarchar](104) NOT NULL,
	[GS_Password] [varchar](128) NOT NULL,
	[GS_IsSalesRep] [bit] NOT NULL,
	[GS_IsDatabaseDeveloper] [bit] NOT NULL,
	[GS_IsController] [bit] NOT NULL,
	[GS_IsSystemAccount] [bit] NOT NULL,
	[GS_IsDeveloper] [bit] NOT NULL,
	[GS_IsBackupOperator] [bit] NOT NULL,
	[GS_IsReadOnlyDBUser] [bit] NOT NULL,
	[GS_EmploymentBasis] [varchar](3) NOT NULL,
	[GS_NameTitle] [nvarchar](10) NOT NULL,
	[GS_NameSuffix] [nvarchar](10) NOT NULL,
	[GS_UserAddress2] [nvarchar](50) NOT NULL,
	[GS_Gender] [varchar](1) NOT NULL,
	[GS_WorkPhone] [varchar](20) NOT NULL,
	[GS_PublishWorkPhone] [bit] NOT NULL,
	[GS_WorkExtension] [varchar](10) NOT NULL,
	[GS_PublishWorkExtension] [bit] NOT NULL,
	[GS_HomePhone] [varchar](20) NOT NULL,
	[GS_PublishHomePhone] [bit] NOT NULL,
	[GS_MobilePhone] [varchar](20) NOT NULL,
	[GS_PublishMobilePhone] [bit] NOT NULL,
	[GS_FaxNum] [varchar](20) NOT NULL,
	[GS_PublishFaxNum] [bit] NOT NULL,
	[GS_Pager] [varchar](20) NOT NULL,
	[GS_SecurityCardNumber] [varchar](20) NOT NULL,
	[GS_EnterpriseCertificationID] [varchar](20) NOT NULL,
	[GS_PublishEmailAddress] [bit] NOT NULL,
	[GS_Birthdate] [date] NULL,
	[GS_EftWages] [bit] NOT NULL,
	[GS_WagesBankName] [nvarchar](35) NOT NULL,
	[GS_WagesBankAccount] [varchar](35) NOT NULL,
	[GS_WagesBankBsb] [varchar](15) NOT NULL,
	[GS_WagesBankSwift] [varchar](35) NOT NULL,
	[GS_NextOfKinRelationship] [varchar](3) NOT NULL,
	[GS_NextOfKin] [nvarchar](50) NOT NULL,
	[GS_NextOfKinHomePhone] [varchar](20) NOT NULL,
	[GS_NextOfKinWorkPhone] [varchar](20) NOT NULL,
	[GS_EmergencyContactRelationship] [varchar](3) NOT NULL,
	[GS_EmergencyContactName] [nvarchar](50) NOT NULL,
	[GS_EmergencyHomePhone] [varchar](20) NOT NULL,
	[GS_EmergencyWorkPhone] [varchar](20) NOT NULL,
	[GS_EmploymentDate] [smalldatetime] NULL,
	[GS_DepartureDate] [smalldatetime] NULL,
	[GS_UserSignature] [varbinary](max) NULL,
	[GS_ProfilePhoto] [varbinary](max) NULL,
	[GS_DueBack] [smalldatetime] NULL,
	[GS_OutOnTask] [varchar](50) NOT NULL,
	[GS_PersonalEDIMailBox] [varchar](20) NOT NULL,
	[GS_BrokerID] [varchar](20) NOT NULL,
	[GS_BrokerPassword] [varchar](128) NOT NULL,
	[GS_BrokerWorkingPassword] [varchar](128) NOT NULL,
	[GS_BrokerPasswordStatus] [varchar](3) NOT NULL,
	[GS_Passport] [varchar](20) NOT NULL,
	[GS_IsInTrainingMode] [bit] NOT NULL,
	[GS_PasswordReuseScratch] [varchar](1024) NOT NULL,
	[GS_LastPasswordChangeDate] [smalldatetime] NULL,
	[GS_ChangePasswordAtNextLogin] [bit] NOT NULL,
	[GS_LastPasswordAttemptDateTime_UTC] [smalldatetime] NULL,
	[GS_LockoutDateTime_UTC] [smalldatetime] NULL,
	[GS_LockoutDateTime] [smalldatetime] NULL,
	[GS_PasswordNeverChanges] [bit] NOT NULL,
	[GS_NextReviewDate] [smalldatetime] NULL,
	[GS_GB_HomeBranch] [uniqueidentifier] NULL,
	[GS_GE_HomeDepartment] [uniqueidentifier] NULL,
	[GS_GB_LastLogonBranch] [uniqueidentifier] NULL,
	[GS_GE_LastLogonDepartment] [uniqueidentifier] NULL,
	[GS_WorkingLanguage] [varchar](7) NOT NULL,
	[GS_LastActivityDate] [smalldatetime] NULL,
	[GS_CommissionBasis] [varchar](3) NOT NULL,
	[GS_NewClientCommissionRate] [decimal](8, 1) NOT NULL,
	[GS_EstablishedClientCommissionRate] [decimal](8, 1) NOT NULL,
	[GS_CommissionMinimumEarning] [money] NOT NULL,
	[GS_IsActivityLogged] [bit] NOT NULL,
	[GS_ActiveDirectoryObjectGuid] [uniqueidentifier] NULL,
	[GS_HashedPassword] [varchar](88) NOT NULL,
	[GS_HashedPasswordIterations] [int] NOT NULL,
	[GS_SystemCreateTimeUtc] [datetime] NULL,
	[GS_SystemCreateUser] [varchar](3) NOT NULL,
	[GS_SystemLastEditTimeUtc] [datetime] NULL,
	[GS_SystemLastEditUser] [varchar](3) NOT NULL,
	[GS_EmailAddress] [nvarchar](254) NOT NULL,
	[GS_FriendlyName] [nvarchar](80) NOT NULL,
	[GS_City] [nvarchar](128) NOT NULL,
	[GS_State] [nvarchar](128) NOT NULL,
	[GS_Postcode] [nvarchar](40) NOT NULL,
	[GS_Title] [nvarchar](128) NOT NULL,
	[GS_FullName] [nvarchar](256) NOT NULL,
	[GS_UserAddress1] [nvarchar](1024) NOT NULL,
	[GS_IsOperational] [bit] NOT NULL,
	[GS_GC_PreferredPaymentCompany] [uniqueidentifier] NULL,
	[GS_IsDevice] [bit] NOT NULL,
	[GS_PER] [uniqueidentifier] NULL,
	[GS_RN_NKCountryCode] [varchar](2) NOT NULL,
	[GS_IsTwoFactorAuthenticationEnabled] [bit] NOT NULL,
	[GS_CanLogin] [bit] NOT NULL,
	[GS_AddressMap] [varchar](50) NOT NULL,
	[GS_GeoLocation] [geography] NOT NULL,
	[GS_ValidationStatus] [char](3) NOT NULL,
	[GS_DomainName] [nvarchar](255) NOT NULL,
	[GS_RN_NKNationalityCode] [varchar](2) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [IncidentApproval](
	[IA_PK] [uniqueidentifier] NOT NULL,
	[IA_ClientReference] [varchar](15) NOT NULL,
	[IA_IncidentNumber] [varchar](15) NOT NULL,
	[IA_LicenceCode] [varchar](9) NOT NULL,
	[IA_Module] [varchar](3) NOT NULL,
	[IA_Criticality] [varchar](3) NOT NULL,
	[IA_Status] [varchar](3) NOT NULL,
	[IA_IncidentSummary] [varchar](80) NOT NULL,
	[IA_IncidentDetails] [varchar](max) NOT NULL,
	[IA_ClientSpecifiedStatus] [varchar](3) NOT NULL,
	[IA_SystemCreateTimeUtc] [smalldatetime] NULL,
	[IA_SystemCreateUser] [varchar](3) NOT NULL,
	[IA_SystemLastEditTimeUtc] [smalldatetime] NULL,
	[IA_SystemLastEditUser] [varchar](3) NOT NULL,
	[IA_GS_NKReportingStaff] [varchar](3) NOT NULL,
	[IA_GS_NKApprovingStaff] [varchar](3) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [MailDBItems](
	[MI_PK] [uniqueidentifier] NOT NULL,
	[MI_Application] [varchar](3) NOT NULL,
	[MI_Status] [varchar](3) NOT NULL,
	[MI_Direction] [char](3) NOT NULL,
	[MI_ReceivedDateTime] [smalldatetime] NOT NULL,
	[MI_SendDateTime] [smalldatetime] NOT NULL,
	[MI_LastAttemptDateTime] [smalldatetime] NULL,
	[MI_ContentType] [varchar](3) NOT NULL,
	[MI_Encoding] [varchar](3) NOT NULL,
	[MI_Subject] [nvarchar](256) NOT NULL,
	[MI_Header] [text] NOT NULL,
	[MI_Body] [text] NOT NULL,
	[MI_From] [nvarchar](128) NOT NULL,
	[MI_ReplyTo] [nvarchar](128) NOT NULL,
	[MI_SystemCreateTimeUtc] [smalldatetime] NULL,
	[MI_SystemCreateUser] [varchar](3) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [StmActivityLog](
	[S7_PK] [uniqueidentifier] NOT NULL,
	[S7_EnterpriseActivity] [char](1) NOT NULL,
	[S7_FormCaption] [nvarchar](132) NOT NULL,
	[S7_KeyStrokes] [int] NOT NULL,
	[S7_MouseClicks] [int] NOT NULL,
	[S7_ControlChanges] [int] NOT NULL,
	[S7_ActiveTime] [int] NOT NULL,
	[S7_InactiveTime] [int] NOT NULL,
	[S7_OpenDateTime] [smalldatetime] NULL,
	[S7_CloseDateTime] [smalldatetime] NULL,
	[S7_GS_NKUser] [varchar](3) NOT NULL,
	[S7_ControllerID] [varchar](132) NOT NULL,
	[S7_ParentID] [uniqueidentifier] NULL,
	[S7_ParentTableCode] [char](2) NOT NULL
) ON [PRIMARY]

CREATE TABLE [StmData](
	[SD_PK] [uniqueidentifier] NOT NULL,
	[SD_Name] [varchar](300) NOT NULL,
	[SD_Owner] [uniqueidentifier] NULL,
	[SD_DepartmentGuid] [uniqueidentifier] NULL,
	[SD_Type] [char](3) NOT NULL,
	[SD_IsLogged] [char](1) NOT NULL,
	[SD_IsReplicated] [char](1) NOT NULL,
	[SD_BinaryValue] [varbinary](max) NULL,
	[SD_GuidValue] [uniqueidentifier] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [StmNumberCache](
	[SG_Value] [int] NOT NULL,
	[SG_SN] [int] NOT NULL
) ON [PRIMARY]

CREATE TABLE [StmNums](
	[SN_Name] [varchar](35) NOT NULL,
	[SN_Value] [int] NOT NULL,
	[SN_Owner] [uniqueidentifier] NULL,
	[SN_ID] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]

CREATE TABLE [StmPrintJob](
	[SP_PK] [uniqueidentifier] NOT NULL,
	[SP_Status] [varchar](3) NOT NULL,
	[SP_RunDateTime] [datetime] NULL,
	[SP_PrintPriority] [tinyint] NOT NULL,
	[SP_Sequence] [int] NOT NULL,
	[SP_Group] [uniqueidentifier] NULL,
	[SP_JobType] [char](3) NOT NULL,
	[SP_CustomProperties] [image] NULL,
	[SP_BlobType] [varchar](3) NOT NULL,
	[SP_EmailFaxDestination] [varchar](128) NOT NULL,
	[SP_EmailAttachmentFormat] [char](3) NOT NULL,
	[SP_EmailSubjectLine] [varchar](128) NOT NULL,
	[SP_EmailSignature] [text] NOT NULL,
	[SP_EmailFromAddress] [varchar](128) NOT NULL,
	[SP_EmailAttachments] [text] NOT NULL,
	[SP_DocumentName] [varchar](40) NOT NULL,
	[SP_DocumentType] [varchar](3) NOT NULL,
	[SP_RelatedBusinessContext] [varchar](3) NOT NULL,
	[SP_ParentGuid] [uniqueidentifier] NULL,
	[SP_ParentTableName] [varchar](35) NOT NULL,
	[SP_RetryAttempts] [tinyint] NOT NULL,
	[SP_EscapeSequence] [image] NULL,
	[SP_Copies] [smallint] NOT NULL,
	[SP_GS_NKJobSubmittedBy] [varchar](3) NOT NULL,
	[SP_WatermarkText] [varchar](128) NOT NULL,
	[SP_WatermarkImage] [image] NULL,
	[SP_NoOfCoverPages] [tinyint] NOT NULL,
	[SP_SQ] [uniqueidentifier] NULL,
	[SP_SB_DeliveryGroup] [uniqueidentifier] NULL,
	[SP_GB] [uniqueidentifier] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [StmPrintJobCopyRecipient](
	[SPR_PK] [uniqueidentifier] NOT NULL,
	[SPR_SP] [uniqueidentifier] NOT NULL,
	[SPR_RecipientType] [varchar](3) NOT NULL,
	[SPR_EmailAddress] [nvarchar](254) NOT NULL
) ON [PRIMARY]

CREATE TABLE [StmScheduleTask](
	[S5_PK] [uniqueidentifier] NOT NULL,
	[S5_ScheduleDescription] [varchar](80) NOT NULL,
	[S5_TaskPeriod] [varchar](3) NOT NULL,
	[S5_WeekDaysOnly] [char](1) NOT NULL,
	[S5_TaskPeriodCount] [int] NOT NULL,
	[S5_DayNumber] [tinyint] NOT NULL,
	[S5_DayList] [varchar](7) NOT NULL,
	[S5_MonthNumber] [tinyint] NOT NULL,
	[S5_WeekDayOccurrenceNumber] [tinyint] NOT NULL,
	[S5_StartDate] [datetime] NULL,
	[S5_EndAfterCount] [int] NOT NULL,
	[S5_EndDate] [datetime] NULL,
	[S5_ScheduleActualRunCount] [int] NOT NULL,
	[S5_AccountingPeriodScheduleFrstRun] [int] NOT NULL,
	[S5_DateScheduleFirstRun] [datetime] NULL,
	[S5_ScheduleType] [varchar](3) NOT NULL,
	[S5_TypeOfDocument] [char](3) NOT NULL,
	[S5_NextScheduledPrintRunTime] [datetime] NULL,
	[S5_CurrentPrintRunTime] [datetime] NULL,
	[S5_CancelledAtDateTime] [datetime] NULL,
	[S5_IsActive] [char](1) NOT NULL,
	[S5_IsPrivate] [char](1) NOT NULL,
	[S5_ScheduleState] [image] NULL,
	[S5_GB] [uniqueidentifier] NULL,
	[S5_ParentTableCode] [char](2) NOT NULL,
	[S5_ParentID] [uniqueidentifier] NOT NULL,
	[S5_DailyStartTime] [smalldatetime] NULL,
	[S5_DailyEndTime] [smalldatetime] NULL,
	[S5_RunTimeInMinutes] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [StmServiceHost](
	[SH_PK] [uniqueidentifier] NOT NULL,
	[SH_IsActive] [char](1) NOT NULL,
	[SH_HostName] [varchar](128) NOT NULL,
	[SH_LastHeartBeat] [smalldatetime] NULL
) ON [PRIMARY]

CREATE TABLE [StmTranslationFeedback](
	[XT_PK] [uniqueidentifier] NOT NULL,
	[XT_EnterpriseCode] [char](3) NOT NULL,
	[XT_DatabaseCode] [char](3) NOT NULL,
	[XT_CompanyCode] [char](3) NOT NULL,
	[XT_ClientStaffInitial] [char](3) NOT NULL,
	[XT_Language] [char](3) NOT NULL,
	[XT_RN_NKDialect] [char](3) NOT NULL,
	[XT_Source] [nvarchar](max) NOT NULL,
	[XT_OriginalTranslation] [nvarchar](max) NOT NULL,
	[XT_SuggestedTranslation] [nvarchar](max) NOT NULL,
	[XT_Comments] [nvarchar](max) NOT NULL,
	[XT_Screenshot] [varbinary](max) NULL,
	[XT_Status] [char](3) NOT NULL,
	[XT_StatusTime] [smalldatetime] NULL,
	[XT_GS_NKReviewer] [char](3) NOT NULL,
	[XT_ReviewComment] [nvarchar](max) NOT NULL,
	[XT_SystemCreateTimeUtc] [smalldatetime] NOT NULL,
	[XT_SystemCreateUser] [char](3) NOT NULL,
	[XT_SystemLastEditTimeUtc] [smalldatetime] NULL,
	[XT_SystemLastEditUser] [char](3) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [StmTranslationFeedbackResource](
	[XQ_PK] [uniqueidentifier] NOT NULL,
	[XQ_ResourceStringKey] [varchar](512) NOT NULL,
	[XQ_ResourceStringLevel] [char](3) NOT NULL,
	[XQ_MatchType] [char](1) NOT NULL,
	[XQ_XT] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

CREATE TABLE [StmUpgrade](
	[SZ_PK] [uniqueidentifier] NOT NULL,
	[SZ_Type] [varchar](3) NOT NULL,
	[SZ_ExeVersionDate] [smalldatetime] NULL,
	[SZ_MajorVersion] [int] NOT NULL,
	[SZ_MinorVersion] [int] NOT NULL,
	[SZ_Release] [int] NOT NULL,
	[SZ_Patch] [int] NOT NULL,
	[SZ_MajorDataVersion] [int] NOT NULL,
	[SZ_MinorDataVersion] [int] NOT NULL,
	[SZ_WorkingVersion] [char](1) NOT NULL,
	[SZ_UpgradeNotes] [varchar](max) NOT NULL,
	[SZ_UpgradeData_Compressed] [varbinary](max) NULL,
	[SZ_SplitCount] [smallint] NOT NULL,
	[SZ_SplitLastItem] [char](1) NOT NULL,
	[SZ_Reference] [varchar](128) NOT NULL,
	[SZ_Cancelled] [char](1) NOT NULL,
	[SZ_CancelledReason] [varchar](128) NOT NULL,
	[SZ_Status] [varchar](3) NOT NULL,
	[SZ_StatusTime] [smalldatetime] NULL,
	[SZ_StatusComment] [varchar](128) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

INSERT [GlbBranch] ([GB_PK], [GB_Code]) VALUES (N'2fdba7fb-60ba-4a03-8336-0defac4f9673', N'B1 ')

INSERT [GlbGroup] ([GG_PK], [GG_IsValid], [GG_Code], [GG_Desc], [GG_IsSystemDefined], [GG_IsSales], [GG_IsActive], [GG_ActiveDirectoryObjectGuid], [GG_SystemCreateTimeUtc], [GG_SystemCreateUser], [GG_SystemLastEditTimeUtc], [GG_SystemLastEditUser], [GG_GC], [GG_IsSecurityEnabled], [GG_DomainName]) VALUES (N'07e6438e-1d90-4812-8c6e-d67e22800bde', 0, N'', N'group1', 0, 0, 1, N'09a4399e-2c8e-4cf6-a14f-3fac4a798bd8', NULL, N'', NULL, N'', NULL, 0, N'fake.domain')

INSERT [GlbGroup] ([GG_PK], [GG_IsValid], [GG_Code], [GG_Desc], [GG_IsSystemDefined], [GG_IsSales], [GG_IsActive], [GG_ActiveDirectoryObjectGuid], [GG_SystemCreateTimeUtc], [GG_SystemCreateUser], [GG_SystemLastEditTimeUtc], [GG_SystemLastEditUser], [GG_GC], [GG_IsSecurityEnabled], [GG_DomainName]) VALUES (N'6a11089b-2e92-4361-a33d-7532f9c1367f', 0, N'', N'group2', 0, 0, 1, N'aced68a0-fd8d-48b5-91b5-386e6ac93916', NULL, N'', NULL, N'', NULL, 0, N'yet.another.fake.domain')

INSERT [GlbStaff] ([GS_PK], [GS_IsValid], [GS_Code], [GS_IsActive], [GS_IsResource], [GS_ResourceType], [GS_LoginName], [GS_Password], [GS_IsSalesRep], [GS_IsDatabaseDeveloper], [GS_IsController], [GS_IsSystemAccount], [GS_IsDeveloper], [GS_IsBackupOperator], [GS_IsReadOnlyDBUser], [GS_EmploymentBasis], [GS_NameTitle], [GS_NameSuffix], [GS_UserAddress2], [GS_Gender], [GS_WorkPhone], [GS_PublishWorkPhone], [GS_WorkExtension], [GS_PublishWorkExtension], [GS_HomePhone], [GS_PublishHomePhone], [GS_MobilePhone], [GS_PublishMobilePhone], [GS_FaxNum], [GS_PublishFaxNum], [GS_Pager], [GS_SecurityCardNumber], [GS_EnterpriseCertificationID], [GS_PublishEmailAddress], [GS_Birthdate], [GS_EftWages], [GS_WagesBankName], [GS_WagesBankAccount], [GS_WagesBankBsb], [GS_WagesBankSwift], [GS_NextOfKinRelationship], [GS_NextOfKin], [GS_NextOfKinHomePhone], [GS_NextOfKinWorkPhone], [GS_EmergencyContactRelationship], [GS_EmergencyContactName], [GS_EmergencyHomePhone], [GS_EmergencyWorkPhone], [GS_EmploymentDate], [GS_DepartureDate], [GS_UserSignature], [GS_ProfilePhoto], [GS_DueBack], [GS_OutOnTask], [GS_PersonalEDIMailBox], [GS_BrokerID], [GS_BrokerPassword], [GS_BrokerWorkingPassword], [GS_BrokerPasswordStatus], [GS_Passport], [GS_IsInTrainingMode], [GS_PasswordReuseScratch], [GS_LastPasswordChangeDate], [GS_ChangePasswordAtNextLogin], [GS_LastPasswordAttemptDateTime_UTC], [GS_LockoutDateTime_UTC], [GS_LockoutDateTime], [GS_PasswordNeverChanges], [GS_NextReviewDate], [GS_GB_HomeBranch], [GS_GE_HomeDepartment], [GS_GB_LastLogonBranch], [GS_GE_LastLogonDepartment], [GS_WorkingLanguage], [GS_LastActivityDate], [GS_CommissionBasis], [GS_NewClientCommissionRate], [GS_EstablishedClientCommissionRate], [GS_CommissionMinimumEarning], [GS_IsActivityLogged], [GS_ActiveDirectoryObjectGuid], [GS_HashedPassword], [GS_HashedPasswordIterations], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser], [GS_EmailAddress], [GS_FriendlyName], [GS_City], [GS_State], [GS_Postcode], [GS_Title], [GS_FullName], [GS_UserAddress1], [GS_IsOperational], [GS_GC_PreferredPaymentCompany], [GS_IsDevice], [GS_PER], [GS_RN_NKCountryCode], [GS_IsTwoFactorAuthenticationEnabled], [GS_CanLogin], [GS_AddressMap], [GS_GeoLocation], [GS_ValidationStatus], [GS_DomainName], [GS_RN_NKNationalityCode]) VALUES (N'6ccdfea4-039b-4af8-b62a-9235f3871cc9', 0, N'ST1', 1, 0, N'', N'staff1', N'', 0, 0, 0, 0, 0, 0, 0, N'', N'', N'', N'', N'', N'', 0, N'', 0, N'', 0, N'', 0, N'', 0, N'', N'', N'', 0, NULL, 0, N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', NULL, NULL, NULL, NULL, NULL, N'', N'', N'', N'', N'', N'', N'', 1, N'', NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, N'EN', NULL, N'', CAST(0.0 AS Decimal(8, 1)), CAST(0.0 AS Decimal(8, 1)), 0.0000, 0, N'fefb39ba-a90a-49be-ba89-959b3bd42ef8', N'', 0, NULL, N'', NULL, N'', N'', N'', N'', N'', N'', N'', N'', N'', 1, NULL, 0, NULL, N'', 0, 1, N'', 0xE61000000104000000000000000001000000FFFFFFFFFFFFFFFF01, N'NYV', N'fake.domain', N'')

INSERT [GlbStaff] ([GS_PK], [GS_IsValid], [GS_Code], [GS_IsActive], [GS_IsResource], [GS_ResourceType], [GS_LoginName], [GS_Password], [GS_IsSalesRep], [GS_IsDatabaseDeveloper], [GS_IsController], [GS_IsSystemAccount], [GS_IsDeveloper], [GS_IsBackupOperator], [GS_IsReadOnlyDBUser], [GS_EmploymentBasis], [GS_NameTitle], [GS_NameSuffix], [GS_UserAddress2], [GS_Gender], [GS_WorkPhone], [GS_PublishWorkPhone], [GS_WorkExtension], [GS_PublishWorkExtension], [GS_HomePhone], [GS_PublishHomePhone], [GS_MobilePhone], [GS_PublishMobilePhone], [GS_FaxNum], [GS_PublishFaxNum], [GS_Pager], [GS_SecurityCardNumber], [GS_EnterpriseCertificationID], [GS_PublishEmailAddress], [GS_Birthdate], [GS_EftWages], [GS_WagesBankName], [GS_WagesBankAccount], [GS_WagesBankBsb], [GS_WagesBankSwift], [GS_NextOfKinRelationship], [GS_NextOfKin], [GS_NextOfKinHomePhone], [GS_NextOfKinWorkPhone], [GS_EmergencyContactRelationship], [GS_EmergencyContactName], [GS_EmergencyHomePhone], [GS_EmergencyWorkPhone], [GS_EmploymentDate], [GS_DepartureDate], [GS_UserSignature], [GS_ProfilePhoto], [GS_DueBack], [GS_OutOnTask], [GS_PersonalEDIMailBox], [GS_BrokerID], [GS_BrokerPassword], [GS_BrokerWorkingPassword], [GS_BrokerPasswordStatus], [GS_Passport], [GS_IsInTrainingMode], [GS_PasswordReuseScratch], [GS_LastPasswordChangeDate], [GS_ChangePasswordAtNextLogin], [GS_LastPasswordAttemptDateTime_UTC], [GS_LockoutDateTime_UTC], [GS_LockoutDateTime], [GS_PasswordNeverChanges], [GS_NextReviewDate], [GS_GB_HomeBranch], [GS_GE_HomeDepartment], [GS_GB_LastLogonBranch], [GS_GE_LastLogonDepartment], [GS_WorkingLanguage], [GS_LastActivityDate], [GS_CommissionBasis], [GS_NewClientCommissionRate], [GS_EstablishedClientCommissionRate], [GS_CommissionMinimumEarning], [GS_IsActivityLogged], [GS_ActiveDirectoryObjectGuid], [GS_HashedPassword], [GS_HashedPasswordIterations], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser], [GS_EmailAddress], [GS_FriendlyName], [GS_City], [GS_State], [GS_Postcode], [GS_Title], [GS_FullName], [GS_UserAddress1], [GS_IsOperational], [GS_GC_PreferredPaymentCompany], [GS_IsDevice], [GS_PER], [GS_RN_NKCountryCode], [GS_IsTwoFactorAuthenticationEnabled], [GS_CanLogin], [GS_AddressMap], [GS_GeoLocation], [GS_ValidationStatus], [GS_DomainName], [GS_RN_NKNationalityCode]) VALUES (N'2e5fbcdc-a326-413c-ab0d-164516d5e7b0', 0, N'ST2', 1, 0, N'', N'staff2', N'', 0, 0, 0, 0, 0, 0, 0, N'', N'', N'', N'', N'', N'', 0, N'', 0, N'', 0, N'', 0, N'', 0, N'', N'', N'', 0, NULL, 0, N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', NULL, NULL, NULL, NULL, NULL, N'', N'', N'', N'', N'', N'', N'', 1, N'', NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, N'EN', NULL, N'', CAST(0.0 AS Decimal(8, 1)), CAST(0.0 AS Decimal(8, 1)), 0.0000, 0, N'9d7ae007-9d72-4053-a5fc-b85800b0dccf', N'', 0, NULL, N'', NULL, N'', N'', N'', N'', N'', N'', N'', N'', N'', 1, NULL, 0, NULL, N'', 0, 1, N'', 0xE61000000104000000000000000001000000FFFFFFFFFFFFFFFF01, N'NYV', N'another.fake.domain', N'')

INSERT [MailDBItems] ([MI_PK], [MI_Application], [MI_Status], [MI_Direction], [MI_ReceivedDateTime], [MI_SendDateTime], [MI_LastAttemptDateTime], [MI_ContentType], [MI_Encoding], [MI_Subject], [MI_Header], [MI_Body], [MI_From], [MI_ReplyTo], [MI_SystemCreateTimeUtc], [MI_SystemCreateUser]) VALUES (N'be72d978-3685-4e94-b737-5d38ad1ebf43', N'STD', N'QUE', N'   ', CAST(N'2007-07-18T16:22:00' AS SmallDateTime), CAST(N'2007-07-18T16:22:00' AS SmallDateTime), NULL, N'', N'', N'', N'', N'', N'', N'', NULL, N'')

INSERT [MailDBItems] ([MI_PK], [MI_Application], [MI_Status], [MI_Direction], [MI_ReceivedDateTime], [MI_SendDateTime], [MI_LastAttemptDateTime], [MI_ContentType], [MI_Encoding], [MI_Subject], [MI_Header], [MI_Body], [MI_From], [MI_ReplyTo], [MI_SystemCreateTimeUtc], [MI_SystemCreateUser]) VALUES (N'a9bff293-a832-466e-9c22-8cb2f10a4dc3', N'STD', N'QWA', N'   ', CAST(N'2007-07-18T16:22:00' AS SmallDateTime), CAST(N'2007-07-18T16:22:00' AS SmallDateTime), NULL, N'', N'', N'', N'', N'', N'', N'', NULL, N'')

INSERT [StmActivityLog] ([S7_PK], [S7_EnterpriseActivity], [S7_FormCaption], [S7_KeyStrokes], [S7_MouseClicks], [S7_ControlChanges], [S7_ActiveTime], [S7_InactiveTime], [S7_OpenDateTime], [S7_CloseDateTime], [S7_GS_NKUser], [S7_ControllerID], [S7_ParentID], [S7_ParentTableCode]) VALUES (N'ec3af408-66dc-408e-9016-7fff9024d4ca', N'Y', N'COR', 0, 0, 4, 0, 0, CAST(N'2070-02-10T00:00:00' AS SmallDateTime), NULL, N'RIS', N'd0d2148a-6188-47b4-8d59-70e591518437', NULL, N'  ')

INSERT [StmActivityLog] ([S7_PK], [S7_EnterpriseActivity], [S7_FormCaption], [S7_KeyStrokes], [S7_MouseClicks], [S7_ControlChanges], [S7_ActiveTime], [S7_InactiveTime], [S7_OpenDateTime], [S7_CloseDateTime], [S7_GS_NKUser], [S7_ControllerID], [S7_ParentID], [S7_ParentTableCode]) VALUES (N'7dc0546d-6a77-47c5-9331-17f88e167556', N'Y', N'COR', 0, 0, 8, 0, 0, CAST(N'2070-02-10T00:00:00' AS SmallDateTime), NULL, N'RIS', N'793C4136-27D7-45A9-8C80-3C825A78883F', NULL, N'  ')

INSERT [StmActivityLog] ([S7_PK], [S7_EnterpriseActivity], [S7_FormCaption], [S7_KeyStrokes], [S7_MouseClicks], [S7_ControlChanges], [S7_ActiveTime], [S7_InactiveTime], [S7_OpenDateTime], [S7_CloseDateTime], [S7_GS_NKUser], [S7_ControllerID], [S7_ParentID], [S7_ParentTableCode]) VALUES (N'1a8e9f8e-c409-4d83-a1e1-553a5472e260', N'Y', N'COR', 0, 0, 3, 0, 0, CAST(N'2070-02-03T00:00:00' AS SmallDateTime), NULL, N'RIS', N'd0d2148a-6188-47b4-8d59-70e591518437', NULL, N'  ')

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'df1717e3-03c7-44dd-ad4b-bcd6a06fa9da', N'Physical_System_MailServer', NULL, NULL, N'STR', N'N', N'N', 0x500072006F00640053006D0074007000530065007200760065007200, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'9219f25b-a581-451a-a351-7f0227da5bc5', N'POP3MailboxEmailAddress', NULL, NULL, N'   ', N'N', N'N', 0x4F0072006900670069006E0061006C0020004D00610069006C0062006F007800200050004F0050003300200045006D00610069006C0020004100640064007200650073007300, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'f05b66c0-af24-425c-a3d5-39a4cc442c33', N'DoAdditionalPOP3Logging', NULL, NULL, N'BOL', N'N', N'N', 0x5400720075006500, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'21dc9557-e35b-4dac-bc08-35af4b7ea9a7', N'POP3 Server', NULL, NULL, N'   ', N'N', N'N', 0x4F0072006900670069006E0061006C00200050004F00500033002000530065007200760065007200, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'6ab6fbbc-d592-4e2b-be9f-d01b51509761', N'POP3Port', NULL, NULL, N'   ', N'N', N'N', 0x4F0072006900670069006E0061006C00200050004F0050003300200050006F0072007400, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'69254a45-5b05-42b6-b714-a88dd250ed22', N'POP3MailboxUserName', NULL, NULL, N'   ', N'N', N'N', 0x4F0072006900670069006E0061006C00200050004F005000330020004D00610069006C0062006F007800200055007300650072004E0061006D006500, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'e05bce8d-f979-4bbb-8482-ad091094eed7', N'POP3MailboxDisplayName', NULL, NULL, N'   ', N'N', N'N', 0x4F0072006900670069006E0061006C00200050004F005000330020004D00610069006C0062006F007800200044006900730070006C00610079004E0061006D006500, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'e90761ce-8f11-43e5-b605-7abbdc3e8406', N'POP3MailboxPassword', NULL, NULL, N'   ', N'N', N'N', 0x4F0072006900670069006E0061006C00200050004F005000330020004D00610069006C0062006F0078002000500061007300730077006F0072006400, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'd7d7aa42-663d-41eb-9c00-ee16a69312ef', N'AllowEmailsToBeSentFromUsersAddress', NULL, NULL, N'BOL', N'N', N'N', 0x460061006C0073006500, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'd319d50c-c48c-4ef4-8561-140595daceaa', N'SMTPDefaultReturnEmailAddress', NULL, NULL, N'STR', N'N', N'N', 0x700072006F0064002E0073006D0074007000400063006100720067006F0077006900730065002E0063006F006D00, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'86b6ddf0-83ce-4b74-a0f4-bb35425b093d', N'SendEmailsInSeparateThread', NULL, NULL, N'BOL', N'N', N'N', 0x5400720075006500, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'5cca8ce0-f205-4859-bc2d-431a8e9c5616', N'SMTPPort', NULL, NULL, N'INT', N'N', N'N', 0x37003100, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'e585488a-45fe-4698-9b5c-9afcfe149581', N'SMTPPassword', NULL, NULL, N'STR', N'N', N'N', 0x700072006F00640073006D0074007000700077006400, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'854fa3a3-5d91-408a-8108-7443bc4e24ce', N'SMTPUsername', NULL, NULL, N'STR', N'N', N'N', 0x500072006F00640053006D007400700055007300650072004E0061006D006500, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'dfffd48c-07db-405a-a692-40c483efcbad', N'UseDotNetSMTPComponentForNonSMIMEApplications', NULL, NULL, N'BOL', N'N', N'N', 0x5400720075006500, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'd144042c-e6e8-4eed-bee7-d1dd6a89f5a9', N'FreightNotesLengthNew', N'd438c595-b8d2-4b56-b3a4-3b3cd37d9418', NULL, N'   ', N'N', N'N', 0x41003000740062006F007800720076005A0046004B004A0050006E0055007A0050006B0072006D0074004100680063004400480069006F0042003200530038006600710042005900320039004A00460037007A00580058006B0056004C005500320064003800390058006A0047004D00520049006A00480032006A00390076006300310044006A0066006E00330074005A007A0045005A00580049006600750057004200690041004300670038005A003600320052006C00540068005A00300035006A00650066006D002F00560049004A00440058006C006700300058004C0031006F006F004B0066003700380032004A006E002B002F00390067007A00730066005100560039007A0077003000330066007900540078004900650046006800760052005600580049007A0050007100460050007A00650052004D004A0058004F006300310066005A004C006500700069006F0032005A00610042004C00760043006C007A00320068007700730054006C0066005A007A006E002F0044006D00320078006D006C00760032004C0033004A0072007800330068007A006D007A004B0034004A00370050007000780036007900620046004C004F004E00340073002B00720070004C004800610069003000530052005400760050004F004F0062004300300061003900570048006A004A00770067005300460053004300750076006A00730069004E0071007900720039006F007600630038006D007600730055006900310072003600470069004F006A00540079003500480050006E00530046006B0062006D0077004200730047006A00530070006C005900350047003000680047004E007400490057002B0034005300560057002B0069002F006B0038006300500032002F00510048006300720065004B007500660056005000440071004500790054002F006C005300380063004C0053006B007800350079006D006E006A004A00580048005500320032007300330035004B004600300077004B00390050003400720059005300750050005800750044007000670067007900460036004B00670064003100450051006500390041004200320041004F0062006D0076004500720050007A006C004800590068004B0077002B0070004C007900380035004100510078007700760052002B002B004E0037004F002B004200480059006F0034004A0036004C006200730043006D0030004F0072004B0071004C002F006A0034005900510059007700340062004900690068004E00550070006C004C00570059004B004800520066004F004A006F004600730041007000610058005300500078006A0037005600340078004E0072006900540057002B005300350074004F0042003700390077007600560056007A005800660049006800720059007700640074007000730050006F0056007100640031004F005A0076006B0077004D00340053003000340061006500390034007A00500045004300500068007300780057006400490065004B00750056006C005200530052004B004700480058007A0034006E004E006A00740068007300780045004B0062006F004100750066004A0050004400580047004200610059006A007A0052006900660068007300300044005300580062006A00390058004600730069007800380039005A007600680033005400620067006200420064002F00770038006600670054004B0076005A00500038005700410051003900780032006E0067004F00770053007A004E00550068006E006200590070004400310033006D004C006F0032003300590079006B007200680059004B0067006E006F0064005700720035006A004200780070006400640075007200780062002F0077007000750053007200640037003300690035004F005400560056002B00510035003700300050006C002F004D00380036007A0031006D005000370035004C005600710039006A0039004900670058006E00590044006D004300470051006B004700490048004C006A0072007A0031004B004E00680069007400770071004D004500670078007100370037003300770062005900610049004C002F007300470067006B00790076006F0056002B006C0042006700630066005600640062004B0078004D006300550037005500450047006400680032004B00580046006B004D004F00630047004D0074006F005A0053004F0038004E00570079004C007300590033003300530049004E0074004500430039005A0063004E004E0036003000630030007A004C0049007600520030007A0069006900310065006C00, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'9f0fd550-0912-4810-87d4-3f6a260e8a33', N'XmlSchemaValidationStrict', NULL, NULL, N'   ', N'N', N'N', 0x58004D004C005F0053006300680065006D0061005F00560061006C00690064006100740069006F006E005F00530074007200690063007400, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'f3debdca-a0b2-4337-9b88-0f505323eb42', N'ContainerCountPackages', NULL, NULL, N'INT', N'Y', N'Y', 0x3900, N'00000000-0000-0000-0000-000000000000')

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'360681b8-74e4-4c04-b10f-127bc541f474', N'BackupFilePath', NULL, NULL, N'   ', N'N', N'N', 0x4200610063006B00750070005F00460069006C0065005F005000610074006800, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'c67c613b-6727-48a8-b58c-3618f8e86a96', N'ConsolsDataImportDirectory', NULL, NULL, N'   ', N'N', N'N', 0x63006F006E0073006F006C0073005F006400690072006500630074006F0072007900, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'edbb20bc-20d9-44ce-b64d-1bb0c23a656d', N'OrgMatchThreshold', NULL, NULL, N'STR', N'Y', N'Y', NULL, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'eccc3183-0137-4f6f-ad2a-5c9c1b8fbcee', N'NZCustomsCertificateCopyToEDocs', NULL, NULL, N'   ', N'N', N'N', 0x63007500730074006F006D007300200063006500720074006900660069006300610074006500, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'b4dc090a-2def-44b3-8a3d-68f7288669b7', N'NZCustomsCertificateCopyToEDocs', N'56a25e2c-dc5b-4516-a508-d4a022ca9a03', NULL, N'   ', N'N', N'N', 0x63007500730074006F006D0073002000630065007200740069006600690063006100740065002000770069007400680020006F0077006E0065007200, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'dacb773b-99c2-485f-bb45-3761c482137a', N'NZDeliveryOrderCopies', N'63752c0e-d53b-4753-84f5-424577382e31', N'2a5075d4-9a98-4f94-8ba4-16d785f2624f', N'   ', N'N', N'N', 0x640065006C006900760065007200790020006F0072006400650072002000770069007400680020006F0077006E0065007200200061006E00640020006400650070006100720074006D0065006E007400, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'0de59922-ddb7-4b95-bf6d-f04ce97d68d6', N'NZDeliveryOrderCopies', NULL, N'14e7be54-dba9-4819-a8ea-84cbf40e6d1d', N'   ', N'N', N'N', 0x640065006C006900760065007200790020006F0072006400650072002000770069007400680020006400650070006100720074006D0065006E00740020006F006E006C007900, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'be536891-2b79-4c36-a62d-0f7481fcbd55', N'PhysicalServerID', NULL, NULL, N'STR', N'N', N'N', 0x50006800790073006900630061006C0053006500720076006500720049004400, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'7789b4cc-8466-4b7e-9c08-480df7cc5dc8', N'WarehouseCountPackages', NULL, NULL, N'DT ', N'Y', N'Y', 0x32003000300039002D00300031002D00300032002000300031003A00300032003A00300033002E00340035003600, N'00000000-0000-0000-0000-000000000000')

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'7c52bc47-fa43-455e-bdb7-bcc4ed5e33d3', N'LockoutSpid', NULL, NULL, N'INT', N'N', N'N', 0x2D003100, N'00000000-0000-0000-0000-000000000000')

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'e3ca369c-222e-4c50-8cb7-59a7731b334f', N'LockoutLoginTime', NULL, NULL, N'DT ', N'N', N'N', 0x30003000300031002D00300031002D00300031002000300030003A00300030003A00300030002E00300030003000, N'00000000-0000-0000-0000-000000000000')

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'cb726866-c542-40bd-ba7d-a4c797756c3a', N'DATABASE_SCHEMA_VERSION', NULL, NULL, N'   ', N'N', N'N', 0x3200300030003000, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'2e8af8e7-5625-43d2-a7e7-cd6b5a790817', N'FreightNotesLengthNew', N'e7ac2090-bb53-43f9-bfa5-687a7bae4467', NULL, N'STR', N'N', N'N', 0x41003000740062006F007800720076005A0046004B004A0050006E0055007A0050006B0072006D0074004100680063004400480069006F0042003200530038006600710042005900320039004A00460037007A00580058006B0056004C005500320064003800390058006A0047004D00520049006A00480032006A0039007600370038005300620061004500340039004C007600700042007900630078005400420076004B00650030003000410033004E0030003800710077005900460037004E004600650037004C006C0053004C007600580076007900310044006300670039006E002B007A0073004A00350076003900690068005500570050007A004700320032003500780051005A006E004B005900770074003000330063005000480062004D003400520053006B005900740036006C00520071006E0031004800790042005A007400320051005100330063002B006B002B0043004C0041006E0079006E006900480072002F006D005A002B004700720067004D0047006A004800420071004E00590044003500700038005A0052004E00480043004A0059006F00650054006A0068007800630057006500500042004F0043004B004A00680069004A005200470066006800770044007A00590041004B00720063004F002B00370050006F006A0073003600480064006300450074003100650054006E0047005100680072006E004C0033005A004B007A002F0033005300780047004F006A006800450079003900410073006A0066006100640053005A005A0062005900340056006F0049004B006C003000440034005500490071007600490046004D00570034005200420030005800710056006A0038007A0049004C00410059005000650030004A0044005500640079005000550049003900310031005000410064007700710079006500790038003800760068004C00430063004D006A00710031006B00480056006D0038004E0038006F005000530070004C00620045006E006F00460079004E007900650077006400340056002F00350032004F0055004A0043006200740078004200520074006B0066005400750032004300590041007400740069006200390037004700310036004D002B0051002F0057004F0041006B004800710033004100340038004B0037006E0059002B004D006F0053005900660071004400420052006F004200620055006600620053006C004800550059006A0034003900570059005500420035002B00350068006400580078006D00510035004F0054007A006C0050004F0063006E006200520048002B0073004C0039002B004C003500540031007A005800680066004C00530052007200680050002B004D0057007100660071003200360058006400670075004E00390042006D0067004F0077006300640074006600680078006C007900350031006F003000680032007800740076004C00360070006600510057006A00570055006300590058004E00610034006B006500790047004C0052005100390071003700630064002B0039006B007500440049006E003500360077005300310076006F004E007600330058007600390071004E0057002F0059004B0038003200540073004E0075004100670076003500770063006D004A007A00640044005600460041005600660050002B0069004B00570074003300370075004C00500049003400640043004D007A00740074003300640078006500560047007700700056006F00310065004600530041004B006E0056006D004E0057004F006A006F0049006C00560046006900590034005400650059004A00480079006700560072004C0044006400430059006A005A00470047005600490044004F004D005000670038007A006E0076007A00730059006B004D004400630067005500560074004D0039004200320049006F00560042006F004A0037002B006300, NULL)

INSERT [StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_DepartmentGuid], [SD_Type], [SD_IsLogged], [SD_IsReplicated], [SD_BinaryValue], [SD_GuidValue]) VALUES (N'0c593337-338b-4239-852a-b23dcda57e76', N'FreightNotesHeaderLength', NULL, NULL, N'STR', N'N', N'N', 0x4700410050004C0063005000340079003800590061004D0068004600480059006500610065004E0047004B00430066004C0065007800730053005300440043007800320059006C00620033006E005500670034007200630030004E00510062004700410049004A00470063007500660079005600310058006F004B00500057005700300046003000560068003500710068006D004B00730062004F00540078004A0071003400630071005A007A0039006A00470064006B005600550046005500580032006E004C004C0056006800760043007100670077004F004F00560038002F00670032004D004E0055006A0067004300520072006D0058006C00340041004A00610073004500630057004B004F00450075004E0032002F00310037006700740069007000530054002F0068007600520059005000490042002F0057004E0056002F003500430067004C006500420069004C0032006B0047005A004B0053004C0062005A006A007900520051004B0037005A004C00640043005000560079007000460078004E00490031005100490035006F003100470048007200530069006B00350066004C00650063006A00590057006D0042003400500052006E0034006600370071006700340054002B0063006F0078006C0063004500430063004100780045005A006100460044005400310032004A00540046006D004600330055006E004D007800640076005300430041006E004100780050006800470063002F00730072007800650033006300670035004700310030005800510038004100500066002F007300330030004B00790032006700560072007A005200690042007200340066005500610045004500420068006700490078004800710064005300320050004B0034006E007500790061005A002B006D0044002F00780041003900610055007500330037002F0030003700630056004F0046004E0066005A004400760046005500320052006C004E00740064006F006F00300053004A00490075007600390059007300370031004500550072006E00480069006D004C006E0071006A00460068005400350035005500340037006F00790038004B0046005900610033006E004C00500043004D0042004A006C0036006E0063003800730055007A007900570045005400710074004300320039007600690044006100700079006E00520054004C0035004F004400450071005900590058004B0078007700350070007A0035004A0041002B006900470032007200480053007700770053007700450047003400310061006200500076005900740036004E004F0065004A00770044004300300064004E006700530046006F00470038004F0063002B002F006600310058007000660030006B004800470050004C0059002F0032006E00570051004500710053004B004C0070002B004E006D005700470052007800320062004D00610056006D00750037006F0032006400680065004C002B0049004F00480049004D002F0053007200300036005A0072004C005A006B00650061006B00630045002F006D007A00620037006F00450079005A0063007100700077006300690055007300530062005600550059004E0045005600480072005000610054006300310074007500300071004E002B0038003800620065006E007A006900650051006B007A004D0050005A00310054007A0055006D00750055005900770066007A007A0078006500490065004F00770035006500760038006E00500067002F00570031004B007A005200720075002B007800630034004C004900720074004A00650067006D0074004D006700520071006600480057005A004600560076004400520043004B00630034004A0062005100620037004F0074006500580056007600440068004E0041004D0033002F0038006A0037005A0048003100730045005600550078004B004B002F00770035002F006D0039006200510036006E00610034006D00730051006A003200470067004600460033006F00510031007400360033004C0066004B0056006A007100480030005A004700760057007400300072005900490062006B00650063003500720038004B00610039006A004C003200780035004C0030007A0053004900680067004A005600330076007500460054004400320064006E007A002B00500075006F004A00530074006600320032005300410036006600500037007700620055006B004A00750062007800580065004D006A0044006F0068005700670062004500780041006700780031006F006400370069006F006D0064006A005700370042006900580037003300740079004A00590035005400450037006A0064004800530069006700310049005300360038003500700048004D0058006E006500410068002B00630031004C004F0054005900640038007200620039002B00650072004E005000310063006300670058005A006E00630077004B0053004E00340034006A0064002F0030004900420033003200660066006E004600530058006500580033006D00690069006E007800300041003200390061007100350074004300620038007300700031004A005A0058002B0057004F007800480062007A004F004C006C00710070005900540051004F0054004E00410063003900330070004A00610069004F0045006C006E00610034006D0068004D006F006A00300045004B0076005900610068006A00580075006B004400380039006800690065006400720077007A00480077005700610076006600690036006B0032002F00740048006D003600500067007500540071007500410053004F00360030004100540057006F003400780043006C0062005600630045004300510046006C0074005A004700550038003700490035003500750039002F006D004500590059004D006A007A00760048004800710039005A00500048004A005400340079006B00790073003900610063006600530038004100350038004400640065006B004A007A0045004300720044006800390034007000660067004C00540036006E005A003600710066004F007400630044005200780048006A0052004B003700670059006F007A004C006200560070006B0067007A006F00560039006300350042004B002F007400590074006800790077006C0032003300590034005500440035003400320079004C0049005100560044006C0052006A0058005800590032006B00390064004100500079003100500057006800740057006C00580032005200680053004900750048007A006800610064002F0033004C004100520052004600470071004100450079004A004C00770049007A006B004600760068004200420057004F005100510036003200650064002B00450032006E005500720059004E00670041004500760051004200440030005100680064002B0069004300350075004600700061006C0063004F004A0079006F0071007700740055005200560049005500580041006E0036004A006C00470067006800500030007100660042004A0074007400440048007A0075007A0072002B002B0057004B006100360077004B002B006A00440053003800670054006A006400770068006200320033004D006D0056007900730078002F005500640039004E006B0036007100410030006F005A006D00360069004B0048007400340052003400560062004A00780031004A0038004F004B0072005000650033007500500078006C00570055004C006F0054007000770061004A00750042004800550039003400590049006F004A00790033003800480079006C003100790072006C0079003000620076004200710065004300560048005300450032007900520059004400670074007000750067006200420045004D006F005A006C006B00450079003600720052004C0068007A0051004B00300056002F00770047005400660045004B005200320068004300340046005600710043004500670075002F003000460070007700440043007A0055006D004F006D0045007600590078004A004B004E005100640049004B00310074006E00370051004A00540034006E0066004E0052005A0041003000780062006D0042004F007700700078003600520070004A0064002F00340076004D0030007400340058005000380062005600650050004B00450057003400550072006A00640075007A004D006E004A00720051004E006D0079004D00370073004B0042004F00520057006C004400630046006A007A004F005A00670034004E005200330068004900510044007A002F0044007A007500500038004800610067006800390074006900660047004B00590067006300610049006F00580069006C0048004800760052006300630041004C0034006F007A0032004B007300430065006A0077003100650058006C0079006900390030006A006700590059006300630078004B004C005600380042004C00650043006A0044003400790030004C00680061006A006E00420062005800550046006800670049006E00430033005800660069004100510069005400630061004F006B006A0072005200660034007500380048006D004D00500043007100430052004300790061002B003100630045006A0034006D00770059004E00520062004C00300050004F004400490054006E0042003500320078003900470031006C007200790055006100500058007200670032003900630038005700510041004A005500520079004F00340063004F0043006300670075004E003900540058006E004500750062006E00510045004D005400320069005400780051006300310053006E0048004D0078006F0032005900350051004300670041002F0063004F005200640064002F0071002B0047007A006100690046006100440038005200470066002B0065005500360070006B002F00750068004E005600620066004B00350071004600590069004800370030002B0064004C00460043004C0044003200780067006D005500520031004E006100300047005700720061004D00730041004A006A00470047004E0068007500340058003400720032006C004700340042006300620046005A00790055007600510054007A002F003800310032007100340038004E004F006D002B006C003600720043007100730044006C0074006F00330061006C0078003800320048004F004400500034004A006700480068006F0055007600680043006C002F00760038005000420076005100620076003800630036006F004200340063005200690054004B004500740031004D007000460069006F004C005A0070004F004B006100390074007700460034002F007000750076004F0039004B00770066006F004B00570063006700620032004A00520043006A006A0068004C00480067004E0039006D007A004F0048005500580056004B006B004B004D00570067006E00730065004E0045007700540054004F00530064002F006A0044006800690033006E004600560059004B006A004E0041004F00510070004A00630030004C006D00570052004A006C006800660067004300680045006100690063006E0057005A004F0046006E004B0032004C0033004800630051005300320074006800580056007200760038007A006A00320044006300450071006900450068006100660064004B007900710073004300420072007400340066004B007000440062005900320064006D0063004C003300760059004A006900580072005A003000320030006900330038002B007300530067002B00340043005800730036006600360069002B0039006A006300780068007900790042007800300056004B004100680067004A00610043004200330052004E006A0071002F007800540032004600570045002F006F0041004500660045006F004200630077003D003D00, NULL)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1001, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1002, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1003, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1004, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1005, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1006, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1007, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1008, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1009, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1010, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1011, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1012, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1013, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1014, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1015, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1016, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1017, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1018, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1019, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1020, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1021, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1022, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1023, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1024, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1025, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1026, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1027, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1028, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1029, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1030, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1031, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1032, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1033, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1034, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1035, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1036, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1037, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1038, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1039, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1040, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1041, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1042, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1043, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1044, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1045, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1046, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1047, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1048, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1049, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1050, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1051, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1052, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1053, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1054, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1055, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1056, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1057, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1058, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1059, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1060, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1061, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1062, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1063, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1064, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1065, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1066, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1067, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1068, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1069, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1070, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1071, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1072, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1073, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1074, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1075, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1076, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1077, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1078, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1079, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1080, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1081, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1082, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1083, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1084, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1085, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1086, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1087, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1088, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1089, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1090, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1091, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1092, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1093, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1094, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1095, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1096, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1097, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1098, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1099, 24)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1094, 7)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1095, 7)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1096, 7)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1097, 7)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1098, 7)

INSERT [StmNumberCache] ([SG_Value], [SG_SN]) VALUES (1099, 7)

SET IDENTITY_INSERT [StmNums] ON 

INSERT [StmNums] ([SN_Name], [SN_Value], [SN_Owner], [SN_ID]) VALUES (N'ARInvoiceNo', 1100, N'921c6a9e-e9d6-413d-9010-83d1051bf1ea', 24)

INSERT [StmNums] ([SN_Name], [SN_Value], [SN_Owner], [SN_ID]) VALUES (N'IncidentApprovalClientRef', 1100, NULL, 7)

SET IDENTITY_INSERT [StmNums] OFF

INSERT [StmPrintJob] ([SP_PK], [SP_Status], [SP_RunDateTime], [SP_PrintPriority], [SP_Sequence], [SP_Group], [SP_JobType], [SP_CustomProperties], [SP_BlobType], [SP_EmailFaxDestination], [SP_EmailAttachmentFormat], [SP_EmailSubjectLine], [SP_EmailSignature], [SP_EmailFromAddress], [SP_EmailAttachments], [SP_DocumentName], [SP_DocumentType], [SP_RelatedBusinessContext], [SP_ParentGuid], [SP_ParentTableName], [SP_RetryAttempts], [SP_EscapeSequence], [SP_Copies], [SP_GS_NKJobSubmittedBy], [SP_WatermarkText], [SP_WatermarkImage], [SP_NoOfCoverPages], [SP_SQ], [SP_SB_DeliveryGroup], [SP_GB]) VALUES (N'c056a785-3171-4c49-b9d2-031e73d710c8', N'QUE', NULL, 0, 0, NULL, N'PRN', NULL, N'XLS', N'', N'TIF', N'', N'', N'', N'', N'', N'', N'', NULL, N'', 0, NULL, 1, N'', N'', NULL, 0, NULL, NULL, NULL)

INSERT [StmPrintJob] ([SP_PK], [SP_Status], [SP_RunDateTime], [SP_PrintPriority], [SP_Sequence], [SP_Group], [SP_JobType], [SP_CustomProperties], [SP_BlobType], [SP_EmailFaxDestination], [SP_EmailAttachmentFormat], [SP_EmailSubjectLine], [SP_EmailSignature], [SP_EmailFromAddress], [SP_EmailAttachments], [SP_DocumentName], [SP_DocumentType], [SP_RelatedBusinessContext], [SP_ParentGuid], [SP_ParentTableName], [SP_RetryAttempts], [SP_EscapeSequence], [SP_Copies], [SP_GS_NKJobSubmittedBy], [SP_WatermarkText], [SP_WatermarkImage], [SP_NoOfCoverPages], [SP_SQ], [SP_SB_DeliveryGroup], [SP_GB]) VALUES (N'af641097-322c-4251-8a41-765d4aff68fc', N'QUE', NULL, 0, 0, NULL, N'PRN', NULL, N'XLS', N'', N'TIF', N'', N'', N'', N'', N'', N'', N'', NULL, N'', 0, NULL, 1, N'', N'', NULL, 0, NULL, NULL, NULL)

INSERT [StmPrintJobCopyRecipient] ([SPR_PK], [SPR_SP], [SPR_RecipientType], [SPR_EmailAddress]) VALUES (N'f37025ca-ea51-4068-86eb-b207404af9ba', N'c056a785-3171-4c49-b9d2-031e73d710c8', N'CC', N'email@address.com')

INSERT [StmScheduleTask] ([S5_PK], [S5_ScheduleDescription], [S5_TaskPeriod], [S5_WeekDaysOnly], [S5_TaskPeriodCount], [S5_DayNumber], [S5_DayList], [S5_MonthNumber], [S5_WeekDayOccurrenceNumber], [S5_StartDate], [S5_EndAfterCount], [S5_EndDate], [S5_ScheduleActualRunCount], [S5_AccountingPeriodScheduleFrstRun], [S5_DateScheduleFirstRun], [S5_ScheduleType], [S5_TypeOfDocument], [S5_NextScheduledPrintRunTime], [S5_CurrentPrintRunTime], [S5_CancelledAtDateTime], [S5_IsActive], [S5_IsPrivate], [S5_ScheduleState], [S5_GB], [S5_ParentTableCode], [S5_ParentID], [S5_DailyStartTime], [S5_DailyEndTime], [S5_RunTimeInMinutes]) VALUES (N'2a54e902-b1ec-472c-bca8-1050828380a9', N'Test Service Provider 4', N'C', N'N', 0, 0, N'NNNNNNN', 0, 0, CAST(N'2008-04-02T00:00:00.000' AS DateTime), 0, NULL, 0, 0, NULL, N'TS4', N'TST', CAST(N'2008-04-02T11:53:22.340' AS DateTime), NULL, NULL, N'N', N'Y', NULL, N'2fdba7fb-60ba-4a03-8336-0defac4f9673', N'SH', N'8335b1fa-8dd7-4274-b8f3-82fee6284d55', CAST(N'1900-01-01T00:00:00' AS SmallDateTime), CAST(N'1900-01-01T00:00:00' AS SmallDateTime), 0)

INSERT [StmScheduleTask] ([S5_PK], [S5_ScheduleDescription], [S5_TaskPeriod], [S5_WeekDaysOnly], [S5_TaskPeriodCount], [S5_DayNumber], [S5_DayList], [S5_MonthNumber], [S5_WeekDayOccurrenceNumber], [S5_StartDate], [S5_EndAfterCount], [S5_EndDate], [S5_ScheduleActualRunCount], [S5_AccountingPeriodScheduleFrstRun], [S5_DateScheduleFirstRun], [S5_ScheduleType], [S5_TypeOfDocument], [S5_NextScheduledPrintRunTime], [S5_CurrentPrintRunTime], [S5_CancelledAtDateTime], [S5_IsActive], [S5_IsPrivate], [S5_ScheduleState], [S5_GB], [S5_ParentTableCode], [S5_ParentID], [S5_DailyStartTime], [S5_DailyEndTime], [S5_RunTimeInMinutes]) VALUES (N'ee7d2b44-7357-44e2-9905-ccbdf75834a2', N'Sales - Missing Client Rates', N'D', N'Y', 1, 0, N'NNNNNNN', 0, 0, CAST(N'2008-03-21T00:00:00.000' AS DateTime), 0, NULL, 0, 0, NULL, N'REP', N'   ', CAST(N'2008-03-21T20:39:21.250' AS DateTime), CAST(N'2008-03-21T20:39:27.410' AS DateTime), NULL, N'N', N'Y', 0x505AD556CD6F1B4514F7AEBFED04DC36D00291585995B8C41B3BA9E352E54389DD3451F3451D2A94502DEBDD677BC87AD79A994DEADE41489C7AE4C6851B378E880B1C3922FE04C4050E9C4182F776371FA5F9306D2E8CB56FC7B3EF7BDEEFCDC494582CF6370E7AD31851917C78D795C0FB9C09D01B9EE5F7C09577DD0E7361427B085C30CF9D9BD2CBF49BD0EABE237D0E732EF8929BCE84B6E5B71C66DD87C1B6B707EEDCAD76B556B6DB53B572B536335D2D27C954E34C0B7AD3EA82ED3BC0F5255FE08210FA03E87B5C368133D3614F4C89F657DDB697444589F705F0D412375DAB9B6B40DFE49274A5429191555748EE5B242194783C91C8370742424FBFE733FBE45C3BDBA15015E5453F9BA9010EDB073E38699044E849C59164BFFFD27706428048FC85F97EC60FE2334CD568A986A51AB66A806AB455A3A31A5DD560AAF1B16AECC58E47269D56A3015B5FBDF3D9D73F6C7CFEFB2FFEAF4FF98FCA9FA89BF417C7BFFD14329F2C7FE1FEEC7C77CD7E9A4D910F6924499A5D182E31E5C2E986D983C2327350A0EE390E04C1BDBDD9A797E96C43AFEF98129A5D0079FCFD4A1368067613356C721BF8AB872BF7B8E7F7970663C8ECF7DC15306DE676D64DD7EC0057123466CFF1CE7725EB41685DE8C70637DBABA18F94F195A1155C1006299B1A5AD951AC24561E5A2C4A887A3E2E9E8F1CB357F7DC36EBF83CC0848892785477192463EB4C208C3A5ADD61A84C7B80418A6C96AA2147848A2F3B426494E0FF0A92ED134EEC6C0FFA202E03F6E9170350B6EEE1F28627A1B0EA5A8E6FC3D142E29932DE59E4569749DC37744BDF694A8E5153380AA949FE81A0B8909D5AD378FBA1E9F860343CB4B16859D880D6A1C138723A038514E66249CADF4B5529594A875311FF4689BAC131235605B8D8F02C7D8D09F951657777686B918589B3317E09DBF9E851B039054A6DEE3211478949056B223E7E4A5A564CD19566CB81C0FE15B29F7F31905257CE3798402F06D4E3B2CB0C1C9BD29D396C558A12564FEA2A013A4210F14E68DB78E4086A8FDABA67E3FF4DCED0DA84D60021991BE031758D4A3E94FA97D099324A72E4457AC730B1A8512C63B1B086472FABDFD09665A23E2E421BAF21F949995D78DC73B4FDA8D68A15BD5CD4C0B53C629C2BFAB25DAACC1417E64773B3E14913DA688294A449436157DC792CD85CB12B65FFCEE4E4C1C1817E30AD7BBC3339552E57263F585FA3EB42CF2C316C1B780780E291947DB154114D6BDA6C0486F9F2ECE4E1145D9A3CCDA7F90455FCFF12AF542229033B5E4F240DC19E40C6887626F15FE01379BB1BE03F93C9BE4E1D367A1204C773414B87406ECD33ED65D3921E4F477165EA5E0FAF6EC00BC45947706C716F9F214833B4D0446F13189A4805AD59C462F1782C99CC674EB3B57AA8EBE6691F9FD3FFDB7BB7170240E48293F03A911BD49C29B0302A7A8647E4896B48F60D24A3F1C2F5E8E256B8114D92F461F165B017E03BD8D235E6EE6DDDCF84D5BADAC86333B1380B24152544E39B486E96A3513A851C8ED45BC4D99EAECD98B50A94CCCA74B9746BA675BBD4AA562BA59A597DB755ADD960B72AA971E4BCBA660AA935CD7DB0B50821F97F00, N'2fdba7fb-60ba-4a03-8336-0defac4f9673', N'SU', N'f376a71e-a130-46b8-b551-7a59b57dedb1', NULL, NULL, 0)

INSERT [StmScheduleTask] ([S5_PK], [S5_ScheduleDescription], [S5_TaskPeriod], [S5_WeekDaysOnly], [S5_TaskPeriodCount], [S5_DayNumber], [S5_DayList], [S5_MonthNumber], [S5_WeekDayOccurrenceNumber], [S5_StartDate], [S5_EndAfterCount], [S5_EndDate], [S5_ScheduleActualRunCount], [S5_AccountingPeriodScheduleFrstRun], [S5_DateScheduleFirstRun], [S5_ScheduleType], [S5_TypeOfDocument], [S5_NextScheduledPrintRunTime], [S5_CurrentPrintRunTime], [S5_CancelledAtDateTime], [S5_IsActive], [S5_IsPrivate], [S5_ScheduleState], [S5_GB], [S5_ParentTableCode], [S5_ParentID], [S5_DailyStartTime], [S5_DailyEndTime], [S5_RunTimeInMinutes]) VALUES (N'5c09cdf2-c37a-452f-aec9-f4e0f288d7f5', N'EDoc Image Compression Task', N'D', N'N', 1, 0, N'NNNNNNN', 0, 0, CAST(N'2008-07-03T00:00:00.000' AS DateTime), 0, NULL, 0, 0, NULL, N'ECT', N'SYS', CAST(N'2009-01-24T01:00:00.000' AS DateTime), NULL, NULL, N'Y', N'Y', 0x3C486F737465645365727669636553657269616C697A61626C6553657474696E6773202F3E, N'2fdba7fb-60ba-4a03-8336-0defac4f9673', N'SH', N'6dec28e9-4616-47ce-868f-8a51c9c2c223', CAST(N'1900-01-01T01:00:00' AS SmallDateTime), NULL, 0)

INSERT [StmServiceHost] ([SH_PK], [SH_IsActive], [SH_HostName], [SH_LastHeartBeat]) VALUES (N'8335b1fa-8dd7-4274-b8f3-82fee6284d55', N'N', N'iev-wpc-01.corporate.cargowise.com', CAST(N'2008-06-02T12:43:00' AS SmallDateTime))

INSERT [StmServiceHost] ([SH_PK], [SH_IsActive], [SH_HostName], [SH_LastHeartBeat]) VALUES (N'6dec28e9-4616-47ce-868f-8a51c9c2c223', N'N', N'iev-wiss-1.corporate.cargowise.com', CAST(N'2008-07-24T15:59:00' AS SmallDateTime))

INSERT [StmTranslationFeedback] ([XT_PK], [XT_EnterpriseCode], [XT_DatabaseCode], [XT_CompanyCode], [XT_ClientStaffInitial], [XT_Language], [XT_RN_NKDialect], [XT_Source], [XT_OriginalTranslation], [XT_SuggestedTranslation], [XT_Comments], [XT_Screenshot], [XT_Status], [XT_StatusTime], [XT_GS_NKReviewer], [XT_ReviewComment], [XT_SystemCreateTimeUtc], [XT_SystemCreateUser], [XT_SystemLastEditTimeUtc], [XT_SystemLastEditUser]) VALUES (N'54e301c0-34eb-497f-a72e-6493228fffbe', N'EDI', N'DAT', N'EDI', N'XXX', N'FRN', N'AU ', N'Start', N'Commencement', N'Début', N'', NULL, N'NEW', CAST(N'2013-05-27T11:36:00' AS SmallDateTime), N'   ', N'', CAST(N'2013-05-27T11:36:00' AS SmallDateTime), N'   ', NULL, N'   ')

INSERT [StmTranslationFeedbackResource] ([XQ_PK], [XQ_ResourceStringKey], [XQ_ResourceStringLevel], [XQ_MatchType], [XQ_XT]) VALUES (N'5cf14e10-0a87-4bbd-8e73-7aa9ec193dfa', N'JobCartageRunSheet|EY_StartTime', N'SHO', N'*', N'54e301c0-34eb-497f-a72e-6493228fffbe')