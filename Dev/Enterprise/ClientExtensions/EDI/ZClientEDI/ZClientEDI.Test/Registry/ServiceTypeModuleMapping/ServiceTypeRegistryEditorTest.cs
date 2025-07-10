using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ServiceTypeRegistryEditor))]
	public class ServiceTypeRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ServiceTypeRegistryItem("", null, null, null, new ServiceTypeRegistryEditorInfo(ModuleListType.MenuSection, false), RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			var defaultCollection = new SystemProductCollection();
			foreach (CodeDescriptionPair pair in new IncidentApprovalLookups(null).MenuSectionList)
			{
				defaultCollection.AddNew(pair.Code, pair.Description, true);
			}

			var editorInfo = new ServiceTypeRegistryEditorInfo(ModuleListType.MenuSection, true);
			return new ServiceTypeRegistryEditor(new ServiceTypeRegistryDataType(defaultCollection), editorInfo);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ServiceTypeRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new SystemProductCollection();
			collection.AddNew("AAA", "Module AAA", false);
			collection.AddNew("BBB", "Module BBB", false);
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ServiceTypeRegistryControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
