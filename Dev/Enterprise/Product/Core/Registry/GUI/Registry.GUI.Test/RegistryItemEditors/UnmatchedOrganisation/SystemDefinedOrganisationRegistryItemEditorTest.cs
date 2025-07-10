using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SystemDefinedOrganisationRegistryItemEditor))]
	sealed class SystemDefinedOrganisationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new SystemDefinedOrganisationRegistryItemEditor(RegistryItem.DataType, null, new BusinessObjectFactory());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SystemDefinedOrganisationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SystemDefinedOrganisationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SystemDefinedOrganisationRegistryItem("", null, null, null, RegistryStorageFlags.System, typeof(UnmatchedOrganisation));
		}

		protected override object[] GetValidRegistryValues()
		{
			UnmatchedOrganisation result = new UnmatchedOrganisation();
			result.IsEnabled = true;
			return new object[] { result };
		}

		#endregion
	}
}
