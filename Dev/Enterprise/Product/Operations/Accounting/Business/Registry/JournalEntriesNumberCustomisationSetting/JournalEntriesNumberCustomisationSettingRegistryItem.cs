using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class JournalEntriesNumberCustomisationSettingRegistryItem : StronglyTypedRegistryItem<JournalEntriesNumberCustomisationSetting>
	{
		public JournalEntriesNumberCustomisationSettingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new JournalEntriesNumberCustomisationSettingRegistryDataType(), storage, options))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);

			if ((newValue as JournalEntriesNumberCustomisationSetting).AllocationOption == AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN)
			{
				var periodCalculator = new AccountingPeriodCalculator(new BusinessObjectFactory());
				var startDateOfFinanceYear = periodCalculator.GetFirstPeriodManagementFromDate(ZDateTime.Now, companyOrOwnerPK).AM_StartDate.ToDateTime();
				AccountingConfigurationRegistry.Instance.AllocateJournalEntriesNumberStartDate.SetValue(companyOrOwnerPK, branchPK, departmentPK, startDateOfFinanceYear);
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JournalEntriesNumberCustomisationSettingRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class JournalEntriesNumberCustomisationSettingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JournalEntriesNumberCustomisationSetting>
	{
		public JournalEntriesNumberCustomisationSettingRegistryDataType()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, JournalEntriesNumberCustomisationSetting proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var fallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK);
			proposedValue.CurrentFallbackLevel = fallbackLevel;
			proposedValue.NumberSequenceCustomisations.CurrentFallbackLevel = fallbackLevel;
			proposedValue.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>().ForEach(x => x.CurrentFallbackLevel = fallbackLevel);
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;

		[SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer")]
		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, JournalEntriesNumberCustomisationSetting proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			var setting = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
			if (setting != null && setting.IsOptionGen)
			{
				throw new RegistryValidationException(Res.GetString("8D2958A0-662B-4BA8-A658-FDFF51C86212", "Number Rule and Allocation Option cannot be edited any more once Allocation Option has been set to GEN."));
			}
			if (proposedValue.IsOptionGen)
			{
				var caption = Res.GetString("5D03C3FD-5A83-4104-BF18-0E11C239A7FC", "Confirm Changing Journal Entries Number Customization");
				var message = Res.GetString("49C4B6F1-AFA0-42D0-806D-FC1FA8142BB4", "Number Rule and Allocation Option cannot be edited any more once Allocation Option has been set to GEN. Are you sure you want to save the change?");
				if (Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) != ZDialogResult.Yes)
				{
					throw new RegistryValidationException(Res.GetString("66F93FD7-767A-4F2E-916B-A7FF825FA239", "Canceled saving this registry value"));
				}
			}
		}
	}
}
