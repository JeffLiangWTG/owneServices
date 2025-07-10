using System.Windows.Forms;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscDeclarationForm))]
	public class MiscDeclarationFormTest : ZFormBasherTest
	{
		public void TestCheckTabIndex()
		{
			using (var form = new MiscDeclarationForm(Declaration))
			{
				form.Show();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertEquals(7, tabControl.Controls.Count);
				var index = 0;
				AssertEquals(tabControl.Controls[index++].Name, "MainTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "EntriesTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "zWorkflowTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "BillingTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "DocDataTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "NotesTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "LogsTabPage");
			}
		}

		public void TestChangeEntriesControlType()
		{
			Declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			using (var form = new MiscDeclarationForm(Declaration))
			{
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var entriesTab = tabControl.FindSingle<ZTabPage>("EntriesTabPage");

				AssertEquals(typeof(PersonalItemsEntriesUserControl), entriesTab.Controls[0].GetType());

				var mainTabPage = tabControl.FindSingle<ZTabPage>("MainTabPage");
				AssertEquals(mainTabPage.Controls[0].GetType(), typeof(PersonalItemsDeclarationUserControl));
			}

			Declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			using (var form = new MiscDeclarationForm(Declaration))
			{
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var entriesTab = tabControl.FindSingle<ZTabPage>("EntriesTabPage");

				AssertEquals(typeof(CarnetEntryUserControl), entriesTab.Controls[0].GetType());

				var mainTabPage = tabControl.FindSingle<ZTabPage>("MainTabPage");
				AssertEquals(mainTabPage.Controls[0].GetType(), typeof(CarnetDeclarationUserControl));
			}

			Declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			using (var form = new MiscDeclarationForm(Declaration))
			{
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = tabControl.FindSingle<ZTabPage>("MainTabPage");
				AssertEquals(mainTabPage.Controls[0].GetType(), typeof(ValuationDeclarationTemplateUserControl));

				var entriesTab = tabControl.FindSingle<ZTabPage>("EntriesTabPage");
				AssertEquals(typeof(MessagesTabUserControl), entriesTab.Controls[0].GetType());
				AssertEquals("Messages", entriesTab.CaptionResourceString.Caption);
			}
		}

		public void TestChangedCaption()
		{
			Declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			using (var form = new MiscDeclarationForm(Declaration))
			{
				AssertContains("(008) Personal Items Declaration", form.FormCaption);
			}
			Declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			using (var form = new MiscDeclarationForm(Declaration))
			{
				AssertContains("(D87) Carnet Temporary Import Certificate", form.FormCaption);
			}
			Declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			using (var form = new MiscDeclarationForm(Declaration))
			{
				AssertContains("(5SM) Valuation Declaration", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new MiscDeclarationForm(Declaration);
			result.ControllerID = ControllerIDs.Customs.JobDeclaration;
			return result;
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}

	[TestedType(typeof(MiscDeclarationForm))]
	sealed class MiscDeclarationFormTestFor5SM : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var result = new MiscDeclarationForm(Declaration);
			result.ControllerID = ControllerIDs.Customs.JobDeclaration;
			return result;
		}

		public void TestCheckTabIndex()
		{
			using (var form = new MiscDeclarationForm(Declaration))
			{
				form.Show();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertEquals(8, tabControl.Controls.Count);
				var index = 0;
				AssertEquals(tabControl.Controls[index++].Name, "MainTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "ValuationTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "EntriesTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "zWorkflowTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "BillingTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "DocDataTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "NotesTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "LogsTabPage");
			}
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
					invoice = declaration.Invoices.AddNew();
					invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
					invoice = declaration.Invoices.AddNew();
					invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodThree;
					invoice = declaration.Invoices.AddNew();
					invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
					invoice = declaration.Invoices.AddNew();
					invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
					invoice = declaration.Invoices.AddNew();
					invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
					invoice = declaration.Invoices.AddNew();
					invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodSix;
					Factory.Save();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
	}
}
