using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(JournalEntriesClassificationGroupControl))]
	public class JournalEntriesClassificationGroupControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new JournalEntriesClassificationGroupCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var journalEntriesClassificationGroupControl = control as JournalEntriesClassificationGroupControl;
			AssertNotNull(journalEntriesClassificationGroupControl);

			var grid = journalEntriesClassificationGroupControl.FindSingleOrDefault<ZGrid>("journalEntriesClassificationGroupGrid");
			AssertNotNull(grid);

			return grid.ReadOnly;
		}

		public void TestGridColumns()
		{
			var columns = new string[]
			{
				"Ledger",
				"TransactionType",
				"GroupCode",
				"GroupCodeDescription",
			};

			var columnCaptions = new string[]
			{
				"Ledger",
				"Transaction Type",
				"Group Code",
				"Code Description"
			};

			var journalEntriesClassificationGroupCollection = new JournalEntriesClassificationGroupCollection();
			using (var form = new ZForm(journalEntriesClassificationGroupCollection))
			{
				var control = new JournalEntriesClassificationGroupControl();
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingleOrDefault<ZGrid>("journalEntriesClassificationGroupGrid");
				AssertNotNull("journalEntriesClassificationGroupGrid", grid);
				AssertEquals(columns.Length, grid.ColumnStyles.Count);

				for (var i = 0; i < grid.ColumnStyles.Count; i++)
				{
					var columnInfo = (ZGridColumnInfo)grid.ColumnStyles[i];
					AssertEquals($"{columnInfo.ColumnName} should be present", columns[i], columnInfo.ColumnName);
					AssertEquals($"Caption", columnCaptions[i], columnInfo.CaptionResourceString.Caption);
					AssertEquals($"{columnInfo.Caption} should be visibility", true, columnInfo.IsVisible);
				}
			}
		}

		public void TestReadOnly_RebindData()
		{
			var editableCompany = GlbCompany.CurrentCompany;
			var editableAssertMessage = "The control should not be read-only when the saved AllocationOption is not GEN.";
			var editableGroupCollection = new JournalEntriesClassificationGroupCollection(new FallbackLevel(editableCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var readOnlyCompany = testObjectCreator.NonCurrentCompany;
			var readOnlyAssertMessage = "The control should be read-only when the saved AllocationOption is GEN.";
			var readOnlyGroupCollection = new JournalEntriesClassificationGroupCollection(new FallbackLevel(readOnlyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			testObjectCreator.SetJournalEntriesNumberAllocationOptionToGen(readOnlyCompany);

			using var form = new ZForm();
			form.Show();

			using var editableControl = BindNewData(form, null, editableGroupCollection);
			AssertEquals(editableAssertMessage, false, editableControl.ReadOnly);

			using var readOnlyControl = BindNewData(form, editableControl, readOnlyGroupCollection);
			AssertEquals(readOnlyAssertMessage, true, readOnlyControl.ReadOnly);

			using var editableControlNew = BindNewData(form, readOnlyControl, editableGroupCollection);
			AssertEquals(editableAssertMessage, false, editableControlNew.ReadOnly);
		}

		RegistryZUserControl BindNewData(ZForm form, RegistryZUserControl oldControl, JournalEntriesClassificationGroupCollection newData)
		{
			if (oldControl != null)
			{
				form.Controls.Remove(oldControl);
			}
			var control = GetNewControl();
			form.Controls.Add(control);
			control.ReadOnly = false;
			control.SetDataBinding(null, string.Empty);
			control.SetDataBinding(newData, string.Empty);
			return control;
		}
	}
}
