using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.Show();
			AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestVehiclesTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var frm = new ZForm(declaration))
		using (var userControl = new ImportInvoiceLineUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabPage = userControl.FindSingle<ZTabPage>("VehiclesTabPage");
			tabPage.Show();
			CombineAssertions(() =>
			{
				AssertEquals("VehiclesTabPage visible", true, tabPage.TabVisible);
				AssertEquals("Caption", "Vehicles", tabPage.CaptionResourceString.Caption);
			});
		}
	}

	public void TestAdditionalDocumentsTabPageWhenNotUCC6()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		using (var frm = new ZForm(declaration))
		using (var userControl = new ImportInvoiceLineUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalDocumentsTabPage");

			AssertEquals("AdditionalDocumentsTabPage visible", expected: true, tabPage.TabVisible);
			AssertEquals("Caption", "Additional Documents", tabPage.CaptionResourceString.Caption);
		}
	}

	public void TestAdditionalDocumentsTabPageWhenUCC6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		using (var frm = new ZForm(declaration))
		using (var userControl = new ImportInvoiceLineUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabControl = userControl.LineDetailTabControl;
			var additionalDocumentsTabPage = tabControl.FindSingleOrDefault<ZTabPage>("AdditionalDocumentsTabPage");
			var addInfoTabPage = tabControl.FindSingle<ZTabPage>("AdditionalInfosTabPage");

			AssertNull("Additional Documents tab page should not exist", additionalDocumentsTabPage);
			AssertNotNull("Add Info Documents tab page should exist", addInfoTabPage);
		}
	}

	public void TestAddtionalProcedureCodeAsStringName()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		using (var form = new ZForm(declaration))
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				form.Controls.Add(control);
				form.Show();

				AssertEquals("[37.2] Nat./UE Reg.", control.FindSingle<EU.GUI.AdditionalProcedureCodesUserControl>(x => x.Name == "AdditionalProcedureCodesUserControl").GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}
	}

	public void TestMethodOfPaymentDropEdit()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			var methodOfPaymentDropEdit = control.FindSingle<ZDropEdit>("MethodOfPaymentDropEdit");
			Assert(methodOfPaymentDropEdit.Visible);
		}
	}

	public void TestESVisibility()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var nif = (ZDropEdit)control.Controls.Find("VatTypeDropEdit", true).FirstOrDefault();
			AssertEquals(false, nif.Visible);
			var adjCode = (ZDropEdit)control.Controls.Find("ValuationAdjustmentCodeDropEdit", true).FirstOrDefault();
			AssertEquals(false, adjCode.Visible);
			var adjPercent = (ZArchitecture.ZCalcEdit)control.Controls.Find("ValuationAdjustmentPercentageCalcEdit", true).FirstOrDefault();
			AssertEquals(false, adjPercent.Visible);
		}
	}

	public void TestLineCalculationsLabels()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		invoice.InvoiceLines.AddNew();

		using (var form = new ZForm(declaration))
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("IGIC", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "JI_Calc_GSTConvertToLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Def. IGIC", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("IGIC Value", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "ValueForGstVatLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("IGIC Additions", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "VATAdditionsLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);

					declaration.ZG_DestinationState = "zz";
					AssertEquals("VAT", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "JI_Calc_GSTConvertToLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Def. VAT", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("VAT Value", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "ValueForGstVatLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("VAT Additions", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "VATAdditionsLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
				});
			}
		}
	}

	public void TestFourthQtyCalcDropEditVisible()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var fourthQtyCalcDropEdit = control.Controls.Find("FourthQtyCalcDropEdit", true)[0];
			AssertNotNull("For Import Declaration, Fourth Quantity must be available on the screen", fourthQtyCalcDropEdit);
			AssertEquals("Fourth Quantity should be visible for Import declarations in ES", true, fourthQtyCalcDropEdit.Visible);
		}
	}

	public void TestCustomsValueLocalCurrencyControlBinding()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var customsValueLocalCurrencyControl = control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "CustomsValueLocalCurrencyControl");
			AssertEquals("CustomsValueLocalCurrencyControl's amount is bound to new JI_Calc_ESCustomsValue fields", "FilteredInvoiceLines.JI_Calc_ESCustomsValue", customsValueLocalCurrencyControl.BindToAmount);
		}
	}

	public void TestCountryOfDestination()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = MessageTypeList.Codes.Import;
		dec.Invoices.AddNew().InvoiceLines.AddNew();
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.SetDataBinding(dec, ZString.Empty);
			form.Controls.Add(control);
			control.JobDeclaration = dec;
			form.Show();
			AssertEquals("Country of Destination", control.FindSingle<ZCodeFindBox>(x => x.Name == "CountryOfDestinationCodeFindBox").GetExtension<ILabelCaptionRenderer>().Caption);
		}
	}

	public void TestGetOrganizationsUserControlType()
	{
		using (var userControl = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("OrganizationsUserControlType", typeof(ImportInvoiceLineOrganizationsUserControl), userControl.GetOrganizationsUserControlTypeExposed());
		}
	}

	public void TestGetValuationIndicatorsUserControlType()
	{
		using (var userControl = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("ValuationIndicatorsUserControlType", typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl), userControl.GetValuationIndicatorsUserControlTypeExposed());
		}
	}
}

sealed class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
{
	public Type GetOrganizationsUserControlTypeExposed() => GetOrganizationsUserControlType();

	public Type GetValuationIndicatorsUserControlTypeExposed() => GetValuationIndicatorsUserControlType();
}
