using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NTx28ResponsePrettyFormatter))]
sealed class NTx28ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INTx28ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INTx28ResponseDetail dataProvider) => new NTx28ResponsePrettyFormatter(factory, dataProvider);
	protected override string DecisionTitle => "Departure response";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INTx28ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NTx28ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NE028ResponsePrettyFormatter(Factory, null));
	});
}
