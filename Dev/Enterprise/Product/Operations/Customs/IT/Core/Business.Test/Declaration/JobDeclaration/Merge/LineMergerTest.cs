using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class LineMergerTest : EU.Business.Testing.LineMergerTest
{
	public void TestDoMergeForEntryHeader()
	{
		var declaration = base.Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		void restoreDeclarationToDefault()
		{
			declaration.JE_MessageType = "IMP";
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CEI = entryInstruction1.PK;
			invoice1.JZ_IncoTerm = "EXW";
			invoice2.JZ_IncoTerm = "EXW";
			invoice1.JZ_ValuationCode = "20";
			invoice2.JZ_ValuationCode = "20";
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		}
		restoreDeclarationToDefault();
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine2.JI_CEI = entryInstruction2.PK;
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 2, declaration.CustomsEntryHeaders.Count);

		restoreDeclarationToDefault();
		entryInstruction1.CEI_Procedure = "40";
		invoiceLine2.JI_CEI = entryInstruction2.PK;
		entryInstruction2.CEI_Procedure = "44";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 2, declaration.CustomsEntryHeaders.Count);

		restoreDeclarationToDefault();
		invoice2.JZ_IncoTerm = "FCA";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 2, declaration.CustomsEntryHeaders.Count);

		restoreDeclarationToDefault();
		invoice2.JZ_ValuationCode = "10";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 2, declaration.CustomsEntryHeaders.Count);

		restoreDeclarationToDefault();
		invoice2.JZ_RX_NKInvoice_Currency = "AUD";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 2, declaration.CustomsEntryHeaders.Count);
	}

	public override void TestLineMergerCreateOneEntryHeader()
	{
		var declaration = base.Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		var invoiceLine3 = invoice2.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = "IMP";
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "80";
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "81";
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "82";
		invoice1.JZ_IncoTerm = "EXW";
		invoice2.JZ_IncoTerm = "EXW";
		invoice1.JZ_ValuationCode = "20";
		invoice2.JZ_ValuationCode = "20";
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();

		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		AssertEquals("Number of CusEntryLines should be", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);
	}

	public void TestDoMergeByNoMerge()
	{
		var declaration = GetDeclarationReadyForDoMergeTest();
		declaration.JE_MergeBy = "NON";
		declaration.DoMerge();

		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		AssertEquals("Number of CusEntryLine should be", 8, declaration.CustomsEntryHeaders[0].MergedLines.Count);
	}

	public void TestDoMergeByTariff()
	{
		var declaration = GetDeclarationReadyForDoMergeTest();
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		void assertMergedResult()
		{
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Number of CusEntryLine should be", 2, entry.MergedLines.Count);
			var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.Tariff).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Tariff Cus Entry Line should be", "80", orderedMergedLines[0].Tariff);
				AssertEquals("Tariff Cus Entry Line should be", "81", orderedMergedLines[1].Tariff);
			});
		}
		assertMergedResult();

		declaration.JE_MergeBy = "TRM";
		declaration.DoMerge();
		assertMergedResult();
	}

	public void TestDoMergeByTariffAndDescription()
	{
		var declaration = GetDeclarationReadyForDoMergeTest();
		declaration.JE_MergeBy = "TRD";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 3, entry.MergedLines.Count);
		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.Tariff).ThenBy(x => x.EffectiveDescription).ToArray();
		CombineAssertions(delegate
		{
			AssertEquals("Tariff Cus Entry Line should be", "80", orderedMergedLines[0].Tariff);
			AssertEquals("Description Cus Entry Line should be", "Description1", orderedMergedLines[0].EffectiveDescription);
			AssertEquals("Tariff Cus Entry Line should be", "81", orderedMergedLines[1].Tariff);
			AssertEquals("Description Cus Entry Line should be", "Description2", orderedMergedLines[1].EffectiveDescription);
			AssertEquals("Tariff Cus Entry Line should be", "81", orderedMergedLines[2].Tariff);
			AssertEquals("Description Cus Entry Line should be", "Description3", orderedMergedLines[2].EffectiveDescription);
		});
	}

	public void TestDoMergeByClassification()
	{
		var declaration = GetDeclarationReadyForDoMergeTest();
		declaration.JE_MergeBy = "CLS";
		declaration.DoMerge();
		void assertMergeResult()
		{
			AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Number of CusEntryLine should be", 3, entry.MergedLines.Count);
			var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.Tariff).ThenBy(x => x?.RandomLine?.Classification?.CC_LookupCode ?? ZString.Empty).ToArray();
			CombineAssertions(delegate
			{
				AssertEquals("Tariff Cus Entry Line should be", "80", orderedMergedLines[0].Tariff);
				AssertEquals("Classification Cus Entry Line should be", "Lookup1", orderedMergedLines[0].RandomLine.Classification.CC_LookupCode);
				AssertEquals("Tariff Cus Entry Line should be", "81", orderedMergedLines[1].Tariff);
				AssertEquals("Classification Cus Entry Line should be", "Lookup2", orderedMergedLines[1].RandomLine.Classification.CC_LookupCode);
				AssertEquals("Tariff Cus Entry Line should be", "81", orderedMergedLines[2].Tariff);
				AssertEquals("Classification Cus Entry Line should be", "Lookup3", orderedMergedLines[2].RandomLine.Classification.CC_LookupCode);
			});
		}
		assertMergeResult();
		declaration.JE_MergeBy = "CLD";
		declaration.DoMerge();
		assertMergeResult();
	}

	public void TestDoMergeByPartNo()
	{
		var declaration = GetDeclarationReadyForDoMergeTest();
		declaration.JE_MergeBy = "PNO";
		declaration.DoMerge();
		void assertMergeResult()
		{
			AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Number of CusEntryLine should be", 3, entry.MergedLines.Count);
			var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.Tariff).ThenBy(x => x?.RandomLine.JI_PartNo).ThenBy(x => x?.RandomLine?.Classification.CC_LookupCode ?? ZString.Empty).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Tariff Cus Entry Line should be", "80", orderedMergedLines[0].Tariff);
				AssertEquals("PartNo Cus Entry Line should be", "PartNo1", orderedMergedLines[0].RandomLine.JI_PartNo);
				AssertEquals("Classification Cus Entry Line should be", "Lookup1", orderedMergedLines[0].RandomLine.Classification.CC_LookupCode);
				AssertEquals("Tariff Cus Entry Line should be", "81", orderedMergedLines[1].Tariff);
				AssertEquals("PartNo Cus Entry Line should be", "PartNo2", orderedMergedLines[1].RandomLine.JI_PartNo);
				AssertEquals("Classification Cus Entry Line should be", "Lookup2", orderedMergedLines[1].RandomLine.Classification.CC_LookupCode);
				AssertEquals("Tariff Cus Entry Line should be", "81", orderedMergedLines[2].Tariff);
				AssertEquals("PartNo Cus Entry Line should be", "PartNo2", orderedMergedLines[2].RandomLine.JI_PartNo);
				AssertEquals("Classification Cus Entry Line should be", "Lookup3", orderedMergedLines[2].RandomLine.Classification.CC_LookupCode);
			});
		}
		assertMergeResult();

		declaration.JE_MergeBy = "PNP";
		declaration.DoMerge();
		assertMergeResult();
	}

	public void TestDoMergeWithDifferentCpc()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);
		ApplyChangesToInvoiceLine(declaration, 0, 2, x => x.JI_Procedure = "4000");
		ApplyChangesToInvoiceLine(declaration, 2, 2, x => x.JI_Procedure = "4100");
		ApplyChangesToInvoiceLine(declaration, 4, 2, x => x.JI_Procedure = "5151");
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 3, entry.MergedLines.Count);
		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.ProcedureCode).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.Cpc should be", "4000", orderedMergedLines[0].ProcedureCode);
			AssertEquals("CusEntryLine.Cpc should be", "4100", orderedMergedLines[1].ProcedureCode);
			AssertEquals("CusEntryLine.Cpc should be", "5151", orderedMergedLines[2].ProcedureCode);
		});
	}

	public void TestDoMergeWithDifferentOrigin()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);

		ApplyChangesToInvoiceLine(declaration, 0, 3, x => x.JI_CountryOfOrigin = "CN");
		ApplyChangesToInvoiceLine(declaration, 3, 6, x => x.JI_CountryOfOrigin = "US");
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 2, entry.MergedLines.Count);

		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.CountryOfOriginCode).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.Origin should be", "CN", orderedMergedLines[0].CountryOfOriginCode);
			AssertEquals("CusEntryLine.Origin should be", "US", orderedMergedLines[1].CountryOfOriginCode);
		});
	}

	public void TestDoMergeWithDifferentAdditionalProcedureCodes()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);

		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);
		ApplyChangesToInvoiceLine(declaration, 0, 3, invoiceLine =>
		{
			var additionalProcedureCode3 = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode3.CY_Code = "51";
			var additionalProcedureCode4 = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode4.CY_Code = "40";
		});

		ApplyChangesToInvoiceLine(declaration, 3, 3, invoiceLine =>
		{
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode.CY_Code = "51";
			var additionalProcedureCode2 = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode2.CY_Code = "71";
		});
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 2, entry.MergedLines.Count);

		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.RandomLine.AdditionalProcedureCodesAsString).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.AdditionalCodes should be", "40,51", orderedMergedLines[0].RandomLine.AdditionalProcedureCodesAsString);
			AssertEquals("CusEntryLine.AdditionalCodes should be", "51,71", orderedMergedLines[1].RandomLine.AdditionalProcedureCodesAsString);
		});
	}

	public void TestDoMergeWithDifferentSupportingDocumentFieldsValues()
	{
		var declaration = base.Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var jobComInvoiceLine1 = invoice.InvoiceLines.AddNew();
		jobComInvoiceLine1.JI_CEI = entryInstruction.PK;
		jobComInvoiceLine1.JI_Tariff = "1";
		jobComInvoiceLine1.JI_Description = "Sic transit";
		jobComInvoiceLine1.JI_PartNo = "K";

		var supportingDocument1 = jobComInvoiceLine1.SupportingDocuments.AddNew();

		var jobComInvoiceLine2 = invoice.InvoiceLines.AddNew();
		jobComInvoiceLine2.JI_CEI = entryInstruction.PK;
		jobComInvoiceLine2.JI_Tariff = "1";
		jobComInvoiceLine2.JI_Description = "Sic transit";
		jobComInvoiceLine2.JI_PartNo = "K";

		var supportingDocument2 = jobComInvoiceLine2.SupportingDocuments.AddNew();

		declaration.DoMerge();

		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("There should be only one entry line at this stage", 1, entry.MergedLines.Count);

		CombineAssertions(() =>
		{
			AssertSupportingDocumentForField("CSI_Code", "A", "B");
			AssertSupportingDocumentForField("CSI_ReferenceNumber", "A", "B");
			AssertSupportingDocumentForField("CSI_Status", SADConstants.CertificateFlag.DER, SADConstants.CertificateFlag.PAP);
			AssertSupportingDocumentForField("CSI_UnitOfQuantity", "KGM", "BAG");
			AssertSupportingDocumentForField("CSI_YearOfIssue", "2020", "2021");
			AssertSupportingDocumentForField("CSI_RN_NKCountryCode", "IT", "ES");
		});

		void AssertSupportingDocumentForField(string fieldName, string value1, string value2)
		{
			declaration.DoMerge();

			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("There should be only one entry line at this stage", 1, entry.MergedLines.Count);

			var originalValue1 = supportingDocument1[fieldName];
			var originalValue2 = supportingDocument2[fieldName];

			supportingDocument1[fieldName] = value1;
			supportingDocument2[fieldName] = value2;
			declaration.DoMerge();

			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals($"There should be 2 entry lines for {fieldName}", 2, entry.MergedLines.Count);

			supportingDocument1[fieldName] = originalValue1;
			supportingDocument2[fieldName] = originalValue2;
		}
	}

	public void TestDoMergeWithDifferentPreference()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);

		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);

		ApplyChangesToInvoiceLine(declaration, 0, 2, x => x.JI_PrimaryPreference = "1");
		ApplyChangesToInvoiceLine(declaration, 2, 2, x => x.JI_PrimaryPreference = "2");
		ApplyChangesToInvoiceLine(declaration, 4, 2, x => x.JI_PrimaryPreference = "3");
		declaration.DoMerge();

		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 3, entry.MergedLines.Count);

		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.PreferenceCode).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.Preference should be", "1", orderedMergedLines[0].PreferenceCode);
			AssertEquals("CusEntryLine.Preference should be", "2", orderedMergedLines[1].PreferenceCode);
			AssertEquals("CusEntryLine.Preference should be", "3", orderedMergedLines[2].PreferenceCode);
		});
	}

	public void TestDoMergeWithDifferentSteelType()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);

		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);

		ApplyChangesToInvoiceLine(declaration, 0, 3, x => x.ZG_SteelType = "1");
		ApplyChangesToInvoiceLine(declaration, 3, 3, x => x.JI_PrimaryPreference = "0");
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 2, entry.MergedLines.Count);

		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.SteelType).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.SteelType should be", "0", orderedMergedLines[0].SteelType);
			AssertEquals("CusEntryLine.SteelType should be", "1", orderedMergedLines[1].SteelType);
		});
	}

	public void TestDoMergeWithDifferentValuationCode()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);
		ApplyChangesToInvoiceLine(declaration, 0, 3, x => x.JI_ValuationCode = "6");
		ApplyChangesToInvoiceLine(declaration, 3, 3, x => x.JI_ValuationCode = "7");
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 2, entry.MergedLines.Count);

		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.ValuationMethod).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.ValuationCode should be", "6", orderedMergedLines[0].ValuationMethod);
			AssertEquals("CusEntryLine.ValuationCode should be", "7", orderedMergedLines[1].ValuationMethod);
		});
	}

	public void TestDoMergeWithDifferentQuota()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);
		ApplyChangesToInvoiceLine(declaration, 0, 3, x => x.JI_ConcessionOrder = "80");
		ApplyChangesToInvoiceLine(declaration, 3, 3, x => x.JI_ConcessionOrder = "90");
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 2, entry.MergedLines.Count);

		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.ValuationMethod).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.Quota should be", "80", orderedMergedLines[0].QuotaOrderNumber);
			AssertEquals("CusEntryLine.Quota should be", "90", orderedMergedLines[1].QuotaOrderNumber);
		});
	}

	public void TestDoMergeWithDifferentBuyer()
	{
		Assert("This test will be implemented in future", condition: true);
	}

	public void TestDoMergeWithDifferentSupplier()
	{
		Assert("This test will be implemented in future", condition: true);
	}

	public void TestDoMergeWithDifferentSupplementaryCodes()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);
		ApplyChangesToInvoiceLine(declaration, 0, 2, x =>
		{
			x.JI_SupplementaryCode1 = "40";
			x.JI_SupplementaryCode2 = "50";
		});
		ApplyChangesToInvoiceLine(declaration, 2, 2, x =>
		{
			x.JI_SupplementaryCode1 = "50";
			x.JI_SupplementaryCode2 = "40";
		});
		ApplyChangesToInvoiceLine(declaration, 4, 2, x =>
		{
			x.JI_SupplementaryCode1 = "40";
			x.JI_SupplementaryCode2 = "50";
			var supplementaryCode = x.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode.CY_Code = "60";
		});
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 2, entry.MergedLines.Count);

		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.SupplementaryCode1).ThenBy(x => x.SupplementaryCode2).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.Quota should be", "40", orderedMergedLines[0].SupplementaryCode1);
			AssertEquals("CusEntryLine.Quota should be", "50", orderedMergedLines[0].SupplementaryCode2);
			AssertEquals("CusEntryLine.Quota should be", "40", orderedMergedLines[1].SupplementaryCode1);
			AssertEquals("CusEntryLine.Quota should be", "50", orderedMergedLines[1].SupplementaryCode2);
			AssertEquals("CusEntryLine.Quota should be", "60", orderedMergedLines[1].RandomLine.AdditionalSupplementaryCodes[0].CY_Code);
		});
	}

	public void TestDoMergeWithDifferentPackingType()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);
		declaration.JE_MergeBy = "TRF";
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);
		var entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 1, entry.MergedLines.Count);

		ApplyChangesToInvoiceLine(declaration, 0, 2, x =>
		{
			SetPackageType(x, "AO");
		});
		ApplyChangesToInvoiceLine(declaration, 2, 2, x =>
		{
			SetPackageType(x, "SC");
		});
		ApplyChangesToInvoiceLine(declaration, 4, 2, x =>
		{
			SetPackageType(x, "MK");
		});
		declaration.DoMerge();
		AssertEquals("Number of CusEntryHeader should be", 1, declaration.CustomsEntryHeaders.Count);

		entry = declaration.CustomsEntryHeaders[0];
		AssertEquals("Number of CusEntryLine should be", 3, entry.MergedLines.Count);

		var orderedMergedLines = entry.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.PackageType).ToArray();
		CombineAssertions(() =>
		{
			AssertEquals("CusEntryLine.PackageType should be", "AO", orderedMergedLines[0].PackageType);
			AssertEquals("CusEntryLine.PackageType should be", "MK", orderedMergedLines[1].PackageType);
			AssertEquals("CusEntryLine.PackageType should be", "SC", orderedMergedLines[2].PackageType);
		});
	}

	public void TestDoMergeSetEntryHeaderMessageType()
	{
		var declaration = GetDeclarationReadyForDoMergeTest(testMergeByOption: false);
		declaration.JE_MergeBy = "TRF";
		declaration.JE_MessageType = "IMP";
		declaration.DoMerge();

		AssertEquals(1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("When JE_ApplicationCode = BLT, merge is run and CH_MessageType is set", DeclarationApplicationCodeList.Codes.Builtin, entryHeader.CH_MessageType);

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		entryHeader.CH_MessageType = ZString.Empty;
		declaration.DoMerge();
		AssertEquals("When JE_ApplicationCode = ITF, merge is not run and CH_MessageType is not set", ZString.Empty, entryHeader.CH_MessageType);

		declaration.JE_ApplicationCode = ZString.Empty;
		entryHeader.CH_MessageType = ZString.Empty;
		declaration.DoMerge();
		AssertEquals("When JE_ApplicationCode = Empty, merge is not run and CH_MessageType is not set", ZString.Empty, entryHeader.CH_MessageType);
	}

	public void TestDeclarationResetApportionedPreviousDocumentsFiredOnMerging()
	{
		var mockDeclaration = Factory.New<DummyJobDeclaration_ResetApportionedPreviousDocuments>();
		var lineMerger = new LineMergerForTest(mockDeclaration);
		lineMerger.OnMergingExposed();
		Assert("Reset Apportioned Previous Documents must be called at least once.", mockDeclaration.ResetApportionedPreviousDocumentsTriggeredCount > 0);
	}

	public void TestDefaultMethodOfPaymentWhenDefermentAccountNumberIsEmpty()
	{
		var dtyTariffCode = "222";

		DutyReferenceDataConfigurationBuilder.New(Factory)
			.AddTariffType("IMP")
			.AddTariff(dtyTariffCode)
			.AddRateCode(RateTypeEnum.Duty, "A00", "0.12 * [KGM] + 0.2 * VFD", preference: "")
			.AddRateCode(RateTypeEnum.Duty, "A20", "0.3 * [KGM]", preference: "")
			.AddRateCode(RateTypeEnum.Duty, "270", "0.12 * [KGM] + 0.2 * VFD", preference: "")
			.AddRateCode(RateTypeEnum.Duty, "275", "0.12 * [KGM] + 0.2 * VFD", preference: "")
			.AddRateCode(RateTypeEnum.Antidumping, "A30", "0.5 * [DTN]", preference: "")
			.AddRateCode(RateTypeEnum.Antidumping, "A35", "0.5 * [VFD]", preference: "")
			.AddRateCode(RateTypeEnum.CounterVailing, "A40", "0.3 * VFD + 0.06 * [KGM]", preference: "")
			.AddRateCode(RateTypeEnum.CounterVailing, "A45", "0.5 * [VFD]", preference: "")
			.Configure();

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		var invLine1 = invoice1.InvoiceLines.AddNew();
		invLine1.JI_CEI = entryInstruction.PK;
		invLine1.JI_Procedure = "A";
		invLine1.JI_Tariff = dtyTariffCode;
		invLine1.JI_CustomsQuantity = 5;
		invLine1.JI_CustomsUnitQty = "DTN";
		invLine1.JI_CustomsSecondQuantity = 300;
		invLine1.JI_CustomsSecondUnitQty = "KGM";
		invLine1.JI_CustomsThirdQuantity = 0.1;
		invLine1.JI_CustomsThirdUnitQty = "KLT";
		invLine1.JI_SupplementaryCode1 = "7000";
		invLine1.JI_LinePrice = 50;

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		var invLine2 = invoice2.InvoiceLines.AddNew();
		invLine2.JI_CEI = entryInstruction.PK;
		invLine2.JI_Procedure = "A";
		invLine2.JI_Tariff = dtyTariffCode;
		invLine2.JI_CustomsQuantity = 2;
		invLine2.JI_CustomsUnitQty = "DTN";
		invLine2.JI_CustomsSecondQuantity = 200;
		invLine2.JI_CustomsSecondUnitQty = "KGM";
		invLine2.JI_CustomsThirdQuantity = 0.2;
		invLine2.JI_CustomsThirdUnitQty = "KLT";
		invLine2.JI_SupplementaryCode1 = "7000";
		invLine2.JI_LinePrice = 20;

		declaration.JE_DefermentAccountNumber = "";
		declaration.DoMerge();

		AssertEquals("[PRE-CONDITION] Entry Headers", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("[PRE-CONDITION] Entry Lines", 1, entryHeader.MergedLines.Count);
		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

		var fees = entryLine.Fees.OfType<CusEntryLineFee>();
		var a35A45Fees = fees.Where(x => x.CF_ChargeType == "A35" || x.CF_ChargeType == "A45");
		AssertGreaterThan("[PRE-CONDITION] A35 and A45 Line Fees Count in Entry Line A", a35A45Fees.Count(), 0);
		var otherFees = fees.Except(a35A45Fees);

		CombineAssertions("Checking Merge default entered MoP", () =>
		{
			AssertEquals("All A35 and A45 fees should have MoP: R", true, a35A45Fees.All(x => x.CF_MethodOfPayment == "R"));
			AssertEquals("Other Fees should have MoP: ", true, otherFees.All(x => x.CF_MethodOfPayment == "A"));
		});

		entryLine.Fees.Cast<CusEntryLineFee>().ForEach(x => x.CF_MethodOfPayment = "");
		declaration.JE_DefermentAccountNumber = "123456A";
		declaration.DoMerge();

		fees = entryLine.Fees.OfType<CusEntryLineFee>();
		a35A45Fees = fees.Where(x => x.CF_ChargeType == "A35" || x.CF_ChargeType == "A45");
		var feesThatStartWithA = fees.Where(x => x.CF_ChargeType.StartsWith("A")).Except(a35A45Fees);
		var feesThatStartWith27 = fees.Where(x => x.CF_ChargeType.StartsWith("27"));
		otherFees = fees.Except(a35A45Fees).Except(feesThatStartWithA).Except(feesThatStartWith27);

		CombineAssertions("Checking Merge default entered MoP", () =>
		{
			AssertEquals("All A35 and A45 fees should have MoP: R", true, a35A45Fees.All(x => x.CF_MethodOfPayment == "R"));
			AssertEquals("Fees that start with A should have MoP: E", true, feesThatStartWithA.All(x => x.CF_MethodOfPayment == "E"));
			AssertEquals("Fees that start with 27 should have MoP: E", true, feesThatStartWith27.All(x => x.CF_MethodOfPayment == "E"));
			AssertEquals("Other Fees should have MoP: ", true, otherFees.All(x => x.CF_MethodOfPayment == "G"));
		});
	}

	protected override void CustomizeSupportingDocumentForLocalCountry(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument)
	{
		var supDocument = (SupportingDocument)supportingDocument;

		supDocument.CSI_YearOfIssue = ZDate.Today.Year.ToString();
	}

	protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

	protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

	protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

	protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override void SetUp()
	{
		base.SetUp();
		Factory.ClearCachedValue<ZString>("ParentDataGrouping_IT");
	}

	class LineMergerForTest : LineMerger
	{
		public LineMergerForTest(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		public void OnMergingExposed() => OnMerging();
	}

	#region Implementation

	JobDeclaration GetDeclarationReadyForDoMergeTest(bool testMergeByOption = true)
	{
		var declaration = base.Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var invoice = declaration.Invoices.AddNew();
		void addInvoiceLines(ZString tariff, ZString description, ZString partNo, ZGuid classificationPk)
		{
			for (int i = 0; i < 2; i = checked(i + 1))
			{
				var jobComInvoiceLine = invoice.InvoiceLines.AddNew();
				jobComInvoiceLine.JI_CEI = entryInstruction.PK;
				jobComInvoiceLine.JI_CC = classificationPk;
				jobComInvoiceLine.JI_Tariff = tariff;
				jobComInvoiceLine.JI_Description = description;
				jobComInvoiceLine.JI_PartNo = partNo;

				var package = declaration.Packages.AddNew();
				package.CW_CR_HouseContainer = billPackingGroup.PK;
				package.CW_PackType = "CT";
				var packageInvoiceLine = jobComInvoiceLine.PackagesPivot.AddNew();
				packageInvoiceLine.CHC_CW = package.PK;
			}
		}
		var classification5 = base.Factory.New<BaseCusClassification>();
		classification5.CC_LookupCode = "Lookup1";
		classification5.CC_TariffNum = "80";
		var classification4 = base.Factory.New<BaseCusClassification>();
		classification4.CC_LookupCode = "Lookup2";
		classification4.CC_TariffNum = "81";
		var classification3 = base.Factory.New<BaseCusClassification>();
		classification3.CC_LookupCode = "Lookup3";
		classification3.CC_TariffNum = "81";
		if (testMergeByOption)
		{
			addInvoiceLines("80", "Description1", "PartNo1", classification5.PK);
			addInvoiceLines("80", "Description1", "PartNo1", classification5.PK);
			addInvoiceLines("81", "Description2", "PartNo2", classification4.PK);
			addInvoiceLines("81", "Description3", "PartNo2", classification3.PK);
		}
		else
		{
			addInvoiceLines("80", "Description1", "PartNo1", classification5.PK);
			addInvoiceLines("80", "Description1", "PartNo1", classification5.PK);
			addInvoiceLines("80", "Description1", "PartNo1", classification5.PK);
		}
		return declaration;
	}

	void ApplyChangesToInvoiceLine(JobDeclaration declaration, int skip, int take, Action<JobComInvoiceLine> action)
	{
		declaration.InvoiceLines.Cast<JobComInvoiceLine>().Skip(skip).Take(take)
			.ToList()
			.ForEach(action);
	}

	void SetPackageType(JobComInvoiceLine invoiceLine, ZString packageType)
	{
		var declaration = invoiceLine.Declaration;
		var packagePivot = invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
		if (packagePivot != null)
		{
			var package = declaration.Packages.Cast<BasePackage>().SingleOrDefault(x => x.PK == packagePivot.CHC_CW);
			package.CW_PackType = packageType;
		}
	}

	#endregion
}

sealed class DummyJobDeclaration_ResetApportionedPreviousDocuments : JobDeclaration
{
	public DummyJobDeclaration_ResetApportionedPreviousDocuments(CargoWise.EntityFramework.BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public int ResetApportionedPreviousDocumentsTriggeredCount { get; set; }

	public override void ResetApportionedPreviousDocuments()
	{
		ResetApportionedPreviousDocumentsTriggeredCount++;
		base.ResetApportionedPreviousDocuments();
	}
}
