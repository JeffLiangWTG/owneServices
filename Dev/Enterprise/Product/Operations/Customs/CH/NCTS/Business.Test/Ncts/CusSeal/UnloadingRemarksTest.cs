using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(UnloadingRemarks))]
sealed class UnloadingRemarksTest : SingleCusCodeDataTest<UnloadingRemarks>
{
	public void TestSetDefaultValues()
	{
		var unloadingRemarks = Factory.New<UnloadingRemarks>();
		AssertEquals("Default ParentTable", CusSealSchema.Constants.Prefix, unloadingRemarks.CY_ParentTableCode);
		AssertEquals("Default Type", CusCodeDataTypeList.Codes.UnloadingRemarks, unloadingRemarks.CY_Type);
	}

	public void TestValidation()
	{
		var unloadingRemarks = Factory.New<UnloadingRemarks>();
		AssertType<UnloadingRemarksValidation>(unloadingRemarks.Validation);
	}

	protected override IEnumerable<string> GetUsedFieldsNames()
	{
		yield return nameof(CusCodeData.CY_Data);
	}

	UnloadingRemarks CusCodeData => cusCodeData ?? (cusCodeData = GetNewCusCodeData(Factory));
	UnloadingRemarks cusCodeData;

	protected override BusinessObject GetNewBusinessObject() => GetNewCusCodeData(Factory);

	protected override IEnumerable<UnloadingRemarks> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var unloadingRemarks = GetNewCusCodeData(factory);
		unloadingRemarks.CY_Data = "0";
		yield return unloadingRemarks;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();

	protected override UnloadingRemarks GetNewCusCodeData(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		var seal = nctsHeader.ArrivalHeaderContainers.AddNew().Seals.AddNew();
		seal.BK_SealNumber = "X";
		return new UnloadingRemarksCollection(seal).FindOrCreate();
	}
}
