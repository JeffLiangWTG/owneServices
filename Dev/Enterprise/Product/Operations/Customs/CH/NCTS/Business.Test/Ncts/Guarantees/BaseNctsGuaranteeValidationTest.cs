using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BaseNctsGuaranteeValidationTest : BusinessObjectValidationTestCase
{
	NctsGuarantee CreateGuarantee()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(MovementType);
		return (NctsGuarantee)(header.MovementHeader?.Guarantees.AddNew() ?? header.ArrivalMovementHeader.GuaranteesForArrival.AddNew());
	}

	protected NctsGuarantee Guarantee => guarantee ??= CreateGuarantee();
	NctsGuarantee guarantee;

	protected abstract string MovementType { get; }
}
