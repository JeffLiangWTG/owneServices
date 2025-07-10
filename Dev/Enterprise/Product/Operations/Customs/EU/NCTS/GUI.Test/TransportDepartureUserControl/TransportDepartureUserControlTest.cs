using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class TransportDepartureUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(IDepartureTransportMeansProvider), control.BindingSource.DataSourceType);
		}

		public void TestInlandTransportModeDropEdit()
		{
			var inlandTransportModeDropEdit = control.InlandTransportModeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", inlandTransportModeDropEdit);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.InlandTransportModeAtDeparture), inlandTransportModeDropEdit.BindTo);
			});
		}

		public void TestTransportAtDepartureTextBox()
		{
			var transportAtDepartureTextBox = control.TransportAtDepartureTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", transportAtDepartureTextBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.TransportAtDeparture), transportAtDepartureTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, transportAtDepartureTextBox.CharacterCasing);
			});
		}

		public void TestTransportAtDepartureCountryCodeFindBox()
		{
			var transportAtDepartureCountryCodeFindBox = control.TransportAtDepartureCountryCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", transportAtDepartureCountryCodeFindBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.TransportCountryAtDeparture), transportAtDepartureCountryCodeFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", false, transportAtDepartureCountryCodeFindBox.ShowDescriptionBox);
				AssertEquals("ModuleID", ZArchitecture.Modules.ModuleId.ZZRefCusCodeList, transportAtDepartureCountryCodeFindBox.ModuleID.ID);
				AssertEquals("CodeBox.MaxWidth", 22, transportAtDepartureCountryCodeFindBox.CodeBox.MaximumSize.Width);
			});
		}

		public void TestTransportAtDepartureTrailer1RegNoTextBox()
		{
			var transportAtDepartureTrailer1RegNoTextBox = control.TransportAtDepartureTrailer1RegNoTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", transportAtDepartureTrailer1RegNoTextBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.Trailer1IDAtDeparture), transportAtDepartureTrailer1RegNoTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, transportAtDepartureTrailer1RegNoTextBox.CharacterCasing);
			});
		}

		public void TestTransportAtDepartureTrailer1NationalityCodeFindBox()
		{
			var transportAtDepartureTrailer1NationalityCodeFindBox = control.TransportAtDepartureTrailer1NationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", transportAtDepartureTrailer1NationalityCodeFindBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.Trailer1NationalityAtDeparture), transportAtDepartureTrailer1NationalityCodeFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", false, transportAtDepartureTrailer1NationalityCodeFindBox.ShowDescriptionBox);
				AssertEquals("ModuleID", ZArchitecture.Modules.ModuleId.ZZRefCusCodeList, transportAtDepartureTrailer1NationalityCodeFindBox.ModuleID.ID);
				AssertEquals("CodeBox.MaxWidth", 22, transportAtDepartureTrailer1NationalityCodeFindBox.CodeBox.MaximumSize.Width);
			});
		}

		public void TestTransportAtDepartureTrailer2RegNoTextBox()
		{
			var transportAtDepartureTrailer2RegNoTextBox = control.TransportAtDepartureTrailer2RegNoTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", transportAtDepartureTrailer2RegNoTextBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.Trailer2IDAtDeparture), transportAtDepartureTrailer2RegNoTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, transportAtDepartureTrailer2RegNoTextBox.CharacterCasing);
			});
		}

		public void TestTransportAtDepartureTrailer2NationalityCodeFindBox()
		{
			var transportAtDepartureTrailer2NationalityCodeFindBox = control.TransportAtDepartureTrailer2NationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", transportAtDepartureTrailer2NationalityCodeFindBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.Trailer2NationalityAtDeparture), transportAtDepartureTrailer2NationalityCodeFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", false, transportAtDepartureTrailer2NationalityCodeFindBox.ShowDescriptionBox);
				AssertEquals("ModuleID", ZArchitecture.Modules.ModuleId.ZZRefCusCodeList, transportAtDepartureTrailer2NationalityCodeFindBox.ModuleID.ID);
				AssertEquals("CodeBox.MaxWidth", 22, transportAtDepartureTrailer2NationalityCodeFindBox.CodeBox.MaximumSize.Width);
			});
		}

		public void TestTransportAtDepartureTypeDropEdit()
		{
			var transportAtDepartureTypeDropEdit = control.TransportAtDepartureTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", transportAtDepartureTypeDropEdit);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.TransportTypeAtDeparture), transportAtDepartureTypeDropEdit.BindTo);
			});
		}

		public void TestAircraftIDAtDepartureTextBox()
		{
			var aircraftIDAtDepartureTextBox = control.AircraftIDAtDepartureTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", aircraftIDAtDepartureTextBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.AircraftIDAtDeparture), aircraftIDAtDepartureTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, aircraftIDAtDepartureTextBox.CharacterCasing);
			});
		}

		public void TestVesselCodeFindBox()
		{
			var vesselCodeFindBox = control.VesselCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", vesselCodeFindBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.VesselNameAtDeparture), vesselCodeFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", false, vesselCodeFindBox.ShowDescriptionBox);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, vesselCodeFindBox.CodeBox.CharacterCasing);
			});
		}

		public void TestVesselCountryCodeFindBox()
		{
			var vesselCountryCodeFindBox = control.VesselCountryCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", vesselCountryCodeFindBox);
				AssertEquals("BindTo", nameof(IDepartureTransportMeansProvider.VesselCountryAtDeparture), vesselCountryCodeFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", false, vesselCountryCodeFindBox.ShowDescriptionBox);
				AssertEquals("ModuleID", ZArchitecture.Modules.ModuleId.ZZRefCusCodeList, vesselCountryCodeFindBox.ModuleID.ID);
				AssertEquals("CodeBox.MaxWidth", 22, vesselCountryCodeFindBox.CodeBox.MaximumSize.Width);
			});
		}

		public void TestAdditionalWagonNumbersButton()
		{
			var additionalWagonNumbersButton = control.AdditionalWagonNumbersButton;
			CombineAssertions(() =>
			{
				AssertType<ZButton>("Type", additionalWagonNumbersButton);
				AssertEquals("Caption", "More..", additionalWagonNumbersButton.CaptionResourceString.Caption);
				AssertEquals("ToolTipCaption", "Additional Wagon Numbers", additionalWagonNumbersButton.ToolTipCaption.ToString());
			});
		}

		[RequiresSTA]
		public void TestAdditionalWagonNumbersButton_Click()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			using (var form = new ZForm(nctsHeader.MovementHeader))
			{
				form.Controls.Add(control);
				form.Show();
				control.AdditionalWagonNumbersButton.PerformClick();
				AssertType<AdditionalWagonNumbersForm>(ZFormModaliser.LastFormShownForTest);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDepartureUserControl();
		}
		TransportDepartureUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
