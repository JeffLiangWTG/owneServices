using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(DefaultNumberOfSupportingDocumentsRegistryItemEditor))]
	public class DefaultNumberOfSupportingDocumentsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DefaultNumberOfSupportingDocumentsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DefaultNumberOfSupportingDocumentsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DefaultNumberOfSupportingDocumentsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DefaultNumberOfSupportingDocumentsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, Collection);
		}

		readonly DefaultNumberOfSupportingDocumentsCollection Collection = new DefaultNumberOfSupportingDocumentsCollection();

		protected override object[] GetValidRegistryValues()
		{
			Collection.AddDefaultValues("abcde", (NoResString)"default test desc", (NoResString)"test desc", 1);
			return new object[] { Collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
