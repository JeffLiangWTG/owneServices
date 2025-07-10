using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.GUI.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class AdditionalTransitOperationUserControlTest : TestCase
{
	public void TestGridColumns()
	{
		using (var control = new AdditionalTransitOperationsUserControl())
		{
			var gridColumnStyles = control.AdditionalTransitOperationsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_LineNo, 0, width: 80);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_IssuerType, 1, width: 80);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_ReferenceNumber, 2, width: 200);
			UserControlTestHelper.AssertColumnStyles<ZMultiLineTextBoxColumnInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_Description, 3, width: 200);
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_Quantity, 4, width: 80, groupName: "Gross mass");
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_UnitOfQuantity, 5, width: 80, groupName: "Gross mass");
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_PackQty, 6, width: 90);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_PackType, 7, width: 90);
			UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(gridColumnStyles, AdditionalTransitOperation.Schema.CSI_Status, 8, width: 100);
		}
	}
}
