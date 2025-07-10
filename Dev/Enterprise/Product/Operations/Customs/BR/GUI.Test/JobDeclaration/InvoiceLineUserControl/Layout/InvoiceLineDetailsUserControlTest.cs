using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ZArchitecture.GUI.ZDropEdit;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestComplementaryDescriptionTextBox()
		{
			AssertType<Customs.GUI.LongTextControl>(control.ComplementaryDescriptionTextBox);
			AssertEquals("ComplementaryDescription", control.BindingSource.GetBindingMember(control.ComplementaryDescriptionTextBox));
		}

		public void TestBRNFEItemNumberTextBox()
		{
			AssertType<ZTextBox>(control.BRNFEItemNumberTextBox);
		}

		public void TestBRNFENumberTextBox()
		{
			AssertType<ZTextBox>(control.BRNFENumberTextBox);
		}

		public void TestCargoPriorityDropEdit()
		{
			AssertType<ZDropEdit>(control.CargoPriorityDropEdit);
		}

		public void TestCountryDestinationCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.CountryDestinationCodeFindBox);
		}

		public void TestCPCGroupBox()
		{
			AssertType<ZGroupBox>(control.CPCGroupBox);
		}

		public void TestCPCDropEdit()
		{
			AssertType<ZDropEdit>(control.CPCDropEdit);
		}

		public void TestCPCFourthDropEdit()
		{
			AssertType<ZDropEdit>(control.CPCFourthDropEdit);
		}

		public void TestCPCThirdDropEdit()
		{
			AssertType<ZDropEdit>(control.CPCThirdDropEdit);
		}

		public void TestCPCSecondDropEdit()
		{
			AssertType<ZDropEdit>(control.CPCSecondDropEdit);
		}

		public void TestNFeLinePriceCalcEdit()
		{
			AssertType<ZCalcFindBox>(control.NFeLinePriceCalcEdit);
		}

		public void TestNaladiNccaCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.NaladiNccaCodeFindBox);
		}

		public void TestNaladiHsCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.NaladiHsCodeFindBox);
		}

		public void TestImportLicenseNumberTextBox()
		{
			AssertType<ZTextBox>(control.ImportLicenseNumberTextBox);
		}

		public void TestGoodsConditionSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.GoodsConditionSeparatorUserControl);
		}

		public void TestTaxRegimeDropEdit()
		{
			AssertType<ZDropEdit>(control.DutyTaxRegimeDropEdit);
		}

		public void TestLegalBasisDropEdit()
		{
			AssertType<ZDropEdit>(control.DutyLegalBaseDropEdit);
		}

		public void TestTaxRegimeSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.DutyTaxRegimeSeparatorUserControl);
		}

		public void TestImportLicenseFineSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.ImportLicenseFineSeparatorUserControl);
		}

		public void TestImportLicenseTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.ImportLicenseTypeDropEdit);
		}

		public void TestImportLicenseAuthorizationDateTextBox()
		{
			AssertType<ZDateEdit>(control.ImportLicenseAuthorizationDateTextBox);
		}

		public void TestImportLicenseFeeTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.ImportLicenseFeeTypeDropEdit);
		}

		public void TestTariffAgreementSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.TariffAgreementSeparatorUserControl);
		}

		public void TestTariffAgreementDropEdit()
		{
			AssertType<ZDropEdit>(control.TariffAgreementDropEdit);
		}

		public void TestGoodsConditionDropEdit()
		{
			AssertType<ZDropEdit>(control.GoodsConditionDropEdit);
		}

		public void TestFullGoodsDescriptionTextBox()
		{
			AssertType<Customs.GUI.LongTextControl>(control.FullGoodsDescriptionTextBox);
			AssertEquals("FullGoodsDescription", control.BindingSource.GetBindingMember(control.FullGoodsDescriptionTextBox));
		}

		public void TestRequiresImportLicenseCheckBox()
		{
			AssertType<ZCheckBox>(control.RequiresImportLicenseCheckBox);
		}

		public void TestEntryInstructionGuidDropEdit()
		{
			var entryDropEdt = control.EntryInstructionGuidDropEdit;
			AssertType<ZGuidDropEdit>(entryDropEdt);
			AssertEquals("ShowInDropDown", ShowInDropDownList.OnlyShowCode, entryDropEdt.ShowInDropDown);
			AssertEquals("ShowDescriptionBox", false, entryDropEdt.ShowDescriptionBox);
			AssertEquals("UseFullWidthForCodeBox", true, entryDropEdt.UseFullWidthForCodeBox);
		}

		InvoiceLineDetailsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
