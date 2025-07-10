using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE009ResponsePrettyFormatter))]
sealed class NE009ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INE009ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INE009ResponseDetail dataProvider) => new NE009ResponsePrettyFormatter(factory, dataProvider);
	protected override string DecisionTitle => "Withdrawal response";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INE009ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NE009ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NE009ResponsePrettyFormatter(Factory, null));
	});
}
