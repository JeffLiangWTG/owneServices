using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CSARevenueSummaryFormForm))]
	sealed class CSARevenueSummaryFormFormTest : ZFormBasherTest
	{
		public void TestDocDataPlugIn()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			Factory.Save();
			using (var form = new CSARevenueSummaryFormForm(header))
			{
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);
				AssertNotNull(plugIn);
			}
		}

		public void TestMenuItems()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			Factory.Save();

			using (var form = new CSARevenueSummaryFormForm(header))
			{
				form.ControllerID = ControllerIDs.Customs.CA.CACSARevenueSummaryForm;
				var messagingMenu = form.FindMenuItem_ForTest("Messaging");
				AssertNotNull(messagingMenu);

				var menuItems = messagingMenu.MenuItems;
				AssertEquals(2, menuItems.Count);

				var sendOriginal = menuItems.FindByText("Send Original RSF Message");
				var sendAdjustment = menuItems.FindByText("Send Adjustment RSF Message");
				AssertNotNull(sendOriginal);
				AssertNotNull(sendAdjustment);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			_ = header.DebitLine;
			_ = header.CreditLine;
			_ = header.InterimLine;
			_ = header.Debits;
			_ = header.Credits;
			_ = header.InterimPayments;
			_ = header.CustomsAssessments;
			_ = header.CSARSFTransactions;
			Factory.Save();

			var result = new CSARevenueSummaryFormForm(header)
			{
				ControllerID = ControllerIDs.Customs.CA.CACSARevenueSummaryForm
			};

			return result;
		}
	}
}
