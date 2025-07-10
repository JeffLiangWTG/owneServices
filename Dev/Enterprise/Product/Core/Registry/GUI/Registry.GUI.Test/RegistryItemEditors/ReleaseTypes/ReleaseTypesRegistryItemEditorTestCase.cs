using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ReleaseTypesRegistryItemEditor))]
	sealed class ReleaseTypesRegistryItemEditorTestCase : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ReleaseTypesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ReleaseTypesRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ReleaseTypesRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ReleaseTypesRegistryItem("", null, null, null, RegistryStorageFlags.System, new ReleaseTypes());
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override object[] GetValidRegistryValues()
		{
			ReleaseTypes result = new ReleaseTypes();
			result.OriginalsNumber = 5;
			result.CopiesNumber = 6;

			ReleaseType releaseType = result.Types.AddNew();
			releaseType.Code = "OBR";
			releaseType.Description = (NoResString)"Original Bill Required at Destination";
			releaseType.CodeMaxLength = 3;
			releaseType.OriginalsNumber = 3;
			releaseType.CopiesNumber = 4;

			return new object[] { result };
		}

		#endregion
	}
}
