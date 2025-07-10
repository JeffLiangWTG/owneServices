using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ExportAmendmentDetailsDecoratorTest : TestCaseWithFactory
	{
		public void TestExportAmendmentDetailsByCusEntryHeaderData()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			entry.CH_EntryReleaseDate = ZDateTime.Today.AddDays(1);

			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var header = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());
			var amendmentDetails = new ExportAmendmentDetails(header, Factory, entry.PK);
			amendmentDetails.Decorate(entry, 0);

			AssertEquals("레디코리아", amendmentDetails.BrokerCompanyName);
			AssertEquals("김환태", amendmentDetails.BrokerCompanyRepresentative);
			AssertEquals("서울특별시 서초구 동광로 41 레디인빌딩", amendmentDetails.SupplierAddress);
			AssertEquals("레디코리아", amendmentDetails.SupplierCompanyName);
			AssertEquals("레디코리1971018", amendmentDetails.SupplierCompanyID);
			AssertEquals(ZDateTime.Today, amendmentDetails.DeclarationDate);
			AssertEquals(ZDateTime.Today.AddDays(1), amendmentDetails.ReleaseDate);
		}
		public void TestExportAmendmentDetailsByCusEntrySnapShotData()
		{
			entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			entry.CusEntryNumber.CE_EntryNum = "6N00220000051X";
			entry.CH_VersionID = 1;
			AssertEquals("This snapshot was created when sending 830.", 1, entry.Snapshots.Count);
			AssertEquals(1u, entry.Snapshots[0].CES_VersionNumber);

			var declaration = entry.Declaration;
			var newBranch = declaration.Company.Branches.AddNew();
			var modifyBroker = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RD2", "레디코리아11");
			var modifyBrokerAddress = modifyBroker.Addresses.AddNew();
			modifyBrokerAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			TestOrgDataSetUpHelper.AddOrgContact(modifyBroker, "박의규", true);
			newBranch.GB_OH_OrgProxy = modifyBroker.PK;
			var modifysupplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RD7", "레디코리아22");
			modifysupplier.OH_IsConsignor = true;
			TestOrgDataSetUpHelper.AddOrgContact(modifysupplier, "김환태", true);
			TestOrgDataSetUpHelper.AddOrgAddress(modifysupplier.MainAddress, "테스트 주소1", "테스트 주소2");
			var supplierCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리0000001" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(modifysupplier, supplierCode);

			declaration.JE_GB = newBranch.PK;
			declaration.JE_OA_SupplierAddress = modifysupplier.MainAddress.PK;

			var amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5AS);
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			new GOVCBR5ASAmendmentSender(amendmentMessageSendingObjectParent.ObjectsToSend, Factory).Send();
			AssertEquals("Snapshot was created.", 2, entry.Snapshots.Count);
			entry.CH_VersionID = 2;
			var amendedItems = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS).AmendedItems;
			var header = new Export5ASHeaderCreator().Create(entry, amendedItems.ToArray());
			var amendmentDetails = new ExportAmendmentDetails(header, Factory, entry.PK);
			amendmentDetails.Decorate(entry, 2);
			AssertionExportAmendmentDetailsData(amendmentDetails, "레디코리아11", "박의규", "테스트 주소1 테스트 주소2", "레디코리아22", "레디코리0000001");

			var newBranch2 = declaration.Company.Branches.AddNew();
			var modifyBroker2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK_A", "신청인상호");
			var modifyBrokerAddress2 = modifyBroker2.Addresses.AddNew();
			modifyBrokerAddress2.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			TestOrgDataSetUpHelper.AddOrgContact(modifyBroker2, "신청인 대표자", true);
			newBranch2.GB_OH_OrgProxy = modifyBroker2.PK;
			var modifysupplier2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK_B", "수출화주상호");
			modifysupplier2.OH_IsConsignor = true;
			TestOrgDataSetUpHelper.AddOrgContact(modifysupplier2, "수출화주 대표자", true);
			TestOrgDataSetUpHelper.AddOrgAddress(modifysupplier2.MainAddress, "수출화주 주소1", "수출화주 주소2");
			var supplierCode2 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "수출화주 통관고유부호" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(modifysupplier2, supplierCode2);

			declaration.JE_GB = newBranch2.PK;
			declaration.JE_OA_SupplierAddress = modifysupplier2.MainAddress.PK;

			amendmentDetails.Decorate(entry, 2);
			AssertionExportAmendmentDetailsData(amendmentDetails, "레디코리아11", "박의규", "테스트 주소1 테스트 주소2", "레디코리아22", "레디코리0000001");

			amendmentDetails.Decorate(entry, 0);
			AssertionExportAmendmentDetailsData(amendmentDetails, "신청인상호", "신청인 대표자", "수출화주 주소1 수출화주 주소2", "수출화주상호", "수출화주 통관고유부호");
		}

		void AssertionExportAmendmentDetailsData(ExportAmendmentDetails amendmentDetails, ZString brokerComanyName, ZString brokerCompanyRepresentative, ZString supplierAddress, ZString supplierCompanyName, ZString supplierCompanyID)
		{
			AssertEquals(brokerComanyName, amendmentDetails.BrokerCompanyName);
			AssertEquals(brokerCompanyRepresentative, amendmentDetails.BrokerCompanyRepresentative);
			AssertEquals(supplierAddress, amendmentDetails.SupplierAddress);
			AssertEquals(supplierCompanyName, amendmentDetails.SupplierCompanyName);
			AssertEquals(supplierCompanyID, amendmentDetails.SupplierCompanyID);
		}

		public void TestTotalCustomsValueWhenStatusIsAccept()
		{
			var fileReader = new TestFileReader(typeof(ExportAmendmentDetailsTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming", "GOVCBR5DT_Status_ANT.xml");
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = "5DT";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_MessageText = messageText;
			var message5AS = SetSnapShotData5AS();
			message5AS.EM_ApplicationReference = "2";

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			message5AS.Reload();

			var exportAmendmentDetailsCollection = new ExportAmendmentDetailsCollection(entry);
			AssertEquals(1, exportAmendmentDetailsCollection.Count);

			var amendmentDetails = exportAmendmentDetailsCollection[0];
			amendmentDetails.Decorate(entry, 2);
			AssertEquals(34587293m, amendmentDetails.BeforeTotalCustomsValue);
			AssertEquals(8600000m, amendmentDetails.AfterTotalCustomsValue);

			AssertEquals("Round(34587293 / 1100.5 = 31,428.70785097683)", 31429m, amendmentDetails.BeforeTotalCustomsValueUSD);
			AssertEquals("Round(8600000 / 1100.5 = 7,814.629704679691)", 7815m, amendmentDetails.AfterTotalCustomsValueUSD);

			AssertEquals("34,587,293($31,429)", amendmentDetails.FormattedBeforeTotalCustomsValue);
			AssertEquals("8,600,000($7,815)", amendmentDetails.FormattedAfterTotalCustomsValue);
		}

		public void TestTotalCustomsValueWhenStatsusIsReject()
		{
			var fileReader = new TestFileReader(typeof(ExportAmendmentDetailsTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming", "GOVCBR5DT_Status_DMS.xml");
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = "5DT";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_MessageText = messageText;
			var message5AS = SetSnapShotData5AS();
			message5AS.EM_ApplicationReference = "2";

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			message5AS.Reload();

			var snapShot = entry.Snapshots.Cast<CusEntrySnapshot>().FirstOrDefault(x => x.CES_VersionNumber == ZShort.Parse(message5AS.EM_ApplicationReference));
			snapShot.CES_Status = "DEL";
			Factory.Save();

			entry = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			var exportAmendmentDetailsCollection = new ExportAmendmentDetailsCollection(entry);
			AssertEquals(1, exportAmendmentDetailsCollection.Count);

			var amendmentDetails = exportAmendmentDetailsCollection[0];
			amendmentDetails.Decorate(entry, 2);
			AssertEquals(34587293m, amendmentDetails.BeforeTotalCustomsValue);
			AssertEquals(0m, amendmentDetails.AfterTotalCustomsValue);

			AssertEquals("Round(34587293 / 1100.5 = 31,428.70785097683)", 31429m, amendmentDetails.BeforeTotalCustomsValueUSD);
			AssertEquals(0m, amendmentDetails.AfterTotalCustomsValueUSD);
		}

		EDIMessage SetSnapShotData5AS()
		{
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today, ZDateTime.Today, 1100.5m, usdCurrency);

			entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			entry.CusEntryNumber.CE_EntryNum = "6N00220000051X";
			entry.CH_VersionID = 1;
			entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 1).Delete();
			entry.ResetIsCustomsValueCalculated();
			var declaration = Factory.Load<JobDeclaration>(entry.Declaration.PK);
			var amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, "5AS");
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			new GOVCBR5ASAmendmentSender(amendmentMessageSendingObjectParent.ObjectsToSend, Factory).Send();
			Factory.Save();

			return entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == "5AS");
		}

		void SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;
		}

		CusEntryHeader entry;
	}
}
