using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LicenseFilterStripBusinessObject))]
	class LicenseFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterForMessageType()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			Factory.Save();

			Assert(!dec1.MatchesFilter(filterBO.Filter));
			Assert(!dec2.MatchesFilter(filterBO.Filter));
			Assert(!dec3.MatchesFilter(filterBO.Filter));
			Assert(!dec4.MatchesFilter(filterBO.Filter));
			Assert(dec5.MatchesFilter(filterBO.Filter));
		}

		public void TestFiltersDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals(LicenseFilterStripBusinessObject.LicenseNumberText, filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.EntryNumber].MultilingualDescription);
				AssertEquals(LicenseFilterStripBusinessObject.LicenseStatusText, filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.EntryStatusText].MultilingualDescription);
			});
		}

		public void TestAvailableFilters()
		{
			CombineAssertions(() =>
			{
				AssertNull(filterBO[Enterprise.Customs.GUI.InvoiceLineFilterConstants.ContainerNumber]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.HouseBill]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.MasterBill]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.PaymentNumber]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.PaymentAmount]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.DateFilterTypes.DateOfExport]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.DateFilterTypes.ETAOfDischarge]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.DateFilterTypes.ETDOfLoading]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.PortFilterTypes.LoadDischarge]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.PortFilterTypes.PortOfFirstArrival]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.PortFilterTypes.OriginDestination]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.FlightVoyageVessel]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ServiceLevel]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ServiceType]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ShipmentSubType]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.OrgFilterTypes.PickupTransportCompany]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.OrgFilterTypes.DeliveryTransportCompany]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.RelatedTransportBookings]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.RelatedContainers]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.ContainerModeCustoms]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ModeFilterTypes.DeliveryDropMode]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ModeFilterTypes.PickupDropMode]);
				AssertNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.ShipmentType]);
				AssertNotNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.NumberFilterTypes.EntryNumber]);
				AssertNotNull(filterBO[Enterprise.Customs.Module.DeclarationFilterConstants.EntryStatusText]);
				AssertNotNull(filterBO[DeclarationFilterConstants.DispatchExpiryDate]);
				AssertNotNull(filterBO[DeclarationFilterConstants.ShipmentExpiryDate]);
			});
		}

		public void TestJobDeclarationLinkedImpLicense()
		{
			var date = ZDateTime.Now;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B00001002";
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_DeclarationReference = "B00001003";
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var impLicDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration.JE_DeclarationReference = "ImpLicRef";

			var impLicDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration2.JE_DeclarationReference = "ImpLicRef2";

			var impLicDeclaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration3.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration3.JE_DeclarationReference = "ImpLicRef3";

			var entryInstruction = impLicDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";

			var entryInstruction3 = impLicDeclaration3.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_Description = "Inst-1";

			var invHeader = impLicDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;

			var invHeader3 = impLicDeclaration3.Invoices.AddNew();
			var invLine3 = invHeader3.InvoiceLines.AddNew();
			invLine3.JI_CEI = entryInstruction3.PK;

			var entryHeader = impLicDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumberSetter("TST1", date);

			var entryHeader3 = impLicDeclaration3.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;
			entryHeader3.MovementReferenceNumberSetter("TST3", date);

			var pivot = Factory.NewWithValidTestData<DeclarationRelatedImportLicenseEntryGenPivot>();
			pivot.XX_Relation1ID = declaration.PK;
			pivot.XX_Relation2ID = entryInstruction.PK;

			var pivot3 = Factory.NewWithValidTestData<DeclarationRelatedImportLicenseEntryGenPivot>();
			pivot3.XX_Relation1ID = declaration3.PK;
			pivot3.XX_Relation2ID = entryInstruction3.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "B00001", new [] { impLicDeclaration, impLicDeclaration3 }, DeclarationFilterConstants.JobDeclarationLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "B00001000", new [] { impLicDeclaration }, DeclarationFilterConstants.JobDeclarationLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "B00001003", new [] { impLicDeclaration3 }, DeclarationFilterConstants.JobDeclarationLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "B00001000", new [] { impLicDeclaration }, DeclarationFilterConstants.JobDeclarationLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "B00001003", new [] { impLicDeclaration3 }, DeclarationFilterConstants.JobDeclarationLinkedImpLicense);
			});
		}

		public void TestImpDeclarationEntryLinkedImpLicense()
		{
			var date = ZDateTime.Now;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B00001002";
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var impLicDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration.JE_DeclarationReference = "ImpLicRef";

			var impLicDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration2.JE_DeclarationReference = "ImpLicRef2";

			var entryInstruction = impLicDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";

			var entryInstruction2 = impLicDeclaration2.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Description = "Inst-1";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumberSetter("2212211521", date);

			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryHeader2.MovementReferenceNumberSetter("2212211441", date);

			var pivot = Factory.NewWithValidTestData<DeclarationRelatedImportLicenseEntryGenPivot>();
			pivot.XX_Relation1ID = declaration.PK;
			pivot.XX_Relation2ID = entryInstruction.PK;

			var pivot2 = Factory.NewWithValidTestData<DeclarationRelatedImportLicenseEntryGenPivot>();
			pivot2.XX_Relation1ID = declaration2.PK;
			pivot2.XX_Relation2ID = entryInstruction2.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "2212211", new [] { impLicDeclaration, impLicDeclaration2 }, DeclarationFilterConstants.ImpDeclarationEntryLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "22122115", new [] { impLicDeclaration }, DeclarationFilterConstants.ImpDeclarationEntryLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "22122114", new [] { impLicDeclaration2 }, DeclarationFilterConstants.ImpDeclarationEntryLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "5512211", System.Array.Empty<JobDeclaration>(), DeclarationFilterConstants.ImpDeclarationEntryLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "2212211521", new [] { impLicDeclaration }, DeclarationFilterConstants.ImpDeclarationEntryLinkedImpLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "2212211441", new [] { impLicDeclaration2 }, DeclarationFilterConstants.ImpDeclarationEntryLinkedImpLicense);
			});
		}

		public void TestExchangeHedgeType()
		{
			var impLicDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration1.JE_DeclarationReference = "ImpLicRef";

			var impLicDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration2.JE_DeclarationReference = "ImpLicRef2";

			var invoice1 = impLicDeclaration1.Invoices.AddNew();
			invoice1.ExchangeHedgeType = ExchangeHedgeList.Codes._1;

			var invoice2 = impLicDeclaration2.Invoices.AddNew();
			invoice2.ExchangeHedgeType = ExchangeHedgeList.Codes._2;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "1", new [] { impLicDeclaration1 }, DeclarationFilterConstants.ExchangeHedgeType);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "2", new [] { impLicDeclaration2 }, DeclarationFilterConstants.ExchangeHedgeType);
			});
		}

		public void TestManufacturerIndicator()
		{
			var impLicDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration1.JE_DeclarationReference = "ImpLicRef";

			var impLicDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration2.JE_DeclarationReference = "ImpLicRef2";

			var invoice1 = impLicDeclaration1.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;

			var invoice2 = impLicDeclaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "1", new [] { impLicDeclaration1 }, DeclarationFilterConstants.ManufacturerIndicator);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "2", new [] { impLicDeclaration2 }, DeclarationFilterConstants.ManufacturerIndicator);
			});
		}

		public void TestManufacturer()
		{
			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer1.OH_Code = "TEST1";
			var manufacturerAddress1 = manufacturer1.Addresses.AddNew();
			manufacturerAddress1.OA_Address1 = "TEST1";
			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer2.OH_Code = "TEST2";
			var manufacturerAddress2 = manufacturer2.Addresses.AddNew();
			manufacturerAddress2.OA_Address1 = "TEST2";

			var impLicDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var impLicDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var impLicDeclaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration3.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var impLicDeclaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration4.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var invoiceLine1 = impLicDeclaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_ManufacturerIndicator = "2";
			var invoiceLine2 = impLicDeclaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_ManufacturerIndicator = "2";
			var invoiceLine3 = impLicDeclaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_ManufacturerIndicator = "2";
			var invoiceLine4 = impLicDeclaration4.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine4.JI_ManufacturerIndicator = "2";

			invoiceLine1.ManufacturerDocAddress.E2_OA_Address = manufacturer1.MainAddress.PK;
			invoiceLine2.ManufacturerDocAddress.E2_OA_Address = manufacturerAddress1.PK;
			invoiceLine3.ManufacturerDocAddress.E2_OA_Address = manufacturer2.MainAddress.PK;
			invoiceLine4.ManufacturerDocAddress.E2_OA_Address = manufacturerAddress2.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleGuidFilterResult<JobDeclaration>(filterBO, manufacturer1.PK, new [] { impLicDeclaration1, impLicDeclaration2 }, DeclarationFilterConstants.Manufacturer);
				ModuleTestHelper.AssertModuleGuidFilterResult<JobDeclaration>(filterBO, manufacturer2.PK, new [] { impLicDeclaration3, impLicDeclaration4 }, DeclarationFilterConstants.Manufacturer);
			});
		}

		public void TestDrawbackModality()
		{
			var impLicDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration1.JE_DeclarationReference = "ImpLicRef";

			var impLicDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration2.JE_DeclarationReference = "ImpLicRef2";

			var invoice1 = impLicDeclaration1.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;

			var invoice2 = impLicDeclaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.DrawbackModality = DrawbackModalityList.Codes.NonGenericSuspension;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "1", new [] { impLicDeclaration1 }, DeclarationFilterConstants.DrawbackModality);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "2", new [] { impLicDeclaration2 }, DeclarationFilterConstants.DrawbackModality);
			});
		}

		public void TestDrawbackCANumber()
		{
			var impLicDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration1.JE_DeclarationReference = "ImpLicRef";

			var impLicDeclaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			impLicDeclaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			impLicDeclaration2.JE_DeclarationReference = "ImpLicRef2";

			var invoice1 = impLicDeclaration1.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.DrawbackCANumber = "TEST1";

			var invoice2 = impLicDeclaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.DrawbackCANumber = "TEST2";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "TEST2", new [] { impLicDeclaration2 }, DeclarationFilterConstants.DrawbackCANumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "TEST", new [] { impLicDeclaration1, impLicDeclaration2 }, DeclarationFilterConstants.DrawbackCANumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "TEST1", new [] { impLicDeclaration1 }, DeclarationFilterConstants.DrawbackCANumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "TEST2", new [] { impLicDeclaration2 }, DeclarationFilterConstants.DrawbackCANumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, "TEST1", new [] { impLicDeclaration2 }, DeclarationFilterConstants.DrawbackCANumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, "TEST2", new [] { impLicDeclaration1 }, DeclarationFilterConstants.DrawbackCANumber);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestValidityILDispatchDate()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header1 = declaration1.CustomsEntryHeaders.AddNew();
			header1.EntryNumber = "TST1";
			header1.CH_ValidityILDispatchDate = ZDateTime.Today;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header2.EntryNumber = "TST2";
			header2.CH_ValidityILDispatchDate = ZDateTime.Today.AddDays(-45);

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header3 = declaration3.CustomsEntryHeaders.AddNew();
			header3.EntryNumber = "TST3";
			header3.CH_ValidityILDispatchDate = ZDateTime.Today.AddDays(-15);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration4.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header4 = declaration4.CustomsEntryHeaders.AddNew();
			header4.EntryNumber = "TST4";
			header4.CH_ValidityILDispatchDate = ZDateTime.Empty;

			Factory.Save();

			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-50), ZDateTime.Today.AddDays(-40), new JobDeclaration[] { declaration2 }, DeclarationFilterConstants.DispatchExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), new JobDeclaration[] { declaration1 }, DeclarationFilterConstants.DispatchExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-20), ZDateTime.Empty, new JobDeclaration[] { declaration1, declaration3 }, DeclarationFilterConstants.DispatchExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, new JobDeclaration[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.DispatchExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, new JobDeclaration[] { declaration4 }, DeclarationFilterConstants.DispatchExpiryDate);
		}

		[TestDate(2023, 1, 1)]
		public void TestValidityILShipmentDate()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header1 = declaration1.CustomsEntryHeaders.AddNew();
			header1.EntryNumber = "TST1";
			header1.CH_ValidityILShipmentDate = ZDateTime.Today;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header2.EntryNumber = "TST2";
			header2.CH_ValidityILShipmentDate = ZDateTime.Today.AddDays(-45);

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header3 = declaration3.CustomsEntryHeaders.AddNew();
			header3.EntryNumber = "TST3";
			header3.CH_ValidityILShipmentDate = ZDateTime.Today.AddDays(-15);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration4.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header4 = declaration4.CustomsEntryHeaders.AddNew();
			header4.EntryNumber = "TST4";
			header4.CH_ValidityILShipmentDate = ZDateTime.Empty;

			Factory.Save();

			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-50), ZDateTime.Today.AddDays(-40), new JobDeclaration[] { declaration2 }, DeclarationFilterConstants.ShipmentExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), new JobDeclaration[] { declaration1 }, DeclarationFilterConstants.ShipmentExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-20), ZDateTime.Empty, new JobDeclaration[] { declaration1, declaration3 }, DeclarationFilterConstants.ShipmentExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, new JobDeclaration[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.ShipmentExpiryDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, new JobDeclaration[] { declaration4 }, DeclarationFilterConstants.ShipmentExpiryDate);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new LicenseFilterStripBusinessObject();

		LicenseFilterStripBusinessObject filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (LicenseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
