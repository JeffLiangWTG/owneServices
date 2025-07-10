using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(CommonPreviousDocument))]
	class CommonPreviousDocumentTest : CusSupportingInfoTest<CommonPreviousDocument>
	{
		public void TestValidation()
		{
			AssertType<CommonPreviousDocumentValidation>(previousDocument.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => CreatePreviousDocument(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreatePreviousDocument(Factory);

		protected override IEnumerable<CommonPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreatePreviousDocument(factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = CreatePreviousDocument(Factory);
		}
		CommonPreviousDocument previousDocument;

		static CommonPreviousDocument CreatePreviousDocument(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.PreviousDocuments.AddNew();
		}
	}
}
