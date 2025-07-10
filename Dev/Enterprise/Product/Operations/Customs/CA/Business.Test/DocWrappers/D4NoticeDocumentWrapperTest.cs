using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineIntegration;
using NUnit.Framework;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class D4NoticeDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestISourceIdentifierProvider()
		{
			var message = CreateD4NoticeMessageAndSetUpTestData();
			Factory.Save();

			var wrapper = new D4NoticeDocumentWrapper(message, YesNoList.Codes.Yes);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("D4NoticeDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", message.PK, supporter?.SourceIdentifier);
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "1", "Invoice/P.O. Number From RR70: FIELD IS MANDATORY", startDate, endDate);
			helper.CreateOrGetLanguage("FR", "French");
			helper.CreateNewOrGetExistingCusCodeListLanguage(code, "FR", "FACTURE/NUMERO DE BON DE COMMANDE: ZONE EST OBLIGATOIRE");

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, "S001", "Positive Functional Acknowledgement.", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, "S002", "Negative Functional Acknowledgement.", startDate, endDate);

			CACSubLocationTest.CreateSubLocation(Factory, "4543", "CANADIAN NATIONAL RAILWAY CO.");
			Factory.Save();

			var message = CreateD4NoticeMessageAndSetUpTestData();
			var wrapper = new D4NoticeDocumentWrapper(message, YesNoList.Codes.Yes);

			CombineAssertions(() =>
			{
				AssertEquals("EventType", "Customs Manifest Status", wrapper.EventType);
				AssertEquals("DocumentType", "test 123 of TotalResponsesCount", wrapper.DocumentType);
				AssertEquals("ProcessingDate", new ZDateTime(2016, 5, 27, 14, 25, 0).ToString(), wrapper.ProcessingDate);
				AssertEquals("SendersReference", "SECONDARY BUSINESS ID", wrapper.SendersReference);
				AssertEquals("ReferenceNumber", "12345000000012", wrapper.ReferenceNumber);
				AssertEquals("RelatedDocument Count", 3, wrapper.RelatedDocuments.Count);
				AssertEquals("First Related Document Number", "99999000070328", wrapper.RelatedDocuments[0].DocumentNumber);
				AssertEquals("First Related Document Type", "RD0-1000", wrapper.RelatedDocuments[0].DocumentType);
				AssertEquals("Second Related Document Number", "99999000070329", wrapper.RelatedDocuments[1].DocumentNumber);
				AssertEquals("Second Related Document Type", "RD0-1001", wrapper.RelatedDocuments[1].DocumentType);
				AssertEquals("Third Related Document Number", "99999000070330", wrapper.RelatedDocuments[2].DocumentNumber);
				AssertEquals("Third Related Document Type", "RD0-1002", wrapper.RelatedDocuments[2].DocumentType);
				AssertEquals("Previous CCN", "99999000070328", wrapper.PreviousCCN);
				AssertEquals("Reference Number", "12345000000012", wrapper.ReferenceNumber);
				AssertEquals("Carrier Code", "1234", wrapper.CarrierCode);

				var pgaDetails = wrapper.PGADetails;
				var pgaDetail = pgaDetails[0];
				AssertEquals("PGA", "CFIA", pgaDetail.PGA);
				AssertEquals("Name", "Canadian Food Inspection Agency", pgaDetail.Name);
				AssertEquals("Type", "Type", pgaDetail.Type);
				AssertEquals("Port", "Port", pgaDetail.Port);
				AssertEquals("Code", "4543", pgaDetail.Code);
				AssertEquals("SubLocation", "CANADIAN NATIONAL RAILWAY CO.", pgaDetail.SubLocation);

				var statuses = wrapper.Statuses;
				var status1 = statuses[0];
				AssertEquals("Code", "S001", status1.Code);
				AssertEquals("Description", "Positive Functional Acknowledgement.", status1.Description);
				var status2 = statuses[1];
				AssertEquals("Code", "S002", status2.Code);
				AssertEquals("Description", "Negative Functional Acknowledgement.", status2.Description);

				AssertEquals("NoticeRecipientType", "Freight Forwarder", wrapper.NoticeRecipientType);
				AssertEquals("NoticeRecipientReferenceNumber", "7000", wrapper.NoticeRecipientReferenceNumber);

				var closeMessageHouseBillses = wrapper.CloseMessageHouseBillses;
				var closeMessageHouseBills = closeMessageHouseBillses[0];
				AssertEquals("DocumentID", "99999000070329", closeMessageHouseBills.DocumentID);
				AssertEquals("DocumentType", "RD0-1001", closeMessageHouseBills.DocumentType);

				var requestingPGAs = wrapper.RequestingPGAs;
				var requestingPGA = requestingPGAs[0];
				AssertEquals("PGA", "CFIA", requestingPGA.PGA);
				AssertEquals("Name", "Canadian Food Inspection Agency", requestingPGA.Name);
				AssertEquals("CommentsSpecialInstructions", "RequestingReviewComments\r\nRequestingSpecialInstructions", requestingPGA.CommentsSpecialInstructions);
				AssertEquals("Errors", "RequestingErrorDescription", requestingPGA.Errors);

				var errorDetails = wrapper.ErrorDetails;
				var errorDetail = errorDetails[0];
				AssertEquals("Code", "1", errorDetail.Code);
				AssertEquals("Description", "Invoice/P.O. Number From RR70: FIELD IS MANDATORY", errorDetail.EnglishDescription);
				AssertEquals("French Description", "FACTURE/NUMERO DE BON DE COMMANDE: ZONE EST OBLIGATOIRE", errorDetail.FrenchDescription);
				AssertEquals("Text", "CRR-123", errorDetail.Text);
				AssertEquals("Location", "CRR-456", errorDetail.Location);

				AssertEquals("ContainerString", "CTN-123, CTN-456", wrapper.ContainerString);
				var expectedRawMessage = @"UNH+2+GOVCBR:D:13A:UN+ECRD40'
BGM+23:::ME1-1212+803629102014:1:1+11'
DTM+9:201412152359:203'
RFF+AGO:SECONDARYBUSINESSID'
RFF+ACE:8XXX2XXXCCN1XXXXXXXXX::LA0-1000'
GOR++5'
STS++2:::0001'
UNS+D'
HYN+3'
UNS+S'
UNT+11+2'
";
				AssertEquals("RawMessage", expectedRawMessage, wrapper.RawMessage);
			});
		}

		UniversalEventMessage CreateD4NoticeMessageAndSetUpTestData()
		{
			var message = Factory.New<UniversalEventMessage>();
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\D4NoticeMessage.xml");
			return message;
		}
	}
}
