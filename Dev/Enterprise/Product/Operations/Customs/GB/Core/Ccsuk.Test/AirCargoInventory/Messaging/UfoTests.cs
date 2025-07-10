using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	class UfoTests : TestCaseWithFactory
	{
		[TestDate(1986, 03, 12, 04, 27, 00)]
		public void TestMakeNewUfoMessage()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H", portName: "Heathrow");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			mawb.InitialiseUFO();
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			AssertEquals("There's multiple PIMAs vailable, but only one shed PIMA, and it's been selected", "CUKAIR98LHRBAC", mawb.Profile);
			var orchestrator = new CreateUFOOrchestrator(mawb);
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			orchestrator.SendFRIMessageForUFO(shutUp);
			AssertContains("Flight", shutUp.InvalidOperationText);
			AssertContains("Arrival", shutUp.InvalidOperationText);
			AssertContains("NPR", shutUp.InvalidOperationText);
			AssertEquals(0, mawb.Messages.Count);
			mawb.CM_FlightNo = "BA123";
			mawb.CM_ArrivalDate = ZDateTime.Now;
			shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			orchestrator.SendFRIMessageForUFO(shutUp);
			mawb.UfoPiecesReceived = 69;
			shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			orchestrator.SendFRIMessageForUFO(shutUp);
			AssertEquals("", shutUp.LastErrorsAsString);
			AssertEquals(null, shutUp.InvalidOperationText);
			AssertEquals(1, mawb.Messages.Count);
			var message = mawb.Messages[0];
			AssertContains("FRI UFO message created", "UNH+1+CUSCAR:2:912:UN:109502+", message.EM_MessageText);
			AssertContains("FRI UFO message created", "BGM+:::FRI+BAC03120427'" +   // Mawb number is UFO plus today's date/time
														"TDT+20+123++++BA:172:3++178:860312:101'" +   // Arrives on BA123 on 12th Mar
														"LOC+11:LHR:145:3::BAC:129:ZZZ'" +  // Arrived at LHR-BAC (from PIMA), without details of destination or origin airport
														"GID+0'" +    // goods item
														"QTY+48:69'" +  // NPR=69, no NPX detail
														"UNT+7+1'",
														message.EM_MessageText);
			AssertContains("Insert Unidentified Freight Object Record", message.EM_MessageInterpretation);
		}
	}
}
