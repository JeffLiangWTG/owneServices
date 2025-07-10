using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusTempStorageSelectableRegLineCollection))]
	sealed class CusTempStorageSelectableRegLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusTempStorageSelectableRegLineCollection>
	{
		protected override CusTempStorageSelectableRegLineCollection GetCollectionToTest()
		{
			return new CusTempStorageSelectableRegLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var regLineMock = new Mock<ICusTempStorageRegLine>();
			return new CusTempStorageSelectableRegLine(regLineMock.Object);
		}
	}
}
