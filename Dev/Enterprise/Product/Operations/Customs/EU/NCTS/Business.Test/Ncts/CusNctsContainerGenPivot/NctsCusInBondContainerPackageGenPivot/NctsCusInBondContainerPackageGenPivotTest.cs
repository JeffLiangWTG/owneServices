using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCusInBondContainerPackageGenPivot))]
	sealed class NctsCusInBondContainerPackageGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRelation1Object()
		{
			(_, _, var package) = SetupData();
			var pivot = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot.Relation1Object = package;
			AssertSame(package, pivot.Relation1Object);
		}

		public void TestContainer()
		{
			(_, var headerContainer, var package) = SetupData();
			var pivot = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			pivot.Relation1Object = package;
			pivot.Relation2Object = headerContainer;
			AssertSame(headerContainer, pivot.Container);
		}

		public void TestSetDefaultValues()
		{
			var pivot = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			AssertEquals("pivot.XX_RelationType", GenPivotTypeDecider.Types.CusNctsContainer, pivot.XX_RelationType);
			AssertEquals("pivot.XX_Relation1TableCode", CusInvPackSchema.Constants.Prefix, pivot.XX_Relation1TableCode);
			AssertEquals("pivot.XX_Relation2TableCode", CusInBondContainerSchema.Constants.Prefix, pivot.XX_Relation2TableCode);
		}

		(NctsHeader header, NctsDepartureHeaderContainer headerContainer, NctsPackage package) SetupData()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var headerContainer = header.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			return (header, headerContainer, package);
		}
	}
}
