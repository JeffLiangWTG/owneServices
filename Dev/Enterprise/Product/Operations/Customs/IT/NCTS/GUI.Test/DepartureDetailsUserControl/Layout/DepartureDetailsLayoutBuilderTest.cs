using CargoWise.Types;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(DepartureDetailsLayoutBuilder))]
sealed class DepartureDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DepartureDetailsLayoutBuilder, NctsHeader, EU.NCTS.GUI.DepartureDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 1;

	protected override DepartureDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new DepartureDetailsLayoutBuilder();

	public void TestDateLimitDateEditVisibility()
	{
		CombineAssertions(() =>
		{
			var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
			movementHeader.IsSimplifiedNctsProcedure = ZBool.False;
			AssertEquals("IsSimplifiedNctsProcedure: false, IsTIRDeclaration: false", true, layout.IsVisible(EU.NCTS.GUI.DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));

			movementHeader.IsSimplifiedNctsProcedure = ZBool.True;
			AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: false", true, layout.IsVisible(EU.NCTS.GUI.DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));

			movementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: true", false, layout.IsVisible(EU.NCTS.GUI.DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));
			AssertSequencesEqual("Visibility dependencies", new[] { movementHeader.BM_InBondEntryTypeInfo }, layout.GetVisibilityDependencies(EU.NCTS.GUI.DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
}
