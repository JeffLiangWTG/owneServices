using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportCancellationDetails))]
	public class ExportCancellationDetailsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExportCancellationDetailsByDKJMessageData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "040", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			AssertExceptionThrown<Exception>(() => new ExportCancellationDetails(null));

			entry.EntryNumber = "1234520100523X";
			entry.CusEntryNumber.CE_EntryType = "EXP";
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime("2021-02-03");
			entry.Declaration.JE_EntrySubmittedDate = new ZDateTime(2021, 02, 03);
			entry.CH_EntryReleaseDate = new ZDateTime(2021, 02, 04);
			entry.Declaration.JE_CustomsOffice = "030";
			entry.Declaration.JE_CustomsDivision = "10";

			Factory.Save();
			var cancellationDetails = new ExportCancellationDetails(messageDKJ);
			cancellationDetails.Decorate(entry);
			AssertEquals("D", cancellationDetails.FaultParty);
			AssertEquals(ExportImputationReasonCodeList.Descriptions.D, cancellationDetails.FaultPartyDescription);
			AssertEquals("99", cancellationDetails.ReasonCode);
			AssertEquals(ExportDeclarationwithdrawReasonCodeList.Descriptions._99, cancellationDetails.ReasonCodeDescription);
			AssertEquals(new ZDateTime("2021-02-02"), cancellationDetails.SubmissionDate);
			AssertEquals(new ZDateTime("2021-02-03"), cancellationDetails.DeclarationDate);
			AssertEquals(new ZDateTime("2021-02-04"), cancellationDetails.ReleaseDate);
			AssertEquals("040", cancellationDetails.MessageSendingObjectDKJ.CustomsOffice);
			AssertEquals("15", cancellationDetails.MessageSendingObjectDKJ.CustomsDivision);
			AssertEquals("인천세관", cancellationDetails.MessageSendingObjectDKJ.CustomsOfficeName);
			AssertNotNull(cancellationDetails.Declarant);
			AssertNotNull(cancellationDetails.Supplier);
			AssertEquals("화주업무 오류", cancellationDetails.AmendReasonDescription);
			AssertEquals(ZDateTime.Empty, cancellationDetails.AuthorisationDate);
			AssertEquals(ZString.Empty, cancellationDetails.AuthorisationNumber);
			AssertEquals(ZString.Empty, cancellationDetails.CustomsOfficerID);
			AssertEquals(ZString.Empty, cancellationDetails.CustomsOfficerName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportCancellationDetailsBy5DTMessageData()
		{
			entry.EntryNumber = "000000000000000";
			entry.CusEntryNumber.CE_EntryType = "EXP";

			var message5DT = entry.Messages.AddNew();
			message5DT.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5DT.EM_MessageType = ElectronicDocumentTypeList.Codes._5DT;
			message5DT.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5DT.EM_ApplicationReference = messageDKJ.EM_MessageNum;
			message5DT.EM_LinkedObject = entry;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Incoming\GOVCBR5DT_Test.xml"));
			message5DT.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			Factory.Save();

			var amendmentDetails = new ExportCancellationDetails(messageDKJ);
			AssertEquals(new ZDateTime("2014-05-06"), amendmentDetails.AuthorisationDate);
			AssertEquals("00000000000000", amendmentDetails.AuthorisationNumber);
			AssertEquals("000-00-00-0000000", amendmentDetails.FormattedAuthorisationNumber);
			AssertEquals("AVC010", amendmentDetails.CustomsOfficerID);
			AssertEquals("담당자명", amendmentDetails.CustomsOfficerName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExportCancellationDetails(messageDKJ);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var message830 = entry.Messages.AddNew();
			message830.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message830.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message830.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			Factory.Save();

			messageDKJ = entry.Messages.AddNew();
			messageDKJ.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			messageDKJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageDKJ.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			var fileReader = new TestFileReader(typeof(ExportCancellationDetailsTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestFilesPath, "GOVCBRDKJ_Test.xml");
			messageDKJ.EM_MessageData = testMsgFile;
			Factory.Save();
		}
		CusEntryHeader entry;
		EDIMessage messageDKJ;
		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing";
	}
}
