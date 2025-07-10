using System.Collections.Generic;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(LEXDeclarationTransportDetailsLayout))]
	sealed class LEXDeclarationTransportDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		protected override IPanelLayoutProvider GetNewPanelLayoutProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;

			return new LEXDeclarationTransportDetailsLayout(declaration);
		}

		public new void TestIncludedControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			AssertSeaIncludedControls(declaration);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertAirIncludedControls(declaration);
		}

		void AssertSeaIncludedControls(JobDeclaration declaration)
		{
			IPanelLayoutProvider layout = new LEXDeclarationTransportDetailsLayout(declaration);
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(4, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals("OverrideValuesCheckBox", row1.Parts[1].Name);

			AssertEquals(3, row2.Parts.Count);
			AssertEquals("VesselCodeFindBox", row2.Parts[1].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals("RadioCallSignTextBox", row3.Parts[1].Name);
			AssertEquals("VoyageDurationCalcEdit", row3.Parts[3].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals("MRNTypeDropEdit", row4.Parts[1].Name);
			AssertEquals("MRNNumberTextBox", row4.Parts[3].Name);
		}

		void AssertAirIncludedControls(JobDeclaration declaration)
		{
			IPanelLayoutProvider layout = new LEXDeclarationTransportDetailsLayout(declaration);
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(2, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];

			AssertEquals(2, row1.Parts.Count);
			AssertEquals("OverrideValuesCheckBox", row1.Parts[1].Name);

			AssertEquals(2, row2.Parts.Count);
			AssertEquals("FlightUserControl", row2.Parts[1].Name);
		}
	}
}
