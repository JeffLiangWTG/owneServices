using System;
using System.Globalization;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemPermitWrapper))]
sealed class EVVGoodsItemPermitWrapperTest : TestCaseWithFactory
{
	public void TestNullConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null detail", () => EVVGoodsItemPermitWrapper.New(null, Factory));

		var permitMock = new Mock<IEvvGoodsItemPermit>();
		AssertExceptionThrown<ArgumentNullException>("Null factory", () => EVVGoodsItemPermitWrapper.New(permitMock.Object, null));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreatePermitTypeCodeList(Factory);
		RefCusCodeTestHelper.CreatePermitAuthorityCodeList(Factory);

		var permitMock = new Mock<IEvvGoodsItemPermit>();
		permitMock.Setup(m => m.PermitType).Returns("23");
		permitMock.Setup(m => m.PermitAuthority).Returns("12");
		permitMock.Setup(m => m.PermitNumber).Returns("ABC123");
		permitMock.Setup(m => m.IssueDate).Returns(new DateTime(2024, 11, 18));
		permitMock.Setup(m => m.AdditionalInformation).Returns("Info");

		var wrapper = EVVGoodsItemPermitWrapper.New(permitMock.Object, Factory);

		AssertEquals("PermitType", "ValidPermitTypeCode", wrapper.PermitType);
		AssertEquals("PermitAuthority", "ValidPermitAuthorityCode", wrapper.PermitAuthority);
		AssertEquals("PermitNumber", "ABC123", wrapper.PermitNumber);
		AssertEquals("IssueDate", new DateTime(2024, 11, 18).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), wrapper.IssueDate);
		AssertEquals("AdditionalInformation", "Info", wrapper.AdditionalInformation);

		permitMock.Setup(m => m.PermitType).Returns("56");
		permitMock.Setup(m => m.PermitAuthority).Returns("45");

		AssertEquals("PermitType - Unknown code", "56", wrapper.PermitType);
		AssertEquals("PermitAuthority - Unknown code", "45", wrapper.PermitAuthority);
	});

	public void TestEmptyProperties() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreatePermitTypeCodeList(Factory);
		RefCusCodeTestHelper.CreatePermitAuthorityCodeList(Factory);

		var permitMock = new Mock<IEvvGoodsItemPermit>();
		permitMock.Setup(m => m.PermitType).Returns(string.Empty);
		permitMock.Setup(m => m.PermitAuthority).Returns(string.Empty);
		permitMock.Setup(m => m.PermitNumber).Returns(string.Empty);
		permitMock.Setup(m => m.IssueDate).Returns(null as DateTime?);

		var wrapper = EVVGoodsItemPermitWrapper.New(permitMock.Object, Factory);

		AssertEquals("PermitType", "---", wrapper.PermitType);
		AssertEquals("PermitAuthority", "---", wrapper.PermitAuthority);
		AssertEquals("PermitNumber", "---", wrapper.PermitNumber);
		AssertEquals("IssueDate", "---", wrapper.IssueDate);
		AssertEquals("AdditionalInformation", "---", wrapper.AdditionalInformation);
	});
}
