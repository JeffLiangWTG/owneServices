using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageProductAttributesInfo))]
	public class PackageProductAttributesInfoTest : TestCase
	{
		public void TestSetters()
		{
			var info = new PackageProductAttributesInfo();
			AssertEquals(false, info.IsSameProductUsedOnAllPackages);
			AssertEquals(false, info.IsAttribute1Used);
			AssertEquals(false, info.IsSameAttribute1UsedOnAllPackages);
			AssertEquals(false, info.IsAttribute2Used);
			AssertEquals(false, info.IsSameAttribute2UsedOnAllPackages);
			AssertEquals(false, info.IsAttribute3Used);
			AssertEquals(false, info.IsSameAttribute3UsedOnAllPackages);
			AssertEquals(false, info.IsExpiryDateUsed);
			AssertEquals(false, info.IsSameExpiryDateUsedOnAllPackages);
			AssertEquals(false, info.IsPackingDateUsed);
			AssertEquals(false, info.IsSamePackingDateUsedOnAllPackages);
			AssertEquals(false, info.IsTrackedSerialUsed);

			info.IsSameProductUsedOnAllPackages = true;
			info.IsAttribute1Used = true;
			info.IsSameAttribute1UsedOnAllPackages = true;
			info.IsAttribute2Used = true;
			info.IsSameAttribute2UsedOnAllPackages = true;
			info.IsAttribute3Used = true;
			info.IsSameAttribute3UsedOnAllPackages = true;
			info.IsExpiryDateUsed = true;
			info.IsSameExpiryDateUsedOnAllPackages = true;
			info.IsPackingDateUsed = true;
			info.IsSamePackingDateUsedOnAllPackages = true;
			info.IsTrackedSerialUsed = true;

			AssertEquals(true, info.IsSameProductUsedOnAllPackages);
			AssertEquals(true, info.IsAttribute1Used);
			AssertEquals(true, info.IsSameAttribute1UsedOnAllPackages);
			AssertEquals(true, info.IsAttribute2Used);
			AssertEquals(true, info.IsSameAttribute2UsedOnAllPackages);
			AssertEquals(true, info.IsAttribute3Used);
			AssertEquals(true, info.IsSameAttribute3UsedOnAllPackages);
			AssertEquals(true, info.IsExpiryDateUsed);
			AssertEquals(true, info.IsSameExpiryDateUsedOnAllPackages);
			AssertEquals(true, info.IsPackingDateUsed);
			AssertEquals(true, info.IsSamePackingDateUsedOnAllPackages);
			AssertEquals(true, info.IsTrackedSerialUsed);
		}
	}
}
