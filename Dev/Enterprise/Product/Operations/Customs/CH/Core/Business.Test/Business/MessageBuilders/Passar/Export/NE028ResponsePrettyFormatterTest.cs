using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE028ResponsePrettyFormatter))]
sealed class NE028ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INE028ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INE028ResponseDetail dataProvider) => new NE028ResponsePrettyFormatter(factory, dataProvider);
	protected override string DecisionTitle => "Export response";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INE028ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NE028ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NE028ResponsePrettyFormatter(Factory, null));
	});
}
