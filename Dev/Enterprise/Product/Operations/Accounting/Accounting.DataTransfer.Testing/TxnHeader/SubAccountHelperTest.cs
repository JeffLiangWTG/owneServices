using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class SubAccountHelperTest : TestCaseWithFactory
	{
		public void TestGetSubAccountPKFromCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TSTORG";

			AssertEquals(orgHeader.PK, SubAccountHelper.GetSubAccountPKFromCode(Factory, Core.Constants.SubAccountType.Organization, "TSTORG"));

			var salesGroup = Factory.NewWithValidTestData<AccGroups>();
			salesGroup.AR_Code = "Accounting";

			AssertEquals(salesGroup.PK, SubAccountHelper.GetSubAccountPKFromCode(Factory, Core.Constants.SubAccountType.SalesGroup, "Accounting"));

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TS";

			AssertEquals(staff.PK, SubAccountHelper.GetSubAccountPKFromCode(Factory, Core.Constants.SubAccountType.StaffAndResources, "TS"));

			var staffGroup = Factory.NewWithValidTestData<GlbGroup>();
			staffGroup.GG_Code = "TestGroup";

			AssertEquals(staffGroup.PK, SubAccountHelper.GetSubAccountPKFromCode(Factory, Core.Constants.SubAccountType.StaffGroup, "TestGroup"));
		}

		public void TestGetSubAccountXmlFromSubAccountId()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TSTORG";

			Xsd.SubAccount subAccount = SubAccountHelper.GetSubAccountXmlFromSubAccountId(Factory, Core.Constants.SubAccountType.Organization, orgHeader.PK);

			AssertEquals(Core.Constants.SubAccountType.Organization, subAccount.Type.Code.ToString());
			AssertEquals(Core.Constants.SubAccountTypeDescriptions.Organization, subAccount.Type.Description);
			AssertEquals(orgHeader.OH_Code, subAccount.Code);

			var salesGroup = Factory.NewWithValidTestData<AccGroups>();
			salesGroup.AR_Code = "Accounting";

			subAccount = SubAccountHelper.GetSubAccountXmlFromSubAccountId(Factory, Core.Constants.SubAccountType.SalesGroup, salesGroup.PK);

			AssertEquals(Core.Constants.SubAccountType.SalesGroup, subAccount.Type.Code.ToString());
			AssertEquals(Core.Constants.SubAccountTypeDescriptions.SalesGroup, subAccount.Type.Description);
			AssertEquals(salesGroup.AR_Code, subAccount.Code);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TS";

			subAccount = SubAccountHelper.GetSubAccountXmlFromSubAccountId(Factory, Core.Constants.SubAccountType.StaffAndResources, staff.PK);

			AssertEquals(Core.Constants.SubAccountType.StaffAndResources, subAccount.Type.Code.ToString());
			AssertEquals(Core.Constants.SubAccountTypeDescriptions.StaffAndResources, subAccount.Type.Description);
			AssertEquals(staff.GS_Code, subAccount.Code);

			var staffGroup = Factory.NewWithValidTestData<GlbGroup>();
			staffGroup.GG_Code = "TestGroup";

			subAccount = SubAccountHelper.GetSubAccountXmlFromSubAccountId(Factory, Core.Constants.SubAccountType.StaffGroup, staffGroup.PK);

			AssertEquals(Core.Constants.SubAccountType.StaffGroup, subAccount.Type.Code.ToString());
			AssertEquals(Core.Constants.SubAccountTypeDescriptions.StaffGroup, subAccount.Type.Description);
			AssertEquals(staffGroup.GG_Code, subAccount.Code);
		}

		public void TestGetSubAccountsXmlFromSubAccounts()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var salesGroup = testObjectCreator.CreateSalesGroup("SG1");
			var staff = testObjectCreator.CreateStaff("SR1");
			var staffGroup = testObjectCreator.CreateStaffGroup("GG1");
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var line = (APInvoiceLine)invoice.Lines.AddNew();

			var subAccount1 = line.SubAccounts.AddNew();
			subAccount1.AL1_AL = line.PK;
			subAccount1.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			subAccount1.AL1_SubClassParentId = testObjectCreator.ABIGAS.PK;

			var subAccount2 = line.SubAccounts.AddNew();
			subAccount2.AL1_AL = line.PK;
			subAccount2.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			subAccount2.AL1_SubClassParentId = salesGroup.PK;

			var subAccount3 = line.SubAccounts.AddNew();
			subAccount3.AL1_AL = line.PK;
			subAccount3.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			subAccount3.AL1_SubClassParentId = staff.PK;

			var subAccount4 = line.SubAccounts.AddNew();
			subAccount4.AL1_AL = line.PK;
			subAccount4.AL1_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			subAccount4.AL1_SubClassParentId = staffGroup.PK;

			var subAccountXML = SubAccountHelper.GetSubAccountsXmlFromSubAccounts(Factory, line.SubAccounts);

			AssertEquals(4, subAccountXML.Count);

			AssertEquals(testObjectCreator.ABIGAS.OH_Code, subAccountXML[0].Code);
			AssertEquals(Core.Constants.SubAccountType.Organization, subAccountXML[0].Type.Code.ToString());
			AssertEquals(Core.Constants.SubAccountTypeDescriptions.Organization, subAccountXML[0].Type.Description);

			AssertEquals(salesGroup.AR_Code, subAccountXML[1].Code);
			AssertEquals(Core.Constants.SubAccountType.SalesGroup, subAccountXML[1].Type.Code.ToString());
			AssertEquals(Core.Constants.SubAccountTypeDescriptions.SalesGroup, subAccountXML[1].Type.Description);

			AssertEquals(staff.GS_Code, subAccountXML[2].Code);
			AssertEquals(Core.Constants.SubAccountType.StaffAndResources, subAccountXML[2].Type.Code.ToString());
			AssertEquals(Core.Constants.SubAccountTypeDescriptions.StaffAndResources, subAccountXML[2].Type.Description);

			AssertEquals(staffGroup.GG_Code, subAccountXML[3].Code);
			AssertEquals(Core.Constants.SubAccountType.StaffGroup, subAccountXML[3].Type.Code.ToString());
			AssertEquals(Core.Constants.SubAccountTypeDescriptions.StaffGroup, subAccountXML[3].Type.Description);
		}

		public void TestCreateSubAccountsFromXml()
		{
			var glHeader = TestObjectCreator.GLHeader1;
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, true);
			var staff = TestObjectCreator.CreateStaff("TST");
			Factory.Save();

			var xmlSubAccountCollection = new Xsd.SubAccountCollection();
			var xmlSubAccount1 = xmlSubAccountCollection.AddNew();
			xmlSubAccount1.Type.Code = Core.Constants.SubAccountType.StaffAndResources;
			xmlSubAccount1.Code = "TST";
			var xmlSubAccount2 = xmlSubAccountCollection.AddNew();
			xmlSubAccount2.Type.Code = Core.Constants.SubAccountType.Organization;
			xmlSubAccount2.Code = TestObjectCreator.ABIGAS.OH_Code;

			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "TEST001", TestObjectCreator.USD, 1m, 100m, 0m, 100m, 0m);
			var line = transaction.Lines[0];

			AssertEquals("Pre-condition, should create sub account records when evaluate AL_AG", 2, line.SubAccounts.Count);
			AssertEquals(glHeader.PK, line.GLHeader.PK);

			SubAccountHelper.CreateSubAccountFromXml(line, xmlSubAccountCollection);

			AssertEquals("Number of sub account records should NOT be changed after import", 2, line.SubAccounts.Count);

			var subAccount1 = line.SubAccounts.Cast<TransactionLineSubAccount>().Single(x => x.AL1_SubClassParentTableCode == OrgHeaderSchema.Constants.Prefix);
			var subAccount2 = line.SubAccounts.Cast<TransactionLineSubAccount>().Single(x => x.AL1_SubClassParentTableCode == GlbStaffSchema.Constants.Prefix);

			AssertEquals(TestObjectCreator.ABIGAS.PK, subAccount1.AL1_SubClassParentId);
			AssertEquals(staff.PK, subAccount2.AL1_SubClassParentId);
		}

		public void TestCreateSubAccountFromXmlWithInsufficientLineSubAccountInfo()
		{
			var glHeader = TestObjectCreator.GLHeader1;
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, true);
			var staff = TestObjectCreator.CreateStaff("TST");
			Factory.Save();

			var xmlSubAccountCollection = new Xsd.SubAccountCollection();
			var xmlSubAccount1 = xmlSubAccountCollection.AddNew();
			xmlSubAccount1.Type.Code = Core.Constants.SubAccountType.StaffAndResources;
			xmlSubAccount1.Code = "TST";

			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "TEST001", TestObjectCreator.USD, 1m, 100m, 0m, 100m, 0m);
			var line = transaction.Lines[0];

			AssertEquals("Pre-condition, should create sub account records when evaluate AL_AG", 2, line.SubAccounts.Count);
			AssertEquals(glHeader.PK, line.GLHeader.PK);

			SubAccountHelper.CreateSubAccountFromXml(line, xmlSubAccountCollection);

			AssertEquals(2, line.SubAccounts.Count);

			var subAccount1 = line.SubAccounts.Cast<TransactionLineSubAccount>().Single(x => x.AL1_SubClassParentTableCode == OrgHeaderSchema.Constants.Prefix);
			var subAccount2 = line.SubAccounts.Cast<TransactionLineSubAccount>().Single(x => x.AL1_SubClassParentTableCode == GlbStaffSchema.Constants.Prefix);

			AssertEquals("Sub Account info, which defined in line GL Account but not in line XML, will be added with empty parent ID.", ZGuid.Empty, subAccount1.AL1_SubClassParentId);
			AssertEquals(staff.PK, subAccount2.AL1_SubClassParentId);
		}

		public void TestCreateSubAccountFromXmlWithInsufficientGLAccountSubAccountInfo()
		{
			var glHeader = TestObjectCreator.GLHeader1;
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, true);
			var staff = TestObjectCreator.CreateStaff("TST");
			Factory.Save();

			var xmlSubAccountCollection = new Xsd.SubAccountCollection();
			var xmlSubAccount1 = xmlSubAccountCollection.AddNew();
			xmlSubAccount1.Type.Code = Core.Constants.SubAccountType.StaffAndResources;
			xmlSubAccount1.Code = "TST";
			var xmlSubAccount2 = xmlSubAccountCollection.AddNew();
			xmlSubAccount2.Type.Code = Core.Constants.SubAccountType.Organization;
			xmlSubAccount2.Code = TestObjectCreator.ABIGAS.OH_Code;

			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "TEST001", TestObjectCreator.USD, 1m, 100m, 0m, 100m, 0m);
			var line = transaction.Lines[0];

			AssertEquals("Pre-condition, should create sub account records when evaluate AL_AG", 1, line.SubAccounts.Count);
			AssertEquals(glHeader.PK, line.GLHeader.PK);

			SubAccountHelper.CreateSubAccountFromXml(line, xmlSubAccountCollection);

			AssertEquals(1, line.SubAccounts.Count);

			AssertEquals("Sub Account info, which defined in line XML but not in line GL Account, will be skipped.", GlbStaffSchema.Constants.Prefix, line.SubAccounts[0].AL1_SubClassParentTableCode);
			AssertEquals(staff.PK, line.SubAccounts[0].AL1_SubClassParentId);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
