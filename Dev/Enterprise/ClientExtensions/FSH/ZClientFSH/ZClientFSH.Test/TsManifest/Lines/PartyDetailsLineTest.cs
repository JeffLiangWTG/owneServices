namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	public class PartyDetailsLineTest : BaseLineTest
	{
		public void TestProperties()
		{
			AssertEquals("AddressCode", "SCHENA", ConsigneeDetails.AddressCode);
			AssertEquals("Address1", "SCHENKER & CO (AUSTR.) PTY. LTD.", ConsigneeDetails.Address1);
			AssertEquals("Address2", "5 FREDERICK ROAD", ConsigneeDetails.Address2);
			AssertEquals("Address3", "ROYAL PARK (ADELAIDE)SA 5014", ConsigneeDetails.Address3);
			AssertEquals("Address4", "P.O.BOX 166", ConsigneeDetails.Address4);
			AssertEquals("Address5", "AUS-PORT ADELAIDE,SA 5014", ConsigneeDetails.Address5);
			AssertNotNull(ConsigneeDetails.OceanBill);
		}
	}
}
