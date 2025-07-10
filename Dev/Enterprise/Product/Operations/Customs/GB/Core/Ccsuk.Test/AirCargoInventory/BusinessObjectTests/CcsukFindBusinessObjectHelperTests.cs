using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class CcsukFindBusinessObjectHelperTests : TestCaseWithFactory
	{
		#region Find Hawb, SplitHouse, SplitBasic helpers

		#region Find Hawb from CusEntryInstruction

		public void TestFindHawbFromEntryInstruction_LinkedDeclaration()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();

			mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "M1111111";
			hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "H1111111";
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindHawbFromEntryInstruction(Factory, cei1));

			hawb1.CS_JE_CustomsFormalEntry = declaration1.PK;
			AssertEquals("Matched via CS_JE_CustomsFormalEntry", "H1111111", CcsukFindBusinessObjectHelper.FindHawbFromEntryInstruction(Factory, cei1).CS_HAWB);
		}

		public void TestFindHawbFromEntryInstruction_LinkedShipment()
		{
			ForwardingShipment shipment1;
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			shipment1 = Factory.New<ForwardingShipment>();
			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			declaration1.JE_JS = shipment1.PK;

			mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "M1111111";
			hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "H1111111";
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindHawbFromEntryInstruction(Factory, cei1));

			hawb1.CS_JS = shipment1.PK;
			hawb1.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("Matched via CS_JS", "H1111111", CcsukFindBusinessObjectHelper.FindHawbFromEntryInstruction(Factory, cei1).CS_HAWB);
		}

		public void TestFindHawbFromEntryInstruction_LinkedNaturalKeys()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.JE_MasterBill = "M11111111111";
			declaration1.JE_HouseBill = "H1111111";
			declaration1.JE_SubLocationOfGoods = "LHRBAC";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			mawb1 = Factory.New<CusMAWB>();
			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_CM = mawb1.PK;
			hawb1.CS_IsMasterHouse = false;

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "M11111111111";
			mawb2.CM_ApplicationCode = ApplicationCodeList.Codes.GbCDSViaCCSUK;
			var hawb3 = Factory.New<CusHAWB>();
			hawb3.CS_HAWB = "H1111111";
			hawb3.CS_IsMasterHouse = false;
			var hawb4 = Factory.New<CusHAWB>();
			hawb4.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			hawb4.CS_IsMasterHouse = false;
			var mawb3 = Factory.New<CusMAWB>();
			mawb3.CM_MAWB = "M11111111111";
			mawb3.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			var hawb5 = Factory.New<CusHAWB>();
			hawb5.CS_CM = mawb3.PK;
			hawb5.CS_IsMasterHouse = true;

			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindHawbFromEntryInstruction(Factory, cei1));

			hawb1.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindHawbFromEntryInstruction(Factory, cei1));

			hawb1.CS_HAWB = "H1111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindHawbFromEntryInstruction(Factory, cei1));

			mawb1.CM_MAWB = "M11111111111";
			mawb1.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			Factory.Save();
			AssertEquals("Matched via natural key CM_MAWB and CS_HAWB and CargoTerminalOperatorAirportAndShed", hawb1.PK, CcsukFindBusinessObjectHelper.FindHawbFromEntryInstruction(Factory, cei1).PK);
		}

		#endregion

		#region Find SplitHouse from CusEntryInstruction

		public void TestFindSplitHouseFromEntryInstruction_LinkedDeclaration()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			SplitHouse splitHouse1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "M1111111";
			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_CM = mawb1.PK;
			hawb1.CS_IsMasterHouse = false;
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			hawb1.CS_JE_CustomsFormalEntry = declaration1.PK;
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			splitHouse1 = (SplitHouse)hawb1.Splits.AddNew();
			splitHouse1.SplitReference = "01";
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			cei1.CEI_SplitReference = "01";
			AssertEquals("Matched via Declaration and CG_MessageReference = CEI_SplitReference", "01", CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1).SplitReference);
		}

		public void TestFindSplitHouseFromEntryInstruction_LinkedShipment()
		{
			ForwardingShipment shipment1;
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			SplitHouse splitHouse1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "M1111111";
			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_CM = mawb1.PK;
			hawb1.CS_IsMasterHouse = false;
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			hawb1.CS_JS = shipment1.PK;
			hawb1.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			splitHouse1 = (SplitHouse)hawb1.Splits.AddNew();
			splitHouse1.SplitReference = "01";
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			cei1.CEI_SplitReference = "01";
			Factory.Save();
			AssertEquals("Matched via Shipment and CG_MessageReference = CEI_SplitReference", "01", CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1).SplitReference);
		}

		public void TestFindSplitHouseFromEntryInstruction_LinkedNaturalKeys()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			SplitHouse splitHouse1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.JE_MasterBill = "M11111111111";
			declaration1.JE_HouseBill = "H1111111";
			declaration1.JE_SubLocationOfGoods = "LHRBAC";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "M11111111111";
			mawb1.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_CM = mawb1.PK;
			hawb1.CS_IsMasterHouse = false;
			hawb1.CS_HAWB = "H1111111";

			splitHouse1 = Factory.New<SplitHouse>();
			splitHouse1.CG_CS = hawb1.PK;
			splitHouse1.SplitReference = "01";
			Factory.Save();
			AssertEquals("No match, no split", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			hawb1.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			hawb1.CS_HAWB = "H1111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			mawb1.CM_MAWB = "M11111111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1));

			cei1.CEI_SplitReference = "01";
			Factory.Save();
			AssertEquals("Matched SplitHouse via natural keys JE_MasterBill=CusMAWB.CM_MAWB, and JE_HouseBill=CS_HAWB, and JE_SubLocationOfGoods=CusHAWB.CargoTerminalOperatorAirportAndShed and CG_MessageReference=CEI_SplitReference", "01", CcsukFindBusinessObjectHelper.FindSplitHouseFromEntryInstruction(Factory, cei1).SplitReference);
		}

		#endregion

		#region Find SplitBasic from CusEntryInstruction

		public void TestFindSplitBasicFromEntryInstruction_LinkedDeclaration()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB basic1;
			SplitConsignment splitBasic1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.JE_HouseBill = "H1111111";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1));

			basic1 = Factory.New<CusMAWB>();
			basic1.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			basic1.CM_MAWB = "M1111111";
			splitBasic1 = basic1.Splits.AddNew();
			splitBasic1.SplitReference = "01";
			hawb1 = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CM, basic1.PK)).FirstOrDefault();
			hawb1.CS_JE_CustomsFormalEntry = declaration1.PK;
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1));

			declaration1.JE_HouseBill = "";
			Factory.Save();
			AssertEquals("Matched via declaration", "01", CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1).SplitReference);
		}

		public void TestFindSplitBasicFromEntryInstruction_LinkedShipment()
		{
			ForwardingShipment shipment1;
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB basic1;
			SplitConsignment splitBasic1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.JE_HouseBill = "H1111111";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;

			basic1 = Factory.New<CusMAWB>();
			basic1.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			basic1.CM_MAWB = "M1111111";
			splitBasic1 = basic1.Splits.AddNew();
			splitBasic1.SplitReference = "01";
			hawb1 = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CM, basic1.PK)).FirstOrDefault();
			hawb1.CS_JS = shipment1.PK;
			hawb1.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1));

			declaration1.JE_HouseBill = "";
			Factory.Save();
			AssertEquals("Matched via declaration", "01", CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1).SplitReference);
		}

		public void TestFindSplitBasicFromEntryInstruction_LinkedNaturalKeys()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB basic1;
			CusHAWB hawb1;
			SplitConsignment splitBasic1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.JE_MasterBill = "M11111111111";
			declaration1.JE_HouseBill = "H1111111";
			declaration1.JE_SubLocationOfGoods = "LHRBAC";
			cei1 = declaration1.CustomsEntryInstructions.AddNew();

			basic1 = Factory.New<CusMAWB>();
			basic1.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			basic1.CM_MAWB = "M1111111";
			splitBasic1 = basic1.Splits.AddNew();
			splitBasic1.SplitReference = "01";
			hawb1 = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CM, basic1.PK)).FirstOrDefault();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1));

			hawb1.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1));

			hawb1.CS_HAWB = "H1111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1));

			basic1.CM_MAWB = "M11111111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1));

			declaration1.JE_HouseBill = "";
			hawb1.CS_HAWB = "";
			Factory.Save();
			AssertEquals("Matched SplitBasic via natural keys JE_MasterBill=CusMAWB.CM_MAWB, and JE_HouseBill=CS_HAWB, and JE_SubLocationOfGoods=CusHAWB.CargoTerminalOperatorAirportAndShed and CG_MessageReference=CEI_SplitReference", "01", CcsukFindBusinessObjectHelper.FindSplitBasicFromEntryInstruction(Factory, cei1).SplitReference);
		}

		#endregion

		#endregion

		#region Find CusEntryInstruction helpers

		#region Find CusEntryInstruction from Hawb

		public void TestFindEntryInstructionFromHawb_LinkedDeclaration()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();

			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_IsMasterHouse = false;
			hawb1.CS_HAWB = "H1111111";
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromHawb(Factory, hawb1));

			hawb1.CS_JE_CustomsFormalEntry = declaration1.PK;
			Factory.Save();
			AssertEquals("Matched via CS_JE_CustomsFormalEntry", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromHawb(Factory, hawb1).PK);
		}

		public void TestFindEntryInstructionFromHawb_LinkedShipment()
		{
			ForwardingShipment shipment1;
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusHAWB hawb1;

			shipment1 = Factory.New<ForwardingShipment>();
			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			declaration1.JE_JS = shipment1.PK;

			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_IsMasterHouse = false;
			hawb1.CS_HAWB = "H1111111";
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromHawb(Factory, hawb1));

			hawb1.CS_JS = shipment1.PK;
			Factory.Save();
			AssertEquals("Matched via CS_JS", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromHawb(Factory, hawb1).PK);
		}

		public void TestFindEntryInstructionFromHawb_LinkedNaturalKeys()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.JE_MasterBill = "M11111111111";
			declaration1.JE_HouseBill = "H1111111";
			declaration1.JE_SubLocationOfGoods = "LHRBAC";
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();

			mawb1 = Factory.New<CusMAWB>();
			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_CM = mawb1.PK;
			hawb1.CS_IsMasterHouse = false;

			mawb1.CM_MAWB = "M11111111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromHawb(Factory, hawb1));

			hawb1.CS_HAWB = "H1111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromHawb(Factory, hawb1));

			hawb1.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			Factory.Save();
			AssertEquals("Matched via natural key JE_MasterBilll=CM_MAWB and JE_HouseBill=CS_HAWB and JE_SubLocationOfGoods=CusHAWB.CargoTerminalOperatorAirportAndShed", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromHawb(Factory, hawb1).PK);
		}

		#endregion

		#region Find CusEntryInstruction from SplitHouse

		public void TestFindEntryInstructionFromSplitHouse_LinkedDeclaration()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			SplitHouse splitHouse1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();

			mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "M1111111";
			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_CM = mawb1.PK;
			hawb1.CS_IsMasterHouse = false;
			splitHouse1 = (SplitHouse)hawb1.Splits.AddNew();
			splitHouse1.SplitReference = "01";
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1));

			hawb1.CS_JE_CustomsFormalEntry = declaration1.PK;
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1));

			cei1.CEI_SplitReference = "01";
			Factory.Save();
			AssertEquals("Matched via CS_JE_CustomsFormalEntry", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1).PK);
		}

		public void TestFindEntryInstructionFromSplitHouse_LinkedShipment()
		{
			ForwardingShipment shipment1;
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			SplitHouse splitHouse1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;

			mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "M1111111";
			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_CM = mawb1.PK;
			hawb1.CS_IsMasterHouse = false;
			splitHouse1 = (SplitHouse)hawb1.Splits.AddNew();
			splitHouse1.SplitReference = "01";
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1));

			hawb1.CS_JS = shipment1.PK;
			hawb1.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1));

			cei1.CEI_SplitReference = "01";
			Factory.Save();
			AssertEquals("Matched via CS_JS and CG_MessageReference=CEI_SplitReference", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1).PK);
		}

		public void TestFindEntryInstructionFromSplitHouse_LinkedNaturalKeys()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			SplitHouse splitHouse1;
			CusMAWB mawb1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.JE_MasterBill = "M11111111111";
			declaration1.JE_HouseBill = "H1111111";
			declaration1.JE_SubLocationOfGoods = "LHRBAC";
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();

			mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "M11111111111";
			hawb1 = Factory.New<CusHAWB>();
			hawb1.CS_CM = mawb1.PK;
			hawb1.CS_IsMasterHouse = false;
			hawb1.CS_HAWB = "H1111111";

			splitHouse1 = Factory.New<SplitHouse>();
			splitHouse1.CG_CS = hawb1.PK;
			splitHouse1.SplitReference = "01";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1));

			hawb1.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1));

			hawb1.CS_HAWB = "H1111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1));

			mawb1.CM_MAWB = "M11111111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1));

			cei1.CEI_SplitReference = "01";
			Factory.Save();
			AssertEquals("Matched via natural keys JE_MasterBill=CusMAWB.CM_MAWB, and JE_HouseBill=CS_HAWB, and JE_SubLocationOfGoods=CusHAWB.CargoTerminalOperatorAirportAndShed and CG_MessageReference=CEI_SplitReference", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitHouse(Factory, splitHouse1).PK);
		}

		#endregion

		#region Find CusEntryInstruction from SplitBasic

		public void TestFindEntryInstructionFromSplitBasic_LinkedDeclaration()
		{
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB basic1;
			SplitConsignment splitBasic1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			basic1 = Factory.New<CusMAWB>();
			basic1.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			splitBasic1 = basic1.Splits.AddNew();
			splitBasic1.SplitReference = "01";
			hawb1 = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CM, basic1.PK)).FirstOrDefault();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1));
			hawb1.CS_JE_CustomsFormalEntry = declaration1.PK;
			Factory.Save();
			AssertEquals("Matched via declaration", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1).PK);
		}

		public void TestFindEntryInstructionFromSplitBasic_LinkedShipment()
		{
			ForwardingShipment shipment1;
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB basic1;
			SplitConsignment splitBasic1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			basic1 = Factory.New<CusMAWB>();
			basic1.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			splitBasic1 = basic1.Splits.AddNew();
			splitBasic1.SplitReference = "01";
			hawb1 = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CM, basic1.PK)).FirstOrDefault();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1));

			hawb1.CS_JS = shipment1.PK;
			hawb1.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			Factory.Save();
			AssertEquals("Matched via shipment", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1).PK);
		}

		public void TestFindEntryInstructionFromSplitBasic_LinkedNaturalKeys()
		{
			ForwardingShipment shipment1;
			CusEntryInstruction cei1;
			JobDeclaration declaration1;
			CusMAWB basic1;
			SplitConsignment splitBasic1;
			CusHAWB hawb1;

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "DEC111111111111";
			declaration1.JE_MasterBill = "M11111111111";
			declaration1.JE_HouseBill = "";
			declaration1.JE_SubLocationOfGoods = "LHRBAC";
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei1 = declaration1.CustomsEntryInstructions.AddNew();
			shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			basic1 = Factory.New<CusMAWB>();
			basic1.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			splitBasic1 = basic1.Splits.AddNew();
			splitBasic1.SplitReference = "01";
			hawb1 = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CM, basic1.PK)).FirstOrDefault();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1));

			hawb1.CS_JS = ZGuid.Empty;
			hawb1.CS_JE_CustomsFormalEntry = ZGuid.Empty;

			hawb1.CargoTerminalOperatorAirportAndShed = "LHRBAC";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1));

			hawb1.CS_HAWB = "H1111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1));

			basic1.CM_MAWB = "M11111111111";
			Factory.Save();
			AssertEquals("No match", null, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1));

			declaration1.JE_HouseBill = "";
			hawb1.CS_HAWB = "";
			Factory.Save();
			AssertEquals("Matched via natural keys", cei1.PK, CcsukFindBusinessObjectHelper.FindCusEntryInstructionFromSplitBasic(Factory, splitBasic1).PK);
		}

		#endregion

		#endregion
	}
}
