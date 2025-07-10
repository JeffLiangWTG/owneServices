using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(PeriodReopenLevelsRegistryItemEditor))]
	public class PeriodReopenLevelsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new PeriodReopenLevelsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PeriodReopenLevelsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PeriodReopenLevelsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PeriodReopenLevelsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			PeriodReopenLevelsCollection collection = new PeriodReopenLevelsCollection();

			PeriodReopenLevels periodReopenLevels = collection.AddNew();
			periodReopenLevels.Range = periodReopenLevels.RangeList[0].Code;
			periodReopenLevels.AuthorisationRequirement = periodReopenLevels.AuthorisationRequirementList[0].Code;
			periodReopenLevels.Days = 200;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
