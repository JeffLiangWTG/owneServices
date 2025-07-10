using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class AIEMRateCalcDataTest : TestCaseWithFactory
	{
		public void TestValueForDuty()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsQuantity = 3;
			invLine.JI_CustomsUnitQty = "KGM";
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.InvoiceLines.Add(invLine);

			var rateCalcData = new AIEMRateCalcData(entryLine, Factory.New<RateView>());
			AssertEquals("ValueForDuty", 0m, rateCalcData.ValueForDuty);

			var fee = entryLine.Fees.AddNew();
			fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			fee.G4_BaseAmount = 2.34;
			AssertEquals("ValueForDuty", 2.34m, rateCalcData.ValueForDuty);

			rateCalcData.CustomsValueFormula = "1.23456 * [KGM]";
			AssertEquals("ValueForDuty", 3.704m, rateCalcData.ValueForDuty);
		}

		public void TestCustomsValue()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var rateCalcData = new AIEMRateCalcData(entryLine, Factory.New<RateView>());
			AssertEquals("CustomsValue", 0m, rateCalcData.CustomsValue);

			var fee = entryLine.Fees.AddNew();
			fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			fee.G4_BaseAmount = 11;
			AssertEquals("CustomsValue", 11m, rateCalcData.CustomsValue);
		}

		public void TestAdditionalInformationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "ES DESC", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;

			foreach (var code in new[] { "DOC1", "DOC2", "DOC3", "DOC4", "DOC5", "DOC6", "DOC7", "DOC8", "GVM1", "GVM2", "GVM3" })
			{
				var attributeNameValuePairs = new Dictionary<string, string[]>();
				attributeNameValuePairs.Add(RefCusCodeListAttributeTypes.Codes.Level, new[] { EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new[] { importCodeType, exportCodeType }, code, code + " DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			var customsEntryInstructions = declaration.CustomsEntryInstructions;
			var entryInstruction1 = customsEntryInstructions.AddNew();
			AddSupportingDocument(entryInstruction1.SupportingDocuments, "DOC1");
			AddSupportingDocument(entryInstruction1.SupportingDocuments, "DOC2");
			AddSupportingDocument(entryInstruction1.SupportingDocuments, "GVM1", CusSupportingInfoTypeList.Codes.GoodsVehicleMovementSystem);

			var invoiceHeader1 = declaration.Invoices.AddNew();
			AddSupportingDocument(invoiceHeader1.SupportingDocuments, "DOC1");
			AddSupportingDocument(invoiceHeader1.SupportingDocuments, "DOC3");
			AddSupportingDocument(invoiceHeader1.SupportingDocuments, "GVM3", CusSupportingInfoTypeList.Codes.GoodsVehicleMovementSystem);

			var invoiceHeader1InvoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceHeader1InvoiceLine1.JI_CEI = entryInstruction1.PK;
			AddSupportingDocument(invoiceHeader1InvoiceLine1.SupportingDocuments, "DOC1");
			AddSupportingDocument(invoiceHeader1InvoiceLine1.SupportingDocuments, "DOC4");
			AddSupportingDocument(invoiceHeader1InvoiceLine1.SupportingDocuments, "GVM4", CusSupportingInfoTypeList.Codes.GoodsVehicleMovementSystem);

			var invoiceHeader1InvoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceHeader1InvoiceLine2.JI_CEI = entryInstruction1.PK;
			AddSupportingDocument(invoiceHeader1InvoiceLine2.SupportingDocuments, "DOC1");
			AddSupportingDocument(invoiceHeader1InvoiceLine2.SupportingDocuments, "DOC5");

			AddSupportingDocument(declaration.SupportingDocuments, "DOC1");
			AddSupportingDocument(declaration.SupportingDocuments, "DOC6");

			var invoiceHeader2 = declaration.Invoices.AddNew();
			AddSupportingDocument(invoiceHeader2.SupportingDocuments, "DOC1");
			AddSupportingDocument(invoiceHeader2.SupportingDocuments, "DOC7");

			var invoiceHeader2InvoiceLine1 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceHeader1InvoiceLine2.JI_CEI = entryInstruction1.PK;
			AddSupportingDocument(invoiceHeader2InvoiceLine1.SupportingDocuments, "DOC8");
			AddSupportingDocument(invoiceHeader2InvoiceLine1.SupportingDocuments, "DOC1");

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions("invoiceHeader1InvoiceLine1", () =>
			{
				var rateCalcData = new AIEMRateCalcData(invoiceHeader1InvoiceLine1.CusEntryLine, Factory.New<RateView>());
				AssertEquals("AdditionalInformationList.Count", 5, rateCalcData.AdditionalInformationList.Count);
				AssertEquals("AdditionalInformationList[0]", new Tuple<string, string>("CERT", "DOC1"), rateCalcData.AdditionalInformationList[0]);
				AssertEquals("AdditionalInformationList[1]", new Tuple<string, string>("CERT", "DOC2"), rateCalcData.AdditionalInformationList[1]);
				AssertEquals("AdditionalInformationList[2]", new Tuple<string, string>("CERT", "DOC3"), rateCalcData.AdditionalInformationList[2]);
				AssertEquals("AdditionalInformationList[3]", new Tuple<string, string>("CERT", "DOC4"), rateCalcData.AdditionalInformationList[3]);
				AssertEquals("AdditionalInformationList[4]", new Tuple<string, string>("CERT", "DOC6"), rateCalcData.AdditionalInformationList[4]);
			});

			CombineAssertions("invoiceHeader1InvoiceLine2", () =>
			{
				var rateCalcData = new AIEMRateCalcData(invoiceHeader1InvoiceLine2.CusEntryLine, Factory.New<RateView>());
				AssertEquals("AdditionalInformationList.Count", 5, rateCalcData.AdditionalInformationList.Count);
				AssertEquals("AdditionalInformationList[0]", new Tuple<string, string>("CERT", "DOC1"), rateCalcData.AdditionalInformationList[0]);
				AssertEquals("AdditionalInformationList[1]", new Tuple<string, string>("CERT", "DOC2"), rateCalcData.AdditionalInformationList[1]);
				AssertEquals("AdditionalInformationList[2]", new Tuple<string, string>("CERT", "DOC3"), rateCalcData.AdditionalInformationList[2]);
				AssertEquals("AdditionalInformationList[3]", new Tuple<string, string>("CERT", "DOC5"), rateCalcData.AdditionalInformationList[3]);
				AssertEquals("AdditionalInformationList[4]", new Tuple<string, string>("CERT", "DOC6"), rateCalcData.AdditionalInformationList[4]);
			});

			CombineAssertions("invoiceHeader2InvoiceLine1", () =>
			{
				var rateCalcData = new AIEMRateCalcData(invoiceHeader2InvoiceLine1.CusEntryLine, Factory.New<RateView>());
				AssertEquals("AdditionalInformationList.Count", 5, rateCalcData.AdditionalInformationList.Count);
				AssertEquals("AdditionalInformationList[0]", new Tuple<string, string>("CERT", "DOC1"), rateCalcData.AdditionalInformationList[0]);
				AssertEquals("AdditionalInformationList[1]", new Tuple<string, string>("CERT", "DOC2"), rateCalcData.AdditionalInformationList[1]);
				AssertEquals("AdditionalInformationList[2]", new Tuple<string, string>("CERT", "DOC6"), rateCalcData.AdditionalInformationList[2]);
				AssertEquals("AdditionalInformationList[3]", new Tuple<string, string>("CERT", "DOC7"), rateCalcData.AdditionalInformationList[3]);
				AssertEquals("AdditionalInformationList[4]", new Tuple<string, string>("CERT", "DOC8"), rateCalcData.AdditionalInformationList[4]);
			});
		}

		void AddSupportingDocument(SupportingDocumentCollection supportingDocuments, string code, string type = CusSupportingInfoTypeList.Codes.SupportingDocument)
		{
			var doc = supportingDocuments.AddNew();
			doc.CSI_Code = code;
			doc.CSI_Type = type;
		}
	}
}
