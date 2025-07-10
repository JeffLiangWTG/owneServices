using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportAmendmentDetails))]
	sealed class ImportAmendmentDetailsTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFaultPartyDescription99()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._929;
			entry.EntryNumber = "416372000190U";
			entry.CusEntryNumber.CE_EntryType = KRJobMessageTypeList.Codes.Import;

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Import\Outgoing\GOVCBR5FE_1.xml"));
			message5FE.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5FE.EM_SystemCreateUser = staff.GS_Code;
			Factory.Save();

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_ApplicationReference = message5FE.EM_MessageNum;
			incomingMessage.EM_LinkedObject = entry;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Import\Incoming\GOVCBR5FK_ANT.xml"));
			incomingMessage.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry.Reload();
			incomingMessage.Reload();
			AssertEquals(message5FE.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var amendmentDetails = entry.ImportAmendmentDetailsCollection[0];
			AssertEquals(ImputationReasonCodeList.Codes._99, amendmentDetails.FaultParty);
			AssertEquals("귀책사유상세사유", amendmentDetails.FaultPartyDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportAmendmentDetailsWhenReceive5FKMessage()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._929;
			entry.EntryNumber = "416372000190U";
			entry.CusEntryNumber.CE_EntryType = KRJobMessageTypeList.Codes.Import;

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Import\Outgoing\GOVCBR5FE_0.xml"));
			message5FE.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			message5FE.EM_SystemCreateUser = staff.GS_Code;
			Factory.Save();

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_ApplicationReference = message5FE.EM_MessageNum;
			incomingMessage.EM_LinkedObject = entry;
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Import\Incoming\GOVCBR5FK_ANT.xml"));
			incomingMessage.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry.Reload();
			incomingMessage.Reload();
			AssertEquals(message5FE.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var amendmentDetails = entry.ImportAmendmentDetailsCollection[0];
			AssertEquals(new ZDateTime("2014-05-06"), amendmentDetails.SubmissionDate);
			AssertEquals(2, amendmentDetails.AmendSequenceNo);
			AssertEquals(ImportDeclarationModifyReasonCodeList.Codes._01, amendmentDetails.ReasonCode);
			AssertEquals(ImportDeclarationModifyReasonCodeList.Descriptions._01, amendmentDetails.ReasonCodeDescription);
			AssertEquals(ImputationReasonCodeList.Codes._01, amendmentDetails.FaultParty);
			AssertEquals(ImputationReasonCodeList.Descriptions._01, amendmentDetails.FaultPartyDescription);
			AssertEquals(ImportAmedmentTypeList.Codes.A, amendmentDetails.AmendType1);
			AssertEquals(ImportAmedmentTypeList.Descriptions.A, amendmentDetails.AmendType1Description);
			AssertEquals(ImportAmedmentTypeList.Codes.D, amendmentDetails.AmendType2);
			AssertEquals(ImportAmedmentTypeList.Descriptions.D, amendmentDetails.AmendType2Description);
			AssertEquals("11", amendmentDetails.NoticeType);
			AssertEquals("승인통보", amendmentDetails.NoticeTypeDescription);
			AssertEquals("0127-020-11-20-0-004951-3", amendmentDetails.CustomsDisbursementBillNumber);
			AssertEquals(new ZDateTime("27-Aug-20 00:02:00"), amendmentDetails.DecisionDate);
			AssertEquals("직원미등록", amendmentDetails.CustomerOfficer);
			AssertEquals(1m, amendmentDetails.PaymentAmount);
			AssertEquals(2m, amendmentDetails.DelayPaymentAmount);
			AssertEquals(3m, amendmentDetails.AmendPenaltyPayable);
			AssertEquals(999999999999m, amendmentDetails.BeforeTotalDutyTax);
			AssertEquals(999999999999m, amendmentDetails.AfterTotalDutyTax);
			AssertEquals(999999999999m, amendmentDetails.DutyTaxDifference);
			AssertEquals(999999999999m, amendmentDetails.BeforeCustomsValue);
			AssertEquals(999999999999m, amendmentDetails.AfterCustomsValue);
			AssertEquals(999999999999m, amendmentDetails.CustomsValueDifference);
		}

		public void TestImportAmendmentDetailsWithoutImportAmendmentMessageDetails()
		{
			var importAmendmentDetails = new ImportAmendmentDetails(new Import5FEHeader(), Factory);
			AssertEquals(ZDate.Empty, importAmendmentDetails.SubmissionDate);
			AssertEquals(0, importAmendmentDetails.AmendSequenceNo);
			AssertEquals(ZString.Empty, importAmendmentDetails.FaultPartyDescription);
			AssertEquals(ZString.Empty, importAmendmentDetails.AmendType1);
			AssertEquals(ZString.Empty, importAmendmentDetails.AmendType2);
			AssertEquals(ZString.Empty, importAmendmentDetails.FaultParty);
			AssertEquals(ZString.Empty, importAmendmentDetails.ReasonCode);
		}
		public void TestImportAmendmentDetailsResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ImportAmendmentDetails), nameof(ImportAmendmentDetails.NoticeType), false, attribute => attribute.Caption == "Review Result");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ImportAmendmentDetails), nameof(ImportAmendmentDetails.NoticeTypeDescription), false, attribute => attribute.Caption == "Review Result Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ImportAmendmentDetails), nameof(ImportAmendmentDetails.DecisionDate), false, attribute => attribute.Caption == "Review Date");
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;

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
			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			return new ImportAmendmentDetails(new Import5FECreator().Create(entry, sendingObject), Factory);
		}
	}
}
