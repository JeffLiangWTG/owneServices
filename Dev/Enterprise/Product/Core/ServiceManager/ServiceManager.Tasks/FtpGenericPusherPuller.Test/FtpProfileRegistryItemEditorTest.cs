using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpProfileRegistryItemEditor))]
	sealed class FtpProfileRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new FtpProfileRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((FtpConfigControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(FtpConfigControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new FtpProfileCollectionRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new FtpProfileCollection();
			var profile1 = collection.AddNew();
			FtpProfileDataTypeTest.SetupProfile(profile1);
			profile1.FriendlyName = "ABC";
			var profile2 = collection.AddNew();
			FtpProfileDataTypeTest.SetupProfile(profile1);
			profile1.FriendlyName = "DEF";
			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var collection1 = (FtpProfileCollection)setValue;
			var collection2 = (FtpProfileCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(collection1[i].FriendlyName, collection2[i].FriendlyName);
				AssertEquals(collection1[i].LocalFolder, collection2[i].LocalFolder);
				AssertEquals(collection1[i].RemoteLocation, collection2[i].RemoteLocation);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
