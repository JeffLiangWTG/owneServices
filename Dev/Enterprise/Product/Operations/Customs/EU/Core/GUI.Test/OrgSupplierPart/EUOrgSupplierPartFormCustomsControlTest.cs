using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EUOrgSupplierPartFormCustomsControl))]
	class EUOrgSupplierPartFormCustomsControlTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlGlobalTest
	{
		[RequiresSTA]
		public void TestPreviousDocsTabPage_Caption()
		{
			using var form = new ZForm();
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();
			var previousDocument = control.FindSingle<ZTabPage>("previousDocsTabPage");

			AssertEquals("[44] Previous Documents", previousDocument.CaptionResourceString.Caption);
		}

		[RequiresSTA]
		public void TestAdditionalInfosTabPage_Caption()
		{
			using var form = new ZForm();
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();
			var additionalInfo = control.FindSingle<ZTabPage>("additionalInfosTabPage");

			AssertEquals("[44] Additional Documents", additionalInfo.CaptionResourceString.Caption);
		}

		public void TestTaxTabPage_Caption()
		{
			using var form = new ZForm();
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();

			var taxTabPage = control.FindSingle<ZTabPage>("taxTabPage");
			AssertEquals("[47] Tax", taxTabPage.CaptionResourceString.Caption);
		}

		public void TestBothDetailTabPage()
		{
			using var form = new ZForm();
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();
			var detailsForBoth = control.FindSingle<ZTabPage>("BothDetailsTabPage");
			AssertNotNull(detailsForBoth.FindSingle<ZCodeFindBox>("TariffFindBoxForBoth"));
			AssertNotNull(detailsForBoth.FindSingle<ZTextBox>("CountryOfOriginTextBoxForBoth"));
		}

		[RequiresSTA]
		public void TestTabsVisibility()
		{
			using var form = new ZForm();
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();
			var details = control.FindSingle<ZTabPage>("DetailsTabPage");
			var detailsForBoth = control.FindSingle<ZTabPage>("BothDetailsTabPage");
			var supportingDocument = control.FindSingle<ZTabPage>("supportingDocsTabPage");
			var additionalInfo = control.FindSingle<ZTabPage>("additionalInfosTabPage");
			var previousDocument = control.FindSingle<ZTabPage>("previousDocsTabPage");
			var tax = control.FindSingle<ZTabPage>("taxTabPage");

			Assert(details.TabVisible);
			Assert(detailsForBoth.TabVisible);
			Assert(supportingDocument.TabVisible);
			Assert(additionalInfo.TabVisible);
			Assert(previousDocument.TabVisible);
			Assert(tax.TabVisible);
		}

		public void TestTabsVisibility_PivotChanged()
		{
			var part = Factory.NewWithValidTestData<Business.MasterFiles.OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			using var form = new ZForm(part);
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();

			AssertTabsVisible("Both", true);

			pivot.CI_ChildType = ClassificationType.IMP;
			AssertTabsVisible("IMP", false);

			pivot.CI_ChildType = ClassificationType.EXP;
			AssertTabsVisible("EXP", false);

			pivot.CI_ChildType = ZString.Empty;
			AssertTabsVisible("Empty", false);

			void AssertTabsVisible(string testCase, bool shouldBeVisibleOnBoth)
			{
				AssertEquals(testCase + "|DetailsTabPage|TabVisible", !shouldBeVisibleOnBoth, control.FindSingleOrDefault<ZTabPage>("DetailsTabPage")?.TabVisible ?? false);
				AssertEquals(testCase + "|BothDetailsTabPage|TabVisible", shouldBeVisibleOnBoth, control.FindSingleOrDefault<ZTabPage>("BothDetailsTabPage")?.TabVisible ?? false);
				AssertEquals(testCase + "|supportingDocsTabPage|TabVisible", !shouldBeVisibleOnBoth, control.FindSingleOrDefault<ZTabPage>("supportingDocsTabPage")?.TabVisible ?? false);
				AssertEquals(testCase + "|additionalInfosTabPage|TabVisible", !shouldBeVisibleOnBoth, control.FindSingleOrDefault<ZTabPage>("additionalInfosTabPage")?.TabVisible ?? false);
				AssertEquals(testCase + "|previousDocsTabPage|TabVisible", !shouldBeVisibleOnBoth, control.FindSingleOrDefault<ZTabPage>("previousDocsTabPage")?.TabVisible ?? false);
				AssertEquals(testCase + "|taxTabPage|TabVisible", false, control.FindSingleOrDefault<ZTabPage>("taxTabPage")?.TabVisible ?? false);
			}
		}

		[RequiresSTA]
		public void TestCusClassPartPivot_ChildTypeChangedConfirmation_HasDetailUnrelatedToBoth()
		{
			var part = Factory.NewWithValidTestData<Business.MasterFiles.OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.PreviousDocuments.AddNew();
			pivot.CI_ChildType = ClassificationType.IMP;

			using var form = new ZForm(part);
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("Should not have changed the value, as cancel was selected", ClassificationType.IMP, pivot.CI_ChildType);
				AssertEquals("Confirmation", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Message on display was", "System is about to delete all existing Supporting Documents, Additional Infos, Previous Documents, Taxes and some details on the Details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("Should have changed the value, as OK was selected", ClassificationType.Both, pivot.CI_ChildType);
			});
		}

		[RequiresSTA]
		public void TestCusClassPartPivot_ChildTypeChangedConfirmation_NoDetailUnrelatedToBoth()
		{
			var part = Factory.NewWithValidTestData<Business.MasterFiles.OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.IMP;

			using var form = new ZForm(part);
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("CI_ChildType", ClassificationType.Both, pivot.CI_ChildType);
				AssertEquals("No Confirmation", false, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			});
		}

		public void TestCusClassPivotHasAttributes()
		{
			using var control = new EUOrgSupplierPartFormCustomsControl();
			AssertNotNull("AttributesTabPage", control.FindSingle<ZTabPage>("AttributesTabPage"));
			AssertNotNull("Attributes1Grid", control.FindSingle<ZGrid>("Attributes1Grid"));
			AssertNotNull("Attributes2Grid", control.FindSingle<ZGrid>("Attributes2Grid"));
			AssertNotNull("Attributes3Grid", control.FindSingle<ZGrid>("Attributes3Grid"));
		}

		public void TestSupplementaryCodes()
		{
			using var control = new EUOrgSupplierPartFormCustomsControl();
			AssertNotNull("CI_AdditionalSupplementsTextBox", control.FindSingle<ZTextBox>(x => x.Name == "CI_AdditionalSupplementsTextBox"));
			AssertNotNull("AdditionalSupplementaryCodesEditButton", control.FindSingle<ZButton>(x => x.Name == "AdditionalSupplementaryCodesEditButton"));
		}

		public void TestAdditionalCPCs()
		{
			using var control = new EUOrgSupplierPartFormCustomsControl();
			AssertNotNull("AdditionalCPCMoreButton", control.FindSingle<ZButton>(x => x.Name == "AdditionalCPCMoreButton"));
			AssertNotNull("AdditionalCPCAsStringTextBox", control.FindSingle<ZTextBox>(x => x.Name == "AdditionalCPCAsStringTextBox"));
		}

		[RequiresSTA]
		public void TestTariffFindBoxAndColumn()
		{
			var part = Factory.NewWithValidTestData<Business.MasterFiles.OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.IMP;

			using var form = new ZForm(part);
			using var control = new EUOrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();
			var pivotGrid = (ZGrid)control.Controls.Find("PivotGrid", true)[0];
			var tariffColumnStyleInfo = pivotGrid.GetColumnStyle("CI_FormattedTariffNum") as Universal.GUI.TariffColumnStyleInfo;
			var tariffFindBox = control.Controls.Find("TariffFindBox", true)[0] as Universal.GUI.TariffFindBox;
			AssertNotNull("TariffColumnStyleInfo", tariffColumnStyleInfo);
			AssertNotNull(tariffFindBox);

			AssertEquals("IMP", tariffColumnStyleInfo.GetTariffType.Invoke());
			AssertEquals("IMP", tariffFindBox.GetTariffType.Invoke());

			pivot.CI_ChildType = ClassificationType.Both;
			AssertEquals("IMP", tariffColumnStyleInfo.GetTariffType.Invoke());
			AssertEquals("IMP", tariffFindBox.GetTariffType.Invoke());

			pivot.CI_ChildType = ClassificationType.EXP;
			AssertEquals("EXP", tariffColumnStyleInfo.GetTariffType.Invoke());
			AssertEquals("EXP", tariffFindBox.GetTariffType.Invoke());
		}

		protected override void AssertTariffColumns(Core.Forms.ZGridColumnInfo columnStyleInfo)
		{
			AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", columnStyleInfo);
		}

		protected override ZUserControl GetUserControl()
		{
			return new EUOrgSupplierPartFormCustomsControl();
		}

		protected override string UserControlName => "EUOrgSupplierPartFormCustomsControl";

		public class EUOrgSupplierPartFormCustomsControlForTest : EUOrgSupplierPartFormCustomsControl
		{
			public Type GetSupplierPartTaxUserControlTypeExposed() => base.GetSupplierPartTaxUserControlType();
		}
	}
}
