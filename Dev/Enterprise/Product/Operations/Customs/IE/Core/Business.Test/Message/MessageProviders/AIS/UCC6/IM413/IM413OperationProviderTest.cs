using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IM413OperationProviderTest : DataProviderTestCase<IM413OperationProvider>
	{
		public void TestIOperation()
		{
			Assert("Should implement IOperation", Provider is IIM413Operation);
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
			entryHeader.CH_BGMReference = "Test";
			AssertEquals("LRN", "Test", Provider.LRN);
		}

		public void TestCustomsRegistrationNumber()
		{
			SetUpTestData();
			entryHeader.CRN = "Test";
			AssertEquals("CustomsRegistrationNumber", "Test", Provider.CustomsRegistrationNumber);
		}

		public void TestDetailsAmended()
		{
			SetUpTestData();
			entryHeader.CH_CustomsMessageRemarks = "TestDetailsAmended";
			AssertEquals("DetailsAmended", "TestDetailsAmended", Provider.DetailsAmended);
		}

		public void TestMRN()
		{
			SetUpTestData();
			AssertEquals("MRN", "MRN001", Provider.MRN);
		}

		protected override IM413OperationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413OperationProvider(entryHeader);
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
				entryHeader.MovementReferenceNumberSetter("MRN001");
			}
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryInstruction entryInstruction;
	}
}
