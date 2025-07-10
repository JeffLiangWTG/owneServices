using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Security.ActiveDirectory.Registry
{
	public class AttributeMapRegistryItem : StronglyTypedRegistryItem<AttributeMap>
	{
		public AttributeMapRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint,
			RegistryStorageFlags storage, RegistryOptions options, AttributeMap defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AttributeMapRegistryDataType(defaultValue), storage, options))
		{
			OnUpdateAction = (companyPK, branchPK, departmentPK, newValue) =>
			{
				if (!string.IsNullOrEmpty(OneOffSyncModeValue))
				{
					ActiveDirectoryRegistry.Instance.OneOffSyncMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OneOffSyncModeValue);
					TryToNudgeActiveDirectorySynchronisationTask();
					OneOffSyncModeValue = string.Empty;
				}
			};
		}

		internal string OneOffSyncModeValue { get; set; } = string.Empty;

		void TryToNudgeActiveDirectorySynchronisationTask()
		{
			try
			{
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(Constants.ActiveDirectorySynchronisationTask.Code);
			}
			catch (Exception e) when (!e.IsCriticalException()) // Don't let nudging crash the service task...
			{
			}
		}
	}

	public class AttributeMapRegistryDataType : RegistryDataType<AttributeMap>
	{
		public AttributeMapRegistryDataType(AttributeMap defaultValue)
			: base(RegistryDataTypes.Codes.Binary, defaultValue)
		{
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, AttributeMap proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			proposedValue.MapItems.RunPreSaveValidation();
			if (proposedValue.MapItems.HasErrors())
			{
				var errors = new ZNotificationCollector(proposedValue.MapItems, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				throw new RegistryValidationException(errors.ToUniqueMessageListString());
			}

			var changedMapping = GetChangedMapping(proposedValue);
			if (changedMapping.Any())
			{
				const string confirmationText = "CONFIRM";
				var caption = Res.GetString("2838B8A8-174F-4CB1-80DF-FDFDF82EF71B", "Confirm Changing Attribute Mapping");
				var changedMappingString = string.Join(System.Environment.NewLine, changedMapping.Select(m => m.Item1 + " -> " + m.Item2));
				var message = Res.GetString("4A5A249F-F8D4-4E55-B23B-07DD5F2315F5", @"As there are undesired risks in both the Domain and in {0} with incorrect mappings, please verify the following Attribute Mappings and confirm the changes.
{1}",
Core.Constants.ProductName,
changedMappingString);

				var warnings = new ZNotificationCollector(proposedValue.MapItems, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetWarnings();
				if (warnings.Any())
				{
					message += System.Environment.NewLine + System.Environment.NewLine;
					message += Res.GetString("FC4F87EB-E413-4297-84BF-C59244D02915", @"Please also confirm the following warning(s):
{0}", warnings.ToUniqueMessageListString());
				}

				if (Globals.Message.ShowConfirmation(message, caption, confirmationText, ZMessageBoxIcon.Warning, ZMessageBoxButtons.OKCancel) != ZDialogResult.OK)
				{
					throw new RegistryValidationException(Res.GetString("e23477c3-8dfc-4ad1-b83c-31d446eeeb70", "Canceled saving this registry value"));
				}
			}

			if (ActiveDirectoryRegistry.Instance.IsIntegrationEnabled && HasNewlySyncedItems(proposedValue) && (ActiveDirectoryRegistry.Instance.SyncDirection == SyncDirection.TwoWay || ActiveDirectoryRegistry.Instance.SyncDirectionGroup == SyncDirection.TwoWay))
			{
				if (!string.IsNullOrEmpty(ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value))
				{
					throw new RegistryValidationException(Res.GetString("e63421e1-cbd0-4cca-85f1-908e455e2397", @"Please wait for the Active Directory Synchronization service task to run before enabling synchronization on another attribute as the Attribute Mapping has recently been modified."));
				}
				else
				{
					var args = new UserResponseArgument();
					args.Caption = Res.GetString("73003dce-6f70-4ca2-bf5e-75997f937f29", "Modify Attribute Mapping");
					args.Message = ModifyWarningQuestion;
					args.Buttons = ZMessageBoxButtons.OKCancel;
					args.DefaultButton = ZMessageBoxDefaultButton.Button1;
					args.Icon = ZMessageBoxIcon.Exclamation;
					args.AnswerList = new SyncModeList();

					var response = UserNotification.QueryUserResponse(args);

					if (!string.IsNullOrEmpty(response))
					{
						((AttributeMapRegistryItem)registryItem).OneOffSyncModeValue = response;
					}
					else
					{
						throw new RegistryValidationException(Res.GetString("b515ad6b-4719-43ce-9bac-462b7bd8d6eb", "Modifying Attribute Mapping canceled"));
					}
				}
			}
		}

		IEnumerable<(ZString, ZString)> GetChangedMapping(AttributeMap proposedMap)
		{
			var defaultItems = AttributeMap.DefaultMap.MapItems.Cast<AttributeMapItem>();
			var proposedItems = proposedMap.MapItems.Cast<AttributeMapItem>();
			foreach (var proposedItem in proposedItems)
			{
				foreach (var currentItem in defaultItems)
				{
					if (proposedItem.EnterpriseColumnName == currentItem.EnterpriseColumnName && proposedItem.ActiveDirectoryAttributeName != currentItem.ActiveDirectoryAttributeName)
					{
						yield return (proposedItem.EnterpriseColumnName, proposedItem.ActiveDirectoryAttributeName);
					}
				}
			}
		}

		bool HasNewlySyncedItems(AttributeMap proposedMap)
		{
			var currentItems = ActiveDirectoryRegistry.Instance.AttributeMapping.Value.MapItems.Cast<AttributeMapItem>();
			var proposedItems = proposedMap.MapItems.Cast<AttributeMapItem>();
			return proposedItems.Any(proposedItem => proposedItem.IsSynced && !currentItems.Any(currentItem => currentItem.IsSynced && currentItem.EnterpriseColumnName == proposedItem.EnterpriseColumnName));
		}

		public IUserNotification UserNotification
		{
			get { return userNotification ?? Globals.Message; }
			set { userNotification = value; }
		}
		IUserNotification userNotification;

		public static string ModifyWarningQuestion
		{
			get
			{
				return ResString.GetMultilingualString("a4dfdf92-6b39-43b1-8380-2ede1eb39984", @"You are about to start synchronizing new attributes.
Please select the Primary Data Source you want to use to pre-populate these new attributes.");
			}
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo() => new AttributeMapRegistryEditorInfo();

		protected override byte[] SerialiseCore(AttributeMap value) => Encoding.Unicode.GetBytes(value.ToStringForSerialisation());

		protected override AttributeMap DeserialiseCore(byte[] value)
		{
			var map = new AttributeMap();
			string mapAsString = Encoding.Unicode.GetString(value);
			foreach (string mapItem in mapAsString.Split(','))
			{
				var mapItemComponents = mapItem.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				if (mapItemComponents.Length == 3)
				{
					var columnName = mapItemComponents[0];
					var attributeName = mapItemComponents[1];
					var isSynced = new ZBool(mapItemComponents[2]);
					if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(attributeName))
					{
						map.MapItems.Add(new AttributeMapItem(columnName, attributeName, isSynced));
					}
				}
			}

			map.AddElementsFromDefault();

			return map;
		}

		protected override AttributeMap CloneValue(AttributeMap value) => value.Clone();
	}

	[RegistryEditor(AttributeMapRegistryEditorInfo.fullyQualifiedEditorClassAndAssembly)]
	public class AttributeMapRegistryEditorInfo : RegistryEditorInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Fully-qualified type name")]
		const string fullyQualifiedEditorClassAndAssembly = "Enterprise.Security.ActiveDirectory.GUI.Registry.AttributeMapRegistryEditor, Enterprise.Security.ActiveDirectory.GUI";

		public override Type BaseDataTypeToBeEdited => typeof(AttributeMapRegistryDataType);
	}
}
