using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVPreviousDocumentWrapperCollection))]
class EVVPreviousDocumentWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVPreviousDocumentWrapperCollection>
{
	protected override EVVPreviousDocumentWrapperCollection GetCollectionToTest() => EVVPreviousDocumentWrapperCollection.New(null, Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var previousDocumentMock = new Mock<IEvvPreviousDocument>();
		return EVVPreviousDocumentWrapper.New(previousDocumentMock.Object, Factory);
	}
}
