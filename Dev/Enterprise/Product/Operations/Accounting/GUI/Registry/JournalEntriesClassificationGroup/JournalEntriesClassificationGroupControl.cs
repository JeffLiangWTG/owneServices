using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	internal partial class JournalEntriesClassificationGroupControl : RegistryBusinessObjectTemplateZUserControl
	{
		public JournalEntriesClassificationGroupControl()
		{
			InitializeComponent();
		}

		void ChangeReadOnlyIfNeed()
		{
			var fallbackLevel = (DataSource as JournalEntriesClassificationGroupCollection)?.CurrentFallbackLevel;
			if (!ReadOnly && fallbackLevel != null)
			{
				var savedSetting = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation
					.GetFallBackValueAtAllLevels(fallbackLevel.CompanyPK(false), fallbackLevel.BranchPK, fallbackLevel.DepartmentPK);
				if (savedSetting?.IsOptionGen ?? false)
				{
					ReadOnly = true;
				}
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			ChangeReadOnlyIfNeed();
			base.OnAfterFirstBinding(e);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			journalEntriesClassificationGroupGrid.ReadOnly = readOnly;
		}
	}
}
