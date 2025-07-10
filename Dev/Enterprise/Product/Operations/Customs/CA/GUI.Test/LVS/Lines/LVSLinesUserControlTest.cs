using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVSLinesUserControlTest : TestCaseWithFactory
	{
		public void TestTariffUserControlForCAGlobalTariff()
		{
			var gridName = "LVSLinesGrid";
			var columnName = JobComInvoiceLine.Schema.JI_FormattedTariff;
			var tariffFindBoxName_TrfCA = "ClassificationNumberFindBox";
			var tariffFindBoxName_SRDb = "ClassificationNumberFrimSRDbFindBox";
			var tariffColumnInfoType = typeof(Universal.GUI.TariffColumnStyleInfo);

			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(testDeclaration))
			{
				form.Show();
				var tariffColumnInfoCaption = ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromTrfCA(form, gridName, columnName, tariffColumnInfoType);
				ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromSRDb(form, gridName, columnName, tariffColumnInfoCaption);
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(form, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}

		public void TestLVXModeGridLayout()
		{
			using (var control = new LVSLinesUserControl())
			{
				CombineAssertions("Default part attribute captions", () =>
				{
					AssertNotNull(control.LVSLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_B3LineNumber));
					AssertNull(control.LVSLinesGrid.GetColumnStyle("LVXB3LineNumber"));
				});

				control.SetLVXMode();
				CombineAssertions("Default part attribute captions", () =>
				{
					AssertNotNull(control.LVSLinesGrid.GetColumnStyle("LVXB3LineNumber"));
					AssertNull(control.LVSLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_B3LineNumber));
				});
			}
		}

		public void TestGetEffectiveImporterForLVSLinesGrid()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice = testDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var lvsLinesUserControl = testForm.Controls.Find("LVSLinesUserControl", true)[0] as LVSLinesUserControl;
				AssertEquals("Effective Importer", null, lvsLinesUserControl.GetEffectiveImporterForLVSLinesGrid());
				testDeclaration.JE_OH_Importer = orgHeader.PK;
				AssertEquals("Effective Importer", orgHeader, lvsLinesUserControl.GetEffectiveImporterForLVSLinesGrid());
				invoice.JZ_OH_Buyer = orgHeader1.PK;
				AssertEquals("Effective Importer", orgHeader1, lvsLinesUserControl.GetEffectiveImporterForLVSLinesGrid());
				lvsLinesUserControl.IsSimplifiedLVSMode = true;
				AssertEquals("Effective Importer", orgHeader, lvsLinesUserControl.GetEffectiveImporterForLVSLinesGrid());
				lvsLinesUserControl.IsSimplifiedLVSMode = false;
				testDeclaration.Invoices.RemoveAll();
				lvxJob.LVXInvoiceHeader.AttachToAdditionalDeclaration(testDeclaration);
				AssertEquals("Effective Importer", null, lvsLinesUserControl.GetEffectiveImporterForLVSLinesGrid());
				lvxJob.LVXInvoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
				AssertEquals("Effective Importer", orgHeader2, lvsLinesUserControl.GetEffectiveImporterForLVSLinesGrid());
			}
		}

		public void TestSetPartAttributeCaptionsByEvents()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice1 = testDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = testDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			Factory.Save();
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var lvsLinesGrid = testForm.Controls.Find("LVSLinesGrid", true)[0] as ZGrid;
				CombineAssertions("Default part attribute captions", () =>
				{
					AssertEquals("Part Attrib. 1", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertEquals("Part Attrib. 2", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertEquals("Part Attrib. 3", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				});

				testDeclaration.JE_OH_Importer = orgHeader.PK;
				invoice1.JZ_OH_Buyer = orgHeader1.PK;
				invoice2.JZ_OH_Buyer = orgHeader2.PK;
				CombineAssertions("Customized part attribute captions by buyer", () =>
				{
					AssertEquals("org1 attr 1", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertEquals("org1 attr 2", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertEquals("org1 attr 3", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				});

				var lvsSubHeadersGrid = testForm.Controls.Find("LVSSubHeadersGrid", true)[0] as LVSSubHeadersGrid;
				lvsSubHeadersGrid.ListManager.Position = 1;
				CombineAssertions("Customized part attribute captions by buyer", () =>
				{
					AssertEquals("org2 attr 1", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertEquals("org2 attr 2", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertEquals("org2 attr 3", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				});

				invoice2.JZ_OH_Buyer = orgHeader1.PK;
				CombineAssertions("Customized part attribute captions by importer", () =>
				{
					AssertEquals("org1 attr 1", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertEquals("org1 attr 2", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertEquals("org1 attr 3", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
				});
			}
		}

		public void TestLVSLinesSerialNumbers()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice1 = testDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			Factory.Save();
			using (var testForm = new JobDeclarationForm(testDeclaration))
			{
				testForm.Show();
				var fLVSLinesGrid = testForm.Controls.Find("LVSLinesGrid", true)[0] as ZGrid;
				AssertEquals("Serial Number column should exist", "Serial #", fLVSLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_SerialNumber));
			}
		}

		public void TestRemissionDropEditVisible()
		{
			var testDeclaration1 = Factory.New<JobDeclaration>();
			testDeclaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice1 = testDeclaration1.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			using (var testForm1 = new JobDeclarationForm(testDeclaration1))
			{
				testForm1.Show();
				invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
				AssertEquals(CalculationMethods.Codes.DeliveredDutyPaid, invoiceLine1.CA_CalculationMethod);
				var remissionDropEdit = (ZDropEdit)testForm1.Controls.Find("RemissionDropEdit", true)[0];
				AssertEquals(false, remissionDropEdit.Visible);
			}

			var testDeclaration2 = Factory.New<JobDeclaration>();
			testDeclaration2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice2 = testDeclaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals(CalculationMethods.Codes.DeliveredDutyPaid, invoiceLine2.CA_CalculationMethod);
			using (var testForm2 = new JobDeclarationForm(testDeclaration2))
			{
				testForm2.Show();
				var remissionDropEdit = (ZDropEdit)testForm2.Controls.Find("RemissionDropEdit", true)[0];
				AssertEquals(false, remissionDropEdit.Visible);
			}
		}

		public void TestRemissionTypeDropEditVisible()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var testDeclaration1 = Factory.New<JobDeclaration>();
				testDeclaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var invoice1 = testDeclaration1.Invoices.AddNew();
				var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
				using (var testForm1 = new JobDeclarationForm(testDeclaration1))
				{
					testForm1.Show();
					var remissionTypeDropEdit = (ZDropEdit)testForm1.Controls.Find("RemissionTypeDropEdit", true)[0];
					Assert("IsCADEnable == true", remissionTypeDropEdit.Visible);
				}
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var testDeclaration2 = Factory.New<JobDeclaration>();
				testDeclaration2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				var invoice2 = testDeclaration2.Invoices.AddNew();
				var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
				invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
				AssertEquals(CalculationMethods.Codes.DeliveredDutyPaid, invoiceLine2.CA_CalculationMethod);
				using (var testForm2 = new JobDeclarationForm(testDeclaration2))
				{
					testForm2.Show();
					var remissionTypeDropEdit = (ZDropEdit)testForm2.Controls.Find("RemissionTypeDropEdit", true)[0];
					Assert("IsCADEnable == false", !remissionTypeDropEdit.Visible);
				}
			}
		}

		public void TestCA_CVforCurrConv()
		{
			var usd = Enterprise.MasterFiles.Business.RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			usd.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 0.719424m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			invoiceHeader.JZ_InvoiceCurrExRate = 10;
			invoiceHeader.CA_TimeLimit = 10;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			invoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceHeader.JZ_RW_NKOriginState = Common.US.USStatesList.Codes.Alabama;
			invoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoiceHeader.CA_USStateOfExport = Common.US.USStatesList.Codes.Alabama;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_B3SubHeaderNumber = 1;
			invoiceLine.JI_LinePrice = 20;
			invoiceLine.JI_Tariff = "22101000";
			invoiceLine.CA_99TariffCode = "4901";
			invoiceLine.CA_ADJCode = AmountTypes.Codes.Percent;
			invoiceLine.CA_ADJValue = 10;
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.CA_CVforCurrConv = 20;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 30;
			invoiceLine.JI_CustomsQuantity = 40;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_CustomsSecondQuantity = 50;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Gram;
			invoiceLine.JI_CustomsThirdQuantity = 60;
			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.Chile;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.CA_RN_NKExport = Core.Constants.CountryCodes.Canada;
			invoiceLine.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.ApportionedCharges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 50m, Core.Constants.CurrencyCodes.Canada);
			invoiceLine.CA_CVforCurrConvOvr = false;
			var isRun = false;
			((IBindingList)invoiceLine).ListChanged += (sender, e) =>
			{
				isRun = true;
			};
			var cvforCurrConv = invoiceLine.CA_CVforCurrConv;
			AssertEquals(false, isRun);
		}

		public void TestCustomAttributeLabels()
		{
			var importer1 = Factory.New<OrgHeader>();
			var importer2 = Factory.New<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = importer1.PK;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var customLabels1 = importer1.CustomFormLabels.AddNew();
				customLabels1.OT_IsMandatory = true;
				customLabels1.OT_FieldName = Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1;
				customLabels1.OT_Caption = "TEST NUMBER 1";
				testForm.Show();
				var lvsLinesGrid = testForm.Controls.Find("LVSLinesGrid", true)[0] as ZGrid;
				AssertEquals(customLabels1.OT_Caption, lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1));
				AssertEquals("Custom Attribute 2", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2));
				AssertEquals("Custom Attribute 3", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3));
				AssertEquals("Custom Attribute 4", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4));
				AssertEquals("Custom Attribute 5", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5));
				AssertEquals("Custom Attribute 6", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6));

				var customLabels2 = importer2.CustomFormLabels.AddNew();
				customLabels2.OT_IsMandatory = true;
				customLabels2.OT_FieldName = Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute3;
				customLabels2.OT_Caption = "TEST NUMBER 3";

				var customLabels3 = importer2.CustomFormLabels.AddNew();
				customLabels3.OT_IsMandatory = true;
				customLabels3.OT_FieldName = Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute5;
				customLabels3.OT_Caption = "TEST NUMBER 5";

				invoice.JZ_OH_Buyer = importer2.PK;
				AssertEquals("Custom Attribute 1", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1));
				AssertEquals("Custom Attribute 2", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2));
				AssertEquals(customLabels2.OT_Caption, lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3));
				AssertEquals("Custom Attribute 4", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4));
				AssertEquals(customLabels3.OT_Caption, lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5));
				AssertEquals("Custom Attribute 6", lvsLinesGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "org";
			orgHeader.MiscServ.OM_IMPartAttrib1Name = "attr 1";
			orgHeader.MiscServ.OM_IMPartAttrib2Name = "attr 2";
			orgHeader.MiscServ.OM_IMPartAttrib3Name = "attr 3";
			orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "org1";
			orgHeader1.MiscServ.OM_IMPartAttrib1Name = "org1 attr 1";
			orgHeader1.MiscServ.OM_IMPartAttrib2Name = "org1 attr 2";
			orgHeader1.MiscServ.OM_IMPartAttrib3Name = "org1 attr 3";
			orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "org2";
			orgHeader2.MiscServ.OM_IMPartAttrib1Name = "org2 attr 1";
			orgHeader2.MiscServ.OM_IMPartAttrib2Name = "org2 attr 2";
			orgHeader2.MiscServ.OM_IMPartAttrib3Name = "org2 attr 3";
		}

		OrgHeader orgHeader;
		OrgHeader orgHeader1;
		OrgHeader orgHeader2;
	}
}
