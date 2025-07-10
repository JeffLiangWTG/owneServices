using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAddress()
		{
			AssertType<CusGoodsLocationAddress>(cusGoodsLocation.Address);
		}

		public void TestLookups()
		{
			AssertType<CusGoodsLocationLookups>(cusGoodsLocation.Lookups);
		}

		public void TestCustomSOfficeRequirementRules()
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			cusGoodsLocation.Validation.ValidateAll();
			AssertEquals("Rule C0062 abandoned.", false, cusGoodsLocation.CGL_CustomsOfficeInfo.Notifications.Any(notification => notification.Message.Contains("C0062")));
		}

		protected override BusinessObject GetNewBusinessObject() => cusGoodsLocation;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => cusGoodsLocation;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader.GoodsLocation;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			cusGoodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		}

		CusGoodsLocation cusGoodsLocation;
	}
}
