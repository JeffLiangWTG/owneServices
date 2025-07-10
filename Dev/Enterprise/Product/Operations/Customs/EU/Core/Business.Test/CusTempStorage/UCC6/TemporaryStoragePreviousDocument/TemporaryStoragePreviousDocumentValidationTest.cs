using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing.CusTempStorage
{
	class TemporaryStoragePreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		TemporaryStorageHeader storageHeader;
		TemporaryStoragePreviousDocument previousDocument;
		TemporaryStoragePreviousDocument previousDocumentBill;
		TemporaryStorageBill storageBill;

		public void TestCheckCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			helper.CreateCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS,
				"Previous Documents Of PNTS");
			helper.CreateCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment,
				"Transport Charges Method Of Payment");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Purpose", "Purpose",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var codeLists1 = helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS,
				PreviousDocumentCodeList.Codes.N355,
				"ENS",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			codeLists1.Attributes.AddNew("Purpose", "Declaration");
			codeLists1.Attributes.AddNew("Purpose", "Presentation");

			var codeLists2 = helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS,
				"N300",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			codeLists2.Attributes.AddNew("Purpose", "Declaration");

			Factory.Save();

			storageBill.PreviousDocuments.Clear();
			previousDocument.CSI_Code = ZString.Empty;
			AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			storageHeader.IsENSReuse = true;
			previousDocument.CSI_Code = ZString.Empty;
			AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_Code = "N302";
			AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(previousDocument.CSI_CodeInfo, "Previous document type must be 'N355'.");
			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.N355;
			AssertNoMessageErrors(previousDocument.CSI_CodeInfo);
		}

		public void TestValidationPreviousDocumentWhenIsReused()
		{
			var messageError = "As Previous Document in Bill Items are identical, it must be sent at Bill level. Previous Document in Bill Item tab or Header should be empty.";
			var header = Factory.New<TemporaryStorageHeader>();
			header.IsENSReuse = true;
			var bill = header.Bills.AddNew();

			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();

			var prev1_1 = item1.PreviousDocuments.AddNew();
			var prev1_2 = item1.PreviousDocuments.AddNew();

			var prev2_1 = item2.PreviousDocuments.AddNew();
			var prev2_2 = item2.PreviousDocuments.AddNew();

			prev1_1.CSI_Code = "CODE11";
			prev1_1.CSI_ReferenceNumber = "ref11";
			prev1_2.CSI_Code = "CODE12";
			prev1_2.CSI_ReferenceNumber = "ref12";

			prev2_1.CSI_Code = "CODE11";
			prev2_1.CSI_ReferenceNumber = "ref11";
			prev2_2.CSI_Code = "CODE12";
			prev2_2.CSI_ReferenceNumber = "ref12";

			prev1_1.Validation.ValidateAll();
			prev1_2.Validation.ValidateAll();
			prev2_1.Validation.ValidateAll();
			prev2_2.Validation.ValidateAll();

			AssertHasRowMessageErrorContaining(prev1_1, messageError);
			AssertHasRowMessageErrorContaining(prev1_2, messageError);
			AssertHasRowMessageErrorContaining(prev2_1, messageError);
			AssertHasRowMessageErrorContaining(prev2_2, messageError);

			prev2_1.CSI_Code = "CODE33";

			prev1_1.Validation.ValidateAll();
			prev1_2.Validation.ValidateAll();
			prev2_1.Validation.ValidateAll();
			prev2_2.Validation.ValidateAll();

			AssertNoRowMessageErrorContaining(prev1_1, messageError);
			AssertNoRowMessageErrorContaining(prev1_2, messageError);
			AssertNoRowMessageErrorContaining(prev2_1, messageError);
			AssertNoRowMessageErrorContaining(prev2_2, messageError);
		}

		public void TestCheckCSI_Code_PreviousDocumentExistsInBothHeaderAndBill()
		{
			var message = "Previous document can only be provided at Header or Bills level, not at both.";
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var previousDocument1 = header.PreviousDocuments.AddNew();

			previousDocument1.CSI_Code = "N355";
			AssertNoMessageError("Previous document is only provided at Header level", previousDocument1.CSI_CodeInfo, message);

			var previousDocument2 = bill.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "N355";
			previousDocument1.Validation.ValidateCSI_Code();
			AssertHasMessageError("Previous document is provided at both Header and Bills level", previousDocument1.CSI_CodeInfo, message);
			AssertHasMessageError("Previous document is provided at both Header and Bills level", previousDocument2.CSI_CodeInfo, message);

			previousDocument2.CSI_Code = "N821";
			previousDocument1.Validation.ValidateCSI_Code();
			AssertHasMessageError("Previous document is provided at both Header and Bills level", previousDocument1.CSI_CodeInfo, message);
			AssertHasMessageError("Previous document is provided at both Header and Bills level", previousDocument2.CSI_CodeInfo, message);

			previousDocument1.Delete();
			previousDocument2.Validation.ValidateCSI_Code();
			AssertNoMessageError("Previous document is only provided at Bills level", previousDocument2.CSI_CodeInfo, message);
		}

		public void TestCheckCSI_Code_PreviousDocumentExistsInBothHeaderAndBillItem()
		{
			var message = "Previous document can only be provided at Header or Bill Item level, not at both.";
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var previousDocument1 = header.PreviousDocuments.AddNew();

			previousDocument1.CSI_Code = "N355";
			AssertNoMessageError("Previous document is only provided at Header level", previousDocument1.CSI_CodeInfo, message);

			var previousDocument2 = packedItem.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "N355";
			previousDocument1.Validation.ValidateCSI_Code();
			AssertHasMessageError("Previous document is provided at both Header and Bill Item level", previousDocument1.CSI_CodeInfo, message);
			AssertHasMessageError("Previous document is provided at both Header and Bill Item level", previousDocument2.CSI_CodeInfo, message);

			previousDocument2.CSI_Code = "N821";
			previousDocument1.Validation.ValidateCSI_Code();
			AssertHasMessageError("Previous document is provided at both Header and Bill Item level", previousDocument1.CSI_CodeInfo, message);
			AssertHasMessageError("Previous document is provided at both Header and Bill Item level", previousDocument2.CSI_CodeInfo, message);

			previousDocument1.Delete();
			previousDocument2.Validation.ValidateCSI_Code();
			AssertNoMessageError("Previous document is only provided at Bill Item level", previousDocument2.CSI_CodeInfo, message);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS,
				PreviousDocumentCodeList.Codes.N355,
				"ENS",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			storageHeader.IsENSReuse = true;
			var storageBill2 = storageHeader.Bills.AddNew();
			var previousDocument2 = storageBill2.PreviousDocuments.AddNew();
			previousDocument2.CSI_ReferenceNumber = "1234567891234567T9";
			previousDocument2.CSI_Code = PreviousDocumentCodeList.Codes.N355;

			previousDocumentBill.CSI_Code = PreviousDocumentCodeList.Codes.N355;
			previousDocumentBill.CSI_ReferenceNumber = "0234567891234567T9";
			AssertHasMessageError(previousDocumentBill.CSI_ReferenceNumberInfo, "Reference Number must be the same as that of other previous documents under the master.");
			previousDocumentBill.CSI_ReferenceNumber = "1234567891234567T9";
			AssertNoMessageError(previousDocumentBill.CSI_ReferenceNumberInfo, "Reference Number must be the same as that of other previous documents under the master.");
		}

		public void TestRuleBR_PN_TS_053_TemporaryStorageBill()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			bill.PreviousDocuments.AddNew();

			var previousDocument = packedItem.PreviousDocuments.AddNew();
			previousDocument.Validation.ValidateAll();
			AssertHasRowMessageError("It is prohibited to add previous document when bill already has at least one", previousDocument, "The Previous Document should be entered at Bill level or Bill Item level, not both.");

			bill.PreviousDocuments.RemoveAndDeleteAll();
			previousDocument.Validation.ValidateAll();
			AssertNoRowMessageError("It is allowed to add previous document when bill has no one", previousDocument, "The Previous Document should be entered at Bill level or Bill Item level, not both.");
		}

		public void TestRuleBR_PN_TS_053_TemporaryStoragePackedItem()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.PreviousDocuments.AddNew();

			var previousDocument = bill.PreviousDocuments.AddNew();
			previousDocument.Validation.ValidateAll();
			AssertHasRowMessageError("It is prohibited to add previous document when packed item already has at least one", previousDocument, "The Previous Document should be entered at Bill level or Bill Item level, not both.");

			packedItem.PreviousDocuments.RemoveAndDeleteAll();
			previousDocument.Validation.ValidateAll();
			AssertNoRowMessageError("It is allowed to add previous document when packed item has no one", previousDocument, "The Previous Document should be entered at Bill level or Bill Item level, not both.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			previousDocument = storageHeader.PreviousDocuments.AddNew();
			storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			storageBill = storageHeader.Bills.AddNew();
			previousDocumentBill = storageBill.PreviousDocuments.AddNew();
		}
	}
}
