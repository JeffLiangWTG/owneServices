using System.Windows.Forms;
using Enterprise.Accounting.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLJournalModuleChina))]
	class GLJournalModuleChinaTest : GLJournalModuleTest
	{
		public override void TestMenuStructure()
		{
			MenuItem[] menuItems = ChinaModule.ContextMenu_ForTestOnly;
			AssertNotNull(ChinaModule.ViewMenuItem);
			AssertNotNull(ChinaModule.NewMenuItem);
			AssertNotNull(ChinaModule.EditMenuItem);
			AssertNotNull(ChinaModule.CopyMenuItem);
			AssertNotNull(menuItems.FindByText(ChinaModule.GetDeleteMenuItemText_ForTestOnly().Caption));
			AssertNotNull(menuItems.FindByText("&Actions"));
			AssertNotNull(menuItems.FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
			AssertNotNull(menuItems.FindByText("Actions").MenuItems.FindByText(AccountingJournalPrintHelper.PrintAccountingJournalText));
			AssertNotNull(menuItems.FindByText("Actions").MenuItems.FindByText(ChinaModule.PrintAccountingVoucherMenuItemText_ForTestOnly));
		}

		public override void TestDisallowDelete()
		{
			Assert("Should allow delete", ChinaModule.AllowDelete);
		}

		protected GLJournalModuleChina ChinaModule
		{
			get { return Module as GLJournalModuleChina; }
		}

		protected override void SetUp()
		{
			OriginalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(OriginalCountry);
		}

		string OriginalCountry;
	}
}
