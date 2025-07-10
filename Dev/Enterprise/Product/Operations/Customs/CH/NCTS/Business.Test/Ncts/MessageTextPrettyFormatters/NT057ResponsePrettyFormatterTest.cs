using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT057ResponsePrettyFormatter))]
sealed class NT057ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INT057ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INT057ResponseDetail dataProvider) => new NT057ResponsePrettyFormatter(factory, dataProvider);

	protected override string DecisionTitle => "Unloading Remarks response";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT057ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT057ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT057ResponsePrettyFormatter(Factory, null));
	});
}
