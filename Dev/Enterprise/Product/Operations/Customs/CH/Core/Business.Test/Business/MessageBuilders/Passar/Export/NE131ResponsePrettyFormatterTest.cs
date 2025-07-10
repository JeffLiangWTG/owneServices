using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE131ResponsePrettyFormatter))]
sealed class NE131ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INE131ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INE131ResponseDetail dataProvider) => new NE131ResponsePrettyFormatter(factory, dataProvider);
	protected override string DecisionTitle => "Data transfer e-Dec to Passar response";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INE131ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NE131ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NE131ResponsePrettyFormatter(Factory, null));
	});
}
