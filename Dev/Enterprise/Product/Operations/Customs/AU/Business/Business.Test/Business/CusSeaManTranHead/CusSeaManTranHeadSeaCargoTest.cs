using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusSeaManTranHeadSeaCargoTest : SeaCargoTestCase
	{
		protected const string ABN1 = "83058313205";
		public void TestPrincipalID()
		{
			GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo = ABN1;
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			AssertEquals("Defaulting of Principal", ABN1, header.BT_PrincipalID);
		}

		const string ABNNumberWithSpaces = "75 006 687 958";
		const string ABNNumberWithOutSpaces = "75006687958";
		public void TestABNSpacesStripped()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			header.BT_ResponsiblePartyID = ABNNumberWithSpaces;
			header.BT_PrincipalID = ABNNumberWithSpaces;
			AssertEquals("Responsible Party ID", ABNNumberWithOutSpaces, header.BT_ResponsiblePartyID);
			AssertEquals("Principal Party ID", ABNNumberWithOutSpaces, header.BT_PrincipalID);
		}

		public void TestDateTimeOfArrivalUTC()
		{
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			header.BT_RL_NKPortOfLastForeignPort = "AUSYD";
			header.BT_PortOfLastForeignPortATD = new ZDateTime(2005, 6, 5, 16, 52, 0);
			AssertEquals("DateTimeOfdepartureUTC", new ZDateTime(2005, 6, 5, 6, 52, 0), header.DateTimeOfDepartureUTC);
			header.BT_RL_NKPortOfLastForeignPort = ZString.Empty;
			AssertEquals("DateTimeOfdepartureUTC", ZDateTime.Empty, header.DateTimeOfDepartureUTC);
		}
	}
}
