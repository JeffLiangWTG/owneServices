using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.SubAccountHelper;

namespace Enterprise.Accounting.Business.Test
{
	public class SubAccountHelperTest : TestCaseWithFactory
	{
		public void TestSubAccountValueType()
		{
			var names = Enum.GetNames(typeof(SubAccountValueType));
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"Code",
				"Description",
			}, names);
		}

		public void TestGetSubAccountTypeList()
		{
			AssertEquals("SubAccountTypeList", @"ORG - Organization
SEG - Sales/Expense Groups
STR - Staff and Resources
SGP - Staff Group", SubAccountHelper.GetSubAccountTypeList(Factory).ElementsAsString);
		}

		public void TestGetSubAccountList()
		{
			AssertType(typeof(OrgHeaderCollection), SubAccountHelper.GetSubAccountList(Factory, OrgHeaderSchema.Constants.Prefix));
			AssertType(typeof(AccGroupsCollection), SubAccountHelper.GetSubAccountList(Factory, AccGroupsSchema.Constants.Prefix));
			AssertType(typeof(GlbStaffAndResourceCollection), SubAccountHelper.GetSubAccountList(Factory, GlbStaffSchema.Constants.Prefix));
			AssertType(typeof(GlbGroupCollection), SubAccountHelper.GetSubAccountList(Factory, GlbGroupSchema.Constants.Prefix));
		}

		public void TestIsMultiSubAccountsReadOnly()
		{
			var bizO = Factory.New<DummySubAccountForTest>();

			bizO.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("SubAccount is not readonly", false, SubAccountHelper.IsSubAccountReadOnly(bizO));

			(bizO.SubAccountParent as DummyMultiSubAccountsForTest).JobPK = TestObjectCreator.Job1.PK;
			AssertEquals("SubAccount is readonly when It has job", true, SubAccountHelper.IsSubAccountReadOnly(bizO));

			(bizO.SubAccountParent as DummyMultiSubAccountsForTest).JobPK = ZGuid.Empty;
			bizO.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("SubAccount is not readonly", false, SubAccountHelper.IsSubAccountReadOnly(bizO));

			AssertEquals("SubAccount is readonly when object is null", true, SubAccountHelper.IsSubAccountReadOnly(null));
		}

		public void TestGetSubAccountValueFromSubAccountId()
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("Test", false, true);
			orgHeader.OH_FullName = "Test Company Name";
			var subAccountType = AccountingMasterFilesConstants.SubAccountTypeList.Organization.Code;
			AssertEquals("organization code", "ZTest", SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, subAccountType, orgHeader.PK, SubAccountValueType.Code));
			AssertEquals("organization name", "Test Company Name", SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, subAccountType, orgHeader.PK, SubAccountValueType.Description));

			var salesGroup = TestObjectCreator.CreateSalesGroup("TestSales");
			salesGroup.AR_Desc = "Test Sales Group Desc";
			subAccountType = AccountingMasterFilesConstants.SubAccountTypeList.SalesGroup.Code;
			AssertEquals("sales group code", "TestSales", SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, subAccountType, salesGroup.PK, SubAccountValueType.Code));
			AssertEquals("sales group name", "Test Sales Group Desc", SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, subAccountType, salesGroup.PK, SubAccountValueType.Description));

			var staff = TestObjectCreator.CreateStaff("AAA");
			staff.GS_FullName = "Test Staff Name";
			subAccountType = AccountingMasterFilesConstants.SubAccountTypeList.StaffAndResources.Code;
			AssertEquals("staff code", "AAA", SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, subAccountType, staff.PK, SubAccountValueType.Code));
			AssertEquals("staff name", "Test Staff Name", SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, subAccountType, staff.PK, SubAccountValueType.Description));

			var group = TestObjectCreator.CreateStaffGroup("TestGroup");
			group.GG_Desc = "Test Group Name";
			subAccountType = AccountingMasterFilesConstants.SubAccountTypeList.StaffGroup.Code;
			AssertEquals("group code", "TestGroup", SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, subAccountType, group.PK, SubAccountValueType.Code));
			AssertEquals("group name", "Test Group Name", SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, subAccountType, group.PK, SubAccountValueType.Description));
		}

		[TestDate(2020, 2, 28)]
		public void TestSetSubClassParentTableCode()
		{
			AssertISupportMutilSubAccountAfterSaved();
			AssertISupportMutilSubAccount();
			AssertSetSubClassParentTableCodeWithDisplayMode();
		}

		void AssertSetSubClassParentTableCodeWithDisplayMode()
		{
			ReleaseFactory();
			var testObjectCreator = new TestObjectCreator(Factory);

			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();

			var glHeader = testObjectCreator.CreateGLHeader();
			testObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
			Factory.Save();

			var glJournalInDB = testObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var lineInDB = glJournalInDB.Lines.AddNew();
			lineInDB.AL_AG = glHeader.PK;
			Factory.Save();
			Assert(lineInDB.IsInDatabase);

			var glJournalNotInDB = testObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var lineNotInDB = glJournalNotInDB.Lines.AddNew();
			lineNotInDB.AL_AG = glHeader.PK;
			Assert(!lineNotInDB.IsInDatabase);

			AssertSetSubClassParentTableCodeWithDisplayMode(glJournalInDB, lineInDB, ODisplayMode.ReadOnly);
			AssertSetSubClassParentTableCodeWithDisplayMode(glJournalInDB, lineInDB, ODisplayMode.Browse);
			AssertSetSubClassParentTableCodeWithDisplayMode(glJournalNotInDB, lineNotInDB, ODisplayMode.ReadOnly);
			AssertSetSubClassParentTableCodeWithDisplayMode(glJournalNotInDB, lineNotInDB, ODisplayMode.Browse);

			void AssertSetSubClassParentTableCodeWithDisplayMode(GLJournal glJournal, DependentTransactionLine line, ODisplayMode displayMode)
			{
				SubAccountHelper.SetSubClassParentTableCode(glJournal, displayMode);
				AssertEquals(displayMode == ODisplayMode.ReadOnly && line.IsInDatabase ? 0 : 1, line.SubAccounts.Count);
			}
		}

		void AssertISupportMutilSubAccount()
		{
			var staffPk = TestObjectCreator.CreateStaff("AAA").PK;
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 1M, 100M, 10M, 0M, TestObjectCreator.CommentChargeCode.PK);
			line.AL_AG = ZGuid.Empty;

			var glHeader1 = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, GlbStaffSchema.Constants.Prefix, false);

			AssertEquals("Per-conditon", false, (line as ISupportMultiSubAccounts).IsJobRelated);
			AssertNull("Per-conditon", line.GLHeader);
			AssertEquals("Per-conditon", 0, line.SubAccounts.Count);
			AssertEquals("Per-conditon", false, line.IsInDatabase);
			AssertArrayEqualsByElements("Per-conditon", Array.Empty<ZString>(), line.SubAccounts.SubAccountElements.Select(x => x.SubAccountTypeParentTableCode).ToArray());

			line.AL_AG = glHeader1.PK;
			SubAccountHelper.SetSubClassParentTableCode(line);
			AssertEquals("SubAccounts count shoule be 1", 1, line.SubAccounts.Count);
			AssertArrayEqualsByElements("Add sub account from GL Header", new ZString[] { GlbStaffSchema.Constants.Prefix }, line.SubAccounts.SubAccountElements.Select(x => x.SubAccountTypeParentTableCode).ToArray());

			line.AL_JH = TestObjectCreator.Job1.PK;
			SubAccountHelper.SetSubClassParentTableCode(line);
			AssertArrayEqualsByElements("all sub account will be deleted when job header has value", Array.Empty<ZString>(), line.SubAccounts.SubAccountElements.Select(x => x.SubAccountTypeParentTableCode).ToArray());

			line.AL_JH = Guid.Empty;
			line.AL_AG = glHeader1.PK;
			line.SubAccounts.FirstSubAccount.AL1_SubClassParentId = staffPk;
			Factory.Save();

			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, false);
			SubAccountHelper.SetSubClassParentTableCode(line);
			AssertArrayEqualsByElements("Add sub account from GL Header", new ZString[] { GlbStaffSchema.Constants.Prefix }, line.SubAccounts.SubAccountElements.Select(x => x.SubAccountTypeParentTableCode).ToArray());

			AssertEquals("sub account count should be 1", 1, line.SubAccounts.Count);
			AssertEquals("first sub account AL1_SubClassParentId should be not empty", staffPk, line.SubAccounts.FirstSubAccount.AL1_SubClassParentId);
		}

		void AssertISupportMutilSubAccountAfterSaved()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV002", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			var glHeader1 = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, GlbStaffSchema.Constants.Prefix, false);
			line.AL_AG = glHeader1.PK;

			SubAccountHelper.SetSubClassParentTableCode(line);
			AssertEquals("sub account count", 2, line.SubAccounts.Count);
			line.SubAccounts.FirstSubAccount.AL1_SubClassParentId = TestObjectCreator.Creditor1.PK;

			Factory.Save();

			AssertEquals("sub account count after saving", 1, line.SubAccounts.Count);
			AssertEquals("invoice HasChanges", false, invoice.HasChanges);
			AssertEquals("invoice IsInDatabase", true, invoice.IsInDatabase);

			var newFactory = Factory.CreateNewFactory();
			var invoice2 = newFactory.Load<APInvoice>(invoice.PK);
			var line2 = invoice2.Lines[0];

			SubAccountHelper.SetSubClassParentTableCode(line2);
			AssertEquals("invoice2 HasChanges", false, invoice2.HasChanges);
			AssertEquals("invoice2 IsInDatabase", true, invoice2.IsInDatabase);
			AssertEquals("sub account count after loading", 1, line2.SubAccounts.Count);

			var glHeader2 = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader2, AccGroupsSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader2, GlbGroupSchema.Constants.Prefix, false);
			Factory.Save();

			line2.AL_AG = glHeader2.PK;
			line2.ReadOnly = false;
			SubAccountHelper.SetSubClassParentTableCode(line2);
			AssertEquals("sub account count after loading", 2, line2.SubAccounts.Count);

			var subAccountList = line2.SubAccounts.Cast<TransactionLineSubAccount>();

			Assert(subAccountList.Any(x => x.SubAccountTypeParentTableCode == "AR"));
			Assert(subAccountList.Any(x => x.SubAccountTypeParentTableCode == "GG"));
		}

		public void TestValidateSubClassParentId()
		{
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, false);
			var checkedEnterMessage = "Please enter a value.";

			var dummy1 = Factory.New<DummySubAccountForTest>();
			AssertEquals("Pre-condition", ZString.Empty, dummy1.SubAccountTypeParentTableCode);
			AssertEquals("Pre-condition", ZGuid.Empty, dummy1.SubAccountParentIdInfo.Value);
			AssertNoErrors("Pre-condition", dummy1.SubAccountParentIdInfo);

			dummy1.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
			(dummy1.SubAccountParent as DummyMultiSubAccountsForTest).GLHeaderPK = TestObjectCreator.GLHeader1.PK;
			SubAccountHelper.ValidateSubClassParentId(dummy1.SubAccountParentIdInfo, dummy1.SubAccountTypeParentTableCode, dummy1.SubAccountParent);
			AssertEquals("SubAccountParentId", ZGuid.Empty, dummy1.SubAccountParentIdInfo.Value);
			AssertHasError("SubAccountParentIdInfo should has error when value is empty and isSubClassValidationRuleMandatory is true in GL Header", dummy1.SubAccountParentIdInfo, checkedEnterMessage);

			var dummy2 = Factory.New<DummySubAccountForTest>();
			dummy2.SubAccountTypeParentTableCode = OrgHeaderSchema.Constants.Prefix;
			dummy2.SubAccountParentId = TestObjectCreator.Creditor1.PK;
			SubAccountHelper.ValidateSubClassParentId(dummy2.SubAccountParentIdInfo, dummy2.SubAccountTypeParentTableCode, dummy2.SubAccountParent);
			AssertEquals("SubAccountParentId", TestObjectCreator.Creditor1.PK, dummy2.SubAccountParentIdInfo.Value);
			AssertNoErrors("SubAccountParentIdInfo should has no error when value is not empty and isSubClassValidationRuleMandatory is true in GL Header", dummy2.SubAccountParentIdInfo);

			var dummy3 = Factory.New<DummySubAccountForTest>();
			dummy3.SubAccountTypeParentTableCode = GlbStaffSchema.Constants.Prefix;
			SubAccountHelper.ValidateSubClassParentId(dummy3.SubAccountParentIdInfo, dummy3.SubAccountTypeParentTableCode, dummy3.SubAccountParent);
			AssertEquals("SubAccountParentId", ZGuid.Empty, dummy3.SubAccountParentIdInfo.Value);
			AssertNoErrors("SubAccountParentIdInfo should has no error when value is empty and isSubClassValidationRuleMandatory is false in GL Header", dummy3.SubAccountParentIdInfo);

			var dummy4 = Factory.New<DummySubAccountForTest>();
			dummy4.SubAccountTypeParentTableCode = GlbStaffSchema.Constants.Prefix;
			var staff = TestObjectCreator.CreateStaff("AAA");
			staff.GS_IsActive = false;
			dummy4.SubAccountParentId = staff.PK;
			SubAccountHelper.ValidateSubClassParentId(dummy4.SubAccountParentIdInfo, dummy4.SubAccountTypeParentTableCode, dummy4.SubAccountParent);
			AssertEquals("SubAccountParentId", staff.PK, dummy4.SubAccountParentIdInfo.Value);
			AssertHasError("SubAccountParentIdInfo should has error when enter value is invalid", dummy4.SubAccountParentIdInfo, "Enter a valid selection.");
		}

		public void TestGetSubAccountsQuery()
		{
			var querySqlText = "";
			var subAccountID = ZGuid.NewZGuid();
			AssertEquals(querySqlText, SubAccountHelper.GetSubAccountsQuery("", ZGuid.Empty).LiteralTextSqlFormatted);

			querySqlText = $@"(
	VT_SubAccountParentPK IN 
	(
		SELECT AL1_AL FROM dbo.AccTransactionLineSubAccount WHERE AL1_SubClassParentTableCode = 'OH' 
		AND
		AL1_SubClassParentId = '{subAccountID}'
	)
)
OR
(
	VT_SubAccountParentPK IN 
	(
		SELECT AHS_AH FROM dbo.AccTransactionHeaderSubAccount WHERE AHS_SubClassParentTableCode = 'OH' 
		AND
		AHS_SubClassParentId = '{subAccountID}'
	)
)
";
			AssertEquals(querySqlText, SubAccountHelper.GetSubAccountsQuery("ORG", subAccountID).LiteralTextSqlFormatted);

			querySqlText = @"VT_SubAccountParentPK IN 
(
	SELECT AL1_AL FROM dbo.AccTransactionLineSubAccount WHERE AL1_SubClassParentTableCode = 'OH'
)
OR
VT_SubAccountParentPK IN 
(
	SELECT AHS_AH FROM dbo.AccTransactionHeaderSubAccount WHERE AHS_SubClassParentTableCode = 'OH'
)
OR
VT_AGForSubAccount IN 
(
	SELECT ASA_AG FROM dbo.AccGLHeaderSubAccount WHERE ASA_SubClass = 'OH'
)
";
			AssertEquals(querySqlText, SubAccountHelper.GetSubAccountsQuery("ORG", ZGuid.Empty).LiteralTextSqlFormatted);

			querySqlText = @"VT_AGForSubAccount IN 
(
	SELECT ASA_AG FROM dbo.AccGLHeaderSubAccount WHERE ASA_SubClass = 'OH'
)
AND
VT_SubAccountParentPK NOT IN 
(
	SELECT AL1_AL FROM dbo.AccTransactionLineSubAccount WHERE AL1_SubClassParentTableCode = 'OH'
)
AND
VT_SubAccountParentPK NOT IN 
(
	SELECT AHS_AH FROM dbo.AccTransactionHeaderSubAccount WHERE AHS_SubClassParentTableCode = 'OH'
)
";
			AssertEquals(querySqlText, SubAccountHelper.GetSubAccountsQuery("ORG", ZGuid.Empty, true).LiteralTextSqlFormatted);

			querySqlText = $@"VT_SubAccountParentPK IN 
(
	SELECT AL1_AL FROM dbo.AccTransactionLineSubAccount WHERE AL1_SubClassParentId = '{subAccountID}'
)
OR
VT_SubAccountParentPK IN 
(
	SELECT AHS_AH FROM dbo.AccTransactionHeaderSubAccount WHERE AHS_SubClassParentId = '{subAccountID}'
)
";
			AssertEquals(querySqlText, SubAccountHelper.GetSubAccountsQuery("", subAccountID).LiteralTextSqlFormatted);
		}

		[ExpectNoExceptions]
		public void TestCopySubAccounts()
		{
			var glHeader1 = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, AccGroupsSchema.Constants.Prefix, false);

			//Prepare
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = glHeader1.PK;
			SubAccountHelper.SetSubClassParentTableCode(line);

				IEnumerable<ISupportSubAccount> copyOfInvoiceLineSubAccounts;
				var copyOfInvoiceLine = GetCopyOfInvoiceLine();
				AssertEquals("Invoice Line: Sub Account Count", 2, line.SubAccounts.Count);
				AssertEquals("Copy Of Invoice Line: Sub Account Count", 2, copyOfInvoiceLine.SubAccounts.Count);

			//Did NOT set any SubAccountParentId
			SubAccountHelper.CopySubAccounts(copyOfInvoiceLine, line);
			copyOfInvoiceLineSubAccounts = ((ISupportMultiSubAccounts)copyOfInvoiceLine).SubAccounts.SubAccountElements;
			AssertEquals("Invoice Line: Sub Account Count", 2, line.SubAccounts.Count);
			AssertEquals("Copy Of Invoice Line: Sub Account Count", 2, copyOfInvoiceLineSubAccounts.Count());
			Assert(copyOfInvoiceLineSubAccounts.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == ZGuid.Empty));
			Assert(copyOfInvoiceLineSubAccounts.Any(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix && x.SubAccountParentId == ZGuid.Empty));

			//Set all SubAccountParentId
			var lineSubAccounts = ((ISupportMultiSubAccounts)line).SubAccounts.SubAccountElements;
			lineSubAccounts.First(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.ABIGAS.PK;
			lineSubAccounts.First(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix).SubAccountParentId = TestObjectCreator.AR1.PK;
			copyOfInvoiceLine = GetCopyOfInvoiceLine();
			SubAccountHelper.CopySubAccounts(copyOfInvoiceLine, line);
			copyOfInvoiceLineSubAccounts = ((ISupportMultiSubAccounts)copyOfInvoiceLine).SubAccounts.SubAccountElements;
			AssertEquals("Invoice Line: Sub Account Count", 2, line.SubAccounts.Count);
			AssertEquals("Copy Of Invoice Line: Sub Account Count", 2, copyOfInvoiceLineSubAccounts.Count());
			Assert(copyOfInvoiceLineSubAccounts.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.ABIGAS.PK));
			Assert(copyOfInvoiceLineSubAccounts.Any(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.AR1.PK));

			//Set one SubAccountParentId, and isRefresh = true
			lineSubAccounts.First(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix).SubAccountParentId = ZGuid.Empty;
			copyOfInvoiceLine = GetCopyOfInvoiceLine();
			SubAccountHelper.CopySubAccounts(copyOfInvoiceLine, line, true);
			copyOfInvoiceLineSubAccounts = ((ISupportMultiSubAccounts)copyOfInvoiceLine).SubAccounts.SubAccountElements;
			AssertEquals("Invoice Line: Sub Account Count", 2, line.SubAccounts.Count);
			AssertEquals("Copy Of Invoice Line: Sub Account Count", 1, copyOfInvoiceLineSubAccounts.Count());
			Assert(copyOfInvoiceLineSubAccounts.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.ABIGAS.PK));

			//Object does not implement ISupportMultiSubAccounts
			var dummy = Factory.New<DummyBusinessObject>();
			var copyOfDummy = Factory.New<DummyBusinessObject>();
			AssertNull(copyOfDummy as ISupportMultiSubAccounts);
			AssertNull(dummy as ISupportMultiSubAccounts);
			SubAccountHelper.CopySubAccounts(copyOfDummy as ISupportMultiSubAccounts, dummy as ISupportMultiSubAccounts);

			//SubAccounts = null
			dummy = Factory.New<DummyMultiSubAccountsForTest>();
			copyOfDummy = Factory.New<DummyMultiSubAccountsForTest>();
			AssertNotNull(copyOfDummy as ISupportMultiSubAccounts);
			AssertNotNull(dummy as ISupportMultiSubAccounts);
			SubAccountHelper.CopySubAccounts(copyOfDummy as ISupportMultiSubAccounts, dummy as ISupportMultiSubAccounts);

			InvoicingLineBase GetCopyOfInvoiceLine()
			{
				var copyOfInvoice = Factory.New<APInvoice>();
				var newCopyOfInvoiceLine = (InvoicingLineBase)copyOfInvoice.Lines.AddNew();
				newCopyOfInvoiceLine.AL_AG = glHeader1.PK;
				return newCopyOfInvoiceLine;
			}
		}

		void SetupLineSubAccount(ISupportSubAccountCollection subAccounts, ZString subAccountTypeParentTableCode, ZGuid subAccountParentId)
		{
			var subAccount = subAccounts.AddNew();
			subAccount.SubAccountTypeParentTableCode = subAccountTypeParentTableCode;
			subAccount.SubAccountParentId = subAccountParentId;
		}

		public void TestGetMultiSubAccountTypeCode()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.USD, 1M, TestObjectCreator.ABIGAS);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			AssertEquals(ZString.Empty, SubAccountHelper.GetMultiSubAccountTypeCode(line, line.Factory));

			var lineSubAccounts = ((ISupportMultiSubAccounts)line).SubAccounts;
			SetupLineSubAccount(lineSubAccounts,AccGroupsSchema.Constants.Prefix, TestObjectCreator.AR1.PK);
			SetupLineSubAccount(lineSubAccounts,OrgHeaderSchema.Constants.Prefix, TestObjectCreator.ABIGAS.PK);
			AssertEquals("ORG: ABIGAS, SEG: AR1", SubAccountHelper.GetMultiSubAccountTypeCode(line, line.Factory));

			SetupLineSubAccount(lineSubAccounts,GlbStaffSchema.Constants.Prefix, TestObjectCreator.GS1.PK);
			SetupLineSubAccount(lineSubAccounts,GlbGroupSchema.Constants.Prefix, TestObjectCreator.GG1.PK);
			AssertEquals("ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1", SubAccountHelper.GetMultiSubAccountTypeCode(line, line.Factory));

			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyMultiSubAccountsForTest>();
			var dummy3 = Factory.New<DummyMultiSubAccountsForTest>();
			dummy3.IsSubAccountSupported = false;
			AssertNull("Per-conditon", dummy1 as ISupportMultiSubAccounts);
			Assert("Per-conditon", dummy2.IsSubAccountSupported);
			Assert("Per-conditon", !dummy3.IsSubAccountSupported);
			AssertEquals(ZString.Empty, SubAccountHelper.GetMultiSubAccountTypeCode(dummy1 as ISupportMultiSubAccounts, Factory));
			AssertEquals(ZString.Empty, SubAccountHelper.GetMultiSubAccountTypeCode(dummy2, Factory));
			AssertEquals(ZString.Empty, SubAccountHelper.GetMultiSubAccountTypeCode(dummy3, Factory));
			AssertEquals(ZString.Empty, SubAccountHelper.GetMultiSubAccountTypeCode(null, null));
		}

		public void TestNotCreateMultiSubAccountsForEliminationJournal()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category1 = list.AddNew();
			category1.Code = "ELM";
			category1.Description = (NoResString)"Category ELM";
			category1.Bool = true; // active
			category1.Bool2 = true; // elimination
			category1.Bool3 = false; // closing
			category1.Bool4 = false; // opening
			var category2 = list.AddNew();
			category2.Code = "OPN";
			category2.Description = (NoResString)"Category 2";
			category2.Bool = true; // active
			category2.Bool2 = false; // elimination
			category2.Bool3 = false; // closing
			category2.Bool4 = false; // opening

			using (AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var eliminationCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty).EliminationCategory;

				var glHeader = TestObjectCreator.CreateGLHeader("TestGLAcc");
				var glHeaderSubAccount1 = TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);

				AssertEquals("Pre condition", 2, glHeader.SubAccountTypes.Count);
				AssertEquals("Pre condition", true, glHeader.SubAccountTypes.ToArray<AccGLHeaderSubAccount>().Where(y => y.ASA_SubClass == "OH").Select(x => x.ASA_IsSubClassValidationRuleMandatory).FirstOrDefault());
				AssertEquals("Pre condition", false, glHeader.SubAccountTypes.ToArray<AccGLHeaderSubAccount>().Where(y => y.ASA_SubClass == "GS").Select(x => x.ASA_IsSubClassValidationRuleMandatory).FirstOrDefault());

				var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
				journal.AH_TransactionCategory = "OPN";
				var journalLine = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, glHeader.PK);
				journalLine.Validation.ValidateAll();

				AssertEquals("Elimination Journal line should contain error", true, journalLine.HasErrors);
				AssertEquals("Elimination journal line should create default sub accounts", 2, journalLine.SubAccounts.Count);
				AssertHasError(journalLine.SubAccounts.ToArray<AccTransactionLineSubAccount>().Where(y => y.AL1_SubClassParentTableCode == "OH").Select(x => x.AL1_SubClassParentIdInfo).FirstOrDefault(), "Please enter a Sub Account.");
				AssertNoErrors(journalLine.SubAccounts.ToArray<AccTransactionLineSubAccount>().Where(y => y.AL1_SubClassParentTableCode == "GS").Select(x => x.AL1_SubClassParentIdInfo).FirstOrDefault());

				journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
				journal.AH_TransactionCategory = eliminationCategory.Code;
				journalLine = (GLJournalLine)journal.Lines.AddNew();
				journalLine.SetContext(BusinessContext.AutoEliminationJournal);
				journalLine.UnsignedOSLineAmount = 10;
				journalLine.DebitCreditSign = nameof(DebitCredit.CR);
				journalLine.AL_AG = glHeader.PK;
				journalLine.Validation.ValidateAll();

				AssertEquals("Elimination Journal line should not contain error", false, journalLine.HasErrors);
				AssertEquals("Elimination journal line should create default sub accounts", 2, journalLine.SubAccounts.Count);
				AssertEquals("AL1_SubClassParentId for journalline should be empty", ZGuid.Empty, journalLine.SubAccounts.ToArray<AccTransactionLineSubAccount>().Where(y => y.AL1_SubClassParentTableCode == "OH").Select(x => x.AL1_SubClassParentId).FirstOrDefault());
				AssertEquals("AL1_SubClassParentId for journalline should be empty", ZGuid.Empty, journalLine.SubAccounts.ToArray<AccTransactionLineSubAccount>().Where(y => y.AL1_SubClassParentTableCode == "GS").Select(x => x.AL1_SubClassParentId).FirstOrDefault());
			}
		}

		class DummyMultiSubAccountsForTest : DummyBusinessObject, ISupportMultiSubAccounts, IObsoleteValidation
		{
			public DummyMultiSubAccountsForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public bool IsSubAccountSupported { get; set; } = true;

			public AccGLHeader GLHeader => Factory.Load<AccGLHeader>(GLHeaderPK);

			public bool IsJobRelated => JobPK.IsValid;

			public ZGuid JobPK { get; set; }

			public ZGuid GLHeaderPK { get; set; }

			public ISupportSubAccountCollection SubAccounts => null;

			bool ISupportMultiSubAccounts.IsMultiSubAccountsSupported => IsSubAccountSupported;
		}

		class DummySubAccountForTest : DummyBusinessObject, ISupportSubAccount, IObsoleteValidation
		{
			public DummySubAccountForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZString SubAccountTypeParentTableCode { get => fSubAccountTypeParentTableCode; set => fSubAccountTypeParentTableCode = value; }
			ZString fSubAccountTypeParentTableCode;

			[List("SubAccountList")]
			public ZGuid SubAccountParentId { get; set; }

			public ZPropertyInfo SubAccountParentIdInfo => GetZPropertyInfo(nameof(SubAccountParentId));

			public ZString SubAccountTypeDisplayCode { get => fSubAccountTypeDisplayCode; set => fSubAccountTypeDisplayCode = value; }
			ZString fSubAccountTypeDisplayCode;

			public ZPropertyInfo SubAccountTypeDisplayCodeInfo => GetZPropertyInfo(nameof(SubAccountTypeDisplayCode));

			public IBusinessObjectCollection SubAccountList => SubAccountHelper.GetSubAccountList(Factory, SubAccountTypeParentTableCode);

			public ISupportMultiSubAccounts SubAccountParent
			{
				get
				{
					if (fSubAccountParent == null)
					{
						fSubAccountParent = Factory.New<DummyMultiSubAccountsForTest>();
					}
					return fSubAccountParent;
				}
			}
			ISupportMultiSubAccounts fSubAccountParent;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
	}
}
