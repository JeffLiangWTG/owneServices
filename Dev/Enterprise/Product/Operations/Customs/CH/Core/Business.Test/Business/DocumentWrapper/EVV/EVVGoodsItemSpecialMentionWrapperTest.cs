using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemSpecialMentionWrapper))]
sealed class EVVGoodsItemSpecialMentionWrapperTest : TestCaseWithFactory
{
	public void TestNullConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => EVVGoodsItemSpecialMentionWrapper.New(null));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		var specialMentionMock = new Mock<IEvvGoodsItemSpecialMention>();
		specialMentionMock.Setup(m => m.SequenceNumber).Returns("1");
		specialMentionMock.Setup(m => m.Text).Returns("Test text");

		var wrapper = EVVGoodsItemSpecialMentionWrapper.New(specialMentionMock.Object);

		AssertEquals("Text", "Test text", wrapper.Text);

		specialMentionMock.Setup(m => m.Text).Returns(string.Empty);

		AssertEquals("Empty Text", ZString.Empty, wrapper.Text);
	});
}
