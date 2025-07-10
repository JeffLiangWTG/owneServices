using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SADLineCommonWrapperTest<TLineWrapper> : TestCaseWithFactory
	where TLineWrapper : ILineCommon
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("An entry line is required", () => GetLineWrapper(null));
		AssertNoExceptionThrown("An entry line with an entry header and an entry instruction is required", () => GetLineWrapper(entryLine));
	}

	public void TestContainers()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertArrayEqualsByElements("Containers should be", Array.Empty<ZString>(), sadLineWrapper.Containers.ToArray());

		var declaration = Factory.New<JobDeclaration>();
		var container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "CNT1";
		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "CNT2";

		var invoiceLine1 = entryLine.InvoiceLines.AddNew();
		var container1InvoiceLine1 = invoiceLine1.ContainersPivot.AddNew();
		container1InvoiceLine1.C2_CO = container1.PK;
		var invoiceLine2 = entryLine.InvoiceLines.AddNew();
		var container1InvoiceLine2 = invoiceLine2.ContainersPivot.AddNew();
		container1InvoiceLine2.C2_CO = container2.PK;

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertArrayEqualsByElements("Containers should be", new ZString[] { "CNT1", "CNT2" }, sadLineWrapper.Containers.ToArray());
	}

	public void TestGoodsDescription()
	{
		var invoiceLine1 = entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_Description = "Tubi lanciamissili; lanciafiamme; lanciagranate; lanciasiluri e dispositivi di lancio simili";
		var invoiceLine2 = entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_Description = "Rivoltelle e pistole, diverse da quelle delle voci)9303)o 9304";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Goods Description should be", "Tubi lanciamissili; lanciafiamme; lanciagranate; lanciasiluri e dispositivi di lancio simili", sadLineWrapper.GoodsDescription);

		invoiceLine1.JI_Description = new string('0', 141);
		AssertEquals("Goods Description should be truncated to 140 chars", 140, sadLineWrapper.GoodsDescription.Length);
		AssertEquals("Goods Description should be", new string('0', 140), sadLineWrapper.GoodsDescription);
	}

	public void TestItemNumber()
	{
		entryLine.CL_LineNumber = 1;

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Item Number should be", 1, sadLineWrapper.ItemNumber);
	}

	public void TestCombinedNomenclature()
	{
		entryLine.CL_AdValoremTariff = "9302000000";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Combined Nomenclature should be", "9302000000", sadLineWrapper.CombinedNomenclature);
	}

	public void TestAdditionalCodes()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNotNull("AdditionalCodes", sadLineWrapper.AdditionalCodes);
		AssertEquals("AdditionalCodes count", 0, sadLineWrapper.AdditionalCodes.Count());

		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_SupplementaryCode1 = "XXX";
		invoiceLine1.JI_SupplementaryCode2 = "YYY";
		var additionalCode1 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode1.CY_Code = "S001";
		var additionalCode2 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode2.CY_Code = "S002";
		var additionalCode3 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode3.CY_Code = "S003";
		var additionalCode4 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode4.CY_Code = "S004";
		var additionalCode5 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode5.CY_Code = "S005";
		var additionalCodeEmpty = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCodeEmpty.CY_Code = "";

		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var additionalCode6 = invoiceLine2.AdditionalSupplementaryCodes.AddNew();
		additionalCode6.CY_Code = "S001";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertArrayEqualsByElements("Additional Codes should be", new ZString[] { "S001", "S002", "S003", "S004", "S005", "XXX", "YYY" }, sadLineWrapper.AdditionalCodes.ToArray());
	}

	public void TestGrossMass()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Gross Mass should be", 0m, sadLineWrapper.GrossMass);

		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_WeightUQ = "KG";
		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 2000m;
		invoiceLine2.JI_WeightUQ = "G";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Gross Mass should be", 102m, sadLineWrapper.GrossMass);
	}

	public void TestProcedure()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_Procedure = "4000ABC";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Procedure should be", "4000", sadLineWrapper.Procedure);

		invoiceLine1.JI_Procedure = "4900";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Procedure should be", "4900", sadLineWrapper.Procedure);

		invoiceLine1.JI_Procedure = "11";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Procedure should be", "11", sadLineWrapper.Procedure);

		invoiceLine1.JI_Procedure = "";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Procedure should be", "", sadLineWrapper.Procedure);
	}

	public void TestNationalProcedures()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_Procedure = "4000ABC";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertSequencesEqual("National Procedures should be", new ZString[] { "ABC" }, sadLineWrapper.NationalProcedures);

		invoiceLine1.JI_Procedure = "";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertSequencesEqual("National Procedures should be", ExpectedNationalProcedures, sadLineWrapper.NationalProcedures);
	}

	public void TestNetMass()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Net Mass should be", 0m, sadLineWrapper.NetMass);

		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_CustomsQuantity = 100m;
		invoiceLine1.JI_CustomsUnitQty = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;

		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_CustomsQuantity = 2000m;
		invoiceLine2.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Net Mass should be", 102m, sadLineWrapper.NetMass);
	}

	public void TestPreviousAdministrativeDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;

		CombineAssertions("When no previous documents are found", () =>
		{
			sadLineWrapper = GetLineWrapper(entryLine);
			var previousAdministrativeDocument = sadLineWrapper.PreviousAdministrativeDocument;
			AssertNotNull("Previous document should not be null", previousAdministrativeDocument);
			AssertType<SADEmptyPreviousDocumentWrapper>("Previous Document type", previousAdministrativeDocument);
		});

		var previousDocument1 = invoiceLine1.PreviousDocuments.AddNew();

		CombineAssertions("When only 1 previous document is found", () =>
		{
			previousDocument1.CSI_Procedure = "A3";
			previousDocument1.CSI_ReferenceNumber = "1";
			declaration.ResetApportionedPreviousDocuments();
			sadLineWrapper = GetLineWrapper(entryLine);
			var previousAdministrativeDocument = sadLineWrapper.PreviousAdministrativeDocument;
			AssertNotNull("Previous document should not be null", previousAdministrativeDocument);
			AssertType<SADPreviousDocumentWrapper>("Previous Document type", previousAdministrativeDocument);
		});

		var previousDocument2 = invoiceLine1.PreviousDocuments.AddNew();

		if (declaration.IsImport)
		{
			CombineAssertions("When 1PA and 1RP documents are found and the declaration is IMP", () =>
			{
				previousDocument2.CSI_Procedure = "2";
				declaration.ResetApportionedPreviousDocuments();
				sadLineWrapper = GetLineWrapper(entryLine);
				var previousAdministrativeDocument = sadLineWrapper.PreviousAdministrativeDocument;
				AssertNotNull("Previous document should not be null", previousAdministrativeDocument);
				AssertType<SADPreviousDocumentWrapper>("Previous Document type", previousAdministrativeDocument);
				AssertEquals("Previous Document should be", "A3", previousAdministrativeDocument.Register);
			});
		}
		else
		{
			CombineAssertions("When 1PA and 1RP documents are found and the declaration is EXP or COM, M2 statement is required", () =>
			{
				previousDocument2.CSI_Procedure = "2";
				AssertPreviousAdministrativeDocumentIsM2Indicator();
			});
		}

		CombineAssertions("When 2 PA documents are found, M2 statement is required", () =>
		{
			previousDocument2.CSI_Procedure = "A3";
			previousDocument2.CSI_ReferenceNumber = "2";
			AssertPreviousAdministrativeDocumentIsM2Indicator();
		});

		CombineAssertions("When 2 or more RP documents are found, M2 statement is required", () =>
		{
			previousDocument1.CSI_Procedure = "2";
			previousDocument1.CSI_ReferenceNumber = "1";
			previousDocument2.CSI_Procedure = "2";
			previousDocument2.CSI_ReferenceNumber = "2";
			var previousDocument3 = invoiceLine1.PreviousDocuments.AddNew();
			previousDocument3.CSI_Procedure = "4";
			previousDocument3.CSI_ReferenceNumber = "3";
			declaration.ResetApportionedPreviousDocuments();
			sadLineWrapper = GetLineWrapper(entryLine);
			var previousAdministrativeDocument = sadLineWrapper.PreviousAdministrativeDocument;
			AssertNotNull("Previous document should not be null", previousAdministrativeDocument);
			AssertType<SADPreviousDocumentM2IndicatorWrapper>("Previous Document type", previousAdministrativeDocument);
		});

		void AssertPreviousAdministrativeDocumentIsM2Indicator()
		{
			declaration.ResetApportionedPreviousDocuments();
			sadLineWrapper = GetLineWrapper(entryLine);
			var previousAdministrativeDocument = sadLineWrapper.PreviousAdministrativeDocument;
			AssertNotNull("Previous document should not be null", previousAdministrativeDocument);
			AssertType<SADPreviousDocumentM2IndicatorWrapper>("Previous Document type", previousAdministrativeDocument);
		}
	}

	public void TestSupplementaryUnit()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Supplementary Unit should be", null, sadLineWrapper.SupplementaryUnit);

		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_CustomsSecondQuantity = 200m;
		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_CustomsSecondQuantity = 500m;

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Supplementary Unit should be", 700m, sadLineWrapper.SupplementaryUnit);

		invoiceLine1.JI_CustomsSecondQuantity = 0m;
		invoiceLine2.JI_CustomsSecondQuantity = 0m;
		AssertNull(sadLineWrapper.SupplementaryUnit);
	}

	public void TestCertificates()
	{
		var supportingDocument1InvoiceHeader = GetSupportingDocument("AAA", "IT", "2019", "1", 1m, "KG", "DER");
		var supportingDocument2InvoiceHeader = GetSupportingDocument("AAA", "IT", "2019", "2", 1m, "KG", "PAP");
		var supportingDocument3InvoiceLine = GetSupportingDocument("AAA", "IT", "2019", "2", 1m, "KG", "PAP");
		var supportingDocument4InvoiceLine = GetSupportingDocument("BBB", "IT", "2019", "2", 1m, "KG", "PAP");

		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		invoiceHeader.SupportingDocuments.Add(supportingDocument1InvoiceHeader);
		invoiceHeader.SupportingDocuments.Add(supportingDocument2InvoiceHeader);

		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_JZ = invoiceHeader.PK;
		invoiceLine1.SupportingDocuments.Add(supportingDocument3InvoiceLine);
		invoiceLine1.SupportingDocuments.Add(supportingDocument4InvoiceLine);

		sadLineWrapper = GetLineWrapper(entryLine);
		var certificates = sadLineWrapper.Certificates;
		AssertNotNull("Certificates should not be null", certificates);
		AssertEquals("Certificates count should be", 3, certificates.Count());
		AssertType<SADCertificateWrapper>("Certificates class type should be", certificates.ElementAt(0));

		TestCertificate((SADCertificateWrapper)certificates.ElementAt(0), "AAA", "IT", "2019", "1", 1m, "KG", true, false);
		TestCertificate((SADCertificateWrapper)certificates.ElementAt(1), "AAA", "IT", "2019", "2", 2m, "KG", false, true);
		TestCertificate((SADCertificateWrapper)certificates.ElementAt(2), "BBB", "IT", "2019", "2", 1m, "KG", false, true);

		AssertExceptionThrown<InvalidOperationException>("An exception should be raised", () =>
		{
			for (int i = 0; i < 99; i++)
			{
				var supportingDocument = GetSupportingDocument("AAA", "", "", i.ToString(), null, "", "");
				invoiceLine1.SupportingDocuments.Add(supportingDocument);
			}
			sadLineWrapper = GetLineWrapper(entryLine);
			sadLineWrapper.Certificates.ToList();
		});
	}

	[TestDate(2019, 12, 18)]
	public void TestCertificate10YY()
	{
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_JZ = invoiceHeader.PK;
		invoiceLine1.JI_CustomsThirdUnitQty = ZString.Empty;
		invoiceLine1.JI_CustomsThirdQuantity = 0;

		sadLineWrapper = GetLineWrapper(entryLine);
		var certificates = sadLineWrapper.Certificates;
		AssertEquals("If no third quantity is set Certificates.Count() should be", 0, certificates.Count());

		invoiceLine1.JI_CustomsThirdUnitQty = "DTNZ";
		sadLineWrapper = GetLineWrapper(entryLine);
		certificates = sadLineWrapper.Certificates;
		AssertEquals("If Third quantity unit measure is set Certificates.Count() should be", 0, certificates.Count());

		invoiceLine1.JI_CustomsThirdUnitQty = ZString.Empty;
		invoiceLine1.JI_CustomsThirdQuantity = 100;
		sadLineWrapper = GetLineWrapper(entryLine);
		certificates = sadLineWrapper.Certificates;
		AssertEquals("If Third quantity is set Certificates.Count() should be", 1, certificates.Count());
		AssertType<SADCertificate10YYWrapper>(certificates.ElementAt(0));
		TestCertificate((SADCertificate10YYWrapper)certificates.ElementAt(0), documentType: "10YY", quantity: 100m);

		invoiceLine1.JI_CustomsThirdUnitQty = "DTNZ";
		invoiceLine1.JI_CustomsThirdQuantity = 100;
		sadLineWrapper = GetLineWrapper(entryLine);
		certificates = sadLineWrapper.Certificates;
		AssertEquals("If Third quantity or third quantity UOM is set Certificates.Count() should be", 1, certificates.Count());
		TestCertificate((SADCertificate10YYWrapper)certificates.ElementAt(0), documentType: "10YY", quantity: 100m);

		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_JZ = invoiceHeader.PK;

		invoiceLine2.JI_CustomsThirdUnitQty = ZString.Empty;
		invoiceLine2.JI_CustomsThirdQuantity = 0;
		sadLineWrapper = GetLineWrapper(entryLine);
		certificates = sadLineWrapper.Certificates;
		AssertEquals("If Third quantity or third quantity UOM is set Certificates.Count() should be", 1, certificates.Count());
		TestCertificate((SADCertificate10YYWrapper)certificates.ElementAt(0), documentType: "10YY", quantity: 100m);

		invoiceLine2.JI_CustomsThirdUnitQty = "DTNZ";
		invoiceLine2.JI_CustomsThirdQuantity = 150;
		sadLineWrapper = GetLineWrapper(entryLine);
		certificates = sadLineWrapper.Certificates;
		AssertEquals("If Third quantity or third quantity UOM is set Certificates.Count() should be", 1, certificates.Count());
		TestCertificate((SADCertificate10YYWrapper)certificates.ElementAt(0), documentType: "10YY", quantity: 250m);

		invoiceLine2.JI_CustomsThirdUnitQty = "ASVX";
		sadLineWrapper = GetLineWrapper(entryLine);
		certificates = sadLineWrapper.Certificates;
		AssertEquals("If Third quantity or third quantity UOM is set Certificates.Count() should be", 1, certificates.Count());
		TestCertificate((SADCertificate10YYWrapper)certificates.ElementAt(0), documentType: "10YY", quantity: 250m);

		AssertExceptionThrown<InvalidOperationException>("An exception should be raised", () =>
		{
			for (int i = 0; i < 99; i++)
			{
				var supportingDocument = GetSupportingDocument("AAA", "", "", i.ToString(), null, "", "");
				invoiceLine1.SupportingDocuments.Add(supportingDocument);
			}
			sadLineWrapper = GetLineWrapper(entryLine);
			sadLineWrapper.Certificates.ToList();
		});
	}

	public void TestNotes()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Notes should be", "", sadLineWrapper.Notes);
	}

	public void TestStatisticalValue()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Statistical Value should be", 0m, sadLineWrapper.StatisticalValueAmount);

		entryLine.CL_StatisticalValue = 12891.212m;
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Statistical Value should be", 12891.212m, sadLineWrapper.StatisticalValueAmount);
	}

	public void TestDuties()
	{
		var lineFee = entryLine.Fees.AddNew();
		lineFee.CF_ChargeType = "A";
		lineFee.CF_BaseValue = 10m;
		lineFee.CF_Rate = 1m;
		lineFee.CF_MethodOfCalculation = "%";
		lineFee.CF_ChargeAmount = 112.12m;
		lineFee.CF_MethodOfPayment = "A";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNotNull("Duties should not be null", sadLineWrapper.Duties);
		AssertEquals("Duties count", 1, sadLineWrapper.Duties.Count());
		var duty = sadLineWrapper.Duties.ElementAt(0);
		AssertType<SADDutyTaxFeeWrapper>("Duties type", duty);
		CombineAssertions("Duty", () =>
		{
			AssertEquals("Type should be", "A", duty.Type);
			AssertEquals("Base should be", 10m, duty.Base);
			AssertEquals("CalculationFactor1 should be", "X", duty.CalculationFactor1);
			AssertEquals("Rate1 should be", 1m, duty.Rate1);
			AssertEquals("CalculationFactor2 should be", "%", duty.CalculationFactor2);
			AssertEquals("Rate2 should be", null, duty.Rate2);
			AssertEquals("CalculationFactor3 should be", "", duty.CalculationFactor3);
			AssertEquals("Rate3 should be", null, duty.Rate3);
			AssertEquals("CalculationFactor4 should be", "", duty.CalculationFactor4);
			AssertEquals("Amount should be", 112.12m, duty.Amount);
			AssertEquals("MethodOfPayment should be", "A", duty.MethodOfPayment);
		});
	}

	public void TestDutiesOrder()
	{
		SetUpFees(entryLine.Fees, "407", "406", "405", "927", "911", "201", "165", "116", "A35", "A30", "A20", "A10", "A00");
		AssertEquals("entryLine.Fees count", 13, entryLine.Fees.Count);

		var lineWrapper = IMLineWrapperFactory.GetIMLineWrapper(entryLine);
		var wrappedDuties = lineWrapper.Duties;
		AssertNotNull("lineWrapper.Duties not null", wrappedDuties);
		AssertEquals("lineWrapper.Duties count", 13, wrappedDuties.Count());
		AssertArrayEqualsByElements("lineWrapper.Duties order", new ZString[] { "A00", "A10", "A20", "A30", "A35", "116", "165", "201", "911", "927", "405", "406", "407" }, wrappedDuties.Select(x => x.Type).ToArray());
	}

	public void TestExcludedDuties()
	{
		AddLineFee(entryLine, UniversalReferenceConstants.DutyMethodOfPayment.ImmediatePaymentInCashA);
		AddLineFee(entryLine, UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentCustomsProcedureF);
		AddLineFee(entryLine, UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE);

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNotNull("Duties should not be null", sadLineWrapper.Duties);
		AssertEquals("Duties count", 2, sadLineWrapper.Duties.Count());

		AddLineFee(entryLine, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR, UniversalReferenceConstants.RefCusRateCodes.TemporaryAntiDumpingDuty);
		AddLineFee(entryLine, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR, UniversalReferenceConstants.RefCusRateCodes.TemportaryCountervailingDuty);
		AddLineFee(entryLine, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR);

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNotNull("Duties should not be null", sadLineWrapper.Duties);
		AssertEquals("Duties count", 4, sadLineWrapper.Duties.Count());

		void AddLineFee(CusEntryLine entryLine, ZString methodOfPayment, string chargeType = null)
		{
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_MethodOfPayment = methodOfPayment;
			lineFee.CF_ChargeType = chargeType;
		}
	}

	public void TestTotalItemTaxedAmount()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("TotalItemTaxedAmount", 0m, sadLineWrapper.TotalItemTaxedAmount);

		var feeA00 = entryLine.Fees.AddNew();
		feeA00.CF_ChargeType = "DTY";
		feeA00.CF_ChargeAmount = 140m;

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("TotalItemTaxedAmount", 140m, sadLineWrapper.TotalItemTaxedAmount);

		var feeCvd = entryLine.Fees.AddNew();
		feeCvd.CF_ChargeType = "CVD";
		feeCvd.CF_ChargeAmount = 160m;

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("TotalItemTaxedAmount", 300m, sadLineWrapper.TotalItemTaxedAmount);

		var feeADD = entryLine.Fees.AddNew();
		feeADD.CF_ChargeType = "ADD";
		feeADD.CF_ChargeAmount = 180;

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("TotalItemTaxedAmount", 480m, sadLineWrapper.TotalItemTaxedAmount);

		var fee405 = entryLine.Fees.AddNew();
		fee405.CF_ChargeType = "VAT";
		fee405.CF_ChargeAmount = 20m;

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("TotalItemTaxedAmount", 500m, sadLineWrapper.TotalItemTaxedAmount);
	}

	public void TestGrandTotalTaxedAmount()
	{
		entryLine.CL_LineNumber = 1;
		var fee11 = entryLine.Fees.AddNew();
		fee11.CF_ChargeAmount = 40.3485;
		fee11.CF_MethodOfPayment = "A";

		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		var fee21 = entryLine2.Fees.AddNew();
		fee21.CF_ChargeAmount = 10.455;
		fee21.CF_MethodOfPayment = "A";

		var entryLine3 = entryHeader.MergedLines.AddNew();
		entryLine3.CL_LineNumber = 3;
		var fee31 = entryLine3.Fees.AddNew();
		fee31.CF_ChargeAmount = 20.9999;
		fee31.CF_MethodOfPayment = "A";
		var fee32 = entryLine3.Fees.AddNew();
		fee32.CF_ChargeAmount = 30.578;
		fee32.CF_MethodOfPayment = "A";

		var sadLineWrapper1 = GetLineWrapper(entryLine);
		AssertNull("GrandTotalTaxedAmount should be null", sadLineWrapper1.GrandTotalTaxedAmount);

		var sadLineWrapper2 = GetLineWrapper(entryLine2);
		AssertNull("GrandTotalTaxedAmount should be null", sadLineWrapper2.GrandTotalTaxedAmount);

		var sadLineWrapper3 = GetLineWrapper(entryLine3);
		AssertEquals("GrandTotalTaxedAmount", 102.39m, sadLineWrapper3.GrandTotalTaxedAmount);
	}

	public abstract void TestCountryOfOrigin();

	protected SupportingDocument GetSupportingDocument(ZString documentType, ZString countryOfIssue, ZString issuingYear, ZString reference, ZDecimal? quantity, ZString unitOfMeasurement, ZString status)
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		supportingDocument.CSI_Code = documentType;
		supportingDocument.CSI_RN_NKCountryCode = countryOfIssue;
		supportingDocument.CSI_YearOfIssue = issuingYear;
		supportingDocument.CSI_ReferenceNumber = reference;
		supportingDocument.CSI_Quantity = quantity ?? ZDecimal.Zero;
		supportingDocument.CSI_UnitOfQuantity = unitOfMeasurement;
		supportingDocument.CSI_Status = status;
		return supportingDocument;
	}

	protected void TestCertificate(ICertificate certToTest, string documentType = "", string countryOfIssue = "", string issuingYear = "", string reference = "", ZDecimal? quantity = null, string unitOfMeasurement = "", bool derogationFlag = false, bool retrospectiveDerogationFlag = false)
	{
		CombineAssertions(() =>
		{
			AssertEquals("Certificate.DocumentType should be", documentType, certToTest.DocumentType);
			AssertEquals("Certificate.CountryOfIssue should be", countryOfIssue, certToTest.CountryOfIssue);
			AssertEquals("Certificate.IssuingYear should be", issuingYear, certToTest.IssuingYear);
			AssertEquals("Certificate.Reference should be", reference, certToTest.Reference);
			AssertEquals("Certificate.Quantity should be", quantity, certToTest.Quantity);
			AssertEquals("Certificate.UnitOfMeasurement should be", unitOfMeasurement, certToTest.UnitOfMeasurement);
			AssertEquals("Certificate.DerogationFlag should be", derogationFlag, certToTest.DerogationFlag);
			AssertEquals("Certificate.RetrospectiveDerogationFlag should be", retrospectiveDerogationFlag, certToTest.RetrospectiveDerogationFlag);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryLine = entryHeader.MergedLines.AddNew();
		sadLineWrapper = GetLineWrapper(entryLine);
	}

	protected abstract TLineWrapper GetLineWrapper(CusEntryLine entryLine);

	protected JobDeclaration jobDeclaration;
	protected CusEntryHeader entryHeader;
	protected CusEntryInstruction entryInstruction;
	protected CusEntryLine entryLine;
	protected TLineWrapper sadLineWrapper;
	protected virtual IReadOnlyList<ZString> ExpectedNationalProcedures { get; }

	void SetUpFees(CusEntryLineFeeCollection lineFeeCollection, params ZString[] rateCodesToAdd)
	{
		foreach (var rateCode in rateCodesToAdd)
		{
			lineFeeCollection.AddOrUpdate(rateCode, 0m);
		}
	}
}
