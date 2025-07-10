using System;
using System.Windows.Forms;
using Enterprise.Accounting.GUI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ComplianceDocumentSupportingRegistryItemEditor))]
	public class ComplianceDocumentSupportingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ComplianceDocumentSupportingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ComplianceDocumentSupportingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ComplianceDocumentSupportingReasonRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ComplianceDocumentSupportingReasonCollection();
			var copy = collection.AddNew();
			copy.Code = "TST";
			copy.Description = (NoResString)"Test code";
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ComplianceDocumentSupportingControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
