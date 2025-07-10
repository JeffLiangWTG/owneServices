using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRSEACRRMessageTestCase : TestCaseWithFactory
	{
		public void TestGetReferenceFromSendersReference()
		{
			CreateTestShipmentInFactory();
			CMRSEACRRMessage message = Factory.New<CMRSEACRRMessage>();
			message.EM_MessageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEACRR+1AI4 918G D215:001+11'
NAD+MR+FGE973N::95'
RFF+ACW:SEACR'
RFF+AFM:9'
RFF+ABO:42243-60087/SYD3::009'
DTM+310:20051011062214:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");

			AssertNotNull("Failed to get shipment with customs Shipment number", message.GetWrappedObject());
		}

		#region Implementation

		protected void CreateTestShipmentInFactory()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CusSCAOceanBill oceanBill = factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBillToFind = oceanBill.HouseBills.AddNew();
			houseBillToFind.CA_BGMReference = "42243-60087";
			factory.Save();
		}

		#endregion
	}
}
