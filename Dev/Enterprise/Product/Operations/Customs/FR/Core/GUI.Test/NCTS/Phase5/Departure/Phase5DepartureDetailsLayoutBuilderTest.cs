using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Phase5
{
	[TestedType(typeof(DepartureDetailsLayoutBuilder<NctsHeader>))]
	sealed class Phase5DepartureDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EU.NCTS.GUI.DepartureDetailsLayoutBuilder<NctsHeader>, NctsHeader, DepartureDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override EU.NCTS.GUI.DepartureDetailsLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting()
			=> new DepartureDetailsLayoutBuilder<NctsHeader>();

		public void TestDateLimitDateEditVisibility()
		{
			CombineAssertions(() =>
			{
				var layout = ((ZArchitecture.GUI.IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
				movementHeader.IsSimplifiedNctsProcedure = ZBool.False;
				AssertEquals("IsSimplifiedNctsProcedure: false, IsTIRDeclaration: false", true, layout.IsVisible(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));

				movementHeader.IsSimplifiedNctsProcedure = ZBool.True;
				AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: false", true, layout.IsVisible(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));

				movementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				AssertEquals("IsTIRDeclaration: true", false, layout.IsVisible(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));
				AssertSequencesEqual("Visibility dependencies", new[] { movementHeader.BM_InBondEntryTypeInfo }, layout.GetVisibilityDependencies(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));
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
}
