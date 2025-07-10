using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(ExcessNPRExclusionEventCodeRegistryItemEditor))]
	public class ExcessNPRExclusionEventCodeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ExcessNPRExclusionEventCodeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ExcessNPRExclusionEventCodeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ExcessNPRExclusionEventCodeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ExcessNPRExclusionEventCodeSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ExcessNPRExclusionEventCodeSettingCollection(new FallbackLevel(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory)
			{
				new ExcessNPRExclusionEventCodeSetting() { EventCode = "Z00" },
				new ExcessNPRExclusionEventCodeSetting() { EventCode = "Z05" }
			};
			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var collection1 = (ExcessNPRExclusionEventCodeSettingCollection)setValue;
			var collection2 = (ExcessNPRExclusionEventCodeSettingCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(collection1[i].EventCode, collection2[i].EventCode);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}

	[TestedType(typeof(ExcessNPRExclusionEventCodeControl))]
	public class ExcessNPRExclusionEventCodeControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ExcessNPRExclusionEventCodeSettingCollection(new FallbackLevel(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ExcessNPRExclusionEventCodeControl)control).NPRExclusionEventCodesGrid.ReadOnly;
		}
	}
}
