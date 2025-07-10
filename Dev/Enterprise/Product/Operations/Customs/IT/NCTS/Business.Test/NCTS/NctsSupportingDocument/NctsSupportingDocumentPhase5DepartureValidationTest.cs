using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSupportingDocumentPhase5DepartureValidationTest : BaseSupportingDocumentAeoValidationTest
{
	public void TestCheckCSI_ReferenceNumber()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.ReferenceNumber);

		supportingDocumentGoodsItem.CSI_Code = "YYY";
		supportingDocumentGoodsItem.CSI_ReferenceNumber = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Reference Number", supportingDocumentGoodsItem.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_ReferenceNumber = "1234";
		AssertNoMessageErrorContaining("YYY requires Reference Number and it is set", supportingDocumentGoodsItem.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "NNN";
		supportingDocumentGoodsItem.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Reference Number", supportingDocumentGoodsItem.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "UND";
		supportingDocumentGoodsItem.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Reference Number", supportingDocumentGoodsItem.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_ItemNumber()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.ItemNumber);

		supportingDocumentGoodsItem.CSI_Code = "YYY";
		supportingDocumentGoodsItem.CSI_ItemNumber = 1234;
		AssertNoMessageErrorContaining("YYY requires Item Number and it is set", supportingDocumentGoodsItem.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_ItemNumber = 0;
		AssertHasMessageErrorContaining("YYY requires Item Number", supportingDocumentGoodsItem.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_ItemNumber = 1234;
		AssertNoMessageErrorContaining("YYY requires Item Number and it is set", supportingDocumentGoodsItem.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "NNN";
		supportingDocumentGoodsItem.CSI_ItemNumber = 0;
		AssertNoMessageErrorContaining("NNN doesn't require Item Number", supportingDocumentGoodsItem.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "UND";
		supportingDocumentGoodsItem.CSI_ItemNumber = 0;
		AssertNoMessageErrorContaining("UND doesn't require Item Number", supportingDocumentGoodsItem.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_RN_NKCountryCode()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Country);

		supportingDocumentGoodsItem.CSI_Code = "YYY";
		supportingDocumentGoodsItem.CSI_RN_NKCountryCode = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Country Code", supportingDocumentGoodsItem.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_RN_NKCountryCode = "IT";
		AssertNoMessageErrorContaining("YYY requires Country Code and it is set", supportingDocumentGoodsItem.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "NNN";
		supportingDocumentGoodsItem.CSI_RN_NKCountryCode = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Country Code", supportingDocumentGoodsItem.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "UND";
		supportingDocumentGoodsItem.CSI_RN_NKCountryCode = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Country Code", supportingDocumentGoodsItem.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_ReferenceNumber2()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Complement);

		supportingDocumentGoodsItem.CSI_Code = "YYY";
		supportingDocumentGoodsItem.CSI_ReferenceNumber2 = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Reference Number 2", supportingDocumentGoodsItem.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_ReferenceNumber2 = "1234";
		AssertNoMessageErrorContaining("YYY requires Reference Number 2 and it is set", supportingDocumentGoodsItem.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "NNN";
		supportingDocumentGoodsItem.CSI_ReferenceNumber2 = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Reference Number 2", supportingDocumentGoodsItem.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "UND";
		supportingDocumentGoodsItem.CSI_ReferenceNumber2 = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Reference Number 2", supportingDocumentGoodsItem.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_YearOfIssue()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Year);

		supportingDocumentGoodsItem.CSI_Code = "YYY";
		supportingDocumentGoodsItem.CSI_YearOfIssue = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires YearOfIssue", supportingDocumentGoodsItem.CSI_YearOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_YearOfIssue = "2023";
		AssertNoMessageErrorContaining("YYY requires YearOfIssue and it is set", supportingDocumentGoodsItem.CSI_YearOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "NNN";
		supportingDocumentGoodsItem.CSI_YearOfIssue = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require YearOfIssue", supportingDocumentGoodsItem.CSI_YearOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentGoodsItem.CSI_Code = "UND";
		supportingDocumentGoodsItem.CSI_YearOfIssue = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require YearOfIssue", supportingDocumentGoodsItem.CSI_YearOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_ReferenceNumberForHeader()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.ReferenceNumber);

		supportingDocumentHeader.CSI_Code = "YYY";
		supportingDocumentHeader.CSI_ReferenceNumber = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Reference Number", supportingDocumentHeader.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentHeader.CSI_ReferenceNumber = "1234";
		AssertNoMessageErrorContaining("YYY requires Reference Number and it is set", supportingDocumentHeader.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentHeader.CSI_Code = "NNN";
		supportingDocumentHeader.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Reference Number", supportingDocumentHeader.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentHeader.CSI_Code = "UND";
		supportingDocumentHeader.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Reference Number", supportingDocumentHeader.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_ReferenceNumberForHouseBill()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.ReferenceNumber);

		supportingDocumentHouseBill.CSI_Code = "YYY";
		supportingDocumentHouseBill.CSI_ReferenceNumber = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Reference Number", supportingDocumentHouseBill.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentHouseBill.CSI_ReferenceNumber = "1234";
		AssertNoMessageErrorContaining("YYY requires Reference Number and it is set", supportingDocumentHouseBill.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentHouseBill.CSI_Code = "NNN";
		supportingDocumentHouseBill.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Reference Number", supportingDocumentHouseBill.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocumentHouseBill.CSI_Code = "UND";
		supportingDocumentHouseBill.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Reference Number", supportingDocumentHouseBill.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
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

		var cusCodeUND = helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, nctsCodeType, "UND", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		supportingDocumentHeader = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
		var bill = nctsHeader.Bills.AddNew();
		supportingDocumentHouseBill = bill.SupportingDocuments.AddNew();
		goodsItem = bill.GoodsItems.AddNew();
		supportingDocumentGoodsItem = goodsItem.SupportingDocuments.AddNew();
	}
	NctsHeader nctsHeader;
	NctsDepartureCargoDesc goodsItem;
	NctsSupportingDocument supportingDocumentGoodsItem;
	NctsSupportingDocument supportingDocumentHeader;
	NctsSupportingDocument supportingDocumentHouseBill;
}
