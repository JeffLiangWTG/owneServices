using System;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(RegistryUserAgreementTypeRegistryEditor))]
	public class RegistryUserAgreementTypeRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RegistryUserAgreementTypeRegistryItem("UserAgreementTypes", (NoResString)"", (NoResString)"User Agreement Types", (NoResString)"List of User Agreement Types that can be implemented in the User Agreement Module", RegistryStorageFlags.System, RegistryOptions.Default, new RegistryUserAgreementTypeCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new RegistryUserAgreementTypeRegistryEditor(new RegistryUserAgreementTypeDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RegistryUserAgreementTypeControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var types = new RegistryUserAgreementTypeCollection[1];
			Factory.Save();
			types[0] = new RegistryUserAgreementTypeCollection();
			var type = (RegistryUserAgreementType)types[0].AddNew();
			type.Code = "CDE";
			type.Level = "USR";
			type.Description = (NoResString)"Code Description";
			return types;
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((RegistryUserAgreementTypeControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		#endregion Implementation
	}
}
