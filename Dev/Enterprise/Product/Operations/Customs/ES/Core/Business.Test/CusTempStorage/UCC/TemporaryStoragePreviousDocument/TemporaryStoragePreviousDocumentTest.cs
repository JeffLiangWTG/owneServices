using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePreviousDocument))]
	sealed class TemporaryStoragePreviousDocumentTest : CusSupportingInfoTest<TemporaryStoragePreviousDocument>
	{
		public void TestCSI_ReferenceNumber2_Caption()
		{
			CombineAssertions(() =>
			{
				var resourceStringInfo = DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_ReferenceNumber2Info);
				AssertEquals("Caption", "Flight Number", resourceStringInfo.Caption);
				AssertEquals("MediumCaption", "Flight Number", resourceStringInfo.MediumCaption);
				AssertEquals("ShortCaption", "Flight Number", resourceStringInfo.ShortCaption);
				AssertEquals("FullDescription", "Number of Flight", resourceStringInfo.FullDescription);
			});
		}

		protected override IEnumerable<TemporaryStoragePreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var previousDocument = bill.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "A";
			yield return previousDocument;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => previousDocument;

		protected override BusinessObject GetNewBusinessObject() => previousDocument;

		protected override void SetUp()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			previousDocument = bill.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "A";
		}
		TemporaryStoragePreviousDocument previousDocument;
	}
}
