using System.Windows.Forms;
using Enterprise.Customs.GB.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.MessageSending.Testing
{
	[TestedType(typeof(EnhancedValidationForm))]
	class EnhancedValidationFormTest : ZFormBasherTest
	{
		public void TestSubmitButtonEnabled_NoTax()
		{
			var wrapperMock = new Mock<IEnhancedValidationEntryWrapper>();
			using var form = new EnhancedValidationForm(wrapperMock.Object);
			form.Show();
			var (submitButton, check1, check2, check3) = FindSubmitButtonAndCheckBoxes(form);

			AssertEquals("--", expected: false, submitButton.Enabled);
			check1.Checked = true;
			AssertEquals("/-", expected: false, submitButton.Enabled);
			check2.Checked = true;
			AssertEquals("//", expected: true, submitButton.Enabled);
			check1.Checked = false;
			AssertEquals("-/", expected: false, submitButton.Enabled);
		}

		public void TestSubmitButtonEnabled_WithTax()
		{
			var wrapperMock = new Mock<IEnhancedValidationEntryWrapper>();
			_ = wrapperMock.Setup(x => x.TaxLineCount).Returns(1);

			using var form = new EnhancedValidationForm(wrapperMock.Object);
			form.Show();
			var (submitButton, check1, check2, check3) = FindSubmitButtonAndCheckBoxes(form);

			AssertEquals("---", expected: false, submitButton.Enabled);
			check1.Checked = true;
			AssertEquals("/--", expected: false, submitButton.Enabled);
			check2.Checked = true;
			AssertEquals("//-", expected: false, submitButton.Enabled);
			check1.Checked = false;
			AssertEquals("-/-", expected: false, submitButton.Enabled);
			check3.Checked = true;
			AssertEquals("-//", expected: false, submitButton.Enabled);
			check1.Checked = true;
			AssertEquals("///", expected: true, submitButton.Enabled);
			check2.Checked = false;
			AssertEquals("/-/", expected: false, submitButton.Enabled);
			check1.Checked = false;
			AssertEquals("--/", expected: false, submitButton.Enabled);
		}

		public void TestCommodityLineLabels()
		{
			var wrapperMock = new Mock<IEnhancedValidationEntryWrapper>();
			_ = wrapperMock.Setup(x => x.CommodityLine1).Returns("0101.01.01 01 from Here");
			_ = wrapperMock.Setup(x => x.CommodityLine2).Returns("0101.01.01 02 from There");
			_ = wrapperMock.Setup(x => x.CommodityLine3).Returns("0101.01.01 03 from Everywhere");
			_ = wrapperMock.Setup(x => x.CommodityLine4).Returns("0101.01.01 04 from Nowhere");
			_ = wrapperMock.Setup(x => x.CommodityLine5).Returns("0101.01.01 05 from Elsewhere");

			for (var i = 1; i < 6; ++i)
			{
				_ = wrapperMock.Setup(x => x.CommodityLineCount).Returns(i);
				using var form = new EnhancedValidationForm(wrapperMock.Object);
				form.Show();
				var (line1Label, line2Label, line3Label, line4Label, line5Label) = FindCommodityLabels(form);
				CombineAssertions(() =>
				{
					AssertEquals("CommodityLine1Label.Visible", expected: true, line1Label.Visible);
					AssertEquals("CommodityLine1Label.Text", "0101.01.01 01 from Here", line1Label.Text);

					AssertEquals("CommodityLine2Label.Visible", i >= 2, line2Label.Visible);
					if (i >= 2) { AssertEquals("CommodityLine2Label.Text", "0101.01.01 02 from There", line2Label.Text); }
					AssertEquals("CommodityLine3Label.Visible", i >= 3, line3Label.Visible);
					if (i >= 3) { AssertEquals("CommodityLine3Label.Text", "0101.01.01 03 from Everywhere", line3Label.Text); }
					AssertEquals("CommodityLine4Label.Visible", i >= 4, line4Label.Visible);
					if (i >= 4) { AssertEquals("CommodityLine3Label.Text", "0101.01.01 04 from Nowhere", line4Label.Text); }
					AssertEquals("CommodityLine5Label.Visible", i >= 5, line5Label.Visible);
					if (i >= 5) { AssertEquals("CommodityLine3Label.Text", "0101.01.01 05 from Elsewhere", line5Label.Text); }
				});
			}
		}

		public void TestTaxOverridesLabels()
		{
			var wrapperMock = new Mock<IEnhancedValidationEntryWrapper>();
			_ = wrapperMock.Setup(x => x.TaxLine1).Returns("A00 (import duty) for 0101.01.01 01");
			_ = wrapperMock.Setup(x => x.TaxLine2).Returns("A00 (import duty) for 0101.01.01 02");
			_ = wrapperMock.Setup(x => x.TaxLine3).Returns("A00 (import duty) for 0101.01.01 03");
			_ = wrapperMock.Setup(x => x.TaxLine4).Returns("A00 (import duty) for 0101.01.01 04");
			_ = wrapperMock.Setup(x => x.TaxLine5).Returns("A00 (import duty) for 0101.01.01 05");

			for (var i = 0; i < 6; ++i)
			{
				_ = wrapperMock.Setup(x => x.TaxLineCount).Returns(i);
				using var form = new EnhancedValidationForm(wrapperMock.Object);
				form.Show();
				var (label1, label2, label3, checkbox) = FindTaxOverridesExtraLabelsAndCheckBox(form);
				var (line1Label, line2Label, line3Label, line4Label, line5Label) = FindTaxOverridesLabels(form);
				CombineAssertions(() =>
				{
					AssertEquals("TaxOverridesLabel.Visible", i > 0, label1.Visible);
					AssertEquals("ManualTaxOverrideLabel.Visible", i > 0, label2.Visible);
					AssertEquals("TaxLineLabel.Visible", i > 0, label3.Visible);
					AssertEquals("ManualTaxCheckBox.Visible", i > 0, checkbox.Visible);
					AssertEquals("TaxLine1Label.Visible", i >= 1, line1Label.Visible);
					if (i >= 1) { AssertEquals("TaxLine1Label.Text", "A00 (import duty) for 0101.01.01 01", line1Label.Text); }
					AssertEquals("TaxLine2Label.Visible", i >= 2, line2Label.Visible);
					if (i >= 2) { AssertEquals("TaxLine2Label.Text", "A00 (import duty) for 0101.01.01 02", line2Label.Text); }
					AssertEquals("TaxLine3Label.Visible", i >= 3, line3Label.Visible);
					if (i >= 3) { AssertEquals("TaxLine3Label.Text", "A00 (import duty) for 0101.01.01 03", line3Label.Text); }
					AssertEquals("TaxLine4Label.Visible", i >= 4, line4Label.Visible);
					if (i >= 4) { AssertEquals("TaxLine4Label.Text", "A00 (import duty) for 0101.01.01 04", line4Label.Text); }
					AssertEquals("TaxLine5Label.Visible", i >= 5, line5Label.Visible);
					if (i >= 5) { AssertEquals("TaxLine5Label.Text", "A00 (import duty) for 0101.01.01 05", line5Label.Text); }
				});
			}
		}

		(ZButton, ZCheckBox, ZCheckBox, ZCheckBox) FindSubmitButtonAndCheckBoxes(EnhancedValidationForm form)
		{
			var submitButton = form.FindSingle<ZButton>("SubmitButton");
			var check1 = form.FindSingle<ZCheckBox>("ClassificationCheckBox");
			var check2 = form.FindSingle<ZCheckBox>("CountryCheckBox");
			var check3 = form.FindSingle<ZCheckBox>("ManualTaxCheckBox");
			CombineAssertions(() =>
			{
				AssertNotNull("Pre-requisite: SubmitButton", submitButton);
				AssertNotNull("Pre-requisite: ClassificationCheckBox", check1);
				AssertNotNull("Pre-requisite: CountryCheckBox", check2);
				AssertNotNull("Pre-requisite: ManualTaxCheckBox", check3);
			});
			return (submitButton, check1, check2, check3);
		}

		(ZLabel, ZLabel, ZLabel, ZLabel, ZLabel) FindCommodityLabels(EnhancedValidationForm form)
		{
			var label1 = form.FindSingleOrDefault<ZLabel>("CommodityLine1Label");
			var label2 = form.FindSingleOrDefault<ZLabel>("CommodityLine2Label");
			var label3 = form.FindSingleOrDefault<ZLabel>("CommodityLine3Label");
			var label4 = form.FindSingleOrDefault<ZLabel>("CommodityLine4Label");
			var label5 = form.FindSingleOrDefault<ZLabel>("CommodityLine5Label");
			CombineAssertions(() =>
			{
				AssertNotNull("Pre-requisite: CommodityLine1Label", label1);
				AssertNotNull("Pre-requisite: CommodityLine2Label", label2);
				AssertNotNull("Pre-requisite: CommodityLine3Label", label3);
				AssertNotNull("Pre-requisite: CommodityLine4Label", label4);
				AssertNotNull("Pre-requisite: CommodityLine5Label", label5);
			});
			return (label1, label2, label3, label4, label5);
		}

		(ZLabel, ZLabel, ZLabel, ZLabel, ZLabel) FindTaxOverridesLabels(EnhancedValidationForm form)
		{
			var label1 = form.FindSingleOrDefault<ZLabel>("TaxLine1Label");
			var label2 = form.FindSingleOrDefault<ZLabel>("TaxLine2Label");
			var label3 = form.FindSingleOrDefault<ZLabel>("TaxLine3Label");
			var label4 = form.FindSingleOrDefault<ZLabel>("TaxLine4Label");
			var label5 = form.FindSingleOrDefault<ZLabel>("TaxLine5Label");
			CombineAssertions(() =>
			{
				AssertNotNull("Pre-requisite: TaxLine1Label", label1);
				AssertNotNull("Pre-requisite: TaxLine2Label", label2);
				AssertNotNull("Pre-requisite: TaxLine3Label", label3);
				AssertNotNull("Pre-requisite: TaxLine4Label", label4);
				AssertNotNull("Pre-requisite: TaxLine5Label", label5);
			});
			return (label1, label2, label3, label4, label5);
		}

		(ZLabel, ZLabel, ZLabel, ZCheckBox) FindTaxOverridesExtraLabelsAndCheckBox(EnhancedValidationForm form)
		{
			var label1 = form.FindSingleOrDefault<ZLabel>("TaxOverridesLabel");
			var label2 = form.FindSingleOrDefault<ZLabel>("ManualTaxOverrideLabel");
			var label3 = form.FindSingleOrDefault<ZLabel>("TaxLineLabel");
			var checkbox = form.FindSingleOrDefault<ZCheckBox>("ManualTaxCheckBox");
			CombineAssertions(() =>
			{
				AssertNotNull("Pre-requisite: TaxOverridesLabel", label1);
				AssertNotNull("Pre-requisite: ManualTaxOverrideLabel", label2);
				AssertNotNull("Pre-requisite: TaxLineLabel", label3);
				AssertNotNull("Pre-requisite: ManualTaxCheckBox", checkbox);
			});
			return (label1, label2, label3, checkbox);
		}

		protected override Form GetFormToBashCore() => new EnhancedValidationForm(new EnhancedValidationEntryWrapper(Factory.New<Business.Declaration.CusEntryHeader>()));
		protected override bool AllowFormSizeFixed => true;
	}
}
