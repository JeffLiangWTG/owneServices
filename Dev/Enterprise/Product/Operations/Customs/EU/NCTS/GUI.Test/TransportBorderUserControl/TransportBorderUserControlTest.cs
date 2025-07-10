using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class TransportBorderUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
		}

		public void TestBorderTransportModeDropEdit()
		{
			var borderTransportModeDropEdit = control.BorderTransportModeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", borderTransportModeDropEdit);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_ExportTransportMode), borderTransportModeDropEdit.BindTo);
			});
		}

		public void TestBorderTransportTypeOfIdDropEdit()
		{
			var borderTransportTypeOfIdDropEdit = control.BorderTransportTypeOfIdDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", borderTransportTypeOfIdDropEdit);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_ActiveBorderIdentificationType), borderTransportTypeOfIdDropEdit.BindTo);
			});
		}

		public void TestBorderTransportIdAndNationalityUserControl()
		{
			AssertType<BorderTransportIdAndNationalityUserControl>(control.BorderTransportIdAndNationalityUserControl);
		}

		public void TestBorderConveyanceNumberTextBox()
		{
			var conveyanceNumberTextBox = control.BorderConveyanceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", conveyanceNumberTextBox);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_ConveyanceNumber), conveyanceNumberTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, conveyanceNumberTextBox.CharacterCasing);
			});
		}

		public void TestBorderOfficeDropEdit()
		{
			var officeCodeFindBox = control.BorderOfficeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", officeCodeFindBox);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_CustomsOfficeAtBorder), officeCodeFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", true, officeCodeFindBox.ShowDescriptionBox);
			});
		}

		public void TestAdditionalTransportBorderUserControl()
		{
			AssertType<AdditionalTransportBorderUserControl>(control.AdditionalTransportBorderUserControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportBorderUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		TransportBorderUserControl control;
	}
}
