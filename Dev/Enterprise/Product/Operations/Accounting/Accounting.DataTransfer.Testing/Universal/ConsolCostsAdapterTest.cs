using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	public partial class ConsolCostsAdapterTest : TestCaseWithFactory
	{
		TestObjectCreator TestObjectCreator;

		public void TestGenerateConsolCostLines()
			=> AssertGenerateConsolCostLines();

		public void TestGenerateConsolCostLines_EnableGovernmentChargeCode()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertGenerateConsolCostLines(enableGovernmentChargeCode: true);
		}

		public void TestGenerateConsolCostLines_EnableSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertGenerateConsolCostLines(enableSupplyType: true);
		}

		[ExpectNoExceptions]
		void AssertGenerateConsolCostLines(bool enableGovernmentChargeCode = false, bool enableSupplyType = false)
		{
			TestObjectCreator.CC1.AC_ChargeGroup = "BRK";
			TestObjectCreator.CC2.AC_ChargeGroup = "BRK";

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			IJobInvoicingPlugIn shipment = consol.Shipments.AddNew();
			Factory.Save();

			ApportionmentListing apportionments = new ApportionmentListing(Factory, consol);
			JobConsolCost cost1 = apportionments.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost1.E6_OSCostAmount = 60m;
			cost1.E6_LocalCostAmount = 60m;
			cost1.E6_AH_APInvoice = ZGuid.Empty;
			cost1.E6_InvoiceNum = "1234321";
			cost1.E6_InvoiceDate = DateTime.Now;
			cost1.E6_PaymentDate = DateTime.Now;
			cost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			cost1.Creditor.CompanyData.OB_APExternalCreditorCode = "AAA";
			cost1.ApportionmentCharges[0].JR_OSCostAmt = 60m;
			cost1.E6_CostReference = "ABC666777";
			cost1.E6_ExchangeRate = 1;
			cost1.E6_PPDCLT = "ALL";
			cost1.E6_ApportionmentMethod = AllocationMethod.Shipment;
			cost1.E6_SellGovtChargeCode = "Sell Govt Chg Code 1";
			cost1.E6_CostGovtChargeCode = "Cost Govt Chg Code 1";
			cost1.E6_GS_NKConsolCostOwner = GlbStaff.CurrentUser.GS_Code;
			cost1.E6_RatingBehaviour = RatingBehaviours.Spot;
			cost1.E6_SupplyType = "DSB";

			JobConsolCost cost2 = apportionments.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			cost2.E6_OSCostAmount = 90m;
			cost2.E6_LocalCostAmount = 90m;
			cost2.E6_AH_APInvoice = ZGuid.Empty;
			cost2.E6_InvoiceNum = "877878";
			cost2.E6_InvoiceDate = DateTime.Now;
			cost2.E6_PaymentDate = DateTime.Now;
			cost2.E6_OH_Creditor = TestObjectCreator.Creditor2.PK;
			cost2.Creditor.CompanyData.OB_APExternalCreditorCode = "BBB";
			cost2.ApportionmentCharges[0].JR_OSCostAmt = 90m;
			cost2.E6_CostReference = "XYZ789987";
			cost2.E6_ExchangeRate = 1;
			cost2.E6_PPDCLT = "ALL";
			cost2.E6_ApportionmentMethod = AllocationMethod.Shipment;
			cost2.E6_SellGovtChargeCode = "Sell Govt Chg Code 2";
			cost2.E6_CostGovtChargeCode = "Cost Govt Chg Code 2";
			cost2.E6_RatingBehaviour = RatingBehaviours.Spot;
			cost2.E6_SupplyType = "";

			Factory.Save();

			IJobInvoicingPlugIn shipment2 = consol.Shipments.AddNew();
			Factory.Save();

			// Simulate Consol simultaneously Loaded by user
			var factoryToLoad = new BusinessObjectFactory();
			ForwardingConsol loadedConsol = factoryToLoad.Load<ForwardingConsol>(consol.PK);

			var loadedApportionmentListing = new ApportionmentListing(factoryToLoad, loadedConsol);
			AssertEquals("Two ConsolCosts", 2, loadedApportionmentListing.CostsCollection.Count);
			AssertEquals("Two Charges for both Shipments", 2, loadedApportionmentListing.CostsCollection[0].ApportionmentCharges.Count);
			AssertEquals("Two Charges for both Shipments", 2, loadedApportionmentListing.CostsCollection[1].ApportionmentCharges.Count);

			// Load in a new Factory to Generate from
			var factoryToGenerate = new BusinessObjectFactory();
			ForwardingConsol consolToGenerate = factoryToGenerate.Load<ForwardingConsol>(consol.PK);

			cost2.E6_SupplyType = "AAA";
			var consolCostsAdapter = new ConsolCostsAdapter();
			var generatedConsolCosts = consolCostsAdapter.Generate(consol, DefaultDataObjectWriterStrategy.TestInstance);
			AssertConsolCostLine("[Consol Cost Line 0]", cost1, generatedConsolCosts.ConsolCostLineCollection[0], enableGovernmentChargeCode, enableSupplyType);
			AssertConsolCostLine("[Consol Cost Line 1]", cost2, generatedConsolCosts.ConsolCostLineCollection[1], enableGovernmentChargeCode, enableSupplyType);

			// Release mMutex created for second Shipment
			loadedApportionmentListing.ReleaseMutexes();
		}

		void AssertConsolCostLine(string comment, JobConsolCost cost, ConsolCostLine consolCostLine, bool enableGovernmentChargeCode = false, bool enableSupplyType = false)
		{
			ChargeCodeGroupList chargeGroupList = new ChargeCodeGroupList();
			ZString description = chargeGroupList.GetDescriptionFromCode("BRK");

			AssertEquals($"{comment} ChargeCode Code", cost.ChargeCode.AC_Code, consolCostLine.ChargeCode.Code);
			AssertEquals($"{comment} ChargeCode Description", cost.ChargeCode.AC_Desc, consolCostLine.ChargeCode.Description);
			AssertEquals($"{comment} ChargeCodeGroup Code", cost.ChargeCode.AC_ChargeGroup, consolCostLine.ChargeCodeGroup.Code);
			AssertEquals($"{comment} ChargeCodeGroup Description", description, consolCostLine.ChargeCodeGroup.Description);
			AssertEquals($"{comment} CostOSCurrency Code", cost.Currency.RX_Code, consolCostLine.CostOSCurrency.Code);
			AssertEquals($"{comment} CostOSCurrency Description", cost.Currency.RX_Desc, consolCostLine.CostOSCurrency.Description);
			AssertEquals($"{comment} CostOSAmount", cost.E6_OSCostAmount, consolCostLine.CostOSAmount);
			AssertEquals($"{comment} CostLocalAmount", cost.E6_LocalCostAmount, consolCostLine.CostLocalAmount);
			AssertEquals($"{comment} CostIsPosted", cost.IsPosted, consolCostLine.CostIsPosted);
			AssertEquals($"{comment} CostAPInvoiceNumber", cost.E6_InvoiceNum, consolCostLine.CostAPInvoiceNumber);
			AssertEquals($"{comment} CostInvoiceDate", cost.E6_InvoiceDate.ToShortTimeString(), consolCostLine.CostInvoiceDate.Value.ToShortTimeString());
			AssertEquals($"{comment} CostDueDate", cost.E6_PaymentDate.ToShortTimeString(), consolCostLine.CostDueDate.Value.ToShortTimeString());
			AssertEquals($"{comment} CostGSTVATID", cost.TaxRate.AT_Code, consolCostLine.CostGSTVATID.TaxCode);
			AssertEquals($"{comment} CostOSGSTVATAmount", cost.E6_OSGSTAmount_Calc, consolCostLine.CostOSGSTVATAmount);
			AssertEquals($"{comment} Creditor Type", nameof(DataContextType.Organization), consolCostLine.Creditor.Type);
			AssertEquals($"{comment} Creditor Key", cost.Creditor.OH_Code, consolCostLine.Creditor.Key);
			AssertEquals($"{comment} ExternalCreditorCode", cost.Creditor.CompanyData.OB_APExternalCreditorCode, consolCostLine.ExternalCreditorCode);
			AssertEquals($"{comment} SupplierReference", cost.E6_CostReference, consolCostLine.SupplierReference);
			AssertEquals($"{comment} CostExchangeRate", cost.E6_ExchangeRate, consolCostLine.CostExchangeRate);
			AssertEquals($"{comment} PrepaidCollectFilter", cost.E6_PPDCLT, consolCostLine.PrepaidCollectFilter);
			AssertEquals($"{comment} ApportionmentMethod", cost.E6_ApportionmentMethod, consolCostLine.ApportionmentMethod);
			AssertEquals($"{comment} IncludeOnCollectInvoice", cost.E6_IsForCollectInvoice, consolCostLine.IncludeOnCollectInvoice);
			AssertEquals($"{comment} ApportionToSubShipments", cost.E6_ApportionToRelatedShipments, consolCostLine.ApportionToSubShipments);

			if (enableGovernmentChargeCode)
			{
				AssertEquals($"{comment} GovernmentReportingSellChargeCode", cost.E6_SellGovtChargeCode, consolCostLine.GovernmentReportingSellChargeCode);
				AssertEquals($"{comment} GovernmentReportingCostChargeCode", cost.E6_CostGovtChargeCode, consolCostLine.GovernmentReportingCostChargeCode);
			}
			else
			{
				AssertEquals($"{comment} GovernmentReportingSellChargeCode", null, consolCostLine.GovernmentReportingSellChargeCode);
				AssertEquals($"{comment} GovernmentReportingCostChargeCode", null, consolCostLine.GovernmentReportingCostChargeCode);
			}

			AssertEquals($"{comment} RatingBehaviour", cost.E6_RatingBehaviour, consolCostLine.RatingBehaviour.Code);
			AssertEquals($"{comment} Consol Cost Owner Code", cost.ConsolCostOwner.GS_Code, consolCostLine.CostOwner.Code);
			AssertEquals($"{comment} Consol Cost Owner Name", cost.ConsolCostOwner.GS_FullName, consolCostLine.CostOwner.Name);

			if (enableSupplyType)
			{
				var expectedSupplyType = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.FindByCode(cost.E6_SupplyType);
				AssertEquals($"{comment} Consol Cost SupplyType", expectedSupplyType?.Code ?? cost.E6_SupplyType, consolCostLine.SupplyType.Code);
				AssertEquals($"{comment} Consol Cost SupplyType", expectedSupplyType?.Description ?? ZString.Empty, consolCostLine.SupplyType.Description);
			}
			else
			{
				AssertEquals($"{comment} Consol Cost SupplyType", null, consolCostLine.SupplyType);
			}
		}

		public void TestGenerateConsolCostLinesWithTaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var costTaxBranch = TestObjectCreator.CreateBranch("B1", "CostTaxBranch", GlbCompany.CurrentCompany);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Shipments.AddNew();
				Factory.Save();

				var apportionment = new ApportionmentListing(Factory, consol);
				var cost = apportionment.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 60m;
				cost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
				cost.E6_GB_CostTaxBranch = costTaxBranch.PK;
				Factory.Save();

				var generatedConsolCosts = new ConsolCostsAdapter().Generate(consol, DefaultDataObjectWriterStrategy.TestInstance);

				AssertEquals(costTaxBranch.GB_Code, generatedConsolCosts.ConsolCostLineCollection[0].CostTaxBranch.Code);
				AssertEquals(costTaxBranch.GB_BranchName, generatedConsolCosts.ConsolCostLineCollection[0].CostTaxBranch.Name);
			}
		}

		public void TestGenerateConsolCostLinesWithPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var shipment2 = consol.Shipments.AddNew();
				Factory.Save();

				var apportionments = new ApportionmentListing(Factory, consol);
				var cost1 = apportionments.CostsCollection.TryAddNew();
				cost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost1.E6_OSCostAmount = 60m;
				cost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
				cost1.E6_PlaceOfSupply = "JH";
				cost1.E6_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;

				JobConsolCost cost2 = apportionments.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
				cost2.E6_OSCostAmount = 90m;
				cost2.E6_OH_Creditor = TestObjectCreator.Creditor2.PK;
				cost2.E6_PlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
				cost2.E6_PlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;

				Factory.Save();

				// Load in a new Factory to Generate from
				var factoryToGenerate = new BusinessObjectFactory();
				var consolToGenerate = factoryToGenerate.Load<ForwardingConsol>(consol.PK);

				var generatedConsolCosts = new ConsolCostsAdapter().Generate(consol, DefaultDataObjectWriterStrategy.TestInstance);

				AssertEquals("JH", generatedConsolCosts.ConsolCostLineCollection[0].PlaceOfSupply.Location.Code);
				AssertEquals("Jharkhand", generatedConsolCosts.ConsolCostLineCollection[0].PlaceOfSupply.Location.Description);
				AssertEquals(PlaceOfSupplyTypes.State.Code, generatedConsolCosts.ConsolCostLineCollection[0].PlaceOfSupply.LocationType.Code);
				AssertEquals(PlaceOfSupplyTypes.State.Description, generatedConsolCosts.ConsolCostLineCollection[0].PlaceOfSupply.LocationType.Description);

				AssertEquals(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, generatedConsolCosts.ConsolCostLineCollection[1].PlaceOfSupply.Location.Code);
				AssertEquals(PlaceOfSupplyListProvider.Descriptions.OutsideTheLoginCountry, generatedConsolCosts.ConsolCostLineCollection[1].PlaceOfSupply.Location.Description);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, generatedConsolCosts.ConsolCostLineCollection[1].PlaceOfSupply.LocationType.Code);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Description, generatedConsolCosts.ConsolCostLineCollection[1].PlaceOfSupply.LocationType.Description);
			}
		}

		delegate void AddConsolCostsDelegate(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection);

		#region Insert

		public void TestImportConsolCosts_Insert_ShipmentsWithValidation()
		{
			AssertImportConsolCosts_Insert_ShipmentsWithValidation();
		}

		[TestDate(2019, 03, 24)]
		public void TestImportConsolCosts_Insert_PlaceOfSupply()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				AssertImportConsolCosts_Insert_ShipmentsWithValidation();
			}
		}

		void AssertImportConsolCosts_Insert_ShipmentsWithValidation()
		{
			BusinessObjectFactory factory = CreateFactoryForImporting();
			GlbDepartment.GetCurrentDepartment(factory).GE_Misc = false;

			TestObjectCreator creator = new TestObjectCreator(factory);
			ForwardingConsol consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001001");

			// generate a universal shipment with jobs
			var shipment1 = creator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);
			var job1 = creator.CreateJob(shipment1);
			var shipment2 = creator.CreateShipment("S00001002", "AUSYD", "NZAKL", consol);
			var job2 = creator.CreateJob(shipment2);

			Shipment universalShipment = GetUniversalShipmentWithConsolCosts(factory, creator, GlbCompany.CurrentCompany, consol, AddConsolCostsForInsert);

			var consolPK = ImportConsolCosts(factory, consol, universalShipment, null);

			AssertEquals(Logger.GetErrors(), false, Logger.HasErrors);
			AssertEquals(Logger.GetWarnings(), false, Logger.HasWarnings);
		}

		public void TestImportConsolCosts_Insert_ShipmentsNoValidation()
		{
			BusinessObjectFactory factory = CreateFactoryForImporting();
			GlbDepartment.GetCurrentDepartment(factory).GE_Misc = false;

			// simulate a service task driven import by suspending validation on the factory
			factory.SuspendValidation();

			TestObjectCreator creator = new TestObjectCreator(factory);
			ForwardingConsol consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001001");

			// generate a universal shipment with jobs
			var shipment1 = creator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);
			var job1 = creator.CreateJob(shipment1);
			var shipment2 = creator.CreateShipment("S00001002", "AUSYD", "NZAKL", consol);
			var job2 = creator.CreateJob(shipment2);

			Shipment universalShipment = GetUniversalShipmentWithConsolCosts(factory, creator, GlbCompany.CurrentCompany, consol, AddConsolCostsForInsert);

			var consolPK = ImportConsolCosts(factory, consol, universalShipment, null);

			AssertEquals(Logger.GetErrors(), false, Logger.HasErrors);
			AssertEquals(Logger.GetWarnings(), false, Logger.HasWarnings);
		}

		public void TestImportConsolCosts_Insert_NoShipmentsWithValidation()
		{
			BusinessObjectFactory factory = CreateFactoryForImporting();

			TestObjectCreator creator = new TestObjectCreator(factory);
			ForwardingConsol consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001001");

			// generate a universal shipment without any jobs
			Shipment universalShipment = GetUniversalShipmentWithConsolCosts(factory, creator, GlbCompany.CurrentCompany, consol, AddConsolCostsForInsert);

			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Error - Unapportioned Amount: Please ensure that this Cost Amount is fully apportioned.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
Error - Unapportioned Amount: Please ensure that this Cost Amount is fully apportioned.";

			var consolPK = ImportConsolCosts(factory, consol, universalShipment, exceptionMessage);

			AssertEquals(Logger.GetErrors(), false, Logger.HasErrors);
			AssertEquals(Logger.GetWarnings(), false, Logger.HasWarnings);
		}

		public void TestImportConsolCosts_Insert_NoShipmentsNoValidation()
		{
			BusinessObjectFactory factory = CreateFactoryForImporting();

			// simulate a service task driven import by suspending validation on the factory
			factory.SuspendValidation();

			TestObjectCreator creator = new TestObjectCreator(factory);
			ForwardingConsol consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001001");

			// generate a universal shipment without any jobs
			Shipment universalShipment = GetUniversalShipmentWithConsolCosts(factory, creator, GlbCompany.CurrentCompany, consol, AddConsolCostsForInsert);

			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Error - Unapportioned Amount: Please ensure that this Cost Amount is fully apportioned.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
Error - Unapportioned Amount: Please ensure that this Cost Amount is fully apportioned.";

			var consolPK = ImportConsolCosts(factory, consol, universalShipment, exceptionMessage);

			AssertEquals(Logger.GetErrors(), false, Logger.HasErrors);
			AssertEquals(Logger.GetWarnings(), false, Logger.HasWarnings);
		}

		public void TestImportConsolCosts_Insert()
		{
			TestImportConsolCosts(AddConsolCostsForInsert, null);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
		}

		public void TestImportConsolCosts_Insert_ValidationError()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=CAF Creditor=AALSHI, Cost OS Amount=100.00
Error - Department: This department is not valid for the charge code specified on this Job.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=DADF Creditor=AALSHI, Cost OS Amount=200.00
Error - Department: This department is not valid for the charge code specified on this Job.
Warning - Charge Code: The DADF code is not a Consol Level Charge. Creditor only defaults for Consol Level charges.
Error - Supplier Cost Reference: This consol cost has a Creditor and an AP Invoice number the same as another cost, but the Supplier Cost Reference does not match.";
			TestImportConsolCosts(AddConsolCostsForInsert_ValidationError, exceptionMessage);
		}

		public void TestImportConsolCosts_Insert_WithWarnings()
		{
			ZGuid consolPK = TestImportConsolCosts(AddConsolCostsForInsert_WithWarning, null);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has Warnings", Logger.HasWarnings);
			var expectedWarning = string.Format(@"Warning - Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Tax Rate is read-only and was not updated.", consolPK);
			AssertContains(expectedWarning, Logger.Logs);
			expectedWarning = string.Format(@"Warning - Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Tax Amount is read-only and was not updated.", consolPK);
			AssertContains(expectedWarning, Logger.Logs);
		}

		public void TestImportConsolCosts_Insert_WithJobCreationException_WhenRunningCW1()
		{
			var originalIsUserInteractive = ZArchitecture.Environment.Globals.IsUserInteractive;
			using (new DisposableAction(() => ZArchitecture.Environment.Globals.IsUserInteractive = originalIsUserInteractive))
			{
				ZArchitecture.Environment.Globals.IsUserInteractive = true;
				TestImportConsolCosts_Insert_WithJobCreationException_Core();
				AssertEquals(false, ErrorReporter.HasBeenReported("TryLoadOrCreateJobWithMutexAndTrackingCore_1"));
			}
		}

		public void TestImportConsolCosts_Insert_WithJobCreationException_WhenNotRunningCW1()
		{
			var originalIsUserInteractive = ZArchitecture.Environment.Globals.IsUserInteractive;
			using (new DisposableAction(() => ZArchitecture.Environment.Globals.IsUserInteractive = originalIsUserInteractive))
			{
				ZArchitecture.Environment.Globals.IsUserInteractive = false;
				var exceptionMessage = @"You have created the job S00001001 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001001 to continue.";

				var exp = AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => TestImportConsolCosts_Insert_WithJobCreationException_Core());
				AssertEquals(exceptionMessage, exp.Message);
				AssertEquals(false, ErrorReporter.HasBeenReported("TryLoadOrCreateJobWithMutexAndTrackingCore_1"));
			}
		}

		public void TestImportConsolCosts_Insert_WithJobCreationException_WhenRunningUMIServiceTask()
		{
			var originalIsUserInteractive = ZArchitecture.Environment.Globals.IsUserInteractive;
			using (new DisposableAction(() => ZArchitecture.Environment.Globals.IsUserInteractive = originalIsUserInteractive))
			{
				ZArchitecture.Environment.Globals.IsUserInteractive = false;
				using (Env.Instance.TemporaryServiceTaskContext("UMI", true))
				{
					var exceptionMessage = @"You have created the job S00001001 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001001 to continue.";

					var exp = AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => TestImportConsolCosts_Insert_WithJobCreationException_Core());
					AssertEquals(exceptionMessage, exp.Message);
					AssertEquals(false, ErrorReporter.HasBeenReported("TryLoadOrCreateJobWithMutexAndTrackingCore_1"));
					ErrorReporter.Clear();
				}
			}
		}

		void TestImportConsolCosts_Insert_WithJobCreationException_Core()
		{
			var factory = CreateFactoryForImporting();
			var creator = new TestObjectCreator(factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001001");
			var shipment = creator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);

			var universalShipment = GetUniversalShipmentWithConsolCosts(factory, creator, GlbCompany.CurrentCompany, consol, AddConsolCostsForInsert);

			var newFactory = new BusinessObjectFactory();
			var newShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			var job = new TestObjectCreator(newFactory).CreateJob(newShipment);

			var exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
You have created the job S00001001 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001001 to continue.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
You have created the job S00001001 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001001 to continue.";

			try
			{
				ImportConsolCosts(factory, consol, universalShipment, exceptionMessage);
			}
			finally
			{
				job.Dispose();
			}
		}

		void AddConsolCostsForInsert(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False,
																"JH", PlaceOfSupplyTypes.State.Code);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False,
																PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, PlaceOfSupplyTypes.PredefinedRule.Code);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Insert;
			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Insert;
		}

		void AddConsolCostsForInsert_WithWarning(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, "GST",
																1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, "GST",
																1m, 200.00m, 200.00m, "AUD", 20.00m, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Insert;
			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Insert;
		}

		void AddConsolCostsForInsert_ValidationError(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine1 = GetConsolCostLine("CAF", "001", InvoiceDate, InvoiceDate, "GST",
																1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("DADF", "001", InvoiceDate, InvoiceDate, "GST",
																1m, 200.00m, 200.00m, "AUD", 20.00m, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Insert;
			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Insert;
		}

		#endregion

		#region Update

		public void TestImportConsolCosts_Update()
		{
			TestImportConsolCosts(AddConsolCostsForUpdate, null);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
		}

		public void TestImportConsolCosts_Update_ConsolCostNotFound()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Consol cost not found when updating consol cost Line.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
Consol cost not found when updating consol cost Line.";
			TestImportConsolCosts(AddConsolCostForUpdate_ConsolCostNotFound, exceptionMessage);
		}

		public void TestImportConsolCosts_Update_NoMatchingCriteria()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=ABIGAS, Cost OS Amount=100.00
No matching criteria specified.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=ABIGAS, Cost OS Amount=200.00
No matching criteria specified.";
			TestImportConsolCosts(AddConsolCostForUpdate_NoMatchingCriteria, exceptionMessage);
		}

		public void TestImportConsolCosts_Update_ValidationError()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=ABIGAS, Cost OS Amount=100.00
Error - Creditor: The selected Creditor is no longer valid. Please choose a new Creditor from the list.
Error - Creditor: Enter a valid Creditor.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=ABIGAS, Cost OS Amount=200.00
Error - Creditor: The selected Creditor is no longer valid. Please choose a new Creditor from the list.
Error - Creditor: Enter a valid Creditor.";
			TestImportConsolCosts(AddConsolCostForUpdate_ValidationError, exceptionMessage);
		}

		public void TestImportConsolCosts_Update_MultipleConsolCosts()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Multiple consol costs found when updating consol cost Line.";
			TestImportConsolCosts(AddConsolCostForUpdate_MultipleConsolCosts, exceptionMessage);
		}

		public void TestImportConsolCosts_Insert_GatewayConsol()
		{
			TestImportConsolCosts(SetUpGatewayConsolForInsert, null);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
		}

		public void TestImportConsolCosts_Update_GatewayConsol()
		{
			TestImportConsolCosts(SetUpGatewayConsolForUpdate, null);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
		}

		public void TestImportConsolCosts_Delete_GatewayConsol()
		{
			TestImportConsolCosts(SetUpGatewayConsolForDelete, null, new int[] { 0, 0 });
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
		}

		void AddConsolCostsForUpdate(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 30m;
			cost.E6_CostReference = "22222";

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Update;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1.FieldName = "SupplierReference";
			matchingCriteria1.Value = "11111";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Update;
			consolCostLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2.FieldName = "SupplierReference";
			matchingCriteria2.Value = "22222";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddConsolCostForUpdate_ConsolCostNotFound(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Update;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1.FieldName = "SupplierReference";
			matchingCriteria1.Value = "11111";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Update;
			consolCostLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2.FieldName = "SupplierReference";
			matchingCriteria2.Value = "22222";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddConsolCostForUpdate_ValidationError(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 30m;
			cost.E6_CostReference = "22222";

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "ABIGAS", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "ABIGAS", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Update;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1.FieldName = "SupplierReference";
			matchingCriteria1.Value = "11111";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Update;
			consolCostLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2.FieldName = "SupplierReference";
			matchingCriteria2.Value = "22222";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddConsolCostForUpdate_NoMatchingCriteria(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 30m;
			cost.E6_CostReference = "22222";

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "ABIGAS", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "ABIGAS", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Update;

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Update;
		}

		void AddConsolCostForUpdate_MultipleConsolCosts(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 30m;
			cost.E6_CostReference = "22222";

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Update;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "Creditor";
			matchingCriteria1.Value = "AALSHI";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
		}

		void SetUpGatewayConsolForInsert(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Insert;

			var gatewayConsol = consol as ForwardingConsol;
			AssertNotNull("Must be a ForwardingConsol", gatewayConsol);
			gatewayConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = gatewayConsol.JK_RL_NKLoadPort;
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);
			Assert("Is Gateway", gatewayConsol.IsGateway());
		}

		void SetUpGatewayConsolForUpdate(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			SetUpGatewayConsol(factory, creator, consol, consolCostLineCollection, InstructionType.Update);
		}

		void SetUpGatewayConsolForDelete(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			SetUpGatewayConsol(factory, creator, consol, consolCostLineCollection, InstructionType.Delete);
		}

		void SetUpGatewayConsol(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection, InstructionType action)
		{
			var gatewayConsol = consol as ForwardingConsol;
			AssertNotNull("Must be a ForwardingConsol", gatewayConsol);
			gatewayConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = gatewayConsol.JK_RL_NKLoadPort;
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			var gatewayDepartment = factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GIS");
			AssertNotNull(gatewayDepartment);
			var frtCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK);
			AssertNotNull("FRT Charge Code should exist", frtCode);

			var gatewayBillingJob = creator.CreateJob(gatewayConsol, false);
			gatewayBillingJob.JH_GE = gatewayDepartment.PK;

			var jobChargeToBecomeConsolCost = gatewayBillingJob.Charges.AddNew();
			jobChargeToBecomeConsolCost.JR_AC = frtCode.PK;
			jobChargeToBecomeConsolCost.JR_OH_SellAccount = gatewayConsol.SendingForwarderPK;
			jobChargeToBecomeConsolCost.JR_RX_NKSellCurrency = "AUD";
			jobChargeToBecomeConsolCost.JR_OSSellAmt = 500m;
			jobChargeToBecomeConsolCost.JR_JH_InternalJob = gatewayBillingJob.PK;

			Enterprise.Accounting.Business.JobInvoicing.GatewaySellToCostSynchroniser.Synchronise(gatewayBillingJob);
			Assert("HasConsolCosts", gatewayConsol.HasConsolCosts(GlbCompany.CurrentCompany));
			Assert("Is Gateway", gatewayConsol.IsGateway());

			// create regular consol costs, i.e. non sell apportionment costs.
			var apportionmentList = new ApportionmentListing(consol.Factory, consol, false);
			var cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = gatewayConsol.SendingForwarderPK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = gatewayConsol.SendingForwarderPK;
			cost.E6_OSCostAmount = 20m;
			cost.E6_CostReference = "22222";

			var creditorCode = gatewayConsol.SendingForwarder.OH_Code;
			var consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, creditorCode, ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = action;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			var matchingCriteria11 = new MatchingCriteria();
			matchingCriteria11.FieldName = "Creditor";
			matchingCriteria11.Value = creditorCode;
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria11);
			var matchingCriteria12 = new MatchingCriteria();
			matchingCriteria12.FieldName = "ChargeCode";
			matchingCriteria12.Value = "FRT";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria12);

			var consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);
			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = action;
			consolCostLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			var matchingCriteria21 = new MatchingCriteria();
			matchingCriteria21.FieldName = "Creditor";
			matchingCriteria21.Value = creditorCode;
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria21);
			var matchingCriteria22 = new MatchingCriteria();
			matchingCriteria22.FieldName = "ChargeCode";
			matchingCriteria22.Value = "BAF";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria22);
		}

		#endregion

		#region UpdateAndInsertIfNotFound

		public void TestImportConsolCosts_UpdateAndInsertIfNotFound()
		{
			TestImportConsolCosts(AddConsolCostsForUpdateAndInsertIfNotFound, null);
			Assert("Has No Errors", !Logger.HasErrors);
			Assert("Has No Warnings", !Logger.HasWarnings);
		}

		public void TestImportConsolCosts_UpdateAndInsertIfNotFound_NoMatchingCriteria()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
No matching criteria specified.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
No matching criteria specified.";
			TestImportConsolCosts(AddConsolCostForUpdateAndInsertIfNotFound_NoMatchingCriteria, exceptionMessage);
		}

		public void TestImportConsolCosts_UpdateAndInsertIfNotFound_MultipleConsolCosts()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Multiple consol costs found when updating consol cost Line.";
			TestImportConsolCosts(AddConsolCostForUpdateAndInsertIfNotFound_MultipleConsolCosts, exceptionMessage);
		}

		public void TestImportConsolCosts_UpdateAndInsertIfNotFound_ValidationError()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=ABIGAS, Cost OS Amount=100.00
Error - Creditor: The selected Creditor is no longer valid. Please choose a new Creditor from the list.
Error - Creditor: Enter a valid Creditor.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=ABIGAS, Cost OS Amount=200.00
Error - Creditor: The selected Creditor is no longer valid. Please choose a new Creditor from the list.
Error - Creditor: Enter a valid Creditor.";
			TestImportConsolCosts(AddConsolCostForUpdateAndInsertIfNotFound_ValidationError, exceptionMessage);
		}

		void AddConsolCostsForUpdateAndInsertIfNotFound(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1.FieldName = "SupplierReference";
			matchingCriteria1.Value = "11111";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			consolCostLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2.FieldName = "SupplierReference";
			matchingCriteria2.Value = "22222";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddConsolCostForUpdateAndInsertIfNotFound_NoMatchingCriteria(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
		}

		void AddConsolCostForUpdateAndInsertIfNotFound_MultipleConsolCosts(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 30m;
			cost.E6_CostReference = "22222";

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "Creditor";
			matchingCriteria1.Value = "AALSHI";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
		}

		void AddConsolCostForUpdateAndInsertIfNotFound_ValidationError(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_CostReference = "11111";

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "ABIGAS", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "ABIGAS", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);
			matchingCriteria1.FieldName = "SupplierReference";
			matchingCriteria1.Value = "11111";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
			consolCostLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
			matchingCriteria2.FieldName = "SupplierReference";
			matchingCriteria2.Value = "22222";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		#endregion

		#region Delete

		public void TestImportConsolCosts_Delete()
		{
			TestImportConsolCosts(AddConsolCostsForDelete, null, new int[] { 0, 0 });
		}

		public void TestImportConsolCosts_Delete_NoConsolCostsFound()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
No consol costs found when deleting consol cost Line.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
No consol costs found when deleting consol cost Line.";
			TestImportConsolCosts(AddConsolCostsForDelete_NoConsolCostsFound, exceptionMessage);
		}

		public void TestImportConsolCosts_Delete_NoMatchingCriteria()
		{
			string exceptionMessage = @"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
No matching criteria specified.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
No matching criteria specified.";
			TestImportConsolCosts(AddConsolCostsForDelete_NoMatchingCriteria, exceptionMessage);
		}

		void AddConsolCostsForDelete(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OSCostAmount = 10m;

			cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK).PK;
			cost.E6_OSCostAmount = 10m;

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Delete;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Delete;
			consolCostLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddConsolCostsForDelete_NoConsolCostsFound(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Delete;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = "ChargeCode";
			matchingCriteria1.Value = "FRT";
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Delete;
			consolCostLine2.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria2 = new MatchingCriteria();
			matchingCriteria2.FieldName = "ChargeCode";
			matchingCriteria2.Value = "BAF";
			consolCostLine2.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria2);
		}

		void AddConsolCostsForDelete_NoMatchingCriteria(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			ApportionmentListing apportionmentList = new ApportionmentListing(consol.Factory, consol);
			JobConsolCost cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK).PK;

			cost = apportionmentList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.LoadAccChargeCode("BAF", GlbCompany.CurrentCompany.PK).PK;

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null,
																1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			consolCostLineCollection.Add(consolCostLine1);
			ConsolCostLine consolCostLine2 = GetConsolCostLine("BAF", "002", InvoiceDate, InvoiceDate, null,
																1m, 200.00m, 200.00m, "AUD", null, "AALSHI", ZBool.False,
																"SHP", ZBool.False, "ALL", "7898789", ZBool.False);
			consolCostLineCollection.Add(consolCostLine2);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = InstructionType.Delete;

			consolCostLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine2.ImportMetaData.Instruction = InstructionType.Delete;
		}

		#endregion

		#region Mapping and Exception Handling

		public void TestImportConsolCosts_NoMappingInvalidChargeCode_Update()
		{
			var errorMessage = "Unable to determine primary key from code: E6_AC_ChargeCode, XXX";
			try
			{
				RunTestImportConsolCostsWithOptionalMapping("ChargeCode", "XXX", InstructionType.Update);
			}
			catch (DataObjectReadFailureException)
			{
				AssertContains("Import Log should contain the following error message: " + errorMessage, errorMessage, Logger.Logs);
			}
		}

		public void TestImportConsolCosts_NoMappingInvalidCreditor_Update()
		{
			var errorMessage = "Unable to determine primary key from code: E6_OH_Creditor, INVALID";
			try
			{
				RunTestImportConsolCostsWithOptionalMapping("Creditor", "INVALID", InstructionType.UpdateAndInsertIfNotFound);
			}
			catch (DataObjectReadFailureException)
			{
				AssertContains("Import Log should contain the following error message: " + errorMessage, errorMessage, Logger.Logs);
			}
		}

		public void TestImportConsolCosts_WithMappingInvalidChargeCode_UpdateAndInsert()
		{
			var mapMessage = "Information - Line 0: Mapped Charge Code code 'XXX' to 'FRT'.";
			var doMapping = true;

			RunTestImportConsolCostsWithOptionalMapping("ChargeCode", "XXX", InstructionType.UpdateAndInsertIfNotFound, doMapping);
			AssertContains("Import Log should contain the mapping message: " + mapMessage, mapMessage, Logger.Logs);
		}

		public void TestImportConsolCosts_WithMappingInvalidCreditor_UpdateAndInsert()
		{
			var mapMessage = "Information - Line 0: Mapped Organization code 'INVALID' to 'AALSHI'.";
			var doMapping = true;

			RunTestImportConsolCostsWithOptionalMapping("Creditor", "INVALID", InstructionType.UpdateAndInsertIfNotFound, doMapping);
			AssertContains("Import Log should contain the mapping message: " + mapMessage, mapMessage, Logger.Logs);
		}

		public void TestImportHelper_MatchingCriteriaException_InvalidColumnName()
		{
			var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema("JobConsol");
			var fakeColumn = new SchemaGuidColumn(tableSchema, "JK_", 1, ZGuid.Empty, false);
			var expectedMsg = "Invalid column name for matching criteria. Column: JK_ Value: ";
			TestObjectCreator creator = new TestObjectCreator(Factory);

			try
			{
				var result = Helpers.GetValue(fakeColumn, ZString.Empty, creator.ABIGAS.PK, Factory);
			}
			catch (MatchingCriteriaException ex)
			{
				AssertEquals("Expected MatchingCriteriaException with message: ", expectedMsg, ex.Message);
			}
		}

		public void TestImportHelper_MatchingCriteriaException_InvalidSchemaColumn()
		{
			var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema("JobConsol");
			var fakeColumn = new SchemaGuidColumn(tableSchema, "XX_YY_TestColumn", 1, ZGuid.Empty, false);
			var expectedMsg = "Could not find schema related to column while performing matching criteria. Column: XX_YY_TestColumn Value: ";
			TestObjectCreator creator = new TestObjectCreator(Factory);

			try
			{
				var result = Helpers.GetValue(fakeColumn, ZString.Empty, creator.ABIGAS.PK, Factory);
			}
			catch (MatchingCriteriaException ex)
			{
				AssertEquals("Expected MatchingCriteriaException with message: ", expectedMsg, ex.Message);
			}
		}

		public void TestImportHelper_MatchingCriteriaException_InvalidCodeColumn()
		{
			var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema("JobConsol");
			var fakeColumn = new SchemaGuidColumn(tableSchema, "JH_JK_", 1, ZGuid.Empty, false);
			var expectedMsg = "Schema must contain a code column in order to perform matching criteria. Schema: Enterprise.ZArchitecture.Schema.JobConsolSchema Column: JH_JK_ Value: ";
			TestObjectCreator creator = new TestObjectCreator(Factory);

			try
			{
				var result = Helpers.GetValue(fakeColumn, ZString.Empty, creator.ABIGAS.PK, Factory);
			}
			catch (MatchingCriteriaException ex)
			{
				AssertEquals("Expected MatchingCriteriaException with message: ", expectedMsg, ex.Message);
			}
		}

		void RunTestImportConsolCostsWithOptionalMapping(string matchingCriteriaField, string matchingCriteriaValue, InstructionType instruction, bool doMapping = false)
		{
			ConsolCostsAdapter adapter = new ConsolCostsAdapter();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			ForwardingConsol consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001001");

			GlbDepartment.GetCurrentDepartment(factory).GE_Misc = false;
			var shipment1 = creator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);
			var job1 = creator.CreateJob(shipment1, false);

			Shipment universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.ConsolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.ConsolCosts.SetConsolCostLineCollection(() => new List<ConsolCostLine>());
			universalShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = universalShipment;

			ConsolCostLine consolCostLine1 = GetConsolCostLine("FRT", "001", InvoiceDate, InvoiceDate, null, 1m, 100.00m, 100.00m, "AUD", null, "AALSHI", ZBool.False, "SHP", ZBool.False, "ALL", "1234321", ZBool.False);
			universalShipment.ConsolCosts.ConsolCostLineCollection.Add(consolCostLine1);

			consolCostLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			consolCostLine1.ImportMetaData.Instruction = instruction;
			consolCostLine1.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
			MatchingCriteria matchingCriteria1 = new MatchingCriteria();
			matchingCriteria1.FieldName = matchingCriteriaField;
			matchingCriteria1.Value = matchingCriteriaValue;
			consolCostLine1.ImportMetaData.MatchingCriteriaCollection.Add(matchingCriteria1);

			if (doMapping)
			{
				var notifications = new NotificationBuffer();
				var importContext = new ValueObjectImportContext(Factory, notifications);
				var orgMapping = Factory.Load<OrgHeader>(importContext.Converter.MappingOrgPK);

				OrgPatternMatchOverride chargeOverride = orgMapping.CreatePatternMatchOverrideForTest();
				chargeOverride.OO_Relationship = (matchingCriteriaField == "ChargeCode" ? Constants.OrgPatternMatchOverrideRelationships.ChargeCodes : Constants.OrgPatternMatchOverrideRelationships.Organisation);
				chargeOverride.OO_LocalCode = (matchingCriteriaField == "ChargeCode" ? creator.FRT.AC_Code : ZString.Empty);
				chargeOverride.OO_ForeignCode = matchingCriteriaValue;
				chargeOverride.OO_LocalGuid = (matchingCriteriaField == "Creditor" ? creator.AALSHI.PK : ZGuid.Empty);
			}
			Factory.Save();
			adapter.ImportConsolCosts(factory, Logger, universalShipment, consol.PK, JobConsolSchema.Constants.Prefix);
		}

		#region Test Import Charges When Job Is Ready For Financial Closure

		public void TestImportConsolCosts_Insert_WhenJobIsReadyForFinancialClosure()
		{
			string exceptionMessage =
@"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Error - Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
Error - Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.";

			using (Env.Instance.TemporaryServiceTaskContext("UMI", true))
			{
				TestImportConsolCosts(PrepareForInsert_WhenJobIsReadyForFinancialClosure, exceptionMessage);
			}
			ErrorReporter.Clear();
		}

		public void TestImportConsolCosts_Update_WhenJobIsReadyForFinancialClosure()
		{
			string exceptionMessage =
@"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Error - Agent Declared Cost: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Invoice Date: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Cost Govt Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Override Rating: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Local Cost Amount: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Overseas Cost Amount: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Payment Date: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Sell Govt Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
Error - Agent Declared Cost: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Invoice Date: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Cost Govt Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Override Rating: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Local Cost Amount: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Overseas Cost Amount: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Payment Date: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Sell Govt Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.";

			using (Env.Instance.TemporaryServiceTaskContext("UMI", true))
			{
				TestImportConsolCosts(PrepareForUpdate_WhenJobIsReadyForFinancialClosure, exceptionMessage);
			}
			ErrorReporter.Clear();
		}

		public void TestImportConsolCosts_UpdateAndInsertIfNotFound_WhenJobIsReadyForFinancialClosure()
		{
			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;
			string exceptionMessage =
@"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
Error - Agent Declared Cost: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Invoice Date: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Cost Govt Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Override Rating: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Local Cost Amount: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Overseas Cost Amount: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Payment Date: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Error - Sell Govt Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
Error - Charge Code: You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.";

			using (Env.Instance.TemporaryServiceTaskContext("UMI", true))
			{
				TestImportConsolCosts(PrepareForUpdateAndInsertIfNotFound_WhenJobIsReadyForFinancialClosure, exceptionMessage);
			}
			ErrorReporter.Clear();
		}

		public void TestImportConsolCosts_Delete_WhenJobIsReadyForFinancialClosure()
		{
			string exceptionMessage =
@"Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=FRT Creditor=AALSHI, Cost OS Amount=100.00
You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.

Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code=JK Charge Code=BAF Creditor=AALSHI, Cost OS Amount=200.00
You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.
";

			using (Env.Instance.TemporaryServiceTaskContext("UMI", true))
			{
				TestImportConsolCosts(PrepareForDelete_WhenJobIsReadyForFinancialClosure, exceptionMessage);

				Assert("Has Errors", Logger.HasErrors);
				Assert("Has No Warnings", !Logger.HasWarnings);
			}
			ErrorReporter.Clear();
		}

		void PrepareForUpdateAndInsertIfNotFound_WhenJobIsReadyForFinancialClosure(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			AddConsolCostsForUpdateAndInsertIfNotFound(factory, creator, consol, consolCostLineCollection);
			PrepareJobIsReadyForFinancialClosure(factory);
		}

		void PrepareForDelete_WhenJobIsReadyForFinancialClosure(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			AddConsolCostsForDelete(factory, creator, consol, consolCostLineCollection);
			PrepareJobIsReadyForFinancialClosure(factory);
		}

		void PrepareForUpdate_WhenJobIsReadyForFinancialClosure(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			AddConsolCostsForUpdate(factory, creator, consol, consolCostLineCollection);
			PrepareJobIsReadyForFinancialClosure(factory);
		}

		void PrepareForInsert_WhenJobIsReadyForFinancialClosure(BusinessObjectFactory factory, TestObjectCreator creator, IGenericJobCostPlugIn consol, List<ConsolCostLine> consolCostLineCollection)
		{
			AddConsolCostsForInsert(factory, creator, consol, consolCostLineCollection);
			PrepareJobIsReadyForFinancialClosure(factory);
		}

		void PrepareJobIsReadyForFinancialClosure(BusinessObjectFactory factory)
		{
			var shipment = factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001001"));
			shipment.Job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;

			Assert("Precondition", shipment.Job.IsReadyForFinancialClosure);
		}

		#endregion

		#endregion

		BusinessObjectFactory CreateFactoryForImporting()
		{
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			return new BusinessObjectFactory();
		}

		ZGuid ImportConsolCosts(BusinessObjectFactory factory, ForwardingConsol consol, Shipment universalShipment, string exceptionMessage)
		{
			try
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				universalShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				Logger.TopLevelDataObject = universalShipment;

				var adapter = new ConsolCostsAdapter();
				adapter.ImportConsolCosts(factory, Logger, universalShipment, consol.PK, JobConsolSchema.Constants.Prefix);

				if (exceptionMessage != null)
				{
					Assert(string.Format("Expecting exception but none was thrown.\r\nExpected Exception Message: {0}", exceptionMessage), false);
				}

				factory.Save();

				AssertConsolCosts(factory, universalShipment, consol, null);
			}
			catch (DataObjectReadFailureException ex)
			{
				AssertNotNull(string.Format("exceptionMessage should not be null if DataObjectReadFailureException is thrown.\r\nThrown Exception Message: {0}", ex.Message), exceptionMessage);
				AssertEquals("Exception message", string.Format(exceptionMessage, consol.PK.ToString()), ex.Message);
			}

			return consol.PK;
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestImportConsolCostsWhenAnyShipmentGetClosed()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;

			var consol = PrepareConsolWithConsolCost();

			var universalShipment = GetUniversalShipmentWithConsolCosts(Factory, TestObjectCreator, GlbCompany.CurrentCompany, consol
				, (f, c, consol, consolCostLineCollection) => {
					consolCostLineCollection.Add(CreateInsert());
					consolCostLineCollection.Add(CreateUpdate());
					consolCostLineCollection.Add(CreateUpdateAndInsertIfNotFound_Found());
					consolCostLineCollection.Add(CreateUpdateAndInsertIfNotFound_NotFound());
					consolCostLineCollection.Add(CreateDelete());
				}
			);
			universalShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Logger.TopLevelDataObject = universalShipment;
			new ConsolCostsAdapter().ImportConsolCosts(Factory, Logger, universalShipment, consol.PK, JobConsolSchema.Constants.Prefix);
			Factory.Save();

			CombineAssertions("When any shipment job is closed by imported company, we should skip INSERT action and record message to log.", () => {
				AssertEquals("HasErrors", false, Logger.HasErrors);
				AssertContains("Logger Warning Message"
					, "The Consol Cost Charges cannot be inserted as at least one of the shipments' Job is closed. Closed Shipment(s):['S1002','S1003']"
					, Logger.GetWarnings());
				AssertEquals("Consol cost number should be changed from 3 to 2 by DELETE action.", 2, consol.GetApportionments().CostsCollection.Count);
				AssertEquals("Consol costs' charge code should be change to FRT by UPDATE action.", "FRT", consol.GetApportionments().CostsCollection[0].ChargeCode.AC_Code);
				AssertEquals("Consol costs' charge code should be change to FRT by UpdateAndInsertIfNotFound action.", "FRT", consol.GetApportionments().CostsCollection[1].ChargeCode.AC_Code);
			});

			ForwardingConsol PrepareConsolWithConsolCost()
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001001");
				AttachShipmentToConsol(consol, "S1001", new[] {
					(GlbCompany.CurrentCompany, JobHeaderStatus.Working.Code)
					, (TestObjectCreator.NonCurrentCompany, JobHeaderStatus.Closed.Code)
				});

				var apportionmentList = new ApportionmentListing(consol.Factory, consol);
				foreach (var chargeCode in new[] { TestObjectCreator.CC1, TestObjectCreator.CC2, TestObjectCreator.CC3 })
				{
					var cost = apportionmentList.CostsCollection.TryAddNew();
					cost.E6_AC_ChargeCode = chargeCode.PK;
					cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
					cost.E6_OSCostAmount = 100m;
				}
				Factory.Save();

				AttachShipmentToConsol(consol, "S1002", new[] {
					(GlbCompany.CurrentCompany, JobHeaderStatus.Closed.Code)
					, (TestObjectCreator.NonCurrentCompany, JobHeaderStatus.Closed.Code)
				});
				AttachShipmentToConsol(consol, "S1003", new[] {
					(GlbCompany.CurrentCompany, JobHeaderStatus.Closed.Code)
					, (TestObjectCreator.NonCurrentCompany, JobHeaderStatus.Closed.Code)
				});
				Factory.Save();

				return consol;
			}

			ConsolCostLine CreateInsert()
			{
				var consolCostLine = GetConsolCostLine(
					"BAF", "APINV_001", InvoiceDate, InvoiceDate, "GST",
					1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
					"SHP", ZBool.False, "ALL", "1234321", ZBool.False);

				consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				consolCostLine.ImportMetaData.Instruction = InstructionType.Insert;

				return consolCostLine;
			}

			ConsolCostLine CreateUpdate()
			{
				var consolCostLine = GetConsolCostLine(
					"FRT", "APINV_001", InvoiceDate, InvoiceDate, "GST",
					1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
					"SHP", ZBool.False, "ALL", "1234321", ZBool.False);

				consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				consolCostLine.ImportMetaData.Instruction = InstructionType.Update;
				consolCostLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() {
					new MatchingCriteria() { FieldName = "ChargeCode", Value = TestObjectCreator.CC1.AC_Code }
				});

				return consolCostLine;
			}

			ConsolCostLine CreateUpdateAndInsertIfNotFound_Found()
			{
				var consolCostLine = GetConsolCostLine(
					"FRT", "APINV_001", InvoiceDate, InvoiceDate, "GST",
					1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
					"SHP", ZBool.False, "ALL", "1234321", ZBool.False);

				consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				consolCostLine.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
				consolCostLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() {
					new MatchingCriteria() { FieldName = "ChargeCode", Value = TestObjectCreator.CC2.AC_Code }
				});

				return consolCostLine;
			}

			ConsolCostLine CreateUpdateAndInsertIfNotFound_NotFound()
			{
				var consolCostLine = GetConsolCostLine(
					TestObjectCreator.CC2.AC_Code, "APINV_001", InvoiceDate, InvoiceDate, "GST",
					1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
					"SHP", ZBool.False, "ALL", "1234321", ZBool.False);

				consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				consolCostLine.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
				consolCostLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() {
					new MatchingCriteria() { FieldName = "ChargeCode", Value = "BAF" }
				});

				return consolCostLine;
			}

			ConsolCostLine CreateDelete()
			{
				var consolCostLine = GetConsolCostLine(
					 TestObjectCreator.CC3.AC_Code, "APINV_001", InvoiceDate, InvoiceDate, "GST",
					1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
					"SHP", ZBool.False, "ALL", "1234321", ZBool.False);

				consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				consolCostLine.ImportMetaData.Instruction = InstructionType.Delete;
				consolCostLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() {
					new MatchingCriteria() { FieldName = "ChargeCode", Value = TestObjectCreator.CC3.AC_Code }
				});

				return consolCostLine;
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestImportConsolCostsWhenShipmentGetClosedByNonImportedCompany()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;

			var consol = PrepareConsolWithConsolCost();

			var universalShipment = GetUniversalShipmentWithConsolCosts(Factory, TestObjectCreator, GlbCompany.CurrentCompany, consol
				, (f, c, consol, consolCostLineCollection) => {
					consolCostLineCollection.Add(CreateInsert());
					consolCostLineCollection.Add(CreateUpdateAndInsertIfNotFound_NotFound());
				}
			);
			universalShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Logger.TopLevelDataObject = universalShipment;
			new ConsolCostsAdapter().ImportConsolCosts(Factory, Logger, universalShipment, consol.PK, JobConsolSchema.Constants.Prefix);
			Factory.Save();

			CombineAssertions("When shipment job is closed by non-imported company, we do not skip INSERT action.", () => {
				AssertEquals("HasErrors", false, Logger.HasErrors);
				AssertNotContains("Logger Warning Message"
					, "The Consol Cost Charges cannot be inserted as at least one of the shipments' Job is closed."
					, Logger.GetWarnings());
				AssertContainsExactElementsInAnyOrder("FRT & BAF Consol costs should be added by INSRT & UpdateAndInsertIfNotFound action."
					, new[] { "FRT", "BAF" }
					, consol.GetApportionments().CostsCollection.Cast<JobConsolCost>().Select(x => (string)x.ChargeCode.AC_Code));
			});

			ForwardingConsol PrepareConsolWithConsolCost()
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001001");
				AttachShipmentToConsol(consol, "S1001", new[] {
					(GlbCompany.CurrentCompany, JobHeaderStatus.Working.Code)
					, (TestObjectCreator.NonCurrentCompany, JobHeaderStatus.Closed.Code)
				});
				Factory.Save();

				AssertEquals("PreCondition", 0, consol.GetApportionments().CostsCollection.Count);
				Factory.ClearCachedValue<ApportionmentListing>($"ApportionmentListing|{consol.PK.ToString()}");

				return consol;
			}

			ConsolCostLine CreateInsert()
			{
				var consolCostLine = GetConsolCostLine(
					"FRT", "APINV_001", InvoiceDate, InvoiceDate, "GST",
					1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
					"SHP", ZBool.False, "ALL", "1234321", ZBool.False);

				consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				consolCostLine.ImportMetaData.Instruction = InstructionType.Insert;

				return consolCostLine;
			}

			ConsolCostLine CreateUpdateAndInsertIfNotFound_NotFound()
			{
				var consolCostLine = GetConsolCostLine(
					"BAF", "APINV_001", InvoiceDate, InvoiceDate, "GST",
					1m, 100.00m, 100.00m, "AUD", 10.00m, "AALSHI", ZBool.False,
					"SHP", ZBool.False, "ALL", "1234321", ZBool.False);

				consolCostLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				consolCostLine.ImportMetaData.Instruction = InstructionType.UpdateAndInsertIfNotFound;
				consolCostLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>() {
					new MatchingCriteria() { FieldName = "ChargeCode", Value = "BAF" }
				});

				return consolCostLine;
			}
		}

		void AttachShipmentToConsol(ForwardingConsol consol, string shipmentNum, params (GlbCompany Company, string JobStatus)[] jobSettings)
		{
			var shipment = TestObjectCreator.CreateShipment(shipmentNum, "AUSYD", "NZAKL", consol);
			foreach (var jobSetting in jobSettings)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, jobSetting.Company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var shipmentJob = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
					shipmentJob.JH_Status = jobSetting.JobStatus;
				}
			}
		}

		ZGuid TestImportConsolCosts(AddConsolCostsDelegate addConsolCostDelegate, string exceptionMessage, int[] consolCostCounts = null)
		{
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var adapter = new ConsolCostsAdapter();
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			GlbDepartment.GetCurrentDepartment(factory).GE_Misc = false;
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C00001001");

			var shipment1 = creator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);
			var job1 = creator.CreateJob(shipment1, false);
			var shipment2 = creator.CreateShipment("S00001002", "AUSYD", "NZAKL", consol);
			var job2 = creator.CreateJob(shipment2, false);

			var universalShipment = GetUniversalShipmentWithConsolCosts(factory, creator, GlbCompany.CurrentCompany, consol, addConsolCostDelegate);

			try
			{
				universalShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				Logger.TopLevelDataObject = universalShipment;

				var sellApportionmentsForGWConsol = GetSellApportionmentForConsol(factory, consol);

				adapter.ImportConsolCosts(factory, Logger, universalShipment, consol.PK, JobConsolSchema.Constants.Prefix);

				sellApportionmentsForGWConsol.ToList().ForEach(x => Assert("sell apportionment should not be changed during consol cost import", !x.HasChanges));

				if (exceptionMessage != null)
				{
					Assert(string.Format("Expecting exception but none was thrown.\r\nExpected Exception Message: {0}", exceptionMessage), false);
				}

				factory.Save();

				AssertConsolCosts(factory, universalShipment, consol, consolCostCounts);
			}
			catch (DataObjectReadFailureException ex)
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					if (shipment.Job != null)
					{
						shipment.Job.Dispose();
					}
				}

				AssertNotNull(string.Format("exceptionMessage should not be null if DataObjectReadFailureException is thrown.\r\nThrown Exception Message: {0}", ex.Message), exceptionMessage);
				AssertEquals("Exception message", string.Format(exceptionMessage, consol.PK.ToString()), ex.Message);
			}

			return consol.PK;
		}

		JobConsolCost[] GetSellApportionmentForConsol(BusinessObjectFactory factory, ForwardingConsol consol)
		{
			var query = new ZQuery(JobConsolCostSchema.E6_ParentID, consol.CostSupporter.PK);
			query.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, consol.CostSupporter.Type);
			query.AddToFilter(JobConsolCostSchema.E6_GatewaySellChargeID, SQLComparisonOperator.NotEqual, null);
			return factory.Load<JobConsolCost>(query);
		}

		ConsolCostLine GetConsolCostLine(ZString? chargeCode, ZString? costAPInvoiceNumber, ZDateTime? costDueDate, ZDateTime? costInvoiceDate, ZString? costGSTVATID,
										ZDecimal? costExchangeRate, ZDecimal? costLocalAmount, ZDecimal? costOSAmount, ZString? costOSCurrency, ZDecimal? costOSGSTVATAmount,
										ZString? creditor, ZBool? costIsPosted, ZString? apportionmentMethod, ZBool? includeOnCollectInvoice, ZString? prepaidCollectFilter,
										ZString? supplierReference, ZBool? apportionToSubShipments, ZString? placeOfSupply = null, ZString? placeOfSupplyType = null)
		{
			ConsolCostLine consolCostLine = new ConsolCostLine(DefaultDataObjectWriterStrategy.TestInstance);

			if (chargeCode.HasValue)
			{
				consolCostLine.ChargeCode = new ChargeCode();
				consolCostLine.ChargeCode.Code = chargeCode;
				consolCostLine.ChargeCode.Description = "International Freight";
			}

			consolCostLine.CostAPInvoiceNumber = costAPInvoiceNumber;
			consolCostLine.CostDueDate = costDueDate;

			if (costGSTVATID.HasValue)
			{
				consolCostLine.CostGSTVATID = new TaxID();
				consolCostLine.CostGSTVATID.TaxCode = costGSTVATID;
				consolCostLine.CostGSTVATID.Description = "Cost Tax Description";
			}

			consolCostLine.CostInvoiceDate = costInvoiceDate;
			consolCostLine.CostLocalAmount = costLocalAmount;
			consolCostLine.CostOSAmount = costOSAmount;

			if (costOSCurrency.HasValue)
			{
				consolCostLine.CostOSCurrency = new Currency();
				consolCostLine.CostOSCurrency.Code = costOSCurrency;
				consolCostLine.CostOSCurrency.Description = "Cost Currency Description";
			}

			if (costOSGSTVATAmount.HasValue)
			{
				consolCostLine.CostOSGSTVATAmount = costOSGSTVATAmount;
			}

			if (creditor.HasValue)
			{
				consolCostLine.Creditor = new OrganizationReference();
				consolCostLine.Creditor.Key = creditor;
				consolCostLine.Creditor.Type = nameof(DataContextType.Organization);
			}

			if (placeOfSupply.HasValue && placeOfSupplyType.HasValue)
			{
				consolCostLine.PlaceOfSupply = new PlaceOfSupply();
				consolCostLine.PlaceOfSupply.Location = new CodeDescriptionPair5Char { Code = placeOfSupply };
				consolCostLine.PlaceOfSupply.LocationType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair { Code = placeOfSupplyType };
			}

			consolCostLine.ApportionmentMethod = apportionmentMethod;
			consolCostLine.IncludeOnCollectInvoice = includeOnCollectInvoice;
			consolCostLine.PrepaidCollectFilter = prepaidCollectFilter;
			consolCostLine.SupplierReference = supplierReference;
			consolCostLine.ApportionToSubShipments = apportionToSubShipments;

			consolCostLine.GovernmentReportingSellChargeCode = "Sell Govt Chg Code";
			consolCostLine.GovernmentReportingCostChargeCode = "Cost Govt Chg Code 1";

			return consolCostLine;
		}

		Shipment GetUniversalShipmentWithConsolCosts(BusinessObjectFactory factory, TestObjectCreator creator, GlbCompany company, IGenericJobCostPlugIn consol, AddConsolCostsDelegate addConsolCostsDelegate)
		{
			Shipment universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.ConsolCosts = new ConsolCosts(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.ConsolCosts.SetConsolCostLineCollection(() => new List<ConsolCostLine>());

			addConsolCostsDelegate?.Invoke(factory, creator, consol, universalShipment.ConsolCosts.ConsolCostLineCollection);
			factory.Save();

			return universalShipment;
		}

		void AssertConsolCosts(BusinessObjectFactory factory, Shipment universalShipment, IGenericJobCostPlugIn consol, int[] consolCostCounts = null)
		{
			int i = 0;

			foreach (var consolCostLine in universalShipment.ConsolCosts.ConsolCostLineCollection)
			{
				var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
				var chargeCode = consolCostLine.ChargeCode != null ? (ZString)consolCostLine.ChargeCode.Code.Value :
					(ZString)getMatchingCriteriaValue(consolCostLine, ConsolCostLineElementType.ChargeCode);
				query.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
				var chargeCode1 = factory.LoadTop1<AccChargeCode>(query);
				query = new ZQuery(JobConsolCostSchema.E6_AC_ChargeCode, chargeCode1.PK);
				query.AddToFilter(JobConsolCostSchema.E6_ParentID, consol.CostSupporter.PK);
				query.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, consol.CostSupporter.Type);
				query.AddToFilter(JobConsolCostSchema.E6_GatewaySellChargeID, null);
				JobConsolCost[] consolCosts = factory.Load<JobConsolCost>(query);

				int length = consolCostCounts == null ? 1 : consolCostCounts[i];
				AssertEquals("consolCosts.Length", length, consolCosts.Length);

				if (length > 0)
				{
					var consolCost = consolCosts[0];
					AssertConsolCost(consolCost, consolCostLine);
				}
				i++;
			}
		}

		void AssertConsolCost(JobConsolCost consolCost, ConsolCostLine consolCostLine)
		{
			if (consolCostLine.ChargeCode != null && consolCostLine.ChargeCode.Code.HasValue)
			{
				AssertNotNull("consolCost.ChargeCode should not be null", consolCost.ChargeCode);
				AssertEquals("ChargeCode should be equal", consolCostLine.ChargeCode.Code, consolCost.ChargeCode.AC_Code);
			}

			if (consolCostLine.CostAPInvoiceNumber.HasValue)
			{
				AssertEquals("APInvoiceNum should be equal", consolCostLine.CostAPInvoiceNumber, consolCost.E6_InvoiceNum);
			}

			if (consolCostLine.CostInvoiceDate.HasValue)
			{
				AssertEquals("CostInvoiceDate should be equal", consolCostLine.CostInvoiceDate, consolCost.E6_InvoiceDate);
			}

			if (consolCostLine.CostDueDate.HasValue)
			{
				AssertEquals("CostDueDate should be equal", consolCostLine.CostDueDate, consolCost.E6_PaymentDate);
			}

			if (consolCostLine.CostGSTVATID != null && consolCostLine.CostGSTVATID.TaxCode.HasValue && !consolCost.E6_AT_TaxRate.IsEmpty)
			{
				AssertNotNull("CostGSTRate should not be null", consolCost.TaxRate);
				AssertEquals("CostGSTRate should be equal", consolCostLine.CostGSTVATID.TaxCode, consolCost.TaxRate.AT_Code);
			}

			if (consolCostLine.CostLocalAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", consolCostLine.CostLocalAmount, consolCost.E6_LocalCostAmount);
			}

			if (consolCostLine.CostOSAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", consolCostLine.CostOSAmount, consolCost.E6_OSCostAmount);
			}

			if (consolCostLine.CostOSCurrency != null && consolCostLine.CostOSCurrency.Code.HasValue)
			{
				AssertEquals("Cost Currency should be equal", consolCostLine.CostOSCurrency.Code, consolCost.E6_RX_NKCurrency);
			}

			if (consolCostLine.CostOSGSTVATAmount.HasValue && !consolCost.E6_AT_TaxRate.IsEmpty)
			{
				AssertEquals("Cost OS GST VAT Amount should be equal", consolCostLine.CostOSGSTVATAmount, consolCost.E6_OSGSTAmount_Calc);
			}

			if (consolCostLine.Creditor != null && consolCostLine.Creditor.Key.HasValue)
			{
				AssertNotNull("CostAccount should not be null", consolCost.Creditor);
				AssertEquals("Creditor should be equal", consolCostLine.Creditor.Key, consolCost.Creditor.OH_Code);
			}

			if (consolCostLine.ApportionmentMethod.HasValue)
			{
				AssertEquals("ApportionmentMethod should be equal to E6_ApportionmentMethod", consolCostLine.ApportionmentMethod, consolCost.E6_ApportionmentMethod);
			}

			if (consolCostLine.SupplierReference.HasValue)
			{
				AssertEquals("SupplierReference should be equal to E6_CostReference", consolCostLine.SupplierReference, consolCost.E6_CostReference);
			}

			if (consolCostLine.IncludeOnCollectInvoice.HasValue)
			{
				AssertEquals("IncludeOnCollectInvoice should be equal to E6_IsForCollectInvoice", consolCostLine.IncludeOnCollectInvoice, consolCost.E6_IsForCollectInvoice);
			}

			if (consolCostLine.PrepaidCollectFilter.HasValue)
			{
				AssertEquals("PrepaidCollectFilter should be equal to E6_PPDCLT", consolCostLine.PrepaidCollectFilter, consolCost.E6_PPDCLT);
			}

			if (consolCostLine.IncludeOnCollectInvoice.HasValue)
			{
				AssertEquals("ApportionToSubShipments should be equal to E6_IsForCollectInvoice", consolCostLine.ApportionToSubShipments, consolCost.E6_ApportionToRelatedShipments);
			}

			if (consolCostLine.GovernmentReportingSellChargeCode.HasValue)
			{
				AssertEquals("ApportionToSubShipments should be equal to E6_SellGovtChargeCode", consolCostLine.GovernmentReportingSellChargeCode, consolCost.E6_SellGovtChargeCode);
			}

			if (consolCostLine.GovernmentReportingCostChargeCode.HasValue)
			{
				AssertEquals("ApportionToSubShipments should be equal to E6_CostGovtChargeCode", consolCostLine.GovernmentReportingCostChargeCode, consolCost.E6_CostGovtChargeCode);
			}

			if (consolCostLine.SupplyType != null && consolCostLine.SupplyType.Code.HasValue)
			{
				var expectedSupplyType = AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value ? consolCostLine.SupplyType.Code : ZString.Empty;
				AssertEquals("ApportionToSubShipments should be equal to E6_SupplyType", expectedSupplyType, consolCost.E6_SupplyType);
			}

			if (consolCostLine.RatingBehaviour != null && consolCostLine.RatingBehaviour.Code.HasValue)
			{
				AssertEquals("RatingBehaviour should be equal to E6_RatingBehaviour", consolCostLine.RatingBehaviour, consolCost.E6_RatingBehaviour);
			}

			if (consolCostLine.PlaceOfSupply != null)
			{
				if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
				{
					AssertEquals("PlaceOfSupply.Location.Code should be equal to E6_PlaceOfSupply", consolCostLine.PlaceOfSupply.Location.Code, consolCost.E6_PlaceOfSupply);
					AssertEquals("PlaceOfSupply.LocationType.Code should be equal to E6_PlaceOfSupplyType", consolCostLine.PlaceOfSupply.LocationType.Code, consolCost.E6_PlaceOfSupplyType);
				}
				else
				{
					AssertEquals(string.Empty, consolCost.E6_PlaceOfSupply);
					AssertEquals(string.Empty, consolCost.E6_PlaceOfSupplyType);
				}
			}
		}

		string getMatchingCriteriaValue(ConsolCostLine consolCostLine, ConsolCostLineElementType consolCostLineElementType)
		{
			string matchingCriteriaValue = string.Empty;
			foreach (MatchingCriteria matchingCriteria in consolCostLine.ImportMetaData.MatchingCriteriaCollection)
			{
				ConsolCostLineElementType key = (ConsolCostLineElementType)Enum.Parse(typeof(ConsolCostLineElementType), matchingCriteria.FieldName, true);
				if (key == consolCostLineElementType)
				{
					matchingCriteriaValue = matchingCriteria.Value;
					break;
				}
			}
			return matchingCriteriaValue;
		}

		#region Implementation

		TestErrorLogger Logger;
		protected UniversalObjectFactory UniversalObjectFactory;
		protected DateTime InvoiceDate;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			Logger = new TestErrorLogger();
			UniversalObjectFactory = new UniversalObjectFactory();
			InvoiceDate = ZDateTime.Now.ToDateTime();

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
			AccChargeCode fRT = Factory.LoadTop1<AccChargeCode>(query);
			fRT.AC_DepartmentFilterList = "ALL";

			query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, "BAF");
			AccChargeCode bAF = Factory.LoadTop1<AccChargeCode>(query);
			bAF.AC_DepartmentFilterList = "ALL";

			Factory.Save();
		}

		#endregion
	}
}
