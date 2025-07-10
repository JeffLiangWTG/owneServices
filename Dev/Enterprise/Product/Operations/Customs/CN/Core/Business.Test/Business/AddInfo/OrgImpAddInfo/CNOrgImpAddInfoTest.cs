using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNOrgImpAddInfo))]
	class CNOrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZO_IntelligentDeclarationType()
		{
			AssertEquals(1, CNOrgImpAddInfo.Get(Factory.New<OrgHeader>()).ZO_IntelligentDeclarationTypeInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => CNOrgImpAddInfo.Get(Factory.New<OrgHeader>());
	}
}
