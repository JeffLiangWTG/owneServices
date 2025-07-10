using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NC124ResponsePrettyFormatter))]
sealed class NC124ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INC124ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INC124ResponseDetail dataProvider) => new NC124ResponsePrettyFormatter(factory, dataProvider);
	protected override string DecisionTitle => "Activation response";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INC124ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NC124ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NC124ResponsePrettyFormatter(Factory, null));
	});
}
