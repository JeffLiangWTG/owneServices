namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSAdditionalDocumentMessageBuilderTest : COLSMessageBuilderTest
	{
		public override void TestCreateNewMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;

			var messageBuilder = new COLSAdditionalDocumentMessageBuilder(colsHeader, "Additional comment");
			var message = messageBuilder.CreateNewMessage();
			var expectedText = "{\"additionalComment\":\"Additional comment\",\"generalDeclaration\":\"True\"}";

			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertEquals("Message type", AUCOLSMessageTypeList.Codes.AddAdditionalDocument, message.EM_MessageType);
				AssertEquals("Message EM_LinkUniqueID", colsHeader.PK, message.EM_LinkUniqueID);
				AssertEquals("Message EM_LinkTable", colsHeader.TableName, message.EM_LinkTable);
				AssertEquals("Message text", expectedText, message.EM_MessageText);
			});
		}
	}
}
