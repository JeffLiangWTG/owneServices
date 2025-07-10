using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseConcurrencyTest : TestCaseWithFactory
	{
		public void TestCusSCAHouseConcurrencyErrorsStopSave()
		{
			Factory.RefreshEnabled = false;
			CusSCAHouse houseFactory1 = Factory.NewWithValidTestData<CusSCAHouse>();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			CusSCAHouse houseFactory2 = factory2.Load<CusSCAHouse>(houseFactory1.PK);
			houseFactory1.CA_MessageStatus = "ACO";
			Factory.Save();
			houseFactory2.CA_MessageStatus = "WTO";
			try
			{
				factory2.Save();
				Fail("Should throw exception");
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				ErrorReporter.Clear();
			}
			try
			{
				factory2.Save();
				Fail("Should throw exception again");
			}
			catch (ZSaveConcurrencyException)
			{
				Assert(true);
				ErrorReporter.Clear();
			}
		}
	}
}
