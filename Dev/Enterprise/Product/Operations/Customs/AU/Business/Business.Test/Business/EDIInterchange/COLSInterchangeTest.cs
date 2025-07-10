using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(COLSInterchange))]
	sealed class COLSInterchangeTest : EDIInterchangeTest
	{
		public void TestGenerateMessageFromInterchange()
		{
			var bodyText = "{\"lrn\":\"\",\"result\":\"VALIDATION FAILED\",\"validationMessages\":[{\"messageCode\":\"EM.03\",\"messageText\":\"A valid Import Permit number must be 6 to 10 digits.\"},{\"messageCode\":\"EM.09\",\"messageText\":\"A Contact name must be provided.\"},{\"messageCode\":\"EM.10\",\"messageText\":\"A valid, 10-digit phone number (including Australian area code) must be provided. \"},{\"messageCode\":\"EM.11\",\"messageText\":\"Email format is invalid.\"},{\"messageCode\":\"EM.05\",\"messageText\":\"A valid Direction Request type must be provided.\"},{\"messageCode\":\"EM.01\",\"messageText\":\"A valid combination of Full Import Declaration (FID)/Entry Number and Broker/Importer Branch ID must be provided.\"}]}";
			var interchangeToProcess = Factory.New<COLSInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			interchangeToProcess.EI_InterchangeType = "CNL";
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "00000000000105";
			interchangeToProcess.EI_From = "AAA336C";
			interchangeToProcess.EI_To = "AAL364P";
			interchangeToProcess.EI_BodyText = bodyText;
			Factory.Save();
			interchangeToProcess.SpawnMessagesFromInterchageTextAndMarkAsReceived(false);
			AssertEquals("Status", "RCV", interchangeToProcess.EI_Status);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "00000000000105000001"));
			AssertNotNull("Messages spawned", message);
			AssertEquals("linked to interchange", interchangeToProcess.PK, message.EM_EI);
			AssertEquals("Message status", "QUE", message.EM_Status);
			AssertEquals("Message type", "CNL", message.EM_MessageType);
			AssertEquals("Message Text", bodyText, message.EM_MessageText);
		}

		public void TestGetMessageData_InterchangeTypeCNA()
		{
			var fileContent = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00, 0x08, 0x00, 0x00, 0x00, 0x21, 0x00, 0xDF, 0xA4, 0xD2, 0x6C, 0x5A, 0x01, 0x00, 0x00, 0x20, 0x05, 0x00, 0x00, 0x13, 0x00, 0x08, 0x02, 0x5B, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x6E, 0x74, 0x5F, 0x54, 0x79, 0x70, 0x65, 0x73, 0x5D, 0x2E, 0x78, 0x6D, 0x6C, 0x20 };
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var doc = declaration.DocManagerInfo.AddFileOrDocument(fileContent, "Ref's;= 测试.pdf", "CIV");
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "XXX456789YYYYMMDDSS9999999";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var entryNumObject = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumObject.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_Category = "CUS";

			var cusStorageDocPivot = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot.CSD_DocType = "INVOICE";
			cusStorageDocPivot.CSD_Description = "XYZ123";
			cusStorageDocPivot.CSD_StorageDocReference = doc.UniqueKey;

			var messageBuilder = new COLSAttachmentMessageBuilder(colsHeader, cusStorageDocPivot, isPartOfOtherMessage: false, isFirstAttachment: true, isLastAttachment: true);
			var attachmentMessage = messageBuilder.CreateNewMessage();
			var interchange = Factory.New<COLSInterchange>();
			interchange.ContainedMessages.Add(attachmentMessage);
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.COLS;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_GB = attachmentMessage.EM_GB;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_IsActive = true;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_InterchangeType = attachmentMessage.EM_MessageType;
			interchange.EI_To = "xT";
			interchange.EI_From = attachmentMessage.Company.LicenceKeyIdentifier;
			interchange.EI_BodyText = attachmentMessage.EM_MessageText;

			using (var binaryReader = ((IMessageDataProvider)interchange).GetMessageData())
			{
				var messageData = ReadAsString(binaryReader);
				var expectedMessageData = $@"Content-Type: multipart/form-data; boundary=""{attachmentMessage.PK}""

--{attachmentMessage.PK}
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=lastDoc

True
--{attachmentMessage.PK}
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=docReference

XYZ123
--{attachmentMessage.PK}
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=docType

INVOICE
--{attachmentMessage.PK}
Content-Type: application/pdf
Content-Disposition: form-data; name=file; filename=""Ref_s__ 测试.pdf""; filename*=""utf-8''Ref_s__%20%E6%B5%8B%E8%AF%95.pdf""

{Encoding.UTF8.GetString(fileContent)}
--{attachmentMessage.PK}--
";
				AssertEquals(expectedMessageData, messageData);
			}
		}

		public void TestGetMessageData_InterchangeTypeNotCNA()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "TST";
			branch.GB_GC = company.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_GC = company.PK;
			declaration.JE_GB = branch.PK;
			Env.Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(branch.PK.ToGuid(), "AA33HF");
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "entry number";
			var entryNumObject = entryHeader.CusEntryNumber;
			entryNumObject.CE_EntryType = CusEntryNumberTypes.Australia.IMP;
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_Category = "CUS";
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "container number";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			colsHeader.QCH_BiosecurityImportConditionURL = "bicon reference";
			colsHeader.QCH_ImportPermitNumber = "permitnum";
			colsHeader.ResponsibleParty.E2_Contact = "123456789012345678901234567890123456789012";
			colsHeader.ResponsibleParty.E2_Phone = "+61 2 9744 8000";
			colsHeader.ResponsibleParty.E2_Mobile = "+61 415 923 947";
			colsHeader.ResponsibleParty.E2_Email = "123456789012345678901234567890123456789012";
			colsHeader.QCH_AlsoNotifyEmail = "thirdparty email address";
			colsHeader.QCH_ApprovedArrangementRefNum = "ref";
			colsHeader.QCH_LateLodgementReason = "lodgement reason";
			colsHeader.QCH_LateLodgementDetails = "lodgement details";
			colsHeader.DeliveryOrUnpack.Address1 = "Address1";
			colsHeader.DeliveryOrUnpack.Address2 = "ABCDEFGHIJKLMNOPQRSTUV";
			colsHeader.DeliveryOrUnpack.City = "City";
			colsHeader.DeliveryOrUnpack.Postcode = "Postcode";
			colsHeader.DeliveryOrUnpack.State = "State";
			colsHeader.QCH_DeliveryClassification = "class";
			var direction1 = colsHeader.Directions.AddNew();
			direction1.AAAddress.Address1 = "AA Address1";
			direction1.AAAddress.E2_CompanyName = "AA Company1";
			direction1.AAAddress.E2_GovRegNum = "AA Reg number1";
			direction1.QCD_CO_Container = container.PK;
			direction1.QCD_Direction = "direction";
			direction1.QCD_TreatmentType = "treatment type1";
			var direction2 = colsHeader.Directions.AddNew();
			direction2.AAAddress.Address1 = "AA Address2";
			direction2.AAAddress.E2_CompanyName = "AA Company2";
			direction2.AAAddress.E2_GovRegNum = "AA Reg number2";
			direction2.QCD_CO_Container = container.PK;
			direction2.QCD_Direction = "";
			direction2.QCD_TreatmentType = "treatment type2";

			var messageBuilder = new COLSLodgementMessageBuilder(colsHeader, "Additional comment");
			var lodgementMessage = messageBuilder.CreateNewMessage();
			var interchange = Factory.New<COLSInterchange>();
			interchange.ContainedMessages.Add(lodgementMessage);
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.COLS;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_GB = lodgementMessage.EM_GB;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_IsActive = true;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_InterchangeType = lodgementMessage.EM_MessageType;
			interchange.EI_To = "xT";
			interchange.EI_From = lodgementMessage.Company.LicenceKeyIdentifier;
			interchange.EI_BodyText = lodgementMessage.EM_MessageText;

			using (var textReader = ((IMessageDataProvider)interchange).GetMessageData())
			{
				var messageData = ReadAsString(textReader);
				var expectedMessageData = "{\"entryNumber\":\"entry number\",\"branchId\":\"AA33HF\",\"biconReference\":\"bicon reference\",\"importPermitNumber\":\"permitnum\",\"contactName\":\"1234567890123456789012345678901234567890\",\"phoneNumber\":\"0297448000\",\"email\":\"1234567890123456789012345678901234567890\",\"thirdPartyInd\":true,\"thirdPartyEmail\":\"thirdparty email address\",\"aaRefNum\":\"ref\",\"lateLodgementReason\":\"lodgement reason\",\"lateLodgementDetails\":\"lodgement details\",\"directionRequests\":[{\"direction\":\"direction\",\"directionLineContainer\":\"CONTAINER NUMBER\",\"treatmentType\":\"treatment type1\",\"location\":\"AA Address1\",\"aaname\":\"AA Company1\",\"aanumber\":\"AA Reg number1\"}],\"deliveryClassification\":\"class\",\"unpackAddress\":\"Address1,ABCDEFGHIJKLMNOPQRSTUV,City,Postcode,Stat\",\"additionalComment\":\"Additional comment\",\"generalDeclaration\":\"True\"}";
				AssertEquals(expectedMessageData, messageData);
			}
		}

		string ReadAsString(BinaryReader reader)
		{
			var bytes = new byte[reader.BaseStream.Length];
			reader.BaseStream.Read(bytes, 0, bytes.Length);
			return Encoding.UTF8.GetString(bytes);
		}
	}
}
