using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDeclarationTemplateUserControl))]
	public class ValuationDeclarationTemplateUserControlTest : TestCaseWithFactory
	{
		public void TestValuationDeclarationTemplateUserControlTest()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "(주)레디코리아");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			declaration.JE_OH_Importer = importer.PK;
			using (var form = new MiscDeclarationForm(declaration))
			{
				form.Show();
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = tabControl.FindSingle<ZTabPage>("MainTabPage");
				var control = mainTabPage.Controls[0];
				AssertEquals(typeof(ValuationDeclarationTemplateUserControl), control.GetType());

				var leftPanel = control.FindSingle<ZPanel>("LeftPanel");
				Assert(leftPanel.FindSingle<ZOrganisationControl>("SupplierOrganisationControl").Visible);
				Assert(leftPanel.FindSingle<ZOrganisationControl>("ImporterOrganisationControl").Visible);

				var payerGroupBox = control.FindSingle<ZGroupBox>("PayerGroupBox");
				Assert(payerGroupBox.FindSingle<ZOrganisationFindBox>("PayerOrganisationFindBox").Visible);

				Assert(leftPanel.FindSingle<ZGroupBox>("DetailsGroupBox").Visible);

				var authorGroupBox = control.FindSingle<ZGroupBox>("AuthorGroupBox");
				Assert(authorGroupBox.FindSingle<ZDropEdit>("AuthorGuidDropEdit").Visible);
				Assert(authorGroupBox.FindSingle<ZTextBox>("AuthorNameTextBox").Visible);
				Assert(authorGroupBox.FindSingle<ZTextBox>("AuthorPhoneTextBox").Visible);
				Assert(authorGroupBox.FindSingle<ZTextBox>("AuthorJobTitleTextBox").Visible);

				var auditorGroupBox = control.FindSingle<ZGroupBox>("ResponsiblePersonGroupBox");
				Assert(auditorGroupBox.FindSingle<ZDropEdit>("AuditorGuidDropEdit").Visible);
				Assert(auditorGroupBox.FindSingle<ZTextBox>("AuditorNameTextBox").Visible);
				Assert(auditorGroupBox.FindSingle<ZTextBox>("AuditorPhoneTextBox").Visible);
				Assert(auditorGroupBox.FindSingle<ZTextBox>("AuditorJobTitleTextBox").Visible);
			}
		}

		public void TestValuationMethodPanelVisible()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "(주)레디코리아");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			declaration.JE_OH_Importer = importer.PK;
			using (var form = new MiscDeclarationForm(declaration))
			{
				form.Show();
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = tabControl.FindSingle<ZTabPage>("MainTabPage");
				var control = mainTabPage.Controls[0];
				var invoice = declaration.Invoices[0];
				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
				Assert(control.FindSingle<ValuationMethodCUserControl>("ValuationMethodCUserControl").Visible);
				Assert(!control.FindSingle<ValuationMethodDUserControl>("ValuationMethodDUserControl").Visible);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;

				Assert(!control.FindSingle<ValuationMethodCUserControl>("ValuationMethodCUserControl").Visible);
				Assert(control.FindSingle<ValuationMethodDUserControl>("ValuationMethodDUserControl").Visible);
			}
		}
	}
}
