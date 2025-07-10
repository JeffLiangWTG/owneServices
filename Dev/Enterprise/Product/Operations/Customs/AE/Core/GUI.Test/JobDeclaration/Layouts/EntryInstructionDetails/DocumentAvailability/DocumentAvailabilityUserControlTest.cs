using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class DocumentAvailabilityUserControlTest : TestCaseWithFactory
{
	public void TestDocumentAvailabilityGridColumnOrder()
	{
		using var userControl = new DocumentAvailabilityUserControl();
		var controlGrid = userControl.DocumentAvailabilityGrid;
		var columnsOrder = controlGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
		var expectedColumnOrder = new[]
		{
			"CSI_Code",
			"CSI_Status",
			"CSI_SubType",
		};
		AssertArrayEqualsByElements(columnsOrder, expectedColumnOrder);
	}

	public void TestDocumentAvailabilityGroupBoxCaption()
	{
		AssertEquals("Document Availability", control.DocumentAvailabilityGroupBox.CaptionResourceString.Caption);
	}

	public void TestDocumentAvailabilityGroupBox()
	{
		var controlGroupBox = control.DocumentAvailabilityGroupBox;

		CombineAssertions(() =>
		{
		controlGroupBox.AssertContainsControl<ZDropEdit>("DocumentTypeDropEdit", x => x.WithBindTo("CustomsEntryInstructions.DocumentAvailability.CSI_Code"));
		controlGroupBox.AssertContainsControl<ZDropEdit>("AvailabilityStatusDropEdit", x => x.WithBindTo("CustomsEntryInstructions.DocumentAvailability.CSI_Status"));
		controlGroupBox.AssertContainsControl<ZDropEdit>("ReasonCodeDropEdit", x => x.WithBindTo("CustomsEntryInstructions.DocumentAvailability.CSI_SubType"));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new DocumentAvailabilityUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	DocumentAvailabilityUserControl control;
}
