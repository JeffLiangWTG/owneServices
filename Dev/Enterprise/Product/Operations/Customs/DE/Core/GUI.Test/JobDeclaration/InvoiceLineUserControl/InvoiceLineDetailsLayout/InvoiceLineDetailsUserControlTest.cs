using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDescriptionLongTextControl()
		{
			AssertType<LongTextControl>(control.DescriptionLongTextControl);
		}

		public void TestFixedMaxLengthEntryInstructionGuidDropEdit() => CombineAssertions(() =>
		{
			AssertType<ZGuidDropEdit>("Type", control.FixedMaxLengthEntryInstructionGuidDropEdit);
			AssertEquals("Max Length", 10, control.FixedMaxLengthEntryInstructionGuidDropEdit.PreBoundMaxLength);
			Assert("Should Not Resize By Max Length", !control.FixedMaxLengthEntryInstructionGuidDropEdit.ShouldResizeByMaxLength);
		});

		public void TestFixedMaxLengthWithDescriptionTariffFindBox() => CombineAssertions(() =>
		{
			AssertType<Universal.GUI.TariffFindBox>("Type", control.FixedMaxLengthWithDescriptionTariffFindBox);

			AssertEquals("Max Length", 10, control.FixedMaxLengthWithDescriptionTariffFindBox.PreBoundMaxLength);
			Assert("Should Not Resize By Max Length", !control.FixedMaxLengthWithDescriptionTariffFindBox.ShouldResize);
		});

		public void TestFixedMaxLengthInvoiceNumberDropEdit() => CombineAssertions(() =>
		{
			AssertType<ZDropEdit>("Type", control.FixedMaxLengthInvoiceNumberDropEdit);
			AssertEquals("Max Length", 10, control.FixedMaxLengthInvoiceNumberDropEdit.PreBoundMaxLength);
			Assert("Should Not Resize By Max Length", !control.FixedMaxLengthInvoiceNumberDropEdit.ShouldResizeByMaxLength);
		});

		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestCessionManagementFlagDropEdit()
		{
			AssertType<ZDropEdit>(control.CessionManagementFlagDropEdit);
		}

		public void TestSupplementaryInformationTextBox()
		{
			AssertType<ZTextBox>(control.SupplementaryInformationTextBox);
		}

		public void TestOriginFederalStateDropEdit()
		{
			AssertType<ZDropEdit>(control.OriginFederalStateDropEdit);
		}

		public void TestInvoiceNumberDropEdit()
		{
			AssertType<ZDropEdit>(control.InvoiceNumberDropEdit);
		}

		public void TestQuotaQtyCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.QuotaQtyCalcDropEdit);
		}

		public void TestTobaccoStampTextBox()
		{
			AssertType<ZTextBox>(control.TobaccoStampTextBox);
		}

		public void TestIsMainPackCheckBox()
		{
			AssertType<ZCheckBox>(control.IsMainPackCheckBox);
		}

		public void TestUsualReplacementCheckBox()
		{
			AssertType<ZCheckBox>(control.UsualReplacementCheckBox);
		}

		public void TestReimportDateDateEdit()
		{
			AssertType<ZDateEdit>(control.ReimportDateEdit);
		}

		public void TestExportCountryCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.ExportCountryCodeFindBox);
		}

		public void TestDecisiveDateDateEdit()
		{
			AssertType<ZDateEdit>(control.DecisiveDateEdit);
		}

		public void TestOutwardMRNTextBox()
		{
			AssertType<ZTextBox>(control.OutwardMRNTextBox);
		}

		public void TestOutwardDecisiveDateEdit()
		{
			AssertType<ZDateEdit>(control.OutwardDecisiveDateEdit);
		}

		public void TestNetLinePriceCurrencyCalcFindBox()
		{
			AssertType<ZCalcFindBox>(control.NetPriceCurrencyCalcFindBox);
		}

		public void TestDgSubstanceUserControl()
		{
			AssertType<DgSubstanceUserControl>(control.DgSubstanceUserControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineDetailsUserControl();
		}
		InvoiceLineDetailsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
