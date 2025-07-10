using System;
using System.Linq;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Customs.JP.Business.Constants;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderMessageProvider))]
	sealed class CusEntryHeaderMessageProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertExceptionThrown<ArgumentNullException>(() => new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader)));
			entryHeader.Delete();
			entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123";
			Factory.Save();
			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals(nameof(provider.DeclarationNumber), "123", provider.DeclarationNumber);

			var declaration = Factory.New<JobDeclaration>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			var bthPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			bthPassword.GP_GS = staff.PK;
			bthPassword.GP_MailBoxID = "TEST1";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.JE_NACCSCredential = bthPassword.PK;
			declaration.ExternalBrokerCode = "TEST1";
			declaration.CusContainers.AddNew();
			declaration.JE_OwnerSectionCode = "TestOwnerSectionCode";
			declaration.JE_OwnerRef = "TestOwnerReferenceNumber";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var containerPivot = invoiceLine.ContainersForInvoiceLinesForBindingOnly[0];
			containerPivot.IsForInvoiceLine = true;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryInstruction.JP_CustomsNotes = "TestCustomsNotes";
			entryInstruction.JP_BrokersNotes = "TestBrokersNotes";
			entryInstruction.JP_OwnersNotes = "TestOwnersNotes";
			entryInstruction.CEI_ContainerCount = 2;
			entryInstruction.CEI_CargoQuantity = 100;
			entryInstruction.CEI_GrossWeight = 100;
			entryInstruction.CEI_CargoQuantityUnit = "KG";
			entryInstruction.CEI_GrossWeightUnit = "GS";
			entryInstruction.CEI_DeclarationCargoType = "B";
			declaration.JE_DeclarationReference = "TestDeclarationReference";

			Factory.Save();

			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			CombineAssertions(() =>
			{
				AssertEquals("CargoType", entryInstruction.CEI_DeclarationCargoType, provider.CargoType);
				AssertEquals("Container Count", 2, provider.ContainerCount);
				AssertEquals("Declarant Code", declaration.ExternalBrokerCode, provider.DeclarantCode);
				AssertEquals("Notes (Customs)", entryInstruction.JP_CustomsNotes, provider.CustomsRemarks);
				AssertEquals("Notes (Customs Broker)", entryInstruction.JP_BrokersNotes, provider.CustomsBrokerRemarks);
				AssertEquals("Notes (Owner)", entryInstruction.JP_OwnersNotes, provider.OwnerRemarks);
				AssertEquals("Owner Section Code", declaration.JE_OwnerSectionCode, provider.OwnerSectionCode);
				AssertEquals("Owner Reference Number", declaration.JE_OwnerRef, provider.OwnerReferenceNumber);
				AssertEquals("Gross Weight", entryInstruction.CEI_CustomsWeight, provider.GrossWeight.Quantity);
				AssertEquals("Gross Weight Unit", entryInstruction.CEI_CustomsWeightUnit, provider.GrossWeight.Unit);
				AssertEquals("Quantity", entryInstruction.CEI_CargoQuantity, provider.Quantity.Quantity);
				AssertEquals("Quantity Unit", entryInstruction.CEI_CargoQuantityUnit, provider.Quantity.Unit);
				AssertEquals("Internal Reference Number", declaration.JE_DeclarationReference, provider.InternalReferenceNumber);
			});
		}

		#region IDA/EDA
		public void TestCustomsDepot()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			entryHeader.Declaration.DepotDocAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			var ccp = entryHeader.Declaration.DepotDocAddress.Organisation.CustomsCodes.AddNew();
			ccp.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			ccp.OK_CustomsRegNo = "CCP123";
			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			CombineAssertions(() =>
			{
				AssertEquals("IDA Depot Code", "CCP123", provider.CustomsDepot.Code);
				AssertEquals("EDA Depot Code", "CCP123", provider.CustomsDepot.Code);
			});
		}

		public void TestAttorneyForCustomsProcedures()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var representative = Factory.NewWithValidTestData<OrgAddress>();
			representative.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.CIE, "C0000123456789012");
			representative.CompanyName = "TestCompany";

			declaration.JE_OA_Representative = representative.PK;
			declaration.JE_ACP_POA = "1235467890";

			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("Attory for Customs Procedure Code", string.Empty, provider.AttorneyForCustomsProcedures.Code);
			AssertEquals("Attory for Customs Procedure Acceptance Number", "1235467890", provider.AttorneyForCustomsProcedures.ReceiptNumber);
			AssertEquals("Attory for Customs Procedure Name", "TestCompany", provider.AttorneyForCustomsProcedures.Name);

			declaration.JE_ACP_POA = string.Empty;
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("Attory for Customs Procedure Code", "C0000123456789012", provider.AttorneyForCustomsProcedures.Code);
			AssertEquals("Attory for Customs Procedure Acceptance Number", string.Empty, provider.AttorneyForCustomsProcedures.ReceiptNumber);
			AssertEquals("Attory for Customs Procedure Name", string.Empty, provider.AttorneyForCustomsProcedures.Name);
		}

		public void TestInspectionWitness()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var inspectionWitness = declaration.InspectionWitness;
			inspectionWitness.E2_OA_Address = org.MainAddress.PK;
			var nuc = org.MainAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.NUC, "NUC12", Core.Constants.CountryCodes.Japan);
			Factory.Save();

			CombineAssertions(() =>
			{
				var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
				AssertEquals("IDA Inspection Witness Code", "NUC12", provider.InspectionWitness.NACCSUserCode);
				AssertEquals("EDA Inspection Witness Code", "NUC12", provider.InspectionWitness.NACCSUserCode);
			});

			inspectionWitness.E2_AddressOverride = true;
			declaration.InspectionWitnessCode = "12345";

			CombineAssertions(() =>
			{
				var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
				AssertEquals("IDA Inspection Witness Code", "12345", provider.InspectionWitness.NACCSUserCode);
				AssertEquals("EDA Inspection Witness Code", "12345", provider.InspectionWitness.NACCSUserCode);
			});
		}

		public void TestImportDeclarationTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ValueType = ValueTypeList.Codes.L;
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.A;
			instruction.CEI_FoodHygieneCertificateType = IDACertificateIdList.Codes._2;
			instruction.CEI_PlantProtectionCertificateType = IDACertificateIdList.Codes._3;
			instruction.CEI_AnimalQuarantineCertificateType = IDACertificateIdList.Codes._4;
			instruction.CEI_SubStyle = JPAdditionalDeclarationTypeList.Codes.Y;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject);

			CombineAssertions(() => {
				AssertEquals("IDA Value Type", instruction.CEI_ValueType, provider.ValueType);
				AssertEquals("IDA Declaration Type", instruction.CEI_Style, provider.DeclarationType);
				AssertEquals("IDA Declaration Sub Type", instruction.CEI_SubStyle, provider.DeclarationSubType);
				AssertEquals("IDA Food Hygiene Certificate Type", instruction.CEI_FoodHygieneCertificateType, provider.FoodHygineCertificateId);
				AssertEquals("IDA Plant Protection Certificate Type", instruction.CEI_PlantProtectionCertificateType, provider.PlantProtectionCertificateId);
				AssertEquals("IDA Animal Quarantine Certificate Type", instruction.CEI_AnimalQuarantineCertificateType, provider.AnimalQuarantineCertificateId);
			});
		}

		public void TestExportDeclarationTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ValueType = ValueTypeList.Codes.L;
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.A;
			instruction.CEI_SubStyle = JPAdditionalDeclarationTypeList.Codes.Y;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject);

			CombineAssertions(() => {
				AssertEquals("EDA Value Type", instruction.CEI_ValueType, provider.ValueType);
				AssertEquals("EDA Declaration Type", instruction.CEI_Style, provider.DeclarationType);
				AssertEquals("EDA Declaration Sub Type", instruction.CEI_SubStyle, provider.DeclarationSubType);
			});
		}

		public void TestBLNumbersAndAWBNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_BillNumber = "00004";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "00001";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));

			AssertEquals(1, provider.BillNumbers.Count());
			AssertEquals("IDA BillNumbers", "00001", provider.BillNumbers.First());

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "00002";

			houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "00003";

			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals(1, provider.BillNumbers.Count());
			AssertEquals("IDA BillNumbers", "00002", provider.BillNumbers.First());

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));

			AssertArrayEqualsByElements("IDA BillNumbers", new string[] { "00004", "00001" }, provider.BillNumbers.ToArray());
		}

		public void TestBillNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_BillNumber = "00001";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("EDA BillNumber should be empty when SEA", string.Empty, provider.BillNumber);

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("EDA BillNumber should be populated when AIR", "00001", provider.BillNumber);
		}

		public void TestComprehensiveDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = "CNT";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));

			AssertEquals("IDA Comprehensive Declaration Type", "C", provider.ComprehensiveDeclarationType);

			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("IDA Comprehensive Declaration Type", "", provider.ComprehensiveDeclarationType);

			declaration.Bills.AddNew();
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			declaration.JE_ContainerMode = "BBK";
			AssertEquals("IDA Comprehensive Declaration Type", "M", provider.ComprehensiveDeclarationType);

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("IDA Comprehensive Declaration Type", "", provider.ComprehensiveDeclarationType);

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = "BLK";
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("IDA Comprehensive Declaration Type", "L", provider.ComprehensiveDeclarationType);

			declaration.JE_ContainerMode = "SCN";
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("IDA Comprehensive Declaration Type", "", provider.ComprehensiveDeclarationType);
		}

		public void TestApprovalCertificates()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var certificate = entryInstruction.ApprovalCertificateInfos.AddNew();
			certificate.CSI_Code = "ISNO";
			certificate.CSI_ReferenceNumber = "1";

			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var sendingObject = new MessageSendingObject(entryHeader);
			for (var i = 0; i < 10; i++)
			{
				entryInstruction.ApprovalCertificateInfos.AddNew();
			}
			var provider = new CusEntryHeaderMessageProvider(sendingObject);

			AssertEquals("IDA Import Approval Certificate Type", "ISNO", provider.ApprovalCertificates.ToArray()[0].Type);
			AssertEquals("IDA Import Approval Certificate Number", "1", provider.ApprovalCertificates.ToArray()[0].Number);
			AssertEquals(11, entryInstruction.ApprovalCertificateInfos.Count);
			AssertEquals("IDA: The output certificates should be no more than 10.", 10, provider.ApprovalCertificates.Count());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			for (var i = 0; i < 5; i++)
			{
				entryInstruction.ApprovalCertificateInfos.AddNew();
			}
			provider = new CusEntryHeaderMessageProvider(sendingObject);

			AssertEquals("EDA Export Approval Certificate Type", "ISNO", provider.ApprovalCertificates.ToArray()[0].Type);
			AssertEquals("EDA Export Approval Certificate Number", "1", provider.ApprovalCertificates.ToArray()[0].Number);
			AssertEquals(16, entryInstruction.ApprovalCertificateInfos.Count);
			AssertEquals("EDA: The output certificates should be no more than 15.", 15, provider.ApprovalCertificates.Count());
		}

		public void TestPortOfLoading()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "ES2BL";
			declaration.JE_PortOfLoadingName = "Test";
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject);

			AssertEquals("IDA Port of Loading Code", "ES2BL", provider.PortOfLoading.Code);
			AssertEquals("IDA Port of Loading Name", "Test", provider.PortOfLoading.Name);

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;

			AssertEquals("EDA Port of Loading Code", "2BL", provider.PortOfLoading.Code);
			AssertEquals("EDA Port of Loading Name", string.Empty, provider.PortOfLoading.Name);

			declaration.JE_RL_NKPortOfLoading = "ZZZ";
			AssertEquals("EDA Port of Loading Code", "ZZZ", provider.PortOfLoading.Code);
		}

		public void TestPortOfOrigin()
		{
			var originUNLOCO = Factory.New<RefUNLOCO>();
			originUNLOCO.RL_Code = "JPABC";
			originUNLOCO.Description = "origin name";
			originUNLOCO.RL_NameWithDiacriticals = "origin proper name";
			originUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = "JPABC";
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject);

			AssertEquals("IDA Port of Origin Code", "JPABC", provider.PortOfOrigin.Code);
			AssertEquals("IDA Port of Origin Name", "origin name", provider.PortOfOrigin.Name);
		}

		public void TestInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceHeader.JZ_InvoiceType = "C";
			invoiceHeader.JZ_ElectronicInvoiceReceiptNumber = "12345";
			invoiceHeader.JZ_InvoiceNumber = "1";
			invoiceHeader.JZ_InvoiceAmountType = "B";
			invoiceHeader.JZ_IncoTerm = "C&F";
			invoiceHeader.JZ_InvoiceAmount = 100;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoiceHeader2.JZ_InvoiceAmount = 200;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject);
			Factory.Save();

			CombineAssertions(() =>
			{
				var invoice_IDA = provider.Invoice;
				AssertEquals("IDA Invoice Type", "C", invoice_IDA.Type);
				AssertEquals("IDA Electronic Invoice Receipt Number", "12345", invoice_IDA.ElectronicReceiptNumber);
				AssertEquals("IDA Invoice Number", "1", invoice_IDA.Number);
				AssertEquals("IDA Invoice Valuation Type", "B", invoice_IDA.PriceTypeCode);
				AssertEquals("IDA Invoice Incoterm", "C&F", invoice_IDA.Incoterm);
				AssertEquals("IDA Invoice Currency", "JPY", invoice_IDA.Price.CurrencyCode);
				AssertEquals("IDA Invoice Price", 300m, invoice_IDA.Price.Amount);

				var invoice_EDA = provider.Invoice;
				AssertEquals("EDA Invoice Type", "C", invoice_EDA.Type);
				AssertEquals("EDA Electronic Invoice Receipt Number", "12345", invoice_EDA.ElectronicReceiptNumber);
				AssertEquals("EDA Invoice Number", "1", invoice_EDA.Number);
				AssertEquals("EDA Invoice Valuation Type", "B", invoice_EDA.PriceTypeCode);
				AssertEquals("EDA Invoice Incoterm", "C&F", invoice_EDA.Incoterm);
				AssertEquals("EDA Invoice Currency", "JPY", invoice_EDA.Price.CurrencyCode);
				AssertEquals("EDA Invoice Price", 300m, invoice_EDA.Price.Amount);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_ValueType = ValueTypeList.Codes.S;
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				provider = new CusEntryHeaderMessageProvider(sendingObject);
				AssertEquals("EDA Invoice Currency", string.Empty, invoice_EDA.Price.CurrencyCode);
				AssertNull("EDA Invoice Price", invoice_EDA.Price.Amount);
			});
		}

		public void TestConsignee()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var consigneeAddress = entryHeader.Declaration.DeclarationConsigneeAddress;
			consigneeAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			consigneeAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			consigneeAddress.Organisation.OH_FullName = "Exporter";
			consigneeAddress.Address.Postcode = "111-111";
			consigneeAddress.Address.State = "Tokyo Area";
			consigneeAddress.Address.City = "Tokyo";
			consigneeAddress.Address.Address1 = "Central Street";
			consigneeAddress.Address.Address2 = "East Part";
			consigneeAddress.Address.UnrestrictedAdditionalAddressInformation = "Gozilla Building 1-1";
			consigneeAddress.Address.OA_Phone = "222-222";

			var fsb = consigneeAddress.Organisation.CustomsCodes.AddNew();
			fsb.OK_CodeType = JapanCodeTypes.FSB;
			fsb.OK_CustomsRegNo = "LPC12345678900000";
			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			CombineAssertions(() =>
			{
				var consignee = provider.Consignee;
				AssertEquals("Consignee Code", "LPC12345678900000", consignee.Code);
				AssertEquals("Consignee Name", "Exporter", consignee.Name);
				AssertEquals("Consignee PostCode", "111-111", consignee.PostCode);
				AssertEquals("Consignee Prefecture", "Central Street", consignee.Street1);
				AssertEquals("Consignee City", "East Part", consignee.Street2);
				AssertEquals("Consignee Street", "Tokyo", consignee.City);
				AssertEquals("Consignee Additional Information", "Tokyo Area", consignee.State);
				AssertEquals("Consignee Additional Phone Number", "AU", consignee.CountryCode);
			});
		}

		public void TestVanningLocations()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var vannAddress = instruction.VanningLocations.AddNew();
			vannAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			vannAddress.E2_AddressOverride = true;
			vannAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			vannAddress.E2_State = "Tokyo Area";
			vannAddress.E2_City = "Tokyo";
			vannAddress.E2_Address1 = "Central Street 1000";
			vannAddress.AdditionalAddressInformation = "Gozilla Building 1-1";
			vannAddress.E2_CompanyName = "Company Name";
			vannAddress.E2_GovRegNumType = JapanCodeTypes.LPC;

			var lpc1 = vannAddress.Organisation.CustomsCodes.AddNew();
			lpc1.OK_CodeType = JapanCodeTypes.LPC;
			lpc1.OK_CustomsRegNo = "LPC12345678900000";
			lpc1.OK_RN_NKCodeCountry = "JP";
			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_Address1 = "1";
			orgAddress1.OA_Code = "1";
			orgAddress1.OA_OH = vannAddress.Organisation.PK;
			lpc1.OK_OA_PremisesAddress = orgAddress1.PK;

			var lpc2 = vannAddress.Organisation.CustomsCodes.AddNew();
			lpc2.OK_CodeType = JapanCodeTypes.CIE;
			lpc2.OK_CustomsRegNo = "LPC12345678900001";
			lpc2.OK_RN_NKCodeCountry = "JP";
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_Address1 = "1";
			orgAddress2.OA_Code = "2";
			orgAddress2.OA_OH = vannAddress.Organisation.PK;
			lpc2.OK_OA_PremisesAddress = orgAddress2.PK;

			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			var vanningLocation = provider.VanningLocation;
			CombineAssertions(() =>
			{
				AssertEquals("VanningLocation Prefecture", vannAddress.State, vanningLocation.Prefecture);
				AssertEquals("VanningLocation City", "Tokyo", vanningLocation.City);
				AssertEquals("VanningLocation Street", "Central Street 1000", vanningLocation.Street);
				AssertEquals("VanningLocation Additional Information", "Gozilla Building 1-1", vanningLocation.AdditionalInformation);
				AssertEquals("VanningLocation Name", "Company Name", vanningLocation.Name);

				var codes = provider.VanningLocationCodes.ToList();
				AssertEquals("VanningLocationCodes 1", lpc1.OK_CustomsRegNo, codes[0]);
			});
		}

		public void TestExporter()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var supplierDocumentaryAddress = entryHeader.Declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			supplierDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			supplierDocumentaryAddress.Organisation.OH_FullName = "Exporter";
			supplierDocumentaryAddress.Address.Postcode = "111-111";
			supplierDocumentaryAddress.Address.State = "Tokyo Area";
			supplierDocumentaryAddress.Address.City = "Tokyo";
			supplierDocumentaryAddress.Address.Address1 = "Central Street";
			supplierDocumentaryAddress.Address.Address2 = "1000";
			supplierDocumentaryAddress.Address.UnrestrictedAdditionalAddressInformation = "Gozilla Building 1-1";
			supplierDocumentaryAddress.Address.OA_Phone = "+1 (273) 549521";

			var lpc = supplierDocumentaryAddress.Address.CustomsCodes.AddNew();
			lpc.OK_CodeType = JapanCodeTypes.LPC;
			lpc.OK_CustomsRegNo = "LPC12345678900000";
			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			CombineAssertions(() =>
			{
				var exporter = provider.Exporter;
				AssertEquals("EDA Importer Code", "LPC12345678900000", exporter.Code);
				AssertEquals("EDA Importer Name", "Exporter", exporter.Name);
				AssertEquals("EDA Importer PostCode", "111111", exporter.PostCode);
				AssertEquals("EDA Importer Prefecture", "Tokyo Area", exporter.Prefecture);
				AssertEquals("EDA Importer City", "Tokyo", exporter.City);
				AssertEquals("EDA Importer Street", "Central Street 1000", exporter.Street);
				AssertEquals("EDA Importer Additional Information", "Gozilla Building 1-1", exporter.AdditionalInformation);
				AssertEquals("EDA Importer Additional Phone Number", "+1273549521", exporter.Phone);
			});
		}

		#region IIDAEntry

		public void TestProperties_IDA()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			declaration.JE_CustomsOffice = "23";
			declaration.JE_CustomsOfficeDepartment = "45";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_RL_NKPortOfArrival = "JPTYO";
			declaration.JE_PaymentDeadlineExtension = "T";
			declaration.JE_PaymentMethod = "C";
			declaration.JE_DefermentAccountNumber = "BANK001";

			entryInstruction.CEI_CustomsOfficeForSpecialDeclarations = "67";
			entryInstruction.CEI_CustomsOfficeDepartmentForSpecialDeclarations = "89";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			entryInstruction.CEI_TradeType = "ABC";
			entryInstruction.CEI_TradeControlOrder = "C";
			entryInstruction.CEI_CommercialValueType = "D";
			entryInstruction.CEI_ContentInspectionResult = "E";
			entryInstruction.CEI_CustomsInspectionCode = "F";
			entryInstruction.CEI_CommonControlNumber = "G";
			entryInstruction.CEI_BondedLocationCode = "2TW14";
			entryInstruction.CEI_BeforePermitApplicationReason = "1A";
			entryInstruction.CEI_DutyDrawback = YesNoList.Codes.Yes;

			invoiceHeader.JZ_FreightType = "A";
			invoiceHeader.JZ_InsuranceType = "B";
			invoiceHeader.JZ_ComprehensiveInsuranceNumber = "INS001";
			invoiceHeader.JZ_ValuationCode = "5";
			invoiceHeader.JZ_AdvanceRulingOnValuation1 = "ADV001";
			invoiceHeader.JZ_AdvanceRulingOnValuation2 = "ADV002";

			Factory.Save();

			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject);

			CombineAssertions(() =>
			{
				AssertEquals("TaxRebateType", "X", provider.TaxRebateType);
				AssertEquals("CustomsOffice", declaration.JE_CustomsOffice, provider.CustomsOffice);
				AssertEquals("CustomsOfficeDepartment", declaration.JE_CustomsOfficeDepartment, provider.CustomsOfficeDepartment);
				AssertEquals("CustomsOfficeForSepcialDeclaration", entryInstruction.CEI_CustomsOfficeForSpecialDeclarations, provider.CustomsOfficeForSepcialDeclaration);
				AssertEquals("CustomsOfficeDepartmentForSpecialDeclaration", entryInstruction.CEI_CustomsOfficeDepartmentForSpecialDeclarations, provider.CustomsOfficeDepartmentForSpecialDeclaration);
				AssertEquals("DeclarationDate", entryInstruction.CEI_DateForDuty.ToDateTime(), provider.DeclarationDate);
				AssertEquals("Arrival Date", ZDateTime.Today, provider.DateOfArrival);
				AssertEquals("Port of Unloading Code", "TYO", provider.PortOfUnloading.Code);
				AssertEquals("TradeType", entryInstruction.CEI_TradeType, provider.TradeType);
				AssertEquals("ImportTradeControlOrdinanceArticle3", entryInstruction.CEI_TradeControlOrder, provider.ImportTradeControlOrdinanceArticle3);
				AssertEquals("ImportApprovalCertificateHasCommercialValue", entryInstruction.CEI_CommercialValueType, provider.ImportApprovalCertificateHasCommercialValue);
				AssertEquals("ResultOfContentInspection", entryInstruction.CEI_ContentInspectionResult, provider.ResultOfContentInspection);
				AssertEquals("CustomsInspectionCode", entryInstruction.CEI_CustomsInspectionCode, provider.CustomsInspectionCode);
				AssertEquals("CommonControlNumber", entryInstruction.CEI_CommonControlNumber, provider.CommonControlNumber);
				AssertEquals("Freight Type", "A", provider.FreightTypeCode);
				AssertEquals("Insurance Type", "B", provider.InsuranceTypeCode);
				AssertEquals("Comprehensive Insurance Number", "INS001", provider.ComprehensiveInsuranceNumber);
				AssertEquals("Valuation Type", "5", provider.ValuationType);
				AssertEquals("Valuation Correction Type", "DP", provider.ValuationCorrectionTypeCode);
				AssertEquals("Advanced Ruling on Valuation 1", "ADV001", provider.AdvanceRulingOnValuation.ToArray()[0]);
				AssertEquals("Advanced Ruling on Valuation 2", "ADV002", provider.AdvanceRulingOnValuation.ToArray()[1]);
				AssertEquals("Bonded Location Code", "2TW14", provider.BondedAreaCode);
				AssertEquals("Payment Deadline Extension Code", "T", provider.PaymentDeadlineExtensionCode);
				AssertEquals("Before Permit Application Code", "1A", provider.BeforePermitApplicationReasonCode);
				AssertEquals("Payment Method", "C", provider.PaymentMethod);
				AssertEquals("Bank Account Number", "BANK001", provider.BankAccountNumber);
			});
		}

		public void TestImportItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_NACCSCode = "X";
			invoiceLine.JI_Description = "1234567890123456789012345678901234567890";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsSecondQuantity = 20;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_Calc_Preference = "WK";
			invoiceLine.JI_Calc_OriginCertifier = "A";
			invoiceLine.JI_Calc_CertificateOfOriginCertifier = "G";
			invoiceLine.JI_TradeControlOrderAppendix = "1234";
			invoiceLine.JI_StorageType = "L";
			invoiceLine.JI_AdvanceRulingOnClassification = "B22222222";
			invoiceLine.JI_AdvanceRulingOnOrigin = "B333333";
			invoiceLine.JI_DutyReductionExemptionRefundCode = "1";
			invoiceLine.JI_DutyReductionAmount = 10;

			var consumptionTax = invoiceLine.DomesticConsumptionTaxes.AddNew();
			consumptionTax.BZ_Tariff = "E1200";
			consumptionTax.BZ_ExemptionReductionCode = "E01";
			consumptionTax.BZ_Value = 100;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_CustomsValue = 100;
			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject);
			Factory.Save();

			for (var i = 0; i < 6; i++)
			{
				invoiceLine.DomesticConsumptionTaxes.AddNew();
			}
			for (var i = 0; i < 99; i++)
			{
				var newLine = invoiceHeader.InvoiceLines.AddNew();
				newLine.JI_CL = entryLine.PK;
			}

			CombineAssertions(() =>
			{
				var importItem = (provider as IIDAEntry).Items.FirstOrDefault();
				AssertEquals("IDA Tariff Code", "123456789", importItem.Item.TariffCode);
				AssertEquals("IDA NACCS Code", "X", importItem.Item.NACCSCode);
				AssertEquals("IDA Goods Description", "1234567890123456789012345678901234567890", importItem.Item.GoodDescription);
				AssertEquals("IDA Goods Origin Code", "AU", importItem.Item.CountryOfOrigin);
				AssertEquals("IDA Quantity 1", new decimal(10), importItem.Item.Quantity1.Quantity);
				AssertEquals("IDA Quantity 1 Unit", "NO", importItem.Item.Quantity1.Unit);
				AssertEquals("IDA Quantity 2", new decimal(20), importItem.Item.Quantity2.Quantity);
				AssertEquals("IDA Quantity 2 Unit", "KG", importItem.Item.Quantity2.Unit);
				AssertEquals("IDA CertificateOfOrigin", "WKAG", importItem.CertificateOfOrigin);
				AssertEquals("IDA ImportTradeControlOrdinanceAppendixCode", "1234", importItem.ImportTradeControlOrdinanceAppendixCode);
				AssertEquals("IDA StorageType", "L", importItem.StorageType);
				AssertNull("IDA Customs Value Apportionment Coefficient", importItem.CustomsValueApportionmentCoefficient);
				AssertNullOrEmpty("IDA Freight Apportionment Type", importItem.FreightApportionmentType);
				AssertEquals("IDA Customs Value", new decimal(100), importItem.CustomsValue.Amount);
				AssertEquals("IDA Advanced Ruling on Classification", "B22222222", importItem.AdvanceRulingOnClassification);
				AssertEquals("IDA Advanced Ruling on Origin", "B333333", importItem.AdvanceRulingOnOrigin);
				AssertEquals("IDA Duty Reduction Exemption Code", "1", importItem.DutyReductionExemptionCode);
				AssertEquals("IDA Duty Reduction Amount", new decimal(10), importItem.DutyReductionAmount);
				var tariff = importItem.DomesticConsumptionTaxCodes.FirstOrDefault();
				AssertEquals("IDA Domestic Consumption Tax Type Code", "E1200", tariff.Type);
				AssertEquals("IDA Domestic Consumption Tax Exemption Reduction Code", "E01", tariff.ReductionCode);
				AssertEquals("IDA Domestic Consumption Tax Reduction Amount", new decimal(100), tariff.ReductionAmount);
				AssertEquals(7, invoiceLine.DomesticConsumptionTaxes.Count);
				AssertEquals("Only the first six domestic consumption taxes will be populated.", 6, importItem.DomesticConsumptionTaxCodes.Count());

				AssertEquals(100, invoiceHeader.InvoiceLines.Count);
				AssertEquals("Should use entry line instead of invoice lines", 1, (provider as IIDAEntry).Items.Count());
			});
		}

		public void TestVessel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			vessel.RV_RadioCallSign = "RCS";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateVesselZZ("TESTV1", "LXBH", "CV", "JP");
			helper.CreateVesselZZ("TESTV2", "9999", "CV", "JP");
			Factory.Save();

			declaration.JE_VesselName = "VESSEL";
			declaration.JE_RadioCallSign = "LXBH";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			sendingObject.ProcedureCode = JPProcedureCodeList.Codes.IDC;
			var provider = new CusEntryHeaderMessageProvider(sendingObject);

			AssertEquals("IDC Vessel Code", "LXBH", provider.Vessel.Code);
			AssertEquals("IDC Vessel Name", ZString.Empty, provider.Vessel.Name);

			declaration.JE_RadioCallSign = "9999";
			AssertEquals("IDC Vessel Code", "9999", provider.Vessel.Code);
			AssertEquals("IDC Vessel Name", "VESSEL", provider.Vessel.Name);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "NO0001";
			declaration.JE_ExportDate = new ZDateTime(2024, 7, 30);
			sendingObject = new MessageSendingObject(entryHeader);
			sendingObject.ProcedureCode = JPProcedureCodeList.Codes.IDA;
			provider = new CusEntryHeaderMessageProvider(sendingObject);
			AssertEquals("IDA Vessel Code", "9999", provider.Vessel.Code);
			AssertEquals("IDA Vessel Name", "NO0001/30JUL", provider.Vessel.Name);

			sendingObject = new MessageSendingObject(entryHeader);
			sendingObject.ProcedureCode = JPProcedureCodeList.Codes.IDC;
			provider = new CusEntryHeaderMessageProvider(sendingObject);
			AssertNull("IDC Vessel Code", provider.Vessel);
		}

		public void TestBondedDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_BondedDate = ZDateTime.Today;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_BondedDate = ZDateTime.Today.AddDays(-1);
			invoiceLine3.JI_CL = entryLine.PK;
			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("Into Bonded Area First Approval Date", ZDateTime.Today.AddDays(-1).ToDateTime(), provider.IntoBondedAreaFirstApprovalDate);
		}

		public void TestOtherLawCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var otherLawCode = entryInstruction.OtherLaws.AddNew();
			otherLawCode.CFR_Reference = "AC";
			for (var i = 0; i < 5; i++)
			{
				entryInstruction.OtherLaws.AddNew();
			}

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("IDA Code for verification based on other laws and regulations (other than customs laws)", "AC", provider.OtherLawCodes.ToArray()[0]);
			AssertEquals(6, entryInstruction.OtherLaws.Count);
			AssertEquals("Only the first five other law codes will be populated.", 5, provider.OtherLawCodes.Count());
		}

		public void TestComprehensiveValuationDeclarationNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var number = invoiceHeader.ComprehensiveValuations.AddNew();
			number.CFR_Reference = "123456";
			for (var i = 0; i < 3; i++)
			{
				invoiceHeader.ComprehensiveValuations.AddNew();
			}

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("IDA Guarantee Number", "123456", provider.ComprehensiveValuationDeclarationNumbers.ToArray()[0]);
			AssertEquals(4, invoiceHeader.ComprehensiveValuations.Count);
			AssertEquals("Only the first three comprehensive valuation declaration numbers will be populated.", 3, provider.ComprehensiveValuationDeclarationNumbers.Count());
		}

		public void TestGuaranteeNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var guarantee1 = entryInstruction.Guarantees.AddNew();
			guarantee1.CFR_Reference = "GUAR001";
			for (var i = 0; i < 2; i++)
			{
				entryInstruction.Guarantees.AddNew();
			}

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("IDA Guarantee Number", "GUAR001", provider.GuaranteeNumbers.ToArray()[0]);
			AssertEquals(3, entryInstruction.Guarantees.Count);
			AssertEquals("Only the first two guarantee numbers will be populated.", 2, provider.GuaranteeNumbers.Count());
		}

		public void TestFreightAndInsurance()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));

			AssertEquals("Freght Currency", "", provider.Freight.CurrencyCode);
			AssertNull("Freight Amount", provider.Freight.Amount);
			AssertEquals("Insurance Currency", "", provider.Insurance.CurrencyCode);
			AssertNull("Insurance Amount", provider.Insurance.Amount);

			var freightCharge1 = invoiceHeader.Charges.AddNew();
			var insuranceCharge1 = invoiceHeader.Charges.AddNew();
			freightCharge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			freightCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;
			freightCharge1.J7_Amount = 100.1;
			insuranceCharge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			insuranceCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;
			insuranceCharge1.J7_Amount = 50.1;

			var freightCharge2 = invoiceHeader2.Charges.AddNew();
			var insuranceCharge2 = invoiceHeader2.Charges.AddNew();
			freightCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			freightCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			freightCharge2.J7_Amount = 100;
			insuranceCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			insuranceCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			insuranceCharge2.J7_Amount = 50;

			var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-2);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
			exchangeRate.RE_SellRate = 2m;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));

			AssertEquals("Freght Currency", "JPY", provider.Freight.CurrencyCode);
			AssertEquals("Freight Amount", 150m, provider.Freight.Amount);
			AssertEquals("Insurance Currency", "JPY", provider.Insurance.CurrencyCode);
			AssertEquals("Insurance Amount", 75m, provider.Insurance.Amount);
		}

		public void TestImporter()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var importerDocumentaryAddress = entryHeader.Declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			importerDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			importerDocumentaryAddress.Organisation.OH_FullName = "Importer";
			importerDocumentaryAddress.Address.Postcode = "111-111";
			importerDocumentaryAddress.Address.State = "Tokyo Area";
			importerDocumentaryAddress.Address.City = "Tokyo";
			importerDocumentaryAddress.Address.Address1 = "Central Street";
			importerDocumentaryAddress.Address.Address2 = "1000";
			importerDocumentaryAddress.Address.UnrestrictedAdditionalAddressInformation = "Gozilla Building 1-1";
			importerDocumentaryAddress.Address.OA_Phone = "+1 (273) 549521";

			var lpc = importerDocumentaryAddress.Address.CustomsCodes.AddNew();
			lpc.OK_CodeType = JapanCodeTypes.LPC;
			lpc.OK_CustomsRegNo = "LPC12345678900000";
			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			CombineAssertions(() =>
			{
				var importer = (provider as IIDAEntry).Importer;
				AssertEquals("IDA Importer Code", "LPC12345678900000", importer.Code);
				AssertEquals("IDA Importer Name", "Importer", importer.Name);
				AssertEquals("IDA Importer PostCode", "111111", importer.PostCode);
				AssertEquals("IDA Importer Prefecture", "Tokyo Area", importer.Prefecture);
				AssertEquals("IDA Importer City", "Tokyo", importer.City);
				AssertEquals("IDA Importer Street", "Central Street 1000", importer.Street);
				AssertEquals("IDA Importer Additional Information", "Gozilla Building 1-1", importer.AdditionalInformation);
				AssertEquals("IDA Importer Additional Phone Number", "+1273549521", importer.Phone);
			});
		}

		public void TestImportTrader()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var declarationConsigneeAddress = entryHeader.Declaration.DeclarationConsigneeAddress;
			declarationConsigneeAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			declarationConsigneeAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			declarationConsigneeAddress.Organisation.OH_FullName = "Consignee";

			var lpc = declarationConsigneeAddress.Address.CustomsCodes.AddNew();
			lpc.OK_CodeType = JapanCodeTypes.LPC;
			lpc.OK_CustomsRegNo = "LPC12345678900000";
			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			CombineAssertions(() =>
			{
				var importTrader = provider.ImportTrader;
				AssertEquals("IDA ImportTrader Code", "LPC12345678900000", importTrader.Code);
				AssertEquals("IDA ImportTrader Name", "Consignee", importTrader.Name);
			});
		}

		public void TestShipper()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var supplierDocumentaryAddress = entryHeader.Declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			supplierDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			supplierDocumentaryAddress.Organisation.OH_FullName = "Supplier";
			supplierDocumentaryAddress.Address.Postcode = "111-111";
			supplierDocumentaryAddress.Address.State = "NSW";
			supplierDocumentaryAddress.Address.City = "Sydney";
			supplierDocumentaryAddress.Address.Address1 = "Central Street";
			supplierDocumentaryAddress.Address.Address2 = "1000";
			supplierDocumentaryAddress.Address.UnrestrictedAdditionalAddressInformation = "Kangroo Building 1-1";
			supplierDocumentaryAddress.Address.OA_Phone = "222-222";

			var lpc = supplierDocumentaryAddress.Organisation.CustomsCodes.AddNew();
			lpc.OK_CodeType = JapanCodeTypes.FSB;
			lpc.OK_CustomsRegNo = "FSB123";
			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			CombineAssertions(() =>
			{
				var shipper = provider.Shipper;
				AssertEquals("IDA Importer Code", "FSB123", shipper.Code);
				AssertEquals("IDA Importer Name", "Supplier", shipper.Name);
				AssertEquals("IDA Importer PostCode", "111-111", shipper.PostCode);
				AssertEquals("IDA Importer Prefecture", "New South Wales", shipper.State);
				AssertEquals("IDA Importer City", "Sydney", shipper.City);
				AssertEquals("IDA Importer Street", "Central Street", shipper.Street1);
				AssertEquals("IDA Importer Additional Information", "1000", shipper.Street2);
			});
		}
		#endregion

		#region IEDAEntry

		public void TestProperties_EDA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			declaration.JE_HouseBill = "HB202402";
			declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Air;
			declaration.JE_RL_NKFinalDestination = "JPAAK";
			declaration.JE_FinalDestinationName = "Kamakura";
			declaration.JE_DateAtOrigin = ZDateTime.Today;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 300m;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			declaration.JE_CustomsOffice = "23";
			declaration.JE_CustomsOfficeDepartment = "45";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			entryInstruction.ExportControlNumber = "67";
			entryInstruction.CEI_TradeType = "ABC";
			entryInstruction.CEI_CustomsInspectionCode = "DE";
			entryInstruction.CEI_PreInspectedCargoType = "F";
			entryInstruction.CEI_LoadingConfirmationIsRequired = true;
			entryInstruction.CEI_ApprovalCertificateCategory = "O";

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10m;
			invoiceLine2.JI_LinePrice = 20m;
			invoiceLine3.JI_LinePrice = 30m;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine3.JI_CL = entryLine.PK;
			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader3.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			invoiceHeader1.JZ_InvoiceAmount = 100m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoiceHeader2.JZ_InvoiceAmount = 200m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoiceHeader3.JZ_InvoiceAmount = 400m;
			invoiceHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;

			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			CombineAssertions(() =>
			{
				AssertEquals("CustomsOffice", declaration.JE_CustomsOffice, provider.CustomsOffice);
				AssertEquals("CustomsOfficeDepartment", declaration.JE_CustomsOfficeDepartment, provider.CustomsOfficeDepartment);
				AssertEquals("DeclarationDate", entryInstruction.CEI_DateForDuty.ToDateTime(), provider.DeclarationDate);
				AssertEquals("ExportControlNumber", entryInstruction.ExportControlNumber, provider.ExportControlNumber);
				AssertEquals("TradeType", entryInstruction.CEI_TradeType, provider.TradeType);
				AssertEquals("CustomsInspectionCode", entryInstruction.CEI_CustomsInspectionCode, provider.CustomsInspectionCode);
				AssertEquals("PreInspectedCargoType", entryInstruction.CEI_PreInspectedCargoType, provider.PreInspectedCargoType);
				AssertEquals("BillNumber Air", declaration.JE_HouseBill, provider.BillNumber);
				AssertEquals("FinalDestination Code", declaration.JE_RL_NKFinalDestination, provider.FinalDestination.Code);
				AssertEquals("FinalDestination Name", declaration.JE_FinalDestinationName, provider.FinalDestination.Name);
				AssertNull("Vessel Air", provider.Vessel);
				AssertEquals("DateOfDeparture", declaration.JE_DateAtOrigin.ToDateTime(), provider.DateOfDeparture);
				AssertEquals("RequiresShippingOrLoadingConfirmation", "Y", provider.RequiresShippingOrLoadingConfirmation);
				AssertEquals("ExportControlNumber", entryInstruction.CEI_ApprovalCertificateCategory, provider.ExportApprovalCertificateClassification);
				AssertEquals("FOB Price", 400m, provider.FOB.Amount);
				AssertEquals("FOB Currency", "JPY", provider.FOB.CurrencyCode);

				entryInstruction.CEI_Style = JPExportDeclarationTypeList.Codes.G;
				AssertEquals("FOB Price", 60m, provider.FOB.Amount);
				AssertEquals("FOB Currency", "JPY", provider.FOB.CurrencyCode);

				AssertEquals("Total Basic Price", null, provider.TotalBasicPrice);

				declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
				declaration.JE_VesselName = "CSCL NEW YORK";
				declaration.JE_RadioCallSign = "RCS";
				provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
				AssertEquals("BillNumber not Air", string.Empty, provider.BillNumber);
				AssertEquals("VesselName Sea when VesselRadioCallSign is not 9999", string.Empty, provider.Vessel.Name);
				AssertEquals("VesselRadioCallSign Sea", "RCS", provider.Vessel.Code);

				declaration.JE_RadioCallSign = "9999";
				provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
				AssertEquals("VesselName Sea when VesselRadioCallSign is 9999", "CSCL NEW YORK", provider.Vessel.Name);
				AssertEquals("VesselRadioCallSign Sea", "9999", provider.Vessel.Code);

				entryInstruction.CEI_ValueType = ValueTypeList.Codes.S;
				provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
				AssertEquals("FOB Price", 300m, provider.FOB.Amount);
				AssertEquals("FOB Currency", "JPY", provider.FOB.CurrencyCode);
			});
		}

		public void TestExportItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_NACCSCode = "X";
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsSecondQuantity = 20;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_TradeControlOrderAppendix = "A";
			invoiceLine.JI_FEFTAArticle48 = "B";
			invoiceLine.JI_DutyReductionExemptionRefundCode = "C";
			invoiceLine.JI_DomesticConsumptionTaxExemptionCode = "D";
			invoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial = true;
			invoiceLine.JI_LinePrice = 3m;

			var otherLaw1 = invoiceLine.OtherLaws.AddNew();
			otherLaw1.CFR_Reference = "M";
			var otherLaw2 = invoiceLine.OtherLaws.AddNew();
			otherLaw2.CFR_Reference = "N";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.EffectiveDescription = "1234567890123456789012345678901234567890";
			invoiceLine.JI_CL = entryLine.PK;

			for (var i = 0; i < 99; i++)
			{
				var newLine = invoiceHeader.InvoiceLines.AddNew();
				newLine.JI_CustomsQuantity = 10;
				newLine.JI_CustomsUnitQty = "NO";
				newLine.JI_CustomsSecondQuantity = 20;
				newLine.JI_CustomsSecondUnitQty = "KG";
				newLine.JI_CL = entryLine.PK;
			}

			Factory.Save();

			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject);
			CombineAssertions(() =>
			{
				var exportItem = (provider as IEDAEntry).Items.FirstOrDefault();
				AssertEquals("EDA Tariff Code", "123456789", exportItem.Item.TariffCode);
				AssertEquals("EDA NACCS Code", "X", exportItem.Item.NACCSCode);
				AssertEquals("EDA Goods Description", "1234567890123456789012345678901234567890", exportItem.Item.GoodDescription);
				AssertEquals("EDA Quantity 1", 1000m, exportItem.Item.Quantity1.Quantity);
				AssertEquals("EDA Quantity 1 Unit", "NO", exportItem.Item.Quantity1.Unit);
				AssertEquals("EDA Quantity 2", 2000m, exportItem.Item.Quantity2.Quantity);
				AssertEquals("EDA Quantity 2 Unit", "KG", exportItem.Item.Quantity2.Unit);
				AssertEquals(100, invoiceHeader.InvoiceLines.Count);
				AssertEquals("Should use entry line instead of invoice line", 1, (provider as IEDAEntry).Items.Count());
				AssertEquals("ExportControlOrdinanceAppendixCode", invoiceLine.JI_TradeControlOrderAppendix, exportItem.ExportControlOrdinanceAppendixCode);
				AssertEquals("ForeignExchangeLawArticle48Code", invoiceLine.JI_FEFTAArticle48, exportItem.ForeignExchangeLawArticle48Code);
				AssertEquals("DutyExemptionReductionRefundCode", invoiceLine.JI_DutyReductionExemptionRefundCode, exportItem.DutyExemptionReductionRefundCode);
				AssertEquals("DomesticConsumptionTaxExemptionCode", invoiceLine.JI_DomesticConsumptionTaxExemptionCode, exportItem.DomesticConsumptionTaxExemptionCode);
				AssertEquals("ExportControlOrdinanceAppendixCode", "P", exportItem.DomesticConsumptionTaxExemptionType);
				AssertEquals("OtherLawCodes", invoiceLine.OtherLaws.Count, exportItem.OtherLawCodes.Count());

				var exportItemOtherLawCodes = exportItem.OtherLawCodes.ToList();
				for (int i = 0; i < exportItemOtherLawCodes.Count; i++)
				{
					AssertEquals("OtherLawCode", invoiceLine.OtherLaws[i].CFR_Reference, exportItemOtherLawCodes[i]);
				}

				entryHeader.InvoiceLines.ToArray()[1].JI_LinePrice = 2m;
				AssertNull("Coefficient", exportItem.BasicPrice.Coefficient);

				invoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial = false;
				sendingObject = new MessageSendingObject(entryHeader);
				provider = new CusEntryHeaderMessageProvider(sendingObject);
				exportItem = (provider as IEDAEntry).Items.FirstOrDefault();
				AssertEquals("ExportControlOrdinanceAppendixCode", "A", exportItem.DomesticConsumptionTaxExemptionType);

				invoiceLine.JI_DomesticConsumptionTaxExemptionCode = string.Empty;
				sendingObject = new MessageSendingObject(entryHeader);
				provider = new CusEntryHeaderMessageProvider(sendingObject);
				exportItem = (provider as IEDAEntry).Items.FirstOrDefault();
				AssertEquals("ExportControlOrdinanceAppendixCode", string.Empty, exportItem.DomesticConsumptionTaxExemptionType);
			});
		}

		#endregion
		#endregion

		public void TestEntrySubmissionProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_DeclarationCondition = ImportDeclarationConditionList.Codes.Z;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = new CusEntryHeaderMessageProvider(sendingObject) as IEntrySubmission;
			AssertEquals(nameof(provider.DeclarationCondition), ImportDeclarationConditionList.Codes.Z, provider.DeclarationCondition);
		}

		public void TestMainPartyTypeWithIDA()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("Default to 2", "2", provider.MainPartyType);

			var importerDocumentaryAddress = entryHeader.Declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
			importerDocumentaryAddress.Address.OA_OH = Factory.New<OrgHeader>().PK;

			var orgCode = importerDocumentaryAddress.Organisation.CustomsCodes.AddNew();
			orgCode.OK_CodeType = JapanCodeTypes.LPC;
			orgCode.OK_CustomsRegNo = "LPC1234567890";

			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("1", provider.MainPartyType);

			orgCode.OK_CodeType = JapanCodeTypes.CIE;
			orgCode.OK_CustomsRegNo = "CIE123450001";

			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("2", provider.MainPartyType);

			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_GovRegNumType = JapanCodeTypes.LPC;
			importerDocumentaryAddress.E2_GovRegNum = ZString.Empty;
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("2", provider.MainPartyType);

			importerDocumentaryAddress.E2_GovRegNum = "LPC1234567890";
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("1", provider.MainPartyType);

			importerDocumentaryAddress.E2_GovRegNumType = JapanCodeTypes.CIE;
			importerDocumentaryAddress.E2_GovRegNum = "CIE123450001";
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("2", provider.MainPartyType);
		}

		public void TestMainPartyTypeWithEDA()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("Default to 2", "2", provider.MainPartyType);

			var supplierDocumentaryAddress = entryHeader.Declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
			supplierDocumentaryAddress.Address.OA_OH = Factory.New<OrgHeader>().PK;

			var orgCode = supplierDocumentaryAddress.Organisation.CustomsCodes.AddNew();
			orgCode.OK_CodeType = JapanCodeTypes.LPC;
			orgCode.OK_CustomsRegNo = "LPC1234567890";

			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("1", provider.MainPartyType);

			orgCode.OK_CodeType = JapanCodeTypes.CIE;
			orgCode.OK_CustomsRegNo = "CIE123450001";

			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("2", provider.MainPartyType);

			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_GovRegNumType = JapanCodeTypes.LPC;
			supplierDocumentaryAddress.E2_GovRegNum = ZString.Empty;
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("2", provider.MainPartyType);

			supplierDocumentaryAddress.E2_GovRegNum = "LPC1234567890";
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("1", provider.MainPartyType);

			supplierDocumentaryAddress.E2_GovRegNumType = JapanCodeTypes.CIE;
			supplierDocumentaryAddress.E2_GovRegNum = "CIE123450001";
			provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("2", provider.MainPartyType);
		}

		public void TestMarksAndNumbers()
		{
			var entryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			instruction.JP_MarksAndNumbers = "MARKS AND NUMBERS SHOULD BE LESS THAN 140 CHARACTERS";
			entryHeader.CH_CEI_Instruction = instruction.PK;
			Factory.Save();

			var provider = new CusEntryHeaderMessageProvider(new MessageSendingObject(entryHeader));
			AssertEquals("MARKS AND NUMBERS SHOULD BE LESS THAN 140 CHARACTERS", provider.MarksAndNumbers);
		}

		public void TestECR()
		{
			var credential = Factory.New<GlbExternalPasswordCUS>();
			credential.GP_MailBoxID = "TestMail";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_NACCSCredential = credential.PK;
			declaration.JE_VesselName = "VESSEL";
			declaration.JE_RadioCallSign = "RCS";
			declaration.JE_CarrierCode = "CC";
			declaration.JE_VoyageFlightNo = "NO1";
			declaration.JE_DateOfArrival = new ZDateTime(2024, 7, 31);
			declaration.JE_ArrivalAtLoadingDate = new ZDate(2024, 11, 22);
			declaration.JE_RL_NKPortOfLoading = "SHG";
			declaration.JE_ExportDate = new ZDateTime(2024, 7, 30);
			declaration.JE_RL_NKPortOfArrival = "TYO";
			declaration.JE_DeclarationReference = "1324555";
			declaration.JE_RL_NKFinalDestination = "USD";
			declaration.JE_ReceiptMode = ReceiptModeList.Codes.Mode52;
			declaration.JE_DeliveryMode = DeliveryModeList.Codes.Mode54;
			declaration.ExternalBrokerCode = "12345";

			var bookingNumber = declaration.AdditionalReferenceNumbers.AddNew();
			bookingNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			bookingNumber.CE_EntryNum = "1234567890123456";

			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			supplierDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			supplierDocumentaryAddress.Organisation.OH_FullName = "Exporter";

			var lpc = supplierDocumentaryAddress.Address.CustomsCodes.AddNew();
			lpc.OK_CodeType = JapanCodeTypes.LPC;
			lpc.OK_CustomsRegNo = "LPC12345678900000";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CargoQuantity = 1;
			instruction.CEI_CargoQuantityUnit = "KG";
			instruction.CEI_GrossWeight = 3;
			instruction.CEI_GrossWeightUnit = "KG";
			instruction.CEI_Volume = 5;
			instruction.CEI_VolumeUnit = "KG";
			instruction.CEI_SpecialCargoCode = "B";
			instruction.CEI_Style = "G";
			instruction.CEI_GoodsDescription = "goods desc";
			instruction.CEI_ECRCargoType = "T";
			instruction.NSI = "111";

			var ecrNote = instruction.Notes.AddNew();
			ecrNote.ST_NoteText = "ECR NOTE TEST";
			ecrNote.ST_Description = StmNoteDescriptions.ECRNotesDescription;

			var moveInDestinationInfo = instruction.MoveInDestinationInfos.AddNew();
			instruction.MoveInDestinationInfos.AddNew();

			moveInDestinationInfo.CSI_Code = "Cod";
			moveInDestinationInfo.CSI_DateOfIssue = new ZDateTime(2022, 5, 17);
			moveInDestinationInfo.CSI_ReferenceNumber = "Kar";
			moveInDestinationInfo.CSI_Quantity = 11;
			moveInDestinationInfo.CSI_Quantity2 = 22;
			moveInDestinationInfo.CSI_Quantity3 = 33;
			moveInDestinationInfo.CSI_Description = "Desc";

			var inventory = moveInDestinationInfo.InventoryNumbers.AddNew();
			inventory.CSI_Code = "C";
			inventory.CSI_Quantity = 6;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			Factory.Save();

			var sendingObject = new MessageSendingObject(entryHeader);
			sendingObject.ProcedureCode = JPProcedureCodeList.Codes.ECR;

			var provider = new CusEntryHeaderMessageProvider(sendingObject);

			CombineAssertions(() =>
			{
				var exporter = provider.Exporter;
				AssertEquals("EDA Importer Code", "LPC12345678900000", exporter.Code);
				AssertEquals("EDA Importer Name", "Exporter", exporter.Name);

				AssertEquals("DeclarantCode", declaration.ExternalBrokerCode, provider.DeclarantCode);
				AssertEquals("GoodsDescription", instruction.CEI_GoodsDescription, provider.GoodsDescription);

				var quantity = provider.Quantity;
				AssertEquals("Quantity Quantity", instruction.CEI_CargoQuantity, quantity.Quantity);
				AssertEquals("Quantity Unit", instruction.CEI_CargoQuantityUnit, quantity.Unit);

				var weight = provider.Weight;
				AssertEquals("Weight Quantity", instruction.CEI_GrossWeight, weight.Quantity);
				AssertEquals("Weight Unit", "KGM", weight.Unit);

				var volume = provider.Volume;
				AssertEquals("Volume Quantity", instruction.CEI_Volume, volume.Quantity);
				AssertEquals("Volume Unit", instruction.CEI_VolumeUnit, volume.Unit);

				AssertEquals("CarrierCode", declaration.JE_CarrierCode, provider.CarrierCode);

				var vesselProvider = provider.Vessel;
				AssertEquals("Vessel Code", declaration.JE_RadioCallSign, vesselProvider.Code);
				AssertEquals("Vessel Name", ZString.Empty, vesselProvider.Name);

				AssertEquals("VoyageNumber", declaration.JE_VoyageFlightNo, provider.VoyageNumber);
				AssertEquals("ArrivalDate", declaration.JE_ArrivalAtLoadingDate.ToDateTime(), provider.ArrivalDate);
				AssertEquals("PortofLoadingCode", declaration.JE_RL_NKPortOfLoading, provider.PortofLoadingCode);
				AssertEquals("DateOfDeparture", declaration.JE_ExportDate.ToDateTime(), provider.DateOfDeparture);
				AssertEquals("PortOfDischarge", declaration.JE_RL_NKPortOfArrival, provider.PortOfDischarge);
				AssertEquals("InternalReferenceNumber", declaration.JE_DeclarationReference, provider.InternalReferenceNumber);
				AssertEquals("FinalDestinationCode", declaration.JE_RL_NKFinalDestination, provider.FinalDestinationCode);
				AssertEquals("BookingNumber", bookingNumber.CE_EntryNum, provider.BookingNumber);
				AssertEquals("DangerousGoodsCode", instruction.CEI_SpecialCargoCode, provider.DangerousGoodsCode);
				AssertEquals("CargoType", "T", provider.CargoType);
				AssertEquals("Notes", ecrNote.ST_NoteText, provider.Notes);
				AssertEquals("ReceiptMode", declaration.JE_ReceiptMode, provider.ReceiptMode);
				AssertEquals("DeliveryMode", declaration.JE_DeliveryMode, provider.DeliveryMode);
				AssertEquals("NSI", instruction.NSI, provider.NSINumber);

				AssertEquals("MoveInDestinations.Count", 2, provider.MoveInDestinations.Count());
				var moveInDestinations = provider.MoveInDestinations.First();
				AssertEquals("MoveInDestination", moveInDestinationInfo.CSI_Code, moveInDestinations.MoveInDestination);
				AssertEquals("MoveInDate", moveInDestinationInfo.CSI_DateOfIssue.ToDateTime(), moveInDestinations.MoveInDate);
				AssertEquals("VanningLocationCode", moveInDestinationInfo.CSI_ReferenceNumber, moveInDestinations.VanningLocationCode);
				AssertEquals("MoveInQuantity", moveInDestinationInfo.CSI_Quantity, moveInDestinations.MoveInQuantity);
				AssertEquals("MoveInWeight", moveInDestinationInfo.CSI_Quantity2, moveInDestinations.MoveInWeight);
				AssertEquals("MoveInVolumn", moveInDestinationInfo.CSI_Quantity3, moveInDestinations.MoveInVolumn);
				AssertEquals("MarksAndNumbers", moveInDestinationInfo.CSI_Description, moveInDestinations.MarksAndNumbers);

				AssertEquals("Inventories.Count", 1, moveInDestinations.Inventories.Count());
				var inventoryProvider = moveInDestinations.Inventories.First();
				AssertEquals("InventoryManagementNumber", inventory.CSI_Code, inventoryProvider.InventoryManagementNumber);
				AssertEquals("MoveInQuantity", inventory.CSI_Quantity, inventoryProvider.MoveInQuantity);

				declaration.JE_RadioCallSign = "9999";
				AssertEquals("Vessel Code", declaration.JE_RadioCallSign, vesselProvider.Code);
				AssertEquals("Vessel Name", declaration.JE_VesselName, vesselProvider.Name);
			});
		}

		public void TestEAC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ValueType = ValueTypeList.Codes.S;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.EntryNumber = "123";
			var sendingObject = new MessageSendingObject(entryHeader);
			sendingObject.DeclarationCorrectionCopyRequest = true;
			var provider = new CusEntryHeaderMessageProvider(sendingObject);
			var providerEAC = new CusEntryHeaderMessageProvider(sendingObject) as IEAC;

			CombineAssertions(() => {
				AssertEquals("Declaration Number", entryHeader.EntryNumber, provider.DeclarationNumber);
				AssertEquals("Declaration Condition", "P", providerEAC.DeclarationCondition);
				AssertEquals("Reserved", ZString.Empty, providerEAC.Reserved);
			});

			sendingObject.DeclarationCorrectionCopyRequest = false;
			AssertEquals("Declaration Condition", ZString.Empty, providerEAC.DeclarationCondition);
		}
	}
}
