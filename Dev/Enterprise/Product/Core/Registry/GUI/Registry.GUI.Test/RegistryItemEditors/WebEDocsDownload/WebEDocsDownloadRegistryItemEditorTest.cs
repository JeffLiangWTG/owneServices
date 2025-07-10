using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebEDocsDownloadRegistryItemEditor))]
	sealed class WebEDocsDownloadRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new WebEDocsDownloadRegistryItemEditor(new WebEDocsDownloadRegistryDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WebEDocsDownloadRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebEDocsDownloadRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebEDocsDownloadRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			var docTypeBOE = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "BOE"));
			var docTypeMSC = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC"));

			var collection = new RefDocTypeEntryCollection();
			collection.AddNew().RefDocTypePK = docTypeBOE.PK;
			collection.AddNew().RefDocTypePK = docTypeMSC.PK;
			var modules = new WebEDocsDownloadEntryDictionary();
			modules.Add("AllModules", new WebEDocsDownloadEntry(true, collection));

			return new[] { modules };
		}

		#endregion
	}
}
