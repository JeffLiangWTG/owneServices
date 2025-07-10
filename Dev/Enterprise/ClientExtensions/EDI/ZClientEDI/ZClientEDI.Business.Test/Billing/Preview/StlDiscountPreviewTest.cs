using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlDiscountPreview))]
	internal class StlDiscountPreviewTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClone()
		{
			var src = new StlDiscountPreview();
			src.Name = "123a";
			src.Percent = 10.99m;
			src.IsActive = true;
			src.Percent_ReadOnly = true;

			var dest = src.Clone();
			AssertEquals("123a", dest.Name);
			AssertEquals(10.99m, dest.Percent);
			AssertEquals(true, dest.IsActive);
			AssertEquals(true, dest.Percent_ReadOnly);

			AssertNotEquals(src.GetHashCode(), dest.GetHashCode());
		}
	}
}
