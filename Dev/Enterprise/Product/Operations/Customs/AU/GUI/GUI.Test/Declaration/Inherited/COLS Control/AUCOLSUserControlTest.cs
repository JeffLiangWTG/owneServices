using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AUCOLSUserControlTest : TestCaseWithFactory
	{
		public void TestTabPage()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryNum = "IMP1234";
			_ = declaration.CreateCOLSHeaderIfRequired();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				using (var testForm = new ZAUCustomsDeclarationForm(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.COLSTabPage;
					var colsTabControl = testForm.CustomsBrokerageUserControl.cOLSUserControl.Controls.Find("AUCOLSTabControl", true).FirstOrDefault() as ZTabControl;
					AssertEquals("Four TabPage", 4, colsTabControl.Controls.Count);
				}
			}
		}

		public void TestMessageUserControl()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryNum = "IMP1234";
			_ = declaration.CreateCOLSHeaderIfRequired();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				using (var testForm = new ZAUCustomsDeclarationForm(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.COLSTabPage;
					var messagesTabPage = testForm.CustomsBrokerageUserControl.cOLSUserControl.Controls.Find("MessagesTabPage", true).FirstOrDefault() as ZTabPage;
					AssertEquals("One UserControl", 1, messagesTabPage.Controls.Count);
					AssertEquals("UserControl Type", typeof(BaseMessagesTabUserControl), messagesTabPage.Controls[0].GetType());
				}
			}
		}

		public void TestAUCOLSDiscardedMessageUserControl()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryNum = "IMP1234";
			_ = declaration.CreateCOLSHeaderIfRequired();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				using (var testForm = new ZAUCustomsDeclarationForm(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.COLSTabPage;
					var discardedMessagesTabPage = testForm.CustomsBrokerageUserControl.cOLSUserControl.Controls.Find("DiscardedMessagesTabPage", true).FirstOrDefault() as ZTabPage;
					AssertEquals("One UserControl", 1, discardedMessagesTabPage.Controls.Count);
					AssertEquals("UserControl Type", typeof(BaseMessagesTabUserControl), discardedMessagesTabPage.Controls[0].GetType());
				}
			}
		}

		public void TestAUCOLSAttachmentsUserControl()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryNum = "IMP1234";
			_ = declaration.CreateCOLSHeaderIfRequired();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				using (var testForm = new ZAUCustomsDeclarationForm(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.COLSTabPage;
					var attachmentsTabPage = testForm.CustomsBrokerageUserControl.cOLSUserControl.Controls.Find("AttachmentsTabPage", true).FirstOrDefault() as ZTabPage;
					AssertEquals("One UserControl", 1, attachmentsTabPage.Controls.Count);
					AssertNotNull("Attachments Tab Page should have AttchmentsGrid", attachmentsTabPage.Controls.Find("AttachmentsGrid", true)[0] as ZGrid);
				}
			}
		}
	}
}
