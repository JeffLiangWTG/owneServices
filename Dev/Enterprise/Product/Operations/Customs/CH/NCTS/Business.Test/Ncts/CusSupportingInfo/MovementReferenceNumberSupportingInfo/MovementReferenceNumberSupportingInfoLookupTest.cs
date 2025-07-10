using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(MovementReferenceNumberSupportingInfo))]
sealed class MovementReferenceNumberSupportingInfoLookupTest : EnterpriseBusinessObjectTestCaseWithListChecking<MovementReferenceNumberSupportingInfo>
{
	public void TestYesNoList() => CombineAssertions(() =>
	{
		AssertEquals("Codes", "N, Y", MRN.Lookups.YesNoList.CodesAsString);
		AssertSame("Cached", MRN.Lookups.YesNoList, MRN.Lookups.YesNoList);
	});

	protected override BusinessObject GetNewBusinessObject() => CreateMRN();

	MovementReferenceNumberSupportingInfo MRN => mrn ?? (mrn = CreateMRN());
	MovementReferenceNumberSupportingInfo mrn;

	MovementReferenceNumberSupportingInfo CreateMRN()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
	}
}
