using System;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(PlugInLayoutProvider))]
sealed class PlugInLayoutProviderTest : EU.GUI.PlugIn.Testing.PlugInLayoutProviderAbstractTest
{
	protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Spain;

	protected override Type GetExpectedDeclarationPreviousDocumentsDetailsLayoutType(EU.Business.Declaration.JobDeclaration declaration) => typeof(PreviousDocumentFieldsLayout);

	protected override Type GetExpectedEntryInstructionPreviousDocumentsDetailsLayoutType(EU.Business.Declaration.JobDeclaration declaration) => typeof(PreviousDocumentFieldsLayout);

	protected override Type GetExpectedInvoiceHeaderPreviousDocumentsDetailsLayoutType(EU.Business.Declaration.JobDeclaration declaration) => typeof(PreviousDocumentFieldsLayout);

	protected override Type GetExpectedInvoiceLinePreviousDocumentsDetailsLayoutType(EU.Business.Declaration.JobDeclaration declaration) => typeof(PreviousDocumentFieldsLayout);
}
