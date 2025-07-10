using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSEnquiryMessageBuilderTest : COLSMessageBuilderTest
	{
		public override void TestCreateNewMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var impNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Australia.IMP, Core.Constants.CountryCodes.Australia);
			impNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			impNumber.CE_EntryNum = "IMP12345";
			var lrnNumber = CusEntryNumber.New(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			lrnNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			lrnNumber.CE_EntryNum = "LRN1";
			var additionalInformation = new COLSEnquiryAdditionalInformation(colsHeader);
			additionalInformation.EnquiryType = "Location Change";
			additionalInformation.ContactName = "Company name";
			additionalInformation.ContactPhone = "+61297448000";
			additionalInformation.ContactEmail = "test@company.com";
			additionalInformation.DocumentRequired = false;

			var messageBuilder = new COLSEnquiryMessageBuilder(colsHeader, additionalInformation, "Additional comment");
			var message = messageBuilder.CreateNewMessage();
			var expectedText = "{\"enquiryType\":\"Location Change\",\"lodgementReferenceNumber\":\"LRN1\",\"fullImportDeclarationNumber\":\"IMP12345\",\"contactName\":\"Company name\",\"contactPhone\":\"0297448000\",\"contactEmail\":\"test@company.com\",\"additionalComments\":\"Additional comment\",\"generalDeclaration\":\"True\",\"documentationRequired\":false}";

			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertEquals("Message type", AUCOLSMessageTypeList.Codes.MakeAnEnquiry, message.EM_MessageType);
				AssertEquals("Message EM_LinkUniqueID", colsHeader.PK, message.EM_LinkUniqueID);
				AssertEquals("Message EM_LinkTable", colsHeader.TableName, message.EM_LinkTable);
				AssertEquals("Message text", expectedText, message.EM_MessageText);
			});
		}
	}
}
