using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ImportJobDeclaration))]
	sealed class ImportJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckDeclarationValuesAfterImport()
		{
			declaration.JE_EntryStatus = "STS";
			declaration.JE_MessageStatus = "MSM";
			declaration.JE_MessageSubType = "MMM";
			declaration.JE_PaymentMethod = "PPP";
			declaration.JE_AddInfo = "Add Info";
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			decToImport.DeclarationPK = declaration.PK;
			decToImport.CountryCode = "NZ";

			var newJobDeclaration = (JobDeclaration)decToImport.CreateNewStandAloneDeclaration(Factory);

			AssertEquals("Entry Status", ZString.Empty, newJobDeclaration.JE_EntryStatus);
			AssertEquals("Message Status", ZString.Empty, newJobDeclaration.JE_MessageStatus);
			AssertEquals("Message Sub Type", "FRM", newJobDeclaration.JE_MessageSubType);
			AssertEquals("Declaration Ref", ZString.Empty, newJobDeclaration.JE_DeclarationReference);
			AssertEquals("Payment Method", "DEF", newJobDeclaration.JE_PaymentMethod);
			AssertEquals("Add Info", ZString.Empty, newJobDeclaration.JE_AddInfo);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, newJobDeclaration.JE_GB);
		}

		BaseJobDeclaration declaration;
		BaseJobComInvoiceHeader invoiceHeader;
		BaseJobComInvoiceLine invoiceLine;
		BaseJobComInvoiceGroupHeader groupHeader;
		ImportJobDeclaration decToImport;

		protected override void SetUp()
		{
			base.SetUp();
			SetUpJobDeclaration();
			decToImport = new ImportJobDeclaration(Factory);
		}

		void SetUpJobDeclaration()
		{
			var nZCompany = Factory.New<GlbCompany>();
			nZCompany.GC_RN_NKCountryCode = "NZ";
			var nZBranch = nZCompany.Branches.AddNew();
			nZBranch.GB_RL_NKHomePort = "NZAKL";

			declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = "CNT";
			declaration.JE_MasterBill = "MasterBill";
			declaration.JE_VesselName = "ADMIRALENGRACHT";
			declaration.JE_VoyageFlightNo = "1234";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_RL_NKPortOfArrival = "ZAAAM";
			declaration.JE_RL_NKPortOfFirstArrival = "ZAAAM";
			declaration.JE_ExportDate = new ZDateTime(2006, 02, 06);
			declaration.JE_DateOfArrival = new ZDateTime(2006, 02, 06);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2006, 02, 06);
			declaration.JE_HouseBill = "House Bill";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_RL_NKFinalDestination = "ZAAAM";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 02, 06);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 02, 06);
			declaration.JE_GoodsDescription = "Description";
			declaration.JE_MarksAndNumbers = "Marks and Nums";
			declaration.JE_OwnerRef = "Owner Ref";
			declaration.JE_GB = nZBranch.PK;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "ConNum";
			container.CO_Seal = "Seal";
			container.CO_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container.CO_FCL_LCL_AIR = "FCL";

			var bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.MasterBill;
			bill3.CU_MasterBill = "Master Bill";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "House Bill 2";
			bill2.CU_MasterBill = "Master Bill";

			var topGroup = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader = topGroup.JobComInvoiceGroupHeaders.AddNew();
			groupHeader.JZ_InvoiceNumber = "Test";
			groupHeader.JZ_AddInfo = "Add Info";

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "Num";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 1500.00M;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			invoiceHeader.JZ_CU_RelatedHouseBill = bill2.PK;
			invoiceHeader.JZ_JZ_GroupInvoiceFK = groupHeader.PK;

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4001.10.00";
			invoiceLine.JI_Description = "Description";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			Factory.Save();
		}
	}
}
