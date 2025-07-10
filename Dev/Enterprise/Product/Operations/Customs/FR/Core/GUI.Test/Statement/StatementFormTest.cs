using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Testing
{
	[TestedType(typeof(StatementForm))]
	public class StatementFormTest : ZFormBasherTest
	{
		public void TestControlsAreVisible()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "002244";
			using (var form = new StatementForm(statement))
			{
				form.Show();
				AssertEquals("The details group box should be visible. This is likely to happen when you delete calling of InitializeComponent() by accident.", true, form.FindSingle<ZGroupBox>("DetailsGroupBox").Visible);
				AssertEquals("The controls in the group box should be visible. This is likely to happen when you delete calling of InitializeComponent() by accident.", true, form.FindSingle<ZTextBox>("StatementNumberTextBox").Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var statement = Factory.New<CusStatementHeader>();
			return new StatementForm(statement);
		}

		public void TestFormCaption()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "002244";
			using (var form = new StatementForm(statement))
			{
				AssertEquals("Liquidation 002244", form.FormCaption);
			}
		}

		public void TestSendDCGMenuItem()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var form = new StatementForm(statement))
			{
				var actionMenuItem = form.Menu.MenuItems.FindByText("Actio&ns");
				var sendDCGMenuItem = actionMenuItem.MenuItems.FindByText("Send DCG");
				AssertNotNull(sendDCGMenuItem);
			}
		}

		public void TestSendDCGSuccessfully()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER1";
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.DAY, "F9A9FB22");

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = importer.PK;
			statement.B2_EntryFilerCode = "DGI002";
			statement.B2_CheckNo = "AUPK";
			statement.B2_ImporterCustomsID = "FR4021885690004";
			statement.B2_PaymentType = "R";
			statement.B2_PeriodStartDate = new ZDate(2021, 08, 01);
			statement.B2_StatementType = "J";
			statement.B2_BranchDesignation = "IMP";
			Factory.Save();

			using (var form = new StatementForm(statement))
			{
				var sendDCGMenu = form.Menu.MenuItems.FindByText("Send DCG", true);
				sendDCGMenu.PerformClick();
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);

				var message = statement.Messages[0];
				AssertEquals(true, message.IsTransmitMessage);
				AssertEquals(MessageTypeList.Codes.DCG, message.EM_MessageType);
				AssertEquals(MessageTypeList.Codes.DCG, message.EM_MessageSubType);
				AssertContains("<schemaID>MessageDcg</schemaID>", message.EM_MessageText);
			}
		}

		public void TestFailedToSendDCG()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER1";
			importer.OH_IsConsignee = true;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = importer.PK;
			statement.B2_BranchDesignation = "IMP";
			statement.B2_EntryFilerCode = "DGI002";
			statement.B2_CheckNo = "AUPK";
			statement.B2_ImporterCustomsID = "FR4021885690004";
			statement.B2_PaymentType = "R";
			statement.B2_PeriodStartDate = new ZDate(2021, 08, 01);
			statement.B2_StatementType = "J";
			Factory.Save();

			using (var form = new StatementForm(statement))
			{
				var sendDCGMenu = form.Menu.MenuItems.FindByText("Send DCG", true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendDCGMenu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Failed to create closure message due to the following errors:" + System.Environment.NewLine + "Representative ID is not configured. Config it in Organization > Config > France tab, with Delta Mode (G2) and Agreement Type (DGI).", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(0, statement.Messages.Count);
			}
		}
	}
}
