using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CommonPreviousDocument))]
public class CommonPreviousDocumentTest : CusSupportingInfoTest<CommonPreviousDocument>
{
	public void TestGetNewValidation()
	{
		AssertType<CommonPreviousDocumentValidation>(DepartureBillPreviousDocument.Validation);
	}

	protected override IEnumerable<CommonPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
		yield return (CommonPreviousDocument)nctsHeader.PreviousDocuments.AddNew();
		yield return (CommonPreviousDocument)nctsHeader.Bills.AddNew().PreviousDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
		return (CommonPreviousDocument)nctsHeader.PreviousDocuments.AddNew();
	}
	NctsHeader DepartureNctsHeader => departureNctsHeader ??= CreateNctsHeader(NctsMovementType.Codes.Departure);
	NctsHeader departureNctsHeader;

	NctsHeader CreateNctsHeader(string movementType)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		return nctsHeader;
	}

	NctsBill DepartureNctsBill => departureNctsBill ??= DepartureNctsHeader.Bills.AddNew();
	NctsBill departureNctsBill;

	CommonPreviousDocument DepartureBillPreviousDocument => departureBillPreviousDocument ??= (CommonPreviousDocument)DepartureNctsBill.PreviousDocuments.AddNew();
	CommonPreviousDocument departureBillPreviousDocument;
}
