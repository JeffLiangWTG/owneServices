using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CommonPreviousDocument))]
	sealed class CommonPreviousDocumentTest : CusSupportingInfoTest<CommonPreviousDocument>
	{
		public void TestValidation()
		{
			AssertType<CommonPreviousDocumentValidation>(document.Validation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = GetNewPreviousDocument(Factory);
		}

		protected override IEnumerable<CommonPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsHeader.Bills.AddNew();
			yield return bill.PreviousDocuments.AddNew();
			yield return nctsHeader.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => document;

		CommonPreviousDocument GetNewPreviousDocument(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsHeader.Bills.AddNew();
			return bill.PreviousDocuments.AddNew();
		}

		CommonPreviousDocument document;
	}
}
