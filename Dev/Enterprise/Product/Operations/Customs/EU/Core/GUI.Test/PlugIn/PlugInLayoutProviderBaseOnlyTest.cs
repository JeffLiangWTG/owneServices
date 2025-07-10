using System;
using System.Collections.Generic;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing;

[TestedType(typeof(PlugInLayoutProvider))]
sealed class PlugInLayoutProviderBaseOnlyTest : PlugInLayoutProviderAbstractTest
{
	protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Latvia;

	protected override Type GetExpectedInvoiceHeaderPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => typeof(PreviousDocumentFieldsLayout);

	protected override Type GetExpectedInvoiceLinePreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => typeof(PreviousDocumentFieldsLayout);

	protected override Type GetExpectedDeclarationPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => typeof(PreviousDocumentFieldsLayout);

	protected override Type GetExpectedEntryInstructionPreviousDocumentsDetailsLayoutType(JobDeclaration declaration) => declaration switch
	{
		{ IsUCC6AndIsExport: true } or { IsUCC6AndIsImport: true } => typeof(UCC6PreviousDocumentFieldsLayout),
		_ => typeof(PreviousDocumentFieldsLayout),
	};

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
