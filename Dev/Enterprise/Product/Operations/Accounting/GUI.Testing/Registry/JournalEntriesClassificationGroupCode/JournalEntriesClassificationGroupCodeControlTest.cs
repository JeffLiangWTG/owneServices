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
	[TestedType(typeof(JournalEntriesClassificationGroupCodeControl))]
	public class JournalEntriesClassificationGroupCodeControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new JournalEntriesClassificationGroupCodeCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var journalEntriesClassificationGroupCodeControl = control as JournalEntriesClassificationGroupCodeControl;
			AssertNotNull(journalEntriesClassificationGroupCodeControl);

			var grid = journalEntriesClassificationGroupCodeControl.FindSingleOrDefault<ZGrid>("journalEntriesClassificationGroupCodeGrid");
			AssertNotNull(grid);

			return grid.ReadOnly;
		}

		public void TestGridColumns()
		{
			var columns = new string[]
			{
				"Code",
				"Description",
				"Prefix",
			};

			var columnCaptions = new string[]
			{
				"Code",
				"Description",
				"Prefix",
			};

			var journalEntriesClassificationGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection();
			using (var form = new ZForm(journalEntriesClassificationGroupCodeCollection))
			{
				var control = new JournalEntriesClassificationGroupCodeControl();
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingleOrDefault<ZGrid>("journalEntriesClassificationGroupCodeGrid");
				AssertNotNull("journalEntriesClassificationGroupCodeGrid", grid);
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
			var editableGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection(new FallbackLevel(editableCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var readOnlyCompany = testObjectCreator.NonCurrentCompany;
			var readOnlyAssertMessage = "The control should be read-only when the saved AllocationOption is GEN.";
			var readOnlyGroupCodeCollection = new JournalEntriesClassificationGroupCodeCollection(new FallbackLevel(readOnlyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			testObjectCreator.SetJournalEntriesNumberAllocationOptionToGen(readOnlyCompany);

			using var form = new ZForm();
			using var control = GetNewControl();
			form.Controls.Add(control);
			form.Show();

			using var editableControl = BindNewData(form, null, editableGroupCodeCollection);
			AssertEquals(editableAssertMessage, false, editableControl.ReadOnly);

			using var readOnlyControl = BindNewData(form, editableControl, readOnlyGroupCodeCollection);
			AssertEquals(readOnlyAssertMessage, true, readOnlyControl.ReadOnly);

			using var editableControlNew = BindNewData(form, readOnlyControl, editableGroupCodeCollection);
			AssertEquals(editableAssertMessage, false, editableControlNew.ReadOnly);
		}

		RegistryZUserControl BindNewData(ZForm form, RegistryZUserControl oldControl, JournalEntriesClassificationGroupCodeCollection newData)
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
