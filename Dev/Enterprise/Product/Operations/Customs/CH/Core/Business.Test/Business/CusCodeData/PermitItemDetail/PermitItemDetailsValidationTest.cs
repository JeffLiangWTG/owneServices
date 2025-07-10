using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class PermitItemDetailsValidationTest : BusinessObjectValidationTestCase
{
	public void TestCY_Code_unique()
	{
		const string uniqueMessageError = "A key may be entered only once.";

		var permit = Factory.New<Permit>();
		var permitItemDetail1 = permit.PermitItemDetails.AddNew();
		permitItemDetail1.CY_Code = PermitItemDetailKeyList.Codes.Key1;
		var permitItemDetail2 = permit.PermitItemDetails.AddNew();
		permitItemDetail2.CY_Code = PermitItemDetailKeyList.Codes.Key2;
		AssertNoMessageError("with unique keys (1st)", permitItemDetail1.CY_CodeInfo, uniqueMessageError);
		AssertNoMessageError("with unique keys (2nd)", permitItemDetail1.CY_CodeInfo, uniqueMessageError);

		var permitItemDetail3 = permit.PermitItemDetails.AddNew();
		permitItemDetail3.CY_Code = PermitItemDetailKeyList.Codes.Key1;
		AssertHasMessageError("with ambigous keys", permitItemDetail3.CY_CodeInfo, uniqueMessageError);
	}

	public void TestCY_Code_siblingsValidated()
	{
		const string uniqueMessageError = "A key may be entered only once.";

		var permit = Factory.New<Permit>();
		var permitItemDetail1 = permit.PermitItemDetails.AddNew();
		var permitItemDetail2 = permit.PermitItemDetails.AddNew();

		CombineAssertions(() =>
		{
			permitItemDetail1.CY_Code = PermitItemDetailKeyList.Codes.Key1;
			permitItemDetail2.CY_Code = PermitItemDetailKeyList.Codes.Key1;
			AssertHasMessageError("error on sibling", permitItemDetail1.CY_CodeInfo, uniqueMessageError);
			permitItemDetail2.CY_Code = PermitItemDetailKeyList.Codes.Key2;
			AssertNoMessageError("no error on sibling", permitItemDetail1.CY_CodeInfo, uniqueMessageError);
		});
	}

	public void TestCY_Data_Key1()
	{
		const string invalidMessage = "For key 1 the value must be a whole number.";

		PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key1;

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(PermitItemDetail.CY_DataInfo);
			PermitItemDetail.CY_Data = "1.2";
			AssertHasMessageError("Non-integer value", PermitItemDetail.CY_DataInfo, invalidMessage);
			PermitItemDetail.CY_Data = "12";
			AssertNoMessageError("Valid value", PermitItemDetail.CY_DataInfo, invalidMessage);
		});
	}

	public void TestCY_Data_Key2()
	{
		const string invalidMessage = "For key 2 the value must be a quantity.";

		PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key2;

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(PermitItemDetail.CY_DataInfo);
			PermitItemDetail.CY_Data = "12x";
			AssertHasMessageError("Non-quantity value", PermitItemDetail.CY_DataInfo, invalidMessage);
			PermitItemDetail.CY_Data = "12";
			AssertNoMessageError("Valid value", PermitItemDetail.CY_DataInfo, invalidMessage);
		});
	}

	public void TestCY_Data_Key3()
	{
		RefCusCodeTestHelper.CreateCITESCommodityTypeList(Factory);
		PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key3;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(PermitItemDetail.CY_DataInfo, RefCusCodeTestHelper.InvalidCITESCommodityTypeListCode, RefCusCodeTestHelper.ValidCITESCommodityTypeListCode);
	}

	public void TestCY_Data_Key4()
	{
		RefCusCodeTestHelper.CreateCITESScientificNameList(Factory);
		PermitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key4;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(PermitItemDetail.CY_DataInfo, RefCusCodeTestHelper.InvalidCITESScientificNameListCode, RefCusCodeTestHelper.ValidCITESScientificNameListCode);
	}

	public void TestCY_Data_NotKey1To4()
	{
		PermitItemDetail.CY_Code = "9";
		PermitItemDetail.CY_Data = ZString.Empty;
		AssertHasMessageError(PermitItemDetail.CY_DataInfo, "You have not entered a Value.");
	}

	PermitItemDetail PermitItemDetail => permitItemDetail ?? (permitItemDetail = Factory.New<PermitItemDetail>());
	PermitItemDetail permitItemDetail;
}
