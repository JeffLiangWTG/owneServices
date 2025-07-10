using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DEPRELMessageBuilderTest : CMRMessageBuilderAbstractTest
	{
		public void TestOriginalDEPRELMessage()
		{
			ZString expectedMessage = CMRExportMessagesTestData.DEPRELOriginal;
			DEPRELMessageBuilder builder = new DEPRELMessageBuilder(Declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		public void TestReplacelDEPRELMessage()
		{
			ZString expectedMessage = CMRExportMessagesTestData.DEPRELReplace;
			DEPRELMessageBuilder builder = new DEPRELMessageBuilder(Declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		public void TestWithdrawDEPRELMessage()
		{
			ZString expectedMessage = CMRExportMessagesTestData.DEPRELWithdrawl;
			DEPRELMessageBuilder builder = new DEPRELMessageBuilder(Declaration);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		protected override CMRMessageBuilder GetMessageBuilderToTest() => new DEPRELMessageBuilder(Declaration);

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_DeclarationReference = "B00000001";
					SetDepotCode(declaration, "9120H");
					SetCTOCode(declaration, "1234X");
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					declaration.DeclarationNumber = "AAAACR74T";
				}
				return declaration;
			}
		}
	}
}
