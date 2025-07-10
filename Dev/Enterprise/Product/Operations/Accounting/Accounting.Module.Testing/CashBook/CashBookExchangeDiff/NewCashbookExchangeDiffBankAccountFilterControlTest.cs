using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Module.Testing
{
	public class NewCashbookExchangeDiffBankAccountFilterControlTest : TestCaseWithFactory
	{
		public void TestMaxFilterStripPanelHeight()
		{
			var bankAccountCollection = new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany);
			using (var filterControlForTest = new NewCashbookExchangeDiffBankAccountFilterControlForTest(bankAccountCollection))
			{
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(220), filterControlForTest.GetMaxFilterStripPanelHeight());
			}
		}

		public void TestGridControl()
		{
			var bankAccountCollection = new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany);
			using (var filterControl = new NewCashbookExchangeDiffBankAccountFilterControl(bankAccountCollection))
			{
				filterControl.Show();
				AssertEquals("column AB_Code should be added.", true, filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(x => x.ColumnName == "AB_Code"));
				AssertEquals("filter grid should be hide", false, filterControl.FilteredGrid.Visible);
			}
		}

		public class NewCashbookExchangeDiffBankAccountFilterControlForTest : NewCashbookExchangeDiffBankAccountFilterControl
		{
			public NewCashbookExchangeDiffBankAccountFilterControlForTest(IBusinessObjectCollection gridCollection)
				: base(gridCollection)
			{ }
			public int GetMaxFilterStripPanelHeight() => MaxFilterStripPanelHeight;
		}
	}
}

