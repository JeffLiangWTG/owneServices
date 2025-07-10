using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobDeclarationValueSetStrategyTest : TestCaseWithFactory
	{
		public virtual void TestJE_MessageTypeChanged()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddInfo = EUOrgImpAddInfo.Get(supplier, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			supplierAddInfo.Deserialise();
			supplierAddInfo.ZO_Box14UseIndirectRepresentationForExporter = true;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerAddInfo = EUOrgImpAddInfo.Get(importer, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			importerAddInfo.Deserialise();
			importerAddInfo.ZO_Box14UseIndirectRepresentation = true;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(EU.Business.RepresentationTypeList.Codes._3Indirect, declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = "";
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(EU.Business.RepresentationTypeList.Codes._3Indirect, declaration.JE_DeclarantType);
		}

		public virtual void TestDefaultSupplierChanged()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();

			var addInfo = EUOrgImpAddInfo.Get(supplier, Core.Constants.CountryCodes.Latvia);
			addInfo.Deserialise();
			addInfo.ZO_Box14UseIndirectRepresentationForExporter = true;
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals(EU.Business.RepresentationTypeList.Codes._3Indirect, declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = "";
			addInfo.ZO_Box14UseIndirectRepresentationForExporter = false;
			Factory.Save();
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("", declaration.JE_DeclarantType);

			addInfo.ZO_Box14UseIndirectRepresentationForExporter = true;
			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("", declaration.JE_DeclarantType);
		}

		public void TestVATDeferStrategy()
		{
			var declaration = Factory.New<DummyJobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("100", declaration.JE_DefermentAccountNumber);

			declaration.JE_DefermentAccountNumber = "XXX";
			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;
			AssertEquals("100", declaration.JE_DefermentAccountNumber);

			declaration.JE_DefermentAccountNumber = "XXX";
			declaration.JE_PaymentMethod = "A";
			AssertEquals("200", declaration.JE_DefermentAccountNumber);

			declaration.JE_DefermentAccountNumber = "XXX";
			declaration.ZG_VATDeferType = "A";
			AssertEquals("300", declaration.JE_DefermentAccountNumber);
		}

		public void TestDefaultEntryInstructionWhenImporterChanged()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			GenerateProcedure("No", YesNoList.Codes.No, YesNoList.Codes.No, "No description", false, MessageTypeList.Codes.Import);
			GenerateProcedure("Ye", YesNoList.Codes.No, YesNoList.Codes.Yes, "Yes description", true, MessageTypeList.Codes.Import);
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceline = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			invoiceline.JI_Procedure = "Ye12367";
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = false", !invoiceline.HasAnyProcedureWithSuspendedVat);

			var invoiceline2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceline2.JI_Procedure = "No12367";
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = true", invoiceline2.HasAnyProcedureWithSuspendedVat);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryHeader entryheader = declaration.CustomsEntryHeaders[0];
			entryheader.CH_CEI_Instruction = entryInstruction.PK;
			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew().CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;

			Assert(!entryheader.IsAllEntryLinesVatSuspended);
			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, entryInstruction.FiscalReferences[0].CFR_Code);

			invoiceline.JI_Procedure = "No12367";
			Assert(entryheader.IsAllEntryLinesVatSuspended);
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(0, entryInstruction.FiscalReferences.Count);
			Factory.Save();

			entryInstruction.FiscalReferences.AddNew().CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			invoiceline.JI_Procedure = "Ye12367";
			Assert(!entryheader.IsAllEntryLinesVatSuspended);
			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, entryInstruction.FiscalReferences[0].CFR_Code);
		}

		RefCusProcedure GenerateProcedure(string procedureCode, string isGuaranteeConsumed, string isGuaranteeReleased, string description, ZBool isCalculeVAT, string messageType)
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = procedureCode;
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = messageType;
			procedure.ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed;
			procedure.ZZ6_IsGuaranteeReleased = isGuaranteeReleased;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";
			procedure.ZZ6_CalculateVAT = isCalculeVAT;
			return procedure;
		}

		class DummyJobDeclaration : JobDeclaration
		{
			public DummyJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override VATDeferStrategy GetVATDeferStrategyCore() => new DummyVATDeferStrategy(this);
		}

		class DummyVATDeferStrategy : VATDeferStrategy
		{
			public DummyVATDeferStrategy(JobDeclaration declaration) : base(declaration)
			{
			}

			public override void OnOrganisationChanged()
			{
				Declaration.JE_DefermentAccountNumber = "100";
			}

			public override void OnPaymentMethodChanged()
			{
				Declaration.JE_DefermentAccountNumber = "200";
			}

			public override void OnVATDeferTypeChanged()
			{
				Declaration.JE_DefermentAccountNumber = "300";
			}
		}
	}
}
