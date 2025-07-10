using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AccReportingBookForm))]
	public class AccReportingBookFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new AccReportingBookForm(Factory.New<AccReportingBook>());
		}

		public void TestControls()
		{
			using (var form = (AccReportingBookForm)GetFormToBashCore())
			{
				form.Show();
				var groupBox = form.Controls.Find("DetailsGroupBox", true);

				AssertEquals(1, groupBox.Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_Code", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_IsActive", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_IsGlobal", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_IncludeChildPresentation", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_Description", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_AAC_AlternateChart", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_IncludePresentationJournals", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_GC_CompanyOfPeriod", false).Length);
				AssertEquals(1, groupBox[0].Controls.Find("ARB_RX_NKCurrency", false).Length);
			}
		}

		public void TestControl_ARB_Code()
		{
			using (var form = (AccReportingBookForm)GetFormToBashCore())
			{
				form.Show();
				var groupBox = form.Controls.Find("DetailsGroupBox", true);
				var code = groupBox[0].Controls.Find("ARB_Code", false)[0];
				var codeTextBox = (ZTextBox)code;

				AssertEquals("Prereq - Initial Text", "", codeTextBox.Text);
				KeySender.PostKeyDown(codeTextBox, Keys.OemOpenBrackets);
				Application.DoEvents();
				AssertEquals("No text after invalid open square brace character keypress", "", codeTextBox.Text);
				KeySender.PostKeyDown(codeTextBox, Keys.A);
				Application.DoEvents();
				AssertEquals("Text after 0 character keypress", "A", codeTextBox.Text);
				KeySender.PostKeyDown(codeTextBox, Keys.Back);
				Application.DoEvents();
				AssertEquals("Text after Back character keypress", "", codeTextBox.Text);
			}
		}

		public void TestSavingFormAddNewRecord()
		{
			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("1");
			Factory.Save();

			var reportingBook = creator.CreateReportingBook("XXA1234567", "Description", chart.PK, "ELM");
			using (var form = new AccReportingBookForm(reportingBook))
			{
				form.Show();

				var reportingBook2 = new BusinessObjectFactory().LoadTop1<AccReportingBook>(new ZQuery(AccReportingBookSchema.ARB_Code, "XXA1234567"));
				AssertNull("There are no records at this time", reportingBook2);

				form.FireSaveButton();
			}

			var reportingBook3 = new BusinessObjectFactory().LoadTop1<AccReportingBook>(new ZQuery(AccReportingBookSchema.ARB_Code, "XXA1234567"));
			AssertEquals("A record has been found", reportingBook3.ARB_Code, reportingBook.ARB_Code);
		}

		public void TestNotHaveNotesTab()
		{
			using (var form = (AccReportingBookForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals("Should not have notes tab", 0, form.Controls.Find("NotesTabPage", true).Length);
			}
		}

		public void TestNotHaveEDocsTab()
		{
			using (var form = (AccReportingBookForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals("Should not have eDocs tab", 0, form.Controls.Find("eDocsTabPage", true).Length);
			}
		}

		public void TestHaveAuditDataTab()
		{
			using (var form = (AccReportingBookForm)GetFormToBashCore())
			{
				form.Show();
				AssertNotNull("Should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		public void TestReportingCurrencyVisible()
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var reportingBookFeatureControlData = new ReportingBookFeatureControlData() { EnableCurrencyTranslation = false };
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out reportingBookFeatureControlData)).Returns(true);

			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingReportingBookFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (var form = new AccReportingBookFormForTest(Factory.New<AccReportingBook>()))
			{
				form.Show();
				form.OnShown_ForTestOnly();
				var currencyCodeFindBox = form.Controls.Find("ARB_RX_NKCurrency", true).FirstOrDefault();
				AssertEquals(false, currencyCodeFindBox?.Visible);
			}

			reportingBookFeatureControlData.EnableCurrencyTranslation = true;
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out reportingBookFeatureControlData)).Returns(true);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (var form = new AccReportingBookFormForTest(Factory.New<AccReportingBook>()))
			{
				form.Show();
				form.OnShown_ForTestOnly();
				var currencyCodeFindBox = form.Controls.Find("ARB_RX_NKCurrency", true).First() as ZCodeFindBox;
				AssertNotNull(currencyCodeFindBox);
				AssertEquals(true, currencyCodeFindBox?.Visible);
			}
		}

		public void TestARB_IncludeChildPresentation()
		{
			var companyCategorySetList = new GLPresentationJournalCategoryCollection();
			var category1 = companyCategorySetList.AddNew();
			category1.Code = "IOS";
			category1.Description = (NoResString)"Category 1";
			var category2 = companyCategorySetList.AddNew();
			category2.Code = "EOC";
			category2.ParentCode = category1.Code;
			category2.Description = (NoResString)"Category 2";
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyCategorySetList);

			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("1");
			Factory.Save();

			var reportingBook = creator.CreateReportingBook("XXA1234567", "Description", chart.PK, "EOC");
			using (var form = new AccReportingBookForm(reportingBook))
			{
				form.Show();
				var groupBox = form.Controls.Find("DetailsGroupBox", true);
				var includeChildPresentation = groupBox[0].Controls.Find("ARB_IncludeChildPresentation", false)[0];
				var includeChildPresentationCheckBox = (ZCheckBox)includeChildPresentation;

				Assert("CheckBox should not be edited", includeChildPresentationCheckBox.ReadOnly);

				reportingBook.ARB_IncludePresentationJournals = "IOS";
				Assert("CheckBox should be edited", !includeChildPresentationCheckBox.ReadOnly);
				reportingBook.ARB_IncludeChildPresentation = true;
				Assert("CheckBox should be checked", includeChildPresentationCheckBox.Checked);

				reportingBook.ARB_IncludePresentationJournals = "";
				Assert("CheckBox should not be edited", includeChildPresentationCheckBox.ReadOnly);
				Assert("CheckBox should not be checked", !includeChildPresentationCheckBox.Checked);
			}
		}

		public void TestMinimunSize()
		{
			using (var form = (AccReportingBookForm)GetFormToBashCore())
			{
				form.Show();
				var minimumsize = form.MinimumSize;
				var setminwidth = 805;
				var setminheight = 300;
				AssertEquals(setminwidth, minimumsize.Width);
				AssertEquals(setminheight, minimumsize.Height);
				AssertGreaterThanOrEqualTo(form.Size.Height, minimumsize.Height);
				AssertGreaterThanOrEqualTo(form.Size.Width, minimumsize.Width);
				form.Height = minimumsize.Height - 1;
				form.Width = minimumsize.Width - 1;
				AssertGreaterThanOrEqualTo(form.Size.Height, minimumsize.Height);
				AssertGreaterThanOrEqualTo(form.Size.Width, minimumsize.Width);
			}
		}

		public void TestCompanyOfPeriodReadonlyAfterSettingIsGlobal()
		{
			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_IsGlobal = true;
			using var form = new AccReportingBookForm(reportingBook);
			form.Show();

			var companyOfPeriodFindBox = (ZGuidFindBox)form.Controls.Find("ARB_GC_CompanyOfPeriod", true)[0];
			Assert("The ZGuidFindBox of ARB_GC_CompanyOfPeriod should be editable when ARB_IsGlobal is true", !companyOfPeriodFindBox.ReadOnly);

			reportingBook.ARB_IsGlobal = false;
			Assert("The ZGuidFindBox of ARB_GC_CompanyOfPeriod should be read-only when ARB_IsGlobal is false", companyOfPeriodFindBox.ReadOnly);

			reportingBook.ARB_IsGlobal = true;
			Assert("The ZGuidFindBox of ARB_GC_CompanyOfPeriod should be editable when ARB_IsGlobal is true", !companyOfPeriodFindBox.ReadOnly);
		}
	}

	class AccReportingBookFormForTest : AccReportingBookForm
	{
		public AccReportingBookFormForTest(AccReportingBook businessEntity) : base(businessEntity)
		{
		}

		public void OnShown_ForTestOnly()
		{
			OnShown(new System.EventArgs());
		}
	}
}
