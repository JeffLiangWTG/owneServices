using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(Seal))]
	public class SealTest : Customs.Business.Testing.CusCodeDataTest<Seal>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.Seal, seal.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.Seal, seal.CY_Code);
		}

		public void TestGetNewLookups()
		{
			AssertType<CusCodeDataLookups>(seal.Lookups);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.Seals.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			seal = header.Seals.AddNew();
		}
		Seal seal;
	}
}
