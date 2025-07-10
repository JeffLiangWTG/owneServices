using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPreviousDocument))]
	sealed class NctsPreviousDocumentTest : CusSupportingInfoTest<NctsPreviousDocument>
	{
		public void TestValidation()
		{
			AssertType<NctsPreviousDocumentPhase5Validation>(previousDocument.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => CreatePreviousDocument(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreatePreviousDocument(Factory);

		protected override IEnumerable<NctsPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreatePreviousDocument(factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = CreatePreviousDocument(Factory);
		}
		NctsPreviousDocument previousDocument;

		NctsPreviousDocument CreatePreviousDocument(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var goodItem = bill.GoodsItems.AddNew();
			return goodItem.PreviousDocuments.AddNew();
		}
	}
}
