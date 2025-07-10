using System;
using System.Collections.Immutable;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PortTaxRateCodeResolverTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When portTaxRateCodesLoader is empty",
			() => new PortTaxRateCodeResolver(portTaxRateCodesLoader: null));
	}

	public void TestIsPortTaxRateCode()
	{
		var rateCodesLoaderMock = new Mock<IPortTaxRateCodesLoader>();
		rateCodesLoaderMock
			.Setup(x => x.GetAllRateCodes())
			.Returns(ImmutableHashSet.Create<ZString>("9AA", "9AB"));
		var portTaxRateCodeResolver = new PortTaxRateCodeResolver(rateCodesLoaderMock.Object);

		CombineAssertions(() =>
		{
			AssertEquals("When RateCode is empty, IsPortTaxRateCode", false, portTaxRateCodeResolver.IsPortTaxRateCode(""));
			AssertEquals("When RateCode is '9AA', IsPortTaxRateCode", true, portTaxRateCodeResolver.IsPortTaxRateCode("9AA"));
			AssertEquals("When RateCode is '9AB', IsPortTaxRateCode", true, portTaxRateCodeResolver.IsPortTaxRateCode("9AB"));
			AssertEquals("When RateCode is 'XYZ', IsPortTaxRateCode", false, portTaxRateCodeResolver.IsPortTaxRateCode("XYZ"));
		});
	}
}
