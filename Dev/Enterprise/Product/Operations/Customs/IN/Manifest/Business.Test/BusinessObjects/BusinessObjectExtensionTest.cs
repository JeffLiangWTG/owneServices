using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

sealed class BusinessObjectExtensionTest : TestCaseWithFactory
{
	public void TestAnyPropertyHasChanges()
	{
		var propertyNames = new[] { AsycudaManifestHeader.Schema.AMA_Voyage, AsycudaManifestHeader.Schema.AMA_GoodsDescription };

		AssertEquals("null bizObj", false, BusinessObjectExtension.AnyPropertyHasChanges(null, propertyNames));

		var header = Factory.New<CGMAsycudaManifestHeader>();
		CombineAssertions("Not in Database", AssertAnyPropertyHasChanges);

		Factory.Save();
		CombineAssertions("In Database", AssertAnyPropertyHasChanges);

		header.Delete();
		AssertEquals("Deleted", true, BusinessObjectExtension.AnyPropertyHasChanges(header, propertyNames));

		void AssertAnyPropertyHasChanges()
		{
			AssertEquals("value is empty", false, BusinessObjectExtension.AnyPropertyHasChanges(header, propertyNames));

			header.AMA_GoodsDescription = "Test";
			AssertEquals("AMA_GoodsDescription has change & AMA_Voyage has no change", true, BusinessObjectExtension.AnyPropertyHasChanges(header, propertyNames));
			AssertEquals("AMA_Voyage has no change", false, BusinessObjectExtension.AnyPropertyHasChanges(header, AsycudaManifestHeader.Schema.AMA_Voyage));
			AssertEquals("AMA_GoodsDescription has change", true, BusinessObjectExtension.AnyPropertyHasChanges(header, AsycudaManifestHeader.Schema.AMA_GoodsDescription));

			header.AMA_GoodsDescription = ZString.Empty;
			AssertEquals("AMA_GoodsDescription has no change", false, BusinessObjectExtension.AnyPropertyHasChanges(header, propertyNames));
		}
	}
}
