namespace Enterprise.Accounting.GUI.ComplianceReport.SAFT.Testing
{
	using System.Windows.Forms;
	using CargoWise.Application;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business;
	using Enterprise.Accounting.Business.AccountingCountryFactory;
	using Enterprise.Accounting.Business.ComplianceReport.SAFT;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Moq;
	using NUnit.Framework;

	public class ReportModeAndCreditorSelectorFormTest : TestCaseWithFactory
	{
		public void TestContinueButtonGenerateSalesInvoices()
		{
			using (var form = new ReportModeAndCreditorSelectorForm(Selector))
			{
				form.Show();

				Assert("Pre-condition: Generate Sales Invoices", Selector.GenerateSalesInvoices);
				Assert("Creditor PK is empty", Selector.CreditorPK.IsEmpty);

				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertNoErrors(Selector);
			}
		}

		public void TestContinueButtonGenerateCreditorInvoices()
		{
			using (var form = new ReportModeAndCreditorSelectorForm(Selector))
			{
				form.Show();

				Selector.GenerateCreditorInvoices = true;
				Assert("Creditor PK is empty", Selector.CreditorPK.IsEmpty);

				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				Assert(Selector.HasErrors);

				var creator = new TestObjectCreator(Factory);
				creator.Creditor1.CompanyData.OB_APCostsSelfBilled = true;
				Selector.CreditorPK = creator.Creditor1.PK;

				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			using (var form = new ReportModeAndCreditorSelectorForm(Selector))
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Selector = new ReportModeAndCreditorSelector(Factory);
		}

		ReportModeAndCreditorSelector Selector;
	}

	[TestedType(typeof(ReportModeAndCreditorSelectorForm))]
	public class ReportModeAndCreditorSelectorFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var mockIReportModeAndCreditorSelectorDefault = new Mock<IReportModeAndCreditorSelectorDefault>();
			mockIReportModeAndCreditorSelectorDefault.Setup(x => x.GenerateSalesInvoices).Returns(true);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IReportModeAndCreditorSelectorDefault>>().Setup(x => x.Get()).Returns(mockIReportModeAndCreditorSelectorDefault.Object);

			var mockAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockAccountingCountryFactory.Object);

			return new ReportModeAndCreditorSelectorForm(new ReportModeAndCreditorSelector(Factory));
		}
	}
}
