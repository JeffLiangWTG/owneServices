using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationPiggyBackedDocAddressValidationFactoryTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new JobDeclarationPiggyBackedDocAddressValidationFactory(null, null));
	}

	public void TestPiggyBackedDocAddressValidation()
	{
		var piggyBackFactory = new JobDeclarationPiggyBackedDocAddressValidationFactory(Factory.New<JobDeclaration>(), null);
		AssertExceptionThrown<ArgumentNullException>("Invoking PiggyBackedDocAddressValidation with null", () => piggyBackFactory.GetNewPiggyBackedDocAddressValidation(null));
	}

	public void TestImporterPiggyBackedDocAddressValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var organization = Factory.NewWithValidTestData<OrgHeader>();
		organization.OH_Code = "IM1";

		declaration.JE_OH_Importer = organization.PK;
		AssertEquals("Pre: for MessageType = IMP, IsUCC6", true, declaration.Configuration.IsUCC6(declaration));
		AssertType<RequiringEoriUcc6TraderJobDocAddressValidation>("Invoking PiggyBackedDocAddressValidation with ImporterDocumentaryAddress the return type when IsUCC6 = true", declaration.PiggyBackedDocAddressValidation(declaration.ImporterDocumentaryAddress));

		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertType<Ucc6ExportImporterJobDocAddressValidation>("Invoking PiggyBackedDocAddressValidation with ImporterDocumentaryAddress the return type when MessageType = EXP", declaration.PiggyBackedDocAddressValidation(declaration.ImporterDocumentaryAddress));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertType<TraderJobDocAddressValidation>("Invoking PiggyBackedDocAddressValidation with ImporterDocumentaryAddress the return type when MessageType = EXP", declaration.PiggyBackedDocAddressValidation(declaration.ImporterDocumentaryAddress));
		}
	}

	public void TestSupplierPiggyBackedDocAddressValidation()
	{
		new ITUniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure40And71ForCurrentCountry();

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		var organization = Factory.NewWithValidTestData<OrgHeader>();
		organization.OH_Code = "IM1";
		declaration.JE_OH_Supplier = organization.PK;

		declaration.JE_MessageType = "IMP";
		CombineAssertions("Assert PiggyBackedDocAddressValidation return object type for IMP declaration", () =>
		{
			AssertType<TraderJobDocAddressValidation>("When at least one Entry Instruction is a Non Warehouse, PiggyBackedDocAddressValidation return type", declaration.PiggyBackedDocAddressValidation(declaration.SupplierDocumentaryAddress));

			entryInstruction1.CEI_Procedure = "71";
			entryInstruction2.CEI_Procedure = "71";
			AssertType<TraderJobDocAddressValidation>("When all entry instructions are non warehouse, Ucc6TraderJobDocAddressValidation expected", declaration.PiggyBackedDocAddressValidation(declaration.SupplierDocumentaryAddress));

			entryInstruction2.CEI_Procedure = "40";
			AssertType<TraderJobDocAddressValidation>("When at least one Entry Instruction is a Non Warehouse, PiggyBackedDocAddressValidation return type", declaration.PiggyBackedDocAddressValidation(declaration.SupplierDocumentaryAddress));
		});

		declaration.JE_MessageType = "EXP";
		CombineAssertions("Assert PiggyBackedDocAddressValidation return object type for EXP declaration", () =>
		{
			entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			AssertType<TraderJobDocAddressValidation>("When all Entry Instructions have ParticipantType: BuyersConsolManySuppliersOneImporter, TraderJobDocAddressValidation expected", declaration.PiggyBackedDocAddressValidation(declaration.SupplierDocumentaryAddress));

			entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
			AssertType<TraderJobDocAddressValidation>("When at least Entry Instructions have ParticipantType: BuyersConsolManySuppliersOneImporter, PiggyBackedDocAddressValidation the return type", declaration.PiggyBackedDocAddressValidation(declaration.SupplierDocumentaryAddress));

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertType<Ucc6ExportSupplierJobDocAddressValidation>("When message type is UCC6 EXP, PiggyBackedDocAddressValidation() for Supplier", declaration.PiggyBackedDocAddressValidation(declaration.SupplierDocumentaryAddress));
			}
		});
	}

	public void TestExporterPiggyBackedDocAddressValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Exporter = Factory.New<OrgHeader>().PK;
		var exporterDocAddress = declaration.ExporterDocAddress;

		declaration.JE_MessageType = "IMP";
		AssertNull("When Message Type is IMP, PiggyBackedDocAddressValidation() for Exporter", declaration.PiggyBackedDocAddressValidation(exporterDocAddress));

		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertType<RequiringEoriUcc6TraderJobDocAddressValidation>("When Message Type is EXP UCC6, RequiredEoriUcc6TraderJobDocAddressValidation for Exporter", declaration.PiggyBackedDocAddressValidation(exporterDocAddress));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertNull("When Message Type is EXP not UCC6, PiggyBackedDocAddressValidation() for Exporter", declaration.PiggyBackedDocAddressValidation(exporterDocAddress));
		}
	}
}
