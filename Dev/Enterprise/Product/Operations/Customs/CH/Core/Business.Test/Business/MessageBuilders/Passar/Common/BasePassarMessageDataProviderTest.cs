using System;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class BasePassarMessageDataProviderTest<TDataProvider> : BasePassarDataProviderTest<TDataProvider>
														where TDataProvider : class, IPassarMessage
{
	public void TestMessageProperties()
	{
		GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123456");

		CombineAssertions(() =>
		{
			AssertNull("Correlation Identifier", DataProvider.CorrelationIdentifier);
			AssertEquals("Message Identification", true, ZGuid.IsGuid(DataProvider.MessageIdentification));
			AssertEquals("MessageSender", "123456", DataProvider.MessageSender);
		});
	}

	public void TestMessageSender_ShouldUseParentCompany_WhenCurrentBranchBIDNotFound()
	{
		const string bid = "123456";

		GlbBranch.CurrentBranch.GB_OH_OrgProxy = CreateBranchOrgHeaderWithoutBID().PK;
		GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, bid);

		AssertNullOrEmpty("Precondition", GlbBranch.CurrentBranch.OrgProxy.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID));
		AssertEquals(bid, DataProvider.MessageSender);

		OrgHeader CreateBranchOrgHeaderWithoutBID() => OrgHeader.New(Factory);
	}

	[TestDate(2023, 06, 04, 16, 00, 01)]
	public void TestPreparationDateAndTime()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Precondition:", new DateTime(2023, 06, 04, 16, 00, 01), ZDateTime.Now);
			AssertEquals("PreparationDateAndTime should return Now", new DateTime(2023, 06, 04, 16, 00, 01), DataProvider.PreparationDateAndTime);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			AssertEquals("Precondition:", new DateTime(2023, 06, 05, 16, 00, 01), ZDateTime.Now);
			AssertEquals("Each invocation should return the same value", new DateTime(2023, 06, 04, 16, 00, 01), DataProvider.PreparationDateAndTime);
		});
	}

	public void TestOppositeInformation() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.OppositeInformation);
		AssertSame("cached", DataProvider.OppositeInformation, DataProvider.OppositeInformation);
	});
}
