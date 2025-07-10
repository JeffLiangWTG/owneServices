using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(CommonPreviousDocument))]
sealed class CommonPreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<CommonPreviousDocument>
{
	public void TestValidationType()
	{
		AssertType<CommonPreviousDocumentValidation>(previousDocument.Validation);
	}

	protected override void SetUp()
	{
		base.SetUp();
		previousDocument = GetNewPreviousDocument(Factory);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return previousDocument;
	}

	protected override IEnumerable<CommonPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewPreviousDocument(factory);
	}

	CommonPreviousDocument GetNewPreviousDocument(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var bill = nctsHeader.Bills.AddNew();
		return bill.PreviousDocuments.AddNew();
	}

	CommonPreviousDocument previousDocument;
}
