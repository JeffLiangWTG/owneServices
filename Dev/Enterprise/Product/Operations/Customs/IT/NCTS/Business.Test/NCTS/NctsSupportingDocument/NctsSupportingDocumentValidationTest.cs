using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSupportingDocumentAeoValidationTest : BaseSupportingDocumentAeoValidationTest
{
	public void TestCheckAEOCertificate_AgainstSupplier()
	{
		var consignor = NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "CO1", nctsHeader.Consignor, "2");
		goodsItem.SupportingDocuments.RemoveAndDeleteAll();

		AssertAeoCertificateValidation(goodsItem.SupportingDocuments, consignor, "Consignor", IT.Business.UniversalReferenceConstants.SupportingDocumentTypes.Y022);
	}

	public void TestCheckAEOCertificate_AgainstImporter()
	{
		var consignee = NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "CE1", nctsHeader.Consignee, suffix: "3");
		goodsItem.SupportingDocuments.RemoveAndDeleteAll();

		AssertAeoCertificateValidation(goodsItem.SupportingDocuments, consignee, "Consignee", IT.Business.UniversalReferenceConstants.SupportingDocumentTypes.Y023);
	}

	public void TestCheckAEOCertificate_AgainstDeclarant()
	{
		var declarant = Factory.New<OrgHeader>();
		nctsHeader.DeclarantAddressPK = declarant.MainAddress.PK;

		goodsItem.SupportingDocuments.RemoveAndDeleteAll();

		AssertAeoCertificateValidation(goodsItem.SupportingDocuments, declarant, "Declarant", IT.Business.UniversalReferenceConstants.SupportingDocumentTypes.Y024);
	}

	public void TestCheckCSI_DateOfIssueIsValidZDateRange()
	{
		var supportingDocument = goodsItem.SupportingDocuments.AddNew();

		supportingDocument.CSI_DateOfIssue = ZDateTime.Today.AddYears(-11);
		AssertNoErrors(supportingDocument.CSI_DateOfIssueInfo);
		AssertHasWarningContaining(supportingDocument.CSI_DateOfIssueInfo, "is more than 1 year old");
	}

	public void TestCheckCSI_RN_NKCountryCode_MandatoryValidation()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Country);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_RN_NKCountryCode = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Country Code", supportingDocument.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_RN_NKCountryCode = "IT";
		AssertNoMessageErrorContaining("YYY requires Country Code and it is set", supportingDocument.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_RN_NKCountryCode = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Country Code", supportingDocument.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_RN_NKCountryCode = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Country Code", supportingDocument.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_RN_NKCountryCode()
	{
		supportingDocument.CSI_RN_NKCountryCode = ZString.Empty;
		AssertNoNotifications("No notifications when code is empty", supportingDocument.CSI_RN_NKCountryCodeInfo);
		supportingDocument.CSI_RN_NKCountryCode = "#@";
		AssertHasMessageErrorContaining(supportingDocument.CSI_RN_NKCountryCodeInfo, ListValidation.InvalidCodeMessageError);
		supportingDocument.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		AssertNoNotifications("No notifications when selected code is in the list", supportingDocument.CSI_RN_NKCountryCodeInfo);
	}

	public void TestCheckCSI_DateOfIssue()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Year);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
		AssertHasMessageErrorContaining("YYY requires Date Of Issue", supportingDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_DateOfIssue = ZDateTime.Now;
		AssertNoMessageErrorContaining("YYY requires Date Of Issue and it is set", supportingDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Date Of Issue", supportingDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Date Of Issue", supportingDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_ReferenceNumber()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.ReferenceNumber);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_ReferenceNumber = ZString.Empty;
		supportingDocument.Validation.ValidateCSI_ReferenceNumber();
		AssertHasMessageErrorContaining("YYY requires Reference Number", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_ReferenceNumber = "1234";
		AssertNoMessageErrorContaining("YYY requires Reference Number and it is set", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Reference Number", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Reference Number", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_Quantity()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Quantity);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_Quantity = ZDecimal.Zero;
		AssertHasMessageErrorContaining("YYY requires Quantity", supportingDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Quantity = 1m;
		AssertNoMessageErrorContaining("YYY requires Quantity and it is set", supportingDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_Quantity = ZDecimal.Zero;
		AssertNoMessageErrorContaining("NNN doesn't require Quantity", supportingDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_Quantity = ZDecimal.Zero;
		AssertNoMessageErrorContaining("UND doesn't require Quantity", supportingDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_UnitOfQuantity()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.UnitOfQuantity);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_UnitOfQuantity = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Unit Of Quantity", supportingDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_UnitOfQuantity = "KG";
		AssertNoMessageErrorContaining("YYY requires Unit Of Quantity and it is set", supportingDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_UnitOfQuantity = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Unit Of Quantity", supportingDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_UnitOfQuantity = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Unit Of Quantity", supportingDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);
	}

	#region Implementation

	void SetUpRefCusCodesForAttributeName(ZString attributeName)
	{
		var nctsCodeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS;

		var helper = new UniversalReferenceTestDataHelper(Factory);

		var dataGroupingEU = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		var dataGroupingIT = helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", dataGroupingEU);

		helper.CreateNewOrGetExistingCusCodeType(nctsCodeType, "Ncts Supporting Document Type");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, nctsCodeType, dataGroupingIT.ZZZ_DataGrouping);

		var cusCodeYYY = helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, nctsCodeType, "YYY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		cusCodeYYY.Attributes.AddNew(attributeName, "Y");

		var cusCodeNNN = helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, nctsCodeType, "NNN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		cusCodeNNN.Attributes.AddNew(attributeName, "N");

		helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, nctsCodeType, "UND", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		supportingDocument = goodsItem.SupportingDocuments.AddNew();
	}
	NctsHeader nctsHeader;
	NctsDepartureCargoDesc goodsItem;
	NctsSupportingDocument supportingDocument;
}
