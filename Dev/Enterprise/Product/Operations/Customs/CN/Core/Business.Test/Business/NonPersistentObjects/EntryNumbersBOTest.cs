using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryNumbersBO))]
	class EntryNumbersBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckMovementReferenceNumberIssueDate()
		{
			testCusEntryHeaderExisting.CH_JE = testDeclaration.PK;
			testEntryNumbersBO.MovementReferenceNumber = "MRNX";
			testEntryNumbersBO.RunPreSaveValidation();
			AssertHasErrorContaining(testEntryNumbersBO.MovementReferenceNumberIssueDateInfo, MandatoryValidation.MustBeEntered);
			testEntryNumbersBO.MovementReferenceNumberIssueDate = new ZDateTime(2018, 1, 5);
			testEntryNumbersBO.ValidateMovementReferenceNumberIssueDate();
			AssertNoErrors(testEntryNumbersBO.MovementReferenceNumberIssueDateInfo);
		}

		[TestDate(2018, 1, 5)]
		public void TestCheckEntryNumbersCUS()
		{
			TestCheckEntryNumbersCUSOrREC(EntryTypeList.Codes.CustomsEntry);
		}

		[TestDate(2018, 1, 5)]
		public void TestCheckEntryNumbersREC()
		{
			TestCheckEntryNumbersCUSOrREC(EntryTypeList.Codes.RecordListing);
		}

		[TestDate(2018, 1, 5)]
		void TestCheckEntryNumbersCUSOrREC(string chMessageType)
		{
			testCusEntryHeaderExisting.CH_JE = testDeclaration.PK;
			testCusEntryHeaderExisting.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "PREX");
			testCusEntryHeaderExisting.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "UNIX");
			testCusEntryHeaderExisting.ManuallySetEntryNumber(CusEntryNumberTypes.China.CIQNumber, "CIQX");
			testCusEntryHeaderExisting.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "MRNX", ZDateTime.Today);
			testCusEntryHeaderExisting.CH_MessageType = chMessageType;
			Factory.Save();
			testCusEntryHeader.CH_MessageType = chMessageType;
			testEntryNumbersBO.PreEntryNumber = "BBB";
			testEntryNumbersBO.MovementReferenceNumber = "BBB";
			AssertNoMessageErrorContaining(testEntryNumbersBO.MovementReferenceNumberInfo, "This number is the same as another number on this job, please make sure a correct number is entered.");
			testEntryNumbersBO.PreEntryNumber = "AAA";
			testEntryNumbersBO.DeclarationUnifiedNumber = "AAA";
			testEntryNumbersBO.MovementReferenceNumber = "AAA";
			testEntryNumbersBO.CIQNumber = "AAA";
			testEntryNumbersBO.RunPreSaveValidation();
			AssertHasMessageError(testEntryNumbersBO.DeclarationUnifiedNumberInfo, "This number is the same as another number on this job, please make sure a correct number is entered.");
			AssertHasMessageError(testEntryNumbersBO.MovementReferenceNumberInfo, "This number is the same as another number on this job, please make sure a correct number is entered.");
			AssertHasMessageError(testEntryNumbersBO.CIQNumberInfo, "This number is the same as another number on this job, please make sure a correct number is entered.");
			testEntryNumbersBO.PreEntryNumber = "PRE";
			testEntryNumbersBO.DeclarationUnifiedNumber = "UNI";
			testEntryNumbersBO.MovementReferenceNumber = "MRN";
			testEntryNumbersBO.CIQNumber = "CIQ";
			testEntryNumbersBO.RunPreSaveValidation();
			AssertHasMessageError(testEntryNumbersBO.PreEntryNumberInfo, "The Customs Office of this job is empty.");
			AssertHasMessageError(testEntryNumbersBO.DeclarationUnifiedNumberInfo, "The Customs Office of this job is empty.");
			AssertHasMessageError(testEntryNumbersBO.MovementReferenceNumberInfo, "The Customs Office of this job is empty.");
			AssertHasMessageError(testEntryNumbersBO.CIQNumberInfo, "The Customs Office of this job is empty.");
			testDeclaration.JE_CustomsOffice = "8765";
			testEntryNumbersBO.RunPreSaveValidation();
			AssertHasMessageError(testEntryNumbersBO.PreEntryNumberInfo, "Pre Entry Number should be 18 digital characters.");
			AssertHasMessageError(testEntryNumbersBO.DeclarationUnifiedNumberInfo, "Declaration Unified Number should be 18 digital characters and start with I or O and then 4 digital year(e.g I20190000123456789)");
			AssertHasMessageError(testEntryNumbersBO.MovementReferenceNumberInfo, "Customs Entry Number should start with 876520181 and followed by a 9-digit sequence number.");
			AssertHasMessageError(testEntryNumbersBO.CIQNumberInfo, "CIQ Number should be 15 digital or 18 digital characters.");
			testEntryNumbersBO.PreEntryNumber = "PREX";
			testEntryNumbersBO.DeclarationUnifiedNumber = "UNIX";
			testEntryNumbersBO.MovementReferenceNumber = "MRNX";
			testEntryNumbersBO.CIQNumber = "CIQX";
			testEntryNumbersBO.RunPreSaveValidation();
			AssertHasError(testEntryNumbersBO.PreEntryNumberInfo, "This number has already been used by another job in the system, please make sure you type in the correct number.");
			AssertHasError(testEntryNumbersBO.DeclarationUnifiedNumberInfo, "This number has already been used by another job in the system, please make sure you type in the correct number.");
			AssertHasError(testEntryNumbersBO.MovementReferenceNumberInfo, "This number has already been used by another job in the system, please make sure you type in the correct number.");
			AssertHasError(testEntryNumbersBO.CIQNumberInfo, "This number has already been used by another job in the system, please make sure you type in the correct number.");
			testEntryNumbersBO.PreEntryNumber = "123456789012345678";
			testEntryNumbersBO.DeclarationUnifiedNumber = "I20180000123456789";
			testEntryNumbersBO.MovementReferenceNumber = "876520181012345680";
			testEntryNumbersBO.CIQNumber = "123456789012345679";
			testEntryNumbersBO.RunPreSaveValidation();
			AssertNoMessageErrors(testEntryNumbersBO.PreEntryNumberInfo);
			AssertNoMessageErrors(testEntryNumbersBO.DeclarationUnifiedNumberInfo);
			AssertNoMessageErrors(testEntryNumbersBO.MovementReferenceNumberInfo);
			AssertNoMessageErrors(testEntryNumbersBO.CIQNumberInfo);
		}

		public void TestEntryNumsAndReadonlys()
		{
			var cusentryheader = Factory.New<CusEntryHeader>();
			var testItem = new EntryNumbersBO(cusentryheader);
			cusentryheader.DeclarationUnifiedNumber = "12";
			cusentryheader.MovementReferenceNumberSetter("34", ZDateTime.Now);
			cusentryheader.PreEntryNumber = "56";
			cusentryheader.CIQNumber = "78";
			Assert("PreEntryNumber should be readonly as CE_EntryIsSystemGenerated=true", testItem.PreEntryNumber_ReadOnly);
			Assert("DeclarationUnifiedNumber should be readonly as CE_EntryIsSystemGenerated=true", testItem.DeclarationUnifiedNumber_ReadOnly);
			Assert("MovementReferenceNumber should be readonly as CE_EntryIsSystemGenerated=true", testItem.MovementReferenceNumber_ReadOnly);
			Assert("MovementReferenceNumberIssueDate should be readonly as CE_EntryIsSystemGenerated=true", testItem.MovementReferenceNumberIssueDate_ReadOnly);
			Assert("CIQNumber should be readonly as CE_EntryIsSystemGenerated=true", testItem.CIQNumber_ReadOnly);
			Assert("PreEntryNumberInfo should be readonly as CE_EntryIsSystemGenerated=true", testItem.PreEntryNumberInfo.ReadOnly);
			Assert("DeclarationUnifiedNumberInfo should be readonly as CE_EntryIsSystemGenerated=true", testItem.DeclarationUnifiedNumberInfo.ReadOnly);
			Assert("MovementReferenceNumberInfo should be readonly as CE_EntryIsSystemGenerated=true", testItem.MovementReferenceNumberInfo.ReadOnly);
			Assert("MovementReferenceNumberIssueDateInfo should be readonly as CE_EntryIsSystemGenerated=true", testItem.MovementReferenceNumberIssueDateInfo.ReadOnly);
			Assert("CIQNumberInfo should be readonly as CE_EntryIsSystemGenerated=true", testItem.CIQNumberInfo.ReadOnly);
			var cusentryheader1 = Factory.New<CusEntryHeader>();
			var testItem1 = new EntryNumbersBO(cusentryheader1);
			testItem1.DeclarationUnifiedNumber = "12";
			testItem1.MovementReferenceNumber = "34";
			testItem1.MovementReferenceNumberIssueDate = ZDateTime.Now;
			testItem1.PreEntryNumber = "56";
			testItem1.CIQNumber = "78";
			testItem1.SetEntryNumbers();
			Assert("PreEntryNumberInfo can edit as CE_EntryIsSystemGenerated=false", !testItem1.PreEntryNumberInfo.ReadOnly);
			Assert("DeclarationUnifiedNumberInfo can edit as CE_EntryIsSystemGenerated=false", !testItem1.DeclarationUnifiedNumberInfo.ReadOnly);
			Assert("MovementReferenceNumberInfo can edit as CE_EntryIsSystemGenerated=false", !testItem1.MovementReferenceNumberInfo.ReadOnly);
			Assert("MovementReferenceNumberIssueDateInfo can edit as CE_EntryIsSystemGenerated=false", !testItem1.MovementReferenceNumberIssueDateInfo.ReadOnly);
			Assert("CIQNumberInfo can edit as CE_EntryIsSystemGenerated=false", !testItem1.CIQNumberInfo.ReadOnly);
		}

		public void TestValidationModeProvider()
		{
			ValidationExtensionsTest.AssertValidationModeProvider(testDeclaration, testEntryNumbersBO.ValidationModeProvider);

			var cusentryheader1 = Factory.New<CusEntryHeader>();
			var testItem1 = new EntryNumbersBO(cusentryheader1);
			AssertNull(testItem1.ValidationModeProvider);
		}

		protected override BusinessObject GetNewBusinessObject() => new EntryNumbersBO(Factory.New<CusEntryHeader>());

		protected override void SetUp()
		{
			base.SetUp();
			testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDeclaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			testCusEntryHeaderExisting = testDeclaration.ActiveEntryHeaders.AddNew();
			Factory.Save();
			testCusEntryHeader = testDeclaration.ActiveEntryHeaders.AddNew();
			testEntryNumbersBO = new EntryNumbersBO(testCusEntryHeader);
		}
		CusEntryHeader testCusEntryHeader;
		CusEntryHeader testCusEntryHeaderExisting;
		JobDeclaration testDeclaration;
		EntryNumbersBO testEntryNumbersBO;
	}
}
