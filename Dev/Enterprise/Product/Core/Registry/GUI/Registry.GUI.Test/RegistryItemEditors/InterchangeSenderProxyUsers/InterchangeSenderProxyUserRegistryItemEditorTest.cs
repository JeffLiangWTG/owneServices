using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(InterchangeSenderProxyUserRegistryItemEditor))]
	sealed class InterchangeSenderProxyUserRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new InterchangeSenderProxyUserRegistryItemEditor(RegistryItem.DataType, null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(InterchangeSenderProxyUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new InterchangeSenderProxyUsersRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new InterchangeSenderProxyUserCollection());
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var setCollection = (InterchangeSenderProxyUserCollection)setValue;
			var getCollection = (InterchangeSenderProxyUserCollection)getValue;

			AssertEquals(setCollection.Count, getCollection.Count);
			base.AssertSetAndGetValuesEqual(setValue, getValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new InterchangeSenderProxyUserCollection();
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((InterchangeSenderProxyUserControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
