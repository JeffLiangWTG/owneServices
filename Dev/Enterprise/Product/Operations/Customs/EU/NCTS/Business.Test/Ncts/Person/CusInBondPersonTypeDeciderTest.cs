using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class CusInBondPersonTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var header = Factory.New<NctsHeader>();
			var cusInBondPerson = Customs.Business.CusInBondPerson.LoadOrCreate<CusInBondPerson>(header, CusInBondPerson.LocationContactType);
			var row = ((INeedRow)cusInBondPerson).Row;

			var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals(typeof(CusInBondPerson), typeForLoad);
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new CusInBondPersonTypeDecider();
		}
		CusInBondPersonTypeDecider typeDecider;
	}
}
