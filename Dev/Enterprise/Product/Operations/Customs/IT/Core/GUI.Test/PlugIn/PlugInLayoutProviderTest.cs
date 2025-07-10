using System;
using System.Collections.Generic;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(PlugInLayoutProvider))]
sealed class PlugInLayoutProviderTest : EU.GUI.PlugIn.Testing.PlugInLayoutProviderAbstractTest
{
	protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Italy;

	protected override Type GetExpectedInvoiceLinePreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => declaration switch
	{
		not null when declaration.IsUCC6AndIsExport => typeof(Ucc6ExportPreviousDocumentFieldsLayout),
		not null when declaration.IsImport => typeof(InvoiceLineImportPreviousDocumentFieldsLayout),
		_ => typeof(PreviousDocumentFieldsLayout)
	};

	protected override Type GetExpectedInvoiceHeaderPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => declaration switch
	{
		not null when declaration.IsImport => typeof(ImportPreviousDocumentFieldsLayout),
		_ => typeof(PreviousDocumentFieldsLayout)
	};

	protected override Type GetExpectedEntryInstructionPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => declaration switch
	{
		not null when declaration.IsUCC6AndIsExport => typeof(Ucc6ExportEntryInstructionPreviousDocumentFieldsLayout),
		_ => typeof(EntryInstructionPreviousDocumentFieldsLayout)
	};

	protected override Type GetExpectedDeclarationPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => typeof(PreviousDocumentFieldsLayout);

	protected override IEnumerable<(string, JobDeclaration)> JobDeclarationsForTest
	{
		get
		{
			foreach (var item in base.JobDeclarationsForTest)
			{
				yield return item;
			}

			foreach (var isUCC6 in new[] { true, false })
			{
				foreach (var messageType in new[] { EUJobMessageTypeList.Codes.Import, EUJobMessageTypeList.Codes.Export, string.Empty })
				{
					var declaration = Factory.New<JobDeclaration>();
					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6))
					{
						declaration.JE_MessageType = messageType;
						yield return ($"UCC6: {declaration.IsUCC6}, MessageType: {declaration.JE_MessageType}", declaration);
					}
				}
			}
		}
	}
}
