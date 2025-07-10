using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	sealed class CC432BCciOperationWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC432BCciOperationWrapper>
	{
		public void TestLRN()
		{
			AssertEquals("LRN should be captured from entryHeader.CorrelationID", "LRN #", Provider.LRN);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertEquals("CustomsRegistrationNumber should be captured from entryHeader.CRN", "CRN #", Provider.CustomsRegistrationNumber);
		}

		protected override CC432BCciOperationWrapper GetProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_EntryStyle = "IM";
			declaration.JE_DeclarationLanguage = Core.Constants.CountryCodes.France;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.C;
			entryInstruction.CEI_DateForDuty = new CargoWise.Types.ZDateTime(2022, 12, 7, 08, 50, 32);
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			entryHeader.CH_BGMReference = "202200000001";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var cusEntryNumber1 = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.France);
			cusEntryNumber1.CE_EntryIsSystemGenerated = true;
			cusEntryNumber1.CE_EntryNum = "LRN #";

			var cusEntryNumber2 = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.EU.CustomsRegistrationNumber, Core.Constants.CountryCodes.France);
			cusEntryNumber2.CE_EntryIsSystemGenerated = true;
			cusEntryNumber2.CE_EntryNum = "CRN #";

			return CC432BCciOperationWrapper.New(entryHeader);
		}
	}
}
