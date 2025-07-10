using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers.Testing
{
	sealed class GbLineTest : TestCaseWithFactory
	{
		public void TestStatements()
		{
			const string cds = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "test");
			var code1 = helper.CreateCusCodeList(cds, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);
			var code2 = helper.CreateCusCodeList(cds, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code2.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);
			var code3 = helper.CreateCusCodeList(cds, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var code4 = helper.CreateCusCodeList(cds, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code4.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);
			var code5 = helper.CreateCusCodeList(cds, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD5", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code5.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item);
			var code6 = helper.CreateCusCodeList(cds, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD6", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = cds;
			declaration.Invoices.DeleteAll();

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();

			var additionalInfo1 = invoice.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "ADD1";
			var additionalInfo2 = invoice.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "ADD2";
			var additionalInfo3 = invoice.AdditionalInfos.AddNew();
			additionalInfo3.CSI_Code = "ADD3";

			invoice.InvoiceLines.RemoveAndDeleteAll();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			var additionalInfo4 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo4.CSI_Code = "ADD4";
			var additionalInfo5 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo5.CSI_Code = "ADD5";
			var additionalInfo6 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo6.CSI_Code = "ADD6";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			var entryLine = entryHeader.AllEntryLines.FirstOrDefault();
			var gbHeader = new GbCDSImportHeader(entryHeader);
			var gbLine = new GbCDSImportLine(gbHeader, entryLine);
			var line = gbLine as ILine;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ADD2", "ADD3", "ADD5", "ADD6" }, line.Statements.Select(x => x.Statement));
		}

		public void TestTaxes()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.Invoices.DeleteAll();

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();

			var additionalInfo1 = invoice.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "ADD1";
			var additionalInfo2 = invoice.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "ADD2";
			var additionalInfo3 = invoice.AdditionalInfos.AddNew();
			additionalInfo3.CSI_Code = "ADD3";

			invoice.InvoiceLines.RemoveAndDeleteAll();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			var additionalInfo4 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo4.CSI_Code = "ADD4";
			var additionalInfo5 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo5.CSI_Code = "ADD5";
			var additionalInfo6 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo6.CSI_Code = "ADD6";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
			var entryLine = entryHeader.AllEntryLines.FirstOrDefault();
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_MethodOfCalculation = "DTN";
			fee1.CF_ChargeType = "A00";
			fee1.CF_BaseValue = 10;

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_MethodOfCalculation = "KGM";
			fee2.CF_ChargeType = "A00";
			fee2.CF_BaseValue = 20;

			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_MethodOfCalculation = "%";
			fee3.CF_ChargeType = "A00";
			fee3.CF_BaseValue = 30;

			var fee4 = entryLine.Fees.AddNew();
			fee3.CF_MethodOfCalculation = "TNE";
			fee3.CF_ChargeType = "A00";
			fee3.CF_BaseValue = 40;

			Assert("Pre-requisite: DTN expected to be convertible mass", ConvertibleUnitsOfMeasure.WeightConversionDictionary.ContainsKey("DTN"));
			Assert("Pre-requisite: TNE expected to be convertible mass", ConvertibleUnitsOfMeasure.WeightConversionDictionary.ContainsKey("TNE"));

			var gbHeader = new GbCDSImportHeader(entryHeader);
			var gbLine = new GbCDSImportLine(gbHeader, entryLine);
			var line = gbLine as ILine;

			using (GBCustomsDataRegistry.Instance.SendDTNTaxBaseToCDS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Convertible mass tax bases should be excluded when registry item is false", 1, line.Taxes.Count());
					AssertEquals("DTN should be excluded when registry item is false", false, line.Taxes.Any(x => x.MethodOfCalculation == "DTN"));
					AssertEquals("TNE should be excluded when registry item is false", false, line.Taxes.Any(x => x.MethodOfCalculation == "TNE"));
				});
			}

			using (GBCustomsDataRegistry.Instance.SendDTNTaxBaseToCDS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Convertible mass tax bases should be included when registry item is true", 4, line.Taxes.Count());
					AssertEquals("DTN should be included when registry item is true", true, line.Taxes.Any(x => x.MethodOfCalculation == "DTN"));
					AssertEquals("TNE should be included when registry item is true", true, line.Taxes.Any(x => x.MethodOfCalculation == "TNE"));
				});
			}
		}
	}
}
