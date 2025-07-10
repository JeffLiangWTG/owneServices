using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public class NctsDepartureMovementHeaderForTest : NctsDepartureMovementHeader
{
	public NctsDepartureMovementHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new INctsGuaranteeCollection<NctsGuaranteeForTest> Guarantees => (INctsGuaranteeCollection<NctsGuaranteeForTest>)base.Guarantees;

	protected override INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuaranteeForTest>(this);

	protected override bool IsGrossWeightUQReadOnlyCore => false;
}
