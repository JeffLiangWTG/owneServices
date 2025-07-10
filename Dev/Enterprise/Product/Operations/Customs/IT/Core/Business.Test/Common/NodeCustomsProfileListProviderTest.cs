using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NodeCustomsProfileListProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Required factory", () => new NodeCustomsProfileListProvider(factory: null, GetSupportingData(new ReadOnlyCollection<OrgHeader>(new List<OrgHeader>()))));
		AssertExceptionThrown<ArgumentNullException>("Required supportingData", () => new NodeCustomsProfileListProvider(Factory, customsProfileListProviderSupportingData: null));
	}

	public void TestGetAccountDetailsFilteredByEligibleOrganizations()
	{
		var supportingData = GetSupportingData(new ReadOnlyCollection<OrgHeader>(new List<OrgHeader> { orgHeaderAA, orgHeaderREP1 }));
		var customsProfileListProvider = (ICustomsProfileListProvider)new NodeCustomsProfileListProvider(Factory, supportingData);
		var availableAccountDetails = customsProfileListProvider.GetAccountDetails();

		AssertContainsInternalCode(availableAccountDetails, "When eligible organizations are set", 2, "AA-1234", "RR-REP1");
		AssertSame("Cached", availableAccountDetails, customsProfileListProvider.GetAccountDetails());
	}

	public void TestGetAccountDetailsFilteredByCompany()
	{
		var supportingData = GetSupportingData(new ReadOnlyCollection<OrgHeader>(new List<OrgHeader>() { null }));
		var customsProfileListProvider = (ICustomsProfileListProvider)new NodeCustomsProfileListProvider(Factory, supportingData);
		var availableAccountDetails = customsProfileListProvider.GetAccountDetails();

		AssertContainsInternalCode(availableAccountDetails, "When no eligible organizations are set", 4, "AA-1234", "BB-5678", "RR-REP1", "RR-REPZ");
		AssertSame("Cached", availableAccountDetails, customsProfileListProvider.GetAccountDetails());
	}

	public void TestAccountDetailsDescription()
	{
		var supportingData = GetSupportingData(new ReadOnlyCollection<OrgHeader>(new List<OrgHeader>()));
		var customsProfileListProvider = (ICustomsProfileListProvider)new NodeCustomsProfileListProvider(Factory, supportingData);
		var availableAccountDetails = customsProfileListProvider.GetAccountDetails();

		CombineAssertions("[PRE-CONDITION]", () =>
		{
			Assert("AA-1234 account detail is available", availableAccountDetails.ContainsCode("AA-1234"));
			Assert("BB-5678 account detail is available", availableAccountDetails.ContainsCode("BB-5678"));
			Assert("RR-REP1 account detail is available", availableAccountDetails.ContainsCode("RR-REP1"));
			Assert("RR-REPZ account detail is available", availableAccountDetails.ContainsCode("RR-REPZ"));
		});

		CombineAssertions("Assert Descriptions", () =>
		{
			AssertEquals("1234 02028530281-001", availableAccountDetails["AA-1234"].Description);
			AssertEquals("5678 MZZTRG44T22S123H-001", availableAccountDetails["BB-5678"].Description);
			AssertEquals("R3P1 REP1R33PP1REP1PA-001", availableAccountDetails["RR-REP1"].Description);
			AssertEquals("R3P1 REPZR33PPZREPZPZ-001", availableAccountDetails["RR-REPZ"].Description);
		});
	}

	public void TestDefaultCode()
	{
		CombineAssertions(() =>
		{
			AssertDefaultCode("When AccountDetails only has 1 element", expectedDefaultCode: "AA-1234", orgHeaderAA);
			AssertDefaultCode("When AccountDetails has 0 or more than 1 element", expectedDefaultCode: "", orgHeaderAA, orgHeaderBB);
		});

		void AssertDefaultCode(string assertionMessage, string expectedDefaultCode, params OrgHeader[] organizations)
		{
			var supportingData = GetSupportingData(organizations.ToList().AsReadOnly());
			var customsProfileListProvider = (ICustomsProfileListProvider)new NodeCustomsProfileListProvider(Factory, supportingData);
			var availableAccountDetails = customsProfileListProvider.GetAccountDetails();
			AssertEquals(assertionMessage, expectedDefaultCode, availableAccountDetails.DefaultCode);
		}
	}

	ICustomsProfileListProviderSupportingData GetSupportingData(IReadOnlyCollection<OrgHeader> orgHeaders)
	{
		var mock = new Mock<ICustomsProfileListProviderSupportingData>();
		mock.Setup(m => m.CompanyPK).Returns(GlbCompany.CurrentCompany.PK);
		mock.Setup(m => m.GetEligibleOrganizations()).Returns(orgHeaders);
		return mock.Object;
	}

	void SetUpOrgHeaders()
	{
		orgHeaderAA = Factory.New<OrgHeader>();
		orgHeaderAA.OH_Code = "AA";
		orgHeaderBB = Factory.New<OrgHeader>();
		orgHeaderBB.OH_Code = "BB";
		orgHeaderREP1 = Factory.New<OrgHeader>();
		orgHeaderREP1.OH_Code = "REP1";
		var orgHeaderREPZ = Factory.New<OrgHeader>();
		orgHeaderREPZ.OH_Code = "REPZ";
	}

	void SetUpAccountsManagementRegistry()
	{
		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("AA-1234", "AA", "02028530281-001")
			.AppendAccount("22222222222-001", "5678").AppendAccountDetail("BB-5678", "BB", "MZZTRG44T22S123H-001")
			.AppendAccount("33333333333-001", "R3P1")
			.AppendAccountDetail("RR-REP1", "REP1", "REP1R33PP1REP1PA-001")
			.AppendAccountDetail("RR-REPZ", "REPZ", "REPZR33PPZREPZPZ-001")
			.Build();
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetUpOrgHeaders();
		Factory.Save();
		SetUpAccountsManagementRegistry();
	}

	OrgHeader orgHeaderAA;
	OrgHeader orgHeaderBB;
	OrgHeader orgHeaderREP1;

	void AssertContainsInternalCode(CodeDescriptionPairList nodes, string assertionMessage, int expectedNodesCount, params string[] expectedInternalCodes)
	{
		AssertEquals("Available nodes count", expectedNodesCount, nodes.Count);
		CombineAssertions(assertionMessage, () =>
		{
			foreach (var internalCode in expectedInternalCodes)
			{
				Assert($"{internalCode} Internal Code is available", nodes.ContainsCode(internalCode));
			}
		});
	}
}
