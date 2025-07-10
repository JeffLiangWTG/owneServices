using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IM413And415OperationProviderTest : DataProviderTestCase<IM413And415OperationProvider>
	{
		public void TestIOperation()
		{
			Assert("Should implement IOperation", Provider is IOperation);
		}

		public void TestMsgType()
		{
			SetUpTestData();
			entryInstruction.CEI_Style = "AB";
			AssertEquals("MsgType", "AB", GetProvider().MsgType);
		}

		public void TestDeclarationType()
		{
			SetUpTestData();
			declaration.JE_EntryStyle = "AB";
			AssertEquals("DeclarationType", "AB", GetProvider().DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			SetUpTestData();
			entryInstruction.CEI_SubStyle = "AIS";
			AssertEquals("AdditionalDeclarationType", "AIS", GetProvider().AdditionalDeclarationType);
		}

		public void TestLanguageCode()
		{
			AssertEquals("LanguageCode", "EN", Provider.LanguageCode);
		}

		public void TestPreferredPaymentMethod()
		{
			SetUpTestData();
			declaration.JE_PaymentMethod = "A";
			AssertEquals("PreferredPaymentMethod", "A", Provider.PreferredPaymentMethod);
		}

		public void TestLRN()
		{
			SetUpTestData();
			entryHeader.CH_BGMReference = "LRN12312";
			var provider = new IM413And415OperationProvider(entryHeader, true);
			AssertEquals("LRN", AISOutboundEDIMessage.LRNPlaceHolder, provider.LRN);
			provider = new IM413And415OperationProvider(entryHeader, false);
			AssertEquals("LRN", "LRN12312", provider.LRN);
		}

		protected override IM413And415OperationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413And415OperationProvider(entryHeader, true);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryInstruction entryInstruction;
	}
}
