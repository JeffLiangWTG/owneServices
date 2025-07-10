using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class JobDeclarationCloneTest : TestCaseWithFactory
	{
		public void TestClone()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			JobDeclaration clone = (JobDeclaration)testDec.TemplateCopy();

			Assert("Clone Different From Dec", clone != testDec);

			AssertEquals("GroupHeaderCount", testDec.JobComInvoiceGroupHeaders.Count, clone.JobComInvoiceGroupHeaders.Count);
			for (int i = 0; i < testDec.JobComInvoiceGroupHeaders.Count; i++)
			{
				AssertEquals("JobComInvoiceGroupHeaders[" + i + "].OverseasFreight", 100m, testDec.JobComInvoiceGroupHeaders[i].Charges[0].J7_Amount);
				AssertEquals("JobComInvoiceGroupHeaders[" + i + "].OverseasFreight", 100m, clone.JobComInvoiceGroupHeaders[i].Charges[0].J7_Amount);
				AssertEquals("JobComInvoiceGroupHeaders[" + i + "].JobComInvoiceHeaders.Count", testDec.JobComInvoiceGroupHeaders[i].JobComInvoiceHeaders.Count, clone.JobComInvoiceGroupHeaders[i].JobComInvoiceHeaders.Count);
				for (int j = 0; j < testDec.JobComInvoiceGroupHeaders[i].JobComInvoiceHeaders.Count; j++)
				{
					AssertEquals("TestDec.JobComInvoiceGroupHeaders[" + i + "].JobComInvoiceHeaders[" + j + "].JobComInvoiceLines.Count", testDec.JobComInvoiceGroupHeaders[i].JobComInvoiceHeaders[j].JobComInvoiceLines.Count, clone.JobComInvoiceGroupHeaders[i].JobComInvoiceHeaders[j].JobComInvoiceLines.Count);
				}
			}
		}

		public void TestCloneWithANote()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			StmNote note = testDec.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;

			JobDeclaration clone = (JobDeclaration)testDec.TemplateCopy();

			AssertEquals(1, clone.Notes.GetAllNotes().Count);
			StmNote clonedNote = ((StmNoteCollection)clone.Notes.GetAllNotes())[0];
			clone.Delete();
			AssertEquals("IsDeleted", true, clonedNote.IsDeleted);
			AssertEquals("IsDeleted", false, ((StmNoteCollection)testDec.Notes.GetAllNotes())[0].IsDeleted);
		}

		public void TestCloneWhenImporterHasANote()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			StmNote note = testDec.Importer.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;

			AssertEquals(0, testDec.Notes.GetAllNotes().Count);
			JobDeclaration clone = (JobDeclaration)testDec.Clone();
			AssertEquals("IsDeleted", false, ((StmNoteCollection)testDec.Importer.Notes.GetAllNotes())[0].IsDeleted);
		}

		public void TestCloneWithJI_PartNoSet()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_Tariff = "8518.30.10 98";
			testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_PartNo = "123";
			JobDeclaration clone = (JobDeclaration)testDec.TemplateCopy();

			AssertEquals("JI_PartNo", testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_PartNo, clone.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_PartNo);
			AssertEquals("JI_PartNo", testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_Description, clone.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_Description);
			AssertEquals("JI_Tarrif", testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_Tariff, clone.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_Tariff);
		}

		public void TestCloneWithJI_CCSet()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CC = ZGuid.NewZGuid();
			JobDeclaration clone = (JobDeclaration)testDec.TemplateCopy();

			AssertEquals("JI_CC", testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CC, clone.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CC);
		}

		public void TestSaveWithAClone()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			var cloneArgs = new BusinessObjectCloneArgs(new[] { JobDeclarationSchema.JE_DeclarationReference.Name });
			var clone = (JobDeclaration)testDec.Clone(cloneArgs);
			string oldDescription = testDec.JE_GoodsDescription;
			clone.JE_GoodsDescription = "New Description";
			Assert(oldDescription == testDec.JE_GoodsDescription);
			Assert(clone.JE_GoodsDescription == "New Description");
			Factory.Save();
			Assert(oldDescription == testDec.JE_GoodsDescription);
			Assert(clone.JE_GoodsDescription == "New Description");

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration testDecLoad = factory2.Load<JobDeclaration>(testDec.PK);
			JobDeclaration cloneLoad = factory2.Load<JobDeclaration>(testDec.PK);
			AssertEquals(testDecLoad, cloneLoad);
			AssertEquals(testDecLoad.JE_GoodsDescription, oldDescription);
			Assert(true);
		}

		public void TestDeleteClone()
		{
			var testDec = Factory.Load<JobDeclaration>(JobDeclarationTest.SetUpAndSaveImportDecWithCPDecsAnswered(""));
			JobDeclaration clone = (JobDeclaration)testDec.Clone();
			clone.Delete();
			Assert(clone.IsDeleted);
			Assert(!testDec.IsDeleted);
			Factory.Save();
		}

		[ExpectNoExceptions()]
		public void TestCloneWithJZ_FOBAmountSet()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader testHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader.JZ_InvoiceAmount = 1000m;
			testHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			testHeader.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			testHeader.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100);
			testHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			testHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100);
			testDec.Clone();
		}

		public void TestInvoiceLinesIsCorrectAfterClone()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader testHeaderX = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader testHeaderY = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceHeader testHeaderZ = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine testLine1 = testHeaderX.JobComInvoiceLines.AddNew();
			JobComInvoiceLine testLine2 = testHeaderX.JobComInvoiceLines.AddNew();
			JobComInvoiceLine testLine3 = testHeaderY.JobComInvoiceLines.AddNew();
			JobComInvoiceLine testLine4 = testHeaderZ.JobComInvoiceLines.AddNew();

			AssertEquals("TestDec.InvoiceLines.Count", 4, testDec.FilteredInvoiceLines.Count);
			JobDeclaration clonedDec = (JobDeclaration)testDec.TemplateCopy();
			AssertEquals("ClonedDec.InvoiceLines.Count", 4, clonedDec.FilteredInvoiceLines.Count);
		}
		protected JobDeclaration CreateSendableDeclaration2()
		{
			OrgHeader supplier = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABABEU");

			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "90093519530");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "955411R");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "2292272C");
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.AddInfo.ZA_MergeBy_Hidden = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.JE_AgentsReference = "198810 -MP";
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			testDec.JE_DateOfArrival = new ZDateTime(2003, 12, 1, 10, 2, 0);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2003, 12, 1, 10, 2, 0);
			testDec.JE_DeclarationReference = "B00103428";
			testDec.JE_EntryStatus = CustomsEntryStatus.ClearCreate.Code;
			testDec.JE_ExportDate = new ZDateTime(2003, 11, 28, 10, 2, 0);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_MasterBill = "PONLCPH22002959";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_OwnerRef = "1200994221";
			testDec.JE_RL_NKFinalDestination = "AUMEL";
			testDec.JE_RL_NKOrigin = "DEFRA";
			testDec.JE_RL_NKPortOfArrival = "AUMEL";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "DEHAM";
			testDec.JE_VesselName = "ADMIRALENGRACHT";
			testDec.JE_TotalNoOfPacks = 1;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_VoyageFlightNo = "2038";
			//TestDec.AddInfo.ZA_MergeBy_Hidden = "NON";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceCurrExRate = 1.000000000m;
			testDec.JobComInvoiceGroupHeaders[0].JZ_InvoiceDate = new ZDateTime(2003, 11, 28);

			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 2300, "USD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 310.7300m, "AUD");

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_AddInfo = "ORG=DK*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=UT";
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			testHeader1.JZ_InvoiceAmount = 124295.5000m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2003, 11, 28);
			testHeader1.JZ_InvoiceNumber = "1";
			testHeader1.JZ_OH_Supplier = supplier.PK;
			testHeader1.JZ_RX_NKInvoice_Currency = "AUD";
			testHeader1.JZ_VolumeUQ = "M3";
			testHeader1.JZ_Weight = 25500.000m;
			testHeader1.JZ_WeightUQ = "KG";

			JobComInvoiceLine testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_AddInfo = "GSTE=FOOD";
			testLine1.JI_CustomsQuantity = 24859.1000m;
			testLine1.JI_CustomsUnitQty = "KG";
			testLine1.JI_Description = "FROZDANPORK";
			testLine1.JI_InvoiceQuantity = 24859.10000m;
			testLine1.JI_InvoiceUQ = "KG";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 24295.5000m;
			testLine1.JI_Tariff = "0203.29.00 41";
			testLine1.JI_Weight = 24859.100m;
			testLine1.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine2 = testHeader1.JobComInvoiceLines.AddNew();
			testLine2.JI_AddInfo = "GSTE=FOOD";
			testLine2.JI_CustomsQuantity = 24859.1000m;
			testLine2.JI_CustomsUnitQty = "KG";
			testLine2.JI_Description = "FROZDANPORK";
			testLine2.JI_InvoiceQuantity = 24859.10000m;
			testLine2.JI_InvoiceUQ = "KG";
			testLine2.JI_LineNo = (short)1;
			testLine2.JI_LinePrice = 100000.0000m;
			testLine2.JI_Tariff = "0203.29.00 41";
			testLine2.JI_Weight = 24859.100m;
			testLine2.JI_WeightUQ = "KG";

			return testDec;
		}

		public void TestPopulateJE_DeclarationReferenceIfNeededForStandAloneDec()
		{
			ZString expectedDecReference = Env.NumberFountains.CustomsJobNo.PeekPreliminaryFormatted(Factory).ToUpper();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("DeclarationReference", "", declaration.JE_DeclarationReference);
			CargoWise.Data.Db.Connection.BeginTransaction();// The method accesses a number fountain so must be in a transaction
			declaration.PopulateJE_DeclarationReferenceIfNeeded();
			CargoWise.Data.Db.Connection.CommitTransaction();// The method accesses a number fountain so must be in a transaction
			AssertEquals("DeclarationReference", expectedDecReference, declaration.JE_DeclarationReference);
		}

		public void TestPopulateJE_DeclarationReferenceIfNeededForNonStandAloneDec()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000001";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertEquals("DeclarationReference", "", declaration.JE_DeclarationReference);
			declaration.PopulateJE_DeclarationReferenceIfNeeded();
			AssertEquals("DeclarationReference", "S00000001", declaration.JE_DeclarationReference);
		}

		[ExpectException(typeof(Exception))]
		public void TestJE_DeclarationReferenceClearedIfSavingUnsuccessful()
		{
			TestJobDeclarationThatThrowsExceptionWhistSaving declaration = Factory.New<TestJobDeclarationThatThrowsExceptionWhistSaving>();
			try
			{
				declaration.Factory.Save();
			}
			finally
			{
				AssertEquals("DeclarationReference", "", declaration.JE_DeclarationReference);
			}
		}

		public void TestRecoverFromUnsuccessfulSave()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_DeclarationReference = "B12345678";
			Assert(!testDec.JE_DeclarationReference.IsEmpty);
			testDec.RecoverFromUnsuccessfulSave();
			Assert(testDec.JE_DeclarationReference.IsEmpty);
			Factory.Save();
			Assert(!testDec.JE_DeclarationReference.IsEmpty);
			testDec.RecoverFromUnsuccessfulSave();
			Assert(!testDec.JE_DeclarationReference.IsEmpty);
		}

		public void TestJE_MessageTypeIsReadonlyIfAnyStateBesidesNotSent()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("JE_MessageTypeNotReadonly", !declaration.JE_MessageTypeInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("JE_MessageTypeNotReadonly", !declaration.JE_MessageTypeInfo.ReadOnly);

			declaration.JE_EntryStatus = CustomsEntryStatus.ClearCreate.Code;
			Assert("JE_MessageTypeReadonly", declaration.JE_MessageTypeInfo.ReadOnly);

			declaration.JE_EntryStatus = CustomsEntryStatus.FailCPDec.Code;
			Assert("JE_MessageTypeReadonly", declaration.JE_MessageTypeInfo.ReadOnly);

			declaration.JE_EntryStatus = CustomsEntryStatus.FailCreate.Code;
			Assert("JE_MessageTypeNotReadonly", !declaration.JE_MessageTypeInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			Assert("JE_MessageTypeNotReadonly", !declaration.JE_MessageTypeInfo.ReadOnly);

			declaration.JE_EntryStatus = CustomsEntryStatus.ClearWithdrawal.Code;
			Assert("JE_MessageTypeReadonly", !declaration.JE_MessageTypeInfo.ReadOnly);

			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			Assert("JE_MessageTypeReadonly", declaration.JE_MessageTypeInfo.ReadOnly);

			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var loadedDeclaration = secondFactory.Load<JobDeclaration>(declaration.PK);

			Assert("JE_MessageTypeReadonly", loadedDeclaration.JE_MessageTypeInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_RequestForPermitNumber = "121234";
			declaration.JE_EntryStatus = ZString.Empty;
			Factory.Save();
			loadedDeclaration.Reload();
			Assert("JE_MessageTypeReadonly", loadedDeclaration.JE_MessageTypeInfo.ReadOnly);
		}

		public void TestDontChangeCurrencyIfCloning()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";

			Customs.Business.BaseJobComInvHeaderCharge oNS = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 100);
			oNS.J7_RX_NKCurrency = "HKD";

			JobDeclaration clonedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("HKD", clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].Charges[2].J7_RX_NKCurrency);
		}

		public void TestPortOfFirstArrivalClearedForExports()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfFirstArrival = "USLAX";
			AssertEquals("Pre-condition", "USLAX", declaration.JE_RL_NKPortOfFirstArrival);
			declaration.JE_MessageType = declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Still LA", "USLAX", declaration.JE_RL_NKPortOfFirstArrival);
			declaration.JE_MessageType = declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Now blank", "", declaration.JE_RL_NKPortOfFirstArrival);
		}

		public void TestCloneWithHouseBillsAndPackingDetails()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_MasterBill = "Master Bill 1";
			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_HouseBill = "House Bill 2";

			Package pack1 = declaration.Packages.AddNew();
			pack1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			pack1.CW_PackQty = 150;

			Package pack2 = declaration.Packages.AddNew();
			pack2.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			pack2.CW_PackQty = 250;

			AssertEquals("PreCondition: Has two house bills", 2, declaration.Bills.Count);
			AssertEquals("PreCondition: Has two Packing records", 2, declaration.PackingGroups.Count);

			JobDeclaration clonedDeclaration = (JobDeclaration)declaration.Clone();
			AssertEquals("Has no house bills", 0, clonedDeclaration.Bills.Count);
			AssertEquals("Has no Packing records", 0, clonedDeclaration.PackingGroups.Count);
		}

		public void TestTemplateCopyWithBillsAndPackingFilledIn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "Master Bill";
			declaration.JE_HouseBill = "House Bill";
			declaration.JE_TotalNoOfPacks = 150;
			AssertEquals("PreCondition: house bills", 2, declaration.Bills.Count);
			AssertEquals("PreCondition: packing record", 1, declaration.PackingGroups.Count);

			JobDeclaration clonedDeclaration = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("Master Bill Is Empty", true, clonedDeclaration.JE_MasterBill.IsEmpty);
			AssertEquals("House Bill is Emtpy", true, clonedDeclaration.JE_HouseBill.IsEmpty);
			AssertEquals("no house bills", 0, clonedDeclaration.Bills.Count);
			AssertEquals("no packing records", 0, clonedDeclaration.PackingGroups.Count);
		}

		public void TestTemplateCopyWithoutBillsAndPackingFilledIn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("PreCondition: no house bills", 0, declaration.Bills.Count);
			AssertEquals("PreCondition: no packing records", 0, declaration.PackingGroups.Count);

			JobDeclaration clonedDeclaration = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("Master Bill Is Empty", true, clonedDeclaration.JE_MasterBill.IsEmpty);
			AssertEquals("House Bill is Emtpy", true, clonedDeclaration.JE_HouseBill.IsEmpty);
			AssertEquals("no house bills", 0, clonedDeclaration.Bills.Count);
			AssertEquals("no packing records", 0, declaration.PackingGroups.Count);
		}

		#region TestJobDeclarationThatThrowsExceptionWhistSaving

		protected class TestJobDeclarationThatThrowsExceptionWhistSaving : JobDeclaration
		{
			public TestJobDeclarationThatThrowsExceptionWhistSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new Exception("Test Exception");
			}
		}

		#endregion
	}
}
