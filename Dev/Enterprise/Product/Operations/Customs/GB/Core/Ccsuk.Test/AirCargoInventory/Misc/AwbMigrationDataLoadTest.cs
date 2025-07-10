using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Helpers;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(AwbMigrationDataLoad))]
	class AwbMigrationDataLoadTest : DataLoadTestCase<AwbMigrationDataLoad>
	{
		public void TestLoadManyRecords()
		{
			CreateBranchesAndBadgesAndCreds();
			var basicAlreadyExists = Factory.New<CusMAWB>();
			basicAlreadyExists.CargoTerminalOperator = "CAX";
			basicAlreadyExists.CargoTerminalOperatorAirport = "LHR";
			basicAlreadyExists.CM_MAWB = "00099999999";
			Factory.Save();

			var testingBranchCode = GlbBranch.CurrentBranch.GB_Code;

			using (DisposableEnvironment.ForBranch(danBranch.PK.ToGuid()))
			using (var testFileName = TempFile.New())
			{
				using (var sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("PIMA,AIRPORT,SHED,MAWB,HAWB,SDC,BRANCHCODE,NPX");  // header
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00011111111,,T,DUK,1");  // OK
					sw.WriteLine("CUKFFW98000CAR,LHR,BAC,00011111111,,T,DUK,1");  // OK - different shed
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00022222222,,T,DUK,1");  // OK
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00033333333,HOUSE001,T,DUK,1");  // OK
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00033333333,HOUSE002,T,DUK,1");  // OK
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00044444444,HOUSE002,T,DAN,1");  // good pima, good branch, but invalid combo - fails
					sw.WriteLine("CUKFFW98000XXX,LHR,CAX,00055555555,,T,DUK,1");  // duff PIMA - fails
					sw.WriteLine("CUKFFW98000WIS,LHR,CAX,00066666666,,T,DAN,1");  //  OK - created in other branch
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00077777777,,T,ABC,1");  // duff branch - fails
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00011111111,,T,DUK,1"); // duplicate mawb - fails
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00033333333,HOUSE001,T,DUK,1"); // duplicate hawb - fails
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00088888888,HOUSE001,T,DUK,1"); // 'duplicate' hawb - loads OK
					sw.WriteLine("CUKFFW98000CAR,LHR,CAX,00099999999,,T,DUK,1");  // fails - already exists			
					sw.WriteLine("CUKFFW98000CAR,LHR,BAC,00099999999,,T,DUK,1");  // OK - exists but at other shed
					sw.WriteLine("CUKFFW98000CAR,LHR,BAC,XXX99999999,,T," + testingBranchCode + ",1");  // fails - wrong company
				}

				var dataLoader = new AwbMigrationDataLoad();
				dataLoader.ImportData(testFileName.Filename, "AWB");
				
				AssertLog(@"MAWB LHRCAX000-11111111 created under branch 'DUK Branch' as specified in the file.
MAWB LHRBAC000-11111111 created under branch 'DUK Branch' as specified in the file.
MAWB LHRCAX000-22222222 created under branch 'DUK Branch' as specified in the file.
MAWB LHRCAX000-33333333 created under branch 'DUK Branch' as specified in the file.HAWB 000-33333333-HOUSE001 created. 
MAWB LHRCAX000-33333333 already exists. HAWB 000-33333333-HOUSE002 created. 
000-44444444 - invalid, skipping. Error - CS_FolioReference: Please select a valid profile (PIMA); Error - CM_GB: This branch does not have access to the current PIMA. Select a new PIMA or revert the branch 
000-55555555 - invalid, skipping. Error - CS_FolioReference: Please select a valid profile (PIMA); Error - CM_GB: This branch does not have access to the current PIMA. Select a new PIMA or revert the branch 
MAWB LHRCAX000-66666666 created. 
Row 10: Branch ABC does not exist, skipping record
MAWB LHRCAX000-11111111 already exists. 
MAWB LHRCAX000-33333333 already exists. HAWB 000-33333333-HOUSE001 already exists. 
MAWB LHRCAX000-88888888 created under branch 'DUK Branch' as specified in the file.HAWB 000-88888888-HOUSE001 created. 
MAWB LHRCAX000-99999999 already exists. 
MAWB LHRBAC000-99999999 created under branch 'DUK Branch' as specified in the file.
XXX-99999999 - invalid, skipping. Error - CS_FolioReference: Please select a valid profile (PIMA); Error - CM_GB: You cannot create a job in a branch that belongs to a different company.; Error - CM_GB: This branch does not have access to the current PIMA. Select a new PIMA or revert the branch", dataLoader.Log);
				AssertEquals("One existing plus 7 new MAWBs", 8, Factory.GetDatabaseCount(typeof(CusMAWB)));
				AssertEquals("Three new HAWBs. Tests that they all get saved", 3, Factory.GetDatabaseCount(typeof(CusHAWB), new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false)));

				// Test a selection
				AssertAwbInDatabase("CUKFFW98000CAR", "LHRCAX", "00022222222", 0, "T", "DUK");
				AssertAwbInDatabase("CUKFFW98000CAR", "LHRCAX", "00033333333", 2, "T", "DUK");
				AssertAwbInDatabase("CUKFFW98000WIS", "LHRCAX", "00066666666", 0, "T", "DAN");
				AssertAwbInDatabase("CUKFFW98000CAR", "LHRCAX", "00088888888", 1, "T", "DUK");

				AssertEquals(2, Factory.GetDatabaseCount(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, "00011111111")));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, "00044444444")));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, "00055555555")));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, "00077777777")));
				AssertEquals(2, Factory.GetDatabaseCount(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, "00099999999")));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), new ZQuery(CusMAWBSchema.CM_MAWB, "XXX99999999")));
			}
		}

		#region Implementation

		protected override AwbMigrationDataLoad GetNewDataLoader()
		{
			return new AwbMigrationDataLoad();
		}

		void AssertAwbInDatabase(string pima, string airportAndShed, string mawbNumber, int hawbs, string sdc, string branch)
		{
			var mawb = Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, mawbNumber));
			AssertEquals(branch, mawb.Branch.GB_Code);
			AssertEquals(airportAndShed, mawb.MasterLevelHouseHelper.CS_WarehouseLocation);
			AssertEquals(hawbs, mawb.ChildBills.Count);
			AssertEquals(sdc, mawb.ShipmentDescriptionCode);
			AssertEquals(pima, mawb.Profile);
			AssertEquals(1, (ZInt)mawb.NumberOfPiecesExpected);
		}

		GlbBranch danBranch;
		void CreateBranchesAndBadgesAndCreds()
		{
			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Name = "DUK Inc.";
			ukCompany.GC_Code = "DUK";
			ukCompany.GC_RN_NKCountryCode = "GB";
			danBranch = ukCompany.Branches.AddNew();
			danBranch.GB_BranchName = "DAN Branch";
			danBranch.GB_Code = "DAN";
			danBranch.GB_RL_NKHomePort = "GBMIK";
			var dukBranch = ukCompany.Branches.AddNew();
			dukBranch.GB_BranchName = "DUK Branch";
			dukBranch.GB_Code = "DUK";
			dukBranch.GB_RL_NKHomePort = "GBDTE";
			Factory.Save();

			using (DisposableEnvironment.ForBranch(danBranch.PK.ToGuid()))
			{
				var badgeCar = new BadgeCodeSetting();
				badgeCar.BadgeCode = "CAR";
				badgeCar.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
				var badgeWis = new BadgeCodeSetting();
				badgeWis.BadgeCode = "WIS";
				badgeWis.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
				var shedCax = new BadgeCodeSetting();
				shedCax.BadgeCode = "CAX";
				shedCax.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
				var dukBadges = new BadgeCodeSettingCollection();
				dukBadges.Add(badgeCar);
				dukBadges.Add(shedCax);
				GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, dukBranch.PK.ToGuid(), Guid.Empty, dukBadges);
				var danBadges = new BadgeCodeSettingCollection();
				danBadges.Add(badgeWis);
				GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, danBranch.PK.ToGuid(), Guid.Empty, danBadges);
				Factory.Save();

				var creds = new CredentialsSettingCollection();
				var carCred = creds.AddNew();
				var caxCred = creds.AddNew();
				var wisCred = creds.AddNew();
				carCred.BadgeCode = "CAR";
				carCred.PIMA = "CUKFFW98000CAR";
				caxCred.BadgeCode = "CAX";
				caxCred.PIMA = "CUKAIR98LHRCAX";
				wisCred.BadgeCode = "WIS";
				wisCred.PIMA = "CUKFFW98000WIS";
				GBCustomsDataRegistry.Instance.Credentials.SetValue(ukCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);
				Factory.Save();
			}
		}

		void AssertLog(string expected, IReadOnlyList<string> logLines)
		{
			var sb = new ZStringBuilder();
			foreach (var line in logLines)
			{
				sb.Append(line);
			}
			AssertContains(expected, sb.ToStringWithNewLineBetweenAppends());
		}

		#endregion
	}
}
