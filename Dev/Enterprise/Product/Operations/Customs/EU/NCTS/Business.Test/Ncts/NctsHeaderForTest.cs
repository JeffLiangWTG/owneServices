using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public class NctsHeaderForTest : NctsHeader
{
	public NctsHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new INctsGuaranteeCollection<NctsGuaranteeForTest> Guarantees => (INctsGuaranteeCollection<NctsGuaranteeForTest>)base.Guarantees;

	protected override INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new NctsGuaranteeCollection<NctsGuaranteeForTest>(this);

	protected override ZInt MaximumGuaranteeCountCore => 1;

	public void SetDefaultValuesExposed() => SetDefaultValues();

	protected override ZString DefaultApplicationCode => CusInBondApplicationCodeList.Codes.NCTS4;

	public void ResetDefaultPrincipalRegistryManagerExposed() => ResetDefaultPrincipalRegistryManager();
	public void ResetDefaultConsigneeConsignorRegistryManagerExposed() => ResetDefaultConsignorConsigneeRegistryManager();

	public bool IsConditionR0520_UserShouldNotSaveAmendmentsForTest { get; set; }
	protected override bool IsConditionR0520_UserShouldNotSaveAmendmentsCore() => IsConditionR0520_UserShouldNotSaveAmendmentsForTest;

	public bool AllArrivalSealStateAreDECForTest { get; set; }
	protected override bool AllArrivalSealStateAreDECCore => AllArrivalSealStateAreDECForTest;

	public bool ExistNonDECEntryCoreForTest { get; set; }
	protected override bool ExistNonDECEntryCore => ExistNonDECEntryCoreForTest;

	public bool IsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesForTest { get; set; }
	protected override bool IsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesCore() => IsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesForTest;
}
