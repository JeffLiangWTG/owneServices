using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVLegalAdvisoryWrapperCollection))]
sealed class EVVLegalAdvisoryWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVLegalAdvisoryWrapperCollection>
{
	protected override EVVLegalAdvisoryWrapperCollection GetCollectionToTest() => EVVLegalAdvisoryWrapperCollection.New(null, Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var legalAdvisoryMock = new Mock<IEvvLegalAdvisory>();
		return EVVLegalAdvisoryWrapper.New(legalAdvisoryMock.Object, Factory);
	}
}
