using System.Collections.Generic;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(IMPDeclarationTransportDetailsLayout))]
	sealed class IMPDeclarationTransportDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		protected override IPanelLayoutProvider GetNewPanelLayoutProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			return new IMPDeclarationTransportDetailsLayout(declaration);
		}

		public new void TestIncludedControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertSeaIncludedControls(declaration);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertAirIncludedControls(declaration);
		}

		void AssertSeaIncludedControls(JobDeclaration declaration)
		{
			IPanelLayoutProvider seaLayout = new IMPDeclarationTransportDetailsLayout(declaration);
			var seaColumns = seaLayout.Layout.Columns;
			AssertEquals(1, seaColumns.Count);

			AssertEquals(8, seaColumns[0].Rows.Count);
			var row1 = seaColumns[0].Rows[0];
			var row2 = seaColumns[0].Rows[1];
			var row3 = seaColumns[0].Rows[2];
			var row4 = seaColumns[0].Rows[3];
			var row5 = seaColumns[0].Rows[4];
			var row6 = seaColumns[0].Rows[5];
			var row7 = seaColumns[0].Rows[6];
			var row8 = seaColumns[0].Rows[7];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals("OverrideValuesCheckBox", row1.Parts[1].Name);

			AssertEquals(3, row2.Parts.Count);
			AssertEquals("OceanBillTextBox", row2.Parts[1].Name);

			AssertEquals(3, row3.Parts.Count);
			AssertEquals("VesselCodeFindBox", row3.Parts[1].Name);

			AssertEquals(3, row4.Parts.Count);
			AssertEquals("VesselCountryCodeFindBox", row4.Parts[1].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals("VoyageNumberTextBox", row5.Parts[1].Name);
			AssertEquals("CarrierKRCCodeFindBox", row5.Parts[3].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals("PortOfLoadingUserControl", row6.Parts[1].Name);

			AssertEquals(2, row7.Parts.Count);
			AssertEquals("PortOfDischargeUserControl", row7.Parts[1].Name);

			AssertEquals(4, row8.Parts.Count);
			AssertEquals("TransshipmentPortCodeFindBox", row8.Parts[1].Name);
			AssertEquals("TransshipmentDateEdit", row8.Parts[3].Name);
		}

		void AssertAirIncludedControls(JobDeclaration declaration)
		{
			IPanelLayoutProvider airLayout = new IMPDeclarationTransportDetailsLayout(declaration);
			var airColumns = airLayout.Layout.Columns;
			AssertEquals(1, airColumns.Count);
			AssertEquals(7, airColumns[0].Rows.Count);
			var row1 = airColumns[0].Rows[0];
			var row2 = airColumns[0].Rows[1];
			var row3 = airColumns[0].Rows[2];
			var row4 = airColumns[0].Rows[3];
			var row5 = airColumns[0].Rows[4];
			var row6 = airColumns[0].Rows[5];
			var row7 = airColumns[0].Rows[6];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals("OverrideValuesCheckBox", row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals("MasterBillTextBox", row2.Parts[1].Name);

			AssertEquals(5, row3.Parts.Count);
			AssertEquals("VoyageFlightNumberTextBox", row3.Parts[1].Name);
			AssertEquals("FolioNumberTextBox", row3.Parts[2].Name);
			AssertEquals("CarrierKRCCodeFindBox", row3.Parts[4].Name);

			AssertEquals(3, row4.Parts.Count);
			AssertEquals("VesselCountryCodeFindBox", row4.Parts[1].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals("PortOfLoadingUserControl", row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals("PortOfDischargeUserControl", row6.Parts[1].Name);

			AssertEquals(4, row7.Parts.Count);
			AssertEquals("TransshipmentPortCodeFindBox", row7.Parts[1].Name);
			AssertEquals("TransshipmentDateEdit", row7.Parts[3].Name);
		}
	}
}
