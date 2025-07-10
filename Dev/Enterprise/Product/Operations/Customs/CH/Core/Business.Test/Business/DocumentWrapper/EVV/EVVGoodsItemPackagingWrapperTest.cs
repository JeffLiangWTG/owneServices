using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemPackagingWrapper))]
sealed class EVVGoodsItemPackagingWrapperTest : TestCaseWithFactory
{
	public void TestNullConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null detail", () => EVVGoodsItemPackagingWrapper.New(null, Factory));

		var packagingMock = new Mock<IEvvGoodsItemPackaging>();
		AssertExceptionThrown<ArgumentNullException>("Null factory", () => EVVGoodsItemPackagingWrapper.New(packagingMock.Object, null));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var packagingMock = new Mock<IEvvGoodsItemPackaging>();
		packagingMock.Setup(m => m.PackagingType).Returns("P1");
		packagingMock.Setup(m => m.Quantity).Returns("100");
		packagingMock.Setup(m => m.PackagingReferenceNumber).Returns("ABC123");

		var wrapper = EVVGoodsItemPackagingWrapper.New(packagingMock.Object, Factory);

		AssertEquals("PackagingType", "UNPKGCodeNoBulk", wrapper.PackagingType);
		AssertEquals("Quantity", "100", wrapper.Quantity);
		AssertEquals("PackagingReferenceNumber", "ABC123", wrapper.PackagingReferenceNumber);

		packagingMock.Setup(m => m.PackagingType).Returns("P99");

		AssertEquals("PackagingType - Unknown code", "P99", wrapper.PackagingType);
	});

	public void TestEmptyProperties() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var packagingMock = new Mock<IEvvGoodsItemPackaging>();
		packagingMock.Setup(m => m.PackagingType).Returns(string.Empty);
		packagingMock.Setup(m => m.Quantity).Returns(string.Empty);
		packagingMock.Setup(m => m.PackagingReferenceNumber).Returns(string.Empty);

		var wrapper = EVVGoodsItemPackagingWrapper.New(packagingMock.Object, Factory);

		AssertEquals("PackagingType", "---", wrapper.PackagingType);
		AssertEquals("Quantity", "---", wrapper.Quantity);
		AssertEquals("PackagingReferenceNumber", "---", wrapper.PackagingReferenceNumber);
	});
}
