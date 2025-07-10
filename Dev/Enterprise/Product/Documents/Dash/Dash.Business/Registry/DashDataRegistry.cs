using System;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business
{
	public sealed class DashDataRegistry : RegistryItemSet
	{
		public const string CommercialInvoiceWorkflowConfigurationRegistryKey = "CommercialInvoiceWorkflowConfiguration";
		public const string AccountPayableInvoiceWorkflowConfigurationRegistryKey = "AccountPayableInvoiceWorkflowConfiguration";
		public static MultilingualString WorkflowConfigurationCaption = ResString.GetMultilingualString("B274FBC2-CBC6-418A-A587-5FA13B7050E3", "Workflow Configuration");
		public static MultilingualString WorkflowConfigurationHint = ResString.GetMultilingualString("B66BA6C9-3DBB-43E1-A06A-7371B753A823", "Set Workflow Configuration");

		public override bool IsForProductivityWise => false;

		#region Singleton

		public static DashDataRegistry Instance => instance ??= new DashDataRegistry();

		[ThreadStatic]
		static DashDataRegistry instance;

		DashDataRegistry()
		{
		}

		#endregion

		public WorkflowConfigurationStepRegistryItem CommercialInvoiceWorkflowConfiguration
		{
			get
			{
				return GetItem(CommercialInvoiceWorkflowConfigurationRegistryKey, delegate
				{
					var defaultValue = new WorkflowConfigurationStepCollection(CommercialInvoiceWorkflowConfigurationPairListProvider);

					defaultValue.AddNew().Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
					defaultValue.AddNew().Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;
					defaultValue.AddNew().Code = SharedConstants.DataProcessingType.Code.Validation;
					defaultValue.AddNew().Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

					return new WorkflowConfigurationStepRegistryItem(
						name: CommercialInvoiceWorkflowConfigurationRegistryKey,
						category: DocManagerRegistry.Categories.System_DocManager_DocumentIngestion_ParseTypes_CommercialInvoice,
						caption: WorkflowConfigurationCaption,
						hint: WorkflowConfigurationHint,
						storage: RegistryStorageFlags.System,
						CommercialInvoiceWorkflowConfigurationPairListProvider,
						options:  DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.Value
								 ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue
								 : RegistryOptions.IsHidden,
						defaultValue);
				});
			}
		}

		public WorkflowConfigurationStepRegistryItem AccountPayableInvoiceWorkflowConfiguration
		{
			get
			{
				return GetItem(AccountPayableInvoiceWorkflowConfigurationRegistryKey, delegate
				{
					var defaultValue = new WorkflowConfigurationStepCollection(AccountPayableInvoiceWorkflowConfigurationPairListProvider);

					defaultValue.AddNew().Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
					defaultValue.AddNew().Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

					return new WorkflowConfigurationStepRegistryItem(
						name: AccountPayableInvoiceWorkflowConfigurationRegistryKey,
						category: DocManagerRegistry.Categories.System_DocManager_DocumentIngestion_ParseTypes_AccountPayableInvoice,
						caption: WorkflowConfigurationCaption,
						hint: WorkflowConfigurationHint,
						storage: RegistryStorageFlags.System,
						AccountPayableInvoiceWorkflowConfigurationPairListProvider,
						options: DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.Value
								 ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue
								 : RegistryOptions.IsHidden,
						defaultValue);
				});
			}
		}

		public static CodeDescriptionPairListProvider CommercialInvoiceWorkflowConfigurationPairListProvider => new (() => GetWorkflowConfigurationPairList(CommercialInvoiceWorkflowConfigurationRegistryKey));

		public static CodeDescriptionPairListProvider AccountPayableInvoiceWorkflowConfigurationPairListProvider => new (() => GetWorkflowConfigurationPairList(AccountPayableInvoiceWorkflowConfigurationRegistryKey));

		static CodeDescriptionPairList GetWorkflowConfigurationPairList(string registryKey)
		{
			var list = new CodeDescriptionPairList();

			list.AddPair(SharedConstants.DataProcessingType.Code.OrganizationMatching, SharedConstants.DataProcessingType.Description.OrganizationMatching);

			if (registryKey == CommercialInvoiceWorkflowConfigurationRegistryKey)
			{
				list.AddPair(SharedConstants.DataProcessingType.Code.ProductCodeMatching, SharedConstants.DataProcessingType.Description.ProductCodeMatching);
				list.AddPair(SharedConstants.DataProcessingType.Code.Validation, SharedConstants.DataProcessingType.Description.Validation);
			}

			list.AddPair(SharedConstants.DataProcessingType.Code.NotifyDownstreamServices, SharedConstants.DataProcessingType.Description.NotifyDownstreamServices);

			return list;
		}
	}
}
