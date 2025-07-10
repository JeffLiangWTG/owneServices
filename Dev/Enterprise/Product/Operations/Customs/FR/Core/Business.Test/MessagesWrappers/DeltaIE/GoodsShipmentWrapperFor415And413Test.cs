using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	sealed class GoodsShipmentWrapperFor415And413Test : Customs.Business.Testing.DataProviderTestCase<GoodsShipmentWrapperFor415And413>
	{
		protected override GoodsShipmentWrapperFor415And413 GetProvider()
		{
			if (entryHeader == null)
			{
				SetUpEntryHeader();
			}
			return GoodsShipmentWrapperFor415And413.New(entryHeader);
		}

		void SetUpEntryHeader()
		{
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryHeader entryHeader;

		public void TestDateOfAcceptance()
		{
			SetUpEntryHeader();
			instruction.CEI_DateForDuty = new ZDateTime(2022, 10, 30, 14, 41, 57);
			instruction.CEI_SubStyle = "V";
			AssertEquals("DateOfAcceptance should equal instruction.CEI_DateForDuty when CEI_SubStyle equals V", "2022-10-30T14:41:57", Provider.DateOfAcceptance);

			instruction.CEI_SubStyle = "A";
			var newProvider1 = GoodsShipmentWrapperFor415And413.New(entryHeader);
			AssertNull("DateOfAcceptance should not be mapped when CEI_SubStyle is not V and additional document E0001 is not captured", newProvider1.DateOfAcceptance);

			var additionalInfo = declaration.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
			var previousDocument = declaration.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN;
			previousDocument.CSI_DateOfIssue = new ZDateTime(2025, 3, 6, 14, 41, 57);
			var newProvider2 = GoodsShipmentWrapperFor415And413.New(entryHeader);
			AssertEquals("DateOfAcceptance should equal NMRN previousDocument.CSI_DateOfIssue when additional document E0001 is captured", "2025-03-06T14:41:57", newProvider2.DateOfAcceptance);
		}
	}
}
