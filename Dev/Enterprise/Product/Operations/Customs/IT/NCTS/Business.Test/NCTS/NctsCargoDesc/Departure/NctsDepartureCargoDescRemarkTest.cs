using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescRemarkTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsDepartureCargoDescRemark(null));
	}

	public void TestRemarks()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		var nctsDepartureCargoDescRemark = new NctsDepartureCargoDescRemark(goodsItem);
		nctsDepartureCargoDescRemark.Remarks = "AAA";

		var cusRemGettingQuery = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, goodsItem.PK)
			.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, goodsItem.TablePrefix)
			.AddToFilter(CusSupportingInfoSchema.CSI_Type, "REM");

		var remarksCusSupInfo = Factory.LoadTop1<CusSupportingInfo>(cusRemGettingQuery);
		AssertNotNull("The remarks value should be stored in a CusSupportingInfo [REM]", remarksCusSupInfo);
		AssertEquals("CSI_Description", "AAA", remarksCusSupInfo.CSI_Description);

		nctsDepartureCargoDescRemark.Remarks = "";
		remarksCusSupInfo = Factory.LoadTop1<CusSupportingInfo>(cusRemGettingQuery);
		AssertNull("When remarks is empty, the CusSupportingInfo [REM] should be deleted", remarksCusSupInfo);
	}
}
