using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.Environment.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class ImportOverridesForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes the business object. This is designer only")]
		public ImportOverridesForm()
			: base()
		{
			InitializeComponent();
		}

		public ImportOverridesForm(RegistryImportBusinessObject importObject, bool hideInactiveChildren = false)
			: base(importObject)
		{
			InitializeComponent();

			RegistryItemTreeViewBuilder.AddFallbackNodes(levelTreeView.Nodes, new BusinessObjectFactory(), addDefaultLevel: false, hideInactiveFallbacks: hideInactiveChildren);
		}

		public new RegistryImportBusinessObject BusinessEntity { get { return (RegistryImportBusinessObject)base.BusinessEntity; } }

		void browseButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.OpenBrowseForm(this);
		}

		void compareButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.OverrideLevel == null) // Cant use regular validation for non z-type
			{
				Globals.Message.ShowError(Res.GetString("319D75A1-4FF9-423B-95C0-F5CF3E2C8F49", "Please select a valid override level"));
			}
			else if (string.IsNullOrEmpty(BusinessEntity.FilePath))
			{
				Globals.Message.ShowError(Res.GetString("0165EF5D-3E90-4D76-A1D2-49259A88A0BC", "Please select a file to load"));
			}
			else
			{
				try
				{
					var loadedRegistryItems = BusinessEntity.LoadedRegistryItems;
					if (loadedRegistryItems == null)
					{
						Globals.Message.ShowError(Res.GetString("07DF438B-8FC2-4EE3-ACEE-E72272745072", "An error occurred while trying to load the registry items. Please select another file"));
					}
					else if (loadedRegistryItems.SuccessfulItems.Count == 0)
					{
						Globals.Message.ShowError(Res.GetString("28209E27-176A-4D6F-BCDA-4F8BE8C453BB", "No items were successfully loaded"));
					}
					else if (!BusinessEntity.Comparison.ItemsThatDifferBetweenBaseAndOverride.Any())
					{
						Globals.Message.ShowError(Res.GetString("93887F22-4B95-4F93-BE30-3E420DD87447", "The loaded registry items and their values at the selected level are identical. \r\nThere is no changes to view or apply."));
					}
					else if (loadedRegistryItems.FailedItems.Count == 0 || IgnoreFailedItems(loadedRegistryItems))
					{
						DialogResult = DialogResult.OK;
					}
				}
				catch (DirectoryNotFoundException)
				{
					Globals.Message.ShowError(Res.GetString("32851CED-7127-4C19-8109-71ADB39AD6E0", "The file could not be found. Please ensure the file exists and try again"));
				}
				catch (IOException)
				{
					Globals.Message.ShowError(Res.GetString("41F23B28-1347-4A2B-BE89-CBD856E31E05", "There was a problem loading the file. This may have occurred because the file is open in another program. \r\nPlease ensure the file is not open in another window, and then try again"));
				}
				catch (XmlException)
				{
					Globals.Message.ShowError(Res.GetString("AB402676-5CB0-4FA4-BA72-483FCCA0499E", "The file is corrupt and the values can not be loaded from the file"));
				}
				catch (InvalidOperationException)
				{
					Globals.Message.ShowError(Res.GetString("EFEBC353-E358-40FB-BC92-0EDBE4DF5C63", "The file you have imported contains elements not accepted by the current system. Please check the file and make sure that both the current system and the system that the file was exported from are running similar major versions of CargoWiseOne. If you importing registry items from older systems it is possible that the file contains some types that are not compatible with the existing registry."));
				}
				catch (RegistryDuplicateException ex)
				{
					Globals.Message.ShowError(Res.GetString("459C6D4A-4523-4B10-B330-0CDC14C86D52", @"The file you have imported contains elements which have same name.

{0}", ex.Message));
				}
			}
		}

		bool IgnoreFailedItems(LoadedRegistryItems loadedRegistryItems)
		{
			MultilingualString errorMessage = ResString.GetMultilingualString("67EABA26-F376-44F8-A3FD-B2B1441A61F6",
@"Some items could not be loaded ({0} out of {1} failed). 

Would you like to continue with the import?", loadedRegistryItems.FailedItems.Count, loadedRegistryItems.TotalCount);

			var context = new DialogDefaultContext(new ZGuid("F6A9A2E5-0312-41C8-B902-6B6409EF1107"),
				errorMessage,
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Error,
				DialogDefaultContext.ToZBlob("IgnoreFailuresInFile|" + BusinessEntity.FilePath));

			return Globals.Message.ShowOrDefault(context, errorMessage) == ZDialogResult.Yes;
		}

		void levelTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (levelTreeView.SelectedNode != null)
			{
				BusinessEntity.OverrideLevel = levelTreeView.SelectedNode.Tag as IOverrideLevel;
			}
		}
	}
}
