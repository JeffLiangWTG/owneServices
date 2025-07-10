using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	class FsrTests : TestCaseWithFactory
	{
		public void TestSimpleCUKFSR_ToCommDb()
		{
			CusMawbForTesting.CargoTerminalOperator = "ABC";
			CusMawbForTesting.CargoTerminalOperatorAirport = "LHR";
			CukFsrCreator creator = new CukFsrCreator(CusMawbForTesting, new CcsukTransmissionMessageFunction.CUKFSR.FSA());
			string edifact = creator.MakeMessageText();
			string expectedRequltsFromSpecs = @"UNH+<<MSGNO PLACEHOLDER>>+CUKFSR:1:912:BT+<<SYSCAR>>'BGM++11177777777+++++FSA'LOC";
			AssertContains(expectedRequltsFromSpecs, edifact);
			AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, creator.RecipientPima);
		}

		public void TestSimpleCUKFSR_ToCommDbNoShed()
		{
			CukFsrCreator creator = new CukFsrCreator(CusMawbForTesting, new CcsukTransmissionMessageFunction.CUKFSR.FsaWithoutShed());
			string edifact = creator.MakeMessageText();
			string expectedRequltsFromSpecs = @"UNH+<<MSGNO PLACEHOLDER>>+CUKFSR:1:912:BT+<<SYSCAR>>'BGM++11177777777+++++FSA'UNT";
			AssertContains(expectedRequltsFromSpecs, edifact);
			AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, creator.RecipientPima);
		}

		public void TestHouseCUKFSR()
		{
			var hawb = CusMawbForTesting.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			cusMawbForTesting.CM_MAWB = "ABC-77777777";
			CukFsrCreator creator = new CukFsrCreator(hawb, new CcsukTransmissionMessageFunction.CUKFSR.FSA());
			string edifact = creator.MakeMessageText();
			string expectedResultsFromSpecs = @"UNH+<<MSGNO PLACEHOLDER>>+CUKFSR:1:912:BT+<<SYSCAR>>'BGM++ABC77777777+++HWB:12345678++FSA'";
			AssertContains(expectedResultsFromSpecs, edifact);
			AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, creator.RecipientPima);
		}

		public void TestSplitHouseCUKFSR()
		{
			var hawb = CusMawbForTesting.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			var split = (SplitHouse)hawb.Splits.AddNew();
			split.SplitReference = "03";
			CukFsrCreator creator = new CukFsrCreator(split, new CcsukTransmissionMessageFunction.CUKFSR.FSA());
			string edifact = creator.MakeMessageText();
			string expectedResultsFromSpecs = @"UNH+<<MSGNO PLACEHOLDER>>+CUKFSR:1:912:BT+<<SYSCAR>>'BGM++11177777777+++HWB:12345678:03++FSA'";
			AssertContains(expectedResultsFromSpecs, edifact);
			AssertEquals(CcsukEdiMessageDiverter.PimaForCommunityDatabase, creator.RecipientPima);
			AssertContains(@"<h3>Freight Status Request</h3>
							<p>
								<b>111-77777777-12345678/03</b>
							</p>
							<p>
								Response requested: FSA
							</p>", creator.MessageInterpretation);
		}

		public void TestMasterWithLocationsAndRecipientTransmissionCukFsnFsr()
		{
			CusMawbForTesting.AgentBadge = "ZPE";
			CukFsrCreator creator = new CukFsrCreator(CusMawbForTesting, new CcsukTransmissionMessageFunction.CUKFSR.FSN());
			string edifact = creator.MakeMessageText();
			string expectedRequltsFromSpecs = @"UNH+<<MSGNO PLACEHOLDER>>+CUKFSR:1:912:BT+<<SYSCAR>>'BGM++11177777777+++++FSN'LOC+11:LHR:145:3::ABC:129:ZZZ'COM+CUKAIR98LHRABC:EI'UNT+5+<<MSGNO PLACEHOLDER>>'";
			AssertEquals(expectedRequltsFromSpecs, edifact);
		}

		public void TestIEMFSR()
		{
			CusAwbToFsrProvider provider = new CusAwbToFsrProvider(CusMawbForTesting, FsrRequestType.Codes.FsnRetransmission);
			IemFsrCreator creator = new IemFsrCreator(provider);
			string edifact = creator.MakeIEMFSR();
			string expectedRequltsFromSpecs = @"UNH+<<MSGNO PLACEHOLDER>>+IEMFSR:0:902:Z1'BGM+740+11177777777'UNT+3+<<MSGNO PLACEHOLDER>>'";
			AssertEquals(expectedRequltsFromSpecs, edifact);
		}

		CusMAWB CusMawbForTesting
		{
			get
			{
				if (cusMawbForTesting == null)
				{
					cusMawbForTesting = Factory.New<CusMAWB>();
					cusMawbForTesting.CM_MAWB = "11177777777";
					cusMawbForTesting.CargoTerminalOperator = "ABC";
					cusMawbForTesting.CargoTerminalOperatorAirport = "LHR";
				}
				return cusMawbForTesting;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var badges = new BadgeCodeSettingCollection();
			var ccsukBadge = new BadgeCodeSetting();
			ccsukBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW;
			ccsukBadge.BadgeCode = "ZPE";
			badges.Add(ccsukBadge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			CredentialsSetting ccsukCred = new CredentialsSetting();
			ccsukCred.BadgeCode = ccsukBadge.BadgeCode;
			ccsukCred.Printer = "CUKAIR01LHRFMZ";  // our local PIMA
			ccsukCred.Company = "ZPE";
			CredentialsSettingCollection allCreds = new CredentialsSettingCollection();
			allCreds.Add(ccsukCred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);
		}
		CusMAWB cusMawbForTesting;
	}
}
