using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
{
	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
	{
		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(Factory.New<CusEntryInstruction>(), "CHCusEntryInstruction");
	}

	public void TestSupportsClone() => Assert(EntryInstruction.SupportsClone());

	public void TestCEI_Style_Import()
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals(2, EntryInstruction.CEI_StyleInfo.MaxLength);
	}

	public void TestCEI_SubStyle_Import()
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals(2, EntryInstruction.CEI_SubStyleInfo.MaxLength);
	}

	public void TestCEI_DateForDuty() => CombineAssertions(() =>
	{
		AssertEquals("Caption", "Assessment Date", DataBoundResourceStrings.GetDataForProperty(EntryInstruction.CEI_DateForDutyInfo).Caption);
		AssertEquals("ReadOnly", true, EntryInstruction.CEI_DeclarationReasonInfo.ReadOnly);
	});

	public void TestCEH_DeclarationReason_Import() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		AssertEquals(2, EntryInstruction.CEI_DeclarationReasonInfo.MaxLength);
		Assert(EntryInstruction.CEI_DeclarationReasonInfo.ReadOnly);

		EntryInstruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Provisional;
		Assert(!EntryInstruction.CEI_DeclarationReasonInfo.ReadOnly);
	});

	public void TestCEH_DeclarationReason_Clearing() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		EntryInstruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Provisional;
		EntryInstruction.CEI_DeclarationReason = "01";
		EntryInstruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Definitive;
		AssertEquals("Declaration reason cleared", ZString.Empty, EntryInstruction.CEI_DeclarationReason);
		Assert(EntryInstruction.CEI_DeclarationReasonInfo.ReadOnly);
	});

	public void TestAllInvoiceLinesAreReturnedGoods() => CombineAssertions(() =>
	{
		AssertEquals("AllInvoiceLinesAreReturnedGoods", false, EntryInstruction.AllInvoiceLinesAreReturnedGoods);
		EntryInstruction.JobDeclaration.InvoiceLines.AddNew().JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods;
		AssertEquals("AllInvoiceLinesAreReturnedGoods", true, EntryInstruction.AllInvoiceLinesAreReturnedGoods);
		var invoiceLine = EntryInstruction.JobDeclaration.InvoiceLines.AddNew();
		AssertEquals("AllInvoiceLinesAreReturnedGoods", false, EntryInstruction.AllInvoiceLinesAreReturnedGoods);
		invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoodsVAT;
		AssertEquals("AllInvoiceLinesAreReturnedGoods", false, EntryInstruction.AllInvoiceLinesAreReturnedGoods);
	});

	public void TestHasPreferentialTariffPreference() => CombineAssertions(() =>
	{
		AssertEquals("HasPreferentialTariffPreference init", false, EntryInstruction.HasPreferentialTariffPreference);

		EntryInstruction.JobDeclaration.InvoiceLines.AddNew().JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods;
		var invoiceHeader = EntryInstruction.JobDeclaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = EntryInstruction.PK;
		invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = EntryInstruction.PK;
		invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
		EntryInstruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Provisional;

		AssertEquals("HasPreferentialTariffPreference false", false, EntryInstruction.HasPreferentialTariffPreference);

		invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		AssertEquals("HasPreferentialTariffPreference true", true, EntryInstruction.HasPreferentialTariffPreference);
	});

	public void TestDefaultValues() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		var instruction = EntryInstruction.JobDeclaration.CustomsEntryInstructions.AddNew();
		AssertEquals($"CEI Style default value for EXP", UniversalReferenceConstants.InputControlCodes.Ordinary, instruction.CEI_Style);
		AssertEquals($"CEI SubStyle default value for EXP", ZString.Empty, instruction.CEI_SubStyle);

		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		instruction = EntryInstruction.JobDeclaration.CustomsEntryInstructions.AddNew();
		AssertEquals($"CEI Style default value for EDA", UniversalReferenceConstants.InputControlCodes.Ordinary, instruction.CEI_Style);
		AssertEquals($"CEI SubStyle default value for EDA", UniversalReferenceConstants.DeclarationTimeCodes.PresentationToCustoms, instruction.CEI_SubStyle);

		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		instruction = EntryInstruction.JobDeclaration.CustomsEntryInstructions.AddNew();
		AssertEquals($"CEI Style default value for IMP", ZString.Empty, instruction.CEI_Style);
		AssertEquals($"CEI SubStyle default value for IMP", ZString.Empty, instruction.CEI_SubStyle);
	});

	public void TestCEI_Description_Import_Default() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		RefCusCodeTestHelper.CreateEnstyCodeList(Factory);
		RefCusCodeTestHelper.CreateEnsubCodeList(Factory);

		EntryInstruction.CEI_Description = ZString.Empty;
		EntryInstruction.CEI_Style = RefCusCodeTestHelper.ValidStyleListCode;
		EntryInstruction.CEI_SubStyle = RefCusCodeTestHelper.ValidEntrySubStyleListCode;
		AssertEquals($"CEI Description default value for IMP", "Definitiv - Gestellung", EntryInstruction.CEI_Description);

		EntryInstruction.CEI_Description = "Existing Description";
		EntryInstruction.CEI_Style = RefCusCodeTestHelper.ValidStyleListCode;
		EntryInstruction.CEI_SubStyle = RefCusCodeTestHelper.ValidEntrySubStyleListCode;
		AssertEquals($"CEI Description not changed for IMP", "Existing Description", EntryInstruction.CEI_Description);
	});

	public void TestCEI_Description_Export_Default() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInputControlList(Factory);
		RefCusCodeTestHelper.CreateProcedureList(Factory);
		EntryInstruction.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		EntryInstruction.CEI_Procedure = ZString.Empty;
		EntryInstruction.CEI_Style = ZString.Empty;
		EntryInstruction.CEI_Description = ZString.Empty;
		AssertEquals(AssertionMessage("All empty"), ZString.Empty, EntryInstruction.CEI_Description);

		EntryInstruction.CEI_Procedure = ZString.Empty;
		EntryInstruction.CEI_Style = RefCusCodeTestHelper.CEIStyleDefaultValuenEXP;
		EntryInstruction.CEI_Description = ZString.Empty;
		AssertEquals(AssertionMessage("No Procedure"), ZString.Empty, EntryInstruction.CEI_Description);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		EntryInstruction.CEI_Style = ZString.Empty;
		EntryInstruction.CEI_Description = ZString.Empty;
		AssertEquals(AssertionMessage("No Declaration Type"), ZString.Empty, EntryInstruction.CEI_Description);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		EntryInstruction.CEI_Style = RefCusCodeTestHelper.CEIStyleDefaultValuenEXP;
		EntryInstruction.CEI_Description = ZString.Empty;
		AssertEquals(AssertionMessage("Procedure + Declaration Type"), $"{RefCusCodeTestHelper.ProcedureCodeDefaultValueExp} - {RefCusCodeTestHelper.CEIStyleDefaultDescriptionEXP}", EntryInstruction.CEI_Description);

		EntryInstruction.CEI_Procedure = ZString.Empty;
		EntryInstruction.CEI_Style = ZString.Empty;
		EntryInstruction.CEI_Description = ZString.Empty;
		EntryInstruction.CEI_Style = RefCusCodeTestHelper.CEIStyleDefaultValuenEXP;
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		AssertEquals(AssertionMessage("Triggered by Procedure"), $"{RefCusCodeTestHelper.ProcedureCode41Exp} - {RefCusCodeTestHelper.CEIStyleDefaultDescriptionEXP}", EntryInstruction.CEI_Description);
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode50Exp;
		AssertEquals(AssertionMessage("Triggered by Procedure / not changed if not empty"), $"{RefCusCodeTestHelper.ProcedureCode41Exp} - {RefCusCodeTestHelper.CEIStyleDefaultDescriptionEXP}", EntryInstruction.CEI_Description);

		EntryInstruction.CEI_Procedure = ZString.Empty;
		EntryInstruction.CEI_Style = ZString.Empty;
		EntryInstruction.CEI_Description = ZString.Empty;
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		EntryInstruction.CEI_Style = RefCusCodeTestHelper.CEIStyle1EXP;
		AssertEquals(AssertionMessage("Triggered by Declaration Type"), $"{RefCusCodeTestHelper.ProcedureCode41Exp} - {RefCusCodeTestHelper.CEIStyle1DescriptionEXP}", EntryInstruction.CEI_Description);
		EntryInstruction.CEI_Style = RefCusCodeTestHelper.CEIStyleDefaultValuenEXP;
		AssertEquals(AssertionMessage("Triggered by Declaration Type / not changed of not empty"), $"{RefCusCodeTestHelper.ProcedureCode41Exp} - {RefCusCodeTestHelper.CEIStyle1DescriptionEXP}", EntryInstruction.CEI_Description);

		string AssertionMessage(string info) => $"CEI_Procedure={EntryInstruction.CEI_NextProcedure} CEI_Style={EntryInstruction.CEI_Style} - {info}";
	});

	public void TestCEI_OA_Warehouse_ReadOnly() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Refund;
		Assert(EntryInstruction.CEI_OA_WarehouseInfo.ReadOnly);
		EntryInstruction.CEI_WarehouseType = UniversalReferenceConstants.WarehouseTypeCodes.BondedWarehouse;
		Assert(!EntryInstruction.CEI_OA_WarehouseInfo.ReadOnly);
	});

	public void TestCEI_OA_Warehouse_Clearing() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Refund;
		EntryInstruction.CEI_WarehouseType = UniversalReferenceConstants.WarehouseTypeCodes.BondedWarehouse;

		var orgHeader = Factory.New<OrgHeader>();
		EntryInstruction.CEI_OA_Warehouse_ZAddress.OrgPK = orgHeader.PK;
		EntryInstruction.CEI_WarehouseType = UniversalReferenceConstants.WarehouseTypeCodes.InterimStorageAbroad;
		AssertEquals("Warehouse cleared", ZGuid.Empty, EntryInstruction.CEI_OA_Warehouse);
		Assert(EntryInstruction.CEI_OA_WarehouseInfo.ReadOnly);
	});

	public void TestCEI_OH_Owner_ReadOnly() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Refund;
		Assert(EntryInstruction.CEI_OH_OwnerInfo.ReadOnly);
		EntryInstruction.CEI_WarehouseType = UniversalReferenceConstants.WarehouseTypeCodes.BondedWarehouse;
		Assert(!EntryInstruction.CEI_OH_OwnerInfo.ReadOnly);
	});

	public void TestCEI_OH_Owner_Clearing() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Refund;
		EntryInstruction.CEI_WarehouseType = UniversalReferenceConstants.WarehouseTypeCodes.BondedWarehouse;
		EntryInstruction.CEI_OH_Owner = Factory.New<OrgAddress>().PK;
		EntryInstruction.CEI_WarehouseType = UniversalReferenceConstants.WarehouseTypeCodes.InterimStorageAbroad;
		AssertEquals("Warehouse cleared", ZGuid.Empty, EntryInstruction.CEI_OH_Owner);
		Assert(EntryInstruction.CEI_OH_OwnerInfo.ReadOnly);
	});

	public void TestCEI_ProcedureCaption()
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		CaptionTestHelper.AssertCaptions(EntryInstruction.CEI_ProcedureInfo, caption: "Procedure Code");
	}

	public void TestCEI_ProcedureOnSaving() => CombineAssertions(() =>
	{
		var invoice = EntryInstruction.JobDeclaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.JobDeclaration.InvoiceLines.AddNew();
		string val = "1";
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		EntryInstruction.CEI_Procedure = val;
		Factory.Save();
		AssertNotEquals("Assert value don't change after saving on import", val, invoiceLine1.JI_Procedure);
		AssertNotEquals("Assert value don't change after saving on import", val, invoiceLine2.JI_Procedure);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		Factory.Save();
		AssertEquals("Assert value change after saving on Export", val, invoiceLine1.JI_Procedure);
		AssertEquals("Assert value change after saving on Export", val, invoiceLine2.JI_Procedure);
	});

	public void TestSupplyChainActors() => CombineAssertions(() =>
	{
		AssertType<CusSupplyChainActorReferenceCollection>("CusSupplyChainActorCollection", EntryInstruction.SupplyChainActors);
		AssertSame("cached", EntryInstruction.SupplyChainActors, EntryInstruction.SupplyChainActors);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var supplyChainActor = AddSupplyChainActor();
		Factory.Save();
		AssertEquals("SupplyChainActors not saved for IMP job", false, supplyChainActor.IsInDatabase);
		AssertEquals("SupplyChainActors deleted for IMP job", true, supplyChainActor.IsDeleted);
		AssertEquals("SupplyChainActors removed for IMP job", 0, EntryInstruction.SupplyChainActors.Count);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		supplyChainActor = AddSupplyChainActor();
		Factory.Save();
		AssertEquals("SupplyChainActors saved for EXP job", true, supplyChainActor.IsInDatabase);
		AssertEquals("SupplyChainActors not deleted for EXP job", false, supplyChainActor.IsDeleted);
		AssertEquals("SupplyChainActors not removed for EXP job", 1, EntryInstruction.SupplyChainActors.Count);

		EntryInstruction.Delete();
		AssertEquals("SupplyChainActors deleted when Entry Instruction deleted", true, supplyChainActor.IsDeleted);

		CusSupplyChainActorReference AddSupplyChainActor()
		{
			var supplyChainActor = EntryInstruction.SupplyChainActors.AddNew();
			supplyChainActor.CFR_Code = "MF";
			supplyChainActor.CFR_Reference = "X";
			return supplyChainActor;
		}
	});

	public void TestPreviousDocuments() => CombineAssertions(() =>
	{
		AssertType<PreviousDocumentCollection>("PreviousDocumentCollection", EntryInstruction.PreviousDocuments);
		AssertSame("cached", EntryInstruction.PreviousDocuments, EntryInstruction.PreviousDocuments);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var previousDocument = EntryInstruction.PreviousDocuments.AddNew();
		Factory.Save();

		AssertEquals("PreviousDocuments not saved for IMP job", false, previousDocument.IsInDatabase);
		AssertEquals("PreviousDocuments deleted for IMP job", true, previousDocument.IsDeleted);
		AssertEquals("PreviousDocuments removed for IMP job", 0, EntryInstruction.PreviousDocuments.Count);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		previousDocument = EntryInstruction.PreviousDocuments.AddNew();
		Factory.Save();

		AssertEquals("PreviousDocuments not saved for EXP job", true, previousDocument.IsInDatabase);
		AssertEquals("PreviousDocuments deleted for EXP job", false, previousDocument.IsDeleted);
		AssertEquals("PreviousDocuments removed for EXP job", 1, EntryInstruction.PreviousDocuments.Count);

		EntryInstruction.Delete();
		AssertEquals("PreviousDocuments deleted when Entry Instruction deleted", true, previousDocument.IsDeleted);
	});

	public void TestSupportingDocuments() => CombineAssertions(() =>
	{
		AssertType<SupportingDocumentCollection>("SupportingDocumentCollection", EntryInstruction.SupportingDocuments);
		AssertSame("cached", EntryInstruction.SupportingDocuments, EntryInstruction.SupportingDocuments);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var supportingDocument = AddSupportingDocument();
		Factory.Save();
		AssertEquals("SupportingDocuments not saved for IMP job", false, supportingDocument.IsInDatabase);
		AssertEquals("SupportingDocuments deleted for IMP job", true, supportingDocument.IsDeleted);
		AssertEquals("SupportingDocuments not removed for EXP job", 0, EntryInstruction.SupportingDocuments.Count);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		supportingDocument = AddSupportingDocument();
		Factory.Save();
		AssertEquals("SupportingDocuments saved for EXP job", true, supportingDocument.IsInDatabase);
		AssertEquals("SupportingDocuments not deleted for EXP job", false, supportingDocument.IsDeleted);
		AssertEquals("SupportingDocuments not removed for EXP job", 1, EntryInstruction.SupportingDocuments.Count);

		EntryInstruction.Delete();
		AssertEquals("supportingDocuments deleted when Entry Instruction deleted", true, supportingDocument.IsDeleted);

		SupportingDocument AddSupportingDocument()
		{
			var supportingDocument = EntryInstruction.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "337";
			supportingDocument.CSI_ReferenceNumber = "1";
			return supportingDocument;
		}
	});

	public void TestAdditionalInformation() => CombineAssertions(() =>
	{
		AssertType<AdditionalInformationCollection>("AdditionalInformations", EntryInstruction.AdditionalInformations);
		AssertSame("cached", EntryInstruction.AdditionalInformations, EntryInstruction.AdditionalInformations);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var additionalInformation = CreateAdditionalInformation();
		Factory.Save();
		AssertEquals("AdditionalInformations not saved for IMP job", false, additionalInformation.IsInDatabase);
		AssertEquals("AdditionalInformations deleted for IMP job", true, additionalInformation.IsDeleted);
		AssertEquals("AdditionalInformations removed for IMP job", 0, EntryInstruction.AdditionalInformations.Count);

		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		additionalInformation = CreateAdditionalInformation();
		Factory.Save();
		AssertEquals("AdditionalInformations saved for EXP job", true, additionalInformation.IsInDatabase);
		AssertEquals("AdditionalInformations not deleted for EXP job", false, additionalInformation.IsDeleted);
		AssertEquals("AdditionalInformations not removed for EXP job", 1, EntryInstruction.AdditionalInformations.Count);

		EntryInstruction.Delete();
		AssertEquals("AdditionalInformations deleted when Entry Instruction deleted", true, additionalInformation.IsDeleted);

		AdditionalInformation CreateAdditionalInformation()
		{
			var additionalInformation = EntryInstruction.AdditionalInformations.AddNew();
			additionalInformation.CSI_Code = "CD";
			additionalInformation.CSI_Description = "Description of CD";
			return additionalInformation;
		}
	});

	public void TestTransportDocuments() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertType<TransportDocument>(EntryInstruction.TransportDocuments.AddNew());

		Factory.Save();
		AssertEquals("TransportDocument saved/loaded for EXP", 1, new BusinessObjectFactory().Load<CusEntryInstruction>(EntryInstruction.PK).TransportDocuments.Count);

		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Factory.Save();
		AssertEquals("TransportDocuments deleted for EDA", 1, EntryInstruction.TransportDocuments.Count);

		AssertType<TransportDocument>(EntryInstruction.TransportDocuments.AddNew());
		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Factory.Save();
		AssertEquals("TransportDocuments deleted for IMP", 0, EntryInstruction.TransportDocuments.Count);
	});

	public void TestIsGSPCertificateRequired() => AssertEquals("IsGSPCertificateRequired false", false, EntryInstruction.IsGSPCertificateRequired);

	public void TestCEI_TransportChargesMethodOfPayment() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		CaptionTestHelper.AssertCaptions(EntryInstruction.CEI_TransportChargesMethodOfPaymentInfo, shortCaption: "Transport MoP", caption: "Method of Payment of Transport Charges");
		var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		AssertEquals(1, instruction.CEI_TransportChargesMethodOfPaymentInfo.MaxLength);
	});

	public void TestCEI_NextProcedure() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		CaptionTestHelper.AssertCaptions(EntryInstruction.CEI_NextProcedureInfo, caption: "Next Procedure", shortCaption: "Next");
		AssertEquals(1, EntryInstruction.CEI_NextProcedureInfo.MaxLength);
		var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		AssertEquals(1, instruction.CEI_NextProcedureInfo.MaxLength);
	});

	public void TestIsSimplified() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertEquals("CEI_Style=1", true, EntryInstruction.IsSimplified);
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("CEI_Style=2", false, EntryInstruction.IsSimplified);
	});

	public void TestIsOrdinary() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("CEI_Style=2", true, EntryInstruction.IsOrdinary);
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertEquals("CEI_Style=1", false, EntryInstruction.IsOrdinary);
	});

	public void TestTotalPriceInCHF()
	{
		var invoiceHeader = EntryInstruction.JobDeclaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = EntryInstruction.PK;
		invoiceLine1.JI_LinePrice = 100;
		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = EntryInstruction.PK;
		invoiceLine2.JI_LinePrice = 100;
		invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;

		AssertEquals("No currency sum = 0", 0m, EntryInstruction.TotalPriceInCHF);

		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;

		AssertEquals("Currency is set, TotalPriceInCHF work normally", 200m, EntryInstruction.TotalPriceInCHF);
	}

	public void TestTotalWeightInKG()
	{
		var invoiceHeader = EntryInstruction.JobDeclaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = EntryInstruction.PK;
		invoiceLine1.JI_Weight = 100;
		invoiceLine1.JI_WeightUQ = "KG";
		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = EntryInstruction.PK;
		invoiceLine2.JI_Weight = 100;
		invoiceLine2.JI_WeightUQ = "KG";

		AssertEquals("Currency is set, TotalPriceInCHF work normally", 200m, EntryInstruction.TotalWeightInKG);
	}

	public void TestIsProcessingTransaction() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateProcedureList(Factory);
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		AssertEquals("True for CEI_Procedure 41", true, EntryInstruction.IsProcessingTransaction);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode50Exp;
		AssertEquals("True for CEI_Procedure 50", true, EntryInstruction.IsProcessingTransaction);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		AssertEquals("False for CEI_Procedure not 41 OR 50", false, EntryInstruction.IsProcessingTransaction);
	});

	public void TestHasAdditionalInformationV1201() => CombineAssertions(() =>
	{
		AssertEquals("false if any V1201 Additional info present", false, EntryInstruction.HasAdditionalInformationV1201);

		var addinfo = EntryInstruction.AdditionalInformations.AddNew();
		addinfo.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.PartialShipmentNumber;
		AssertEquals("true if any V1201 Additional info present", true, EntryInstruction.HasAdditionalInformationV1201);
	});

	public void TestCEI_WarehouseType()
	{
		var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		AssertEquals(1, instruction.CEI_WarehouseTypeInfo.MaxLength);
	}

	public void TestCEI_DeclarationReason_ReadOnly()
	{
		CombineAssertions(() =>
		{
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			AssertEquals($"CEI_Style={instruction.CEI_Style}", true, instruction.CEI_DeclarationReasonInfo.ReadOnly);
			instruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Provisional;
			AssertEquals($"CEI_Style={instruction.CEI_Style}", false, instruction.CEI_DeclarationReasonInfo.ReadOnly);
			instruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Definitive;
			AssertEquals($"CEI_Style={instruction.CEI_Style}", true, instruction.CEI_DeclarationReasonInfo.ReadOnly);
		});
	}

	public void TestCEI_PartialDelivery()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		AssertEquals("Default value", false, instruction.CEI_PartialDelivery);
	}

	protected override BusinessObject GetNewBusinessObject() => EntryInstruction;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => EntryInstruction;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew();

	CusEntryInstruction EntryInstruction => entryInstruction ??= GetEntryInstruction();
	CusEntryInstruction entryInstruction;

	CusEntryInstruction GetEntryInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		return declaration.CustomsEntryInstructions.AddNew();
	}
}
