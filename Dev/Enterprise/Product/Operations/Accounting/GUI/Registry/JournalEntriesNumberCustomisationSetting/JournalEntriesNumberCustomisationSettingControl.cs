using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	internal partial class JournalEntriesNumberCustomisationSettingControl : RegistryBusinessObjectTemplateZUserControl
	{
		public JournalEntriesNumberCustomisationSettingControl()
		{
			InitializeComponent();
		}

		void ChangeReadOnlyIfNeed()
		{
			var fallbackLevel = (BoundBusinessObject as JournalEntriesNumberCustomisationSetting)?.CurrentFallbackLevel;
			if (!ReadOnly && fallbackLevel != null)
			{
				var savedSetting =
					AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetFallBackValueAtAllLevels(fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK);
				if (savedSetting != null && savedSetting.IsOptionGen)
				{
					ReadOnly = true;
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			ChangeReadOnlyIfNeed();
			base.OnCurrentDataItemChanged(e);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			accountingTransactionsNumberSequenceCustomisationControl.ReadOnly = readOnly;
		}
	}
}
