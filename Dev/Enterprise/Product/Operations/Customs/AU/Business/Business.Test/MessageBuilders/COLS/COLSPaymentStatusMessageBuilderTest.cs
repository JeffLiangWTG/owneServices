namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSPaymentStatusMessageBuilderTest : COLSMessageBuilderTest
	{
		public override void TestCreateNewMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;

			var messageBuilder = new COLSPaymentStatusMessageBuilder(colsHeader);
			var message = messageBuilder.CreateNewMessage();

			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertEquals("Message type", AUCOLSMessageTypeList.Codes.PaymentStatus, message.EM_MessageType);
				AssertEquals("Message EM_LinkUniqueID", colsHeader.PK, message.EM_LinkUniqueID);
				AssertEquals("Message EM_LinkTable", colsHeader.TableName, message.EM_LinkTable);
				AssertEquals("Message text", "{}", message.EM_MessageText);
			});
		}
	}
}
