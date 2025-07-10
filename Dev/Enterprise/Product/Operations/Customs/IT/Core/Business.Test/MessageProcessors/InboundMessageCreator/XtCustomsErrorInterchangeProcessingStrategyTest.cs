using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XtCustomsErrorInterchangeProcessingStrategyTest : XTradeInboundMessageCreatorAbstractTest
{
	public void TestProcessXHubErrorInterchange_EDIMessageUnpacking()
	{
		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.IsTransmitInterchange = true;
		sentInterchange.EI_SessionGUID = ZGuid.NewZGuid();

		responseInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;

		ProcessInterchange(responseInterchange);
		AssertContainedMessagesCount(responseInterchange, expectedCount: 1);
	}

	[TestDate]
	public void TestProcessXHubErrorInterchange_OriginatesFromIvistoRequest_CreateNewCusPollingTransaction()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);

		var ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();
		responseInterchange.EI_SessionGUID = ivistoRequestInterchange.EI_SessionGUID;

		ProcessInterchange(responseInterchange);

		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(ivistoRequestInterchange, "OPN", ZDateTime.Now.AddDays(1), "IVI");
	}

	[TestDate]
	public void TestProcessXHubErrorInterchange_OriginatesFromIvistoRequest_UpdateCusPollingTransaction()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);

		var ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();
		helper.CreateIvistoPollingTransaction("PND", ZDateTime.Now, ivistoRequestInterchange, ZDate.Today);

		responseInterchange.EI_SessionGUID = ivistoRequestInterchange.EI_SessionGUID;

		ProcessInterchange(responseInterchange);

		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(ivistoRequestInterchange, "OPN", ZDateTime.Now.AddDays(1), "IVI");
	}

	public void TestProcessXHubErrorInterchange_OriginatesFromNonIvistoRequest_DoNotCreateNewCusPollingTransaction()
	{
		var eur1RequestInterchange = CreateEur1RequestInterchange();
		responseInterchange.EI_SessionGUID = eur1RequestInterchange.EI_SessionGUID;

		ProcessInterchange(responseInterchange);

		CusPollingTransactionTestHelper.AssertNoPollingTransactionsForInterchange(eur1RequestInterchange);
	}

	public void TestProcessXHubErrorInterchange_ThrowsCouldNotFindRelatedBusinessObjectException()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);

		var ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();

		responseInterchange.EI_SessionGUID = ZGuid.NewZGuid();

		AssertExceptionThrown<CouldNotFindRelatedBusinessObjectException>(() => ProcessInterchange(responseInterchange));

		AssertContainedMessagesCount("When CouldNotFindRelatedBusinessObjectException is thrown", responseInterchange, expectedCount: 0);
	}

	[TestDate]
	public void TestProcessXHubErrorInterchange_OriginatesFromIrildesRequest_CreateNewCusPollingTransaction()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);

		var irildesRequestInterchange = helper.CreateIrildesRequestInterchange();
		responseInterchange.EI_SessionGUID = irildesRequestInterchange.EI_SessionGUID;

		ProcessInterchange(responseInterchange);

		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(irildesRequestInterchange, "OPN", ZDateTime.Now.AddDays(1), "IRI");
	}

	[TestDate]
	public void TestProcessXHubErrorInterchange_OriginatesFromIrildesRequest_UpdateCusPollingTransaction()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);

		var irildesRequestInterchange = helper.CreateIrildesRequestInterchange();
		helper.CreateIrildesPollingTransaction("PND", ZDateTime.Now, irildesRequestInterchange, ZDateTime.Now);

		responseInterchange.EI_SessionGUID = irildesRequestInterchange.EI_SessionGUID;

		ProcessInterchange(responseInterchange);

		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(irildesRequestInterchange, "OPN", ZDateTime.Now.AddDays(1), "IRI");
	}

	protected override void SetUp()
	{
		base.SetUp();

		responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = "ITH";
		responseInterchange.EI_InterchangeType = "XER";
	}

	EDIInterchange responseInterchange;

	EDIInterchange CreateEur1RequestInterchange()
	{
		var transmitInterchange = Factory.New<EDIInterchange>();
		transmitInterchange.EI_ApplicationCode = "ITH";
		transmitInterchange.EI_InterchangeType = "EU1";
		transmitInterchange.IsTransmitInterchange = true;
		transmitInterchange.EI_SessionGUID = ZGuid.NewZGuid();
		return transmitInterchange;
	}
}
