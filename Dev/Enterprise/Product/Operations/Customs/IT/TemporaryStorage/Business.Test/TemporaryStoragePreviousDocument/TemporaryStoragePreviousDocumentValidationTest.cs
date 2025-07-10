using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStoragePreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumber()
	{
		const string shouldHaveSameReferenceNumberMessage = "All lines should have the same Reference Number for Previous Document.";

		previousDocument.CSI_ReferenceNumber = "REF1";
		var previousDocument2 = storageBill.PreviousDocuments.AddNew();
		previousDocument2.CSI_ReferenceNumber = "REF2";
		AssertHasWarning("When Reference Numbers are inconsistent in previous document", previousDocument2.CSI_ReferenceNumberInfo, shouldHaveSameReferenceNumberMessage);

		previousDocument2.CSI_ReferenceNumber = "REF1";
		AssertNoWarning("When Reference Numbers are consistent in previous document", previousDocument2.CSI_ReferenceNumberInfo, shouldHaveSameReferenceNumberMessage);

		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_ReferenceNumberInfo);
	}

	public void TestCheckCSI_Code()
	{
		const string shouldHaveSameTypeMessage = "All lines should have the same Type for Previous Document.";

		previousDocument.CSI_Code = "CODE1";
		var previousDocument2 = storageBill.PreviousDocuments.AddNew();
		previousDocument2.CSI_Code = "CODE2";
		AssertHasWarning("When Types are inconsistent in previous document", previousDocument2.CSI_CodeInfo, shouldHaveSameTypeMessage);

		previousDocument2.CSI_Code = "CODE1";
		AssertNoWarning("When Types are consistent in previous document", previousDocument2.CSI_CodeInfo, shouldHaveSameTypeMessage);
	}

	public void TestCheckCSI_LineNo()
	{
		const string warningMessage = "All lines should have a different Goods Item Identifier for Previous Document.";

		previousDocument.CSI_LineNo = 1;
		var previousDocument2 = storageBill.PreviousDocuments.AddNew();
		previousDocument2.CSI_LineNo = 2;
		AssertNoWarning("When Identifier is unique", previousDocument2.CSI_LineNoInfo, warningMessage);

		previousDocument2.CSI_LineNo = 1;
		AssertHasWarning("When duplicate Identifiers exist", previousDocument2.CSI_LineNoInfo, warningMessage);
	}

	public void TestCheckCSI_PackType()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "UnitedNationsRecommendations", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UNPKG");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1A", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

		Factory.Save();

		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(previousDocument.CSI_PackTypeInfo, previousDocument.CSI_PackQtyInfo);
		ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_PackTypeInfo, "XX", "1A");
	}

	public void TestCheckCSI_UnitOfQuantity()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "UnitedNationsRecommendations", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CUSUQ");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(previousDocument.CSI_UnitOfQuantityInfo, previousDocument.CSI_QuantityInfo);
		ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_UnitOfQuantityInfo, "GRM", "KGM");
	}

	public void TestCheckRuleBR_PN_TS_053()
	{
		var bill = Factory.New<TemporaryStorageBill>();
		var packedItem = bill.PackedItems.AddNew();

		var previousDocumentAtPackedItem = packedItem.PreviousDocuments.AddNew();
		var previousDocumentAtBill = bill.PreviousDocuments.AddNew();

		previousDocumentAtBill.Validation.ValidateAll();
		AssertNoRowMessageError(previousDocumentAtBill, "The Previous Document should be entered at Bill level or Bill Item level, not both.");

		previousDocumentAtPackedItem.Validation.ValidateAll();
		AssertNoRowMessageError(previousDocumentAtPackedItem, "The Previous Document should be entered at Bill level or Bill Item level, not both.");
	}

	protected override void SetUp()
	{
		base.SetUp();
		storageHeader = Factory.New<TemporaryStorageHeader>();
		storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
		storageBill = storageHeader.Bills.AddNew();
		previousDocument = storageBill.PreviousDocuments.AddNew();
	}

	TemporaryStorageHeader storageHeader;
	TemporaryStoragePreviousDocument previousDocument;
	TemporaryStorageBill storageBill;
}
