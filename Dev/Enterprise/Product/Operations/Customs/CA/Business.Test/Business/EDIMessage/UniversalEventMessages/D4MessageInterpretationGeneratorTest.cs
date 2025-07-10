using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class D4MessageInterpretationGeneratorTest : UniversalEventMessageInterpretationGeneratorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestProperties()
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
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, "S001", "Positive Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, "S002", "Negative Functional Acknowledgement.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			CACSubLocationTest.CreateSubLocation(Factory, "4543", "CANADIAN NATIONAL RAILWAY CO.");
			Factory.Save();

			var message = CreateD4NoticeMessageAndSetUpTestData(Factory, File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\D4NoticeMessage.xml"));
			var generator = new D4MessageInterpretationGenerator(Factory, message);

			CombineAssertions(() =>
			{
				AssertEquals("EventType", "Customs Manifest Status", generator.EventType);
				AssertEquals("DocumentType", "test 123 of TotalResponsesCount", generator.DocumentType);
				AssertEquals("ProcessingDate", new ZDateTime(2016, 5, 27, 14, 25, 0), generator.ProcessingDate);
				AssertEquals("SendersReference", "SECONDARY BUSINESS ID", generator.SendersReference);
				AssertEquals("ReferenceNumber", "12345000000012", generator.ReferenceNumber);
				AssertEquals("RelatedDocument Count", 3, generator.RelatedDocuments.Count);
				AssertEquals("First Related Document Number", "99999000070328", generator.RelatedDocuments[0].DocumentNumber);
				AssertEquals("First Related Document Type", "RD0-1000", generator.RelatedDocuments[0].DocumentType);
				AssertEquals("Second Related Document Number", "99999000070329", generator.RelatedDocuments[1].DocumentNumber);
				AssertEquals("Second Related Document Type", "RD0-1001", generator.RelatedDocuments[1].DocumentType);
				AssertEquals("Third Related Document Number", "99999000070330", generator.RelatedDocuments[2].DocumentNumber);
				AssertEquals("Third Related Document Type", "RD0-1002", generator.RelatedDocuments[2].DocumentType);

				var pgaDetails = generator.PGADetails.ToList();
				var pgaDetail = pgaDetails[0];
				AssertEquals("PGA", "CFIA", pgaDetail.PGA);
				AssertEquals("Name", "Canadian Food Inspection Agency", pgaDetail.Name);
				AssertEquals("Type", "Type", pgaDetail.Type);
				AssertEquals("Port", "Port", pgaDetail.Port);
				AssertEquals("Code", "4543", pgaDetail.Code);
				AssertEquals("SubLocation", "CANADIAN NATIONAL RAILWAY CO.", pgaDetail.SubLocation);

				var statuses = generator.Statuses.ToList();
				var status1 = statuses[0];
				AssertEquals("Code", "S001", status1.Code);
				AssertEquals("Description", "Positive Functional Acknowledgement.", status1.Description);
				var status2 = statuses[1];
				AssertEquals("Code", "S002", status2.Code);
				AssertEquals("Description", "Negative Functional Acknowledgement.", status2.Description);

				AssertEquals("NoticeRecipientType", "Freight Forwarder", generator.NoticeRecipientType);
				AssertEquals("NoticeRecipientReferenceNumber", "7000", generator.NoticeRecipientReferenceNumber);

				var closeMessageHouseBillses = generator.CloseMessageHouseBillses.ToList();
				var closeMessageHouseBills = closeMessageHouseBillses[0];
				AssertEquals("DocumentID", "99999000070329", closeMessageHouseBills.DocumentID);
				AssertEquals("DocumentType", "RD0-1001", closeMessageHouseBills.DocumentType);

				var requestingPGAs = generator.RequestingPGAs.ToList();
				var requestingPGA = requestingPGAs[0];
				AssertEquals("PGA", "CFIA", requestingPGA.PGA);
				AssertEquals("Name", "Canadian Food Inspection Agency", requestingPGA.Name);
				AssertEquals("CommentsSpecialInstructions", "RequestingReviewComments\r\nRequestingSpecialInstructions", requestingPGA.CommentsSpecialInstructions);
				AssertEquals("Errors", "RequestingErrorDescription", requestingPGA.Errors);

				var errorDetails = generator.ErrorDetails.ToList();
				var errorDetail = errorDetails[0];
				AssertEquals("Code", "1", errorDetail.Code);
				AssertEquals("Description", "Invoice/P.O. Number From RR70: FIELD IS MANDATORY", errorDetail.EnglishDescription);
				AssertEquals("French Description", "FACTURE/NUMERO DE BON DE COMMANDE: ZONE EST OBLIGATOIRE", errorDetail.FrenchDescription);
				AssertEquals("Text", "CRR-123", errorDetail.Text);
				AssertEquals("Location", "CRR-456", errorDetail.Location);

				AssertEquals("ContainerString", "CTN-123, CTN-456", generator.ContainerString);
			});
		}

		protected override UniversalEventMessage CreateD4NoticeMessageAndSetUpTestData(BusinessObjectFactory factory, string messageText)
		{
			var message = Factory.New<UniversalEventMessage>();
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			message.EM_MessageText = messageText;
			return message;
		}
	}
}
