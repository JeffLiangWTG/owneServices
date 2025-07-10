using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(MovementReferenceNumberSupportingInfoCollection))]
sealed class MovementReferenceNumberSupportingInfoCollectionTest : CusSupportingInfoCollectionTest<MovementReferenceNumberSupportingInfo>
{
	protected override CusSupportingInfoCollection<MovementReferenceNumberSupportingInfo> GetCusSupportingInfoCollection()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers;
	}
}
