using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class K84DailyReportDocumentWrapperTest : K84ReportDocumentWrapperTestCase
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new K84DailyReportDocumentWrapper(message);

			var supporter = wrapper as DocumentEngineIntegration.ISourceIdentifierProvider;
			AssertNotNull("CADImportDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", message.PK, supporter?.SourceIdentifier);
		}

		#endregion

		#region Interchange String

		const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+101231:0605+258++++++1'
UNG+CUSDEC+NOTICE+U10207V1+101231:0605+258+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
DTM+137:20110408:102'
RFF+ABP:10207'
UNS+D'
DMS+K10'
DTM+130:20110411:102'
DTM+353:20110408:102'
NAD+VC+0497'
LIN+++000001170'
MOA+155:51738'
MOA+105:100'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501223'
MOA+201:200'
MOA+128:1501423'
LIN+++000001147'
MOA+1:13000'
MOA+161:13000'
MOA+128:13000'
LIN+++35'
MOA+155:51738'
MOA+105:200'
MOA+4:1000000'
MOA+1:462385'
MOA+161:1514323'
MOA+201:300'
MOA+128:1514623'
LIN+++000001181'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++35'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001192'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001238'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001227'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
DMS+K36'
MOA+155:155214'
MOA+4:3000000'
MOA+1:1348155'
MOA+161:4503369'
MOA+128:4503369'
DMS+K40'
DTM+130:20110429:102'
MOA+155:258690'
MOA+105:100'
MOA+4:5000000'
MOA+1:2259925'
MOA+161:7518715'
MOA+201:300'
MOA+128:7519015'
UNS+S'
UNT+70+1'
UNE+1+258'
UNZ+1+258'";

		#endregion

		public override void TestProperties()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER1";
			var importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_IsImporterDirectPayment = true;
			CreatedDeclaration(Factory, importer, "B00001111", "10207000001170");
			CreatedDeclaration(Factory, importer, "B00001112", "10207000001147");

			CreatedDeclaration(Factory, null, "B00001113", "10207000001181", JobMessageTypeList.Codes.LowValueShipments);

			importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER3";
			importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_IsGSTDirectPayment = true;

			CreatedDeclaration(Factory, importer, "B00001114", "10207000001192");
			CreatedDeclaration(Factory, importer, "B00001115", "10207000001238");
			CreatedDeclaration(Factory, importer, "B00001116", "10207000001227", ZDateTime.UtcNow);
			CreatedDeclaration(Factory, importer, "B00001117", "10207000001227", ZDateTime.UtcNow.AddHours(1));

			var tran8Dec = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001117"));
			AssertNotNull(tran8Dec);
			var entryHeader = tran8Dec.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var sentMessage = Factory.New<B3Message>();
			sentMessage.EM_MessageText = @"UNH+" + EDIMessage.MessageNumberPlaceHolder + "+CUSDEC:S:99B:UN'BGM+:::AB+930+9'CST++I'LOC+41+497'LOC+11+423'LOC+18+12345'RFF+TN:1227'";
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_To = "TO";
			interchange.EI_From = "FROM";
			interchange.EI_InterchangeNum = "156";
			sentMessage.EM_EI = interchange.PK;
			entryHeader.Messages.Add(sentMessage);
			var responseMessage = Factory.New<B3Message>();
			responseMessage.EM_MessageText = @"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I99'RFF+ABO:1227'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageSubType = EntryStatusList.Codes.Clear;
			responseMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			entryHeader.Messages.Add(responseMessage);
			Factory.Save();

			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new K84DailyReportDocumentWrapper(message);
			AssertEquals("CurrentDate", new ZDateTime(2011, 4, 8), wrapper.CurrentDate);
			AssertEquals("AccountSecurityNumber", "10207", wrapper.AccountSecurityNumber);

			AssertEquals("PreviousDaysAccountings.Count", 1, wrapper.PreviousDaysAccountings.Count);
			var accounting = wrapper.PreviousDaysAccountings[0];
			AssertEquals("StatementDate", new ZDateTime(2011, 4, 11), accounting.StatementDate);
			AssertEquals("AccountingDate", new ZDateTime(2011, 4, 8), accounting.AccountingDate);
			AssertEquals("AccountingOfficeNumber", "0497", accounting.AccountingOffice);

			AssertEquals("Transactions.Count", 8, accounting.Transactions.Count);

			AssertEquals("TransactionNumber 1", "000001170", accounting.Transactions[0].TransactionNumber);
			AssertAmounts(accounting.Transactions[0].Amounts, 517.38m, 1, 10000, 4493.85m, 15012.23m, 2, 15014.23m);
			AssertAmounts(accounting.Transactions[0].K35Amounts, 0, 0, 0, 0, 0, 0, 0m);
			AssertEquals("IMPORTER1", accounting.Transactions[0].ImporterCode);
			Assert(accounting.Transactions[0].IsImporterSecuritySetOnOrganization);
			Assert(!accounting.Transactions[0].IsGSTDirectSetOnOrganization);
			AssertEquals("B00001111", accounting.Transactions[0].Declaration.JE_DeclarationReference);
			AssertEquals("TransactionNumber 2", "000001147", accounting.Transactions[1].TransactionNumber);
			AssertAmounts(accounting.Transactions[1].Amounts, 0, 0, 0, 130, 130, 0, 130);
			AssertEquals("IMPORTER1", accounting.Transactions[1].ImporterCode);
			AssertEquals("B00001112", accounting.Transactions[1].Declaration.JE_DeclarationReference);
			AssertEquals("TransactionNumber 3", "35", accounting.Transactions[2].TransactionNumber);
			AssertAmounts(accounting.Transactions[2].Amounts, 0, 0, 0, 0, 0, 0, 0m);
			AssertAmounts(accounting.Transactions[2].K35Amounts, 517.38m, 2, 10000, 4623.85m, 15143.23m, 3, 15146.23m);
			AssertEquals("IMPORTER1", accounting.Transactions[2].ImporterCode);

			AssertEquals("TransactionNumber 4", "000001181", accounting.Transactions[3].TransactionNumber);
			AssertAmounts(accounting.Transactions[3].Amounts, 517.38m, 0, 10000, 4493.85m, 15011.23m, 0, 15011.23m);
			AssertEquals("VARIOUS", accounting.Transactions[3].ImporterCode);
			AssertEquals("B00001113", accounting.Transactions[3].Declaration.JE_DeclarationReference);
			AssertEquals("TransactionNumber 5", "35", accounting.Transactions[4].TransactionNumber);
			AssertAmounts(accounting.Transactions[4].K35Amounts, 517.38m, 0, 10000, 4493.85m, 15011.23m, 0, 15011.23m);
			AssertEquals("VARIOUS", accounting.Transactions[4].ImporterCode);

			AssertEquals("TransactionNumber 6", "000001192", accounting.Transactions[5].TransactionNumber);
			AssertAmounts(accounting.Transactions[5].Amounts, 517.38m, 0, 10000, 4493.85m, 15011.23m, 0, 15011.23m);
			AssertEquals("IMPORTER3", accounting.Transactions[5].ImporterCode);
			Assert(!accounting.Transactions[5].IsImporterSecuritySetOnOrganization);
			Assert(accounting.Transactions[5].IsGSTDirectSetOnOrganization);
			AssertEquals("B00001114", accounting.Transactions[5].Declaration.JE_DeclarationReference);
			AssertEquals("TransactionNumber 7", "000001238", accounting.Transactions[6].TransactionNumber);
			AssertAmounts(accounting.Transactions[6].Amounts, 517.38m, 0, 10000, 4493.85m, 15011.23m, 0, 15011.23m);
			AssertEquals("IMPORTER3", accounting.Transactions[6].ImporterCode);
			AssertEquals("B00001115", accounting.Transactions[6].Declaration.JE_DeclarationReference);
			Assert("Importer did not pay", !accounting.Transactions[6].IsImporterSecuritySetOnLastAcceptedB3Message);
			AssertEquals("TransactionNumber 8", "000001227", accounting.Transactions[7].TransactionNumber);
			AssertAmounts(accounting.Transactions[7].Amounts, 517.38m, 0, 10000, 4493.85m, 15011.23m, 0, 15011.23m);
			AssertEquals("IMPORTER3", accounting.Transactions[7].ImporterCode);
			AssertEquals("B00001117", accounting.Transactions[7].Declaration.JE_DeclarationReference);
			Assert("Importer did pay", accounting.Transactions[7].IsImporterSecuritySetOnLastAcceptedB3Message);

			AssertEquals("DailyAccountingTotals.Count", 2, wrapper.DailyAccountingTotals.Count);
			var total = wrapper.DailyAccountingTotals[0];
			AssertEquals("Broker Total", total.TotalsDescription);
			AssertAmounts(total.Amounts, 1552.14m, 0, 30000, 13481.55m, 45033.69m, 0, 45033.69m);

			total = wrapper.DailyAccountingTotals[1];
			AssertEquals("Report Grand Total", total.TotalsDescription);
			AssertEquals("StatementDate", new ZDateTime(2011, 04, 29), total.StatementDate);
			AssertAmounts(total.Amounts, 2586.90m, 1, 50000, 22599.25m, 75187.15m, 3, 75190.15m);
		}

		internal static void CreatedDeclaration(BusinessObjectFactory factory, OrgHeader importer, ZString jobNumber, ZString transactionNo, string messageType = "")
		{
			var declaration = factory.New<JobDeclaration>();
			if (!string.IsNullOrEmpty(messageType))
			{
				declaration.JE_MessageType = messageType;
			}

			if (importer != null)
			{
				declaration.JE_OH_Importer = importer.PK;
			}

			declaration.JE_DeclarationReference = jobNumber;
			var transactionNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumber.EntryType.CATransactionNumber, Constants.CountryCodes.Canada);
			transactionNumber.CE_EntryNum = transactionNo;
			declaration.JE_PaymentMethod = Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
		}

		internal static void CreatedDeclaration(BusinessObjectFactory factory, OrgHeader importer, ZString jobNumber, ZString transactionNo, ZDateTime createTime, string messageType = "")
		{
			var declaration = factory.New<JobDeclaration>();
			if (!string.IsNullOrEmpty(messageType))
			{
				declaration.JE_MessageType = messageType;
			}

			if (importer != null)
			{
				declaration.JE_OH_Importer = importer.PK;
			}

			declaration.JE_DeclarationReference = jobNumber;
			var transactionNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumber.EntryType.CATransactionNumber, Constants.CountryCodes.Canada);
			transactionNumber.CE_EntryNum = transactionNo;
			declaration.JE_PaymentMethod = Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
			declaration.JE_SystemCreateTimeUtc = createTime;
		}
	}
}
