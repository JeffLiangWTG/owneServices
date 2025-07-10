using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDeparturePayInfoTypeDeciderTest : TestCaseWithFactory
{
	public void TestAddNewAndLoadFromDatabase()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var departureMoveHeader = nctsHeader.MovementHeader;
		AssertType<NctsDeparturePayInfo>("Brand new PayInfo Type", departureMoveHeader.PayInfoCollection.AddNew());
		Factory.Save();

		var nctsHeaderOnSeparateFactory = new BusinessObjectFactory().Load<NctsHeader>(nctsHeader.PK);
		var departureMoveHeaderOnSeparateFactory = nctsHeaderOnSeparateFactory.MovementHeader;
		AssertEquals("PayInfoCollection Count", 1, departureMoveHeaderOnSeparateFactory.PayInfoCollection.Count);
		AssertType<NctsDeparturePayInfo>("Loaded PayInfo Type", departureMoveHeaderOnSeparateFactory.PayInfoCollection[0]);
	}
}
