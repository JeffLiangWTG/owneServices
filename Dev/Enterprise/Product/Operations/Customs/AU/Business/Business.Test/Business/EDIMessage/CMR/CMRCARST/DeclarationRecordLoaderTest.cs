using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DeclarationRecordLoaderTest : TestCaseWithFactory
	{
		public void TestLoadRecordWithBlankValues()
		{
			BusinessObject result = decLoader.LoadRecord(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("EntryHeader", result);
		}

		public void TestLoadRecordUsingReferenceNumberOnly()
		{
			declaration.JE_MasterBill = "TBA";
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_DeclarationReference = "B00001000";
			declaration.DoMerge();
			var cusEntryHeader1 = declaration.CustomsEntryHeaders[0];
			cusEntryHeader1.CH_BGMReference = "B00001000/XXXX";

			var declaration2 = JobDeclaration.New(Factory);
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration2.JE_MasterBill = "TBA";
			declaration2.JE_DeclarationReference = "B00001001";
			declaration2.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var shutterUpper1 = new SendsMessagesToCustomsShutterUpperer();
			declaration2.MessageInitiator = shutterUpper1;
			declaration2.DoMerge();
			var cusEntryHeader2 = declaration2.CustomsEntryHeaders[0];
			cusEntryHeader2.CH_BGMReference = "B00001001/XXXX";
			Factory.Save();

			var result = decLoader.LoadRecord("TBA", ZString.Empty, ZString.Empty, ZString.Empty, "B00001000/XXXX", ZString.Empty, ZString.Empty);
			AssertEquals(cusEntryHeader1, result);

			result = decLoader.LoadRecord("TBA", ZString.Empty, ZString.Empty, ZString.Empty, "B00001001/XXXX", ZString.Empty, ZString.Empty);
			AssertEquals(cusEntryHeader2, result);
		}

		public void TestLoadRecordUsingContainer()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "TBA";
			declaration.JE_HouseBill = ZString.Empty;
			declaration.CusContainers.AddNew().CO_ContainerNumber = "C1";
			declaration.DoMerge();
			var cusEntryHeader1 = declaration.CustomsEntryHeaders[0];

			var declaration2 = JobDeclaration.New(Factory);
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration2.JE_MasterBill = "TBA";
			declaration2.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration2.CusContainers.AddNew().CO_ContainerNumber = "C2";
			var shutterUpper1 = new SendsMessagesToCustomsShutterUpperer();
			declaration2.MessageInitiator = shutterUpper1;
			declaration2.DoMerge();
			var cusEntryHeader2 = declaration2.CustomsEntryHeaders[0];
			Factory.Save();

			var result = decLoader.LoadRecord("TBA", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "C1");
			AssertEquals(cusEntryHeader1, result);

			result = decLoader.LoadRecord("TBA", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "C2");
			AssertEquals(cusEntryHeader2, result);
		}

		public void TestLoadRecordWithDeclarationNotCreatedInAustralia()
		{
			GlbCompany nZCompany = Factory.New<GlbCompany>();
			nZCompany.GC_RN_NKCountryCode = "NZ";
			GlbBranch nZBranch = nZCompany.Branches.AddNew();
			nZBranch.GB_RL_NKHomePort = "NZAKL";

			declaration.JE_GB = nZBranch.PK;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			BusinessObject result = decLoader.LoadRecord("MasterBill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader", result);
		}

		public void TestLoadRecordWithDeclarationCreatedInAustralia()
		{
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			BusinessObject result = decLoader.LoadRecord("MasterBill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Found EntryHeader", result);
		}

		public void TestLoadRecordWithMasterBill()
		{
			declaration.JE_HouseBill = ZString.Empty;
			declaration.DoMerge();
			Factory.Save();

			CusEntryHeader result = decLoader.LoadRecord("MAWB", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this MAWB", result);

			result = decLoader.LoadRecord("MasterBill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MAWB", result);
			AssertEquals(masterBill1, result.Declaration.PrimaryMasterBill);
		}

		public void TestLoadRecordWithMasterBillAndHouseBill()
		{
			declaration.DoMerge();
			Factory.Save();

			BusinessObject result = decLoader.LoadRecord("MAWB", "House Bill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this MAWB and HAWB Combo", result);

			result = decLoader.LoadRecord("Master Bill", "HAWB", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this MAWB and HAWB Combo", result);

			result = decLoader.LoadRecord("MasterBill", "House Bill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MAWB and HAWB", result);
		}

		public void TestLoadRecordWithBGMRegerenceAndEntryNumber()
		{
			declaration.DoMerge();
			CusEntryHeader cusEntryHeader1 = declaration.CustomsEntryHeaders[0];
			declaration.JE_DeclarationReference = "B00001000";

			JobDeclaration declaration2 = JobDeclaration.New(Factory);
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration2.JE_MasterBill = "Master Bill2";
			declaration2.JE_HouseBill = "House Bill";
			declaration2.JE_DeclarationReference = "B00001001";

			Bill masterBill2 = declaration2.PrimaryMasterBill;
			Bill houseBill2 = declaration2.PrimaryHouseBill;
			JobComInvoiceHeader invHeader2 = declaration2.Invoices.AddNew();
			JobComInvoiceLine invLine2 = invHeader2.JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer shutterUpper = new SendsMessagesToCustomsShutterUpperer();
			declaration2.MessageInitiator = shutterUpper;
			declaration2.DoMerge();
			CusEntryHeader cusEntryHeader2 = declaration2.CustomsEntryHeaders[0];
			cusEntryHeader2.CH_BGMReference = "B00001001/XXXX";
			cusEntryHeader2.EntryNumber = "EntryNum";
			Factory.Save();

			BusinessObject result = decLoader.LoadRecord("MasterBill", "House Bill", ZString.Empty, ZString.Empty, "B00001001/XXXX", "EntryNum", ZString.Empty);
			AssertEquals(cusEntryHeader2, result);

			result = decLoader.LoadRecord("MasterBill", "House Bill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals(cusEntryHeader1, result);

			result = decLoader.LoadRecord("MasterBill", "House Bill", ZString.Empty, ZString.Empty, "B00009999/XXXX", "EntryNum", ZString.Empty);
			AssertEquals(cusEntryHeader1, result);

			result = decLoader.LoadRecord("MasterBill", "House Bill", ZString.Empty, ZString.Empty, "B00001001/XXXX", "EntryNum2", ZString.Empty);
			AssertEquals(cusEntryHeader1, result);

			result = decLoader.LoadRecord("MasterBill", "House Bill2", ZString.Empty, ZString.Empty, "B00001001/XXXX", "EntryNum2", ZString.Empty);
			AssertNull(result);
		}

		public void TestLoadRecordWithMultiMasterAndHouseBills()
		{
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_MasterBill = "MasterBill";

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_HouseBill = "House Bill 1";
			bill3.CU_MasterBill = "MasterBill";

			declaration.DoMerge();
			Factory.Save();

			CusEntryHeader result = decLoader.LoadRecord("MAWB", "House Bill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this MAWB and HAWB Combo", result);

			result = decLoader.LoadRecord("Master Bill", "HAWB", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this MAWB and HAWB Combo", result);

			result = decLoader.LoadRecord("MasterBill", "House Bill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MAWB and HAWB", result);
			CusEntryHeader header = result;
			AssertEquals("Master/House", houseBill1.PK, header.Bills[0].PK);

			result = decLoader.LoadRecord("MasterBill", "House Bill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MAWB and HAWB", result);

			result = decLoader.LoadRecord("MasterBill", "House Bill 1", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MAWB and HAWB", result);
		}

		public void TestLoadRecordWithMasterAndVoyageAndLloyds()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VoyageFlightNo = " 01234";
			declaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			declaration.JE_HouseBill = ZString.Empty;
			declaration.DoMerge();
			Factory.Save();

			CusEntryHeader result = decLoader.LoadRecord("Master Bill", ZString.Empty, "12 34", "8811924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this Combo", result);

			result = decLoader.LoadRecord("Master Bill", ZString.Empty, "1234", "881 1924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this Combo", result);

			result = decLoader.LoadRecord("MasterBill", ZString.Empty, "1234", "8811924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this Combo", result);
		}

		public void TestLoadRecordWithVoyageInfo()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VoyageFlightNo = "1234";
			declaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			declaration.DoMerge();
			Factory.Save();

			BusinessObject result = decLoader.LoadRecord("Master Bill", "HouseBill", "1234", "8811924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this Combo", result);

			result = decLoader.LoadRecord("Master Bill", "House Bill", "12 34", "8811924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this Combo", result);

			result = decLoader.LoadRecord("Master Bill", "House Bill", "1234", "881 1924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this Combo", result);

			result = decLoader.LoadRecord("MasterBill", "House Bill", "1234", "8811924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this Combo", result);
		}

		public void TestLoadRecordWithVoyageInfoDecHasNoVessel()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VoyageFlightNo = "1234";
			declaration.DoMerge();
			Factory.Save();

			BusinessObject result = decLoader.LoadRecord("MasterBill", "House Bill", "1234", "8811924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No EntryHeader with this Combo", result);
		}

		public void TestLoadRecordWithVoyageInfoInDifferentFormats()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VoyageFlightNo = " 0n3m3";
			declaration.JE_MasterBill = "Master Bill";
			declaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			declaration.DoMerge();
			Factory.Save();

			BusinessObject result = decLoader.LoadRecord("Master Bill", "House Bill", "N3M3", "8811924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this Combo", result);

			result = decLoader.LoadRecord("Master Bill", "House Bill", "N3 M3", "8811924", ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("Entry Header with this Combo", result);
		}

		public void TestWithMultiEntryHeadersForAir()
		{
			JobDeclaration testDeclaration = declaration;
			invHeader.JZ_CU_RelatedHouseBill = masterBill1.PK;//needs to be direct
			houseBill1.CU_CU_ParentBill = ZGuid.Empty;

			Bill masterBill2 = testDeclaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "Master Bill2";

			Bill houseBill2 = (Bill)masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "House Bill2";

			JobComInvoiceHeader invHeader2 = testDeclaration.Invoices.AddNew();
			invHeader2.AddInfo.ZA_EFD = "050505";
			invHeader2.JobComInvoiceLines.AddNew();
			invHeader2.JZ_CU_RelatedHouseBill = houseBill2.PK;

			testDeclaration.DoMerge();
			Factory.Save();

			AssertEquals("2 entry Headers", 2, testDeclaration.CustomsEntryHeaders.Count);

			CusEntryHeader result = decLoader.LoadRecord("MasterBill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MasterBill", result);
			AssertEquals("Master/House", masterBill1.PK, result.Bills[0].PK);

			result = decLoader.LoadRecord("Master Bill2", "House Bill2", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MasterBill and House Bill", result);
			AssertEquals("Master/House", houseBill2.PK, result.Bills[0].PK);
		}

		public void TestWithMultiEntryHeadersForSea()
		{
			JobDeclaration testDeclaration = declaration;
			testDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			invHeader.JZ_CU_RelatedHouseBill = masterBill1.PK;
			houseBill1.CU_CU_ParentBill = ZGuid.Empty;

			Bill masterBill2 = testDeclaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "Master Bill2";

			Bill houseBill2 = (Bill)masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "House Bill2";

			JobComInvoiceHeader invHeader2 = testDeclaration.Invoices.AddNew();
			invHeader2.AddInfo.ZA_EFD = "050505";
			invHeader2.JobComInvoiceLines.AddNew();

			invHeader2.JZ_CU_RelatedHouseBill = houseBill2.PK;
			testDeclaration.JE_VoyageFlightNo = "1234";
			testDeclaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			testDeclaration.DoMerge();
			Factory.Save();

			AssertEquals("2 entry Headers", 2, testDeclaration.CustomsEntryHeaders.Count);

			CusEntryHeader result = decLoader.LoadRecord("MasterBill", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MAWB and HAWB", result);
			AssertEquals("Master/House", masterBill1.PK, result.Bills[0].PK);

			result = decLoader.LoadRecord("Master Bill2", "House Bill2", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNotNull("Entry Header with this MAWB and HAWB", result);
			AssertEquals("Master/House", houseBill2.PK, result.Bills[0].PK);
		}

		public void TestGetPossibleHouseBills()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "Master Bill3";
			declaration.Branch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry(Core.Constants.CountryCodes.China);
			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUXXX";
			branch.GB_GC = company.PK;
			var otherCountryDeclaration = factory.New<JobDeclaration>();
			otherCountryDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			otherCountryDeclaration.JE_MasterBill = "Master Bill3";
			otherCountryDeclaration.JE_GB = branch.PK;
			factory.Save();
			var bills = (Bill[])Factory.Load(typeof(Bill), decLoader.GetPossibleHouseBills("Master Bill3", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty));
			AssertEquals("Only load bills of current comany", 1, bills.Length);
			var bill = bills[0];
			AssertEquals("AU Declaration", bill.Declaration.PK, declaration.PK);
		}

		#region Implemenation

		protected override void SetUp()
		{
			base.SetUp();
			decLoader = new DeclarationRecordLoader(Factory);

			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_MasterBill = "Master Bill";
			declaration.JE_HouseBill = "House Bill";

			masterBill1 = declaration.PrimaryMasterBill;
			houseBill1 = declaration.PrimaryHouseBill;

			invHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();

			SendsMessagesToCustomsShutterUpperer shutterUpper = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = shutterUpper;
		}

		DeclarationRecordLoader decLoader;
		JobDeclaration declaration;
		JobComInvoiceHeader invHeader;
		Bill masterBill1;
		Bill houseBill1;

		#endregion
	}
}
