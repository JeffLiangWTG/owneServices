using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(XXXMasterAirCargo))]
	sealed class XXXMasterAirCargoTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2005, 8, 23)]
		public void TestConstructor()
		{
			XXXMasterAirCargo masterAirCargo = new XXXMasterAirCargo(Factory);
			AssertNotNull(masterAirCargo);
			AssertNotNull("MatchingMasterbills", masterAirCargo.MatchingMasterbills);
			AssertEquals("MasterAirCargo.MatchingMasterbills.Count is zero", 0, masterAirCargo.MatchingMasterbills.Count);
			AssertNotNull("AirCargos", masterAirCargo.AirCargos);
			AssertEquals("MasterAirCargo.Cargos.Count is zero", 0, masterAirCargo.AirCargos.Count);
			AssertNotNull("SelectedAirCargosListTest", masterAirCargo.SelectedAirCargosListTest);
			AssertEquals("SelectedAirCargosListTest.Count", 0, masterAirCargo.SelectedAirCargosListTest.Count);
		}

		public void TestLoadSearchData()
		{
			TestCaseHelper.ClearTable(CusMAWBSchema.Constants.TableName);
			CusMAWB mawb1 = CreateDummyCusMAWB("MAWB1", "QF001", new ZDateTime(2005, 08, 29), "SGSIN", "AUSYD");
			CusMAWB mawb2 = CreateDummyCusMAWB("MAWB2", "QF002", new ZDateTime(2005, 08, 28), "SGSIN", "AUSYD");
			CusMAWB mawb3 = CreateDummyCusMAWB("MAWB3", "QF002", new ZDateTime(2005, 08, 29), "NZAKL", "AUSYD");
			CusMAWB mawb4 = CreateDummyCusMAWB("MAWB4", "QF001", new ZDateTime(2005, 08, 28), "USLAX", "AUMEL");
			CusMAWB mawb5 = CreateDummyCusMAWB("MAWB5", "QF001", new ZDateTime(2005, 08, 29), "SGSIN", "AUSYD");
			Factory.Save();
			XXXAirCargo airCargo = new XXXAirCargo(Factory, FlightRec1, Consignment1, SydBranch.GB_Code);
			AssertNotNull("AirCargo should not be null", airCargo);
			airCargo.SearchMasterBill = "";
			airCargo.SearchFlightNo = "";
			airCargo.SearchArrivalDate = ZDateTime.Empty;
			airCargo.SearchPortOfLoading = "";
			airCargo.SearchPortOfDischarge = "";
			XXXMasterAirCargo masterAirCargo = new XXXMasterAirCargo(Factory);
			AssertNotNull("PreCondition: MasterAirCargo should not be null", masterAirCargo);
			masterAirCargo.LoadSearchData(airCargo);
			AssertEquals("MasterAirCargo.MatchingMasterbills.Count", 5, masterAirCargo.MatchingMasterbills.Count);
			airCargo.SearchPortOfDischarge = "AUSYD";
			masterAirCargo.LoadSearchData(airCargo);
			AssertEquals("MasterAirCargo.MatchingMasterbills.Count", 4, masterAirCargo.MatchingMasterbills.Count);
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb1", true, masterAirCargo.MatchingMasterbills.Contains(mawb1.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb2", true, masterAirCargo.MatchingMasterbills.Contains(mawb2.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb3", true, masterAirCargo.MatchingMasterbills.Contains(mawb3.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb5", true, masterAirCargo.MatchingMasterbills.Contains(mawb5.PK));
			airCargo.SearchPortOfLoading = "SGSIN";
			masterAirCargo.LoadSearchData(airCargo);
			AssertEquals("MasterAirCargo.MatchingMasterbills.Count", 3, masterAirCargo.MatchingMasterbills.Count);
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb1", true, masterAirCargo.MatchingMasterbills.Contains(mawb1.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb2", true, masterAirCargo.MatchingMasterbills.Contains(mawb2.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb5", true, masterAirCargo.MatchingMasterbills.Contains(mawb5.PK));
			airCargo.SearchArrivalDate = new ZDateTime(2005, 08, 29);
			masterAirCargo.LoadSearchData(airCargo);
			AssertEquals("MasterAirCargo.MatchingMasterbills.Count", 2, masterAirCargo.MatchingMasterbills.Count);
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb1", true, masterAirCargo.MatchingMasterbills.Contains(mawb1.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb5", true, masterAirCargo.MatchingMasterbills.Contains(mawb5.PK));
			airCargo.SearchPortOfLoading = "";
			masterAirCargo.LoadSearchData(airCargo);
			AssertEquals("MasterAirCargo.MatchingMasterbills.Count", 3, masterAirCargo.MatchingMasterbills.Count);
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb1", true, masterAirCargo.MatchingMasterbills.Contains(mawb1.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb3", true, masterAirCargo.MatchingMasterbills.Contains(mawb3.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb5", true, masterAirCargo.MatchingMasterbills.Contains(mawb5.PK));
			airCargo.SearchFlightNo = "QF001";
			masterAirCargo.LoadSearchData(airCargo);
			AssertEquals("MasterAirCargo.MatchingMasterbills.Count", 2, masterAirCargo.MatchingMasterbills.Count);
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb1", true, masterAirCargo.MatchingMasterbills.Contains(mawb1.PK));
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb5", true, masterAirCargo.MatchingMasterbills.Contains(mawb5.PK));
			airCargo.SearchMasterBill = "MAWB1";
			masterAirCargo.LoadSearchData(airCargo);
			AssertEquals("MasterAirCargo.MatchingMasterbills.Count", 1, masterAirCargo.MatchingMasterbills.Count);
			AssertEquals("MasterAirCargo.MatchingMasterbills should containt Mawb1", true, masterAirCargo.MatchingMasterbills.Contains(mawb1.PK));
		}

		public void TestSetSelectedElements()
		{
			XXXAirCargo[] selectedAirCargos = new XXXAirCargo[3];
			selectedAirCargos[0] = AirCargo1;
			selectedAirCargos[1] = AirCargo2;
			selectedAirCargos[2] = AirCargo3;
			XXXMasterAirCargo masterAirCargo = new XXXMasterAirCargo(Factory);
			AssertNotNull(masterAirCargo);
			AssertEquals("PreCondition: AirCargo.SelectedAirCargosList.Count", 0, masterAirCargo.SelectedAirCargosListTest.Count);
			masterAirCargo.SetSelectedElements(selectedAirCargos);
			AssertEquals("AirCargo.SelectedAirCargosList.Count", 3, masterAirCargo.SelectedAirCargosListTest.Count);
			AssertEquals("SelectedAirCargosListTest[0]", AirCargo1.PK, masterAirCargo.SelectedAirCargosListTest[0]);
			AssertEquals("SelectedAirCargosListTest[1]", AirCargo2.PK, masterAirCargo.SelectedAirCargosListTest[1]);
			AssertEquals("SelectedAirCargosListTest[2]", AirCargo3.PK, masterAirCargo.SelectedAirCargosListTest[2]);
		}

		public void TestPopulateNewData_NoAirCargoSelected()
		{
			AssertPreConditionSetup();
			CusMAWB masterBill = Factory.New<CusMAWB>();
			AssertEquals("PreCondition: MasterAirCargo.SelectedAirCargosListTest.Count is zero", 0, MasterAirCargo.SelectedAirCargosListTest.Count);
			AssertEquals("PreCondition: Masterbill has no housebill", 0, masterBill.ChildBills.Count);
			MasterAirCargo.PopulateNewData(masterBill);
			AssertEquals("No Housebill added to Masterbill as there is no aircargo selected", 0, masterBill.ChildBills.Count);
		}

		public void TestPopulateNewData()
		{
			AssertPreConditionSetup();
			MasterAirCargo.Progress += new TNTProgressEventHandler(Progress);
			try
			{
				ProgressCalled = false;
				CusMAWB masterBill = Factory.New<CusMAWB>();
				XXXAirCargo[] selectedAirCargos = new XXXAirCargo[2];
				selectedAirCargos[0] = AirCargo1;
				selectedAirCargos[1] = AirCargo3;
				MasterAirCargo.SetSelectedElements(selectedAirCargos);
				AssertEquals("PreCondition: MasterAirCargo.SelectedAirCargosListTest.Count", 2, MasterAirCargo.SelectedAirCargosListTest.Count);
				AssertEquals("PreCondition: Masterbill has no housebill", 0, masterBill.ChildBills.Count);
				MasterAirCargo.PopulateNewData(masterBill);
				AssertEquals("2 new Housebills added to Masterbill", 2, masterBill.ChildBills.Count);
				CusHAWB houseBill1 = null;
				CusHAWB houseBill2 = null;
				if (masterBill.ChildBills[0].CS_HAWB == Consignment1.HouseBill)
				{
					houseBill1 = masterBill.ChildBills[0];
					houseBill2 = masterBill.ChildBills[1];
				}
				else
				{
					houseBill1 = masterBill.ChildBills[1];
					houseBill2 = masterBill.ChildBills[0];
				}

				AssertEquals("Masterbill should contain Housebill1", Consignment1.HouseBill, houseBill1.CS_HAWB);
				AssertEquals("Masterbill should contain Housebill2", Consignment3.HouseBill, houseBill2.CS_HAWB);
				AssertEquals("Progress Called", true, ProgressCalled);
			}
			finally
			{
				MasterAirCargo.Progress -= new TNTProgressEventHandler(Progress);
			}
		}

		public void TestPopulateData_NoAirCargoSelected()
		{
			AssertPreConditionSetup();
			CusMAWB masterBill = Factory.New<CusMAWB>();
			AssertEquals("PreCondition: MasterAirCargo.SelectedAirCargosListTest.Count is zero", 0, MasterAirCargo.SelectedAirCargosListTest.Count);
			AssertEquals("PreCondition: Masterbill has no housebill", 0, masterBill.ChildBills.Count);
			MasterAirCargo.PopulateData(masterBill);
			AssertEquals("No Housebill added to Masterbill as there is no aircargo selected", 0, masterBill.ChildBills.Count);
		}

		public void TestPopulateData()
		{
			AssertPreConditionSetup();
			MasterAirCargo.Progress += new TNTProgressEventHandler(Progress);
			try
			{
				ProgressCalled = false;
				CusMAWB masterBill = Factory.New<CusMAWB>();
				XXXAirCargo[] selectedAirCargos = new XXXAirCargo[2];
				selectedAirCargos[0] = AirCargo1;
				selectedAirCargos[1] = AirCargo3;
				MasterAirCargo.SetSelectedElements(selectedAirCargos);
				AssertEquals("PreCondition: MasterAirCargo.SelectedAirCargosListTest.Count", 2, MasterAirCargo.SelectedAirCargosListTest.Count);
				AssertEquals("PreCondition: Masterbill has no housebill", 0, masterBill.ChildBills.Count);
				MasterAirCargo.PopulateData(masterBill);
				AssertEquals("2 new Housebills added to Masterbill", 2, masterBill.ChildBills.Count);
				CusHAWB houseBill1 = null;
				CusHAWB houseBill2 = null;
				if (masterBill.ChildBills[0].CS_HAWB == Consignment1.HouseBill)
				{
					houseBill1 = masterBill.ChildBills[0];
					houseBill2 = masterBill.ChildBills[1];
				}
				else
				{
					houseBill1 = masterBill.ChildBills[1];
					houseBill2 = masterBill.ChildBills[0];
				}

				AssertEquals("Masterbill should contain Housebill1", Consignment1.HouseBill, houseBill1.CS_HAWB);
				AssertEquals("Masterbill should contain Housebill2", Consignment3.HouseBill, houseBill2.CS_HAWB);
				AssertEquals("Progress Called", true, ProgressCalled);
			}
			finally
			{
				MasterAirCargo.Progress -= new TNTProgressEventHandler(Progress);
			}
		}

		public void TestIsValid_NoConsignment()
		{
			XXXMasterAirCargo masterAirCargo = new XXXMasterAirCargo(Factory);
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			string expectedMessage = "No Air Cargo Consignment to import";
			AssertEquals("AirCargo.IsValid should be false as there are no consignment to import", false, masterAirCargo.IsValid);
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf(expectedMessage) >= 0);
		}

		public void TestIsValid_WithErrors()
		{
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			AssertPreConditionSetup();
			AirCargo1.Buffer.Notify(new ErrorNotification(ErrorType.Error, "AirCargo1 has error"));
			AirCargo3.Buffer.Notify(new ErrorNotification(ErrorType.Error, "AirCargo3 has error"));
			userNotification.AddAnswer(DialogResult.No);
			AssertEquals("AirCargo.IsValid should be false as there are errors and the user response is NO", false, MasterAirCargo.IsValid);
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf("AirCargo1 has error") >= 0);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf("AirCargo3 has error") >= 0);
			userNotification.ClearMessagesAndAnswers();
			userNotification.AddAnswer(DialogResult.Yes);
			AssertEquals("AirCargo.IsValid should be true as there are errors and the user response is YES", true, MasterAirCargo.IsValid);
			lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf("AirCargo1 has error") >= 0);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, true, lastMessage.IndexOf("AirCargo3 has error") >= 0);
		}

		public void TestIsValid_WithNoErrors()
		{
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			AssertPreConditionSetup();
			AssertEquals("AirCargo.IsValid should be true as there are no errors", true, MasterAirCargo.IsValid);
			AssertEquals("Expected no new error message", 1, userNotification.PreviousMessages.Length);
		}

		public void TestSave()
		{
			AssertPreConditionSetup();
			CusMAWB masterBill = Factory.New<CusMAWB>();
			AirCargo1.PopulateNewData(masterBill);
			AirCargo3.PopulateNewData(masterBill);
			AssertEquals("PreCondition: MasterBill has 2 housebills", 2, masterBill.ChildBills.Count);
			Factory.Save();
			CusHAWB houseBill1 = null;
			CusHAWB houseBill2 = null;
			if (masterBill.ChildBills[0].CS_HAWB == Consignment1.HouseBill)
			{
				houseBill1 = masterBill.ChildBills[0];
				houseBill2 = masterBill.ChildBills[1];
			}
			else
			{
				houseBill1 = masterBill.ChildBills[1];
				houseBill2 = masterBill.ChildBills[0];
			}

			AssertEquals("PreCondition: HouseBill1's HAWB", Consignment1.HouseBill, houseBill1.CS_HAWB);
			AssertEquals("PreCondition: HouseBill2's HAWB", Consignment3.HouseBill, houseBill2.CS_HAWB);
			XXXAirCargo[] selectedAirCargos = new XXXAirCargo[2];
			selectedAirCargos[0] = AirCargo1;
			selectedAirCargos[1] = AirCargo3;
			AssertEquals("PreCondition: AirCargo1's LinkedHouseBillRef is empty", "", AirCargo1.LinkedHouseBillRef);
			AssertEquals("PreCondition: AirCargo2's LinkedHouseBillRef is empty", "", AirCargo2.LinkedHouseBillRef);
			AssertEquals("PreCondition: AirCargo3's LinkedHouseBillRef is empty", "", AirCargo3.LinkedHouseBillRef);
			AssertEquals("PreCondition: MasterAirCargo.SelectedAirCargosListTest.Count", 0, MasterAirCargo.SelectedAirCargosListTest.Count);
			MasterAirCargo.Save();
			AssertEquals("AirCargo1's LinkedHouseBillRef is still empty as it was not selected", "", AirCargo1.LinkedHouseBillRef);
			AssertEquals("AirCargo2's LinkedHouseBillRef is still empty as it was not selected", "", AirCargo2.LinkedHouseBillRef);
			AssertEquals("AirCargo3's LinkedHouseBillRef is still empty as it was not selected", "", AirCargo3.LinkedHouseBillRef);
			MasterAirCargo.SetSelectedElements(selectedAirCargos);
			AssertEquals("PreCondition: MasterAirCargo.SelectedAirCargosListTest.Count", 2, MasterAirCargo.SelectedAirCargosListTest.Count);
			AssertEquals("PreCondition: MasterAirCargo.SelectedAirCargosListTest[0] has AirCargo1.PK", AirCargo1.PK, MasterAirCargo.SelectedAirCargosListTest[0]);
			AssertEquals("PreCondition: MasterAirCargo.SelectedAirCargosListTest[1] has AirCargo3.PK", AirCargo3.PK, MasterAirCargo.SelectedAirCargosListTest[1]);
			MasterAirCargo.Save();
			AssertEquals("AirCargo1's LinkedHouseBill is HouseBill1", houseBill1.PK, AirCargo1.LinkedHouseBill.PK);
			AssertEquals("AirCargo1's LinkedHouseBillRef is HouseBill1.CS_MessageReference", houseBill1.CS_MessageReference, AirCargo1.LinkedHouseBillRef);
			AssertEquals("AirCargo2's LinkedHouseBillRef is still empty as it was not selected", "", AirCargo2.LinkedHouseBillRef);
			AssertEquals("AirCargo3's LinkedHouseBill is HouseBill2", houseBill2.PK, AirCargo3.LinkedHouseBill.PK);
			AssertEquals("AirCargo3's LinkedHouseBillRef is HouseBill2.CS_MessageReference", houseBill2.CS_MessageReference, AirCargo3.LinkedHouseBillRef);
		}

		void AssertPreConditionSetup()
		{
			AssertNotNull("PreCondition: MasterAirCargo is not null", MasterAirCargo);
			AssertNotNull("PreCondition: MasterAirCargo.AirCargos is not null", MasterAirCargo.AirCargos);
			AssertEquals("PreCondition: MasterAirCargo.AirCargos.Count", 3, MasterAirCargo.AirCargos.Count);
			AssertNotNull("PreCondition: AirCargo1 is not null", AirCargo1);
			AssertNotNull("PreCondition: AirCargo2 is not null", AirCargo2);
			AssertNotNull("PreCondition: AirCargo3 is not null", AirCargo3);
			AssertEquals("PreCondition: MasterAirCargo contain AirCargo1", true, MasterAirCargo.AirCargos.Contains(AirCargo1));
			AssertEquals("PreCondition: MasterAirCargo contain AirCargo2", true, MasterAirCargo.AirCargos.Contains(AirCargo2));
			AssertEquals("PreCondition: MasterAirCargo contain AirCargo3", true, MasterAirCargo.AirCargos.Contains(AirCargo3));
		}

		void Progress(object sender, TNTProgressEventArgs e)
		{
			ProgressCalled = true;
		}
		bool ProgressCalled;
		XXXMasterAirCargo MasterAirCargo
		{
			get
			{
				if (fMasterAirCargo == null)
				{
					fMasterAirCargo = new XXXMasterAirCargo(Factory);
					fMasterAirCargo.AirCargos.Add(AirCargo1);
					fMasterAirCargo.AirCargos.Add(AirCargo2);
					fMasterAirCargo.AirCargos.Add(AirCargo3);
				}

				return fMasterAirCargo;
			}
		}
		XXXMasterAirCargo fMasterAirCargo;

		FlightRecord FlightRec1
		{
			get
			{
				if (fFlightRec1 == null)
				{
					fFlightRec1 = new FlightRecord(FlightDetailLine);
					fFlightRec1.MasterBill = "MasterBill1";
				}

				return fFlightRec1;
			}
		}
		FlightRecord fFlightRec1;

		ConsignmentRecord Consignment1
		{
			get
			{
				if (fConsignment1 == null)
				{
					fConsignment1 = new ConsignmentRecord(ConsignmentLine);
					fConsignment1.HouseBill = "HouseBill1";
				}

				return fConsignment1;
			}
		}
		ConsignmentRecord fConsignment1;

		XXXAirCargo AirCargo1
		{
			get
			{
				if (fAirCargo1 == null)
				{
					fAirCargo1 = new XXXAirCargo(Factory, FlightRec1, Consignment1, SydBranch.GB_Code);
				}

				return fAirCargo1;
			}
		}
		XXXAirCargo fAirCargo1;

		ConsignmentRecord Consignment2
		{
			get
			{
				if (fConsignment2 == null)
				{
					fConsignment2 = new ConsignmentRecord(ConsignmentLine);
					fConsignment2.HouseBill = "HouseBill2";
				}

				return fConsignment2;
			}
		}
		ConsignmentRecord fConsignment2;

		XXXAirCargo AirCargo2
		{
			get
			{
				if (fAirCargo2 == null)
				{
					fAirCargo2 = new XXXAirCargo(Factory, FlightRec1, Consignment2, SydBranch.GB_Code);
				}

				return fAirCargo2;
			}
		}
		XXXAirCargo fAirCargo2;

		FlightRecord FlightRec3
		{
			get
			{
				if (fFlightRec3 == null)
				{
					fFlightRec3 = new FlightRecord(FlightDetailLine);
					fFlightRec3.MasterBill = "MasterBill3";
				}

				return fFlightRec3;
			}
		}
		FlightRecord fFlightRec3;

		ConsignmentRecord Consignment3
		{
			get
			{
				if (fConsignment3 == null)
				{
					fConsignment3 = new ConsignmentRecord(ConsignmentLine);
					fConsignment3.HouseBill = "HouseBill3";
				}

				return fConsignment3;
			}
		}
		ConsignmentRecord fConsignment3;

		XXXAirCargo AirCargo3
		{
			get
			{
				if (fAirCargo3 == null)
				{
					fAirCargo3 = new XXXAirCargo(Factory, FlightRec3, Consignment3, SydBranch.GB_Code);
				}

				return fAirCargo3;
			}
		}
		XXXAirCargo fAirCargo3;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new XXXMasterAirCargo(Factory);
		}

		CusMAWB CreateDummyCusMAWB(ZString masterBill, ZString flightNo, ZDateTime arrivalDate, ZString portOfLoading, ZString portOfDischarge)
		{
			CusMAWB result = Factory.New<CusMAWB>();
			result.CM_MAWB = masterBill;
			result.CM_FlightNo = flightNo;
			result.CM_ArrivalDate = arrivalDate;
			result.CM_RL_NKLoadPort = portOfLoading;
			result.CM_RL_NKDischargePort = portOfDischarge;
			return result;
		}

		GlbBranch SydBranch
		{
			get
			{
				if (fSydBranch == null)
				{
					fSydBranch = GetSydBranch();
				}

				return fSydBranch;
			}
		}

		GlbBranch fSydBranch;
		GlbBranch GetSydBranch()
		{
			string branchCode = "SYD";
			GlbBranch result = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (result == null)
			{
				result = Factory.New<GlbBranch>();
				result.GB_Code = branchCode;
				Factory.Save();
			}

			return result;
		}

		const string FlightDetailLine = "01BA0151SINWHR150705A12527013486 M0000151  SINWHRTP   231.423                                                                                                                                                                                                                                                                                                                                                                                                                                            .";
		const string ConsignmentLine = "03940432180 SINWHR20908767HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             SG 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     WHERE IS THIS STATE            AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SA                             AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "NS1233233234.34AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
	}
}
