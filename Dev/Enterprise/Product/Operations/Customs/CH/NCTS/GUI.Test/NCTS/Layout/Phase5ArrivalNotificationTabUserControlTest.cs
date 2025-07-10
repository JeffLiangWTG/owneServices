using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.GUI.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class Phase5ArrivalNotificationTabUserControlTest : TestCaseWithFactory
{
	public void TestMovementReferenceNumbersGroupBoxVisibility()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
		Factory.Save();

		using (var form = new NctsMovementForm(nctsHeader))
		using (var parent = new Phase5ArrivalNotificationTabUserControl())
		{
			form.Controls.Add(parent);
			form.Show();

			CombineAssertions(() =>
			{
				var movementReferenceNumbersGroupBox = parent.MovementReferenceNumbersGroupBox;
				AssertEquals("MRN GroupBox visible", true, movementReferenceNumbersGroupBox.Visible);
				nctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.False;
				AssertEquals("MRN GroupBox not visible", false, movementReferenceNumbersGroupBox.Visible);
			});
		}
	}

	public void TestGridColumns() => CombineAssertions(() =>
	{
		using (var control = new Phase5ArrivalNotificationTabUserControl())
		{
			var gridColumnStyles = control.MovementReferenceNumbersGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, MovementReferenceNumberSupportingInfo.Schema.CSI_LineNo, 0);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, MovementReferenceNumberSupportingInfo.Schema.CSI_ReferenceNumber, 1);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(gridColumnStyles, MovementReferenceNumberSupportingInfo.Schema.CSI_Status, 2);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, MovementReferenceNumberSupportingInfo.Schema.CSI_Description, 3);
		}
	});
}
