using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT009ResponsePrettyFormatter))]
sealed class NT009ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INT009ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INT009ResponseDetail dataProvider) => new NT009ResponsePrettyFormatter(factory, dataProvider);

	protected override string DecisionTitle => "Withdrawal response";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT009ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT009ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT009ResponsePrettyFormatter(Factory, null));
	});
}
