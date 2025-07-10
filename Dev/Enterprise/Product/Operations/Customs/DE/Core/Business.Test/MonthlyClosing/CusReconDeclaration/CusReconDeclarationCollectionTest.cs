using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusReconDeclarationCollection))]
	class CusReconDeclarationCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconDeclarationCollection>
	{
		protected override CusReconDeclarationCollection GetCollectionToTest() => new CusReconDeclarationCollection(Factory);
	}
}
