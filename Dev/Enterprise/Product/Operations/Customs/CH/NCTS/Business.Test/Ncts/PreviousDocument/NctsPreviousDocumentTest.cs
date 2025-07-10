using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPreviousDocument))]
sealed class NctsPreviousDocumentTest : CusSupportingInfoTest<NctsPreviousDocument>
{
	public void TestValidation() => AssertType<NctsPreviousDocumentValidation>(PreviousDocument.Validation);

	protected override BusinessObject GetNewBusinessObject() => CreateNctsPreviousDocument(Factory);

	protected override IEnumerable<NctsPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return CreateNctsPreviousDocument(factory);
	}

	NctsPreviousDocument PreviousDocument => previousDocument ?? (previousDocument = CreateNctsPreviousDocument(Factory));
	NctsPreviousDocument previousDocument;

	NctsPreviousDocument CreateNctsPreviousDocument(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var bill = header.Bills.AddNew();
		var nctsArrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
		return nctsArrivalCargoDesc.PreviousDocuments.AddNew();
	}
}
