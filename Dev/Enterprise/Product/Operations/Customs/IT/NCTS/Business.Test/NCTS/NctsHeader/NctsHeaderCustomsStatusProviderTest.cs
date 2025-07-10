using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderCustomsStatusProviderTest : TestCase
{
	public void TestAwaitingMessageStatus()
	{
		AssertEquals(nameof(provider.AwaitingMessageStatus), NctsMessageStatusList.Codes.DepartureDeclarationSent, provider.AwaitingMessageStatus);
	}

	public void TestAcknowledgedMessageStatus()
	{
		AssertEquals(nameof(provider.AcknowledgedMessageStatus), NctsMessageStatusList.Codes.Ok, provider.AcknowledgedMessageStatus);
	}

	public void TestClearedMessageStatus()
	{
		AssertEquals(nameof(provider.ClearedMessageStatus), NctsMessageStatusList.Codes.Ok, provider.ClearedMessageStatus);
	}

	public void TestErrorMessageStatus()
	{
		AssertEquals(nameof(provider.ErrorMessageStatus), NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, provider.ErrorMessageStatus);
	}

	public void TestRegisteredCustomsStatus()
	{
		AssertEquals(nameof(provider.RegisteredCustomsStatus), NctsTransitStatusList.Codes.DeclarationMrnAllocated, provider.RegisteredCustomsStatus);
	}

	public void TestUnderControlCustomsStatus()
	{
		AssertEquals(nameof(provider.UnderControlCustomsStatus), NctsTransitStatusList.Codes.GoodsNotReleasedForTransit, provider.UnderControlCustomsStatus);
	}

	public void TestClearedCustomsStatus()
	{
		AssertEquals(nameof(provider.ClearedCustomsStatus), NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, provider.ClearedCustomsStatus);
	}

	public void TestNbRejectedCustomsStatus()
	{
		AssertEquals(nameof(provider.NbRejectedCustomsStatus), NctsTransitStatusList.Codes.NbRejected, provider.NbRejectedCustomsStatus);
	}

	public void TestArrivalCustomsStatus()
	{
		AssertEquals(nameof(provider.ArrivalCustomsStatus), NctsTransitStatusList.Codes.GoodsWrittenOff, provider.ArrivalCustomsStatus);
	}

	public void TestStatusWithInformationOrderCollection()
	{
		var expectedValues = new CustomsStatusOrder[]
		{
			new CustomsStatusOrder(ZString.Empty, 0),
			new CustomsStatusOrder(NctsTransitStatusList.Codes.DeclarationMrnAllocated, 1),
			new CustomsStatusOrder(NctsTransitStatusList.Codes.NbRejected, 2),
			new CustomsStatusOrder(NctsTransitStatusList.Codes.GoodsNotReleasedForTransit, 3),
			new CustomsStatusOrder(NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, 4),
			new CustomsStatusOrder(NctsTransitStatusList.Codes.GoodsWrittenOff, 5)
		};
		CustomsStatusOrderTestHelper.AssertCollection(expectedValues, provider.StatusWithInformationOrderCollection.ToArray());
	}

	protected override void SetUp()
	{
		base.SetUp();
		provider = new NctsHeaderCustomsStatusProvider();
	}

	ISadCustomsStatusProvider provider;
}
