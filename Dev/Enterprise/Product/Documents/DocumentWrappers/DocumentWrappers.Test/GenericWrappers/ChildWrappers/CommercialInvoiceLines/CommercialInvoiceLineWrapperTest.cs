using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Moq.Protected;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.Business.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.Business.CusEntryLineFee;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CommercialInvoiceLineWrapper))]
	sealed class CommercialInvoiceLineWrapperTest : Base.Testing.GenericWrapperTest
	{
		public void TestExporterManufacturerConsigneeAddress()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var exporterAddress = Factory.New<OrgAddress>();
			invoiceLine.JI_OA_ExporterAddress = exporterAddress.PK;
			var manufacturerAddress = Factory.New<OrgAddress>();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			var consigneeAddress = Factory.New<OrgAddress>();
			invoiceLine.JI_OA_ConsigneeAddress = consigneeAddress.PK;

			var wrapper = new CommercialInvoiceLineWrapper(invoiceLine, Factory);
			AssertEquals(exporterAddress, wrapper.ExporterAddress);
			AssertEquals(manufacturerAddress, wrapper.ManufacturerAddress);
			AssertEquals(consigneeAddress, wrapper.ConsigneeAddress);
		}

		public override void TestWrapperMappingsEmpty()
		{
			CommercialInvoiceLineWrapper wrapperEmpty = new CommercialInvoiceLineWrapper(null, Factory);
			AssertEquals("0", wrapperEmpty.ToString());
			AssertEquals(null, wrapperEmpty.Invoice);
			AssertEquals(ZString.Empty, wrapperEmpty.CountryOfOrigin.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.LinePrice.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.UnitPrice.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.CustomsQuantity.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.LineQuantity.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.NetWeight.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.Volume.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.Weight.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.ConcessionCode);
			AssertEquals(ZString.Empty, wrapperEmpty.Description);
			AssertEquals(ZInt.Zero, wrapperEmpty.LineNo);
			AssertEquals(ZString.Empty, wrapperEmpty.LookupCode);
			AssertEquals(ZString.Empty, wrapperEmpty.PartNumber);
			AssertEquals(ZString.Empty, wrapperEmpty.TariffCode);
			AssertEquals(ZString.Empty, wrapperEmpty.OrderNumber);
			AssertEquals(ZDecimal.Zero, wrapperEmpty.DutyAmount);
			AssertEquals(ZString.Empty, wrapperEmpty.DutyRateDescription);
			AssertEquals(ZDecimal.Zero, wrapperEmpty.GSTRate);
			AssertEquals(ZString.Empty, wrapperEmpty.RefCountryCode);
			AssertEquals(ZString.Empty, wrapperEmpty.CustomAttrib1);
			AssertEquals(ZString.Empty, wrapperEmpty.CustomAttrib2);
			AssertEquals(ZString.Empty, wrapperEmpty.CustomAttrib3);
			AssertEquals(ZString.Empty, wrapperEmpty.CustomAttrib4);
			AssertEquals(ZString.Empty, wrapperEmpty.CustomAttrib5);
			AssertEquals(ZString.Empty, wrapperEmpty.CustomAttrib6);
			AssertEquals("Not Merged", wrapperEmpty.MergedLineNumber);
		}

		public void TestFormattedDescription()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			line.JI_Description = "BOSTON STEAMERS";
			line.JI_NDescription = "NBOSTON STEAMERS";
			line.JI_CountryOfOrigin = "US";
			var wrapper = new CommercialInvoiceLineWrapper(line, Factory);
			AssertEquals(wrapper.FormattedDescription, "BOSTON STEAMERS\r\nNBOSTON STEAMERS;C/O:US");

			line.JI_Tariff = "5201000043";
			wrapper = new CommercialInvoiceLineWrapper(line, Factory);
			AssertEquals(wrapper.FormattedDescription, "BOSTON STEAMERS\r\nNBOSTON STEAMERS;C/O:US;HS:5201000043");

			declaration.JE_MessageType = "EXP";
			wrapper = new CommercialInvoiceLineWrapper(line, Factory);
			AssertEquals(wrapper.FormattedDescription, "BOSTON STEAMERS\r\nNBOSTON STEAMERS");
		}

		public void TestDescription()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			line.JI_Description = "LINE1";
			line.JI_NDescription = "LINE2";
			var wrapper = new CommercialInvoiceLineWrapper(line, Factory);
			AssertEquals(wrapper.Description, "LINE1\r\nLINE2");
			line.JI_NDescription = ZString.Empty;
			wrapper = new CommercialInvoiceLineWrapper(line, Factory);
			AssertEquals(wrapper.FormattedDescription, "LINE1");
			line.JI_Description = ZString.Empty;
			line.JI_NDescription = "LINE2";
			wrapper = new CommercialInvoiceLineWrapper(line, Factory);
			AssertEquals(wrapper.FormattedDescription, "LINE2");
			line.JI_NDescription = ZString.Empty;
			wrapper = new CommercialInvoiceLineWrapper(line, Factory);
			AssertEquals(wrapper.FormattedDescription, ZString.Empty);
		}

		public void TestWrapperMappingsFull()
		{
			BaseCusClassification classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "ROTTEN";
			classification.CC_TariffNum = "TARIFF";
			classification.CC_Description = "BOSTON STEAMERS";

			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceNumber = "FORGOTTEN";
			invoiceHeader.JZ_InvoiceAmount = 5434.44m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "ZAR";

			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5434.44m;
			invoiceLine.JI_InvoiceQuantity = 2;
			invoiceLine.JI_InvoiceUQ = "PK";
			invoiceLine.JI_NetWeight = 433.8m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_Volume = 34.332m;
			invoiceLine.JI_VolumeUQ = "CF";
			invoiceLine.JI_Weight = 344.8m;
			invoiceLine.JI_WeightUQ = "LB";
			invoiceLine.JI_PartNo = "STINKER";
			invoiceLine.JI_OrderNumber = "ROTTER";
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_Description = "BOSTON STEAMERS";
			invoiceLine.JI_NDescription = "N BOSTON STEAMERS";
			invoiceLine.JI_ConcessionOrder = "123456G";
			invoiceLine.JI_CountryOfOrigin = "ZA";
			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomAttrib1 = "Attrib1";
			invoiceLine.JI_CustomAttrib2 = "Attrib2";
			invoiceLine.JI_CustomAttrib3 = "Attrib3";
			invoiceLine.JI_CustomAttrib4 = "Attrib4";
			invoiceLine.JI_CustomAttrib5 = "Attrib5";
			invoiceLine.JI_CustomAttrib6 = "Attrib6";

			var entryLine = Factory.New<CusEntryLine>();
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "999";
			entryLine.CL_CH = entryHeader.PK;
			entryLine.CL_DutyPercent = 0.5m;
			entryLine.CL_LineNumber = 1234;
			invoiceLine.JI_CL = entryLine.PK;

			CusEntryLineFee lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			lineFee.CF_ChargeAmount = 20m;

			CommercialInvoiceLineWrapper wrapperFull = new CommercialInvoiceLineWrapper(invoiceLine, Factory);
			AssertEquals("1", wrapperFull.ToString());
			AssertEquals("FORGOTTEN", wrapperFull.Invoice.InvoiceNumber);
			AssertEquals("ZA - South Africa", wrapperFull.CountryOfOrigin.ToString());
			AssertEquals("5,434.44 ZAR", wrapperFull.LinePrice.ToString());
			AssertEquals("2,717.22 ZAR", wrapperFull.UnitPrice.ToString());
			AssertEquals("150 KGM", wrapperFull.CustomsQuantity.ToString());
			AssertEquals("2 PK", wrapperFull.LineQuantity.ToString());
			AssertEquals("433.8 KG", wrapperFull.NetWeight.ToString());
			AssertEquals("34.332 CF", wrapperFull.Volume.ToString());
			AssertEquals("344.8 LB", wrapperFull.Weight.ToString());
			AssertEquals("123456G", wrapperFull.ConcessionCode);
			AssertEquals("BOSTON STEAMERS\r\nN BOSTON STEAMERS", wrapperFull.Description);
			AssertEquals(1, wrapperFull.LineNo);
			AssertEquals("ROTTEN", wrapperFull.LookupCode);
			AssertEquals("STINKER", wrapperFull.PartNumber);
			AssertEquals("TARIFF", wrapperFull.TariffCode);
			AssertEquals("ROTTER", wrapperFull.OrderNumber);
			AssertEquals("Attrib1", wrapperFull.CustomAttrib1);
			AssertEquals("Attrib2", wrapperFull.CustomAttrib2);
			AssertEquals("Attrib3", wrapperFull.CustomAttrib3);
			AssertEquals("Attrib4", wrapperFull.CustomAttrib4);
			AssertEquals("Attrib5", wrapperFull.CustomAttrib5);
			AssertEquals("Attrib6", wrapperFull.CustomAttrib6);
			AssertEquals("ZA", wrapperFull.RefCountryCode.ToString());
			AssertEquals("DutyAmount", 0.00m, wrapperFull.DutyAmount);
			AssertEquals("MergedLineNumber", "1234/999", wrapperFull.MergedLineNumber);
			AssertEquals("GSTRate", 0.00m, wrapperFull.GSTRate);
			AssertEquals("Duty rate description", "0.50%", wrapperFull.DutyRateDescription);
			AssertEquals("ProvProgTariff", "", wrapperFull.ProvProgTariff);
			AssertEquals("ProvProgDutyRate", "", wrapperFull.ProvProgDutyRate);
			AssertEquals("ProvProgCustomsQty", "", wrapperFull.ProvProgCustomsQty);
			AssertEquals("ProvProgTariff1 ", "", wrapperFull.ProvProgTariff1);
			AssertEquals("ProvProgDutyRate1 ", "", wrapperFull.ProvProgDutyRate1);
			AssertEquals("ProvProgCustomsQty1 ", "", wrapperFull.ProvProgCustomsQty1);
			AssertEquals("ProvProgTariff2 ", "", wrapperFull.ProvProgTariff2);
			AssertEquals("ProvProgDutyRate2 ", "", wrapperFull.ProvProgDutyRate2);
			AssertEquals("ProvProgCustomsQty2 ", "", wrapperFull.ProvProgCustomsQty2);
			AssertEquals("ProvProgTariff3 ", "", wrapperFull.ProvProgTariff3);
			AssertEquals("ProvProgDutyRate3 ", "", wrapperFull.ProvProgDutyRate3);
			AssertEquals("ProvProgCustomsQty3 ", "", wrapperFull.ProvProgCustomsQty3);
			AssertEquals("ProvProgTariff4 ", "", wrapperFull.ProvProgTariff4);
			AssertEquals("ProvProgDutyRate4 ", "", wrapperFull.ProvProgDutyRate4);
			AssertEquals("ProvProgCustomsQty4 ", "", wrapperFull.ProvProgCustomsQty4);
			AssertEquals("ProvProgTariff5 ", "", wrapperFull.ProvProgTariff5);
			AssertEquals("ProvProgDutyRate5 ", "", wrapperFull.ProvProgDutyRate5);
			AssertEquals("ProvProgCustomsQty5 ", "", wrapperFull.ProvProgCustomsQty5);

			//CS00142357
			invoiceLine.JI_InvoiceQuantity = 2.2584;
			AssertEquals("2.2584 PK", wrapperFull.LineQuantity.ToString());
		}

		public void TestEuDutyAmountsAsString()
		{
			var cusEntryLine = Factory.New<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>();
			var invoiceLine = Factory.New<Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine>();
			invoiceLine.JI_CL = cusEntryLine.PK;
			var a30Fee = cusEntryLine.Fees.AddNew();
			a30Fee.CF_ChargeType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			a30Fee.CF_ChargeAmount = 202;
			AssertEquals("PreReq", "A30:202.00", invoiceLine.DutyAmountsAsString);

			var wrapper = new CommercialInvoiceLineWrapper(invoiceLine, Factory);
			AssertEquals("A30:202.00", wrapper.DutyAmountsAsString);
		}

		public void TestQuantitiesForMultipleLines()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 2.254m;
			invoiceLine.JI_InvoiceUQ = "PK";
			invoiceLine.JI_NetWeight = 433.8m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_Weight = 344.8m;
			invoiceLine.JI_WeightUQ = "LB";
			invoiceLine.JI_CustomsUnitQty = "T";
			invoiceLine.JI_CustomsQuantity = 345.98702m;
			invoiceLine.JI_Volume = 34.33m;
			invoiceLine.JI_VolumeUQ = "CF";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 2.254999999m;
			invoiceLine2.JI_InvoiceUQ = "PK";
			invoiceLine2.JI_NetWeight = 433m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_Weight = 344.88m;
			invoiceLine2.JI_WeightUQ = "LB";
			invoiceLine2.JI_CustomsUnitQty = "T";
			invoiceLine2.JI_CustomsQuantity = 345.98m;
			invoiceLine2.JI_Volume = 34.338m;
			invoiceLine2.JI_VolumeUQ = "CF";

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 2m;
			invoiceLine3.JI_InvoiceUQ = "PK";
			invoiceLine3.JI_NetWeight = 433.9991m;
			invoiceLine3.JI_NetWeightUQ = "KG";
			invoiceLine3.JI_Weight = 344m;
			invoiceLine3.JI_WeightUQ = "LB";
			invoiceLine3.JI_CustomsUnitQty = "T";
			invoiceLine3.JI_CustomsQuantity = 345m;
			invoiceLine3.JI_Volume = 34.3m;
			invoiceLine3.JI_VolumeUQ = "CF";

			var wrapper = new CommercialInvoiceLineWrapper(invoiceLine, Factory);
			var wrapper2 = new CommercialInvoiceLineWrapper(invoiceLine2, Factory);
			var wrapper3 = new CommercialInvoiceLineWrapper(invoiceLine3, Factory);

			//decimals are aligned across the lines 
			AssertEquals("2.254 PK", wrapper.LineQuantity.ToString());
			AssertEquals("2.255 PK", wrapper2.LineQuantity.ToString());//form has a decimal places
			AssertEquals("2.000 PK", wrapper3.LineQuantity.ToString());

			AssertEquals("433.800 KG", wrapper.NetWeight.ToString());
			AssertEquals("433.000 KG", wrapper2.NetWeight.ToString());//form has a decimal places
			AssertEquals("433.999 KG", wrapper3.NetWeight.ToString());

			AssertEquals("344.80 LB", wrapper.Weight.ToString());
			AssertEquals("344.88 LB", wrapper2.Weight.ToString());
			AssertEquals("344.00 LB", wrapper3.Weight.ToString());

			AssertEquals("345.98702 T", wrapper.CustomsQuantity.ToString());
			AssertEquals("345.98000 T", wrapper2.CustomsQuantity.ToString());
			AssertEquals("345.00000 T", wrapper3.CustomsQuantity.ToString());

			AssertEquals("34.330 CF", wrapper.Volume.ToString());
			AssertEquals("34.338 CF", wrapper2.Volume.ToString());
			AssertEquals("34.300 CF", wrapper3.Volume.ToString());
		}

		public void TestRefCountryFallsBackToInvoiceHeader()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var wrapper = new CommercialInvoiceLineWrapper(invoiceLine, Factory);

			AssertEquals("AU", wrapper.RefCountryCode);
		}

		public void TestClassificationDetails()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";

			var invoiceLineMock = Factory.NewMoq<BaseJobComInvoiceLine>();
			invoiceLineMock.Protected().Setup<ZString>("ClassificationDetailsForGenericWrapperCore").Returns((ZString)"Test Details");
			var invoiceLine = invoiceLineMock.Object;
			invoiceLine.JI_JZ = invoiceHeader.PK;

			var wrapper = new CommercialInvoiceLineWrapper(invoiceLine, Factory);

			AssertEquals("Test Details", wrapper.ClassificationDetails);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CommercialInvoiceLine                          (Default Field: LineNo)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Invoice                                 CommercialInvoice
CountryOfOrigin                         Country
LinePrice                               Money
UnitPrice                               Money
CustomsQuantity                         ValueAndUnit
LineQuantity                            ValueAndUnit
NetWeight                               ValueAndUnit
Volume                                  ValueAndUnit
Weight                                  ValueAndUnit
ClassificationDetails                   String
ConcessionCode                          String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
Description                             String
DutyAmount                              Decimal
DutyAmountsAsString                     String
DutyRateDescription                     String
FormattedDescription                    String
GSTRate                                 Decimal
LineNo                                  Int
LookupCode                              String
MergedLineNumber                        String
OrderNumber                             String
PartNumber                              String
ProvProgCustomsQty                      String
ProvProgCustomsQty1                     String
ProvProgCustomsQty2                     String
ProvProgCustomsQty3                     String
ProvProgCustomsQty4                     String
ProvProgCustomsQty5                     String
ProvProgDutyRate                        String
ProvProgDutyRate1                       String
ProvProgDutyRate2                       String
ProvProgDutyRate3                       String
ProvProgDutyRate4                       String
ProvProgDutyRate5                       String
ProvProgTariff                          String
ProvProgTariff1                         String
ProvProgTariff2                         String
ProvProgTariff3                         String
ProvProgTariff4                         String
ProvProgTariff5                         String
RefCountryCode                          String
TariffCode                              String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CountryOfOrigin : ZA - South Africa
CustomsQuantity : 150 KGM
Invoice : FORGOTTEN
LinePrice : 5,434.44 ZAR
LineQuantity : 2 PK
NetWeight : 433.8 KG
Registry : (No Default Field Value Available on Registry)
UnitPrice : 2,717.22 ZAR
Volume : 34.332 CF
Weight : 344.8 LB
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceNumber = "FORGOTTEN";
			invoiceHeader.JZ_InvoiceAmount = 5434.44m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "ZAR";

			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5434.44m;
			invoiceLine.JI_InvoiceQuantity = 2;
			invoiceLine.JI_InvoiceUQ = "PK";
			invoiceLine.JI_NetWeight = 433.8m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_Volume = 34.332m;
			invoiceLine.JI_VolumeUQ = "CF";
			invoiceLine.JI_Weight = 344.8m;
			invoiceLine.JI_WeightUQ = "LB";
			invoiceLine.JI_CountryOfOrigin = "ZA";
			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomAttrib1 = "Attrib1";
			invoiceLine.JI_CustomAttrib2 = "Attrib2";
			invoiceLine.JI_CustomAttrib3 = "Attrib3";
			invoiceLine.JI_CustomAttrib4 = "Attrib4";
			invoiceLine.JI_CustomAttrib5 = "Attrib5";
			invoiceLine.JI_CustomAttrib6 = "Attrib6";

			return new CommercialInvoiceLineWrapper(invoiceLine, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CommercialInvoiceLineWrapper(null, Factory);
		}
	}
}
