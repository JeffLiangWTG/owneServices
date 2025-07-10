using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondUnionCollectionForSeaCargo))]
	class CusUnderbondUnionCollectionForSeaCargoTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestGetListOfAllPossibleProvidersWithNull()
		{
			var parentMock = new Mock<IAUCusUnderbondUnionCollectionParent>();
			var dependentCollection = new ICusUnderbondDependentCollectionParent[] { null };
			parentMock.Setup(m => m.GetAllPossibleCollectionProviders()).Returns(dependentCollection);
			new CusUnderbondUnionCollectionForSeaCargo(parentMock.Object).Load();
		}

		[ExpectNoExceptions]
		public void TestQueryProcessorShouldNotRunOutOfInternalResources()
		{
			var dependentCollection = Enumerable.Range(0, 50_000).Select(x =>
			{
				var bizObjMock = new Mock<ICusUnderbondDependentCollectionParent>();
				bizObjMock.Setup(m => m.Identifier).Returns(ZGuid.NewZGuid());
				return bizObjMock.Object;
			}).ToArray();
			var parentMock = new Mock<IAUCusUnderbondUnionCollectionParent>();
			parentMock.Setup(m => m.Factory).Returns(new BusinessObjectFactory());
			parentMock.Setup(m => m.GetAllPossibleCollectionProviders()).Returns(dependentCollection);
			new CusUnderbondUnionCollectionForSeaCargo(parentMock.Object).Load();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusUnderbondUnionCollectionForSeaCargo(Factory.New<CusSCAOceanBill>());
		}
	}
}
