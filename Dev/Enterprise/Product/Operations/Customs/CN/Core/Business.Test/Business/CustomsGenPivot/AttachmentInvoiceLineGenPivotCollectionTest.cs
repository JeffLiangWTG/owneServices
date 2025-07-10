using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AttachmentInvoiceLineGenPivotCollection))]
	sealed class AttachmentInvoiceLineGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationType()
		{
			var collection = GetCollectionToTest() as AttachmentInvoiceLineGenPivotCollection;
			AssertEquals("RelationType", GenPivotTypeDecider.Types.AttachmentInvoiceLineLink, collection.AddNew().XX_RelationType);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var group = Factory.New<CusStorageDocPivot>();
			return new AttachmentInvoiceLineGenPivotCollection(group);
		}
	}
}
