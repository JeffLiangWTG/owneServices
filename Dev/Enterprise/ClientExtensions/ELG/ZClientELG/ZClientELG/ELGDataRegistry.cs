using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.ELG
{
	internal sealed class ELGDataRegistry : RegistryItemSet
	{
		ELGDataRegistry()
		{
		}

		internal static ELGDataRegistry Instance
		{
			get { return instance ?? (instance = new ELGDataRegistry()); }
		}
		[ThreadStatic]
		static ELGDataRegistry instance;

		public override bool IsForProductivityWise => false;

		#region EnableSagAccountsExportItem

		public bool EnableSagDataExport
		{
			get { return SagDataTransferSwitchRegistryItem.Value.EnableInterface; }
		}

		#endregion

		#region SagDataTransferSwitchRegistryItem

		internal ServiceTaskDataTransferSwitchRegistryItem SagDataTransferSwitchRegistryItem
		{
			get
			{
				return GetItem("ELGDataTransferSwitchRegistryItem", delegate
				{
					return new ServiceTaskDataTransferSwitchRegistryItem(
						"ELGDataTransferSwitchRegistryItem",
						(NoResString)sageInterfaceRegoLocation,
						(NoResString)"Data Export Settings",
						(NoResString)"Please fill in all the fields provided below.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		public ZString SagExportDirectory
		{
			get { return SagDataTransferSwitchRegistryItem.Value.Directory; }
		}

		public ZGuid SagExportNotifyGroup
		{
			get { return SagDataTransferSwitchRegistryItem.Value.GroupPK; }
		}

		#endregion

		#region BranchDepartmentCodeMappingRegistryItem

		internal BranchDepartmentCodeMappingRegistryBusinessObjectCollection BranchDepartmentCodeCollection
		{
			get { return BranchDepartmentCodeCollectionItem.Value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal BranchDepartmentCodeMappingRegistryItem BranchDepartmentCodeCollectionItem
		{
			get
			{
				return GetItem("ELGBranchDepartmentCodeTranslation", delegate
				{
					return new BranchDepartmentCodeMappingRegistryItem("ELGBranchDepartmentCodeTranslation",
						(NoResString)sageInterfaceRegoLocation,
						(NoResString)branchDepartmentCodeMappingErrorCategory,
						(NoResString)"Please enter the profit centre and nominal department against the CargoWise One Branch and Department codes.",
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region TransportAndChargeCodeMappingRegistryItem

		internal TransportAndChargeCodeMappingRegistryBusinessObjectCollection TransportAndChargeCodeCollection
		{
			get { return TransportAndChargeCodeCollectionItem.Value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal TransportAndChargeCodeMappingRegistryItem TransportAndChargeCodeCollectionItem
		{
			get
			{
				return GetItem("ELGTransportAndChargeCodeTranslation", delegate
				{
					return new TransportAndChargeCodeMappingRegistryItem("ELGTransportAndChargeCodeTranslation",
						(NoResString)sageInterfaceRegoLocation,
						(NoResString)transportAndChargeCodeMappingCaption,
						(NoResString)"Please enter the nominal cost code and nominal revenue code against the CargoWise One Transport Mode and Charge codes.",
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region SageAccountCodeMappingRegistryItem

		internal SageAccountCodeMappingRegistryBusinessObjectCollection SageAccountCodeCollection
		{
			get { return SageAccountCodeCollectionItem.Value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal SageAccountCodeMappingRegistryItem SageAccountCodeCollectionItem
		{
			get
			{
				return GetItem("ELGSageAccountCodeTranslation", delegate
				{
					return new SageAccountCodeMappingRegistryItem("ELGSageAccountCodeTranslation",
						sageInterfaceRegoLocation,
						sageAccountCodeMappingCaption,
						"Please enter the Sage Account Code against the CargoWise One Organisation Code, Ledger Type and Currency.",
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Is Environment Valid

		internal static ZStringBuilder GetIsExportEnvironmentValid()
		{
			ZStringBuilder message = new ZStringBuilder();
			if (Instance.SagExportDirectory.IsEmpty || Env.CurrentUser.IsBatchProcessor && !FolderExists)
			{
				message.Append(GetMessage("Data Export Settings", "Export Directory not specified or does not exists.", sageInterfaceRegoLocation));
			}
			if (!NotificationGroupValidator.IsValid(Instance.SagExportNotifyGroup, new BusinessObjectFactory()))
			{
				message.Append(GetMessage("Data Export Settings", "Notification group not specified or has no email recipients.", sageInterfaceRegoLocation));
			}
			if (!IsValidBranchDepartmentCodeMappingRegistryItem)
			{
				message.Append(GetMessage(branchDepartmentCodeMappingErrorCategory, "You must have at least one Branch Department Code translation set.", sageInterfaceRegoLocation));
			}
			if (!IsValidTransportAndChargeCodeMappingRegistryItem)
			{
				message.Append(GetMessage(transportAndChargeCodeMappingCaption, "You must have at least one Transport Mode and Charge Code translation set.", sageInterfaceRegoLocation));
			}
			if (!IsValidSageAccountCodeMappingRegistryItem)
			{
				message.Append(GetMessage(sageAccountCodeMappingCaption, "You must have at least one Sage Account Code translation set.", sageInterfaceRegoLocation));
			}
			return message;
		}

		static bool IsValidBranchDepartmentCodeMappingRegistryItem
		{
			get { return (Instance.BranchDepartmentCodeCollection.Count > 0); }
		}

		static bool IsValidTransportAndChargeCodeMappingRegistryItem
		{
			get { return (instance.TransportAndChargeCodeCollection.Count > 0); }
		}

		static bool IsValidSageAccountCodeMappingRegistryItem
		{
			get { return (instance.SageAccountCodeCollection.Count > 0); }
		}

		static bool FolderExists
		{
			get
			{
				return Directory.Exists(ClientSharedComponents.SharedUtil.GetFinalPath(Instance.SagExportDirectory));
			}
		}

		internal static ZString GetMessage(String errorCategory, String errorReason, String registryLocation)
		{
			return ZString.Format(environmentErrorMessageTemplate, errorCategory, errorReason, string.Join(" -> ", registryLocation.Split('/')));
		}

		#endregion

		internal const string branchDepartmentCodeMappingErrorCategory = "Profit Centre Translation Table";
		internal const string transportAndChargeCodeMappingCaption = "Nominal Code Translation Table";
		internal const string sageAccountCodeMappingCaption = "Sage Account Code Translation Table";

		const string regoLocation = "ELG Client Extensions";
		internal const string sageInterfaceRegoLocation = regoLocation + @"/Sage Accounts Interface";
		const string environmentErrorMessageTemplate = "Issue with the \"{0}\" registry setting(s): {1}  Please check in Admin -> Registry -> {2} -> {0}";
	}
}
