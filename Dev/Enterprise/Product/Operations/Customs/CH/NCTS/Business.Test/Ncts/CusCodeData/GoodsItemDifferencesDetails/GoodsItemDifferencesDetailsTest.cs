using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(GoodsItemDifferencesDetails))]
sealed class GoodsItemDifferencesDetailsTest : SingleCusCodeDataTest<GoodsItemDifferencesDetails>
{
	public void TestSetDefaultValues() => CombineAssertions(() =>
	{
		AssertEquals(CusInBondCargoDescSchema.Constants.Prefix, CusCodeData.CY_ParentTableCode);
		AssertEquals(CH.Business.CusCodeDataTypeList.Codes.UnloadingRemarks, CusCodeData.CY_Type);
	});

	public void TestLookups() => AssertType<GoodsItemDifferencesDetailsLookups>(CusCodeData.Lookups);

	public void TestValidation() => AssertType<GoodsItemDifferencesDetailsValidation>(CusCodeData.Validation);

	public void TestCY_CodeMaxLength() => AssertEquals("MaxLength", 2, CusCodeData.CY_CodeInfo.MaxLength);

	public void TestCY_CodeReadOnly() => AssertReadOnly(CusCodeData.CY_CodeInfo);

	public void TestCY_DataMaxLength() => AssertEquals("MaxLength", 512, CusCodeData.CY_DataInfo.MaxLength);

	public void TestCY_DataReadOnly() => AssertReadOnly(CusCodeData.CY_DataInfo);

	void AssertReadOnly(ZPropertyInfo property) => CombineAssertions(() =>
	{
		CusCodeData.Parent.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={CusCodeData.Parent.BY_UnloadedState}", true, property.ReadOnly);

		CusCodeData.Parent.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={CusCodeData.Parent.BY_UnloadedState}", false, property.ReadOnly);

		CusCodeData.Parent.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={CusCodeData.Parent.BY_UnloadedState}", false, property.ReadOnly);

		CusCodeData.Parent.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={CusCodeData.Parent.BY_UnloadedState}", false, property.ReadOnly);
	});

	GoodsItemDifferencesDetails CusCodeData => cusCodeData ?? (cusCodeData = GetNewCusCodeData(Factory));
	GoodsItemDifferencesDetails cusCodeData;

	protected override BusinessObject GetNewBusinessObject() => GetNewCusCodeData(Factory);

	protected override IEnumerable<GoodsItemDifferencesDetails> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var goodsItemDifferencesDetails = GetNewCusCodeData(factory);
		goodsItemDifferencesDetails.CY_Code = UnloadingRemarkCodeList.Codes.Unknown;
		yield return goodsItemDifferencesDetails;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();

	protected override GoodsItemDifferencesDetails GetNewCusCodeData(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalGoodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		return arrivalGoodsItem.GoodsItemDifferencesDetail;
	}

	protected override IEnumerable<string> GetUsedFieldsNames()
	{
		yield return nameof(CusCodeData.CY_Code);
		yield return nameof(CusCodeData.CY_Data);
	}
}
