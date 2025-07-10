using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(BaseCusContainerCollection<BaseCusContainer>))]
	class CusContainerCollectionTest : Customs.Business.Testing.CusContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new BaseCusContainerCollection<BaseCusContainer>(Factory.New<JobDeclaration>(), Factory);
	}
}
