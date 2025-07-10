using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(HouseConsignmentDifferences))]
sealed class HouseConsignmentDifferencesTest : SingleCusCodeDataTest<HouseConsignmentDifferences>
{
	public void TestSetDefaultValues() => CombineAssertions(() =>
	{
		AssertEquals(CusInBondBillSchema.Constants.Prefix, CusCodeData.CY_ParentTableCode);
		AssertEquals(CH.Business.CusCodeDataTypeList.Codes.UnloadingRemarks, CusCodeData.CY_Type);
	});

	public void TestLookups() => AssertType<HouseConsignmentDifferencesLookups>(CusCodeData.Lookups);

	public void TestValidation() => AssertType<HouseConsignmentDifferencesValidation>(CusCodeData.Validation);

	public void TestCY_Code_MaxLength() => AssertEquals("MaxLength", 2, CusCodeData.CY_CodeInfo.MaxLength);

	public void TestCY_Code_ReadOnly() => AssertReadOnly(CusCodeData.CY_CodeInfo);

	public void TestCY_Data_MaxLength() => AssertEquals("MaxLength", 512, CusCodeData.CY_DataInfo.MaxLength);

	public void TestCY_Data_ReadOnly() => AssertReadOnly(CusCodeData.CY_DataInfo);

	void AssertReadOnly(ZPropertyInfo property) => CombineAssertions(() =>
	{
		CusCodeData.Parent.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={CusCodeData.Parent.MovementDetail.B9_UnloadedState}", true, property.ReadOnly);

		CusCodeData.Parent.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={CusCodeData.Parent.MovementDetail.B9_UnloadedState}", true, property.ReadOnly);

		CusCodeData.Parent.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={CusCodeData.Parent.MovementDetail.B9_UnloadedState}", false, property.ReadOnly);

		CusCodeData.Parent.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals($"{property.HasHumanReadableName} BY_UnloadedState={CusCodeData.Parent.MovementDetail.B9_UnloadedState}", true, property.ReadOnly);
	});

	protected override BusinessObject GetNewBusinessObject() => GetNewCusCodeData(Factory);

	protected override IEnumerable<HouseConsignmentDifferences> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var houseConsignmentDifferences = GetNewCusCodeData(factory);
		houseConsignmentDifferences.CY_Code = UnloadingRemarkCodeList.Codes.Unknown;
		yield return houseConsignmentDifferences;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();

	protected override HouseConsignmentDifferences GetNewCusCodeData(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.Bills.AddNew().HouseConsignmentDifference;
	}

	protected override IEnumerable<string> GetUsedFieldsNames()
	{
		yield return nameof(CusCodeData.CY_Code);
		yield return nameof(CusCodeData.CY_Data);
	}

	HouseConsignmentDifferences CusCodeData => cusCodeData ?? (cusCodeData = GetNewCusCodeData(Factory));
	HouseConsignmentDifferences cusCodeData;
}
