using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(PreviousDocument))]
sealed class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
{
	public void TestLookupsType()
	{
		AssertType<PreviousDocumentLookups>(previousDocument.Lookups);
	}

	public void TestValidationType_WhenParentIsDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "EXP";
			var previousDocument = declaration.PreviousDocuments.AddNew();
			AssertType<PreviousDocumentValidation>("When parent declaration is not import", previousDocument.Validation);

			var orphanPreviousDocument = Factory.New<PreviousDocument>();
			AssertType<PreviousDocumentValidation>("When parent declaration is not found", orphanPreviousDocument.Validation);
		});
	}

	public void TestValidationType_WhenParentIsInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var previousDocumentOnInvoiceLine = invoiceLine.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertType<InvoiceLineExportPreviousDocumentValidation>("When Declaration Type is EXP", previousDocumentOnInvoiceLine.Validation);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertType<ImportPreviousDocumentValidation>("When parent declaration is import", previousDocumentOnInvoiceLine.Validation);

			declaration.JE_MessageType = "";
			AssertType<PreviousDocumentValidation>("When parent declaration is not found", previousDocumentOnInvoiceLine.Validation);
		});
	}

	public void TestValidationType_WhenParentIsEntryInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var previousDocumentOnEntryInstruction = entryInstruction.PreviousDocuments.AddNew();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "EXP";
			AssertType<PreviousDocumentValidation>("For Non Ucc6 Export", previousDocumentOnEntryInstruction.Validation);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertType<Ucc6ExportEntryInstructionPreviousDocumentValidation>("For Ucc6 Export", previousDocumentOnEntryInstruction.Validation);
			}

			declaration.JE_MessageType = "IMP";
			AssertType<ImportPreviousDocumentValidation>("For Import", previousDocumentOnEntryInstruction.Validation);
		});
	}

	public void TestSetDefaultQuantityValues()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		declaration.JE_MessageType = "IMP";
		var invoiceLinePreviousDocument1 = invoiceLine.PreviousDocuments.AddNew();
		CombineAssertions("When Declaration is not EXP UCC6", () =>
		{
			AssertEquals("CSI_UnitOfQuantity", Core.Constants.Weight.Kilograms, invoiceLinePreviousDocument1.CSI_UnitOfQuantity);
			AssertEquals("CSI_UnitOfQuantity3", Core.Constants.Weight.Kilograms, invoiceLinePreviousDocument1.CSI_UnitOfQuantity3);
		});

		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var invoiceLinePreviousDocument2 = invoiceLine.PreviousDocuments.AddNew();
			CombineAssertions("When Declaration is EXP UCC6", () =>
			{
				AssertEquals("CSI_UnitOfQuantity", "", invoiceLinePreviousDocument2.CSI_UnitOfQuantity);
				AssertEquals("CSI_UnitOfQuantity3", "", invoiceLinePreviousDocument2.CSI_UnitOfQuantity3);
			});
		}

		declaration.JE_MessageType = "IMP";
		var orphanPreviousDocument = Factory.New<PreviousDocument>();
		CombineAssertions("When Previous Document is not attached to any Declaration", () =>
		{
			AssertEquals("CSI_UnitOfQuantity", "", orphanPreviousDocument.CSI_UnitOfQuantity);
			AssertEquals("CSI_UnitOfQuantity3", "", orphanPreviousDocument.CSI_UnitOfQuantity3);
		});

		orphanPreviousDocument.CSI_UnitOfQuantity = "G";
		orphanPreviousDocument.CSI_UnitOfQuantity3 = "T";
		declaration.PreviousDocuments.Add(orphanPreviousDocument);
		CombineAssertions("When Previous Document Parent changes but quantity are not empty", () =>
		{
			AssertEquals("CSI_UnitOfQuantity", "G", orphanPreviousDocument.CSI_UnitOfQuantity);
			AssertEquals("CSI_UnitOfQuantity3", "T", orphanPreviousDocument.CSI_UnitOfQuantity3);
		});
	}

	public void TestCSIProcedure()
	{
		PreviousDocumentHelperTest.TestSetDefaultAndEmptyField(previousDocument);
	}

	public void TestReadOnlyFields()
	{
		PreviousDocumentHelperTest.TestReadOnlyFields(previousDocument);
	}

	public void TestWeightAndQuantitiesFieldAreReadOnlyForProcedureNUM()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var previousDocument = declaration.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			previousDocument.CSI_Procedure = "NUM";
			AssertEquals("CSI_QuantityInfo ReadOnly", true, previousDocument.CSI_QuantityInfo.ReadOnly);
			AssertEquals("CSI_UnitOfQuantityInfo ReadOnly", true, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);
			AssertEquals("CSI_Quantity3Info ReadOnly", true, previousDocument.CSI_Quantity3Info.ReadOnly);
			AssertEquals("CSI_UnitOfQuantity3Info ReadOnly", true, previousDocument.CSI_UnitOfQuantity3Info.ReadOnly);
			AssertEquals("CSI_PackQtyInfo ReadOnly", true, previousDocument.CSI_PackQtyInfo.ReadOnly);
			AssertEquals("CSI_PackTypeInfo ReadOnly", true, previousDocument.CSI_PackTypeInfo.ReadOnly);
		});

		CombineAssertions(() =>
		{
			previousDocument.CSI_Procedure = "A3";
			AssertEquals("CSI_QuantityInfo ReadOnly", false, previousDocument.CSI_QuantityInfo.ReadOnly);
			AssertEquals("CSI_UnitOfQuantityInfo ReadOnly", false, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);
			AssertEquals("CSI_Quantity3Info ReadOnly", false, previousDocument.CSI_Quantity3Info.ReadOnly);
			AssertEquals("CSI_UnitOfQuantity3Info ReadOnly", false, previousDocument.CSI_UnitOfQuantity3Info.ReadOnly);
			AssertEquals("CSI_PackQtyInfo ReadOnly", false, previousDocument.CSI_PackQtyInfo.ReadOnly);
			AssertEquals("CSI_PackTypeInfo ReadOnly", false, previousDocument.CSI_PackTypeInfo.ReadOnly);
		});
	}

	protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		yield return invoiceLine.PreviousDocuments.AddNew();
		var product = factory.New<OrgSupplierPart>();
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		var pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;
		yield return pivot.PreviousDocuments.AddNew();
	}

	public void TestIsSummaryDeclarationDocument()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		CombineAssertions(() =>
		{
			previousDocument.CSI_Procedure = "";
			AssertEquals("Empty is not a summary declaration document", false, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "2";
			AssertEquals("2 is not a summary declaration document", false, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "A3";
			AssertEquals("A3 is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "PF";
			AssertEquals("PF is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "MRN";
			AssertEquals("MRN is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "NN";
			AssertEquals("NN is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "CIM";
			AssertEquals("CIM is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
			previousDocument.CSI_Procedure = "A44";
			AssertEquals("A44 is a summary declaration document", true, previousDocument.IsSummaryDeclarationDocument);
		});
	}

	public void TestPreviousDocumentHumanReadableName()
	{
		AssertEquals("Previous Document", previousDocument.HumanReadableName);
	}

	public void TestIsPreviousProcedureDocument()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		CombineAssertions(() =>
		{
			previousDocument.CSI_Procedure = "";
			AssertEquals("Empty is not a previous procedure document", false, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "A3";
			AssertEquals("A3 is not a previous procedure document", false, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "2";
			AssertEquals("2 is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "2S";
			AssertEquals("2S is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "2T";
			AssertEquals("2T is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "5";
			AssertEquals("5 is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "5S";
			AssertEquals("5S is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "5T";
			AssertEquals("5T is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "7";
			AssertEquals("7 is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "7S";
			AssertEquals("7S is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
			previousDocument.CSI_Procedure = "7T";
			AssertEquals("7T is a previous procedure document", true, previousDocument.IsPreviousProcedureDocument);
		});
	}

	public void TestDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationPreviousDocument = declaration.PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoicePreviousDocument = invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var invoiceLinePreviousDocument = invoiceLine.PreviousDocuments.AddNew();
		AssertSame(declaration, declarationPreviousDocument.Declaration);
		AssertSame(declaration, invoicePreviousDocument.Declaration);
		AssertSame(declaration, invoiceLinePreviousDocument.Declaration);

		var orphanPreviousDocument = Factory.New<PreviousDocument>();
		AssertNull("The document doesn't have a parent declaration", orphanPreviousDocument.Declaration);
	}

	public void TestEffectiveNetMass()
	{
		previousDocument.CSI_Quantity = 200;
		previousDocument.CSI_UnitOfQuantity = "T";

		CombineAssertions("EffectiveNetMass", () =>
		{
			AssertEquals("Amount", 200m, previousDocument.EffectiveNetMass.Amount);
			AssertEquals("Unit", "T", previousDocument.EffectiveNetMass.Unit);
			AssertEquals("InKilogramsSafe", 200000m, previousDocument.EffectiveNetMass.InKilogramsSafe);
		});

		previousDocument.CSI_Quantity = ZDecimal.Zero;
		previousDocument.CSI_UnitOfQuantity = "";

		CombineAssertions("EffectiveNetMass", () =>
		{
			AssertEquals("Amount", 0m, previousDocument.EffectiveNetMass.Amount);
			AssertEquals("Unit", "", previousDocument.EffectiveNetMass.Unit);
			AssertEquals("InKilogramsSafe", 0m, previousDocument.EffectiveNetMass.InKilogramsSafe);
			AssertEquals("IsEmpty", true, previousDocument.EffectiveNetMass.IsEmpty);
		});
	}

	public void TestEffectiveGrossMass()
	{
		previousDocument.CSI_Quantity3 = 100;
		previousDocument.CSI_UnitOfQuantity3 = "KG";

		CombineAssertions("EffectiveGrossMass", () =>
		{
			AssertEquals("Amount", 100m, previousDocument.EffectiveGrossMass.Amount);
			AssertEquals("Unit", "KG", previousDocument.EffectiveGrossMass.Unit);
			AssertEquals("InKilogramsSafe", 100m, previousDocument.EffectiveGrossMass.InKilogramsSafe);
		});

		previousDocument.CSI_Quantity3 = ZDecimal.Zero;
		previousDocument.CSI_UnitOfQuantity3 = "";

		CombineAssertions("EffectiveGrossMass", () =>
		{
			AssertEquals("Amount", 0m, previousDocument.EffectiveGrossMass.Amount);
			AssertEquals("Unit", "", previousDocument.EffectiveGrossMass.Unit);
			AssertEquals("IsEmpty", true, previousDocument.EffectiveGrossMass.IsEmpty);
		});
	}

	[ExpectNoExceptions]
	public void TestSetCSI_StatusTriggersCSI_ReferenceNumberValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_ReferenceNumber");
		mockPreviousDocument.Object.CSI_Status = "X";
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_StatusNoTriggersWithSuspendValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		using (mockPreviousDocument.Object.GetValidationSuspender())
		{
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_ReferenceNumber", Times.Never());
			mockPreviousDocument.Object.CSI_Status = "X";
			mockPreviousDocumentValidation.VerifyAll();
		}
	}

	[ExpectNoExceptions]
	public void TestSetCSI_UnitOfQuantityTriggersCSI_QuantityValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_Quantity");
		mockPreviousDocument.Object.CSI_UnitOfQuantity = "KGM";
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_UnitOfQuantityNoTriggersWithSuspendValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		using (mockPreviousDocument.Object.GetValidationSuspender())
		{
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_Quantity", Times.Never());
			mockPreviousDocument.Object.CSI_UnitOfQuantity = "KGM";
			mockPreviousDocumentValidation.VerifyAll();
		}
	}

	[ExpectNoExceptions]
	public void TestSetCSI_UnitOfQuantity3TriggersCSI_Quantity3Validation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_Quantity3");
		mockPreviousDocument.Object.CSI_UnitOfQuantity3 = "KGM";
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_UnitOfQuantity3NoTriggersWithSuspendValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		using (mockPreviousDocument.Object.GetValidationSuspender())
		{
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_Quantity3", Times.Never());
			mockPreviousDocument.Object.CSI_UnitOfQuantity3 = "KGM";
			mockPreviousDocumentValidation.VerifyAll();
		}
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureTriggersCSI_TariffValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_Tariff");
		mockPreviousDocument.Object.CSI_Procedure = "A3";
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureTriggersCSI_QuantityValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_Quantity");
		mockPreviousDocument.Object.CSI_Procedure = "A3";
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureTriggersCSI_Quantity3Validation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_Quantity3");
		mockPreviousDocument.Object.CSI_Procedure = "A3";
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureTriggersPackageQuantityValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_PackQty");
		mockPreviousDocument.Object.CSI_Procedure = "A3";
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetCSI_ProcedureNoTriggersWithSuspendValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		using (mockPreviousDocument.Object.GetValidationSuspender())
		{
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_PackQty", Times.Never());
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_Quantity3", Times.Never());
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_Quantity", Times.Never());
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_Tariff", Times.Never());
			mockPreviousDocument.Object.CSI_Procedure = "A3";
			mockPreviousDocumentValidation.VerifyAll();
		}
	}

	[ExpectNoExceptions]
	public void TestSetFormattedTariffTriggersCSI_Quantity2Validation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_Quantity2");
		mockPreviousDocument.Object.FormattedTariff = "1111111111";
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetFormattedTariffNoTriggersWithSuspendValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		using (mockPreviousDocument.Object.GetValidationSuspender())
		{
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_Quantity2", Times.Never());
			mockPreviousDocument.Object.FormattedTariff = "1111111111";
			mockPreviousDocumentValidation.VerifyAll();
		}
	}

	[ExpectNoExceptions]
	public void TestSetPackageQuantityTriggersPackageQuantityValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		mockPreviousDocumentValidation.Protected().Setup("CheckCSI_PackQty");
		mockPreviousDocument.Object.CSI_PackQty = 1;
		mockPreviousDocumentValidation.VerifyAll();
	}

	[ExpectNoExceptions]
	public void TestSetPackageQuantityNoTriggersWithSuspendValidation()
	{
		var mockPreviousDocument = Factory.NewMoq<PreviousDocument>();
		var mockPreviousDocumentValidation = new Mock<PreviousDocumentValidation>(mockPreviousDocument.Object);
		mockPreviousDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockPreviousDocumentValidation.Object);
		using (mockPreviousDocument.Object.GetValidationSuspender())
		{
			mockPreviousDocumentValidation.Protected().Verify("CheckCSI_PackQty", Times.Never());
			mockPreviousDocument.Object.CSI_PackQty = 1;
			mockPreviousDocumentValidation.VerifyAll();
		}
	}

	public void TestAsIPreviousDocumentReferenceNumberProvider()
	{
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_ReferenceNumber = "12345X";

		CombineAssertions(() =>
		{
			var referenceNumberProvider = ((IPreviousDocumentReferenceNumberProvider)previousDocument).ReferenceNumberProvider;
			AssertEquals("Procedure", "A3", referenceNumberProvider.Procedure);
			AssertEquals("ReferenceNumber", "12345X", referenceNumberProvider.ReferenceNumber);
			AssertEquals("ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, referenceNumberProvider.ReferenceNumberInfo);
			AssertEquals("ReferenceNumberCin", "X", referenceNumberProvider.ReferenceNumberCin);
			AssertEquals("ReferenceNumberWithoutCin", "12345", referenceNumberProvider.ReferenceNumberWithoutCin);
		});
	}

	public void TestAsIPreviousDocumentUniversalTariffProvider()
	{
		previousDocument.CSI_Tariff = "6402121000";
		previousDocument.CSI_Procedure = "A2";
		CombineAssertions(() =>
		{
			var previousDocumentUniversalTariffProvider = (IPreviousDocumentUniversalTariffProvider)previousDocument;
			AssertEquals("TariffCode", "6402121000", previousDocumentUniversalTariffProvider.TariffCode);
			AssertEquals("FormattedTariff", "6402.12.10 00", previousDocumentUniversalTariffProvider.FormattedTariff);
			AssertEquals("UniversalTariffType", Universal.Constants.TariffTypes.Export, previousDocumentUniversalTariffProvider.UniversalTariffType);
			AssertEquals("Quantity2Info", previousDocument.CSI_Quantity2Info, previousDocumentUniversalTariffProvider.Quantity2Info);
			AssertEquals("Procedure", "A2", previousDocumentUniversalTariffProvider.Procedure);
		});
	}

	public void TestCSI_ReferenceNumberMaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "EXP";
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertEquals("When parent declaration is Ucc6 Export, MaxLength", 70, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				AssertEquals("When parent declaration is Non Ucc6 Export, MaxLength", 8, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
			}

			declaration.JE_MessageType = "IMP";
			AssertEquals("When parent declaration is import, MaxLength", 35, previousDocument.CSI_ReferenceNumberInfo.MaxLength);

			declaration.JE_MessageType = "ABX";
			AssertEquals("When parent declaration is not import, MaxLength", 8, previousDocument.CSI_ReferenceNumberInfo.MaxLength);

			var orphanPreviousDocument = Factory.New<PreviousDocument>();
			AssertEquals("When parent declaration is not found, MaxLength", 8, orphanPreviousDocument.CSI_ReferenceNumberInfo.MaxLength);
		});
	}

	public void TestCSI_CodeMaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "EXP";
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertEquals("For Ucc6 Export, MaxLength", 4, previousDocument.CSI_CodeInfo.MaxLength);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				AssertEquals("For Non Ucc6 Export, MaxLength", 3, previousDocument.CSI_CodeInfo.MaxLength);
			}

			declaration.JE_MessageType = "IMP";
			AssertEquals("For Import, MaxLength", 3, previousDocument.CSI_CodeInfo.MaxLength);

			var orphanPreviousDocument = Factory.New<PreviousDocument>();
			AssertEquals("Without declaration, MaxLength", 3, orphanPreviousDocument.CSI_CodeInfo.MaxLength);
		});
	}

	public void TestCSI_PackTypeMaxLength()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "EXP";
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertEquals("For Ucc6 Export, MaxLength", 2, previousDocument.CSI_PackTypeInfo.MaxLength);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				AssertEquals("For Non Ucc6 Export, MaxLength", 3, previousDocument.CSI_PackTypeInfo.MaxLength);
			}

			declaration.JE_MessageType = "IMP";
			AssertEquals("For Import, MaxLength", 3, previousDocument.CSI_PackTypeInfo.MaxLength);

			var orphanPreviousDocument = Factory.New<PreviousDocument>();
			AssertEquals("Without declaration, MaxLength", 3, orphanPreviousDocument.CSI_PackTypeInfo.MaxLength);
		});
	}

	public void TestCSI_PackTypeResourceStringData()
	{
		CombineAssertions(() =>
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_PackTypeInfo);
			AssertEquals("ShortCaption", "Pkg Type", resourceStringData.ShortCaption);
			AssertEquals("Caption", "Package Type", resourceStringData.Caption);
		});
	}

	public void TestISupportMultipleResourceStringData()
	{
		var supportMultipleResourceStringData = (ISupportMultipleResourceStringData)Factory.New<PreviousDocument>();
		AssertEquals("When parent declaration is not found, MultipleKeysToUse", 0, supportMultipleResourceStringData.MultipleKeysToUse.Count);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		supportMultipleResourceStringData = declaration.PreviousDocuments.AddNew();
		AssertEquals("When parent declaration is found, MultipleKeysToUse (contains declaration keys)", 2, supportMultipleResourceStringData.MultipleKeysToUse.Count);
	}

	public void TestISupportMultipleResourceStringData_Ucc6Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		ISupportMultipleResourceStringData supportMultipleResourceStringData = entryInstruction.PreviousDocuments.AddNew();

		CombineAssertions("For Non Ucc6 Export", () =>
		{
			AssertNotNull(supportMultipleResourceStringData);
			var multipleKeysToUse = supportMultipleResourceStringData?.MultipleKeysToUse ?? Array.Empty<string>();
			AssertEquals(1, multipleKeysToUse.Count);
			AssertContainsExactElementsInAnyOrder(new[] { PreviousDocument.EntryInstructionResourceStringKey }, multipleKeysToUse);
		});

		CombineAssertions("For Ucc6 Export", () =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertNotNull(supportMultipleResourceStringData);
				var multipleKeysToUse = supportMultipleResourceStringData?.MultipleKeysToUse ?? Array.Empty<string>();
				AssertEquals(2, multipleKeysToUse.Count);
				AssertContainsExactElementsInAnyOrder(new[] { PreviousDocument.EntryInstructionResourceStringKey, PreviousDocument.Ucc6ExportEntryInstructionResourceStringKey }, multipleKeysToUse);
			}
		});
	}

	public void TestCSI_ReferenceNumberResourceStringData()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_ReferenceNumberInfo);
		AssertEquals("Caption", "Number + CIN", resourceStringData.Caption);

		var uccResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_ReferenceNumberInfo, JobDeclaration.CaptionKeyUCC);
		AssertEquals("Caption", "Number", uccResourceStringData.Caption);

		var entryInstructionResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_ReferenceNumberInfo, PreviousDocument.Ucc6ExportEntryInstructionResourceStringKey);
		AssertEquals("Caption with Ucc6ExportEntryInstructionResourceStringKey", "Reference Number", entryInstructionResourceStringData.Caption);
		AssertEquals("FullDescription with Ucc6ExportEntryInstructionResourceStringKey", "[12 01 001 000] Reference Number", entryInstructionResourceStringData.FullDescription);

		var invoiceLineImportMultipleCaptionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_ReferenceNumberInfo, PreviousDocument.InvoiceLineImportResourceStringKey);
		AssertEquals("Caption with invoiceLineImport", "Number", invoiceLineImportMultipleCaptionResourceString.Caption);
		AssertEquals("ShortCaption with invoiceLineImport", "Number", invoiceLineImportMultipleCaptionResourceString.ShortCaption);
		AssertEquals("MediumCaption with invoiceLineImport", "Number", invoiceLineImportMultipleCaptionResourceString.MediumCaption);
	}

	public void TestCSI_QuantityResourceStringData()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_QuantityInfo);
		AssertEquals("Caption", "Net Mass", resourceStringData.Caption);

		var entryInstructionResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_QuantityInfo, PreviousDocument.EntryInstructionResourceStringKey);
		AssertEquals("Caption", "Mass", entryInstructionResourceStringData.Caption);

		var ucc6ExportMultipleCaptionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_QuantityInfo, JobDeclaration.CaptionKeyExportUCC6);
		AssertEquals("Caption with Ucc6Export", "Quantity", ucc6ExportMultipleCaptionResourceString.Caption);
		AssertEquals("ShortCaption with Ucc6Export", "Qty", ucc6ExportMultipleCaptionResourceString.ShortCaption);
		AssertEquals("MediumCaption with Ucc6Export", "Quantity", ucc6ExportMultipleCaptionResourceString.MediumCaption);
		AssertEquals("FullDescription with Ucc6Export", "Previous Document Quantity", ucc6ExportMultipleCaptionResourceString.FullDescription);

		var invoiceLineImportMultipleCaptionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_QuantityInfo, PreviousDocument.InvoiceLineImportResourceStringKey);
		AssertEquals("Caption with invoiceLineImport", "Mass", invoiceLineImportMultipleCaptionResourceString.Caption);
		AssertEquals("ShortCaption with invoiceLineImport", "Mass", invoiceLineImportMultipleCaptionResourceString.ShortCaption);
		AssertEquals("MediumCaption with invoiceLineImport", "Mass", invoiceLineImportMultipleCaptionResourceString.MediumCaption);
	}

	public void TestCSI_UnitOfQuantityResourceStringData()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_UnitOfQuantityInfo);
		AssertEquals("Caption", "Net Mass UQ", resourceStringData.Caption);

		var entryInstructionResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_UnitOfQuantityInfo, PreviousDocument.EntryInstructionResourceStringKey);
		AssertEquals("Caption", "Mass UQ", entryInstructionResourceStringData.Caption);

		var ucc6ExportMultipleCaptionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_UnitOfQuantityInfo, JobDeclaration.CaptionKeyExportUCC6);
		AssertEquals("Caption with Ucc6Export", "UOM", ucc6ExportMultipleCaptionResourceString.Caption);
		AssertEquals("ShortCaption with Ucc6Export", "UOM", ucc6ExportMultipleCaptionResourceString.ShortCaption);
		AssertEquals("MediumCaption with Ucc6Export", "Unit Of Measure", ucc6ExportMultipleCaptionResourceString.MediumCaption);
		AssertEquals("FullDescription with Ucc6Export", "Quantity Unit of Measure", ucc6ExportMultipleCaptionResourceString.FullDescription);

		var invoiceLineImportMultipleCaptionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_UnitOfQuantityInfo, PreviousDocument.InvoiceLineImportResourceStringKey);
		AssertEquals("Caption with invoiceLineImport", "Mass Unit", invoiceLineImportMultipleCaptionResourceString.Caption);
		AssertEquals("ShortCaption with invoiceLineImport", "Mass Unit", invoiceLineImportMultipleCaptionResourceString.ShortCaption);
		AssertEquals("MediumCaption with invoiceLineImport", "Mass Unit", invoiceLineImportMultipleCaptionResourceString.MediumCaption);
	}

	public void TestCSI_CodeResourceStringData()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_CodeInfo);
		AssertEquals("Caption without MultipleKey", "Document", resourceStringData.Caption);

		var entryInstructionResourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(previousDocument.CSI_CodeInfo, PreviousDocument.Ucc6ExportEntryInstructionResourceStringKey);
		AssertEquals("Caption with Ucc6ExportEntryInstructionResourceStringKey", "Type", entryInstructionResourceStringData.Caption);
		AssertEquals("FullDescription with Ucc6ExportEntryInstructionResourceStringKey", "[12 01 002 000] Type", entryInstructionResourceStringData.FullDescription);
	}

	protected override void SetUp()
	{
		base.SetUp();

		previousDocument = Factory.New<PreviousDocumentForTest>();
		Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.Add(previousDocument);
	}

	PreviousDocumentForTest previousDocument;
}

class PreviousDocumentForTest : PreviousDocument, IPreviousDocumentForTesting
{
	public PreviousDocumentForTest(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public JobComInvoiceLine ParentLine => (Parent as JobComInvoiceLine);
}

static class PreviousDocumentTestHelper
{
	public static PreviousDocument CreateDocument(
		  BusinessObjectFactory factory
		, ZString csiCode
		, ZDate dateOfIssue
		, ZString referenceNumber
		, ZString subType
		, ZString status
		, ZString customsOffice
		, ZShort lineNo
		, ZString procedure
		, ZString referenceNumber2
		, ZString tariff
		, ZString unitOfQuantity2
		, ZDecimal netMass
		, ZDecimal supplementaryQuantity
		, ZDecimal grossMass
		, ZInt packageQuantity
		, string unitOfNetMass = "KG"
		, string unitOfGrossMass = "KG"
		)
	{
		var previousDocument = factory.New<PreviousDocument>();
		previousDocument.CSI_Procedure = procedure;
		previousDocument.CSI_Code = csiCode;
		previousDocument.CSI_DateOfIssue = dateOfIssue;
		previousDocument.CSI_ReferenceNumber = referenceNumber;
		previousDocument.CSI_SubType = subType;
		previousDocument.CSI_Status = status;
		previousDocument.CSI_CustomsOffice = customsOffice;
		previousDocument.CSI_LineNo = lineNo;
		previousDocument.CSI_ReferenceNumber2 = referenceNumber2;
		previousDocument.CSI_Tariff = tariff;
		previousDocument.CSI_UnitOfQuantity2 = unitOfQuantity2;
		previousDocument.CSI_Quantity = netMass;
		previousDocument.CSI_UnitOfQuantity = unitOfNetMass;
		previousDocument.CSI_Quantity2 = supplementaryQuantity;
		previousDocument.CSI_Quantity3 = grossMass;
		previousDocument.CSI_UnitOfQuantity3 = unitOfGrossMass;
		previousDocument.CSI_PackQty = packageQuantity;
		return previousDocument;
	}

	public static IMergedPreviousDocument CreateMergedPreviousDocument(
		ZString csiCode
		, ZDate dateOfIssue
		, ZString referenceNumber
		, ZString referenceNumberCin
		, ZString subType
		, ZString status
		, ZString customsOffice
		, ZShort lineNo
		, ZString procedure
		, ZString referenceNumber2
		, ZString tariff
		, ZDecimal netMass
		, ZDecimal supplementaryQuantity
		, ZDecimal grossMass
		, ZInt packageQuantity
		, bool isPreviousProcedureDocument = false
		, bool isSummaryDeclarationDocument = false
)
	{
		var previousDocumentMock = new Mock<IMergedPreviousDocument>();
		previousDocumentMock.Setup(m => m.Register).Returns(procedure);
		previousDocumentMock.Setup(m => m.Category).Returns(csiCode);
		previousDocumentMock.Setup(m => m.Date).Returns(dateOfIssue);
		previousDocumentMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
		previousDocumentMock.Setup(m => m.ReferenceNumberCin).Returns(referenceNumberCin);
		previousDocumentMock.Setup(m => m.Type).Returns(subType);
		previousDocumentMock.Setup(m => m.Series).Returns(status);
		previousDocumentMock.Setup(m => m.CustomsOffice).Returns(customsOffice);
		previousDocumentMock.Setup(m => m.ItemNumber).Returns(lineNo);
		previousDocumentMock.Setup(m => m.Mrn).Returns(referenceNumber2);
		previousDocumentMock.Setup(m => m.Tariff).Returns(tariff);
		previousDocumentMock.Setup(m => m.NetMass).Returns(netMass);
		previousDocumentMock.Setup(m => m.SupplementaryQuantity).Returns(supplementaryQuantity);
		previousDocumentMock.Setup(m => m.GrossMass).Returns(grossMass);
		previousDocumentMock.Setup(m => m.PackageQuantity).Returns(packageQuantity);
		previousDocumentMock.Setup(m => m.IsPreviousProcedureDocument).Returns(isPreviousProcedureDocument);
		previousDocumentMock.Setup(m => m.IsSummaryDeclarationDocument).Returns(isSummaryDeclarationDocument);
		return previousDocumentMock.Object;
	}
}
