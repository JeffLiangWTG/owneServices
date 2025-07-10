using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionWithEnabledAndDefaultsRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionWithEnabledAndDefaultCollection, CodeDescriptionWithEnabledAndDefaultCollection>
	{
		public CodeDescriptionWithEnabledAndDefaultsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionWithEnabledAndDefaultCollection defaultValue, bool isDefaultRequired)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionWithEnabledAndDefaultsRegistryDataType(defaultValue, isDefaultRequired), editorInfo, storage, options, defaultValue), editorInfo)
		{
		}

		public override int MaxLength
		{
			get { return DefaultValue.CodeMaxLength; }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CodeDescriptionWithEnabledAndDefaultsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionWithEnabledAndDefaultCollection>
	{
		public CodeDescriptionWithEnabledAndDefaultsRegistryDataType(CodeDescriptionWithEnabledAndDefaultCollection defaultValue, bool isDefaultRequired)
			: base(defaultValue)
		{
			this.isDefaultRequired = isDefaultRequired;
		}

		#region Serialise / Deserialise

		protected override CodeDescriptionWithEnabledAndDefaultCollection DeserialiseCore(byte[] value)
		{
			var deserializedCollection = base.DeserialiseCore(value);
			deserializedCollection.CodeMaxLength = DefaultValue.CodeMaxLength;

			var deserializedItemsByCode = new Dictionary<ZString, CodeDescriptionWithEnabledAndDefault>(deserializedCollection.Count);
			foreach (CodeDescriptionWithEnabledAndDefault item in deserializedCollection)
			{
				deserializedItemsByCode.Add(item.Code, item);
			}

			var alreadyHasDefault = deserializedCollection.Default != null;
			foreach (var systemDefinedItems in DefaultValue.Cast<CodeDescriptionWithEnabledAndDefault>().Where(item => item.IsSystemDefined))
			{
				CodeDescriptionWithEnabledAndDefault systemDefinedItemsInDeserializedCollection;
				if (!deserializedItemsByCode.TryGetValue(systemDefinedItems.Code, out systemDefinedItemsInDeserializedCollection))
				{
					deserializedCollection.AddNewSystemDefined(systemDefinedItems.Code, systemDefinedItems.Description, !alreadyHasDefault && systemDefinedItems.IsDefault, systemDefinedItems.IsEnabled);
					alreadyHasDefault |= systemDefinedItems.IsDefault;
				}
				else
				{
					systemDefinedItemsInDeserializedCollection.IsSystemDefined = true;
				}
			}

			return deserializedCollection;
		}

		#endregion

		readonly bool isDefaultRequired;

		protected override void ValidateCore(IRegistryItem registryItem, CodeDescriptionWithEnabledAndDefaultCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (isDefaultRequired && proposedValue.Default == null)
			{
				throw new RegistryValidationException(ResString.GetMultilingualString("621b0ed6-ae5e-4241-b6f6-69a469fe570d", "Please select a default."));
			}
		}
	}
}
