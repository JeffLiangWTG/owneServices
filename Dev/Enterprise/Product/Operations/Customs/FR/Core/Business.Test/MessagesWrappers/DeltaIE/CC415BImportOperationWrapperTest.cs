using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	sealed class CC415BImportOperationWrapperTest : DataProviderTestCase<CC415BImportOperationWrapper>
	{
		public void TestLRN()
		{
			AssertEquals("LRN should equal CE_EntryNum where CE_EntryType=LRN.", "0123456789", Provider.LRN);
		}

		public void TestDeclarationType()
		{
			AssertEquals("DeclarationType should equal declaration JE_EntryStyle.", "IM", Provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("AdditionalDeclarationType should equal entry instruction CEI_SubStyle.", EntrySubstyleCodePairList.Codes.C, Provider.AdditionalDeclarationType);
		}

		public void TestPresentationNotificationEstimateDateAndTime()
		{
			AssertEquals("PresentationNotificationEstimateDateAndTime should equal entry instruction CEI_DateOfDuty.", "2022-12-07T08:50:32", Provider.PresentationNotificationEstimateDateAndTime);
		}

		public void TestLanguageCode()
		{
			AssertEquals("LanguageCode should equal declaration JE_DeclarationLanguage.", Core.Constants.CountryCodes.France, Provider.LanguageCode);
		}

		protected override CC415BImportOperationWrapper GetProvider()
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
			var cusEntryNumber = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			cusEntryNumber.CE_EntryNum = "9876543210";
			var cusEntryNumber2 = CusEntryNumber.New(entryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.France);
			cusEntryNumber2.CE_EntryIsSystemGenerated = true;
			cusEntryNumber2.CE_EntryNum = "0123456789";
			return CC415BImportOperationWrapper.New(entryHeader);
		}
	}
}
