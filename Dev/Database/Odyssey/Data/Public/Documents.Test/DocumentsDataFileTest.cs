using System;
using System.Data;
using System.IO;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	class DocumentsDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestDataFileExists()
		{
			var dataFile = new DocumentsDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public const string InsertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'SysTemplate', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('7D6292B9-B4B3-4D51-A43C-88E66981F356', 'UserTemplate', 'Consol', null, 0, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('A17A23AF-4FD1-4B9D-BEB1-D8A7D21D9F2C', 'ClientTemplate', 'Consol', null, 1, 1, null, '');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'Sys Doc', 'Consol', '', 1, 1, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('42BF5EA6-B821-4A14-8758-DAF5894670AE', 'User Doc', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('B1347F4B-1AA3-4DBE-9D17-DA504F1E8312', 'User Doc With Sys Template', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('3064323F-2EA4-4F3F-80B1-A83C901CFD43', 'Client Doc', 'Consol', '', 1, 1, 1, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('4BB1F035-639F-4513-A888-B5E9B89CA5E5', 'Client Doc With Sys Template', 'Consol', '', 1, 1, 1, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('6EF40688-A338-4FCA-8ABC-D7B293E5ACCC', 'Sys Doc Pack', 'Consol', '', 1, 1, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('1F1D03F3-8793-4179-916C-0FD192AC76C7', 'Client Doc Pack', 'Consol', '', 1, 1, 1, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('2AB901B7-6BD5-470B-AC32-2561B2C09EC2', 'Client Doc Pack With Sys Doc', 'Consol', '', 1, 1, 1, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortcut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('F396D7BA-B393-4AA0-8AD4-3E5D6C6E83CB', 'User Doc Pack', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Inward, SF_SU_Outward, SF_Index, SF_IsSystemDefined, SF_IsClientSpecific) VALUES('16C6C292-CF16-417E-BF30-AED769CBD9DB', '6EF40688-A338-4FCA-8ABC-D7B293E5ACCC', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 1, 1, 0);
				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Inward, SF_SU_Outward, SF_Index, SF_IsSystemDefined, SF_IsClientSpecific) VALUES('5B637D92-26FB-4481-846F-5C8D73BACC8A', '1F1D03F3-8793-4179-916C-0FD192AC76C7', '3064323F-2EA4-4F3F-80B1-A83C901CFD43', 2, 1, 1);
				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Inward, SF_SU_Outward, SF_Index, SF_IsSystemDefined, SF_IsClientSpecific) VALUES('3D9F4CA5-570C-4EA5-A5F4-7B96F31099CC', '2AB901B7-6BD5-470B-AC32-2561B2C09EC2', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 3, 1, 1);
				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Inward, SF_SU_Outward, SF_Index, SF_IsSystemDefined, SF_IsClientSpecific) VALUES('6A8AD397-20EF-4420-B498-E2B71DE22196', 'F396D7BA-B393-4AA0-8AD4-3E5D6C6E83CB', '42BF5EA6-B821-4A14-8758-DAF5894670AE', 4, 0, 0);

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('6D55F313-5341-4988-A821-18820CD116B8', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'A17A23AF-4FD1-4B9D-BEB1-D8A7D21D9F2C', 'Client Pivot With ClientTemp & SysMenu', 1, 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('C80D29AC-14AA-4C81-AE47-06FBE42696D5', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'Client Pivot With SysTemp & SysMenu', 1, 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('5F39AB23-2CB0-466E-BF65-80BBC66741BB', '4BB1F035-639F-4513-A888-B5E9B89CA5E5', '59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'Client Pivot With Sys Template', 1, 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('B0E2C7A6-BA5D-4F47-AD41-F08281632054', '3064323F-2EA4-4F3F-80B1-A83C901CFD43', 'A17A23AF-4FD1-4B9D-BEB1-D8A7D21D9F2C', 'Client Pivot', 1, 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('D609D2BC-A8A3-4485-8E65-51D2B6E329F1', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'Sys Pivot', 1, 0);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('3E76B20C-12D2-47A6-911C-4FA02FEB298A', 'B1347F4B-1AA3-4DBE-9D17-DA504F1E8312', '59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'User Doc With Sys Template', 0, 0);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('A2EABA76-C79A-4B8B-93FC-F3ABBA104E4D', '42BF5EA6-B821-4A14-8758-DAF5894670AE', '7D6292B9-B4B3-4D51-A43C-88E66981F356', 'User Pivot', 0, 0);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('AB9FF15B-B889-4353-AF8C-7630E42B6096', '42BF5EA6-B821-4A14-8758-DAF5894670AE', '7D6292B9-B4B3-4D51-A43C-88E66981F356', 'Non System Client Specific', 0, 1);

				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('A5041175-1C66-4DCA-9162-F97C72E7B423', 'CON', 'ABC', 'System DocType', 1, 1);
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('EDF5D015-F51C-4C40-8CFB-BAC60234F6FA', 'CON', 'XYZ', 'Another System DocType', 1, 1);
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('9AA946FE-766B-45E5-A209-F413B8BE6844', 'CON', 'JKL', 'Non-system DocType', 1, 0);

				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined, SX_IsClientSupressed) VALUES ('288490DC-BD97-44BE-9387-C92556CA030F', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'A5041175-1C66-4DCA-9162-F97C72E7B423', 1, 0);
				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined, SX_IsClientSupressed) VALUES ('15BC2B76-D332-4221-B4D1-C1EA037E136A', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'EDF5D015-F51C-4C40-8CFB-BAC60234F6FA', 1, 1);
				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined, SX_IsClientSupressed) VALUES ('6B168000-C270-4828-870A-617AE7DB8FC6', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '9AA946FE-766B-45E5-A209-F413B8BE6844', 0, 1);

				INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_IsClientSpecific, S3_SI, S3_Description) VALUES ('AC5C202F-9F26-4ce3-BC42-CBF7F4384348', 1, 1, '6D55F313-5341-4988-A821-18820CD116B8', 'Default');
				INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_IsClientSpecific, S3_SI, S3_Description) VALUES ('5918C5B8-A9B6-4ea8-B4BB-77F76454AE0E', 1, 0, 'D609D2BC-A8A3-4485-8E65-51D2B6E329F1', 'Default');
				INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_IsClientSpecific, S3_SI, S3_Description) VALUES ('7425809B-77E0-42E2-94FE-7B7EEE348EAB', 0, 1, 'AB9FF15B-B889-4353-AF8C-7630E42B6096', 'Default');
				INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_IsClientSpecific, S3_SI, S3_Description) VALUES ('BEB0AABA-FD3E-4278-AB73-48FF81170970', 0, 0, 'A2EABA76-C79A-4B8B-93FC-F3ABBA104E4D', 'Default');

				INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3, S4_IsSystemDefined, S4_IsClientSpecific) VALUES ('61B8CD6C-E8FB-4b7c-89D1-4F1E3DA38664', 'AC5C202F-9F26-4ce3-BC42-CBF7F4384348', 1, 1);
				INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3, S4_IsSystemDefined, S4_IsClientSpecific) VALUES ('0806867D-6429-410f-896C-A79D9930A0E3', '5918C5B8-A9B6-4ea8-B4BB-77F76454AE0E', 1, 0);
				INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3, S4_IsSystemDefined, S4_IsClientSpecific) VALUES ('60B712F9-9448-46DD-AED8-932B448C4308', '7425809B-77E0-42E2-94FE-7B7EEE348EAB', 0, 1);
				INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3, S4_IsSystemDefined, S4_IsClientSpecific) VALUES ('B61761EF-BC81-49DC-9CA0-B749B4BA6FE4', 'BEB0AABA-FD3E-4278-AB73-48FF81170970', 0, 0);

				INSERT dbo.RateAttachmentSet(TS_PK, TS_SU, TS_AttachmentName, TS_IsSystemDefined, TS_IsClientSpecific, TS_SystemLastEditTimeUtc, TS_SystemLastEditUser, TS_SystemCreateTimeUtc, TS_SystemCreateUser) VALUES ('22BB5C6A-CC32-4d7c-9862-CC8772CE1B36', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'System Attachment', 1, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.RateAttachmentSet(TS_PK, TS_SU, TS_AttachmentName, TS_IsSystemDefined, TS_IsClientSpecific, TS_SystemLastEditTimeUtc, TS_SystemLastEditUser, TS_SystemCreateTimeUtc, TS_SystemCreateUser) VALUES ('F7C96E8A-92C8-4399-B0B7-F535008118CE', '3064323F-2EA4-4F3F-80B1-A83C901CFD43', 'Client Attachment', 1, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.RateAttachmentSet(TS_PK, TS_SU, TS_AttachmentName, TS_IsSystemDefined, TS_IsClientSpecific, TS_SystemLastEditTimeUtc, TS_SystemLastEditUser, TS_SystemCreateTimeUtc, TS_SystemCreateUser) VALUES ('39076B8A-B8AD-453c-88E0-FB681D49D2E2', '42BF5EA6-B821-4A14-8758-DAF5894670AE', 'User Attachment', 0, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.RateAttachmentSet(TS_PK, TS_SU, TS_AttachmentName, TS_IsSystemDefined, TS_IsClientSpecific, TS_SystemLastEditTimeUtc, TS_SystemLastEditUser, TS_SystemCreateTimeUtc, TS_SystemCreateUser) VALUES ('6C23E0E6-814E-43ff-B520-50D2EFD5FAD9', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'User => System U', 0, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.RatingHeader(TH_PK, TH_RateType, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES ('6214718E-8AB6-4C04-B836-BF4D5F8677CC', 'COS', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.RateAttachment(TA_PK, TA_TH, TA_TS, TA_SystemLastEditTimeUtc, TA_SystemLastEditUser, TA_SystemCreateTimeUtc, TA_SystemCreateUser) VALUES ('A6807F21-C1B8-4741-B749-CE61A272EEA5', '6214718E-8AB6-4C04-B836-BF4D5F8677CC', '22BB5C6A-CC32-4d7c-9862-CC8772CE1B36', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				";

		protected virtual DocumentsDataFile GetDocumentsDataFile()
		{
			return new DocumentsDataFile();
		}

		public void TestDocumentsDataFile()
		{
			Db.Connection.ExecuteNonQuery(InsertSql);

			DocumentsDataFile docFile = GetDocumentsDataFile();
			var data = docFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 9, data.Tables.Count);

			AssertEquals("RateAttachmentSet", 1, data.Tables["RateAttachmentSet"].Rows.Count);
			AssertEquals("System Attachment", data.Tables["RateAttachmentSet"].Rows[0]["TS_AttachmentName"].ToString());

			AssertEquals("StmMenuItem row count", 2, data.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("Menu Name", "Sys Doc", data.Tables["StmMenuItem"].Rows[0]["SU_MenuName"].ToString());
			AssertEquals("Menu Name", "Sys Doc Pack", data.Tables["StmMenuItem"].Rows[1]["SU_MenuName"].ToString());

			AssertEquals("StmTemplate row count", 1, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("Template1 Name", "SysTemplate", data.Tables["StmTemplate"].Rows[0]["SO_Name"].ToString());

			AssertEquals("StmMenuTemplatePivot row count", 1, data.Tables["StmMenuTemplatePivot"].Rows.Count);
			AssertEquals("Pivot Title", "Sys Pivot", data.Tables["StmMenuTemplatePivot"].Rows[0]["SI_DocumentTitle"].ToString());

			AssertEquals("StmMenuMenuPivot row count", 1, data.Tables["StmMenuMenuPivot"].Rows.Count);
			AssertEquals("Menu Pivot Index", (Int16)1, data.Tables["StmMenuMenuPivot"].Rows[0]["SF_Index"]);

			AssertEquals("StmMenuEDocs row count", 2, data.Tables["StmMenuEDocs"].Rows.Count);
			AssertEquals("Menu's DocType", new Guid("EDF5D015-F51C-4C40-8CFB-BAC60234F6FA"), data.Tables["StmMenuEDocs"].Rows[0]["SX_RT_DocType"]);
			AssertEquals("Menu's DocType", new Guid("A5041175-1C66-4DCA-9162-F97C72E7B423"), data.Tables["StmMenuEDocs"].Rows[1]["SX_RT_DocType"]);

			AssertEquals("RefDocType row count - user rows should not be loaded", 2, data.Tables["RefDocType"].Rows.Count);
			AssertEquals("RefDocType's doc type ", "XYZ", data.Tables["RefDocType"].Rows[0]["RT_DocType"]);
			AssertEquals("RefDocType's doc type ", "ABC", data.Tables["RefDocType"].Rows[1]["RT_DocType"]);

			AssertEquals("StmMenuDocumentConfig row count", 1, data.Tables["StmMenuDocumentConfig"].Rows.Count);
			AssertEquals("StmMenuDocumentConfig's PK", new Guid("5918C5B8-A9B6-4ea8-B4BB-77F76454AE0E"), data.Tables["StmMenuDocumentConfig"].Rows[0]["S3_PK"]);

			AssertEquals("StmMenuDocumentConfigItem row count", 1, data.Tables["StmMenuDocumentConfigItem"].Rows.Count);
			AssertEquals("StmMenuDocumentConfigItem's PK", new Guid("0806867D-6429-410f-896C-A79D9930A0E3"), data.Tables["StmMenuDocumentConfigItem"].Rows[0]["S4_PK"]);
		}

		public void TestRateAttachmentsNotBeingDeleted()
		{
			Db.Connection.ExecuteNonQuery(InsertSql);

			DocumentsDataFile docFile = GetDocumentsDataFile();
			var data = docFile.LoadDataFromDatabase();

			int count = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.RateAttachment WHERE TA_PK = 'A6807F21-C1B8-4741-B749-CE61A272EEA5'");

			AssertEquals("Precondition: RateAttachment with PK: A6807F21-C1B8-4741-B749-CE61A272EEA5 exists", 1, count);

			var dataFile = new DocumentsDataFile();

			AssertNoExceptionThrown((dataFile as IFixReferencesAndDuplicates).PerformExtraDataManipulationBeforeEnablingConstraints);

			count = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.RateAttachment WHERE TA_PK = 'A6807F21-C1B8-4741-B749-CE61A272EEA5'");

			AssertEquals("RateAttachment with PK: A6807F21-C1B8-4741-B749-CE61A272EEA5 still exists", 1, count);
		}

		protected virtual string TemplateBolbExpected()
		{
			return string.Empty;
		}

		public void TestSavingTemplateBlobsCorrectly()
		{
			string script = InsertSql + " UPDATE dbo.StmTemplate SET SO_Template = cast('Binary data' as varbinary(max))";
			Db.Connection.ExecuteNonQuery(script);

			DocumentsDataFile docFile = GetDocumentsDataFile();
			DataSet data = docFile.LoadDataFromDatabase();

			AssertEquals("The SO_Template column should be NULL", TemplateBolbExpected(), data.Tables["StmTemplate"].Rows[0]["SO_Template"].ToString());
		}

		public void TestRowsAreReturnedInTheSameOrder()
		{
			DocumentTablesCleaner.Clean();

			string sqlText = @"
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES ('292277AC-7D16-4577-AE87-C41983A4138D', 'TestMenu1', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES ('51078B3A-4767-45B6-9C95-25D7BD8AB994', 'TestMenu2', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE9F', 'TestMenu3', 1)

			insert into dbo.StmTemplate (SO_PK, SO_Name, SO_IsSystemDefined) values ('410D8F73-2DF8-4E61-9687-68C0DE977CB9', 'TestTemplate1', 1)
			insert into dbo.StmTemplate (SO_PK, SO_Name, SO_IsSystemDefined) values ('80052AD4-51FB-4E38-9C9E-40A7A830D73A', 'TestTemplate2', 1)
			insert into dbo.StmTemplate (SO_PK, SO_Name, SO_IsSystemDefined) values ('5F843B5F-A539-4A67-A9A5-6D148242FD93', 'TestTemplate3', 1)

			insert into dbo.StmMenuTemplatePivot (SI_PK, SI_SO, SI_SU, SI_DocumentTItle, SI_IsSystemDefined) values ('4672DCBF-F721-4347-B37F-6C0EBA662868', '410D8F73-2DF8-4E61-9687-68C0DE977CB9', '292277AC-7D16-4577-AE87-C41983A4138D', 'TestPivot1', 1)
			insert into dbo.StmMenuTemplatePivot (SI_PK, SI_SO, SI_SU, SI_DocumentTItle, SI_IsSystemDefined) values ('29EFD561-FD01-40E7-AEAB-E4EDF9FB5874', '80052AD4-51FB-4E38-9C9E-40A7A830D73A', '51078B3A-4767-45B6-9C95-25D7BD8AB994', 'TestPivot2', 1)
			insert into dbo.StmMenuTemplatePivot (SI_PK, SI_SO, SI_SU, SI_DocumentTItle, SI_IsSystemDefined) values ('5389CFE6-F439-483C-8010-2923D370AEFC', '5F843B5F-A539-4A67-A9A5-6D148242FD93', '5692FF9B-FE58-4805-AB7F-297700BABE9F', 'TestPivot3', 1)

			insert into dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined) values ('DCD26ECB-0CCA-4BFF-A9C6-DBECAFD162CD', '292277AC-7D16-4577-AE87-C41983A4138D', '51078B3A-4767-45B6-9C95-25D7BD8AB994', 1)
			insert into dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined) values ('20927BCD-58FD-40FA-B575-1ABC4ABCEF30', '51078B3A-4767-45B6-9C95-25D7BD8AB994', '5692FF9B-FE58-4805-AB7F-297700BABE9F', 1)

			insert into dbo.RefDocType (RT_PK, RT_DocType, RT_IsSystem, RT_ReferenceType) VALUES ('46875A76-CE6B-4F68-B7ED-62004D501D08', 'ABC', 1, 'ALL')
			insert into dbo.RefDocType (RT_PK, RT_DocType, RT_IsSystem, RT_ReferenceType) VALUES ('B1CB30A3-0646-4FE3-82C0-D5823130304B', 'DEF', 1, 'ALL')

			insert into dbo.StmMenuEDocs (SX_PK, SX_RT_DocType, SX_SU, SX_IsSystemDefined) VALUES ('1E70D678-0D17-4C2F-A69D-CAD88B33AC95', '46875A76-CE6B-4F68-B7ED-62004D501D08', '292277AC-7D16-4577-AE87-C41983A4138D', 1)
			insert into dbo.StmMenuEDocs (SX_PK, SX_RT_DocType, SX_SU, SX_IsSystemDefined) VALUES ('8B2A2052-0CC6-4C5E-9FC6-9A6A98204BAA', 'B1CB30A3-0646-4FE3-82C0-D5823130304B', '292277AC-7D16-4577-AE87-C41983A4138D', 1)
			insert into dbo.StmMenuEDocs (SX_PK, SX_RT_DocType, SX_SU, SX_IsSystemDefined) VALUES ('9C0E8F9D-FC10-4DC4-9A44-E4A8AE1B5E6C', 'B1CB30A3-0646-4FE3-82C0-D5823130304B', '51078B3A-4767-45B6-9C95-25D7BD8AB994', 1)";

			Db.Connection.ExecuteNonQuery(sqlText);

			DocumentsDataFile dataFile = GetDocumentsDataFile();
			DataSet dS = dataFile.LoadDataFromDatabase();

			AssertEquals("Precondition: table data", 3, dS.Tables[StmMenuItemSchema.Constants.TableName].Rows.Count);
			AssertEquals("Precondition: table data", 3, dS.Tables[StmTemplateSchema.Constants.TableName].Rows.Count);
			AssertEquals("Precondition: table data", 3, dS.Tables[StmMenuTemplatePivotSchema.Constants.TableName].Rows.Count);
			AssertEquals("Precondition: table data", 2, dS.Tables[StmMenuMenuPivotSchema.Constants.TableName].Rows.Count);
			AssertEquals("Precondition: table data", 3, dS.Tables[StmMenuEDocsSchema.Constants.TableName].Rows.Count);
			AssertEquals("Precondition: table data", 2, dS.Tables[RefDocTypeSchema.Constants.TableName].Rows.Count);

			DataTable stmMenuItemTable = dS.Tables[StmMenuItemSchema.Constants.TableName];

			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuItemTable.TableName), new Guid("51078B3A-4767-45B6-9C95-25D7BD8AB994"), stmMenuItemTable.Rows[0][StmMenuItemSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuItemTable.TableName), new Guid("5692FF9B-FE58-4805-AB7F-297700BABE9F"), stmMenuItemTable.Rows[1][StmMenuItemSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuItemTable.TableName), new Guid("292277AC-7D16-4577-AE87-C41983A4138D"), stmMenuItemTable.Rows[2][StmMenuItemSchema.PK.Name]);

			DataTable stmTemplateTable = dS.Tables[StmTemplateSchema.Constants.TableName];
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmTemplateTable.TableName), new Guid("80052AD4-51FB-4E38-9C9E-40A7A830D73A"), stmTemplateTable.Rows[0][StmTemplateSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmTemplateTable.TableName), new Guid("410D8F73-2DF8-4E61-9687-68C0DE977CB9"), stmTemplateTable.Rows[1][StmTemplateSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmTemplateTable.TableName), new Guid("5f843b5f-a539-4a67-a9a5-6d148242fd93"), stmTemplateTable.Rows[2][StmTemplateSchema.PK.Name]);

			DataTable stmMenuTemplatePivotTable = dS.Tables[StmMenuTemplatePivotSchema.Constants.TableName];
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuTemplatePivotTable.TableName), new Guid("5389CFE6-F439-483C-8010-2923D370AEFC"), stmMenuTemplatePivotTable.Rows[0][StmMenuTemplatePivotSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuTemplatePivotTable.TableName), new Guid("4672DCBF-F721-4347-B37F-6C0EBA662868"), stmMenuTemplatePivotTable.Rows[1][StmMenuTemplatePivotSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuTemplatePivotTable.TableName), new Guid("29EFD561-FD01-40E7-AEAB-E4EDF9FB5874"), stmMenuTemplatePivotTable.Rows[2][StmMenuTemplatePivotSchema.PK.Name]);

			DataTable stmMenuMenuPivotTable = dS.Tables[StmMenuMenuPivotSchema.Constants.TableName];
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuMenuPivotTable.TableName), new Guid("20927bcd-58fd-40fa-b575-1abc4abcef30"), stmMenuMenuPivotTable.Rows[0][StmMenuMenuPivotSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuMenuPivotTable.TableName), new Guid("dcd26ecb-0cca-4bff-a9c6-dbecafd162cd"), stmMenuMenuPivotTable.Rows[1][StmMenuMenuPivotSchema.PK.Name]);

			DataTable refDocTypeTable = dS.Tables[RefDocTypeSchema.Constants.TableName];
			AssertEquals(String.Format("Order by PK should be preserved ({0})", refDocTypeTable.TableName), new Guid("46875a76-ce6b-4f68-b7ed-62004d501d08"), refDocTypeTable.Rows[0][RefDocTypeSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", refDocTypeTable.TableName), new Guid("b1cb30a3-0646-4fe3-82c0-d5823130304b"), refDocTypeTable.Rows[1][RefDocTypeSchema.PK.Name]);

			DataTable stmMenuEDocsTable = dS.Tables[StmMenuEDocsSchema.Constants.TableName];
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuEDocsTable.TableName), new Guid("8b2a2052-0cc6-4c5e-9fc6-9a6a98204baa"), stmMenuEDocsTable.Rows[0][StmMenuEDocsSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuEDocsTable.TableName), new Guid("1e70d678-0d17-4c2f-a69d-cad88b33ac95"), stmMenuEDocsTable.Rows[1][StmMenuEDocsSchema.PK.Name]);
			AssertEquals(String.Format("Order by PK should be preserved ({0})", stmMenuEDocsTable.TableName), new Guid("9c0e8f9d-fc10-4dc4-9a44-e4a8ae1b5e6c"), stmMenuEDocsTable.Rows[2][StmMenuEDocsSchema.PK.Name]);
		}

		public void TestEDocsProviderPlaceholdersAreIgnored()
		{
			DocumentTablesCleaner.Clean();

			Guid docTypePK1 = Guid.NewGuid();
			Guid docTypePK2 = Guid.NewGuid();
			Guid menuEDocsPK1 = Guid.NewGuid();
			Guid menuEDocsPK2 = Guid.NewGuid();
			Guid menuItemPK = Guid.NewGuid();

			string sqlText =
@"INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_FilterList) VALUES (@menuItemPK, 'Menu', 1, @filterList)
INSERT dbo.RefDocType (RT_PK, RT_DocType, RT_IsSystem, RT_ReferenceType) VALUES (@docTypePK1, 'ABC', 1, 'ALL')
INSERT dbo.RefDocType (RT_PK, RT_DocType, RT_IsSystem, RT_ReferenceType) VALUES (@docTypePK2, 'XYZ', 1, 'ALL')
INSERT dbo.StmMenuEDocs (SX_PK, SX_RT_DocType, SX_SU, SX_IsSystemDefined) VALUES (NEWID(), @docTypePK1, @menuItemPK, 0)
INSERT dbo.StmMenuEDocs (SX_PK, SX_RT_DocType, SX_SU, SX_IsSystemDefined) VALUES (NEWID(), @docTypePK2, @menuItemPK, 1)";

			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.AddParameter("@docTypePK1", SqlDbType.UniqueIdentifier, docTypePK1);
				command.AddParameter("@docTypePK2", SqlDbType.UniqueIdentifier, docTypePK2);
				command.AddParameter("@filterList", SqlDbType.VarChar, DocumentsUpgradeTask.EDocsProviderPlaceholderTag);
				command.AddParameter("@menuItemPK", SqlDbType.UniqueIdentifier, menuItemPK);
				command.ExecuteNonQuery();
			}

			DocumentsDataFile dataFile = GetDocumentsDataFile();
			using (DataSet dataSet = dataFile.LoadDataFromDatabase())
			{
				foreach (DataTable table in dataSet.Tables)
				{
					if (table.TableName == RefDocTypeSchema.Constants.TableName)
					{
						AssertEquals("There should be two rows in the RefDocType table.", 2, table.Rows.Count);
					}
					else
					{
						AssertEquals("There should not be any data in the " + table.TableName + " table.", 0, table.Rows.Count);
					}
				}
			}
		}

		#region Test TestPerformExtraDataManipulationBeforeEnablingConstraints

		public void TestDeleteOrphanClientSpecificPivots()
		{
			var dataFile = new DocumentsDataFile();

			AssertNoExceptionThrown("Precondition: no errors", (dataFile as IFixReferencesAndDuplicates).PerformExtraDataManipulationBeforeEnablingConstraints);

			Db.Connection.ExecuteNonQuery("alter table dbo.StmMenuTemplatePivot nocheck constraint all");
			Db.Connection.ExecuteNonQuery("insert into dbo.StmTemplate(SO_PK, SO_Name) values('47EBECF3-9118-4B04-BED9-C51EBAE76A9B', 'Test Template XXX')");
			Db.Connection.ExecuteNonQuery("insert into dbo.StmMenuTemplatePivot(SI_PK, SI_SU, SI_SO, SI_IsSystemDefined, SI_IsClientSpecific) values('16C359B9-0BA7-4AC2-B5F7-FA6970E4DE76', 'D9769A47-C586-4CBD-921D-238ABB637371', '47EBECF3-9118-4B04-BED9-C51EBAE76A9B', 0, 0)");

			AssertNoExceptionThrown("Should delete orphan non-system non-client pivots", (dataFile as IFixReferencesAndDuplicates).PerformExtraDataManipulationBeforeEnablingConstraints);

			Db.Connection.ExecuteNonQuery("insert into dbo.StmMenuTemplatePivot(SI_PK, SI_SU, SI_SO, SI_IsSystemDefined, SI_IsClientSpecific) values('16C359B9-0BA7-4AC2-B5F7-FA6970E4DE76', 'D9769A47-C586-4CBD-921D-238ABB637371', '47EBECF3-9118-4B04-BED9-C51EBAE76A9B', 0, 1)");

			AssertNoExceptionThrown("Should delete orphan non-system client pivots", (dataFile as IFixReferencesAndDuplicates).PerformExtraDataManipulationBeforeEnablingConstraints);

			Db.Connection.ExecuteNonQuery("insert into dbo.StmMenuTemplatePivot(SI_PK, SI_SU, SI_SO, SI_IsSystemDefined, SI_IsClientSpecific) values('16C359B9-0BA7-4AC2-B5F7-FA6970E4DE76', 'D9769A47-C586-4CBD-921D-238ABB637371', '47EBECF3-9118-4B04-BED9-C51EBAE76A9B', 1, 1)");

			AssertNoExceptionThrown("Should delete orphan system client pivots", (dataFile as IFixReferencesAndDuplicates).PerformExtraDataManipulationBeforeEnablingConstraints);

			Db.Connection.ExecuteNonQuery("insert into dbo.StmMenuTemplatePivot(SI_PK, SI_SU, SI_SO, SI_IsSystemDefined, SI_IsClientSpecific) values('16C359B9-0BA7-4AC2-B5F7-FA6970E4DE76', 'D9769A47-C586-4CBD-921D-238ABB637371', '47EBECF3-9118-4B04-BED9-C51EBAE76A9B', 1, 0)");

			AssertExceptionThrown("Should not delete orphan system non-client pivots, throw exception",
				typeof(ApplicationException),
				"Following StmMenuTeplatePivot(s) reference missing StmMenuItem(s): 16c359b9-0ba7-4ac2-b5f7-fa6970e4de76",
				(dataFile as IFixReferencesAndDuplicates).PerformExtraDataManipulationBeforeEnablingConstraints);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			DocumentTablesCleaner.Clean();
		}

		#endregion
	}
}
