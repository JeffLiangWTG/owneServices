using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.eHub;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.eHub.Testing
{
	[TestedType(typeof(ScavengingSettingsListControlItemEditor))]
	sealed class ScavengingSettingsListControlItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ScavengingSettingsListControlItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((ScavengingSettingsUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ScavengingSettingsUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ScavengingSettingCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ScavengingSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			collection.Add(new ScavengingSetting() { TaskName = "TestTask1" });
			collection.Add(new ScavengingSetting() { TaskName = "TestTask2" });
			collection.Add(new ScavengingSetting() { TaskName = "TestTask3" });
			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var collection1 = (ScavengingSettingCollection)setValue;
			var collection2 = (ScavengingSettingCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].TaskName", i.ToString()), collection1[i].TaskName, collection2[i].TaskName);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].PeriodStart", i.ToString()), collection1[i].PeriodStart, collection2[i].PeriodStart);
				AssertEquals(string.Format("GetValueFromEditorPane()[{0}].PeriodEnd", i.ToString()), collection1[i].PeriodEnd, collection2[i].PeriodEnd);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
