using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT008ResponsePrettyFormatter))]
sealed class NT008ResponsePrettyFormatterTest : DecisionResponsePrettyFormatterTest<INT008ResponseDetail>
{
	protected override IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, INT008ResponseDetail dataProvider) => new NT008ResponsePrettyFormatter(factory, dataProvider);

	protected override string DecisionTitle => "Arrival Notification response";

	protected override void ConfigureSpecificFormatterData(Mock<INT008ResponseDetail> responseDetailMock) => responseDetailMock.Setup(r => r.ArrivalReferenceNumber).Returns(ArrivalReferenceNumber);

	protected override string SpecificFormatterData => $"<h3>Customs Arrival Reference Number: {ArrivalReferenceNumber}</h3>";

	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT008ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT008ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT008ResponsePrettyFormatter(Factory, null));
	});

	const string ArrivalReferenceNumber = "240830-GTAN-7dmC1";
}
