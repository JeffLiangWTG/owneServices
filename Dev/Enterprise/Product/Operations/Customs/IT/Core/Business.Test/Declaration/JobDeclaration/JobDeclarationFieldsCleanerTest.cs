using System;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationFieldsCleanerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new JobDeclarationFieldsCleaner(declaration: null));
	}

	[ExpectNoExceptions]
	public void TestMessageDependentStrategiesAreInvoked()
	{
		var declarationStrategyMock = new Mock<ICleanUpStrategy>();
		var declarationPreviousDocumentStrategyMock = new Mock<ICleanUpStrategy>();
		var declarationSupportingDocumentStrategyMock = new Mock<ICleanUpStrategy>();
		var declarationShipmentIncoTermStrategyMock = new Mock<ICleanUpStrategy>();

		var entryInstructionStrategyMock = new Mock<ICleanUpStrategy>();

		var invoiceStrategyMock = new Mock<ICleanUpStrategy>();
		var invoicePreviousDocumentStrategyMock = new Mock<ICleanUpStrategy>();
		var invoiceSupportingDocumentStrategyMock = new Mock<ICleanUpStrategy>();

		var invoiceLineStrategyMock = new Mock<ICleanUpStrategy>();
		var invoiceLinePreviousDocumentStrategyMock = new Mock<ICleanUpStrategy>();
		var invoiceLineSupportingDocumentStrategyMock = new Mock<ICleanUpStrategy>();

		var declaration = Factory.New<JobDeclaration>();
		var declarationPreviousDocument = declaration.PreviousDocuments.AddNew();
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoicePreviousDocument = invoice.PreviousDocuments.AddNew();
		var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();

		var invoiceLine = invoice.InvoiceLines.AddNew();
		var invoiceLinePreviousDocument = invoiceLine.PreviousDocuments.AddNew();
		var invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();

		var strategyFactoryMock = new Mock<IBusinessObjectCleanUpStrategyFactory>();
		strategyFactoryMock.Setup(x => x.CreateForDeclaration(declaration)).Returns(() => declarationStrategyMock.Object);
		strategyFactoryMock.Setup(x => x.CreateForPreviousDocument(declarationPreviousDocument)).Returns(() => declarationPreviousDocumentStrategyMock.Object);
		strategyFactoryMock.Setup(x => x.CreateForSupportingDocument(declarationSupportingDocument)).Returns(() => declarationSupportingDocumentStrategyMock.Object);
		strategyFactoryMock.Setup(x => x.CreateForDeclarationShipmentIncoTerm(declaration)).Returns(() => declarationShipmentIncoTermStrategyMock.Object);

		strategyFactoryMock.Setup(x => x.CreateForEntryInstruction(entryInstruction)).Returns(() => entryInstructionStrategyMock.Object);

		strategyFactoryMock.Setup(x => x.CreateForInvoice(invoice)).Returns(() => invoiceStrategyMock.Object);
		strategyFactoryMock.Setup(x => x.CreateForPreviousDocument(invoicePreviousDocument)).Returns(() => invoicePreviousDocumentStrategyMock.Object);
		strategyFactoryMock.Setup(x => x.CreateForSupportingDocument(invoiceSupportingDocument)).Returns(() => invoiceSupportingDocumentStrategyMock.Object);

		strategyFactoryMock.Setup(x => x.CreateForInvoiceLine(invoiceLine)).Returns(() => invoiceLineStrategyMock.Object);
		strategyFactoryMock.Setup(x => x.CreateForPreviousDocument(invoiceLinePreviousDocument)).Returns(() => invoiceLinePreviousDocumentStrategyMock.Object);
		strategyFactoryMock.Setup(x => x.CreateForSupportingDocument(invoiceLineSupportingDocument)).Returns(() => invoiceLineSupportingDocumentStrategyMock.Object);

		var fieldsCleaner = new JobDeclarationFieldsCleaner(declaration);
		fieldsCleaner.SetCleanUpStrategyFactory(strategyFactoryMock.Object);

		((IJobDeclarationFieldsCleaner)fieldsCleaner).CleanUpMessageDependentFieldsIfNoLongerApplicable();

		declarationStrategyMock.Verify(x => x.CleanUp(), Times.Once());
		declarationPreviousDocumentStrategyMock.Verify(x => x.CleanUp(), Times.Once());
		declarationSupportingDocumentStrategyMock.Verify(x => x.CleanUp(), Times.Once());
		declarationShipmentIncoTermStrategyMock.Verify(x => x.CleanUp(), Times.Once());

		entryInstructionStrategyMock.Verify(x => x.CleanUp(), Times.Once());

		invoiceStrategyMock.Verify(x => x.CleanUp(), Times.Once());
		invoicePreviousDocumentStrategyMock.Verify(x => x.CleanUp(), Times.Once());
		invoiceSupportingDocumentStrategyMock.Verify(x => x.CleanUp(), Times.Once());

		invoiceLineStrategyMock.Verify(x => x.CleanUp(), Times.Once());
		invoiceLinePreviousDocumentStrategyMock.Verify(x => x.CleanUp(), Times.Once());
		invoiceLineSupportingDocumentStrategyMock.Verify(x => x.CleanUp(), Times.Once());
	}

	[ExpectNoExceptions]
	public void TestTransportModeInlandDependentStrategiesAreInvoked()
	{
		var declaration = Factory.New<JobDeclaration>();

		var declarationTransportModeInlandStrategyMock = new Mock<ICleanUpStrategy>();
		var strategyFactoryMock = new Mock<IBusinessObjectCleanUpStrategyFactory>();
		strategyFactoryMock.Setup(x => x.CreateForDeclarationTransportModeInland(declaration)).Returns(() => declarationTransportModeInlandStrategyMock.Object);

		var fieldsCleaner = new JobDeclarationFieldsCleaner(declaration);
		fieldsCleaner.SetCleanUpStrategyFactory(strategyFactoryMock.Object);

		((IJobDeclarationFieldsCleaner)fieldsCleaner).CleanUpTransportModeInlandDependentFieldsIfNoLongerApplicable();
		declarationTransportModeInlandStrategyMock.Verify(x => x.CleanUp(), Times.Once());
	}

	[ExpectNoExceptions]
	public void TestCleanUpShipmentIncoTermFieldsStrategyIsInvoked()
	{
		var declaration = Factory.New<JobDeclaration>();

		var declarationIncoTermStrategyMock = new Mock<ICleanUpStrategy>();
		var strategyFactoryMock = new Mock<IBusinessObjectCleanUpStrategyFactory>();
		strategyFactoryMock.Setup(x => x.CreateForDeclarationShipmentIncoTerm(declaration)).Returns(() => declarationIncoTermStrategyMock.Object);

		var fieldsCleaner = new JobDeclarationFieldsCleaner(declaration);
		fieldsCleaner.SetCleanUpStrategyFactory(strategyFactoryMock.Object);

		((IJobDeclarationFieldsCleaner)fieldsCleaner).CleanUpShipmentIncoTermFieldsIfNoLongerApplicable();
		declarationIncoTermStrategyMock.Verify(x => x.CleanUp(), Times.Once());
	}

	public void TestCleanUpStrategyFactory()
	{
		var declaration = Factory.New<JobDeclaration>();
		var fieldsCleaner = new JobDeclarationFieldsCleanerForTest(declaration);
		AssertType<BusinessObjectCleanUpStrategyFactory>("Type", fieldsCleaner.CleanUpStrategyFactoryExposed);
	}

	class JobDeclarationFieldsCleanerForTest : JobDeclarationFieldsCleaner
	{
		public JobDeclarationFieldsCleanerForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		public IBusinessObjectCleanUpStrategyFactory CleanUpStrategyFactoryExposed => CleanUpStrategyFactory;
	}
}
