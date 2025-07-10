using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EnterpriseBusinessObjectFactoryExtensionsTest : TestCaseWithFactory
	{
		public void TestGetCachedCodeDescriptionPairList()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			CodeDescriptionPairList pairListForTest = factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AccountOrderType);
			Assert("Should return CodeDescriptionPairList", pairListForTest != null);

			CodeDescriptionPairList anotherPairListForTest = pairListForTest;
			pairListForTest = factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AccountOrderType);
			Assert("Should return same CodeDescriptionPairList when called again with same parameter", pairListForTest == anotherPairListForTest);
		}
	}
}
