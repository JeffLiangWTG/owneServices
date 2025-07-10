using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GenerateImportLicenseObject))]
	class GenerateImportLicenseObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var date = ZDateTime.Now;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", date);

			var importLicenseObject = new GenerateImportLicenseObject(entryLine);
			importLicenseObject.ImportLicenseDeclarationPK = declaration.PK;
			AssertEquals("ImportLicenseDeclarationPKForGenerate should be equal", declaration.PK, importLicenseObject.ImportLicenseDeclaration.PK);
		}

		public void TestPossibleImportLicenseDeclarationForGenerate_List()
		{
			var date = ZDateTime.Now;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", date);

			var importLicenseObject = new GenerateImportLicenseObject(entryLine);
			AssertType<GenerateLicenseJobDeclarationModuleCollection>(importLicenseObject.PossibleImportLicenseDeclarationForGenerate_List);
		}

		public void TestGenerateImportLicense()
		{
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.Address1 = "Manufacture Address";

			var date = ZDateTime.Now;
			var dateDiff = ZDateTime.Now.AddDays(-1);
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var iswDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			iswDeclaration.JE_DeclarationReference = "ISW_DEC";
			var iswInstruction = iswDeclaration.CustomsEntryInstructions.AddNew();
			iswInstruction.CEI_Description = "ISW_TEST";
			var iswInvHeader = iswDeclaration.Invoices.AddNew();
			iswInvHeader.JZ_OH_Supplier = supplier.PK;
			iswInvHeader.JZ_OH_Buyer = buyer.PK;
			iswInvHeader.JZ_InvoiceDate = date;
			iswInvHeader.JZ_ValuationDateOverride = dateDiff;
			iswInvHeader.JZ_ClusterKey = 1;
			iswInvHeader.JZ_CU_RelatedHouseBill = new ZGuid();
			iswInvHeader.JZ_StandAloneInvoiceDirection = "IMP";
			iswInvHeader.ExchangeHedgeType = ExchangeHedgeList.Codes._1;

			var iswInvLine1 = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine1.JI_CEI = iswInstruction.PK;
			iswInvLine1.JI_PartNo = "PROD_DEMO";
			iswInvLine1.JI_Tariff = "11111111";
			iswInvLine1.DutyTaxRegime = "2";
			iswInvLine1.DutyLegalBase = "1";
			iswInvLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			iswInvLine1.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			iswInvLine1.NaladiHs = "01234567";
			iswInvLine1.JI_LinePrice = 100m;
			iswInvLine1.JI_Weight = 50M;
			iswInvLine1.JI_WeightUQ = "KG";
			iswInvLine1.JI_NetWeight = 50M;
			iswInvLine1.JI_NetWeightUQ = "KG";

			var iswInvLine2 = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine2.JI_CEI = iswInstruction.PK;
			iswInvLine2.JI_PartNo = "PROD_DEMO";
			iswInvLine2.JI_Tariff = "11111111";
			iswInvLine2.DutyTaxRegime = "2";
			iswInvLine2.DutyLegalBase = "1";
			iswInvLine2.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			iswInvLine2.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			iswInvLine2.NaladiHs = "01234567";
			iswInvLine2.JI_LinePrice = 200m;
			iswInvLine2.JI_Weight = 100M;
			iswInvLine2.JI_WeightUQ = "KG";
			iswInvLine2.JI_NetWeight = 100M;
			iswInvLine2.JI_NetWeightUQ = "KG";

			var tariffDetach1 = iswInvLine1.TariffDetachs.AddNew();
			tariffDetach1.CY_Code = "000";
			var tariffDetach2 = iswInvLine1.TariffDetachs.AddNew();
			tariffDetach2.CY_Code = "222";

			iswInvLine1.NVECusCodeDataCollection.GetFirstElementHaving("BA").CY_Data = "1111";
			iswInvLine1.NVECusCodeDataCollection.GetFirstElementHaving("BB").CY_Data = "9999";

			var additionalTariff = iswInvLine1.AdditionalTariffs.AddNew();
			additionalTariff.TariffCode = "111111_001";
			additionalTariff.ExNumber = "001";
			additionalTariff.TariffType = "XXXX";
			additionalTariff.LegalActType = "XX";
			additionalTariff.LegalActIssuingBody = "X";
			additionalTariff.LegalActNumber = "123";
			additionalTariff.LegalActYear = "2021";
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;

			var iswEntryHeader = iswDeclaration.ActiveEntryHeaders.AddNew();
			iswEntryHeader.CH_JE = iswDeclaration.PK;
			iswEntryHeader.CH_BGMReference = "ISW_DEC-100";
			iswEntryHeader.CH_CEI_Instruction = iswInstruction.PK;
			var iswEntryLine = iswEntryHeader.MergedLines.AddNew();

			iswInvLine1.JI_CL = iswEntryLine.PK;
			iswInvLine1.JI_CO = new ZGuid();
			iswInvLine1.JI_JO = new ZGuid();
			iswInvLine1.JI_ClusterKey = 1;
			iswInvLine1.JI_MatchingKey = "A";
			iswInvLine1.JI_PrimaryPreference = "A";
			iswInvLine1.JI_ValuationCode = "A";
			iswInvLine1.JI_BrandName = "A";
			iswInvLine1.JI_Model = "A";
			iswInvLine1.JI_SerialNumber = "A";

			iswInvLine2.JI_CL = iswEntryLine.PK;
			iswInvLine2.JI_CO = new ZGuid();
			iswInvLine2.JI_JO = new ZGuid();
			iswInvLine2.JI_ClusterKey = 1;
			iswInvLine2.JI_MatchingKey = "B";
			iswInvLine2.JI_PrimaryPreference = "B";
			iswInvLine2.JI_ValuationCode = "B";
			iswInvLine2.JI_BrandName = "B";
			iswInvLine2.JI_Model = "B";
			iswInvLine2.JI_SerialNumber = "B";

			iswEntryLine.CL_LineNumber = 1;

			iswInvLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.OtherAdditionsCustomsValue, 10m, Core.Constants.CurrencyCodes.UnitedStates);

			var chargeNotIncludedInInvoice = iswInvLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.EngineeringProjects, 10m, Core.Constants.CurrencyCodes.UnitedStates);
			chargeNotIncludedInInvoice.J7_IsNotIncludedInInvoice = false;

			iswInvLine1.ApportionedCharges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 15m, Core.Constants.CurrencyCodes.UnitedStates);
			iswInvLine1.ApportionedCharges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 40m, Core.Constants.CurrencyCodes.UnitedStates);
			iswInvLine1.ApportionedCharges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 80m, Core.Constants.CurrencyCodes.UnitedStates);

			iswInvLine2.ApportionedCharges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, 25m, Core.Constants.CurrencyCodes.UnitedStates);
			iswInvLine2.ApportionedCharges.AddNew(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, 12m, Core.Constants.CurrencyCodes.UnitedStates);

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC_DEC";

			var generateIL = new GenerateImportLicenseObject(iswEntryLine);
			generateIL.ImportLicenseDeclarationPK = licDeclaration.PK;

			CombineAssertions(() =>
			{
				Assert("GenerateImportLicense should be TRUE", generateIL.GenerateImportLicense());
				AssertEquals("Must contain 1 entry instruction", 1, licDeclaration.CustomsEntryInstructions.Count);
				AssertEquals("Must contain 1 invoice", 1, licDeclaration.Invoices.Count);
				AssertEquals("Must contain 1 invoice line", 2, licDeclaration.InvoiceLines.Count);

				var licInstruction = licDeclaration.CustomsEntryInstructions[0];
				AssertEquals("CEI_Description should be cloned", "ISW_DEC-1", licInstruction.CEI_Description);
				AssertEquals("CEI_JE should be cloned", licDeclaration.PK, licInstruction.CEI_JE);

				var licInvHeader = licDeclaration.Invoices[0];
				AssertEquals("JZ_JE", licDeclaration.PK, licInvHeader.JZ_JE);
				AssertEquals("JZ_JZ_GroupInvoiceFK", licDeclaration.TopGroupInvoice.PK, licInvHeader.JZ_JZ_GroupInvoiceFK);
				AssertNotEquals("JZ_ClusterKey NOT be cloned", iswInvHeader.JZ_ClusterKey, licInvHeader.JZ_ClusterKey);
				AssertEquals("JZ_CU_RelatedHouseBill NOT be cloned", ZGuid.Empty, licInvHeader.JZ_CU_RelatedHouseBill);
				AssertEquals("JZ_OH_Supplier should be cloned", supplier.PK, licInvHeader.JZ_OH_Supplier);
				AssertEquals("JZ_OH_Buyer should NOT be cloned", ZGuid.Empty, licInvHeader.JZ_OH_Buyer);
				AssertEquals("JZ_ValuationDateOverride should NOT be cloned", ZDateTime.Empty, licInvHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_StandAloneInvoiceDirection should NOT be cloned", ZString.Empty, licInvHeader.JZ_StandAloneInvoiceDirection);
				AssertEquals("JZ_Weight should be", 150m, licInvHeader.JZ_Weight);
				AssertEquals("JZ_NetWeight should be", 150m, licInvHeader.JZ_NetWeight);
				AssertEquals("JZ_InvoiceAmount should be", 300m, licInvHeader.JZ_InvoiceAmount);
				AssertEquals("ExchangeHedgeCollection should be", 1, licInvHeader.ExchangeHedgeCollection.Count);

				var licInvLine = licDeclaration.InvoiceLines[0];
				AssertEquals("JI_PartNo should be cloned", "PROD_DEMO", licInvLine.JI_PartNo);
				AssertEquals("JI_Tariff should be cloned", "11111111", licInvLine.JI_Tariff);

				AssertEquals("DutyTaxRegime should be cloned", "2", licInvLine.DutyTaxRegime);
				AssertEquals("DutyLegalBase should be cloned", "1", licInvLine.DutyLegalBase);
				AssertEquals("JI_ManufacturerIndicator should be cloned", ManufacturerIndicatorList.Codes._2, licInvLine.JI_ManufacturerIndicator);
				AssertEquals("ManufacturerDocAddressPK should be cloned", manufacturerAddress.PK, licInvLine.ManufacturerDocAddressPK);
				AssertEquals("JI_SecondaryPreference should be cloned", "XXXX", licInvLine.JI_SecondaryPreference);
				AssertEquals("JI_JZ should be equal to", licInvHeader.PK, licInvLine.JI_JZ);
				AssertEquals("JI_CEI should be equal to", licInstruction.PK, licInvLine.JI_CEI);
				AssertEquals("LIC JI_ParentTableCode should be set", "JI", licInvLine.JI_ParentTableCode);
				AssertEquals("LIC JI_ParentID should be set", iswInvLine1.PK, licInvLine.JI_ParentID);
				AssertEquals("ISW JI_ParentTableCode should be set", "JI", iswInvLine1.JI_ParentTableCode);
				AssertEquals("ISW JI_ParentID should be set", licInvLine.PK, iswInvLine1.JI_ParentID);
				AssertEquals("JI_CL should NOT be cloned", ZGuid.Empty, licInvLine.JI_CL);
				AssertEquals("JI_CO should NOT be cloned", ZGuid.Empty, licInvLine.JI_CO);
				AssertEquals("JI_JO should NOT be cloned", ZGuid.Empty, licInvLine.JI_JO);
				AssertNotEquals("JI_ClusterKey should NOT be cloned", iswInvLine1.JI_ClusterKey, licInvLine.JI_ClusterKey);
				AssertNotEquals("JI_MatchingKey should NOT be cloned", iswInvLine1.JI_MatchingKey, licInvLine.JI_MatchingKey);
				AssertEquals("JI_PrimaryPreference should NOT be cloned", ZString.Empty, licInvLine.JI_PrimaryPreference);
				AssertEquals("JI_ValuationCode should NOT be cloned", ZString.Empty, licInvLine.JI_ValuationCode);
				AssertEquals("JI_BrandName should NOT be cloned", ZString.Empty, licInvLine.JI_BrandName);
				AssertEquals("JI_Model should NOT be cloned", ZString.Empty, licInvLine.JI_Model);
				AssertEquals("JI_SerialNumber should NOT be cloned", ZString.Empty, licInvLine.JI_SerialNumber);
				AssertEquals("NaladiHs should be cloned", "01234567", licInvLine.NaladiHs);

				AssertEquals("Charges should be cloned", 1, licInvLine.Charges.Count);

				AssertEquals("J7_ChargeType should be", chargeNotIncludedInInvoice.J7_ChargeType, licInvLine.Charges[0].J7_ChargeType);
				AssertEquals("J7_Amount should be", chargeNotIncludedInInvoice.J7_Amount, licInvLine.Charges[0].J7_Amount);
				AssertEquals("J7_DistributeBy should be", chargeNotIncludedInInvoice.J7_DistributeBy, licInvLine.Charges[0].J7_DistributeBy);
				AssertEquals("J7_IsNotIncludedInInvoice should be", chargeNotIncludedInInvoice.J7_IsNotIncludedInInvoice, licInvLine.Charges[0].J7_IsNotIncludedInInvoice);
				AssertEquals("J7_RX_NKCurrency should be", chargeNotIncludedInInvoice.J7_RX_NKCurrency, licInvLine.Charges[0].J7_RX_NKCurrency);

				var apportionatedOFCCharge = licDeclaration.Invoices[0].Charges.GetCharge(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect).Single();
				AssertEquals("J7_Amount should be", 67m, apportionatedOFCCharge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency should be", Core.Constants.CurrencyCodes.UnitedStates, apportionatedOFCCharge.J7_RX_NKCurrency);

				var apportionatedOFPCharge = licDeclaration.Invoices[0].Charges.GetCharge(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid).Single();
				AssertEquals("J7_Amount should be", 105m, apportionatedOFPCharge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency should be", Core.Constants.CurrencyCodes.UnitedStates, apportionatedOFPCharge.J7_RX_NKCurrency);

				AssertEquals("TariffDetachCollection should be cloned", 2, licInvLine.TariffDetachs.Count);
				AssertEquals("TariffDetachCollection should be cloned", "000,222", licInvLine.TariffDetachConcatenated);

				AssertEquals("NVECusCodeDataCollection should be cloned", 2, licInvLine.NVECusCodeDataCollection.Count);
				AssertEquals("NVECusCodeDataCollection[0].Specification should be cloned", "1111", licInvLine.NVECusCodeDataCollection.GetFirstElementHaving("BA").CY_Data);
				AssertEquals("NVECusCodeDataCollection[1].Specification should be cloned", "9999", licInvLine.NVECusCodeDataCollection.GetFirstElementHaving("BB").CY_Data);

				var pivot = iswDeclaration.AttachedImportLicenseEntries.Cast<DeclarationRelatedImportLicenseEntryGenPivot>().FirstOrDefault();

				AssertEquals("AttachedImportLicenseEntries should be created", 1, iswDeclaration.AttachedImportLicenseEntries.Count);
				AssertEquals("Relation1ID should be iswDeclaration.PK", iswDeclaration.PK, pivot.Relation1ID);
				AssertEquals("Relation2ID should be licInstruction.PK", licInstruction.PK, pivot.Relation2ID);
			});

			licDeclaration.InvoiceLines.RemoveAndDeleteAll();
			licDeclaration.Invoices.RemoveAll();

			iswInvLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			iswInvLine1.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			Assert("GenerateImportLicense should be TRUE", generateIL.GenerateImportLicense());
			var licInvLine1 = licDeclaration.InvoiceLines[0];
			AssertEquals("ManufacturerDocAddressPK should NOT be cloned when ManufacturerIndicator <> 2", manufacturerAddress.PK, licInvLine1.ManufacturerDocAddressPK);

			generateIL.ImportLicenseDeclarationPK = ZGuid.Empty;
			Assert("GenerateImportLicense should be FALSE", !generateIL.GenerateImportLicense());
		}

		public void TestCanGenerateImportLicense()
		{
			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC_DEC";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00000001-1";

			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CH = entryHeader.PK;

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine.PK;
			invLine1.ImportLicenseNumber = "123456";

			var generateLIC = new GenerateImportLicenseObject(entryLine);
			AssertEquals("There is already an Import License registered from this Entry Line", generateLIC.CanGenerateImportLicense());

			invLine1.ImportLicenseNumber = ZString.Empty;
			entryHeader.MovementReferenceNumberSetter("TST1");

			AssertEquals("Entry Header is already registered", generateLIC.CanGenerateImportLicense());

			entryHeader.MovementReferenceNumberSetter(ZString.Empty);
			AssertEquals(ZString.Empty, generateLIC.CanGenerateImportLicense());

			generateLIC.ImportLicenseDeclarationPK = licDeclaration.PK;
			generateLIC.GenerateImportLicense();

			AssertEquals("There is License Job created from this Entry Line. Please, detach License and repeat the operation.", generateLIC.CanGenerateImportLicense());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusEntryLine = Factory.NewWithValidTestData<CusEntryLine>();
			return new GenerateImportLicenseObject(cusEntryLine);
		}
	}
}
