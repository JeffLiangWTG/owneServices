using System;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Tests
{
	[TestedType(typeof(DashDataRegistry))]
	public class DashDataRegistryTests : RegistryItemSetTestCaseWithFactory<DashDataRegistry>
	{
		public void Test_CommercialInvoice_WorkflowConfiguration_RegistryItem_CommercialInvoiceDocumentParsingEnabled()
		{
			TestCommercialInvoiceWorkflowConfigurationHelper(enableCommercialInvoiceDocumentParsing: true);
		}

		public void Test_CommercialInvoice_WorkflowConfiguration_RegistryItem_CommercialInvoiceDocumentParsingDisabled()
		{
			TestCommercialInvoiceWorkflowConfigurationHelper(enableCommercialInvoiceDocumentParsing: false);
		}

		public void Test_AccountPayableInvoice_WorkflowConfiguration_RegistryItem_AccountPayableInvoiceDocumentParsingEnabled()
		{
			TestAccountPayableInvoiceWorkflowConfigurationHelper(enableAccountPayableInvoiceDocumentParsing: true);
		}

		public void Test_AccountPayableInvoice_WorkflowConfiguration_RegistryItem_Test_AccountPayableInvoiceParsingDisabled()
		{
			TestAccountPayableInvoiceWorkflowConfigurationHelper(enableAccountPayableInvoiceDocumentParsing: false);
		}

		#region ConditionallyVisibleRegistryItems

		protected override System.Collections.Generic.IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return DashDataRegistry.CommercialInvoiceWorkflowConfigurationRegistryKey;
				yield return DashDataRegistry.AccountPayableInvoiceWorkflowConfigurationRegistryKey;
			}
		}

		#endregion

		#region Helpers

		void TestCommercialInvoiceWorkflowConfigurationHelper(bool enableCommercialInvoiceDocumentParsing)
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCommercialInvoiceDocumentParsing))
			{
				var defaultValues = new WorkflowConfigurationStepCollection(DashDataRegistry.CommercialInvoiceWorkflowConfigurationPairListProvider);
				defaultValues.AddNew().Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
				defaultValues.AddNew().Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;
				defaultValues.AddNew().Code = SharedConstants.DataProcessingType.Code.Validation;
				defaultValues.AddNew().Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

				TestGenericRegistryItem(ItemSet.CommercialInvoiceWorkflowConfiguration,
					"CommercialInvoiceWorkflowConfiguration",
					"System/DocManager/Document Ingestion/Parse Types/Commercial Invoice",
					"Workflow Configuration",
					"Set Workflow Configuration",
					RegistryStorageFlags.System,
					enableCommercialInvoiceDocumentParsing
					? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue
					: RegistryOptions.IsHidden,
					defaultValues);

				var newDefaultValues = new WorkflowConfigurationStepCollection(DashDataRegistry.CommercialInvoiceWorkflowConfigurationPairListProvider);
				var organizationItem = newDefaultValues.AddNew();
				organizationItem.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
				var ndsItem = newDefaultValues.AddNew();
				ndsItem.Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;
				AssertEquals(SharedConstants.DataProcessingType.Description.OrganizationMatching, organizationItem.Description);
				AssertEquals(SharedConstants.DataProcessingType.Description.NotifyDownstreamServices, ndsItem.Description);

				ItemSet.CommercialInvoiceWorkflowConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDefaultValues);
				AssertEquals("Value should be set", newDefaultValues, ItemSet.CommercialInvoiceWorkflowConfiguration.Value);
			}
		}

		void TestAccountPayableInvoiceWorkflowConfigurationHelper(bool enableAccountPayableInvoiceDocumentParsing)
		{
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableAccountPayableInvoiceDocumentParsing))
			{
				var defaultValues = new WorkflowConfigurationStepCollection(DashDataRegistry.AccountPayableInvoiceWorkflowConfigurationPairListProvider);
				defaultValues.AddNew().Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
				defaultValues.AddNew().Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

				TestGenericRegistryItem(ItemSet.AccountPayableInvoiceWorkflowConfiguration,
					"AccountPayableInvoiceWorkflowConfiguration",
					"System/DocManager/Document Ingestion/Parse Types/AP Invoice",
					"Workflow Configuration",
					"Set Workflow Configuration",
					RegistryStorageFlags.System,
					enableAccountPayableInvoiceDocumentParsing
					? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue
					: RegistryOptions.IsHidden,
					defaultValues);

				var newDefaultValues = new WorkflowConfigurationStepCollection(DashDataRegistry.AccountPayableInvoiceWorkflowConfigurationPairListProvider);
				var organizationItem = newDefaultValues.AddNew();
				organizationItem.Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
				var ndsItem = newDefaultValues.AddNew();
				ndsItem.Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;
				AssertEquals(SharedConstants.DataProcessingType.Description.OrganizationMatching, organizationItem.Description);
				AssertEquals(SharedConstants.DataProcessingType.Description.NotifyDownstreamServices, ndsItem.Description);

				ItemSet.AccountPayableInvoiceWorkflowConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDefaultValues);
				AssertEquals("Value should be set", newDefaultValues, ItemSet.AccountPayableInvoiceWorkflowConfiguration.Value);
			}
		}

		#endregion
	}
}
