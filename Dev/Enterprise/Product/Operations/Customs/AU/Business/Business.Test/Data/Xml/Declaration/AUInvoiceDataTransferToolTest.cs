using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AUInvoiceDataTransferToolTest
	{
		public AUInvoiceDataTransferToolTest(IValueObjectDataAdapter invoiceDataAdapter, GetInvoiceHeaderDelegate getInvoiceHeader, BusinessObjectFactory factory)
		{
			this.InvoiceDataAdapter = invoiceDataAdapter;
			this.GetInvoiceHeader = getInvoiceHeader;
			this.factory = factory;
		}

		public readonly GetInvoiceHeaderDelegate GetInvoiceHeader;
		public readonly IValueObjectDataAdapter InvoiceDataAdapter;
		public delegate BaseJobComInvoiceHeader GetInvoiceHeaderDelegate();
		readonly BusinessObjectFactory factory;

		public void TestSetInvoiceHeaderAdditionalInfo()
		{
			JobComInvoiceHeader invHead = (JobComInvoiceHeader)GetInvoiceHeader();
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.AdditionalCustomsInformationCollection addCusInfos = xmlInvoiceHeader.AddCustomsDetails;
			Xsd.AdditionalCustomsInformation addCusInfo = addCusInfos.AddNew();
			addCusInfo.CustomsDetailType = "ValuationBasis";
			addCusInfo.CustomsDetailValue = "UT";
			addCusInfo = addCusInfos.AddNew();
			addCusInfo.CustomsDetailType = "ADJ";
			addCusInfo.CustomsDetailValue = "123AUD";
			TestCaseWithFactory.AssertEquals("PreCondition: ZA_ValuationBasis_Hidden", "", invHead.AddInfo.ZA_ValuationBasis_Hidden);
			TestCaseWithFactory.AssertEquals("PreCondition: ZA_ADJ", "", invHead.AddInfo.ZA_ADJ);
			ValueObjectImportContext importContext = new ValueObjectImportContext(factory, new NotificationBuffer());
			InvoiceDataAdapter.ImportFromValueObject(invHead, xmlInvoiceHeader, importContext);
			TestCaseWithFactory.AssertEquals("123AUD", invHead.AddInfo.ZA_ADJ);
			TestCaseWithFactory.AssertEquals("UT", invHead.AddInfo.ZA_ValuationBasis_Hidden);
		}

		public void TestSetInvoiceLinesAdditionalInfo()
		{
			JobComInvoiceHeader invHead = (JobComInvoiceHeader)GetInvoiceHeader();
			JobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();
			Xsd.AdditionalCustomsInformationCollection addCusInfos = xmlInvoiceLine.LineClassification.AddCustomsDetails;
			Xsd.AdditionalCustomsInformation addCusInfo = addCusInfos.AddNew();
			addCusInfo.CustomsDetailType = "ValuationBasis";
			addCusInfo.CustomsDetailValue = "UT";
			TestCaseWithFactory.AssertEquals("PreCondition: ZA_ValuationBasis_Hidden", "", invLine.AddInfo.ZA_ValuationBasis_Hidden);
			ValueObjectImportContext importContext = new ValueObjectImportContext(factory, new NotificationBuffer());
			InvoiceDataAdapter.ImportFromValueObject(invHead, xmlInvoiceHeader, importContext);
			TestCaseWithFactory.AssertEquals("UT", invLine.AddInfo.ZA_ValuationBasis_Hidden);
			addCusInfo.CustomsDetailType = "ORG";
			addCusInfo.CustomsDetailValue = "UK";
			TestCaseWithFactory.AssertEquals("PreCondition: ZA_ORG", "", invLine.AddInfo.ZA_ORG);
			InvoiceDataAdapter.ImportFromValueObject(invHead, xmlInvoiceHeader, importContext);
			TestCaseWithFactory.AssertEquals("UK", invLine.AddInfo.ZA_ORG);
			addCusInfo.CustomsDetailType = "ADJ";
			addCusInfo.CustomsDetailValue = "123AUD";
			TestCaseWithFactory.AssertEquals("PreCondition: ZA_ADJ", "", invLine.AddInfo.ZA_ADJ);
			InvoiceDataAdapter.ImportFromValueObject(invHead, xmlInvoiceHeader, importContext);
			TestCaseWithFactory.AssertEquals(123m, invLine.AddInfo.AdjustmentAmount_Hidden);
			TestCaseWithFactory.AssertEquals("AUD", invLine.AddInfo.AdjustmentCurrency.RX_Code);
		}

		public void TestSetXmlInvoiceHeaderAdditionalInfo()
		{
			JobComInvoiceHeader invHeader = (JobComInvoiceHeader)GetInvoiceHeader();
			invHeader.AddInfo.LoadPropertiesFromString("ValuationBasis_Hidden=UT*ADJ=123AUD");
			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHeader, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.AdditionalCustomsInformationCollection addCusInfos = xmlInvoiceHeader.AddCustomsDetails;
			TestCaseWithFactory.Assert("ValuationBasis should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "ValuationBasis"));
			TestCaseWithFactory.Assert("ADJ should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "ADJ"));
			TestCaseWithFactory.AssertEquals("ValuationBasis Value", "UT", GetXmlAddInfoValue(addCusInfos, "ValuationBasis"));
			TestCaseWithFactory.AssertEquals("ADJ Value", "123AUD", GetXmlAddInfoValue(addCusInfos, "ADJ"));
		}

		public void TestSetXmlInvoiceLinesAdditionalInfo()
		{
			JobComInvoiceHeader invHead = (JobComInvoiceHeader)GetInvoiceHeader();
			JobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();
			invLine.AddInfo.LoadPropertiesFromString("ValuationBasis_Hidden=UT*ADJ=123AUD");
			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHead, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.AdditionalCustomsInformationCollection addCusInfos = xmlInvoiceHeader.InvoiceLines[0].LineClassification.AddCustomsDetails;
			TestCaseWithFactory.Assert("ValuationBasis should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "ValuationBasis"));
			TestCaseWithFactory.Assert("ADJ should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "ADJ"));
			TestCaseWithFactory.AssertEquals("ValuationBasis Value", "UT", GetXmlAddInfoValue(addCusInfos, "ValuationBasis"));
			TestCaseWithFactory.AssertEquals("ADJ Value", "123AUD", GetXmlAddInfoValue(addCusInfos, "ADJ"));
		}

		public void TestAUSetInvoiceLineSummary()
		{
			JobDeclaration jobDec = factory.New<JobDeclaration>();
			jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			jobDec.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			jobDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			JobComInvoiceHeader invHead = jobDec.Invoices.AddNew();
			invHead.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, Enterprise.Core.Constants.CurrencyCodes.Australia);
			invHead.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 20, Enterprise.Core.Constants.CurrencyCodes.Australia);
			invHead.JZ_IncoTerm = "CIF";
			invHead.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			JobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();
			invLine.JI_PartNo = "PartNO";
			invLine.JI_LinePrice = 1000m;
			TestCaseWithFactory.AssertEquals("Precondition: Jobdec doesn't have cusentry", 0, jobDec.CustomsEntryHeaders.Count);
			TestCaseWithFactory.AssertEquals(0m, invLine.JI_Calc_CustomsTransportAndInsuranceInLocalCurrency);
			ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
			Xsd.InvoiceHeader invoiceHeaderXsd = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHead, context);
			Xsd.InvoiceLineSummary summary = invoiceHeaderXsd.InvoiceLines[0].Summary;
			TestCaseWithFactory.AssertEquals("T&I", invLine.TransportAndInsuranceInLocalCurrency, summary.TransportAndInsurance.Value);
			jobDec.DoMerge();
			context = new ValueObjectExportContext(new NotificationBuffer());
			invLine.AddInfo.ZA_TILV = "30AUD";
			TestCaseWithFactory.AssertEquals(30m, invLine.TransportAndInsuranceInLocalCurrency);
			TestCaseWithFactory.AssertEquals(30m, invLine.JI_Calc_CustomsTransportAndInsuranceInLocalCurrency);
			invoiceHeaderXsd = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHead, context);
			summary = invoiceHeaderXsd.InvoiceLines[0].Summary;
			TestCaseWithFactory.AssertEquals("T&I value is copied from dbo.cusentryline", invLine.JI_Calc_CustomsTransportAndInsuranceInLocalCurrency, summary.TransportAndInsurance.Value);
		}

		public void TestAUSetInvoiceLineDetail()
		{
			JobDeclaration jobDec = factory.New<JobDeclaration>();
			JobComInvoiceHeader invHead = jobDec.Invoices.AddNew();
			JobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();
			Xsd.InvoiceHeader invHeaderXsd = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine invLineXsd = invHeaderXsd.InvoiceLines.AddNew();
			invLineXsd.LineClassification = new Xsd.InvoiceLineLineClassification();
			invLineXsd.LineClassification.Preference = "T";
			invLineXsd.LineClassification.TreatmentCode = "UT";
			ValueObjectImportContext context = new ValueObjectImportContext(factory, new NotificationBuffer());
			InvoiceDataAdapter.ImportFromValueObject(invHead, invHeaderXsd, context);
			TestCaseWithFactory.AssertEquals("T", invLine.AddInfo.ZA_PRF);
			TestCaseWithFactory.AssertEquals("UT", invLine.AddInfo.ZA_TreatmentCode_Hidden);
		}

		public void TestAUSetInvoiceLineDetail_Bond()
		{
			JobDeclaration jobDec = factory.New<JobDeclaration>();
			JobComInvoiceHeader invHead = jobDec.Invoices.AddNew();
			JobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();
			Xsd.InvoiceHeader invHeaderXsd = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine invLineXsd = invHeaderXsd.InvoiceLines.AddNew();
			invLineXsd.Bond = true;
			ValueObjectImportContext context = new ValueObjectImportContext(factory, new NotificationBuffer());
			InvoiceDataAdapter.ImportFromValueObject(invHead, invHeaderXsd, context);
			TestCaseWithFactory.AssertEquals("Bond", true, invLine.JI_IsPackToBondForLine);
			invLineXsd.Bond = false;
			InvoiceDataAdapter.ImportFromValueObject(invHead, invHeaderXsd, context);
			TestCaseWithFactory.AssertEquals("Bond", false, invLine.JI_IsPackToBondForLine);
		}

		public void TestNewBusinessObject()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			NotificationBuffer notification = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(factory, notification);
			xmlInvoiceHeader.IsGroupInvoice = Xsd.TrueFalse.@false;
			TestCaseWithFactory.AssertEquals(typeof(JobComInvoiceHeader), InvoiceDataAdapter.NewBusinessObject(xmlInvoiceHeader, context).GetType());
		}

		public void TestSetXmlInvoiceLineDetails_ClassInfo()
		{
			JobDeclaration jobDec = factory.New<JobDeclaration>();
			JobComInvoiceHeader invHead = jobDec.Invoices.AddNew();
			JobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();
			Classification @class = factory.New<Classification>();
			@class.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			@class.InstrumentType = "AAA";
			@class.InstrumentCode = "BBB";
			@class.TreatmentCode = "CCC";
			invLine.JI_CC = @class.PK;
			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHead, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.InvoiceLine xsdLine = xmlInvoiceHeader.InvoiceLines[0];
			TestCaseWithFactory.AssertEquals("InstrumentType", "AAA", xsdLine.LineClassification.InstrumentType);
			TestCaseWithFactory.AssertEquals("InstrumentCode", "BBB", xsdLine.LineClassification.InstrumentCode);
			TestCaseWithFactory.AssertEquals("TreatmentCode", "CCC", xsdLine.LineClassification.TreatmentCode);
		}

		public void TestSetXmlInvoiceLineDetails_Bond()
		{
			JobDeclaration jobDec = factory.New<JobDeclaration>();
			JobComInvoiceHeader invHead = jobDec.Invoices.AddNew();
			JobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();
			invLine.JI_IsPackToBondForLine = true;
			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHead, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.InvoiceLine xsdLine = xmlInvoiceHeader.InvoiceLines[0];
			TestCaseWithFactory.AssertEquals("Bond", true, xsdLine.Bond);
			invLine.JI_IsPackToBondForLine = false;
			xmlInvoiceHeader = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHead, new ValueObjectExportContext(new NotificationBuffer()));
			xsdLine = xmlInvoiceHeader.InvoiceLines[0];
			TestCaseWithFactory.AssertEquals("Bond", false, xsdLine.Bond);
		}

		protected bool ItemInXmlAddInfoCollection(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
		{
			return Enterprise.Customs.DataTransfer.Testing.AddInfoDataTransferToolTest.ItemInXmlAddInfoCollection(addCusInfos, itemType);
		}

		protected string GetXmlAddInfoValue(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
		{
			return Enterprise.Customs.DataTransfer.Testing.AddInfoDataTransferToolTest.GetXmlAddInfoValue(addCusInfos, itemType);
		}
	}
}
