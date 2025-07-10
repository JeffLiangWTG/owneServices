using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDJobDeclarationValidationTest : ImportJobDeclarationValidationTest
	{
		public void TestCheckJE_ContainerMode()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec1.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertHasMessageError(dec1.JE_ContainerModeInfo, IMDJobDeclarationValidation.EnterContainerMessageError);
			dec1.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
			AssertHasMessageError(dec1.JE_ContainerModeInfo, IMDJobDeclarationValidation.EnterContainerMessageError);
			dec1.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertHasMessageError(dec1.JE_ContainerModeInfo, IMDJobDeclarationValidation.EnterContainerMessageError);

			var container = dec1.CusContainers.AddNew();
			container.CO_ContainerNumber = "PONU1234565";
			dec1.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertNoMessageError(dec1.JE_ContainerModeInfo, IMDJobDeclarationValidation.EnterContainerMessageError);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_TransportMode = Core.Constants.TransportModes.Other;
			dec2.JE_ContainerMode = Core.Constants.ContainerModes.Empty;
			AssertNoMessageError(dec2.JE_ContainerModeInfo, IMDJobDeclarationValidation.EnterContainerMessageError);

			dec2.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertNoMessageError(dec2.JE_ContainerModeInfo, IMDJobDeclarationValidation.EnterContainerMessageError);

			dec2.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertNoMessageError(dec2.JE_ContainerModeInfo, IMDJobDeclarationValidation.EnterContainerMessageError);
		}

		public void TestCheckJE_TotalNoOfPacks()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 0;
			AssertHasMessageError(declaration.JE_TotalNoOfPacksInfo, IMDJobDeclarationValidation.EnterPackagesMessageError);
			AssertNoMessageError(declaration.JE_TotalNoOfPacksInfo, IMDJobDeclarationValidation.PackingDetailsLineMessageError);

			declaration.JE_TotalNoOfPacks = 1;
			AssertNoMessageError(declaration.JE_TotalNoOfPacksInfo, IMDJobDeclarationValidation.EnterPackagesMessageError);
			AssertNoMessageError(declaration.JE_TotalNoOfPacksInfo, IMDJobDeclarationValidation.PackingDetailsLineMessageError);

			declaration.Packages.RemoveAll();
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertNoMessageError(declaration.JE_TotalNoOfPacksInfo, IMDJobDeclarationValidation.EnterPackagesMessageError);
			AssertHasMessageError(declaration.JE_TotalNoOfPacksInfo, IMDJobDeclarationValidation.PackingDetailsLineMessageError);

			declaration.Packages.AddNew();
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertNoMessageError(declaration.JE_TotalNoOfPacksInfo, IMDJobDeclarationValidation.EnterPackagesMessageError);
			AssertNoMessageError(declaration.JE_TotalNoOfPacksInfo, IMDJobDeclarationValidation.PackingDetailsLineMessageError);
		}

		public void TestCheckJE_AmberStatement()
		{
			declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			IMDJobDeclarationValidation declarationValidation = (IMDJobDeclarationValidation)GetNewValidationProvider(declaration);

			declaration.JE_AmberStatement = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_AmberStatementInfo);

			declaration.JE_AmberStatement = "Foo";
			AssertNoMessageErrors(declaration.JE_AmberStatementInfo);

			declaration.AddInfo.ZA_HART_Hidden = "DUMP";
			declaration.JE_AmberStatement = ZString.Empty;
			AssertHasMessageErrors(declaration.JE_AmberStatementInfo);

			declaration.JE_AmberStatement = "Foo";
			AssertNoMessageErrors(declaration.JE_AmberStatementInfo);

			declaration.JE_AmberStatement = ZString.Replicate('1', 2560);
			AssertNoErrors("Amber Statement", declaration.JE_AmberStatementInfo);

			StmNote amberStatementNote = ((StmNoteCollection)declaration.Notes.GetAllNotes())[0];
			amberStatementNote.ST_NoteDataAsText = ZString.Replicate('1', 2561);
			declarationValidation.ValidateJE_AmberStatement();
			AssertHasErrors("Amber Statement", declaration.JE_AmberStatementInfo);

			declaration.JE_AmberStatement = ZString.Replicate('1', 2560);
			AssertNoErrors("Amber Statement", declaration.JE_AmberStatementInfo);
		}

		public void TestCheckJE_PaidUnderProtestStatement()
		{
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			IMDJobDeclarationValidation declarationValidation = (IMDJobDeclarationValidation)GetNewValidationProvider(declaration);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declarationValidation.ValidateJE_PaidUnderProtestStatement();
			AssertNoMessageErrors(declaration.JE_PaidUnderProtestStatementInfo);

			declaration.JE_PaidUnderProtestStatement = "I pity the foo.";
			AssertHasMessageErrors(declaration.JE_PaidUnderProtestStatementInfo);

			invoiceLine.AddInfo.ZA_PUP = "Y";
			declarationValidation.ValidateJE_PaidUnderProtestStatement();
			AssertNoMessageErrors(declaration.JE_PaidUnderProtestStatementInfo);

			declaration.AddInfo.ZA_FPUP_Hidden = "AAACCPJC3";
			declaration.JE_PaidUnderProtestStatement = "";
			AssertNoMessageErrors(declaration.JE_PaidUnderProtestStatementInfo);

			declaration.AddInfo.ZA_FPUP_Hidden = "";
			declaration.JE_PaidUnderProtestStatement = ZString.Empty;
			AssertHasMessageErrors(declaration.JE_PaidUnderProtestStatementInfo);

			declaration.JE_PaidUnderProtestStatement = ZString.Replicate('1', 4000);
			AssertNoErrors("Paid Under Protest", declaration.JE_PaidUnderProtestStatementInfo);

			StmNote paidUnderProtestNote = ((StmNoteCollection)declaration.Notes.GetAllNotes())[0];
			paidUnderProtestNote.ST_NoteDataAsText = ZString.Replicate('1', 4001);
			declarationValidation.ValidateJE_PaidUnderProtestStatement();
			AssertHasErrors("Paid Under Protest", declaration.JE_PaidUnderProtestStatementInfo);

			declaration.JE_PaidUnderProtestStatement = ZString.Replicate('1', 4000);
			AssertNoErrors("Paid Under Protest", declaration.JE_PaidUnderProtestStatementInfo);
		}

		public void TestAgentReferenceValidation()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			declaration.JE_AgentsReference = "qwertyuiopasdfghjklzxcvbnm";
			AssertNoWarnings("Agent Reference", declaration.JE_AgentsReferenceInfo);

			declaration.JE_AgentsReference = "1234567890";
			AssertNoWarnings("Agent Reference", declaration.JE_AgentsReferenceInfo);

			declaration.JE_AgentsReference = ".,-()=+:'/";
			AssertNoWarnings("Agent Reference", declaration.JE_AgentsReferenceInfo);

			declaration.JE_AgentsReference = "!@#$%^&*";
			AssertHasWarnings("Agent Reference", declaration.JE_AgentsReferenceInfo);

			declaration.JE_AgentsReference = "Test.,-()=@+:'/";
			AssertHasWarnings("Agent Reference", declaration.JE_AgentsReferenceInfo);

			declaration.JE_AgentsReference = "Test.,-()=+:'/";
			AssertNoWarnings("Agent Reference", declaration.JE_AgentsReferenceInfo);
		}

		public void TestOwnerRefValidation()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			declaration.JE_OwnerRef = "qwertyuiopasdfgh";
			AssertNoWarnings("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = "1234567890";
			AssertNoWarnings("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = ".,-()=+:'/";
			AssertNoWarnings("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = "!@#$%^&*";
			AssertHasWarnings("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = "12345678901234567890";
			AssertNoWarnings("Owner Reference", declaration.JE_OwnerRefInfo);

			declaration.JE_OwnerRef = "123456789012345678901";
			AssertHasWarning(declaration.JE_OwnerRefInfo, IMDJobDeclarationValidation.OwnersRefWillBeTruncatedWarning);
		}

		public void TestAgentReferenceValidationForNSR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.NSR;
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_AgentsReference = "Test";
			AssertHasWarnings("Has a warning", declaration.JE_AgentsReferenceInfo);

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.FAR;
			declaration.Validation.ValidateJE_AgentsReference();
			AssertNoWarnings("Has no warnings", declaration.JE_AgentsReferenceInfo);

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;
			declaration.Validation.ValidateJE_AgentsReference();
			AssertNoWarnings("Has no warnings", declaration.JE_AgentsReferenceInfo);

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.DEF;
			declaration.Validation.ValidateJE_AgentsReference();
			AssertNoWarnings("Has no warnings", declaration.JE_AgentsReferenceInfo);
		}

		public void TestTotalNumberOfPacksValidationDoesntApplyForExWarehouse()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_TotalNoOfPacks = 123;
			AssertEquals("Packs has no warning", false, declaration.JE_TotalNoOfPacksInfo.HasWarnings());
		}

		public void TestTotalNumberOfPacksValidation()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TotalNoOfPacks = 123;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "teapot";
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertEquals("Packs has a message error", true, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());

			Package pack1 = declaration.Packages.AddNew();
			pack1.CW_PackQty = 23;
			pack1.CW_HouseBill = houseBill.CU_BillUniqueCode;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertEquals("Packs has a warning", true, declaration.JE_TotalNoOfPacksInfo.HasWarnings());

			Package pack2 = declaration.Packages.AddNew();
			pack2.CW_HouseBill = houseBill.CU_BillUniqueCode;
			pack2.CW_PackQty = 100;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertEquals("Packs has no warning", false, declaration.JE_TotalNoOfPacksInfo.HasWarnings());

			pack1.CW_PackQty = int.MaxValue;
			declaration.JE_TotalNoOfPacks = int.MaxValue;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertHasMessageErrorContaining(declaration.JE_TotalNoOfPacksInfo, "The total number of packages exceeds the maximum allowed amount. Check packing lines for data entry errors.");

			pack1.CW_PackQty = 50;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertEquals("Packs has a warning", true, declaration.JE_TotalNoOfPacksInfo.HasWarnings());
			AssertNoMessageErrorContaining(declaration.JE_TotalNoOfPacksInfo, "The total number of packages exceeds the maximum allowed amount. Check packing lines for data entry errors.");

			declaration.JE_TotalNoOfPacks = 150;
			AssertEquals("Packs has no warning", false, declaration.JE_TotalNoOfPacksInfo.HasWarnings());
		}

		public void TestTotalNumberOfPacksValidationForTransportModeOther()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TotalNoOfPacks = 123;
			declaration.Packages.RemoveAll();
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertEquals("Packs has a msg error", true, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("Packs has no msg error", false, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
		}

		public void TestPaymentMethod()
		{
			declaration.JE_PaymentMethod = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);

			OrgHeader importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "Importer's Bank details are not complete. Please do the following.\r\nPress F3 in the Importer field in Declaration tab and go to Consignee on Organisation form. Fill in BSB and Account Number there.");

			importer.MiscServ.OM_IMEFTBankBSB = "123";
			importer.MiscServ.OM_IMEFTBankAccount = "123456789";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			AssertEquals("No message error", false, declaration.JE_PaymentMethodInfo.HasMessageErrors());

			AccBankAccount brokerAccount = Factory.New<AccBankAccount>();
			brokerAccount.AB_AccountNum = "123456";
			brokerAccount.AB_BSB = "";

			Env.Registry.SetCustomsPaymentBankAccountForCurrentCompany(brokerAccount.PK.ToGuid());
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "Broker's Bank details are not complete. Please do the following.\r\nGo to Registry -> Customs -> Australia -> Payment Bank Account and select a bank account and fill in BSB and Account Number.");

			brokerAccount.AB_BSB = "123";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertEquals("No message error", false, declaration.JE_PaymentMethodInfo.HasMessageErrors());
		}

		public void TestPaymentMethodRefresh()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP";
			importer.MiscServ.OM_IMEFTBankBSB = "";
			Factory.Save();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "Importer's Bank details are not complete. Please do the following.\r\nPress F3 in the Importer field in Declaration tab and go to Consignee on Organisation form. Fill in BSB and Account Number there.");

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader reloadedImporter = factory2.Load<OrgHeader>(importer.PK);
			reloadedImporter.MiscServ.OM_IMEFTBankBSB = "123";
			reloadedImporter.MiscServ.OM_IMEFTBankAccount = "123456789";
			factory2.Save();

			AssertEquals("123", importer.MiscServ.OM_IMEFTBankBSB);
			AssertEquals("123456789", importer.MiscServ.OM_IMEFTBankAccount);

			AssertEquals("No message error", false, declaration.JE_PaymentMethodInfo.HasMessageErrors());
		}

		public void TestIMDMarksAndNumbers()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			IMDJobDeclarationValidation declarationValidation = (IMDJobDeclarationValidation)GetNewValidationProvider(declaration);
			declarationValidation.ValidateJE_MarksAndNumbersShort();
			Assert("No error on Marks and Numbers", !declaration.JE_MarksAndNumbersShortInfo.HasMessageErrors());

			declaration.JE_MarksAndNumbersShort = "Marks and Numbers";
			declarationValidation = (IMDJobDeclarationValidation)GetNewValidationProvider(declaration);
			declarationValidation.ValidateJE_MarksAndNumbersShort();
			Assert("No error on Marks and Numbers", !declaration.JE_MarksAndNumbersShortInfo.HasMessageErrors());
		}

		public void TestMasterBillForSea()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_MasterBill();
			AssertEquals("Master Bill has an error", true, declaration.JE_MasterBillInfo.HasMessageErrors());

			declaration.JE_MasterBill = "Master Bill";
			declaration.Validation.ValidateJE_MasterBill();
			AssertEquals("Masterbill is mandatory", false, declaration.JE_MasterBillInfo.HasMessageErrors());
		}

		public void TestMasterBillForPost()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration.Validation.ValidateJE_MasterBill();
			AssertEquals("Master Bill has no error", false, declaration.JE_MasterBillInfo.HasMessageErrors());
		}

		public void TestMasterBillForAir()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_MasterBill();
			AssertEquals("Master Bill has an error", true, declaration.JE_MasterBillInfo.HasMessageErrors());

			declaration.JE_MasterBill = "000280857";
			declaration.Validation.ValidateJE_MasterBill();
			AssertEquals("Master Bill has an error", true, declaration.JE_MasterBillInfo.HasMessageErrors());

			declaration.JE_MasterBill = "08155555555";
			declaration.PrimaryMasterBill.PackingGroups.AddNew().Packages.AddNew();
			declaration.Validation.ValidateJE_MasterBill();
			AssertEquals("Master Bill has an error", false, declaration.JE_MasterBillInfo.HasMessageErrors());
		}

		public void TestImporterValidationWhenABNAndCIDEmtpy()
		{
			OrgHeader header = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = header.PK;
			AssertHasMessageErrors("Importer", declaration.JE_OH_ImporterInfo);

			header.CustomsClientID = "12345678901";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrors("Importer has CID", declaration.JE_OH_ImporterInfo);

			header.CustomsClientID = ZString.Empty;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrors("Importer", declaration.JE_OH_ImporterInfo);

			header.LocalBusinessRegNo = "12345678901";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrors("Importer has ABN", declaration.JE_OH_ImporterInfo);
		}

		public void TestImporterValidationForCID()
		{
			var header = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = header.PK;
			AssertHasMessageErrors("Importer", declaration.JE_OH_ImporterInfo);

			header.CustomsClientID = "12345";
			var cusCode = header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrors("Importer has CID of incorrect length", declaration.JE_OH_ImporterInfo);

			header.CustomsClientID = "12345678901";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrors("Importer has CID of correct length", declaration.JE_OH_ImporterInfo);

			header.CustomsClientID = "123456789012";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrors("Importer has CID of incorrect length", declaration.JE_OH_ImporterInfo);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError("No CID for Importer", declaration.JE_OH_ImporterInfo, "ABN or the CID must be entered for an Importer.");
		}

		public void TestImporterValidation()
		{
			OrgHeader header = OrgHeader.New(Factory);
			header.LocalBusinessRegNo = "1234567890";
			declaration.JE_OH_Importer = header.PK;
			AssertEquals("Importer", true, declaration.JE_OH_ImporterInfo.HasMessageErrors());

			header.LocalBusinessRegNo = "12345678901";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertEquals("Importer", false, declaration.JE_OH_ImporterInfo.HasMessageErrors());

			header.LocalBusinessRegNo = "123456789012";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertEquals("Importer", true, declaration.JE_OH_ImporterInfo.HasMessageErrors());

			header.LocalBusinessRegNo = "12345678901234";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertEquals("Importer", false, declaration.JE_OH_ImporterInfo.HasMessageErrors());
		}

		public void TestDischargePortIsMandatory()
		{
			declaration.JE_RL_NKPortOfArrival = ZString.Empty;
			AssertHasMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);
		}

		public void TestDischargePortIsNotMandatoryForExWarehouse()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_RL_NKPortOfArrival = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);
		}

		#region Container Validation

		public void TestContainerValidationForExWarehouse()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode has no error", false, declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public void TestContainerValidationForFCL()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode has an error", true, declaration.JE_ContainerModeInfo.HasMessageErrors());

			CusContainer container = declaration.CusContainers.AddNew();
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode has an error", true, declaration.JE_ContainerModeInfo.HasMessageErrors());

			container.CO_ContainerNumber = "Num";
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode doesn't have an error", false, declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public void TestContainerValidationForLCL()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode has an error", true, declaration.JE_ContainerModeInfo.HasMessageErrors());

			CusContainer container = declaration.CusContainers.AddNew();
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode has an error", true, declaration.JE_ContainerModeInfo.HasMessageErrors());

			container.CO_ContainerNumber = "Num";
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode doesn't have an error", false, declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public void TestContainerValidationForFCX()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode has an error", true, declaration.JE_ContainerModeInfo.HasMessageErrors());

			CusContainer container = declaration.CusContainers.AddNew();
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode has an error", true, declaration.JE_ContainerModeInfo.HasMessageErrors());

			container.CO_ContainerNumber = "Num";
			declaration.Validation.ValidateJE_ContainerMode();
			AssertEquals("Container Mode doesn't have an error", false, declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public void TestContainerValidationForTransportModesOtherThanSea()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Container Mode doesn't have an error", false, declaration.JE_ContainerModeInfo.HasMessageErrors());

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("Container Mode doesn't have an error", false, declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		public void TestContainerValidationForBulkAndBreakBulk()
		{
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Container Mode doesn't have an error as no container is needed", false, declaration.JE_ContainerModeInfo.HasMessageErrors());

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Container Mode doesn't have an error as no container is needed", false, declaration.JE_ContainerModeInfo.HasMessageErrors());
		}

		#endregion

		#region Implementation

		protected override JobDeclarationValidation GetNewValidationProvider(JobDeclaration jobDeclaration)
		{
			return new IMDJobDeclarationValidation(jobDeclaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}

		#endregion
	}
}
