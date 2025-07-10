using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemDetailWrapper))]
sealed class EVVGoodsItemDetailWrapperTest : TestCaseWithFactory
{
	public void TestNullConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null detail", () => EVVGoodsItemDetailWrapper.New(null, Factory));

		var detailMock = new Mock<IEvvGoodsItemDetail>();
		AssertExceptionThrown<ArgumentNullException>("Null factory", () => EVVGoodsItemDetailWrapper.New(detailMock.Object, null));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateGoodsItemsDetailNameTypeList(Factory);

		var detailMock = new Mock<IEvvGoodsItemDetail>();
		detailMock.Setup(m => m.Name).Returns("1");
		detailMock.Setup(m => m.Value).Returns("Test value");

		var wrapper = EVVGoodsItemDetailWrapper.New(detailMock.Object, Factory);

		AssertEquals("Valid Name", "1 - Description", wrapper.Name);
		AssertEquals("Value", "Test value", wrapper.Value);

		detailMock.Setup(m => m.Name).Returns("99");

		AssertEquals("Invalid Name", "99", wrapper.Name);
	});
}
