using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(CompanyConfigurationUserControl))]
	sealed class CompanyConfigurationUserControlTest : BasherTest
	{
		public void TestCompanyConfigurationUserControl()
		{
			using (var userControl = new CompanyConfigurationUserControl())
			{
				AssertEquals("Customs Configuration GroupBox should be added", true, userControl.CustomsConfigurationGroupBox.Visible);
				AssertEquals("Caution Label should be added", "These settings should be changed exercising caution. Normally these configurations would be set up prior to using the customs system for this company. Changing any of these settings will cause the customs system for this company to behave differently.", userControl.CautionLabel.CaptionResourceString.Caption);
				AssertEquals("IsReciprocalExchangeRate DropEdit should be added", "Exchange Rate Is Reciprocal Override", userControl.IsReciprocalExchangeRateDropEdit.CaptionResourceString.Caption);
				AssertEquals("IsReciprocalHelpText Label should be added", @"Changing the ‘Exchange Rates Is Reciprocal Override’ value where Customs Exchange Rates already exist for this country should only be done where, either the Customs exchange rates are also changed to a reciprocal / non - reciprocal format OR the override of this setting is being actioned in order to correct the conflict between Accounting Exchange rate format and Customs Exchange rate format.", userControl.IsReciprocalHelpTextLabel.CaptionResourceString.Caption);
				AssertEquals("Note Label should be added", $"NOTE: In the case that exchange rate data is published by {BrandingFactory.Instance.ProductName}, changing this setting normally will have no effect.", userControl.NoteLabel.Text);
				AssertEquals("CustomsValue DropEdit should be added", "Customs Value Calculation", userControl.CustomsValueCodeDropEdit.CaptionResourceString.Caption);
				AssertEquals("VATValueCode DropEdit should be added", "VAT Value Calculation", userControl.VATValueCodeDropEdit.CaptionResourceString.Caption);
				AssertEquals("CustomsValueCodeForExport DropEdit should be added", "Customs Value Export", userControl.CustomsValueCodeForExportDropEdit.CaptionResourceString.Caption);
				AssertEquals("ValidationDateImport DropEdit should be added", "Valuation Date Import", userControl.ValuationDateImportDropEdit.CaptionResourceString.Caption);
				AssertEquals("ValidationDateExport DropEdit should be added", "Valuation Date Export", userControl.ValuationDateExportDropEdit.CaptionResourceString.Caption);
			}
		}

		public override Form GetFormToBash()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, Core.Constants.CountryCodes.Botswana, Core.Constants.CountryCodes.Botswana, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var form = new ZChildForm { CaptionRenderingEnabled = true };
			var userControl = new CompanyConfigurationUserControl { Dock = DockStyle.Fill };
			form.Controls.Add(userControl);
			var provider = (GlbCompanyWrapperProvider)GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Botswana);
			var glbCompany = Factory.New<GlbCompany>();
			glbCompany.GC_Code = "ZAC";
			form.SetDataBinding(provider.GetWrapper(glbCompany), string.Empty);
			return form;
		}

		protected override bool AllowHasChangesOnFormOpen => true;
	}
}
