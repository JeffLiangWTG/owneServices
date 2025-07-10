using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(VerboseLoggingRegistryItemEditor))]
	sealed class VerboseLoggingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new VerboseLoggingRegistryItem(string.Empty,
				null,
				null,
				null,
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new VerboseLoggingRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(VerboseLoggingControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new VerboseLoggingCollection();

			Add("UPG", "UPG Description");
			Add("FBK", "FBK Description");

			return new object[] { collection };

			void Add(string code, string description)
			{
				var item = collection.AddNew();
				item.Code = code;
				item.Description = (NoResString)description;
				item.Value = ZDateTime.Empty;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var control = (RegistryZUserControl)editorPane;
			return !control.ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
