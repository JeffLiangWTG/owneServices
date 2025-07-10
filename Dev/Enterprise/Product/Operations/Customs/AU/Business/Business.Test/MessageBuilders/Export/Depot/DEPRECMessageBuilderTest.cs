namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DEPRECMessageBuilderTest : CMRMessageBuilderAbstractTest
	{
		public void TestOriginalDEPRECMessage()
		{
			var expectedMessage = CMRExportMessagesTestData.DEPRECOriginal;
			var builder = new DEPRECMessageBuilder(declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		public void TestReplacelDEPRECMessage()
		{
			var expectedMessage = CMRExportMessagesTestData.DEPRECReplace;
			var builder = new DEPRECMessageBuilder(declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		public void TestWithdrawDEPRECMessage()
		{
			var expectedMessage = CMRExportMessagesTestData.DEPRECWithdrawl;
			var builder = new DEPRECMessageBuilder(declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000001";
			SetDepotCode(declaration, "9120H");
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.DeclarationNumber = "AAAACR74T";
		}

		protected override CMRMessageBuilder GetMessageBuilderToTest() => new DEPRECMessageBuilder(declaration);

		JobDeclaration declaration;
	}
}
