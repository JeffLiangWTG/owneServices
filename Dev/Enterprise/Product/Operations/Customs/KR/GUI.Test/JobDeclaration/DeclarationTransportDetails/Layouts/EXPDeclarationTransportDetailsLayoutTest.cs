using System.Collections.Generic;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EXPDeclarationTransportDetailsLayout))]
	sealed class EXPDeclarationTransportDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		protected override IPanelLayoutProvider GetNewPanelLayoutProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			return new EXPDeclarationTransportDetailsLayout(declaration);
		}

		public new void TestIncludedControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertSeaIncludedControls(declaration);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertAirIncludedControls(declaration);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertMailIncludedControls(declaration);
		}

		void AssertSeaIncludedControls(JobDeclaration declaration)
		{
			IPanelLayoutProvider seaLayout = new EXPDeclarationTransportDetailsLayout(declaration);
			var seaColumns = seaLayout.Layout.Columns;
			AssertEquals(1, seaColumns.Count);

			AssertEquals(6, seaColumns[0].Rows.Count);
			var row1 = seaColumns[0].Rows[0];
			var row2 = seaColumns[0].Rows[1];
			var row3 = seaColumns[0].Rows[2];
			var row4 = seaColumns[0].Rows[3];
			var row5 = seaColumns[0].Rows[4];
			var row6 = seaColumns[0].Rows[5];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals("OverrideValuesCheckBox", row1.Parts[1].Name);

			AssertEquals(3, row2.Parts.Count);
			AssertEquals("OceanBillTextBox", row2.Parts[1].Name);

			AssertEquals(3, row3.Parts.Count);
			AssertEquals("VesselCodeFindBox", row3.Parts[1].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals("VoyageNumberTextBox", row4.Parts[1].Name);
			AssertEquals("CarrierKRCCodeFindBox", row4.Parts[3].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals("PortOfLoadingUserControl", row5.Parts[1].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals("PortOfDischargeUserControl", row6.Parts[1].Name);
		}

		void AssertAirIncludedControls(JobDeclaration declaration)
		{
			IPanelLayoutProvider airLayout = new EXPDeclarationTransportDetailsLayout(declaration);
			var airColumns = airLayout.Layout.Columns;
			AssertEquals(1, airColumns.Count);

			AssertEquals(5, airColumns[0].Rows.Count);
			var row1 = airColumns[0].Rows[0];
			var row2 = airColumns[0].Rows[1];
			var row3 = airColumns[0].Rows[2];
			var row4 = airColumns[0].Rows[3];
			var row5 = airColumns[0].Rows[4];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals("OverrideValuesCheckBox", row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals("MasterBillTextBox", row2.Parts[1].Name);

			AssertEquals(5, row3.Parts.Count);
			AssertEquals("VoyageFlightNumberTextBox", row3.Parts[1].Name);
			AssertEquals("FolioNumberTextBox", row3.Parts[2].Name);
			AssertEquals("CarrierKRCCodeFindBox", row3.Parts[4].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals("PortOfLoadingUserControl", row4.Parts[1].Name);

			AssertEquals(2, row5.Parts.Count);
			AssertEquals("PortOfDischargeUserControl", row5.Parts[1].Name);
		}

		void AssertMailIncludedControls(JobDeclaration declaration)
		{
			IPanelLayoutProvider mailLayout = new EXPDeclarationTransportDetailsLayout(declaration);
			var mailColumns = mailLayout.Layout.Columns;
			AssertEquals(1, mailColumns.Count);

			AssertEquals(4, mailColumns[0].Rows.Count);
			var row1 = mailColumns[0].Rows[0];
			var row2 = mailColumns[0].Rows[1];
			var row3 = mailColumns[0].Rows[2];
			var row4 = mailColumns[0].Rows[3];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals("OverrideValuesCheckBox", row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals("CarrierKRCCodeFindBox", row2.Parts[1].Name);

			AssertEquals(6, row3.Parts.Count);
			AssertEquals("PortOfLoadingCodeFindBox", row3.Parts[1].Name);
			AssertEquals("IATALoadPortDropEdit", row3.Parts[3].Name);
			AssertEquals("ExportDateEdit", row3.Parts[5].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals("PortOfDischargeUserControl", row4.Parts[1].Name);
		}
	}
}
