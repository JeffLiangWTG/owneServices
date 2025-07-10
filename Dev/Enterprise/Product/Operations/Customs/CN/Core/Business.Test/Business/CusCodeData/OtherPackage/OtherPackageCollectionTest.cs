using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(OtherPackageCollection))]
	class OtherPackageCollectionTest : CusCodeDataCollectionTest<OtherPackage>
	{
		protected override CusCodeDataCollection<OtherPackage> GetCusCodeDataCollection()
		{
			return new OtherPackageCollection(Factory.New<CusEntryInstruction>());
		}
	}
}
