using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(JournalEntriesNumberCustomisationSettingControl))]
	public class JournalEntriesNumberCustomisationSettingControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new JournalEntriesNumberCustomisationSetting();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var journalEntriesNumberCustomisationSettingControl = control as JournalEntriesNumberCustomisationSettingControl;
			AssertNotNull(journalEntriesNumberCustomisationSettingControl);

			return journalEntriesNumberCustomisationSettingControl.ReadOnly;
		}

		public void TestElementsInControl()
		{
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting();
			using (var form = new ZForm(journalEntriesNumberCustomisationSetting))
			{
				var control = new JournalEntriesNumberCustomisationSettingControl();
				form.Controls.Add(control);
				form.Show();

				var numberRuleDropEdit = control.FindSingleOrDefault<ZDropEdit>("numberRuleDropEdit");
				AssertNotNull("numberRuleDropEdit", numberRuleDropEdit);
				AssertEquals("Number Rule", numberRuleDropEdit.CaptionResourceString.Caption);

				var allocationOptionDropEdit = control.FindSingleOrDefault<ZDropEdit>("allocationOptionDropEdit");
				AssertNotNull("allocationOptionDropEdit", allocationOptionDropEdit);
				AssertEquals("Allocation Option", allocationOptionDropEdit.CaptionResourceString.Caption);

				var sequenceResetOptionDropEdit = control.FindSingleOrDefault<ZDropEdit>("sequenceResetOptionDropEdit");
				AssertNotNull("sequenceResetOptionDropEdit", sequenceResetOptionDropEdit);
				AssertEquals("Sequence Reset Option", sequenceResetOptionDropEdit.CaptionResourceString.Caption);

				AssertNotNull("accountingTransactionsNumberSequenceCustomisationControl", control.FindSingleOrDefault<AccountingTransactionsNumberSequenceCustomisationControl>("accountingTransactionsNumberSequenceCustomisationControl"));
			}
		}

		[TestDate(2024, 7, 21)]
		public void TestReadOnly_RebindData()
		{
			var registryItem = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation;
			var activeCompanies = GlbCompany.GetActiveCompanies();
			var editableCompany = activeCompanies.First();
			var editableSettingFallback = new FallbackLevel(editableCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var editableSetting = new JournalEntriesNumberCustomisationSetting(editableSettingFallback)
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.NON,
			};
			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			testObjectCreator.CreateTestPeriodsForEntireYear(editableCompany, 2024);
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions
				.SetValue(editableSettingFallback.CompanyPK(false), Guid.Empty, Guid.Empty, true);
			using var disposableForEditableSetting = registryItem.SetTemporaryValue(editableSettingFallback.CompanyPK(false), editableSettingFallback.BranchPK, editableSettingFallback.DepartmentPK, editableSetting);

			var readOnlyCompany = activeCompanies.Last();
			var readOnlySettingFallback = new FallbackLevel(readOnlyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var readOnlySetting = new JournalEntriesNumberCustomisationSetting(readOnlySettingFallback)
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
			};
			testObjectCreator.CreateTestPeriodsForEntireYear(readOnlyCompany, 2024);
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions
				.SetValue(readOnlySettingFallback.CompanyPK(false), Guid.Empty, Guid.Empty, true);
			using var disposableForReadOnlySetting = registryItem.SetTemporaryValue(readOnlySettingFallback.CompanyPK(false), readOnlySettingFallback.BranchPK, readOnlySettingFallback.DepartmentPK, readOnlySetting);

			using var form = new ZForm();
			using var control = GetNewControl();
			form.Controls.Add(control);
			form.Show();

			var readOnlyAssertMessage = "The control should be read-only when the saved AllocationOption is GEN.";
			var editableAssertMessage = "The control should not be read-only when the saved AllocationOption is not GEN.";

			BindNewData(control, editableSetting);
			AssertEquals(editableAssertMessage, false, control.ReadOnly);

			editableSetting.AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN;
			control.Refresh();
			AssertEquals(editableAssertMessage, false, control.ReadOnly);

			BindNewData(control, readOnlySetting);
			AssertEquals(readOnlyAssertMessage, true, control.ReadOnly);

			BindNewData(control, editableSetting);
			AssertEquals(editableAssertMessage, false, control.ReadOnly);
		}

		void BindNewData(RegistryZUserControl control, JournalEntriesNumberCustomisationSetting newData)
		{
			control.ReadOnly = false;
			control.SetDataBinding(null, string.Empty);
			control.SetDataBinding(newData, string.Empty);
		}
	}
}
