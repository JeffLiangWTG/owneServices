using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.GUI.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

internal class CustomsOfficesUserControlTest : TestCase
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
	}

	public void TestColumns()
	{
		var gridColumnStyles = control.CustomsOfficesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
		UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(gridColumnStyles, NctsEuOfficeCode.Schema.CY_Order, 0, width: 59);
		UserControlTestHelper.AssertColumnStyles<ZDropEditColumnStyleInfo>(gridColumnStyles, NctsEuOfficeCode.Schema.CY_Code, 1, width: 59);
		UserControlTestHelper.AssertColumnStyles<ZCodeFindBoxColumnStyleInfo>(gridColumnStyles, NctsEuOfficeCode.Schema.CY_Data, 2, width: 70);
		UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, NctsEuOfficeCode.Schema.CY_OfficeDescription, 3, width: 200);
		UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, NctsEuOfficeCode.Schema.EstimatedNumberOfDays, 4, width: 59);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new CustomsOfficesUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	CustomsOfficesUserControl control;
}
