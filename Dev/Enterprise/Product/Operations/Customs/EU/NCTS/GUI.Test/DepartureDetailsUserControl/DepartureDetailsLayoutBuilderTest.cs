using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(DepartureDetailsLayoutBuilder<NctsHeader>))]
	sealed class DepartureDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DepartureDetailsLayoutBuilder<NctsHeader>, NctsHeader, DepartureDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override DepartureDetailsLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new DepartureDetailsLayoutBuilder<NctsHeader>();

		public void TestTirCarnetNumberTextBoxVisibility()
		{
			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
				AssertEquals("BM_InBondEntryType is empty", false, layout.IsVisible(DepartureDetailsControlBag.Instance.TirCarnetNumberTextBox, nctsHeader));

				movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				AssertEquals("BM_InBondEntryType is 'TIR'", true, layout.IsVisible(DepartureDetailsControlBag.Instance.TirCarnetNumberTextBox, nctsHeader));

				movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				AssertEquals("BM_InBondEntryType isn't empty or 'TIR'", false, layout.IsVisible(DepartureDetailsControlBag.Instance.TirCarnetNumberTextBox, nctsHeader));
				AssertSequencesEqual("Visibility dependencies", new[] { movementHeader.BM_InBondEntryTypeInfo }, layout.GetVisibilityDependencies(DepartureDetailsControlBag.Instance.TirCarnetNumberTextBox, nctsHeader));
			});
		}

		public void TestDateLimitDateEditVisibility()
		{
			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
				movementHeader.IsSimplifiedNctsProcedure = ZBool.False;
				AssertEquals("IsSimplifiedNctsProcedure: false, IsTIRDeclaration: false", false, layout.IsVisible(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));

				movementHeader.IsSimplifiedNctsProcedure = ZBool.True;
				AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: false", true, layout.IsVisible(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));

				movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				AssertEquals("IsSimplifiedNctsProcedure: true, IsTIRDeclaration: true", false, layout.IsVisible(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));
				AssertSequencesEqual("Visibility dependencies", new[] { movementHeader.IsSimplifiedNctsProcedureInfo, movementHeader.BM_InBondEntryTypeInfo }, layout.GetVisibilityDependencies(DepartureDetailsControlBag.Instance.DateLimitDateEdit, nctsHeader));
			});
		}

		public void TestSimplifiedProcedureAndReducedDataSetUserControlVisibility()
		{
			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
				AssertEquals("IsTIRDeclaration: false", true, layout.IsVisible(DepartureDetailsControlBag.Instance.SimplifiedProcedureAndReducedDataSetUserControl, nctsHeader));

				movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				AssertEquals("IsTIRDeclaration: true", false, layout.IsVisible(DepartureDetailsControlBag.Instance.SimplifiedProcedureAndReducedDataSetUserControl, nctsHeader));
				AssertSequencesEqual("Visibility dependencies", new[] { movementHeader.BM_InBondEntryTypeInfo }, layout.GetVisibilityDependencies(DepartureDetailsControlBag.Instance.SimplifiedProcedureAndReducedDataSetUserControl, nctsHeader));
			});
		}

		public void TestAdditionalDeclarationTypeDropEditVisibility()
		{
			AssertAdditionalDeclarationTypeDropEditVisibility(false, false);
			AssertAdditionalDeclarationTypeDropEditVisibility(true, true);
		}

		void AssertAdditionalDeclarationTypeDropEditVisibility(ZBool inputVisibility, bool expectedResult)
		{
			var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
			nctsConfigurationMock.Protected().Setup<ZBool>("UseAdditionalDeclarationTypeCore").Returns(inputVisibility);
			nctsHeader.Factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject()).Returns(nctsConfigurationMock.Object);

			var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object } };
			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
				AssertEquals("AdditionalDeclarationTypeDropEdit Visibility", expectedResult, layout.IsVisible(DepartureDetailsControlBag.Instance.AdditionalDeclarationTypeDropEdit, nctsHeader));
			}
		}

		public void TestPresentationDateTimeVisibility()
		{
			AssertPresentationDateTimeVisibility(false, false);
			AssertPresentationDateTimeVisibility(true, true);
		}

		void AssertPresentationDateTimeVisibility(ZBool inputVisibility, bool expectedResult)
		{
			var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
			nctsConfigurationMock.Protected().Setup<ZBool>("UsePresentationDateTimeCore").Returns(inputVisibility);
			nctsHeader.Factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject()).Returns(nctsConfigurationMock.Object);

			var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object } };
			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
				AssertEquals("PresentationDateTime Visibility", expectedResult, layout.IsVisible(DepartureDetailsControlBag.Instance.PresentationDateTimeOffsetEdit, nctsHeader));
			}
		}

		public void TestOverrideFreightDetailsCheckBoxVisibility()
		{
			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new Phase5DepartureDetailsLayout()).Layout;
				AssertEquals("IsPluggedIn: false", false, layout.IsVisible(DepartureDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, nctsHeader));

				var shipment = Factory.New<ForwardingShipment>();
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				AssertEquals("IsPluggedIn - shipment: true", true, layout.IsVisible(DepartureDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, nctsHeader));

				var consol = Factory.New<ForwardingConsol>();
				nctsHeader.BH_ParentID = consol.PK;
				nctsHeader.BH_ParentTableCode = consol.TablePrefix;
				AssertEquals("IsPluggedIn - consol: true", true, layout.IsVisible(DepartureDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, nctsHeader));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}
}
