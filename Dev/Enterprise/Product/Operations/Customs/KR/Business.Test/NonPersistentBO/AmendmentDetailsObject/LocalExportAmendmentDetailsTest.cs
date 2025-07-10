using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportAmendmentDetails))]
	sealed class LocalExportAmendmentDetailsTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLocalExportAmendmentDetailsByAmendMessageData()
		{
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = ElectronicDocumentTypeList.Codes._5DQ;
			entry1.CH_VersionID = 1;
			var message5DS = entry1.Messages.AddNew();
			message5DS.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5DS.EM_MessageType = ElectronicDocumentTypeList.Codes._5DS;
			message5DS.EM_SystemCreateUser = staff.GS_Code;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DS.xml"));
			message5DS.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DS.EM_MessageSubType = "1";
			message5DS.EM_ApplicationReference = "2";
			Factory.Save();

			var amendmentDetails = entry1.LocalExportAmendmentDetailsCollection[0];
			AssertEquals("12345-67-890123", amendmentDetails.FormattedDeclarationNumber);
			AssertEquals(new ZDateTime("2022-02-19"), amendmentDetails.SubmissionDate);
			AssertEquals("1", amendmentDetails.AmendType);
			AssertEquals("정정", amendmentDetails.AmendTypeDescription);
			AssertEquals("1", amendmentDetails.ReasonCode);
			AssertEquals("기재오류", amendmentDetails.ReasonCodeDescription);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry2.CH_VersionID = 1;
			var message5DR = entry2.Messages.AddNew();
			message5DR.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5DR.EM_MessageType = ElectronicDocumentTypeList.Codes._5DR;
			message5DR.EM_SystemCreateUser = staff.GS_Code;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DR.xml"));
			message5DR.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR.EM_MessageSubType = "2";
			message5DR.EM_ApplicationReference = "2";
			Factory.Save();

			amendmentDetails = entry2.LocalExportAmendmentDetailsCollection[0];
			AssertEquals("01234-56-789012", amendmentDetails.FormattedDeclarationNumber);
			AssertEquals(new ZDateTime("2022-02-09"), amendmentDetails.SubmissionDate);
			AssertEquals("2", amendmentDetails.AmendType);
			AssertEquals("취하", amendmentDetails.AmendTypeDescription);
			AssertEquals("2", amendmentDetails.ReasonCode);
			AssertEquals("계약취소(반품)", amendmentDetails.ReasonCodeDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLocalExportAmendmentDetailsWhenReceiveRR3Message()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var entry_ANT = declaration.CustomsEntryHeaders.AddNew();
			entry_ANT.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry_ANT.EntryNumber = "4271120006680";
			entry_ANT.CusEntryNumber.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entry_ANT.CH_VersionID = 1;

			var message5DR_ANT = entry_ANT.Messages.AddNew();
			message5DR_ANT.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5DR_ANT.EM_MessageType = ElectronicDocumentTypeList.Codes._5DR;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DR.xml"));
			message5DR_ANT.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR_ANT.EM_SystemCreateUser = staff.GS_Code;
			message5DR_ANT.EM_ApplicationReference = "2";
			Factory.Save();

			var incomingMessageR38_ANT = entry_ANT.Messages.AddNew();
			incomingMessageR38_ANT.EM_MessageType = ElectronicDocumentTypeList.Codes._R38;
			incomingMessageR38_ANT.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageR38_ANT.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessageR38_ANT.EM_ApplicationReference = message5DR_ANT.EM_MessageNum;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Incoming\GOVCBRR38_5DR.xml"));
			incomingMessageR38_ANT.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));

			var incomingMessageRR3_ANT = entry_ANT.Messages.AddNew();
			incomingMessageRR3_ANT.EM_MessageType = ElectronicDocumentTypeList.Codes._RR3;
			incomingMessageRR3_ANT.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageRR3_ANT.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessageRR3_ANT.EM_ApplicationReference = message5DR_ANT.EM_MessageNum;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Incoming\GOVCBRRR3_5DR_StatusIsANTOrCCL.xml"));
			incomingMessageRR3_ANT.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR_ANT.EM_MessageSubType = MessageSubTypeLocalExport.Amendment;
			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry_ANT.Reload();
			incomingMessageR38_ANT.Reload();
			incomingMessageRR3_ANT.Reload();

			var amendmentDetailsANT = entry_ANT.LocalExportAmendmentDetailsCollection[0];
			AssertEquals(new ZDateTime("2020-11-17 16:05:40"), amendmentDetailsANT.AmendAuthorisationDate);
			AssertEquals("서울세관", amendmentDetailsANT.CustomerOfficer);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, amendmentDetailsANT.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.AmendmentAccepted, amendmentDetailsANT.MessageStatusDescription);
			AssertEquals(LocalExportProcessResultTypeCodeList.Codes.D1, amendmentDetailsANT.NoticeType);
			AssertEquals(LocalExportProcessResultTypeCodeList.Descriptions.D1, amendmentDetailsANT.NoticeTypeDescription);

			var entry_DMS = declaration.CustomsEntryHeaders.AddNew();
			entry_DMS.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry_DMS.EntryNumber = "4271120006681";
			entry_DMS.CusEntryNumber.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entry_DMS.CH_VersionID = 1;

			var message5DR_DMS = entry_DMS.Messages.AddNew();
			message5DR_DMS.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5DR_DMS.EM_MessageType = ElectronicDocumentTypeList.Codes._5DR;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DR.xml"));
			message5DR_DMS.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR_DMS.EM_SystemCreateUser = staff.GS_Code;
			message5DR_DMS.EM_ApplicationReference = "2";
			Factory.Save();

			var incomingMessageR38_DMS = entry_DMS.Messages.AddNew();
			incomingMessageR38_DMS.EM_MessageType = ElectronicDocumentTypeList.Codes._R38;
			incomingMessageR38_DMS.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageR38_DMS.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessageR38_DMS.EM_ApplicationReference = message5DR_DMS.EM_MessageNum;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Incoming\GOVCBRR38_5DR.xml"));
			incomingMessageR38_DMS.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));

			var incomingMessageRR3_DMS = entry_DMS.Messages.AddNew();
			incomingMessageRR3_DMS.EM_MessageType = ElectronicDocumentTypeList.Codes._RR3;
			incomingMessageRR3_DMS.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessageRR3_DMS.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessageRR3_DMS.EM_ApplicationReference = message5DR_DMS.EM_MessageNum;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Incoming\GOVCBRRR3_5DR_StatusIsDMS.xml"));
			incomingMessageRR3_DMS.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR_DMS.EM_MessageSubType = MessageSubTypeLocalExport.Amendment;
			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry_DMS.Reload();
			incomingMessageR38_DMS.Reload();
			incomingMessageRR3_DMS.Reload();

			var amendmentDetailsDMS = entry_DMS.LocalExportAmendmentDetailsCollection[0];
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, amendmentDetailsDMS.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.AmendmentAccepted, amendmentDetailsDMS.MessageStatusDescription);
			AssertEquals(LocalExportProcessResultTypeCodeList.Codes.D3, amendmentDetailsDMS.NoticeType);
			AssertEquals(LocalExportProcessResultTypeCodeList.Descriptions.D3, amendmentDetailsDMS.NoticeTypeDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLocalExportAmendmentDetailsWhenReceiveR38Message()
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.EntryNumber = "6N00220000052X";
			entry.CusEntryNumber.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entry.CH_VersionID = 1;

			var message5DR = entry.Messages.AddNew();
			message5DR.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5DR.EM_MessageType = ElectronicDocumentTypeList.Codes._5DR;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DR.xml"));
			message5DR.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR.EM_SystemCreateUser = staff.GS_Code;
			message5DR.EM_ApplicationReference = "2";
			Factory.Save();

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R38;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_ApplicationReference = message5DR.EM_MessageNum;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Incoming\GOVCBRR38_5DR.xml"));
			incomingMessage.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR.EM_MessageSubType = MessageSubTypeLocalExport.Cancellation;

			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry.Reload();
			incomingMessage.Reload();
			var amendmentDetails = entry.LocalExportAmendmentDetailsCollection[0];
			AssertEquals("040-10-20-004861-1", amendmentDetails.FormattedCustomsReferenceNumber);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, amendmentDetails.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.CancellationAccepted, amendmentDetails.MessageStatusDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLocalExportAmendmentDetailsWhenReceiveR20Message()
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.EntryNumber = "6N00220000052X";
			entry.CusEntryNumber.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entry.CH_VersionID = 1;

			var message5DR = entry.Messages.AddNew();
			message5DR.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5DR.EM_MessageType = ElectronicDocumentTypeList.Codes._5DR;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DR.xml"));
			message5DR.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR.EM_SystemCreateUser = staff.GS_Code;
			message5DR.EM_ApplicationReference = "2";
			Factory.Save();

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_ApplicationReference = message5DR.EM_MessageNum;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Common\Incoming\GOVCBRR20_5DR.xml"));
			incomingMessage.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR.EM_MessageSubType = MessageSubTypeLocalExport.Cancellation;

			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry.Reload();
			incomingMessage.Reload();
			var amendmentDetails = entry.LocalExportAmendmentDetailsCollection[0];
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationRejected, amendmentDetails.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.CancellationRejected, amendmentDetails.MessageStatusDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void Test5DRAmendedItems_WithEDIMessage()
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.EntryNumber = "4271120006680";
			entry.CusEntryNumber.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entry.CH_VersionID = 1;

			var message5DR = entry.Messages.AddNew();
			message5DR.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5DR.EM_MessageType = ElectronicDocumentTypeList.Codes._5DR;
			message5DR.EM_ApplicationReference = "2";
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DR_Test.xml"));
			message5DR.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DR.EM_SystemCreateUser = staff.GS_Code;
			Factory.Save();

			var amendmentDetails = entry.LocalExportAmendmentDetailsCollection[0];
			var headerItems = amendmentDetails.HeaderAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.DataItemNo).ToArray();
			AssertEquals(1, headerItems.Length);
			AssertAmendedItems(headerItems[0], 0, "11A", "반입(적재)장소", "01023010-보세구역이름", "01023010-새 경남창고", "3", "U", "Update");

			var lineItems = amendmentDetails.LineAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.ItemSequenceNumber).ThenBy(item => item.DataItemNo).ToArray();
			AssertEquals(2, lineItems.Length);
			AssertAmendedItems(lineItems[0], 1, "21", "품행 및 규격", "STAINLESS STEEL", "Change STAINLESS STEEL", "3", "U", "Update");
			AssertAmendedItems(lineItems[1], 2, "11B", "작업선", "CY_Code Test1", "Change SAMARIA TEST", "3", "U", "Update");
		}
		public void Test5DRAmendedItems_WithoutEDIMessage()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			var localExportEntryHeader = new LocalExport5DPEntryHeaderCreator().Create(entry);
			CreateSnapshot(entry, localExportEntryHeader, ElectronicDocumentTypeList.Codes._5DP);

			var declaration = entry.Declaration;
			declaration.JE_LocationOfGoods = "";
			declaration.JE_SubLocationOfGoods = "새 경남창고";
			declaration.JE_ExportGoodsType = "2";
			declaration.Invoices[0].JZ_Weight = 90;
			declaration.Invoices[0].JZ_NoOfPacks = 10;
			declaration.Invoices[0].JZ_DRWApplicantType = "2";

			var entryline = entry.MergedLines[0];
			entryline.CL_AdValoremTariff = "0987654321";
			entryline.CL_Description = "Change STAINLESS STEEL";
			entryline.CL_CustomsValue = 500m;

			entryline.InvoiceLines[1].Delete();
			var invoiceline = (JobComInvoiceLine)entryline.InvoiceLines[0];
			invoiceline.JI_InvoiceQuantity = 8888m;
			invoiceline.JI_InvoiceUQ = "G";
			invoiceline.JI_NoOfPacks = 88;
			invoiceline.JI_PackType = "BK";
			invoiceline.JI_NetWeight = 9999m;
			invoiceline.JI_NetWeightUQ = "G";
			invoiceline.JI_LinePrice = 9999m;
			invoiceline.JI_InboundDate = new ZDateTime(2013, 01, 02);
			invoiceline.JI_PreviousEntryNumber = "010151234567002000";
			invoiceline.JI_OriginalStateDocType = "02";
			invoiceline.JI_SerialNumber = "010101010";
			invoiceline.JI_SequenceNumber = 1;
			invoiceline.JI_Ingredient = "ABCD9589376";
			invoiceline.SupportingDocumentReferenceNumber = "L172770912346";
			invoiceline.SupportingDocumentCode = "2";
			entry.MergedLines[1].Delete();

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DR).AmendedItems;
			var amendHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			var amendmentDetails = new LocalExportAmendmentDetails(amendHeader, Factory);

			var headerItems = amendmentDetails.HeaderAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.DataItemNo).ToArray();
			AssertEquals(5, headerItems.Length);
			AssertAmendedItems(headerItems[0], 0, "11A", "반입(적재)장소", "01023010-보세구역이름", "01023010-새 경남창고", "3", "U", "Update");
			AssertAmendedItems(headerItems[1], 0, "14", "구분부호", "1", "2", "3", "U", "Update");
			AssertAmendedItems(headerItems[2], 0, "15", "환급신청인", "1", "2", "3", "U", "Update");
			AssertAmendedItems(headerItems[3], 0, "16", "총포장갯수", "100", "10", "3", "U", "Update");
			AssertAmendedItems(headerItems[4], 0, "18", "총중량(KG)", "900", "90", "3", "U", "Update");

			var lineItems = amendmentDetails.LineAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.ItemSequenceNumber).ThenBy(item => item.DataItemNo).ToArray();
			AssertEquals(16, lineItems.Length);

			AssertAmendedItems(lineItems[0], 1, "21", "품행 및 규격", "STAINLESS STEEL", "Change STAINLESS STEEL", "3", "U", "Update");
			AssertAmendedItems(lineItems[1], 1, "22", "물품식별번호", "000000000", "010101010", "3", "U", "Update");
			AssertAmendedItems(lineItems[2], 1, "23", "품목번호", "1234567890", "0987654321", "3", "U", "Update");
			AssertAmendedItems(lineItems[3], 1, "24A", "수량", "9999", "8888", "3", "U", "Update");
			AssertAmendedItems(lineItems[4], 1, "24B", "단위", "KG", "G", "3", "U", "Update");
			AssertAmendedItems(lineItems[5], 1, "25", "순중량(KG)", "10000", "9.999", "3", "U", "Update");
			AssertAmendedItems(lineItems[6], 1, "26", "공급금액(FOB)", "1000", "500", "3", "U", "Update");
			AssertAmendedItems(lineItems[7], 1, "27", "포장갯수", "99", "88", "3", "U", "Update");
			AssertAmendedItems(lineItems[8], 1, "28", "포장종류", "VL", "BK", "3", "U", "Update");
			AssertAmendedItems(lineItems[9], 1, "29", "근거서류종류", "1", "2", "3", "U", "Update");
			AssertAmendedItems(lineItems[10], 1, "30", "반입(적재)근거서류번호", "L172770912345", "L172770912346", "3", "U", "Update");
			AssertAmendedItems(lineItems[11], 1, "31", "반입일자", "20130101", "20130102", "3", "U", "Update");
			AssertAmendedItems(lineItems[12], 1, "32A", "원상태근거번호", "010151234567001999", "010151234567002000", "3", "U", "Update");
			AssertAmendedItems(lineItems[13], 1, "32B", "원상태근거서류종류", "01", "02", "3", "U", "Update");
			AssertAmendedItems(lineItems[14], 1, "37", "구매주문서번호", "ABCD9589375", "ABCD9589376", "3", "U", "Update");
			AssertAmendedItems(lineItems[15], 2, "", "", "", "", "2", "D", "Delete");

			var allAmendItems = amendmentDetails.AllAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.ItemSequenceNumber).ThenBy(item => item.DataItemNo).ToArray();
			AssertEquals(21, allAmendItems.Length);

			AssertAmendedItems(allAmendItems[0], 0, "11A", "반입(적재)장소", "01023010-보세구역이름", "01023010-새 경남창고", "3", "U", "Update");
			AssertAmendedItems(allAmendItems[3], 0, "16", "총포장갯수", "100", "10", "3", "U", "Update");
			AssertAmendedItems(allAmendItems[5], 1, "21", "품행 및 규격", "STAINLESS STEEL", "Change STAINLESS STEEL", "3", "U", "Update");
			AssertAmendedItems(allAmendItems[15], 1, "30", "반입(적재)근거서류번호", "L172770912345", "L172770912346", "3", "U", "Update");
			AssertAmendedItems(allAmendItems[20], 2, "", "", "", "", "2", "D", "Delete");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void Test5DSAmendedItems_WithEDIMessage()
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DQ;
			entry.EntryNumber = "3271420001710";
			entry.CusEntryNumber.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entry.CH_VersionID = 1;

			var message5DS = entry.Messages.AddNew();
			message5DS.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5DS.EM_MessageType = ElectronicDocumentTypeList.Codes._5DS;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DS_Test.xml"));
			message5DS.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5DS.EM_SystemCreateUser = staff.GS_Code;
			message5DS.EM_ApplicationReference = "2";
			Factory.Save();

			var amendmentDetails = entry.LocalExportAmendmentDetailsCollection[0];
			var headerItems = amendmentDetails.HeaderAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.DataItemNo).ToArray();
			AssertEquals(1, headerItems.Length);
			AssertAmendedItems(headerItems[0], 0, "16", "총포장갯수", "19", "18", "3", "U", "Update");

			var lineItems = amendmentDetails.LineAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.ItemSequenceNumber).ThenBy(item => item.DataItemNo).ToArray();
			AssertEquals(3, lineItems.Length);
			AssertAmendedItems(lineItems[0], 1, "24A", "수량", "5", "4", "3", "U", "Update");
			AssertAmendedItems(lineItems[1], 1, "26", "공급금액(FOB)", "112432740", "82227990", "3", "U", "Update");
			AssertAmendedItems(lineItems[2], 2, "27", "포장갯수", "5", "4", "3", "U", "Update");
		}
		public void Test5DSAmendedItems_WithoutEDIMessage()
		{
			var vessel = CreateRefVessel("M/V MARIA", "1234567", "M/V MARIA");
			CreateRefVessel("Oerssleff TEST", "ZDNI9", "");
			CreateRefVessel("Change SAMARIA TEST", "9182", "");
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			var newTransportMean = entry.Declaration.TransportMeans.AddNew();
			newTransportMean.CY_Order = 2;
			newTransportMean.CY_Code = "Oerssleff TEST";
			newTransportMean.CY_Data = "111111";
			var localExportEntryHeader = new LocalExport5DQEntryHeaderCreator().Create(entry);
			CreateSnapshot(entry, localExportEntryHeader, ElectronicDocumentTypeList.Codes._5DQ);

			var declaration = entry.Declaration;
			declaration.JE_LocationOfGoods = "";
			declaration.JE_SubLocationOfGoods = "새 반입장소";
			vessel.RV_RadioCallSign = "M/V MARIA2";
			declaration.JE_ExportGoodsType = "2";
			declaration.Invoices[0].JZ_Weight = 90;
			declaration.Invoices[0].JZ_NoOfPacks = 10;
			declaration.Invoices[0].JZ_DRWApplicantType = "2";
			declaration.DeclarationRefs[0].J3_ReferenceNumber = "NEW 20GLKO0080I";
			declaration.JE_NoOfCrew = 30;
			declaration.JE_VoyageDuration = 17;

			var transportMean1 = declaration.TransportMeans[0];
			transportMean1.CY_Order = 1;
			transportMean1.CY_Code = "Change SAMARIA TEST";
			transportMean1.CY_Data = "제주 허12 3456";

			var transportMean2 = declaration.TransportMeans.AddNew();
			transportMean2.CY_Order = 3;
			transportMean2.CY_Code = "Add SAMARIA TEST";

			var entryLine = entry.MergedLines[1];
			entry.MergedLines[0].InvoiceLines[0].Delete();
			entry.MergedLines[0].Delete();

			entryLine.CL_AdValoremTariff = "1234567900";
			entryLine.CL_Description = "Change STAINLESS STEEL";
			entryLine.CL_CustomsValue = 500m;

			entryLine.InvoiceLines[0].Delete();
			var invoiceline = (JobComInvoiceLine)entryLine.InvoiceLines[0];
			invoiceline.SupportingDocumentReferenceNumber = "L172770912350";
			invoiceline.SupportingDocumentCode = "2";
			invoiceline.JI_InboundDate = new ZDateTime(2013, 12, 31);
			invoiceline.JI_PreviousEntryNumber = "010151234567002000";
			invoiceline.JI_OriginalStateDocType = "02";
			invoiceline.JI_SerialNumber = "정정물품식별번호";
			invoiceline.JI_InvoiceUQ = Core.Constants.Weight.Grams;
			invoiceline.JI_PackType = "PL";

			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DS).AmendedItems;
			var amendHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			var amendmentDetails = new LocalExportAmendmentDetails(amendHeader, Factory);

			var headerItems = amendmentDetails.HeaderAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.DataItemNo).ToArray();
			AssertEquals(9, headerItems.Length);
			AssertAmendedItems(headerItems[0], 0, "11A", "반입(적재)장소", "경남창고", "새 반입장소", "3", "U", "Update");
			AssertAmendedItems(headerItems[1], 0, "12", "선박호출부호", "M/V MARIA", "M/V MARIA2", "3", "U", "Update");
			AssertAmendedItems(headerItems[2], 0, "13", "입항보고제출번호(MRN)", "20GLKO0080I", "NEW 20GLKO0080I", "3", "U", "Update");
			AssertAmendedItems(headerItems[3], 0, "14", "구분부호", "1", "2", "3", "U", "Update");
			AssertAmendedItems(headerItems[4], 0, "15", "환급신청인", "1", "2", "3", "U", "Update");
			AssertAmendedItems(headerItems[5], 0, "16", "총포장갯수", "100", "10", "3", "U", "Update");
			AssertAmendedItems(headerItems[6], 0, "18", "총중량(KG)", "900", "90", "3", "U", "Update");
			AssertAmendedItems(headerItems[7], 0, "38", "승무원수", "15", "30", "3", "U", "Update");
			AssertAmendedItems(headerItems[8], 0, "39", "항해예정일수", "24", "17", "3", "U", "Update");

			var lineItems = amendmentDetails.LineAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.ItemSequenceNumber).ThenBy(item => item.DataItemNo).ToArray();
			AssertEquals(19, lineItems.Length);
			AssertAmendedItems(lineItems[0], 1, "11B", "작업선", "CY_Code Test1", "Change SAMARIA TEST", "3", "U", "Update");
			AssertAmendedItems(lineItems[1], 1, "11C", "작업선박번호", "1", "9182", "3", "U", "Update");
			AssertAmendedItems(lineItems[2], 1, "11D", "운송차량번호", "1", "제주 허12 3456", "3", "U", "Update");
			AssertAmendedItems(lineItems[3], 1, "21", "품행 및 규격", "STAINLESS STEEL", "Change STAINLESS STEEL", "3", "U", "Update");
			AssertAmendedItems(lineItems[4], 1, "22", "물품식별번호", "물품식별번호", "정정물품식별번호", "3", "U", "Update");
			AssertAmendedItems(lineItems[5], 1, "23", "품목번호", "1234567890", "1234567900", "3", "U", "Update");
			AssertAmendedItems(lineItems[6], 1, "24A", "수량", "9999", "4444", "3", "U", "Update");
			AssertAmendedItems(lineItems[7], 1, "24B", "단위", "KG", "G", "3", "U", "Update");
			AssertAmendedItems(lineItems[8], 1, "25", "순중량(KG)", "9999", "4444", "3", "U", "Update");
			AssertAmendedItems(lineItems[9], 1, "26", "공급금액(FOB)", "1000", "500", "3", "U", "Update");
			AssertAmendedItems(lineItems[10], 1, "27", "포장갯수", "99", "44", "3", "U", "Update");
			AssertAmendedItems(lineItems[11], 1, "28", "포장종류", "VL", "PL", "3", "U", "Update");
			AssertAmendedItems(lineItems[12], 1, "29", "근거서류종류", "1", "2", "3", "U", "Update");
			AssertAmendedItems(lineItems[13], 1, "30", "반입(적재)근거서류번호", "L172770912345", "L172770912350", "3", "U", "Update");
			AssertAmendedItems(lineItems[14], 1, "31", "반입일자", "20130101", "20131231", "3", "U", "Update");
			AssertAmendedItems(lineItems[15], 1, "32A", "원상태근거번호", "010151234567001999", "010151234567002000", "3", "U", "Update");
			AssertAmendedItems(lineItems[16], 1, "32B", "원상태근거서류종류", "01", "02", "3", "U", "Update");
			AssertAmendedItems(lineItems[17], 2, "", "", "", "", "2", "D", "Delete");
			AssertAmendedItems(lineItems[18], 3, "11B", "작업선", "", "Add SAMARIA TEST", "", "", "Add");

			var allAmendItems = amendmentDetails.AllAmendedItems.OfType<LocalExportAmendItemWrapper>().OrderBy(item => item.ItemSequenceNumber).ThenBy(item => item.DataItemNo).ToArray();
			AssertEquals(28, allAmendItems.Length);

			AssertAmendedItems(allAmendItems[0], 0, "11A", "반입(적재)장소", "경남창고", "새 반입장소", "3", "U", "Update");
			AssertAmendedItems(allAmendItems[8], 0, "39", "항해예정일수", "24", "17", "3", "U", "Update");
			AssertAmendedItems(allAmendItems[9], 1, "11B", "작업선", "CY_Code Test1", "Change SAMARIA TEST", "3", "U", "Update");
			AssertAmendedItems(allAmendItems[22], 1, "30", "반입(적재)근거서류번호", "L172770912345", "L172770912350", "3", "U", "Update");
			AssertAmendedItems(allAmendItems[26], 2, "", "", "", "", "2", "D", "Delete");
			AssertAmendedItems(allAmendItems[27], 3, "11B", "작업선", "", "Add SAMARIA TEST", "", "", "Add");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIDs()
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.CH_VersionID = 1;

			var message5DR = entry.Messages.AddNew();
			message5DR.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message5DR.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5DR.EM_MessageType = ElectronicDocumentTypeList.Codes._5DR;
			message5DR.EM_SystemCreateUser = "ORG";
			message5DR.EM_ApplicationReference = "2";
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\LocalExport\Outgoing\GOVCBR5DR_Test.xml"));
			message5DR.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			Factory.Save();

			var amendmentDetails = entry.LocalExportAmendmentDetailsCollection[0];
			var allAmendedItems = amendmentDetails.AllAmendedItems;

			AssertEquals("Header", allAmendedItems[0].ID);
			AssertEquals("Entry Line : 1", allAmendedItems[1].ID);
			AssertEquals("Transport Means : 2", allAmendedItems[2].ID);
		}
		public void TestLocalExportAmendmentDetailsResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(LocalExportAmendmentDetails), nameof(LocalExportAmendmentDetails.NoticeType), false, attribute => attribute.Caption == "Review Result");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(LocalExportAmendmentDetails), nameof(LocalExportAmendmentDetails.NoticeTypeDescription), false, attribute => attribute.Caption == "Review Result Desc.");
		}

		RefVessel CreateRefVessel(string rv_Code, string rv_MalaysiaVesselId, string rv_RadioCallSign)
		{
			RefVessel result = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, rv_Code));
			if (result == null)
			{
				result = RefVessel.New(Factory);
				result.RV_Code = rv_Code;
				result.RV_MalaysiaVesselId = rv_MalaysiaVesselId;
				result.RV_RadioCallSign = rv_RadioCallSign;
			}
			return result;
		}

		void CreateSnapshot(CusEntryHeader entry, LocalExportEntryHeader originalHeader, string messageType)
		{
			using (var stream = KRXmlObjectSerializer.Serialize(originalHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, messageType);
				Factory.Save();
			}
		}
		void AssertAmendedItems(LocalExportAmendItemWrapper wrapper, ZShort sequenceNo, ZString dataItemID, ZString dataItemDescription, ZString beforeValue, ZString afterValue, ZString amendType, ZString amendTypeDescription, ZString amendTypeDescriptionForEntry)
		{
			AssertEquals(sequenceNo, wrapper.ItemSequenceNumber);
			AssertEquals(dataItemID, wrapper.DataItemNo);
			AssertEquals(dataItemDescription, wrapper.DataItemDescription);
			AssertEquals(amendType, wrapper.AmendType);
			AssertEquals(amendTypeDescription, wrapper.AmendTypeDescription);
			AssertEquals(amendTypeDescriptionForEntry, wrapper.AmendTypeDescriptionForEntry);
			AssertEquals(beforeValue, wrapper.BeforeValue);
			AssertEquals(afterValue, wrapper.AfterValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;

			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ORG";
			staff.GS_LoginName = "Origin";
			staff.GS_EmailAddress = "OriginalSender@wisetechglobal.com";
			Factory.Save();
		}
		GlbStaff staff;
		JobDeclaration declaration;

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new LocalExportAmendmentDetails(new LocalExportAmendmentHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>()), Factory);
		}
	}
}
