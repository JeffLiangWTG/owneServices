using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.Vietnam;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using MessageTypes = Enterprise.Accounting.ElectronicMessaging.Vietnam.VietnamEInvoiceAPICommandList.Codes;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	sealed class TransactionBatchToGEIConverterForVietnamTest : TestCaseWithFactory
	{
		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversion_Invoice()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.SetCountry("VN");
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

				var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
				taxRate.SetRate_ForTestOnly(10, 1);
				var consol = TestObjectCreator.CreateConsol();
				consol.JK_MasterBillNum = "TestMAWB";

				var transport = consol.MostInterestingTransportForBinding[0];
				transport.JW_VoyageFlight = "TestFlight";
				transport.JW_Vessel = "TestVessel";

				var shipment = TestObjectCreator.CreateShipment("S00001007");
				shipment.ConsigneePK = TestObjectCreator.Creditor1.PK;
				shipment.ConsignorPK = TestObjectCreator.Creditor2.PK;
				shipment.JS_RL_NKOrigin = "VNHAN";
				shipment.JS_RL_NKDestination = "TWTPE";
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(-1);
				shipment.JS_E_ARV = ZDateTime.Now;
				shipment.JS_HouseBill = "ShipmentHouseBill";
				shipment.JS_OuterPacks = 2;
				shipment.JS_F3_NKPackType = "CTN";
				shipment.JS_TotalPackageCount = 2;
				shipment.JS_ActualWeight = 1.5M;
				shipment.JS_UnitOfWeight = "KG";
				shipment.JS_ActualChargeable = 1.7M;
				shipment.JS_ActualVolume = 5.2M;
				shipment.JS_UnitOfVolume = "M3";
				shipment.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
				consol.Shipments.Add(shipment);

				var declaration = TestObjectCreator.CreateDeclaration("D0001");
				declaration.JE_JS = shipment.PK;
				declaration.JE_OwnerRef = "OwnerReference";

				var job = TestObjectCreator.CreateJob(shipment);
				TestObjectCreator.Staff.GS_FullName = "StaffFullName";
				job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

				var jobCharge = job.Charges.AddNew();
				jobCharge.JR_AC = TestObjectCreator.CC1.PK;
				jobCharge.JR_LocalSellAmt = 100m;
				jobCharge.JR_OSSellAmt = 100m;
				jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge.JR_AT_SellGSTRate = taxRate.PK;

				var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test City", "01", "VN", "1234", "", "");
				address.PrimaryOrgAddressAdditionalInfoDetail = "Additional Information";
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "VNLAP";
				TestObjectCreator.Debtor.Addresses.Add(address);

				var arInvoice = CreateARInvoiceByPk(job, branch, address, Guid.Parse("8677665B-B6C9-4F4B-85DC-DFB688B77051"));
				var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
				line.AL_JH = job.PK;
				line.AL_AT = taxRate.PK;
				line.AL_LineAmount = 100m;
				jobCharge.JR_AL_ARLine = line.PK;

				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "12345675");

				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());

				AssertGEIMessage(typeof(VietnamEInvoice), eInvoice, batch, branch, MessageTypes.SendReceivablesInvoice, VietnamEInvoiceSRNPayloadJson);
			}
		}

		[TestDate(2025, 02, 08, 00, 00, 00)]
		public void TestConversion_InvoiceWithoutComplianceBook()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.SetCountry("VN");
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_MaximumNumberDigits = 8;
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_Code = "AAA";
				sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.XD_Prefix = "PREFIX";
				sequence.XD_NextNumber = 1;
				sequence.XD_EndNumber = 10;
				sequence.XD_IsActive = true;
				sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;

				var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test City", "01", "VN", "1234", "", "");
				address.PrimaryOrgAddressAdditionalInfoDetail = "Additional Information";
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "VNLAP";
				TestObjectCreator.Debtor.Addresses.Add(address);

				var arInvoice = CreateARInvoiceByPk(null, branch, address, Guid.Parse("8677665B-B6C9-4F4B-85DC-DFB688B77051"));
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "PREFIX00000001";

				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "12345675");

				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				AssertGEIMessage(typeof(VietnamEInvoice), eInvoice, batch, branch, MessageTypes.SendReceivablesInvoice, VietnamEInvoiceSRNPayloadJsonForComplianceBookEmpty);

				arInvoice.AH_XD_ComplianceBook = Guid.Empty;
				Factory.Save();

				converter = new TransactionBatchToGEIConverterForVietnam();
				(eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				AssertGEIMessage(typeof(VietnamEInvoice), eInvoice, batch, branch, MessageTypes.SendReceivablesInvoice, VietnamEInvoiceSRNPayloadJsonForComplianceBookEmpty);
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversion_Invoice_Declaration()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.SetCountry("VN");
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
				taxRate.SetRate_ForTestOnly(10, 1);

				var declaration = (BaseJobDeclaration)TestObjectCreator.CreateDeclaration("D0001");
				declaration.JE_OH_Supplier = TestObjectCreator.AALSHI.PK;
				declaration.JE_OH_Importer = TestObjectCreator.ABIGAS.PK;
				declaration.JE_GoodsDescription = "GoodsDescription";
				declaration.JE_OwnerRef = "OwnerRef";
				declaration.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
				declaration.JE_MasterBill = "MasterBill";
				declaration.JE_HouseBill = "HouseBilling";
				declaration.JE_TransportMode = "AIR";
				declaration.JE_DateAtFinalDestination = ZDateTime.Now.AddDays(-1);
				declaration.JE_DateAtOrigin = ZDateTime.Now;
				declaration.JE_OH_ShippingLine = TestObjectCreator.Creditor1.PK;
				declaration.JE_TotalNoOfPacks = 2;
				declaration.JE_TotalNoOfPacksPackType = "PKG";
				declaration.JE_TotalWeight = 30;
				declaration.JE_TotalWeightUnit = "KG";
				declaration.JE_TotalVolume = 60M;
				declaration.JE_TotalVolumeUnit = "M3";
				declaration.JE_RL_NKOrigin = "VNHAN";
				declaration.JE_RL_NKFinalDestination = "TWTPE";

				var transport = declaration.TransportsIncludingRelated.AddNew();
				transport.JW_VoyageFlight = "TestFlight";
				transport.JW_Vessel = "TestVessel";
				transport.JW_ETD = ZDateTime.Now.AddDays(-1);

				var job = TestObjectCreator.CreateJob(declaration);
				TestObjectCreator.Staff.GS_FullName = "StaffFullName";
				job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

				var jobCharge = job.Charges.AddNew();
				jobCharge.JR_AC = TestObjectCreator.CC1.PK;
				jobCharge.JR_LocalSellAmt = 100m;
				jobCharge.JR_OSSellAmt = 100m;
				jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge.JR_AT_SellGSTRate = taxRate.PK;

				var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test City", "01", "VN", "1234", "", "");
				address.OA_AdditionalAddressInformation = "Additional Information";
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "VNLAP";
				TestObjectCreator.Debtor.Addresses.Add(address);

				var arInvoice = CreateARInvoiceByPk(job, branch, address, Guid.Parse("8677665B-B6C9-4F4B-85DC-DFB688B77051"));
				var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
				line.AL_JH = job.PK;
				line.AL_AT = taxRate.PK;
				line.AL_LineAmount = 100m;
				jobCharge.JR_AL_ARLine = line.PK;

				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "12345675");

				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());

				AssertGEIMessage(typeof(VietnamEInvoice), eInvoice, batch, branch, MessageTypes.SendReceivablesInvoice, VietnamEInvoiceSRNPayloadJsonForDeclaration);
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversion_PeriodicInvoice_SingleJob()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			AccEInvoicingBatch batch = null;
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.SetCountry("VN");
				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

				var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
				taxRate.SetRate_ForTestOnly(10, 1);
				var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
				var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

				var declaration = (BaseJobDeclaration)TestObjectCreator.CreateDeclaration("D0001");
				declaration.JE_OH_Supplier = TestObjectCreator.AALSHI.PK;
				declaration.JE_OH_Importer = TestObjectCreator.ABIGAS.PK;
				declaration.JE_GoodsDescription = "GoodsDescription";
				declaration.JE_OwnerRef = "OwnerRef";
				declaration.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
				declaration.JE_MasterBill = "MasterBill";
				declaration.JE_HouseBill = "HouseBilling";
				declaration.JE_TransportMode = "AIR";
				declaration.JE_DateAtFinalDestination = ZDateTime.Now.AddDays(-1);
				declaration.JE_DateAtOrigin = ZDateTime.Now;
				declaration.JE_OH_ShippingLine = TestObjectCreator.Creditor1.PK;
				declaration.JE_TotalNoOfPacks = 2;
				declaration.JE_TotalNoOfPacksPackType = "PKG";
				declaration.JE_TotalWeight = 30;
				declaration.JE_TotalWeightUnit = "KG";
				declaration.JE_TotalVolume = 60M;
				declaration.JE_TotalVolumeUnit = "M3";
				declaration.JE_RL_NKOrigin = "VNHAN";
				declaration.JE_RL_NKFinalDestination = "TWTPE";

				var transport = declaration.TransportsIncludingRelated.AddNew();
				transport.JW_VoyageFlight = "TestFlight";
				transport.JW_Vessel = "TestVessel";
				transport.JW_ETD = ZDateTime.Now.AddDays(-1);

				var job = TestObjectCreator.CreateJob(declaration);
				TestObjectCreator.Staff.GS_FullName = "StaffFullName";
				job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

				var jobCharge = job.Charges.AddNew();
				jobCharge.JR_AC = TestObjectCreator.CC1.PK;
				jobCharge.JR_LocalSellAmt = 50m;
				jobCharge.JR_OSSellAmt = 50m;
				jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge.JR_AT_SellGSTRate = taxRate.PK;

				var jobCharge2 = job.Charges.AddNew();
				jobCharge2.JR_AC = TestObjectCreator.CC2.PK;
				jobCharge2.JR_LocalSellAmt = 50m;
				jobCharge2.JR_OSSellAmt = 50m;
				jobCharge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge2.JR_AT_SellGSTRate = taxRate.PK;

				var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test City", "01", "VN", "1234", "", "");
				address.OA_AdditionalAddressInformation = "Additional Information";
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "VNLAP";
				TestObjectCreator.Debtor.Addresses.Add(address);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 15M, TestObjectCreator.Debtor);
				arInvoice.AH_Desc = "TEST";
				arInvoice.AH_GB = branch.PK;
				arInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
				arInvoice.AH_InvoiceDate = ZDateTime.Today;
				arInvoice.AH_PostDate = ZDateTime.Today;
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "XI12345";
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
				arInvoice.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;
				arInvoice.AH_OA_InvoiceAddressOverride = address.PK;
				arInvoice.Header.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
				arInvoice.AH_ChequeOrReference = "SellReference";
				arInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
				arInvoice.AH_JH = job.PK;

				var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
				line.AL_JH = job.PK;
				line.AL_AT = taxRate.PK;
				line.AL_LineAmount = 50m;
				jobCharge.JR_AL_ARLine = line.PK;

				var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
				line2.GenericCharge = TestObjectCreator.CC2.PK;
				line2.AL_JH = job.PK;
				line2.AL_AT = taxRate.PK;
				line2.AL_LineAmount = 50m;
				jobCharge2.JR_AL_ARLine = line2.PK;

				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);

				TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "12345675");

				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);

				var expectedError = string.Empty;
				AssertEquals("Error", expectedError, validationErrors.ToString());

				AssertEquals("MessagingSystem", "Vietnam electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", VietnamEInvoiceAPICommandList.Codes.SendReceivablesInvoice, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("Company", branch.Company.GC_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
				AssertEquals("Branch", branch.GB_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);

				var expectPayload = VietnamEInvoiceTestHelper.GetEmbeddedResourceAsString("Conversion_PeriodicInvoice_SingleJob__VietnamEInvoiceSRNPayload.json").Trim().Replace("{sid}", arInvoice.PK.ToString());

				var vietnamEInvoice = JsonConvert.DeserializeObject<VietnamEInvoice>(eInvoice.Payload.ToUTF8FromBase64());
				var actualPayload = JsonConvert.SerializeObject(vietnamEInvoice, Formatting.Indented);
				AssertEquals(expectPayload, actualPayload);
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversion_PeriodicInvoice_MultipleJob()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			AccEInvoicingBatch batch = null;
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.SetCountry("VN");
				batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

				var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
				taxRate.SetRate_ForTestOnly(10, 1);
				var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
				var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

				var declaration = (BaseJobDeclaration)TestObjectCreator.CreateDeclaration("D0001");
				declaration.JE_OH_Supplier = TestObjectCreator.AALSHI.PK;
				declaration.JE_OH_Importer = TestObjectCreator.ABIGAS.PK;
				declaration.JE_GoodsDescription = "GoodsDescription";
				declaration.JE_OwnerRef = "OwnerRef";
				declaration.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
				declaration.JE_MasterBill = "MasterBill";
				declaration.JE_HouseBill = "HouseBilling";
				declaration.JE_TransportMode = "AIR";
				declaration.JE_DateAtFinalDestination = ZDateTime.Now.AddDays(-1);
				declaration.JE_DateAtOrigin = ZDateTime.Now;
				declaration.JE_OH_ShippingLine = TestObjectCreator.Creditor1.PK;
				declaration.JE_TotalNoOfPacks = 2;
				declaration.JE_TotalNoOfPacksPackType = "PKG";
				declaration.JE_TotalWeight = 30;
				declaration.JE_TotalWeightUnit = "KG";
				declaration.JE_TotalVolume = 60M;
				declaration.JE_TotalVolumeUnit = "M3";
				declaration.JE_RL_NKOrigin = "VNHAN";
				declaration.JE_RL_NKFinalDestination = "TWTPE";

				var transport = declaration.TransportsIncludingRelated.AddNew();
				transport.JW_VoyageFlight = "TestFlight";
				transport.JW_Vessel = "TestVessel";
				transport.JW_ETD = ZDateTime.Now.AddDays(-1);

				var job = TestObjectCreator.CreateJob(declaration);
				TestObjectCreator.Staff.GS_FullName = "StaffFullName";
				job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

				var jobCharge = job.Charges.AddNew();
				jobCharge.JR_AC = TestObjectCreator.CC1.PK;
				jobCharge.JR_LocalSellAmt = 100m;
				jobCharge.JR_OSSellAmt = 100m;
				jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge.JR_AT_SellGSTRate = taxRate.PK;

				var consol = TestObjectCreator.CreateConsol();
				consol.JK_MasterBillNum = "TestMAWB";

				var transport2 = consol.MostInterestingTransportForBinding[0];
				transport2.JW_VoyageFlight = "TestFlight";
				transport2.JW_Vessel = "TestVessel";

				var shipment = TestObjectCreator.CreateShipment("S00001007");
				shipment.ConsigneePK = TestObjectCreator.Creditor1.PK;
				shipment.ConsignorPK = TestObjectCreator.Creditor2.PK;
				shipment.JS_RL_NKOrigin = "VNHAN";
				shipment.JS_RL_NKDestination = "TWTPE";
				shipment.JS_E_DEP = ZDateTime.Now.AddDays(-1);
				shipment.JS_E_ARV = ZDateTime.Now;
				shipment.JS_HouseBill = "ShipmentHouseBill";
				shipment.JS_OuterPacks = 2;
				shipment.JS_F3_NKPackType = "CTN";
				shipment.JS_TotalPackageCount = 2;
				shipment.JS_ActualWeight = 1.5M;
				shipment.JS_UnitOfWeight = "KG";
				shipment.JS_ActualChargeable = 1.7M;
				shipment.JS_ActualVolume = 5.2M;
				shipment.JS_UnitOfVolume = "M3";
				shipment.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
				consol.Shipments.Add(shipment);

				var declaration2 = TestObjectCreator.CreateDeclaration("D0002");
				declaration2.JE_JS = shipment.PK;
				declaration2.JE_OwnerRef = "OwnerReference";

				var job2 = TestObjectCreator.CreateJob(shipment);
				TestObjectCreator.Staff.GS_FullName = "StaffFullName";
				job2.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;
				TestObjectCreator.SetExchangeRate(job2, TestObjectCreator.AUD, 1);

				var jobCharge2 = job2.Charges.AddNew();
				jobCharge2.JR_AC = TestObjectCreator.CC1.PK;
				jobCharge2.JR_LocalSellAmt = 100m;
				jobCharge2.JR_OSSellAmt = 100m;
				jobCharge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge2.JR_AT_SellGSTRate = taxRate.PK;

				var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test City", "01", "VN", "1234", "", "");
				address.OA_AdditionalAddressInformation = "Additional Information";
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "VNLAP";
				TestObjectCreator.Debtor.Addresses.Add(address);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 15M, TestObjectCreator.Debtor);
				arInvoice.AH_Desc = "TEST";
				arInvoice.AH_GB = branch.PK;
				arInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
				arInvoice.AH_InvoiceDate = ZDateTime.Today;
				arInvoice.AH_PostDate = ZDateTime.Today;
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "XI12345";
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
				arInvoice.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;
				arInvoice.AH_OA_InvoiceAddressOverride = address.PK;
				arInvoice.Header.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
				arInvoice.AH_ChequeOrReference = "SellReference";
				arInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

				var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
				line.AL_JH = job.PK;
				line.AL_AT = taxRate.PK;
				line.AL_LineAmount = 100m;
				jobCharge.JR_AL_ARLine = line.PK;

				var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
				line2.AL_JH = job2.PK;
				line2.AL_AT = taxRate.PK;
				line2.AL_LineAmount = 100m;
				jobCharge2.JR_AL_ARLine = line2.PK;

				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);

				TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "12345675");

				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);

				var expectedError = string.Empty;
				AssertEquals("Error", expectedError, validationErrors.ToString());

				AssertEquals("MessagingSystem", "Vietnam electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", VietnamEInvoiceAPICommandList.Codes.SendReceivablesInvoice, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("Company", branch.Company.GC_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
				AssertEquals("Branch", branch.GB_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);

				var expectPayload = VietnamEInvoiceTestHelper.GetEmbeddedResourceAsString("Conversion_PeriodicInvoice_MultipleJob__VietnamEInvoiceSRNPayload.json").Trim().Replace("{sid}", arInvoice.PK.ToString());

				var vietnamEInvoice = JsonConvert.DeserializeObject<VietnamEInvoice>(eInvoice.Payload.ToUTF8FromBase64());
				var actualPayload = JsonConvert.SerializeObject(vietnamEInvoice, Formatting.Indented);
				AssertEquals(expectPayload, actualPayload);
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversion_InvoiceDocumentRequest()
		{
			CreateTransactionForDocumentRequest createInvoice = delegate(GlbBranch branch)
			{
				return CreateARInvoiceByPk(null, branch, null, new Guid("8677665B-B6C9-4F4B-85DC-DFB688B77051"));
			};

			AssertDocumentRequest(createInvoice);
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversion_ReversalCRDDocumentRequest()
		{
			CreateTransactionForDocumentRequest createReversalCRD = delegate(GlbBranch branch)
			{
				var newBranch = TestObjectCreator.CreateBranch("XXX", branch.Company);
				var originalInvoice = CreateARInvoiceByPk(null, branch, null, new Guid("8677665B-B6C9-4F4B-85DC-DFB688B77051"));

				TestObjectCreator.CreateEInvoicingTransactionPivot(originalInvoice, status: Enterprise.Core.Constants.EInvoicingPivotState.Succeed);
				Factory.Save();
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(originalInvoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var arCreditNote = (ARCreditNote)TestObjectCreator.ReverseTransaction(originalInvoice, out _);
					arCreditNote.AH_GB = newBranch.PK;
					arCreditNote.Lines[0].AL_GB = arCreditNote.AH_GB;
					return arCreditNote;
				}
			};

			AssertDocumentRequest(createReversalCRD);
		}

		delegate InvoicingBase CreateTransactionForDocumentRequest(GlbBranch branch);

		void AssertDocumentRequest(CreateTransactionForDocumentRequest createTransaction)
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

				var transaction = createTransaction.Invoke(branch);
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, transaction, Core.Constants.EInvoicingPivotState.Batched, Core.Constants.EInvoicingPivotActionType.DocumentAction);
				TestObjectCreator.SetCustomsCodeForOrgHeader(transaction.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "12345675");

				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());

				AssertGEIMessage(typeof(VietnamEInvoiceDocument), eInvoice, batch, branch, MessageTypes.RequestDocumentForInvoice, VietnamEInvoiceRDNPayloadJson);
			}
		}

		public void TestPopulateAdditionalTransactionInfo()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbCompany.CurrentCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var accountDetails1 = TestObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.AddNew();
				accountDetails1.A1_IsDefaultAccount = true;
				accountDetails1.A1_RX_NKAccountCurrency = "USD";
				accountDetails1.A1_PaymentMethod = "TAX";
				accountDetails1.A1_AccountName = "Account Name US";
				accountDetails1.A1_BankName = "Bank Name US";
				accountDetails1.A1_BankAccount = "11";
				accountDetails1.A1_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;

				var accountDetails2 = TestObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.AddNew();
				accountDetails2.A1_IsDefaultAccount = true;
				accountDetails2.A1_RX_NKAccountCurrency = "VND";
				accountDetails2.A1_PaymentMethod = "TAX";
				accountDetails2.A1_AccountName = "Account Name VN";
				accountDetails2.A1_BankName = "Bank Name VN";
				accountDetails2.A1_BankAccount = "22";
				accountDetails2.A1_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;

				var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test City", "01", "VN", "1234", "666", "777");
				address.OA_AdditionalAddressInformation = "Additional Information";
				address.OA_RL_NKRelatedPortCode = "VNKAH";
				address.OA_CompanyNameOverride = "Test Company Name Override";
				TestObjectCreator.Debtor.Addresses.Add(address);

				TestObjectCreator.SetCustomsCodeForOrgHeader(GlbCompany.CurrentCompany.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "12345675");
				TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.Debtor, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0104128565-999");

				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				arInvoice.AH_OA_InvoiceAddressOverride = address.PK;
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());

				Assert("Address", eInvoice.Payload.ToUTF8FromBase64().Contains(@"""baddr"":""ADDITIONAL INFORMATION, ADDRESS1, ADDRESS2, TEST CITY, 1234, VIET NAM"""));
				Assert("VATRegistrationNum", eInvoice.Payload.ToUTF8FromBase64().Contains(@"""btax"":""0104128565-999"""));
				Assert("BankName", eInvoice.Payload.ToUTF8FromBase64().Contains(@"""bbank"":""Bank Name VN"""));
				Assert("AccountNumber", eInvoice.Payload.ToUTF8FromBase64().Contains(@"""bacc"":""22"""));

				batch = TestObjectCreator.CreateEInvoicingBatch(101, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.USD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				converter = new TransactionBatchToGEIConverterForVietnam();
				(eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());

				Assert("BankName", eInvoice.Payload.ToUTF8FromBase64().Contains(@"""bbank"":""Bank Name US"""));
				Assert("AccountNumber", eInvoice.Payload.ToUTF8FromBase64().Contains(@"""bacc"":""11"""));

				batch = TestObjectCreator.CreateEInvoicingBatch(102, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.VND, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				converter = new TransactionBatchToGEIConverterForVietnam();
				(eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());

				Assert("BankName", eInvoice.Payload.ToUTF8FromBase64().Contains(@"""bbank"":""Bank Name VN"""));
				Assert("AccountNumber", eInvoice.Payload.ToUTF8FromBase64().Contains(@"""bacc"":""22"""));
			}
		}

		public void TestPopulateAdditionalContactInfo_INV()
		{
			Func<InvoicingBase> createAction = () => TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			TestPopulateAdditionalContactInfo(createAction);
		}

		public void TestPopulateAdditionalContactInfo_CRD()
		{
			Func<InvoicingBase> createAction = () =>
			{
				var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor, VietnamComplianceInfo.ComplianceSubTypeCodes.TXI);
				arInvoice.AH_TransactionReference = "PREFIX00000001";

				var arCreditNote = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.ABIGAS, testObjectCreator.AUD, 1m, "Incorrect amount");
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;

				return arCreditNote;
			};

			TestPopulateAdditionalContactInfo(createAction);
		}

		void TestPopulateAdditionalContactInfo(Func<InvoicingBase> createTransaction)
		{
			TestObjectCreator.ABIGAS.Contacts.RemoveAndDeleteAll();
			var contact1 = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS, "C1", "contact1@client1.com");
			contact1.OC_Phone = "1333444555";
			var apDocument = contact1.Documents.AddNew();
			apDocument.OD_DocumentGroup = ContactType.Payables.Code;

			var orgContact = TestObjectCreator.ABIGAS.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.OC_IsActive && x.Documents.Cast<OrgDocument>().Any(y => y.OD_DocumentGroup == ContactType.Receivables.Code || y.OD_DocumentGroup == ContactType.All.Code));
			AssertNull(orgContact);
			AssertNotNull(TestObjectCreator.ABIGAS.Addresses.MainAddress);

			AssertAdditionalContactInfo(createTransaction, ZGuid.Empty, 100, null);

			var contact2 = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS, "C2", "contact2@client2.com");
			contact2.OC_Phone = "1666777888";
			var arDocument = contact2.Documents.AddNew();
			arDocument.OD_DocumentGroup = ContactType.Receivables.Code;

			AssertAdditionalContactInfo(createTransaction, ZGuid.Empty, 101, contact2);
			AssertAdditionalContactInfo(createTransaction, contact1.PK, 102, contact1);
		}

		#region Export Multiple Debtor Organisation Contact Email Registry

		public void TestPopulateAdditionalContactInfo_ExportMultipleDebtorOrganizationContactEmailRegistryIsSetToDEF()
		{
			Func<InvoicingBase> createAction = () => TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice),
				"001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS,
				TestObjectCreator.CC1.PK);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ExportMultipleDebtorOrganizationContactEmailCodes.DEF))
			{
				TestPopulateAdditionalContactInfo_ExportMultipleDebtorOrganizationContactEmailRegistryIsSetToDEF(createAction);
			}
		}

		void TestPopulateAdditionalContactInfo_ExportMultipleDebtorOrganizationContactEmailRegistryIsSetToDEF(Func<InvoicingBase> createTransaction)
		{
			TestObjectCreator.ABIGAS.Contacts.RemoveAndDeleteAll();

			var contact1 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C1", "contact1@client1.com", "11111111", ContactType.Payables.Code, ContactNotifyModes.Email);
			AssertAdditionalContactInfo(createTransaction, ZGuid.Empty, 100, null);

			var contact2 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C2", "contact2@contact2.com", "22222222", ContactType.Receivables.Code, ContactNotifyModes.Email);
			var contact3 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C3", "contact3@contact3.com", "33333333", ContactType.Receivables.Code, ContactNotifyModes.Email);
			var contact4 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C4", "contact4@contact4.com", "44444444", ContactType.Receivables.Code, ContactNotifyModes.Email);
			var contact5 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C5", "contact5@contact5.com", "55555555", ContactType.Receivables.Code, ContactNotifyModes.DoNotDeliver);
			var contact6 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C6", "contact6@contact6.com", "66666666", ContactType.Payables.Code, ContactNotifyModes.DoNotDeliver);

			AssertAdditionalContactInfo(createTransaction, ZGuid.Empty, 101, contact2);
			AssertAdditionalContactInfo(createTransaction, contact6.PK, 102, contact6);
		}

		public void TestPopulateAdditionalContactInfo_ExportMultipleDebtorOrganizationContactEmailRegistryIsSetToMAR()
		{
			Func<InvoicingBase> createAction = () => TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice),
				"001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS,
				TestObjectCreator.CC1.PK);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ExportMultipleDebtorOrganizationContactEmailCodes.MAR))
			{
				TestPopulateAdditionalContactInfo_ExportMultipleDebtorOrganizationContactEmailRegistryIsSetToMAR(createAction);
			}
		}

		void TestPopulateAdditionalContactInfo_ExportMultipleDebtorOrganizationContactEmailRegistryIsSetToMAR(Func<InvoicingBase> createTransaction)
		{
			TestObjectCreator.ABIGAS.Contacts.RemoveAndDeleteAll();

			var contact1 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C1", "contact1@client1.com", "11111111", ContactType.Payables.Code, ContactNotifyModes.Email);
			var contact2 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C2", "contact2@contact2.com", "22222222", ContactType.Receivables.Code, ContactNotifyModes.Email);
			var contact3 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C3", "contact3@contact3.com", "33333333", ContactType.Receivables.Code, ContactNotifyModes.Email);
			var contact4 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C4", "contact4@contact4.com", "44444444", ContactType.Receivables.Code, ContactNotifyModes.Email);
			var contact5 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C5", "contact5@contact5.com", "55555555", ContactType.Receivables.Code, ContactNotifyModes.DoNotDeliver);
			var contact6 = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C6", "contact6@contact6.com", "66666666", ContactType.Payables.Code, ContactNotifyModes.DoNotDeliver);

			AssertAdditionalContactInfo(createTransaction, ZGuid.Empty, 101, contact2);
			AssertAdditionalContactInfo(createTransaction, contact6.PK, 102, contact6);
		}

		OrgContact CreateContactWithDocumentDetails(OrgHeader orgHeader, ZString name, ZString email, ZString phone, ZString documentGroup, ZString deliveryBy)
		{
			var contact = TestObjectCreator.CreateContact(orgHeader, name, email);
			contact.OC_Phone = phone;

			var contactDocument = contact.Documents.AddNew();
			contactDocument.OD_DocumentGroup = documentGroup;
			contactDocument.OD_DeliverBy = deliveryBy;

			return contact;
		}

		ZString GetExpectedEmail(InvoicingBase invoice, OrgContact expectedOrgContact)
		{
			if (AccountingMasterFilesRegistry.Instance.IncludeContactEmailsOfLocalDebtorOrganizationOnly.Value
				&& !IsTransactionFromLocalDebtor(invoice))
			{
				return string.Empty;
			}

			if (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.Value == ExportMultipleDebtorOrganizationContactEmailCodes.MAR)
			{
				var orgHeader = invoice.Header;
				var overrideContactEmail = invoice.InvoiceContactOverride?.OC_Email ?? ZString.Empty;
				var marContactEmails = orgHeader.Contacts.Cast<OrgContact>().Where(x =>
					x.OC_IsActive && !x.OC_Email.IsEmpty && x.OC_Email != overrideContactEmail && x.Documents.Cast<OrgDocument>().Any(y =>
						(y.OD_DeliverBy == ContactNotifyModes.Email &&
						(y.OD_DocumentGroup == ContactType.Receivables.Code || y.OD_DocumentGroup == ContactType.All.Code)))).Select(x => x.OC_Email).Distinct();

				var result = overrideContactEmail;

				if (marContactEmails.Any())
				{
					result += (result.IsEmpty ? "" : ";") + string.Join(";", marContactEmails);
				}

				return result;
			}
			else
			{
				return expectedOrgContact?.Email ?? ZString.Empty;
			}
		}

		#endregion

		#region Include Contact Emails of Local Debtor Organization Only

		public void TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly_ForeignDebtor_MAR()
		{
			TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly("AU", "MAR");
		}

		public void TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly_ForeignDebtor_DEF()
		{
			TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly("AU", "DEF");
		}

		public void TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly_LocalDebtor_MAR()
		{
			TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly("VN", "MAR");
		}

		public void TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly_LocalDebtor_DEF()
		{
			TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly("VN", "DEF");
		}

		bool IsTransactionFromLocalDebtor(InvoicingBase invoice)
		{
			var debtorCountry = invoice.InvoiceAddressOverride?.Country.Code ?? string.Empty;
			return debtorCountry == CountryCodes.VietNam;
		}

		void TestPopulateAdditionalContactInfo_IncludeContactEmailsOfLocalDebtorOrganizationOnly(string countryCode, string exportMultipleDebtorOrganizationContactEmailCodes)
		{
			var isLocal = countryCode == CountryCodes.VietNam;
			var org = TestObjectCreator.ABIGAS;
			var address = TestObjectCreator.CreateAddress(org, OrgAddressType.Receivables, true, $"{countryCode} Address1", $"{countryCode} Address2", $"{countryCode} Test City", "01", countryCode, "1234", "666", "777");
			Factory.Save();

			Func<InvoicingBase> createAction = () =>
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice),
					"001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, org, TestObjectCreator.CC1.PK);
				invoice.AH_OA_InvoiceAddressOverride = address.PK;

				return invoice;
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.IncludeContactEmailsOfLocalDebtorOrganizationOnly.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, exportMultipleDebtorOrganizationContactEmailCodes))
			{
				AssertEquals("Pre-condition", isLocal, IsTransactionFromLocalDebtor(createAction()));

				if (exportMultipleDebtorOrganizationContactEmailCodes == ExportMultipleDebtorOrganizationContactEmailCodes.MAR)
				{
					TestPopulateAdditionalContactInfo_ExportMultipleDebtorOrganizationContactEmailRegistryIsSetToMAR(createAction);
				}
				else
				{
					TestPopulateAdditionalContactInfo_ExportMultipleDebtorOrganizationContactEmailRegistryIsSetToMAR(createAction);
				}
			}
		}

		#endregion

		void AssertAdditionalContactInfo(Func<InvoicingBase> createTransaction, ZGuid invoiceContactOverride, ZInt batchNumber, OrgContact expectedOrgContact)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoice = createTransaction();
				invoice.AH_OC_InvoiceContactOverride = invoiceContactOverride;

				var batch = TestObjectCreator.CreateEInvoicingBatch(batchNumber, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				var decodedPayload = eInvoice.Payload.ToUTF8FromBase64();
				Assert("Btel", decodedPayload.Contains($@"""btel"":""{expectedOrgContact?.OC_Phone ?? ZString.Empty}"""));
				Assert("Buyer", decodedPayload.Contains($@"""buyer"":""{expectedOrgContact?.Name ?? ZString.Empty}"""));
				Assert("Bmail", decodedPayload.Contains($@"""bmail"":""{GetExpectedEmail(invoice, expectedOrgContact)}"""));
			}
		}

		[TestDate(2020, 08, 24, 08, 36, 00)]
		public void TestConversion_CreditNote()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingFormNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "world_peace"))
			{
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_Code = "AAA";
				sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.XD_Prefix = "PREFIX";
				sequence.XD_IsActive = true;
				sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;

				var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor, VietnamComplianceInfo.ComplianceSubTypeCodes.TXI);
				arInvoice.AH_XD_ComplianceBook = sequence.PK;
				arInvoice.AH_TransactionReference = "PREFIX00000001";

				var arCreditNote = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.ABIGAS, testObjectCreator.AUD, 1m, "Incorrect amount");
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				arCreditNote.SupportingDocumentNumber = "This is a supporting document number";

				TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

				AccEInvoicingBatch batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				testObjectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, Core.Constants.EInvoicingPivotState.Queued, Core.Constants.EInvoicingPivotActionType.Cancel);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				AssertEquals("Warning", string.Empty, validationWarnings.ToString());

				AssertGEIMessage(typeof(VietnamEInvoiceCancellation), eInvoice, batch, branch, MessageTypes.CancelReceivablesInvoice, VietnamEInvoiceRCNPayloadJson);
			}
		}

		[TestDate(2020, 08, 24, 08, 36, 00)]
		public void TestConversion_CancelCircular78()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("VNHOA");
			Factory.Save();
			var branch = company.FirstActiveBranch;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingFormNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "world_peace"))
			{
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_MaximumNumberDigits = 8;
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_Code = "AAA";
				sequence.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.XD_Prefix = "PREFIX";
				sequence.XD_IsActive = true;
				sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;

				var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor, VietnamComplianceInfo.ComplianceSubTypeCodes.TXI);
				arInvoice.AH_XD_ComplianceBook = sequence.PK;
				arInvoice.AH_TransactionReference = "PREFIX00000001";

				var arCreditNote = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.ABIGAS, testObjectCreator.AUD, 1m, "Incorrect amount");
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				arCreditNote.SupportingDocumentNumber = "This is a supporting document number";

				TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

				AccEInvoicingBatch batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				testObjectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, Core.Constants.EInvoicingPivotState.Queued, Core.Constants.EInvoicingPivotActionType.Cancel);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				AssertEquals("Warning", string.Empty, validationWarnings.ToString());

				AssertGEIMessage(typeof(VietnamEInvoiceCancellationCircular), eInvoice, batch, branch, MessageTypes.CancelReceivablesCircular78Invoice, VietnamEInvoiceRCNCircular78PayloadJson);
			}
		}

		void AssertGEIMessage(Type type, GlobalElectronicInvoicing eInvoice, AccEInvoicingBatch batch, GlbBranch branch, string messageType, string testCase)
		{
			AssertEquals("MessagingSystem", "Vietnam electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("MessageType", messageType, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("branch", branch.GB_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);
			AssertEquals("Company", branch.Company.GC_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
			AssertEquals("IsProductionSystemSpecified", true, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified);

			AssertEquals("Credentials", 2, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			AssertEquals("Credentials_username_key", nameof(Username), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Key);
			var username = eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Value.Value;
			AssertEquals("Credentials_username_value", Username, username);

			AssertEquals("Credentials_password_key", nameof(Password), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[1].Key);
			var password = eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[1].Value;
			Assert("Credentials_password_value", password.Encrypted);
			AssertEquals("Credentials_password_value", Password, EInvoicingTestHelper.DecodePassword(password.Value));

			var expectPayload = VietnamEInvoiceTestHelper.GetEmbeddedResourceAsString(testCase);

			var vietnamEInvoice = JsonConvert.DeserializeObject(eInvoice.Payload.ToUTF8FromBase64(), type);
			var actualPayload = JsonConvert.SerializeObject(vietnamEInvoice, Formatting.Indented);
			AssertEquals(expectPayload, actualPayload);
		}

		ARInvoice CreateARInvoiceByPk(Job job, GlbBranch branch, OrgAddress address, Guid pk)
		{
			var arInvoice = Factory.NewWithPrimaryKey<ARInvoice>(pk);
			arInvoice.AH_GC = branch.GB_GC;
			arInvoice.AH_OH = TestObjectCreator.Debtor.PK;
			arInvoice.AH_JH = job?.PK ?? ZGuid.Empty;
			arInvoice.AH_Desc = "TEST";
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice.AH_TransactionNum = TestObjectCreator.GetRandomString(5);
			arInvoice.AH_GB = branch.PK;
			arInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			arInvoice.AH_InvoiceDate = ZDateTime.Today;
			arInvoice.AH_PostDate = ZDateTime.Today;
			arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			arInvoice.AH_TransactionReference = "XI12345";

			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			arInvoice.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;
			arInvoice.AH_OA_InvoiceAddressOverride = address?.PK ?? ZGuid.Empty;
			arInvoice.Header.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";

			return arInvoice;
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Username);
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Password);
		}

		const string Username = "admin";
		const string Password = "12345";
		const string VietnamEInvoiceRCNPayloadJson = @"VietnamEInvoiceRCNPayload.json";
		const string VietnamEInvoiceRCNCircular78PayloadJson = @"VietnamEInvoiceRCNCircularPayload.json";
		const string VietnamEInvoiceSRNPayloadJson = @"Conversion_Invoice_VietnamEInvoiceSRNPayload.json";
		const string VietnamEInvoiceRDNPayloadJson = @"Conversion_Invoice_VietnamEInvoiceRDNPayload.json";
		const string VietnamEInvoiceSRNPayloadJsonForDeclaration = @"Conversion_Invoice_Declaration_VietnamEInvoiceSRNPayload.json";
		const string VietnamEInvoiceSRNPayloadJsonForComplianceBookEmpty = @"Conversion_Invoice_VietnamEInvoiceSRNPayloadForComplianceBookEmpty.json";

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
