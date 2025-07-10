using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemSpecialMentionWrapperCollection))]
sealed class EVVGoodsItemSpecialMentionWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVGoodsItemSpecialMentionWrapperCollection>
{
	protected override EVVGoodsItemSpecialMentionWrapperCollection GetCollectionToTest() => EVVGoodsItemSpecialMentionWrapperCollection.New(null, Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var specialMentionMock = new Mock<IEvvGoodsItemSpecialMention>();
		return EVVGoodsItemSpecialMentionWrapper.New(specialMentionMock.Object);
	}
}
