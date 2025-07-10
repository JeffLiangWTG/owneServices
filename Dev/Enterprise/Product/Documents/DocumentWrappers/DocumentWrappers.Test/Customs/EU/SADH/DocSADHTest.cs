using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.UnitedKingdom)]
	public class DocSADHTest : DocumentWrappers.Testing.DocBaseWrapperTest
	{
		public void TestExcelTemplateMustHavePrintAreaSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();

			Enumerable.Range(0, 4).ForEach(i => AddNewEntryLine(i));
			Factory.Save();

			var entryHeaderSadHMenuItemPK = "A412ABB6-0162-46EA-AB55-6C1177A8B131";
			AssertNumberOfPrintedPages(numberOfPages, entryHeaderSadHMenuItemPK, entryHeader);

			var customsSadHPlainTextMenuItemPK = "ED2D13CB-57FE-4F20-86F5-4ADC23E1C92D";
			AssertNumberOfPrintedPages(numberOfPages, customsSadHPlainTextMenuItemPK, declaration);

			void AssertNumberOfPrintedPages(int expectedPages, string documentMenuItemPk, BusinessObject bizO)
			{
				if (bizO is not IDocumentSupportable)
				{
					Fail($"The Bizo: {bizO.HumanReadableName} does not implement the {nameof(IDocumentSupportable)}. Test cannot be run");
				}

				if (bizO is not IDocManagerSupport)
				{
					Fail($"The Bizo: {bizO.HumanReadableName} does not implement the {nameof(IDocManagerSupport)}. Test cannot be run");
				}

				var documentSupportable = bizO as IDocumentSupportable;
				var docManagerSupport = bizO as IDocManagerSupport;

				var sadHMenuItemPk = new ZGuid(documentMenuItemPk);
				var documentCommandThatWeWillFireAsIfUserClickedIt = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(StmMenuItemSchema.PK, sadHMenuItemPk));

				var silentDocumentPrinter = new SilentDocumentPrinter(Factory, documentSupportable, documentCommandThatWeWillFireAsIfUserClickedIt);
				silentDocumentPrinter.Print(ZGuid.Empty, 0, true, forceCorrectBizObjWhenPrintingToBothEdocsAndPaper: true, businessObjectForPrintJobParent: docManagerSupport.DocManagerInfo());
				Factory.Save();

				var filterQuery = new ZQuery(StmPrintJobSchema.SP_DocumentName, SQLComparisonOperator.Contains, "SADH C88");

				var printedJobs = Factory.Load<StmPrintJob>(filterQuery);
				AssertGreaterThanOrEqualTo($"[PRE-CONDITION] Printed Jobs Count for {documentCommandThatWeWillFireAsIfUserClickedIt.SU_MenuName}", printedJobs.Length, 1);

				int i = 0;
				foreach (var printedJob in printedJobs)
				{
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(printedJob.SP_CustomProperties);
						using (var stream = new MemoryStream())
						using (var pdfExport = new FlexCelPdfExportSafe(excelInterface.Xls, true))
						{
							pdfExport.Export(stream);

							var message = @"Looks like you have changed the print area by accident, if you have to do so, please double check that you have not broken the report, see discussion for details:
https://teams.microsoft.com/l/message/19:333de70778314179b35dead529441dc1@thread.skype/1641988430883?tenantId=8b493985-e1b4-4b95-ade6-98acafdbdb01&groupId=188ba793-8136-4b10-b984-10097d7bb40c&parentMessageId=1641988430883&teamName=Development%20Customs%20Team&channelName=General&createdTime=1641988430883";
							AssertEquals(message, expectedPages, pdfExport.Progress.TotalPage);
						}
					}
					i++;
				}
			}

			void AddNewEntryLine(int itemNumber)
			{
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_Description = new ZString(itemNumber.ToString())
					.PadRight(CusEntryLineSchema.CL_Description.MaxLength, itemNumber.ToString().ToCharArray().First());
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		public virtual int numberOfPages => 2;

		public void TestBox2Override()
		{
			var supplier = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			supplierAddress.OA_Address1 = "address1";
			supplierAddress.OA_Address2 = "address2";
			supplierAddress.OA_City = "city";
			supplierAddress.OA_State = "state";
			supplierAddress.OA_PostCode = "111";
			supplierAddress.OA_RL_NKRelatedPortCode = "COBOG";

			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertContains("ADDRESS1", wrapper.Box2Supplier.ToString());
			AssertContains("ADDRESS2", wrapper.Box2Supplier.ToString());
			AssertContains("CITY", wrapper.Box2Supplier.ToString());
			AssertContains("111", wrapper.Box2Supplier.ToString());
			AssertContains("STATE", wrapper.Box2Supplier.ToString());
			AssertContains("COLOMBIA", wrapper.Box2Supplier.ToString());

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_Address1 = "new address1";
			declaration.SupplierDocumentaryAddress.E2_Address2 = "new address2";
			declaration.SupplierDocumentaryAddress.E2_City = "new city";
			declaration.SupplierDocumentaryAddress.E2_Postcode = "222";
			declaration.SupplierDocumentaryAddress.E2_State = "new state";
			declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "MX";

			wrapper = DocSADH.New(entryHeader, Factory);

			AssertContains("NEW ADDRESS1", wrapper.Box2Supplier.ToString());
			AssertContains("NEW ADDRESS2", wrapper.Box2Supplier.ToString());
			AssertContains("NEW CITY", wrapper.Box2Supplier.ToString());
			AssertContains("222", wrapper.Box2Supplier.ToString());
			AssertContains("NEW STATE", wrapper.Box2Supplier.ToString());
			AssertContains("MEXICO", wrapper.Box2Supplier.ToString());
		}

		public void TestShowBox2SupplierCountryCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals(true, wrapper.ShowBox2SupplierCountryCode);
		}

		public void TestBox8Override()
		{
			var importer = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var importerAddress = importer.Addresses.AddNew();
			importerAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			importerAddress.OA_Address1 = "address1";
			importerAddress.OA_Address2 = "address2";
			importerAddress.OA_City = "city";
			importerAddress.OA_State = "state";
			importerAddress.OA_PostCode = "111";
			importerAddress.OA_RL_NKRelatedPortCode = "COBOG";

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertContains("ADDRESS1", wrapper.Box8Importer.ToString());
			AssertContains("ADDRESS2", wrapper.Box8Importer.ToString());
			AssertContains("CITY", wrapper.Box8Importer.ToString());
			AssertContains("111", wrapper.Box8Importer.ToString());
			AssertContains("STATE", wrapper.Box8Importer.ToString());
			AssertContains("COLOMBIA", wrapper.Box8Importer.ToString());

			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_Address1 = "new address1";
			declaration.ImporterDocumentaryAddress.E2_Address2 = "new address2";
			declaration.ImporterDocumentaryAddress.E2_City = "new city";
			declaration.ImporterDocumentaryAddress.E2_Postcode = "222";
			declaration.ImporterDocumentaryAddress.E2_State = "new state";
			declaration.ImporterDocumentaryAddress.E2_RN_NKCountryCode = "MX";

			wrapper = DocSADH.New(entryHeader, Factory);

			AssertContains("NEW ADDRESS1", wrapper.Box8Importer.ToString());
			AssertContains("NEW ADDRESS2", wrapper.Box8Importer.ToString());
			AssertContains("NEW CITY", wrapper.Box8Importer.ToString());
			AssertContains("222", wrapper.Box8Importer.ToString());
			AssertContains("NEW STATE", wrapper.Box8Importer.ToString());
			AssertContains("MEXICO", wrapper.Box8Importer.ToString());
		}

		public void TestShowBox8ImporterCountryCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals(true, wrapper.ShowBox8ImporterCountryCode);
		}

		public virtual void TestBox14DeclarantRepresentative()
		{
			OrgHeader declarantOrgHeader = Factory.New<OrgHeader>();
			declarantOrgHeader.OH_FullName = "My Test org for Box14";

			OrgAddress declarantAddress = declarantOrgHeader.Addresses.AddNew();
			declarantAddress.OA_Address1 = "Address Line from Address against Org for Box14!";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName", "EDI", wrapper.Box14DeclarantRepresentative.CompanyName.Left(3));

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("wrapper.ShowDeclarantRepresentativeAddress - rep 2", true, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName - rep 2", declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
			AssertEquals("wrapper.Box14DeclarantRepresentative.Address1 - rep 2", declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertEquals("wrapper.ShowDeclarantRepresentativeAddress - rep 3", true, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("wrapper.Box14DeclarantRepresentative.CompanyName - rep 3", declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
			AssertEquals("wrapper.Box14DeclarantRepresentative.Address1 - rep 3", declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals("wrapper.ShowDeclarantRepresentativeAddress for self rep", false, wrapper.ShowDeclarantRepresentativeAddress);
			AssertEquals("Box 14 should be empty for self rep", null, wrapper.Box14DeclarantRepresentative);

			AssertEquals("Box 14 should be empty for self rep", ZString.Empty, wrapper.Box14AlternativeText);
		}

		public void TestEPUandENO()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);

			entryHeader.EntryNumber = "071-041987W";
			AssertEquals("wrapper.EPU", "071", wrapper.EPU);
			AssertEquals("wrapper.ENO", "041987W", wrapper.ENO);

			entryHeader.EntryNumber = "120-A00537W";
			AssertEquals("wrapper.EPU", "120", wrapper.EPU);
			AssertEquals("wrapper.ENO", "A00537W", wrapper.ENO);

			entryHeader.EntryNumber = "120A00537W";
			AssertEquals("wrapper.EPU", "", wrapper.EPU);
			AssertEquals("wrapper.ENO", "120A00537W", wrapper.ENO);
		}

		[TestDate(2008, 7, 1)]
		public virtual void TestDOE()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CHF";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			CusEntryNumber num = CusEntryNumber.New(entryHeader, "EXP", declaration.CountryCode);
			num.CE_EntryNum = "071-12465A";
			num.CE_IssueDate = ZDateTime.BrettsBirthday;

			AssertEquals("wrapper.DOE - from CE_IssueDate", ExpectedDOE, wrapper.DOE);

			num.CE_IssueDate = ZDateTime.Empty;
			AssertEquals("wrapper.DOE - from last message", new ZDateTime(2008, 7, 1), wrapper.DOE);

			EDIMessage message = entryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2008, 7, 2);
			AssertEquals("wrapper.DOE - from last TRANSMITTED message", new ZDateTime(2008, 7, 1), wrapper.DOE);

			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			AssertEquals("wrapper.DOE -  - from last transmitted message", new ZDateTime(2008, 7, 2), wrapper.DOE);
		}

		public void TestBox1aEntryStyleBox1bSubStyle()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CHF";
			declaration.JE_EntryStyle = "EX";

			var cei = declaration.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "AAA";
			cei.CEI_SubStyle = "B";

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = ZGuid.Empty;  // Simulates GB Chief instructions
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("EX", wrapper.Box1aEntryStyle);
			AssertEquals("B", wrapper.Box1bSubStyle);

			declaration.JE_ApplicationCode = "CDS";
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_SubStyle = "Y";

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = cei2.PK;
			AssertNotNull(entryHeader2.EntryInstruction);
			var wrapper2 = DocSADH.New(entryHeader2, Factory);
			AssertEquals("EX", wrapper2.Box1aEntryStyle);
			AssertEquals("Y", wrapper2.Box1bSubStyle);
		}

		[TestDate(2008, 6, 8)]
		public virtual void TestBox22CurrencyAndTotalAmountInvoice()
		{
			RefCurrency gbp = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.UnitedKingdom);
			RefCurrency aud = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.Australia);
			RefCurrency usd = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			JobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceHeader1.JZ_InvoiceAmount = 100.00m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine1.JI_LinePrice = 100.00m;

			invoiceHeader2.JZ_InvoiceAmount = 200.00m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine2.JI_LinePrice = 200.00m;

			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("wrapper.Box22TotalPriceInvoiced", 300.00m, wrapper.Box22TotalPriceInvoiced);
			AssertEquals("wrapper.Box22TotalPriceCurrency", "USD", wrapper.Box22TotalPriceCurrency);

			invoiceHeader1.JZ_RX_NKInvoice_Currency = aud.RX_Code;
			AssertEquals("wrapper.Box22TotalPriceInvoiced", 192.90m, wrapper.Box22TotalPriceInvoiced);
			AssertEquals("wrapper.Box22TotalPriceCurrency", CountrySpecificCurrency, wrapper.Box22TotalPriceCurrency);

			invoiceHeader2.JZ_RX_NKInvoice_Currency = aud.RX_Code;
			AssertEquals("wrapper.Box22TotalPriceInvoiced", 300.00m, wrapper.Box22TotalPriceInvoiced);
			AssertEquals("wrapper.Box22TotalPriceCurrency", "AUD", wrapper.Box22TotalPriceCurrency);
		}

		protected virtual ZString CountrySpecificCurrency { get { return "GBP"; } }

		public virtual void TestBox18IdentityOfTransportAtDeparture()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);

			declaration.ZG_Box18TransportID = "RAIL 1234";
			AssertEquals("declaration.ZG_Box18TransportID", "RAIL 1234", wrapper.Box18IdentityOfTransportAtDeparture);
		}

		public void TestBox18IdentityOfTransportAtDeparture_Import()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);

			declaration.ZG_Box18TransportID = "RAIL 1234";
			AssertEquals("declaration.ZG_Box18TransportID", ZString.Empty, wrapper.Box18IdentityOfTransportAtDeparture);
		}

		public void TestBox18NationalityOfTransportAtDeparture_Export_NotUcc6()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			using (TemporarilySetUCC6Configuration(declaration, false))
			{
				declaration.ZG_Box18TransportNationality = "CN";
				AssertEquals("declaration.ZG_Box18TransportNationality", "CN", wrapper.Box18TransportNationalityAtDeparture);
			}
		}

		public void TestBox18NationalityOfTransportAtDeparture_Export_Ucc6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_RN_NKTransportNationalityInland = "IT";
			declaration.JE_RN_NKTrailer1Nationality = "IN";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = GetNewDocumentWrapper(entryHeader);

			using (TemporarilySetUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					declaration.JE_TransportModeInland = Core.Constants.TransportModes.Air;
					declaration.JE_TransportIDInland = "123";
					AssertEquals("AIR, Transport ID not empty", "IT", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportIDInland = string.Empty;
					declaration.JE_AircraftRegistrationInland = "123";
					AssertEquals("AIR, Transport ID empty but aircraft registration not empty", "IN", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_AircraftRegistrationInland = string.Empty;
					AssertEquals("AIR, Transport ID and aircraft registration empty", ZString.Empty, wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportModeInland = Core.Constants.TransportModes.Rail;
					declaration.JE_TransportIDInland = "123";
					AssertEquals("Rail, Transport ID not empty", "IT", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportIDInland = string.Empty;
					declaration.JE_Trailer1RegNo = "123";
					AssertEquals("Rail, Transport ID empty but trailer registration number not empty", "IN", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_Trailer1RegNo = string.Empty;
					AssertEquals("Rail, Transport ID and trailer registration number empty", ZString.Empty, wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
					declaration.JE_TransportIDInland = "123";
					AssertEquals("Sea, Transport ID not empty", "IT", wrapper.Box18TransportNationalityAtDeparture);

					declaration.JE_TransportIDInland = "123";
					AssertEquals("Sea, Transport ID empty", "IT", wrapper.Box18TransportNationalityAtDeparture);
				});
			}
		}

		public void TestBox18NationalityOfTransportAtDeparture_Import()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);

			declaration.ZG_Box18TransportNationality = "CN";
			AssertEquals("declaration.ZG_Box18TransportNationality", ZString.Empty, wrapper.Box18TransportNationalityAtDeparture);
		}

		public virtual void TestBox24TransactionNature()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationCode = "12";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("12", wrapper.Box24TransactionNature);
		}

		public void TestBox29ExitOffice()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_CustomsOffice = "HU000069";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IT018100");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "IT016199");

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			declaration.JE_MessageType = "EXP";
			AssertContains("When declaration.JE_MessageType is EXP, Box29ExitOffice", entryHeader.OfficeOfExit, wrapper.Box29ExitOffice);

			declaration.JE_MessageType = "IMP";
			AssertContains("When declaration.JE_MessageType is IMP, Box29ExitOffice", entryHeader.OfficeOfEntry, wrapper.Box29ExitOffice);
		}

		public void TestBox21IdentityOfTransportCrossingTheBorder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);

			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF253";
			AssertEquals("declaration.IdentityOfTransportCrossingTheBorder", "QF253", wrapper.Box21IdentityOfTransportCrossingTheBorder);
			declaration.JE_ExportDate = new ZDateTime(2008, 9, 12);
			AssertEquals("declaration.IdentityOfTransportCrossingTheBorder", "QF253 / 12-Sep-08", wrapper.Box21IdentityOfTransportCrossingTheBorder);

			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_VoyageFlightNo = "66";
			declaration.JE_VesselName = "PERINEAL EXPLORER";
			AssertEquals("declaration.IdentityOfTransportCrossingTheBorder", "PERINEAL EXPLORER / 66", wrapper.Box21IdentityOfTransportCrossingTheBorder);

			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Road;
			declaration.JE_VesselName = "BJG-010";
			AssertEquals("declaration.IdentityOfTransportCrossingTheBorder", "BJG-010", wrapper.Box21IdentityOfTransportCrossingTheBorder);

			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Rail;
			declaration.JE_VesselName = "AS3245";
			AssertEquals("declaration.IdentityOfTransportCrossingTheBorder", "AS3245", wrapper.Box21IdentityOfTransportCrossingTheBorder);

			declaration.JE_RN_NKTransportNationality = "NL";
			AssertEquals("Box21TransportNationality", "NL", wrapper.Box21TransportNationality);
		}

		public void TestBox26TransportModeInland()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_TransportModeInland = "SEA";
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Box26TransportModeInland", "1", wrapper.Box26TransportModeInland);

			declaration.JE_TransportModeInland = "13";
			wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Box26TransportModeInland", "13", wrapper.Box26TransportModeInland);
		}

		public void TestBox27PortOfLoading()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfLoading = "PORT";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertEquals("Box27PortOfLoading", "PORT", wrapper.Box27PortOfLoading);
		}

		public void TestBoxS28Seals()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.CusContainers.AddNew().CO_Seal = "S0001";
			declaration.CusContainers.AddNew().CO_Seal = "S0002";

			foreach (Enterprise.Customs.Business.NonPersistentCusContainer container in invoiceLine.ContainersForInvoiceLinesForBindingOnly)
			{
				container.IsForInvoiceLine = true;
			}

			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("S0001, S0002", wrapper.BoxS28Seals);
		}

		public void TestBoxS28Seals_WhenEntryContainsMultipleDuplicatedSeals()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			AddSeal("S0001", "S0002");
			AddSeal("S0001", "S0002");

			SetContainerForInvoiceLine(invoiceLine1);
			SetContainerForInvoiceLine(invoiceLine2);

			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("S0001, S0002", wrapper.BoxS28Seals);

			void AddSeal(string seal1, string seal2)
			{
				var cnt = declaration.CusContainers.AddNew();
				cnt.CO_Seal = seal1;
				cnt.CO_SecondSeal = seal2;
			}

			void SetContainerForInvoiceLine(JobComInvoiceLine invoiceLine)
			{
				invoiceLine.ContainersForInvoiceLinesForBindingOnly
					.Cast<Enterprise.Customs.Business.NonPersistentCusContainer>()
					.ForEach(c => c.IsForInvoiceLine = true);
			}
		}

		public void TestIsIndirectExport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "EXP";
				declaration.CustomsOffices.RemoveAndDeleteAll();
				var cusOffice = declaration.CustomsOffices.AddNew();
				cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
				cusOffice.CY_Data = "FR000025";
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();

				var wrapper = DocSADH.New(entryHeader, Factory);

				AssertEquals(true, wrapper.IsIndirectExport);
			}
		}

		public virtual void TestTimeLimitDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var number = Factory.New<CusEntryNumber>();
			number.CE_IssueDate = new ZDateTime(1987, 12, 11);
			number.CE_ParentID = entryHeader.PK;
			number.CE_ParentTable = CusEntryHeader.Schema.TableName;
			number.CE_EntryType = "EXP";

			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals(ZString.Empty, wrapper.TimeLimitDate);
		}

		public virtual void TestShowExportAccompanyingDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals(true, wrapper.ShowExportAccompanyingDocument);
		}

		public void TestBox30LocationOfGoodsUnitedKingdom()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
				CusEntryNumber number = Factory.New<CusEntryNumber>();
				number.CE_IssueDate = new ZDateTime(1987, 12, 11);
				number.CE_ParentID = entryHeader.PK;
				number.CE_ParentTable = CusEntryHeader.Schema.TableName;
				number.CE_EntryType = "EXP";

				AssertBox30LocationOfGoods(declaration, entryHeader, "", "", "");
				AssertBox30LocationOfGoods(declaration, entryHeader, "LHR", "", "GBLHR");
				AssertBox30LocationOfGoods(declaration, entryHeader, "LHR", "BAC", "GBLHRBAC");
			}
		}

		void AssertBox30LocationOfGoods(JobDeclaration declaration, CusEntryHeader entryHeader, ZString locationOfGoods, ZString shedCode, ZString expectedAssertion)
		{
			declaration.JE_LocationOfGoods = locationOfGoods;
			if (!shedCode.IsEmpty)
			{
				declaration.SubLocation = shedCode;
			}
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals(expectedAssertion, wrapper.Box30LocationOfGoods);
		}

		public virtual void TestBox47aCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertEquals("<Box47aCaption>", "Type", wrapper.Box47aCaption);
		}

		public virtual void TestBox47aMethodPaiement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertEquals("<Box47aMethodPayment>", "MP", wrapper.Box47eMethodPayment);
		}

		public void TestBox54Details_Box54Place()
		{
			var (_, entryHeader) = CreateEntryHeader();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("entryHeader.Box54Place", "Felixstowe", wrapper.Box54Place);
		}

		public void TestBox54Details_Box54SignatoryNameAndPosition()
		{
			var broker = CreateBroker("F_N");
			var (_, entryHeader) = CreateEntryHeader(broker.GS_Code);

			var wrapper = DocSADH.New(entryHeader, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.Box54SignatoryNameAndPosition", broker.GS_FullName + " (Headkicker)", wrapper.Box54SignatoryNameAndPosition);

				broker.GS_Title = "";
				AssertEquals("entryHeader.Box54SignatoryNameAndPosition", broker.GS_FullName, wrapper.Box54SignatoryNameAndPosition);
			});
		}

		public void TestBox54Details_Box54SignatoryContactDetails()
		{
			var broker = CreateBroker("F_N");
			var (_, entryHeader) = CreateEntryHeader(broker.GS_Code);

			var wrapper = DocSADH.New(entryHeader, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.Box54SignatoryContactDetails", "Tel:+61 420 019 999  Direct:+61 2 5555 9999", wrapper.Box54SignatoryContactDetails);

				broker.GS_WorkPhone = "";
				AssertEquals("entryHeader.Box54SignatoryContactDetails", "Tel:+61 420 019 999", wrapper.Box54SignatoryContactDetails);
			});
		}

		public virtual void TestBox54Details_Box54NameOfDeclarantAndRepresentative()
		{
			var broker = CreateBroker("F_N");
			var (declaration, entryHeader) = CreateEntryHeader(broker.GS_Code);

			var wrapper = DocSADH.New(entryHeader, Factory);

			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative", "EDI CUSTOMS BROKERS by Some Big Company Ltd", wrapper.Box54NameOfDeclarantAndRepresentative);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative", "Some Big Company Ltd", wrapper.Box54NameOfDeclarantAndRepresentative);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative", "Some Big Company Ltd", wrapper.Box54NameOfDeclarantAndRepresentative);

				var oh = Factory.New<OrgHeader>();
				oh.OH_FullName = "Daniel Declaring party";
				var oa = oh.Addresses.AddNewMainAddress();
				declaration.JE_OA_DeclarantAddress = oa.PK;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative", "Daniel Declaring party by Some Big Company Ltd", wrapper.Box54NameOfDeclarantAndRepresentative);
			});
		}

		public void TestBox54UserDetailsComeFromTheLastPersonToSendToCustoms_NonSystemAccount()
		{
			var broker = CreateBroker("F_N");
			var (_, entryHeader) = CreateEntryHeader();

			var sentMessage1 = entryHeader.Messages.AddNew();
			sentMessage1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			sentMessage1.EM_SystemCreateUser = broker.GS_Code;

			var sentMessage2 = entryHeader.Messages.AddNew();
			sentMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			sentMessage2.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code; // CargoWise Support

			var wrapper = DocSADH.New(entryHeader, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.Box54SignatoryNameAndPosition", "Frederika Nerkus (Headkicker)", wrapper.Box54SignatoryNameAndPosition);
				AssertEquals("entryHeader.Box54SignatoryContactDetails", "Tel:+61 420 019 999  Direct:+61 2 5555 9999", wrapper.Box54SignatoryContactDetails);
			});
		}

		public void TestBox54UserDetailsFallingBackToDeclarationBroker_NoMessage()
		{
			var broker = CreateBroker("F_N");
			var (_, entryHeader) = CreateEntryHeader(broker.GS_Code);

			var wrapper = DocSADH.New(entryHeader, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.Box54SignatoryNameAndPosition", "Frederika Nerkus (Headkicker)", wrapper.Box54SignatoryNameAndPosition);
				AssertEquals("entryHeader.Box54SignatoryContactDetails", "Tel:+61 420 019 999  Direct:+61 2 5555 9999", wrapper.Box54SignatoryContactDetails);
			});
		}

		public void TestBox54UserDetails_Empty_NoMessage_NoBrokerInfo()
		{
			var (_, entryHeader) = CreateEntryHeader();
			var wrapper = DocSADH.New(entryHeader, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("entryHeader.Box54SignatoryNameAndPosition", "", wrapper.Box54SignatoryNameAndPosition);
				AssertEquals("entryHeader.Box54SignatoryContactDetails", "Tel:+61 420 019 999", wrapper.Box54SignatoryContactDetails);
			});
		}

		public (JobDeclaration, CusEntryHeader) CreateEntryHeader(string cusAgent = "")
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_GS_NKCusAgent = cusAgent;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			declaration.Branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			declaration.Branch.GB_RL_NKHomePort = "GBFXT";
			declaration.Branch.GB_BranchName = "WAHOO WAHOONIES";
			declaration.Branch.GB_Phone = "0420 019 999";
			declaration.Branch.Company.GC_Name = "Some Big Company Ltd";

			return (declaration, entryHeader);
		}

		public GlbStaff CreateBroker(ZString staffCode)
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = staffCode;
			broker.GS_FullName = "Frederika Nerkus";
			broker.GS_WorkPhone = "02 5555 9999";
			broker.GS_Title = "Headkicker";
			broker.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			return broker;
		}

		public void TestBoxDContainerSealsAffixed()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";
			container1.CO_Seal = "SEAL1";

			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_Seal = "SEAL2";
			container2.CO_ContainerNumber = "2";

			CusContainer container0 = declaration.CusContainers.AddNew();
			container0.CO_ContainerNumber = "0";

			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("SEAL1, SEAL2", wrapper.BoxDContainerSealsAffixed);
		}

		public void TestPagination()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine line1 = entryHeader.MergedLines.AddNew();
			CusEntryLine line2 = entryHeader.MergedLines.AddNew();
			CusEntryLine line3 = entryHeader.MergedLines.AddNew();
			CusEntryLine line4 = entryHeader.MergedLines.AddNew();
			CusEntryLine line5 = entryHeader.MergedLines.AddNew();
			CusEntryLine line6 = entryHeader.MergedLines.AddNew();
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);

			AssertEquals(declaration, wrapper.Declaration);
			AssertEquals(entryHeader, wrapper.EntryHeader);

			AssertEquals(3, wrapper.Pages.Count);
			AssertEquals(line1, wrapper.Pages[0].Line1.WrappedObject);
			AssertEquals(null, wrapper.Pages[0].Line2);
			AssertEquals(null, wrapper.Pages[0].Line3);

			AssertEquals(line2, wrapper.Pages[1].Line1.WrappedObject);
			AssertEquals(line3, wrapper.Pages[1].Line2.WrappedObject);
			AssertEquals(line4, wrapper.Pages[1].Line3.WrappedObject);

			AssertEquals(line5, wrapper.Pages[2].Line1.WrappedObject);
			AssertEquals(line6, wrapper.Pages[2].Line2.WrappedObject);
			AssertEquals(null, wrapper.Pages[2].Line3);
		}

		public virtual void TestBox14DetailsFooter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals(ZString.Empty, wrapper.Box14DetailsFooter);
		}

		public virtual void TestBox23ExchangeRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions("Exchange rate", () =>
			{
				var wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("When no exchange rate has been provided, empty string is expected", ZString.Empty, wrapper.Box23ExchangeRate);

				invoiceHeader.JZ_InvoiceCurrExRate = 1.200000m;
				AssertEquals("When exchange rate has been provided, it must be shown in the wrapper", "1.200000", wrapper.Box23ExchangeRate);
			});
		}

		public virtual void TestBox19HasContainer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("0", wrapper.Box19HasContainer);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("0", wrapper.Box19HasContainer);
			var container = declaration.CusContainers.AddNew();
			AssertEquals("0", wrapper.Box19HasContainer);
			container.CO_ContainerNumber = "CNT1";
			AssertEquals("1", wrapper.Box19HasContainer);
		}

		public virtual void TestBox15SupplierState()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals(ZString.Empty, wrapper.Box15SupplierState);
		}

		public virtual void TestEadBarcode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("21IT1S31T0003580T");

			SetCustomsOfficeOfExit(declaration, "HU000069");
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals($"When Box29 Office of Exit is not empty, {nameof(DocSADH.EadBarcode)}", ExpectedEadBarCode, wrapper.EadBarcode);

			SetCustomsOfficeOfExit(declaration, "");
			wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals($"When Box29 Office of Exit is empty, {nameof(DocSADH.EadBarcode)}", "", wrapper.EadBarcode);
		}

		public void TestLabelsCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("DispatchOfficeTitleCaption Test", "A OFFICE OF DISPATCH/EXPORT/DESTINATION", wrapper.DispatchOfficeTitleCaption);
				AssertEquals("DispatchOfficeTitleBisPageCaption Test", "OFFICE OF DISPATCH/EXPORT/DESTINATION", wrapper.DispatchOfficeTitleBisPageCaption);
				AssertEquals("Box10LabelPart2Caption Test", "last consig.", wrapper.Box10LabelPart2Caption);
				AssertEquals("Box11LabelPart1Caption Test", "Trad./Prod", wrapper.Box11LabelPart1Caption);
				AssertEquals("Box11LabelPart2Caption Test", "country/region.", wrapper.Box11LabelPart2Caption);
				AssertEquals("Box18LabelCaption Test", "18 Identity and nationality of means of transport at departure/on arrival", wrapper.Box18LabelCaption);
				AssertEquals("Box27LabelCaption Test", "27 Place of loading/unloading", wrapper.Box27LabelCaption);
				AssertEquals("BoxDLetterLabelCaption Test", "D/J", wrapper.BoxDLetterLabelCaption);
				AssertEquals("BoxDLabelCaption Test", "CONTROL BY OFFICE OF DEPARTURE/DESTINATION", wrapper.BoxDLabelCaption);
				AssertEquals("Box43LabelCaption Test", "VM Code", wrapper.Box43LabelCaption);
				AssertEquals("Box31PackagesAndDescriptionOfGoodsCaption Test", "Marks and numeration - Container(s) number(s) – Number and class", wrapper.Box31PackagesAndDescriptionOfGoodsCaption);
				AssertEquals("Box1LabelCaption Test", "1 D E C L A R A T I O N", wrapper.Box1LabelCaption);
				AssertEquals("Box2LabelCaption Test", "2 Consignor/Exporter", wrapper.Box2LabelCaption);
				AssertEquals("Box3LabelCaption Test", "Forms", wrapper.Box3LabelCaption);
				AssertEquals("Box4LabelCaption Test", "Loading lists", wrapper.Box4LabelCaption);
				AssertEquals("Box5LabelCaption Test", "Items", wrapper.Box5LabelCaption);
				AssertEquals("Box6LabelCaption Test", "Total packages", wrapper.Box6LabelCaption);
				AssertEquals("Box7LabelCaption Test", "Reference number", wrapper.Box7LabelCaption);
				AssertEquals("Box8LabelCaption Test", "8 Consignee", wrapper.Box8LabelCaption);
				AssertEquals("Box9LabelCaption Test", "Person responsible for financial settlement", wrapper.Box9LabelCaption);
				AssertEquals("Box12LabelCaption Test", "Value details", wrapper.Box12LabelCaption);
				AssertEquals("Box13LabelCaption Test", "CAP", wrapper.Box13LabelCaption);
				AssertEquals("Box14LabelCaption Test", "Declarant/Representative", wrapper.Box14LabelCaption);
				AssertEquals("Box15LabelCaption Test", "Country/Region of dispatch/export", wrapper.Box15LabelCaption);
				AssertEquals("Box15CodeLabelCaption Test", "C disp./exp. Code", wrapper.Box15CodeLabelCaption);
				AssertEquals("Box16LabelCaption Test", "Country/Region of origin", wrapper.Box16LabelCaption);
				AssertEquals("Box17LabelCaption Test", "Country/Region of destination", wrapper.Box17LabelCaption);
				AssertEquals("Box17CodeLabelCaption Test", "Country/Region destin. Code", wrapper.Box17CodeLabelCaption);
				AssertEquals("Box20LabelCaption Test", "Delivery terms", wrapper.Box20LabelCaption);
				AssertEquals("Box21LabelCaption Test", "Identity and nationality of active means of transport crossing the border", wrapper.Box21LabelCaption);
				AssertEquals("Box22LabelCaption Test", "Currency and total amount invoiced", wrapper.Box22LabelCaption);
				AssertEquals("Box23LabelCaption Test", "Exchange rate", wrapper.Box23LabelCaption);
				AssertEquals("Box24Part1LabelCaption Test", "Nature of", wrapper.Box24Part1LabelCaption);
				AssertEquals("Box24Part2LabelCaption Test", "transaction", wrapper.Box24Part2LabelCaption);
				AssertEquals("Box25Part1LabelCaption Test", "Mode of Transport", wrapper.Box25Part1LabelCaption);
				AssertEquals("Box25Part2LabelCaption Test", "at the border", wrapper.Box25Part2LabelCaption);
				AssertEquals("Box26Part1LabelCaption Test", "Inland mode", wrapper.Box26Part1LabelCaption);
				AssertEquals("Box26Part2LabelCaption Test", "of transport", wrapper.Box26Part2LabelCaption);
				AssertEquals("Box28LabelCaption Test", "Financial and banking data", wrapper.Box28LabelCaption);
				AssertEquals("Box30LabelCaption Test", "Location of goods", wrapper.Box30LabelCaption);
				AssertEquals("Box31LabelCaption Test", "Packages and description of goods", wrapper.Box31LabelCaption);
				AssertEquals("Box32LabelCaption Test", "Item", wrapper.Box32LabelCaption);
				AssertEquals("Box33LabelCaption Test", "Commodity Code", wrapper.Box33LabelCaption);
				AssertEquals("Box34LabelCaption Test", "Country/Region origin Code", wrapper.Box34LabelCaption);
				AssertEquals("Box35LabelCaption Test", "Gross Mass (kg)", wrapper.Box35LabelCaption);
				AssertEquals("Box36LabelCaption Test", "Preference", wrapper.Box36LabelCaption);
				AssertEquals("Box37LabelCaption Test", "P R O C E D U R E", wrapper.Box37LabelCaption);
				AssertEquals("Box38LabelCaption Test", "Net Mass (kg)", wrapper.Box38LabelCaption);
				AssertEquals("Box39LabelCaption Test", "Quota", wrapper.Box39LabelCaption);
				AssertEquals("Box40LabelCaption Test", "Summary declaration/Previous document", wrapper.Box40LabelCaption);
				AssertEquals("Box41LabelCaption Test", "Supplementary Units", wrapper.Box41LabelCaption);
				AssertEquals("Box42LabelCaption Test", "Item price", wrapper.Box42LabelCaption);
				AssertEquals("Box44LabelCaption Test", "Additional information/ Documents produced/ Certificates and authorizations", wrapper.Box44LabelCaption);
				AssertEquals("AICodeLabelCaption Test", "A.I.Code", wrapper.AICodeLabelCaption);
				AssertEquals("Box45LabelCaption Test", "Adjustment", wrapper.Box45LabelCaption);
				AssertEquals("Box46LabelCaption Test", "Statistical value", wrapper.Box46LabelCaption);
				AssertEquals("Box47LabelCaption Test", "Calculation of taxes", wrapper.Box47LabelCaption);
				AssertEquals("TaxBaseLabelCaption Test", "Tax base", wrapper.TaxBaseLabelCaption);
				AssertEquals("TaxRateLabelCaption Test", "Rate", wrapper.TaxRateLabelCaption);
				AssertEquals("TaxAmountLabelCaption Test", "Amount", wrapper.TaxAmountLabelCaption);
				AssertEquals("Box48LabelCaption Test", "Deferred Payment", wrapper.Box48LabelCaption);
				AssertEquals("Box49LabelCaption Test", "Identification of warehouse", wrapper.Box49LabelCaption);
				AssertEquals("BoxBLabelCaption Test", "ACCOUNTING DETAILS", wrapper.BoxBLabelCaption);
				AssertEquals("Box50LabelCaption Test", "Principal", wrapper.Box50LabelCaption);
				AssertEquals("Box50SignatureLabelCaption Test", "Signature:", wrapper.Box50SignatureLabelCaption);
				AssertEquals("Box50RepresentedLabelCaption Test", "represented by", wrapper.Box50RepresentedLabelCaption);
				AssertEquals("Box50PlaceDateLabelCaption Test", "Place and date:", wrapper.Box50PlaceDateLabelCaption);
				AssertEquals("BoxCLabelCaption Test", "OFFICE OF DEPARTURE", wrapper.BoxCLabelCaption);
				AssertEquals("Box51LabelCaption Test", "Intended offices of transit (and country/region)", wrapper.Box51LabelCaption);
				AssertEquals("Box52Part1LabelCaption Test", "Guarantee", wrapper.Box52Part1LabelCaption);
				AssertEquals("Box52Part2LabelCaption Test", "not valid for", wrapper.Box52Part2LabelCaption);
				AssertEquals("Box52Part3LabelCaption Test", "Code", wrapper.Box52Part3LabelCaption);
				AssertEquals("Box53LabelCaption Test", "Office of destination (and country/region)", wrapper.Box53LabelCaption);
				AssertEquals("Box54Part1LabelCaption Test", "Place and date:", wrapper.Box54Part1LabelCaption);
				AssertEquals("Box54Part2LabelCaption Test", "Signature and name of declarant/representative", wrapper.Box54Part2LabelCaption);
				AssertEquals("Box47BisTotalFirstItemLabelCaption Test", "Total first item:", wrapper.Box47BisTotalFirstItemLabelCaption);
				AssertEquals("Box47BisTotalSecondItemLabelCaption Test", "Total second item:", wrapper.Box47BisTotalSecondItemLabelCaption);
				AssertEquals("Box47BisTotalThirdItemLabelCaption Test", "Total third item:", wrapper.Box47BisTotalThirdItemLabelCaption);
				AssertEquals("Box47BisSummaryLabelCaption Test", "Summary", wrapper.Box47BisSummaryLabelCaption);
				AssertEquals("Box47BisGTLabelCaption Test", "GT", wrapper.Box47BisGTLabelCaption);
			});
		}

		public virtual void TestBox10LabelPart1Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Box10LabelPart1Caption Test", "Cty.1st.dest/", wrapper.Box10LabelPart1Caption);
		}

		public virtual void TestTitleEUCommunityLabelCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("TitleEUCommunityLabelCaption Test", "EUROPEAN COMMUNITY", wrapper.TitleEUCommunityLabelCaption);
		}

		public virtual void TestBox31PackagesAndDescriptionOfGoodsCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Box31PackagesAndDescriptionOfGoodsCaption Test", "Marks and numeration - Container(s) number(s) – Number and class", wrapper.Box31PackagesAndDescriptionOfGoodsCaption);
		}

		public void TestBox17CountryOfDestinationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Spain;
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Test Box17 Country Code data", "ES", wrapper.Box17CountryOfDestinationCode);
		}

		public virtual void TestShowBox18LabelText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("Print Box18 Label when Export", true, wrapper.ShowBox18LabelText);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("Print Box18 Label when Import", false, wrapper.ShowBox18LabelText);
			});
		}

		public void TestShowBoxDLabelsText()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Print BoxD Labels", true, wrapper.ShowBoxDLabelsText);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => DocSADH.New(entryHeader: null, Factory));
			AssertExceptionThrown<ArgumentNullException>(() => DocSADH.New(entryHeader: Factory.New<CusEntryHeader>(), Factory));
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				DocSADH.New(declaration.CustomsEntryHeaders.AddNew(), Factory);
			});
		}

		public virtual void TestBox12ValueDetails()
		{
			var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Test Box12 data", "0.0", wrapper.Box12ValueDetails);
		}

		public virtual void TestBox15CountryOfOrigin()
		{
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "FRCED"));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = uNLOCO.RL_Code;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Test Box15CountryOfOrigin data", uNLOCO.Country.Description, wrapper.Box15CountryOfOrigin);
		}

		public virtual void TestBox17CountryOfDestination()
		{
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "FRCED"));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = uNLOCO.RL_Code;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Test Box17CountryOfDestination data", uNLOCO.Country.Description, wrapper.Box17CountryOfDestination);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return DocSADH.New(entryHeader, Factory);
		}

		public virtual void TestBox6TotalNoOfPacks()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();

				var documentWrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals(nameof(DocSADH.Box6TotalNoOfPacks), "0", documentWrapper.Box6TotalNoOfPacks);

				var billPackingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();

				var package1 = declaration.Packages.AddNew();
				package1.CW_CR_HouseContainer = billPackingGroup.PK;
				package1.CW_PackQty = 100;
				package1.CW_PackType = "CT";
				package1.CW_MarksAndNos = "M&N1";
				var package2 = declaration.Packages.AddNew();
				package2.CW_CR_HouseContainer = billPackingGroup.PK;
				package2.CW_PackQty = 200;
				package2.CW_PackType = "PK";
				package2.CW_MarksAndNos = "M&N2";

				var invoiceHeader1 = declaration.Invoices.AddNew();
				var invoiceHeader2 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
				var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();

				var packageCtInvoiceLine1 = invoiceLine1.PackagesPivot.AddNew();
				packageCtInvoiceLine1.CHC_CW = package1.PK;
				packageCtInvoiceLine1.CHC_NumberOfPacks = 99;
				var packagePkInvoiceLine1 = invoiceLine2.PackagesPivot.AddNew();
				packagePkInvoiceLine1.CHC_CW = package2.PK;
				packagePkInvoiceLine1.CHC_NumberOfPacks = 50;

				var entryLine = entryHeader.MergedLines.AddNew();

				invoiceLine1.JI_CL = entryLine.PK;
				invoiceLine2.JI_CL = entryLine.PK;

				entryHeader.ResetInvoiceHeadersAndLines();
				documentWrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals(nameof(DocSADH.Box6TotalNoOfPacks), "149", documentWrapper.Box6TotalNoOfPacks);
			}
		}

		protected virtual DocSADH GetNewDocumentWrapper(CusEntryHeader entryHeader) => DocSADH.New(entryHeader, Factory);

		protected virtual void SetCustomsOfficeOfExit(JobDeclaration declaration, ZString customsOffice) => declaration.JE_CustomsOffice = customsOffice;

		protected virtual ZString ExpectedCountryOfOrigin => ZString.Empty;

		protected virtual ZString ExpectedEadBarCode => "21IT1S31T0003580T";

		protected virtual ZDateTime ExpectedDOE => ZDateTime.BrettsBirthday;

		protected IDisposable TemporarilySetUCC6Configuration(JobDeclaration declaration, bool value)
		{
			ClearDeclarationConfiguration(declaration);
			return TemporarilySetConfigurationAndReturnMock(declaration.Factory, value,declaration.GetDefaultDataGroupingCode()).disposable;
		}

		void ClearDeclarationConfiguration(JobDeclaration declaration)
		{
			typeof(JobDeclaration).GetField("configuration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(declaration, null);
		}

		(IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilySetConfigurationAndReturnMock(BusinessObjectFactory factory, bool value, string countryOrGrouping)
		{
			var configurationTypeName = nameof(DeclarationConfiguration);
			var configurationMock = new Mock<DeclarationConfiguration>();
			configurationMock.CallBase = true;

			configurationMock.Protected()
				.Setup<ZBool>("IsUCC6Core", ItExpr.IsAny<BusinessObject>())
				.Returns(new ZBool(value));

			countryOrGrouping = countryOrGrouping ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			factory.ClearCachedValue<DeclarationConfiguration>($"{configurationTypeName}_{countryOrGrouping}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(configurationMock.Object);

			var configuration = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			return (ObjectFactory.Substitute(configurationTypeName, configuration), configurationMock);
		}
	}
}
