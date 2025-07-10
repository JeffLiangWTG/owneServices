using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JournalEntriesClassificationGroupCodeRegistryItem : StronglyTypedRegistryItem<JournalEntriesClassificationGroupCodeCollection>
	{
		public JournalEntriesClassificationGroupCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new JournalEntriesClassificationGroupCodeRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JournalEntriesClassificationGroupCodeRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class JournalEntriesClassificationGroupCodeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JournalEntriesClassificationGroupCodeCollection>
	{
		public JournalEntriesClassificationGroupCodeRegistryDataType()
		{
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;

		protected override void ValidateCore(IRegistryItem registryItem, JournalEntriesClassificationGroupCodeCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			proposedValue.CurrentFallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK);
			proposedValue.Cast<JournalEntriesClassificationGroupCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK));
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, JournalEntriesClassificationGroupCodeCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			var customizationSetting = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
			if (customizationSetting?.IsOptionGen ?? false)
			{
				throw new RegistryValidationException(Res.GetString("9203676C-0CE7-4654-8263-8F444051F8FF",
					"The Journal Entries Classification Group Code cannot be edited any more once the Allocation Option has been set to GEN in 'Journal Entries Number Customization' Registry."));
			}
		}
	}
}
