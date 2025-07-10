using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVLegalAdvisoryWrapper))]
sealed class EVVLegalAdvisoryWrapperTest : TestCaseWithFactory
{
	public void TestNew() => CombineAssertions(() =>
	{
		var legalAdvisory = new Mock<IEvvLegalAdvisory>();
		AssertExceptionThrown<ArgumentNullException>("Null message", () => EVVLegalAdvisoryWrapper.New(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("Null factory", () => EVVLegalAdvisoryWrapper.New(legalAdvisory.Object, null));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		LegalAdvisoryMock.Setup(x => x.SequenceNumber).Returns(1);
		LegalAdvisoryMock.Setup(x => x.Title).Returns("Hinweis:");
		LegalAdvisoryMock.Setup(x => x.Text).Returns("Text");

		AssertEquals("SequenceNumber", 1, LegalAdvisoryWrapper.SequenceNumber);
		AssertEquals("Title", "Hinweis:", LegalAdvisoryWrapper.Title);
		AssertEquals("Text", "Text", LegalAdvisoryWrapper.Text);
	});

	Mock<IEvvLegalAdvisory> LegalAdvisoryMock => legalAdvisoryMock ??= new Mock<IEvvLegalAdvisory>();
	Mock<IEvvLegalAdvisory> legalAdvisoryMock;

	EVVLegalAdvisoryWrapper LegalAdvisoryWrapper => legalAdvisoryWrapper ??= EVVLegalAdvisoryWrapper.New(LegalAdvisoryMock.Object, Factory);
	EVVLegalAdvisoryWrapper legalAdvisoryWrapper;
}
