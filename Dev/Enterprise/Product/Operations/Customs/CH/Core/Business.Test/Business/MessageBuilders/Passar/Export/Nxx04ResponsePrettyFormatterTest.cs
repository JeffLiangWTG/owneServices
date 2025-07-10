using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(Nxx04ResponsePrettyFormatter))]
sealed class Nxx04ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INxx04ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INxx04ResponseDetail dataProvider) => new Nxx04ResponsePrettyFormatter(factory, dataProvider);

	protected override string DecisionTitle => "Amendment response";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INxx04ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new Nxx04ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new Nxx04ResponsePrettyFormatter(Factory, null));
	});
}
