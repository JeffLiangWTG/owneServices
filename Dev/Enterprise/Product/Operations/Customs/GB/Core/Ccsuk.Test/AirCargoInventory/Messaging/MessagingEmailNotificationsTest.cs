using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	abstract class EmailNotificationsTester : TestCaseWithFactory
	{
		protected abstract IRegistryItem Rego { get; }
		protected abstract ZString MessageText1 { get; }
		protected abstract ZString MessageText2 { get; }

		public void TestSendingMessageEmailsCompanyLevelGroup()
		{
			SetupRegistryValueAtCompanyAndBranchLevelForTest();

			AssertEquals(0, Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals(Core.Constants.EmailTo.NominatedGroup, GBCustomsDataRegistry.Instance.CustomsResponseNotificationsItem.GetFallBackValueAtAllLevels(branchMIK.Company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(Core.Constants.EmailTo.NominatedGroup, GBCustomsDataRegistry.Instance.CustomsResponseNotificationsItem.GetFallBackValueAtAllLevels(branchDKK.Company.PK.ToGuid(), Guid.Empty, Guid.Empty));

			CuscarInboundParserTests.SetupAndAssertNotificationForBranch(
				MessageText1,
				Rego,
				Factory, branchMIK, createNotificationGroup: false, awbReference: "801-11112222", createInterchangeForMessageToo: eiHeader);

			AssertContains("userCOMKYN@XXX.com", Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);

			CuscarInboundParserTests.SetupAndAssertNotificationForBranch(
				MessageText2,
				Rego,
				Factory, branchDKK, createNotificationGroup: false, awbReference: "801-33334444", createInterchangeForMessageToo: eiHeader);

			AssertContains("userCOMNUK@XXX.com", Environment.Env.OutgoingCustomsMailManager.EmailsCreated[1].Recipients[0].Email);
		}

		public void TestGettingRegistryValueAtCompanyAndBranchLevel()
		{
			SetupRegistryValueAtCompanyAndBranchLevelForTest();
			AssertEquals("email notification group should be the company KYN group", companyEmailGroup_KYN.PK.ToGuid(), GBCustomsDataRegistry.Instance.GetRegistryItemGuid(Rego, "", branchMIK.Company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("email notification group should be the company NUK group", companyEmailGroup_NUK.PK.ToGuid(), GBCustomsDataRegistry.Instance.GetRegistryItemGuid(Rego, "", branchDKK.Company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("email notification group should be the company KYN group", companyEmailGroup_KYN.PK, GBCustomsDataRegistry.Instance.GetRegistryItemGuid(Rego, "", branchMIK.Company.PK.ToGuid(), branchMIK.PK.ToGuid(), Guid.Empty));
			AssertEquals("email notification group should be the company NUK group", companyEmailGroup_NUK.PK, GBCustomsDataRegistry.Instance.GetRegistryItemGuid(Rego, "", branchDKK.Company.PK.ToGuid(), branchDKK.PK.ToGuid(), Guid.Empty));
		}

		void SetupRegistryValueAtCompanyAndBranchLevelForTest()
		{
			branchMIK = SetupCompanyAndBranchForTest("KYN", "MIK", "GBMIK");
			branchDKK = SetupCompanyAndBranchForTest("NUK", "DKK", "GBDKK");
			var postmastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsItem.SetValue(branchMIK.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsItem.SetValue(branchDKK.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			companyEmailGroup_KYN = AddNotificationGroup("STAFF_C_KYN");
			AddUserToGroup(companyEmailGroup_KYN, "userCOMKYN@XXX.com", "userCOMKYN");
			companyEmailGroup_NUK = AddNotificationGroup("STAFF_C_NUK");
			AddUserToGroup(companyEmailGroup_NUK, "userCOMNUK@XXX.com", "userCOMNUK");
			SetRegistryItemToGroup(companyEmailGroup_KYN.PK.ToGuid(), Rego, branchMIK.Company.PK.ToGuid(), Guid.Empty);
			SetRegistryItemToGroup(companyEmailGroup_NUK.PK.ToGuid(), Rego, branchDKK.Company.PK.ToGuid(), Guid.Empty);
			Factory.Save();
		}

		GlbBranch branchMIK;
		GlbBranch branchDKK;
		GlbGroup companyEmailGroup_KYN;
		GlbGroup companyEmailGroup_NUK;
		readonly string eiHeader = "UNB+UNOA:2+CUKAIR98LHRCWE:IATA+CUKFFW98000CAR:IATA+120511:1108+944'";

		GlbBranch SetupCompanyAndBranchForTest(ZString companyCode, ZString branchCode, ZString branchPortCode)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			company.GC_Code = companyCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = branchCode;
			branch.GB_RL_NKHomePort = branchPortCode;
			return branch;
		}

		void SetRegistryItemToGroup(Guid groupPKGuid, IRegistryItem registryItem, Guid companyPk, Guid branchPk)
		{
			registryItem.SetValue(companyPk, branchPk, Guid.Empty, groupPKGuid);
		}

		GlbGroup AddNotificationGroup(ZString groupCode)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = groupCode;
			return group;
		}

		GlbStaff AddUserToGroup(GlbGroup group, ZString emailAddress, ZString loginName)
		{
			var user = Factory.New<GlbStaff>();
			user.GS_EmailAddress = emailAddress;
			user.GS_LoginName = loginName;
			group.Staff.Add(user);
			return user;
		}
	}

	class CusCarFrcNotificationsTester : EmailNotificationsTester
	{
		protected override IRegistryItem Rego
		{
			get { return GBCustomsDataRegistry.Instance.NotificationCcsukCuscarFrc; }
		}

		protected override ZString MessageText1
		{
			get { return @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+80111112222'GIS+S2Y'TDT+20'LOC+11:LHR:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+12+MSGREF'"; }
		}

		protected override ZString MessageText2
		{
			get { return @"UNH+MSGREF+CUSCAR:2:912:UN:109503'BGM+:::FRC+80133334444'GIS+S2Y'TDT+20'LOC+11:LHR:145:3::BAC:129:ZZZ'NAD+CB+DAN'GID+0'FTX+AAA+++MACHINERY'QTY+118:50'QTY+48:51'MEA+WT++KGM:500'UNT+12+MSGREF'"; }
		}
	}

	class GenralNotificationsTester : EmailNotificationsTester
	{
		protected override IRegistryItem Rego
		{
			get { return GBCustomsDataRegistry.Instance.NotificationCcsukGenralText; }
		}

		protected override ZString MessageText1
		{
			get { return @"UNH+950+GENRAL:0:912:UN+14A5F03D618F4FD19195B677DC248663'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++ONE 80111112222 801-11112222'UNT+5+950'"; }
		}

		protected override ZString MessageText2
		{
			get { return @"UNH+950+GENRAL:0:912:UN+14A5F03D618F4FD19195B677DC248663'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++TWO 80133334444 801-33334444'UNT+5+950'"; }
		}
	}
}
