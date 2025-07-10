using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSPackage))]
	class EMCSPackageTest : CusInvPackTest<EMCSJobDeclaration>
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			return declaration.EMCSPackages.AddNew();
		}
	}
}
