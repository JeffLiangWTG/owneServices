using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class NctsArrivalUserControlTest : TestCaseWithFactory
	{
		public void TestSealsTabPage()
		{
			using (var control = new NctsArrivalUserControl())
			{
				control.SetDataBinding(header, "");
				AssertNotNull(control.FindSingle<ZTabPage>("SealsTabPage"));
			}
		}

		public void TestEnRouteSealsGrid()
		{
			using (var control = new NctsArrivalUserControl())
			{
				control.SetDataBinding(header, "");
				var sealsGrid = control.FindSingle<ZGrid>("EnRouteSealsGrid");
				var eventPlaceColumn = sealsGrid.GetColumnStyle(EnRouteSeal.Schema.BN_EventPlace);
				CombineAssertions(() =>
				{
					AssertEquals("BN_EventPlace characterCasing", System.Windows.Forms.CharacterCasing.Normal, eventPlaceColumn.CharacterCasing);
					AssertEquals("BN_EventPlace visible", true, eventPlaceColumn.IsVisible);
					AssertEquals("BN_EventCountryCode visible", true, sealsGrid.GetColumnStyle(EnRouteSeal.Schema.BN_EventCountryCode).IsVisible);
					AssertEquals("BN_NoOfSeals visible", true, sealsGrid.GetColumnStyle(EnRouteSeal.Schema.BN_NoOfSeals).IsVisible);
				});
			}
		}

		public void TestEnRouteIncidentsGrid()
		{
			using (var control = new NctsArrivalUserControl())
			{
				control.SetDataBinding(header, "");
				var grid = control.FindSingle<ZGrid>("EnRouteIncidentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("BN_EventPlace characterCasing", System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle(EnRouteIncident.Schema.BN_EventPlace).CharacterCasing);
					AssertEquals("BN_Information characterCasing", System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle(EnRouteIncident.Schema.BN_Information).CharacterCasing);
					AssertEquals("BN_EndorsementAuthority characterCasing", System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle(EnRouteIncident.Schema.BN_EndorsementAuthority).CharacterCasing);
					AssertEquals("BN_EndorsementPlace characterCasing", System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle(EnRouteIncident.Schema.BN_EndorsementPlace).CharacterCasing);
				});
			}
		}

		public void TestEnRouteTransshipmentsGrid()
		{
			using (var control = new NctsArrivalUserControl())
			{
				control.SetDataBinding(header, "");
				var grid = control.FindSingle<ZGrid>("EnRouteTransshipmentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("BN_EventPlace characterCasing", System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle(EnRouteTransshipment.Schema.BN_EventPlace).CharacterCasing);
					AssertEquals("BN_TransportID characterCasing", System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle(EnRouteTransshipment.Schema.BN_TransportID).CharacterCasing);
					AssertEquals("BN_EndorsementAuthority characterCasing", System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle(EnRouteTransshipment.Schema.BN_EndorsementAuthority).CharacterCasing);
					AssertEquals("BN_EndorsementPlace characterCasing", System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle(EnRouteTransshipment.Schema.BN_EndorsementPlace).CharacterCasing);
				});
			}
		}

		public void TestCustomerReferenceNumber()
		{
			using (var control = new NctsArrivalUserControl())
			{
				control.SetDataBinding(header, "");
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, control.FindSingle<ZTextBox>("zTextBoxCRN").CharacterCasing);
			}
		}

		public void TestSealNumberGrid()
		{
			using (var control = new NctsArrivalUserControl())
			{
				control.SetDataBinding(header, "");
				var sealNumbersGrid = control.FindSingle<ZGrid>("SealNumbersGrid");
				CombineAssertions(() =>
				{
					AssertEquals("BC_Seal1", true, sealNumbersGrid.GetColumnStyle(SealContainer.Schema.BC_Seal1).IsVisible);
				});
			}
		}

		public void TestDestinationCustomsOfficeCodeFindBoxisBindToDestinationCustomsOfficeCodeForArrival()
		{
			using (var control = new NctsArrivalUserControl())
			{
				control.SetDataBinding(header, "");
				var desCusFindBox = control.FindSingleOrDefault<ZCodeFindBox>("DestinationCustomsOfficeCodeCodeFindBox");
				AssertNotNull(desCusFindBox);

				AssertEquals("DestinationCustomsOfficeCodeForArrival", desCusFindBox.BindTo);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
		}
		NctsHeader header;
	}
}
