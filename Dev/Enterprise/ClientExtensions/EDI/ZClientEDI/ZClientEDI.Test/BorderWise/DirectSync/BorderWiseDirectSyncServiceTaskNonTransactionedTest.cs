using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.BorderWise;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.BorderWise
{
	class BorderWiseDirectSyncServiceTaskNonTransactionedTest : NonTransactionedTestCase
	{
		protected override bool ShouldApplyClientSpecificDatabaseSchema => true;

		[StressTest]
		public void TestRunTask_ShouldConnectToUMPAndRunSyncProcess()
		{
			using (var umpConnection = SetupDatabases())
			{
				InsertSampleData(umpConnection);
				var serviceTask = new BorderWiseDirectSyncServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();

				try
				{
					AssertUpdatedData(umpConnection);
				}
				finally
				{
					ErrorReporter.Clear();
				}

				var logCompletion = "Sync complete with 9 updates.\r\n";

				var serviceLog = serviceTask.ServiceLogger.ToString();

				var expectedReportMessage = FormattableString.Invariant(
$@"Information|Contacts disabled: {contactPK1_1}, {contactPK1_2}
Contacts updated: {contactPK3_1}, {contactPK3_2}, {contactExistsInCWButDeletedInUmpPk}
Organizations disabled: {orgPK1}
Organizations updated: {orgPK2}, {orgPK3}
Addresses updated: {addressPK}
New organizations not synced: {string.Join(", ", activeNewOrgPks.OrderBy(o => o))}
New contacts not synced: {string.Join(", ", activeNewContactPks.OrderBy(o => o))}
Organizations master org updated: {orgPk_MasterOrgDeleted}, {orgPk_SelfAsMasterOrg}, {orgPK4}
Existing contacts not deleted updated: {contactExistsInCWButDeletedInUmpPk}"
);

				AssertContains(expectedReportMessage, serviceLog);
				AssertContains(logCompletion, serviceLog);
				AssertPerformanceLog(serviceLog);
			}
		}

		[StressTest]
		public void TestRunTask_ShouldNotReportIssue_WhenNoUpdateExists()
		{
			using (var umpConnection = SetupDatabases())
			{
				ProcessActiveOrgsAndContacts(umpConnection, true);
				var serviceTask = new BorderWiseDirectSyncServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();

				AssertNullOrEmpty(ErrorReporter.LastKeyReported);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);

				var serviceLog = serviceTask.ServiceLogger.ToString();
				var logCompletion = "Sync complete with 0 updates.\r\n";

				AssertContains(logCompletion, serviceLog);
				AssertPerformanceLog(serviceLog);

				ErrorReporter.Clear();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connectionString = FormattableString.Invariant($"Server={Db.ServerName};Database={TestDatabaseName};Integrated Security=True;");
			EDIDataRegistry.Instance.BorderWiseUmpDatabaseConnectionString.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, connectionString);
		}

		protected override void TearDown()
		{
			base.TearDown();

			using (var connection = Db.NewAdminConnection())
			{
				connection.ExecuteNonQuery(FormattableString.Invariant($"IF EXISTS (SELECT name FROM sys.databases WHERE name = '{TestDatabaseName}') DROP DATABASE {TestDatabaseName}"));
			}
		}

		#region Database setup

		DbConnection SetupDatabases()
		{
			var connection = Db.NewAdminConnection();

			try
			{
				connection.CreateDatabase(TestDatabaseName);

				((ICurrentDbControl)connection).UseDatabase(TestDatabaseName);
				CreateTablesInRemoteDatabase(connection);
			}
			catch
			{
				connection.Dispose();
				throw;
			}

			return connection;
		}

		static void CreateTablesInRemoteDatabase(AdminConnection connection)
		{
			const string createOrgHeaderTableSql =
@"CREATE TABLE [dbo].[OrgHeader](
	[OH_PK] [uniqueidentifier] NOT NULL,
	[OH_Code] [nvarchar](12) NOT NULL,
	[OH_IsActive] [bit] NOT NULL,
	[OH_FullName] [nvarchar](100) NOT NULL,
	[OH_Language] [varchar](7) NOT NULL,
	[OH_WebSite] [nvarchar](250) NOT NULL,
	[OH_RegNoType] [varchar](3) NOT NULL,
	[OH_RegNo] [nvarchar](35) NOT NULL,
	[OH_ServerFarmPattern] [nvarchar](100) NOT NULL,
	[EdiProdRecordId] [uniqueidentifier] NOT NULL,
	[IsSynchronized] [bit] NOT NULL,
	[OH_HasAccess] [bit] NOT NULL,
	[OH_MasterOrgPk] [uniqueidentifier] NULL,
 CONSTRAINT [PK_OrgHeader] PRIMARY KEY CLUSTERED 
(
	[OH_PK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT (newsequentialid()) FOR [OH_PK]
ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT ('') FOR [OH_Code]
ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT ('') FOR [OH_FullName]
ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT ('') FOR [OH_Language]
ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT ('') FOR [OH_WebSite]
ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT ('') FOR [OH_RegNoType]
ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT ('') FOR [OH_RegNo]
ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT ('') FOR [OH_ServerFarmPattern]
ALTER TABLE [dbo].[OrgHeader] ADD  DEFAULT ((0)) FOR [IsSynchronized]
";
			const string createOrgContactTableSql =
@"CREATE TABLE [dbo].[OrgContact](
	[OC_PK] [uniqueidentifier] NOT NULL,
	[OC_IsActive] [bit] NOT NULL,
	[OC_ContactName] [nvarchar](256) NOT NULL,
	[OC_Language] [varchar](7) NOT NULL,
	[OC_Title] [nvarchar](35) NOT NULL,
	[OC_RN_NKNationality] [varchar](2) NOT NULL,
	[OC_Gender] [varchar](1) NOT NULL,
	[OC_Phone] [varchar](20) NOT NULL,
	[OC_PhoneExtension] [varchar](10) NOT NULL,
	[OC_Mobile] [varchar](20) NOT NULL,
	[OC_Birthday] [datetime2](7) NULL,
	[OC_OH] [uniqueidentifier] NOT NULL,
	[OC_Email] [nvarchar](254) NOT NULL,
	[EdiProdRecordId] [uniqueidentifier] NOT NULL,
	[OC_RO] [int] NULL,
	[OC_IsDeleted] [bit] NOT NULL,
	[OC_LastLoginDate] [datetime2](7) NULL,
	[IsSynchronized] [bit] NOT NULL,
	[OC_PasswordHash] [varbinary](20) NULL,
	[OC_PasswordHashIterations] [int] NOT NULL,
	[OC_PasswordSalt] [varbinary](16) NULL,
	[OC_HasAccess] [bit] NOT NULL,
	[OC_BorderWiseAccess] [bit] NOT NULL,
 CONSTRAINT [PK_OrgContact] PRIMARY KEY CLUSTERED 
(
	[OC_PK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT (newsequentialid()) FOR [OC_PK]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_ContactName]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_Language]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_Title]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_RN_NKNationality]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_Gender]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_Phone]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_PhoneExtension]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_Mobile]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ('') FOR [OC_Email]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ((0)) FOR [OC_RO]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ((0)) FOR [OC_IsDeleted]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ((0)) FOR [IsSynchronized]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ((0)) FOR [OC_PasswordHashIterations]
ALTER TABLE [dbo].[OrgContact] ADD  DEFAULT ((1)) FOR [OC_BorderWiseAccess]

ALTER TABLE [dbo].[OrgContact]  WITH CHECK ADD  CONSTRAINT [OrgContact_OC_OH_FK2_OrgHeader_RRR_120N] FOREIGN KEY([OC_OH])
REFERENCES [dbo].[OrgHeader] ([OH_PK])
ALTER TABLE [dbo].[OrgContact] CHECK CONSTRAINT [OrgContact_OC_OH_FK2_OrgHeader_RRR_120N]
";
			const string createOrgAddressTableSql =
@"CREATE TABLE [dbo].[OrgAddress](
	[OA_PK] [uniqueidentifier] NOT NULL,
	[OA_IsActive] [bit] NOT NULL,
	[OA_Code] [nvarchar](25) NOT NULL,
	[OA_Address1] [nvarchar](50) NOT NULL,
	[OA_Address2] [nvarchar](50) NOT NULL,
	[OA_State] [nvarchar](25) NOT NULL,
	[OA_PostCode] [nvarchar](10) NOT NULL,
	[OA_Phone] [varchar](20) NOT NULL,
	[OA_OH] [uniqueidentifier] NOT NULL,
	[OA_Email] [nvarchar](254) NOT NULL,
	[OA_CountryCode] [varchar](2) NOT NULL,
	[OA_City] [nvarchar](50) NOT NULL,
	[OA_Language] [varchar](7) NOT NULL,
	[EdiProdRecordId] [uniqueidentifier] NOT NULL,
	[IsSynchronized] [bit] NOT NULL,
	[OA_IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_OrgAddress] PRIMARY KEY CLUSTERED 
(
	[OA_PK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT (newsequentialid()) FOR [OA_PK]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_Code]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_Address1]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_Address2]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_State]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_PostCode]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_Phone]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_Email]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_CountryCode]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('') FOR [OA_City]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ('EN') FOR [OA_Language]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ((0)) FOR [IsSynchronized]
ALTER TABLE [dbo].[OrgAddress] ADD  DEFAULT ((0)) FOR [OA_IsDeleted]

ALTER TABLE [dbo].[OrgAddress]  WITH CHECK ADD  CONSTRAINT [OrgAddress_OA_OH_FK2_OrgHeader_CRR_120N] FOREIGN KEY([OA_OH])
REFERENCES [dbo].[OrgHeader] ([OH_PK])
ON DELETE CASCADE
ALTER TABLE [dbo].[OrgAddress] CHECK CONSTRAINT [OrgAddress_OA_OH_FK2_OrgHeader_CRR_120N]
";

			connection.ExecuteNonQuery(createOrgHeaderTableSql);
			connection.ExecuteNonQuery(createOrgContactTableSql);
			connection.ExecuteNonQuery(createOrgAddressTableSql);
		}

		const string TestDatabaseName = "BorderWiseDirectSyncServiceTaskTest";

		#endregion

		#region Sample data setup

		Guid orgPK1, orgPK2, orgPK3, orgPK4, orgPk_SelfAsMasterOrg, orgPk_SelfAsMasterOrg2, orgPk_MasterOrgDeleted;
		Guid contactPK1_1, contactPK1_2, contactPK2, contactPK3_1, contactPK3_2;
		Guid contactExistsInCWButDeletedInUmpPk;
		Guid addressPK;
		readonly List<Guid> activeNewOrgPks = new List<Guid>();
		readonly List<Guid> activeNewContactPks = new List<Guid>();

		void InsertSampleData(DbConnection umpConnection)
		{
			// Using some specific PKs to ensure error report is deterministic.

			ProcessActiveOrgsAndContacts(umpConnection);

			orgPK1 = Guid.NewGuid();
			orgPK2 = new Guid("cd8204e8-dd1b-4435-8755-ff962f70b732");
			orgPK3 = new Guid("e3272af3-a234-4f6d-ab62-b65035daa8ac");
			orgPK4 = new Guid("90DFDCBD-645E-470F-AE23-F6DBB2F92EE9");
			orgPk_SelfAsMasterOrg = new Guid("8E31FEDB-FFE5-4F9B-8C02-F4F3816145FF");
			orgPk_SelfAsMasterOrg2 = new Guid("c397f17b-03b8-419a-b446-a96881f6b91e");
			orgPk_MasterOrgDeleted = new Guid("7E12D10F-809D-46F4-A0BF-68EF58FD0591");
			contactPK1_1 = new Guid("cde4b564-c11a-4d61-9c66-5496dc1dfca8");
			contactPK1_2 = new Guid("d74afa4d-a301-465a-b438-44d67a290888");
			contactPK2 = Guid.NewGuid();
			contactPK3_1 = new Guid("15b90f94-1fbe-4aca-a50f-cdfc365cfbe6");
			contactPK3_2 = new Guid("27e231a0-cdb2-45fb-991d-1636db9587f9");
			contactExistsInCWButDeletedInUmpPk = new Guid("c9b0f1a2-3d4e-4f5b-8c6d-7e8f9a0b1c2d");
			addressPK = Guid.NewGuid();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "POCKETSES";
			org2.OH_FullName = "What has it got in its pocketses?";
			org2.OH_Language = "EN-me";

			var contact2_1 = org2.Contacts.AddNew();
			contact2_1.OC_ContactName = "Davey";
			contact2_1.OC_Language = "EN-au";
			contact2_1.OC_Title = "Flightmaster General";
			contact2_1.OC_RN_NKNationality = "DE";
			contact2_1.OC_Gender = "N";
			contact2_1.OC_Phone = "555";
			contact2_1.OC_PhoneExtension = "669";
			contact2_1.OC_Mobile = "04567890";
			contact2_1.OC_Birthday = ZDateTime.BrettsBirthday;
			contact2_1.OC_Email = "daveybaby@daveeast.com";
			contact2_1.OC_WebAccessEnabled = true;

			contact2_1.OC_PasswordHash = new byte[] { 3, 33 };
			contact2_1.OC_PasswordSalt = new byte[] { 4, 44 };
			contact2_1.OC_PasswordHashIterations = 200_000;

			var contact2_2 = org2.Contacts.AddNew();
			contact2_2.OC_ContactName = "Wavey";
			contact2_2.OC_Language = "EN-au";
			contact2_2.OC_Title = "Flightmaster General";
			contact2_2.OC_RN_NKNationality = "DE";
			contact2_2.OC_Gender = "N";
			contact2_2.OC_Phone = "555";
			contact2_2.OC_PhoneExtension = "669";
			contact2_2.OC_Mobile = "04567890";
			contact2_2.OC_Birthday = ZDateTime.BrettsBirthday;
			contact2_2.OC_Email = "daveywavey@daveeast.com";
			contact2_2.OC_WebAccessEnabled = true;

			var contactExistsInCWButDeletedInUMP = org2.Contacts.AddNew();
			contactExistsInCWButDeletedInUMP.OC_ContactName = "Existing contact";
			contactExistsInCWButDeletedInUMP.OC_Email = "notdeleted@zhou.com";
			contactExistsInCWButDeletedInUMP.OC_WebAccessEnabled = false;
			contactExistsInCWButDeletedInUMP.OC_IsActive = false;
			contactExistsInCWButDeletedInUMP.OC_RN_NKNationality = "AU";
			contactExistsInCWButDeletedInUMP.OC_Gender = "N";
			contactExistsInCWButDeletedInUMP.OC_Language = "EN-au";
			contactExistsInCWButDeletedInUMP.OC_Title = "User";
			contactExistsInCWButDeletedInUMP.OC_Phone = "555";
			contactExistsInCWButDeletedInUMP.OC_PhoneExtension = "669";
			contactExistsInCWButDeletedInUMP.OC_Mobile = "04567890";
			contactExistsInCWButDeletedInUMP.OC_Birthday = ZDateTime.BrettsBirthday;
			contactExistsInCWButDeletedInUMP.OC_PasswordHash = new byte[] { 5, 55 };
			contactExistsInCWButDeletedInUMP.OC_PasswordSalt = new byte[] { 6, 66 };
			contactExistsInCWButDeletedInUMP.OC_PasswordHashIterations = 10000;

			Factory.Save();

			contact2_2.Person.PER_PasswordHash = new byte[] { 5, 55 };
			contact2_2.Person.PER_PasswordSalt = new byte[] { 6, 66 };
			contact2_2.Person.PER_PasswordHashIterations = 100_000;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "Zen Desk";
			org3.OH_FullName = "And some things that have not yet come to pass";
			org3.OH_Language = "EN-au";

			var address = org3.MainAddress;
			address.OA_Code = "AAA";
			address.OA_Address1 = "CompuGlobal Hyper Mega Net";
			address.OA_Address2 = "123 Fake St";
			address.OA_State = "VIC";
			address.OA_PostCode = "3001";
			address.OA_Phone = "555";
			address.OA_Email = "Email@emails.co.nz";
			address.OA_City = "Fapple";
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Language = "EN-au";

			Factory.Save();

			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "MasOrg";
			org4.OH_FullName = "Master org testing";
			org4.OH_Language = "EN-au";

			var org5 = Factory.New<OrgHeader>();
			org5.OH_Code = "MasterOrg2";
			org5.OH_FullName = "Master org same as self org";
			org5.OH_Language = "EN-au";

			var org6 = Factory.New<OrgHeader>();
			org6.OH_Code = "MasterOrg3";
			org6.OH_FullName = "Master org is deleted";
			org6.OH_Language = "EN-au";

			var org7 = Factory.New<OrgHeader>();
			org7.OH_Code = "MasterOrg4";
			org7.OH_FullName = "Master org same as self org 2";
			org7.OH_Language = "EN-au";

			Factory.Save();

			CreateMasterOrg(org4.PK, org3.PK);
			CreateMasterOrg(org5.PK, org5.PK);
			CreateMasterOrg(org6.PK, default, false);
			CreateMasterOrg(org7.PK, org7.PK);

			InsertOrgHeaderToUMPDatabase(umpConnection, orgPK1, Guid.NewGuid());
			InsertContact(contactPK1_1, Guid.NewGuid(), orgPK1);
			InsertContact(contactPK1_2, Guid.NewGuid(), orgPK1);
			InsertContact(contactPK2, Guid.NewGuid(), orgPK1, isDeleted: true);

			InsertOrgHeaderToUMPDatabase(umpConnection, orgPK2, org2.PK.ToGuid(), hasAccess: true, isActive: false);
			InsertContact(contactPK3_1, contact2_1.PK.ToGuid(), orgPK2, isActive: false, isDeleted: false, hasAccess: false,
				"Zaphod Beeblebrox", "EN-us", "Nyah", "No", "M", "qwe", "rty", "uiop", new ZDateTime(2015, 7, 14), "davey@things.co.uk", "0x111", "0x222", 28);
			InsertContact(contactPK3_2, contact2_2.PK.ToGuid(), orgPK2, isActive: false, isDeleted: false, hasAccess: false,
				"Zaphod Beeblebrox", "EN-us", "Nyah", "No", "M", "qwe", "rty", "uiop", new ZDateTime(2015, 7, 14), "davey@things.co.uk", "0x111", "0x222", 28);
			InsertContact(contactExistsInCWButDeletedInUmpPk, contactExistsInCWButDeletedInUMP.PK.ToGuid(), orgPK2, isDeleted: true);

			InsertOrgHeaderToUMPDatabase(umpConnection, orgPK3, org3.PK.ToGuid(), code: "ABC", name: "One Two Three", language: "EN-us");

			InsertOrgHeaderToUMPDatabase(umpConnection, orgPK4, org4.PK.ToGuid(), code: "MasOrg", name: "Master org testing", language: "EN-au", masterOrgPk: Guid.NewGuid());

			InsertOrgHeaderToUMPDatabase(umpConnection, orgPk_SelfAsMasterOrg, org5.PK.ToGuid(), code: "MasterOrg2", name: "Master org same as self org", language: "EN-au", masterOrgPk: Guid.NewGuid());

			InsertOrgHeaderToUMPDatabase(umpConnection, orgPk_MasterOrgDeleted, org6.PK.ToGuid(), code: "MasterOrg3", name: "Master org is deleted", language: "EN-au", masterOrgPk: Guid.NewGuid());

			InsertOrgHeaderToUMPDatabase(umpConnection, orgPk_SelfAsMasterOrg2, org7.PK.ToGuid(), code: "MasterOrg4", name: "Master org same as self org 2", language: "EN-au", masterOrgPk: null);

			InsertAddress(addressPK, address.PK.ToGuid(), orgPK2, false);

			void InsertContact(Guid pk, Guid ediProdPK, Guid orgPK, bool isActive = true, bool isDeleted = false, bool hasAccess = true,
	string contactName = "", string language = "", string title = "", string nationality = "", string gender = "O", string phone = "", string phoneExtension = "",
	string mobile = "", ZDateTime birthday = default, string email = "",
	string passwordHash = "0x", string passwordSalt = "0x", int passwordHashIterations = 69)
			{
				var insertSql = FormattableString.Invariant($@"
INSERT dbo.OrgContact (
	OC_PK,
	EdiProdRecordId,
	OC_OH,
	OC_IsActive,
	OC_IsDeleted,
	OC_HasAccess,
	OC_ContactName,
	OC_Language,
	OC_Title,
	OC_RN_NKNationality,
	OC_Gender,
	OC_Phone,
	OC_PhoneExtension,
	OC_Mobile,
	OC_Birthday,
	OC_Email,
	OC_PasswordHash,
	OC_PasswordSalt,
	OC_PasswordHashIterations
) VALUES (
	'{pk}',
	'{ediProdPK}',
	'{orgPK}',
	{(isActive ? 1 : 0)},
	{(isDeleted ? 1 : 0)},
	{(hasAccess ? 1 : 0)},
	'{contactName}',
	'{language}',
	'{title}',
	'{nationality}',
	'{gender}',
	'{phone}',
	'{phoneExtension}',
	'{mobile}',
	'{birthday.SqlFormat}',
	'{email}',
	{passwordHash},
	{passwordSalt},
	{passwordHashIterations}
)
");

				umpConnection.ExecuteNonQuery(insertSql);
			}
			void InsertAddress(Guid pk, Guid ediProdPK, Guid orgPK, bool isActive, string code = "", string address1 = "", string address2 = "", string state = "", string postCode = "",
				string phone = "", string email = "", string city = "", string countryCode = "", string language = "")
			{
				var insertSql = FormattableString.Invariant($@"
INSERT dbo.OrgAddress (
	OA_PK,
	EdiProdRecordId,
	OA_OH,
	OA_IsActive,
	OA_Code,
	OA_Address1,
	OA_Address2,
	OA_State,
	OA_PostCode,
	OA_Phone,
	OA_Email,
	OA_City,
	OA_CountryCode,
	OA_Language
) VALUES (
	'{pk}',
	'{ediProdPK}',
	'{orgPK}',
	{(isActive ? 1 : 0)},
	'{code}',
	'{address1}',
	'{address2}',
	'{state}',
	'{postCode}',
	'{phone}',
	'{email}',
	'{city}',
	'{countryCode}',
	'{language}'
)
");

				umpConnection.ExecuteNonQuery(insertSql);
			}
		}

		void InsertOrgHeaderToUMPDatabase(DbConnection umpConnection, Guid pk, Guid ediProdPK, bool hasAccess = true, bool isActive = true, string code = "", string name = "", string language = "", Guid? masterOrgPk = default)
		{
			var escapedName = name.Replace("'", "''");
			var insertSql = FormattableString.Invariant($@"
INSERT dbo.OrgHeader (
	OH_PK,
	EdiProdRecordId,
	OH_HasAccess,
	OH_IsActive,
	OH_Code,
	OH_FullName,
	OH_Language,
	OH_MasterOrgPk
) VALUES (
	'{pk}',
	'{ediProdPK}',
	{(hasAccess ? 1 : 0)},
	{(isActive ? 1 : 0)},
	'{code}',
	'{escapedName}',
	'{language}',");

			insertSql += masterOrgPk.HasValue ? "'" + masterOrgPk.Value + "'" : "null";
			insertSql += ")";
			umpConnection.ExecuteNonQuery(insertSql);
		}

		void CreateMasterOrg(ZGuid ediOrgPk, ZGuid masterOrgPk, bool isActiveLicenseDb = true)
		{
			var query = new ZQuery(OrgHeaderSchema.PK, ediOrgPk);
			var ediOrgHeader = Factory.Load<EDIOrgHeader>(query).First();

			ediOrgHeader.CreateAndLoadLicenceForOrg();

			var licenceDB = ediOrgHeader.LicCompany.LicDatabases.AddNew();
			licenceDB.LD_Product = ProductTypes.Codes.BorderWise;
			licenceDB.LD_ServerCode = ProductTypes.Codes.BorderWise;
			licenceDB.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			licenceDB.LD_OH_WebAccessOrg = masterOrgPk;
			licenceDB.LD_IsActive = isActiveLicenseDb;
			licenceDB.LD_ServerCode = "BOR";
			licenceDB.LD_Status = DatabaseStatusList.Codes.REG;
			licenceDB.LD_LicenceType = DatabaseTypes.Codes.Production;

			Factory.Save();
		}

		void ProcessActiveOrgsAndContacts(DbConnection umpConnection, bool shouldSyncToUmp = false)
		{
			var activeOrgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			activeOrgHeaderQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			var activeOrgs = Factory.Load<OrgHeader>(activeOrgHeaderQuery);
			foreach (var org in activeOrgs)
			{
				activeNewOrgPks.Add(org.PK.ToGuid());
			}

			var activeOrgContactQuery = new ZDBOnlyQuery(typeof(OrgContact));
			activeOrgContactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			var activeContacts = Factory.Load<OrgContact>(activeOrgContactQuery);
			foreach (var contact in activeContacts)
			{
				activeNewContactPks.Add(contact.PK.ToGuid());
			}

			if (shouldSyncToUmp)
			{
				foreach (var org in activeOrgs)
				{
					org.OH_IsActive = false;
				}

				foreach (var contact in activeContacts)
				{
					contact.OC_IsActive = false;
				}

				Factory.Save();
			}
		}

		#endregion

		#region Assertions

		void AssertUpdatedData(DbConnection umpConnection)
		{
			AssertContact("Should disable non-deleted contact that can't be found in ediProd", contactPK1_1, false, true, true);
			AssertContact("Should disable non-deleted contact that can't be found in ediProd", contactPK1_2, false, true, true);
			AssertContact("Should leave deleted contact even though it can't be found in ediProd", contactPK2, true, true, true);
			AssertContact("Should update contact from equivalent in ediProd", contactPK3_1, true, false, true,
				"Davey", "EN-au", "Flightmaster General", "DE", 'N', "555", "669", "04567890", ZDateTime.BrettsBirthday, "daveybaby@daveeast.com", "0321", "042C", 200000);
			AssertContact("Should update contact from equivalent in ediProd (and consider GlbPerson for password)", contactPK3_2, true, false, true,
				"Wavey", "EN-au", "Flightmaster General", "DE", 'N', "555", "669", "04567890", ZDateTime.BrettsBirthday, "daveywavey@daveeast.com", "0537", "0642", 100000);
			AssertContact("Should update contact make it not deleted as it exists in ediProd", contactExistsInCWButDeletedInUmpPk, false, false, false,
				"Existing contact", "EN-au", "User", "AU", 'N', "555", "669", "04567890", ZDateTime.BrettsBirthday, "notdeleted@zhou.com", "0537", "0642", 10000);

			AssertOrganisation("Should disable organisation that can't be found in ediProd", orgPK1, false);
			AssertOrganisation("Should update organisation from equivalent in ediProd", orgPK2, true, "POCKETSES", "What has it got in its pocketses?", "EN-me");
			AssertOrganisation("Should update organisation from equivalent in ediProd", orgPK3, true, "Zen Desk", "And some things that have not yet come to pass", "EN-au");

			AssertAddress("Should update address from equivalent in ediProd", addressPK, orgPK3, true, "CompuGlobal Hyper Mega Ne", "CompuGlobal Hyper Mega Net", "123 Fake St", "VIC", "3001", "555", "Email@emails.co.nz", "Fapple", "AU", "EN-au");

			void AssertContact(string message, Guid pk, bool isActive, bool isDeleted, bool hasAccess,
				string contactName = "", string language = "", string title = "", string nationality = "", char gender = 'O', string phone = "", string phoneExtension = "",
				string mobile = "", ZDateTime birthday = default, string email = "",
				string passwordHash = "", string passwordSalt = "", int passwordHashIterations = 69)
			{
				var selectSql = FormattableString.Invariant($@"
SELECT TOP 1
	OC_IsActive,
	OC_IsDeleted,
	OC_HasAccess,
	OC_ContactName,
	OC_Language,
	OC_Title,
	OC_RN_NKNationality,
	OC_Gender,
	OC_Phone,
	OC_PhoneExtension,
	OC_Mobile,
	OC_Birthday,
	OC_Email,
	OC_PasswordHash,
	OC_PasswordSalt,
	OC_PasswordHashIterations
FROM dbo.OrgContact
WHERE OC_PK = '{pk}'
");

				using (var reader = umpConnection.Command(selectSql).ExecuteReader())
				{
					Assert(reader.Read());

					CombineAssertions(message, () =>
					{
						AssertEquals("OC_IsActive", isActive, reader.GetBoolean(0));
						AssertEquals("OC_IsDeleted", isDeleted, reader.GetBoolean(1));
						AssertEquals("OC_HasAccess", hasAccess, reader.GetBoolean(2));
						AssertEquals("OC_ContactName", contactName, reader.GetString(3));
						AssertEquals("OC_Language", language, reader.GetString(4));
						AssertEquals("OC_Title", title, reader.GetString(5));
						AssertEquals("OC_RN_NKNationality", nationality, reader.GetString(6));
						AssertEquals("OC_Gender", gender.ToString(), reader.GetString(7));
						AssertEquals("OC_Phone", phone, reader.GetString(8));
						AssertEquals("OC_PhoneExtension", phoneExtension, reader.GetString(9));
						AssertEquals("OC_Mobile", mobile, reader.GetString(10));
						AssertEquals("OC_Birthday", birthday, reader.GetDateTime(11));
						AssertEquals("OC_Email", email, reader.GetString(12));
						AssertEquals("OC_PasswordHash", passwordHash, ToString((byte[])reader.GetValue(13)));
						AssertEquals("OC_PasswordSalt", passwordSalt, ToString((byte[])reader.GetValue(14)));
						AssertEquals("OC_PasswordHashIterations", passwordHashIterations, reader.GetInt32(15));
					});
				}

				string ToString(byte[] bytes)
				{
					return BitConverter.ToString(bytes).Replace("-", "");
				}
			}

			void AssertOrganisation(string message, Guid pk, bool isActive, string code = "", string name = "", string language = "")
			{
				var selectSql = FormattableString.Invariant($@"
SELECT TOP 1
	OH_IsActive,
	OH_Code,
	OH_FullName,
	OH_Language
FROM dbo.OrgHeader
WHERE OH_PK = '{pk}'
");

				using (var reader = umpConnection.Command(selectSql).ExecuteReader())
				{
					Assert(reader.Read());

					CombineAssertions(message, () =>
					{
						AssertEquals("OH_IsActive", isActive, reader.GetBoolean(0));
						AssertEquals("OH_Code", code, reader.GetString(1));
						AssertEquals("OH_FullName", name, reader.GetString(2));
						AssertEquals("OH_Language", language, reader.GetString(3));
					});
				}
			}

			void AssertAddress(string message, Guid pk, Guid orgPK, bool isActive, string code = "", string address1 = "", string address2 = "", string state = "", string postCode = "",
				string phone = "", string email = "", string city = "", string countryCode = "", string language = "")
			{
				var selectSql = FormattableString.Invariant($@"
SELECT TOP 1
	OA_OH,
	OA_IsActive,
	OA_Code,
	OA_Address1,
	OA_Address2,
	OA_State,
	OA_PostCode,
	OA_Phone,
	OA_Email,
	OA_City,
	OA_CountryCode,
	OA_Language
FROM dbo.OrgAddress
WHERE OA_PK = '{pk}'
");

				using (var reader = umpConnection.Command(selectSql).ExecuteReader())
				{
					Assert(reader.Read());

					CombineAssertions(message, () =>
					{
						AssertEquals("OA_OH", orgPK, reader.GetGuid(0));
						AssertEquals("OA_IsActive", isActive, reader.GetBoolean(1));
						AssertEquals("OA_Code", code, reader.GetString(2));
						AssertEquals("OA_Address1", address1, reader.GetString(3));
						AssertEquals("OA_Address2", address2, reader.GetString(4));
						AssertEquals("OA_State", state, reader.GetString(5));
						AssertEquals("OA_PostCode", postCode, reader.GetString(6));
						AssertEquals("OA_Phone", phone, reader.GetString(7));
						AssertEquals("OA_Email", email, reader.GetString(8));
						AssertEquals("OA_City", city, reader.GetString(9));
						AssertEquals("OA_CountryCode", countryCode, reader.GetString(10));
						AssertEquals("OA_Language", language, reader.GetString(11));
					});
				}
			}
		}

		void AssertPerformanceLog(string serviceLog)
		{
			Enum.GetNames(typeof(EntitySyncEvents)).ForEach(operation =>
			{
				AssertContains($"Operation {operation} took", serviceLog);
			});
		}
		#endregion
	}
}
