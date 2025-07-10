using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	internal sealed partial class ManifestPluginMenu : KMenuItem
	{
		public ManifestPluginMenu(CustomsJobVoyageWrapper voyageWrapper)
			: base(Res.GetString("50f8809d-43f3-42ac-a8fb-5573851f3b95", "Customs Manifest"))
		{
			if (voyageWrapper == null)
			{
				throw new ArgumentNullException(nameof(voyageWrapper));
			}

			this.voyageWrapper = voyageWrapper;

			SetupMenuItems();
		}

		void SetupMenuItems()
		{
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Customs.AU.ManifestPluginMenu.CreateExportManifests", "Create Export Manifests"), CreateExportManifest));
			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Customs.AU.ManifestPluginMenu.CreateImportManifests", "Create Import Manifests"), CreateImportManifest));
		}

		void CreateExportManifest(object sender, EventArgs args)
		{
			if (voyageWrapper.Voyage.HasChanges || !voyageWrapper.Voyage.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("0a337468-3f61-4404-9bc9-230a886d801c", "The Sailing Schedule should be saved before creating Export Manifest"));
			}
			else if (voyageWrapper.Voyage.JV_VoyageType.IsEmpty)
			{
				Globals.Message.Show(Res.GetString("0d324729-0ee2-4a3f-8db8-a0c088fbafeb", "The Voyage Type should be set up before creating Export Manifest"));
			}
			else
			{
				BusinessObjectFactory createFactory = new BusinessObjectFactory();
				createFactory.NameForDebugging = "Create Export Manifest Factory";

				ExportManifestFromSailingCreator creator = new ExportManifestFromSailingCreator(createFactory, voyageWrapper);
				ZFormModaliser.ShowDialogAndDispose(new ExportManifestImportForm(creator.CreateExportManifests()));
			}
		}

		void CreateImportManifest(object sender, EventArgs args)
		{
			if (voyageWrapper.Voyage.Sailings.Count > 0)
			{
				BusinessObjectFactory createFactory = new BusinessObjectFactory();
				createFactory.NameForDebugging = "Create Import Manifest Factory";

				ImportManifestFromSailingCreator creator = new ImportManifestFromSailingCreator(createFactory, voyageWrapper);
				TemporaryManifestsCollection manifests = creator.CreateImportManifests().Manifests;
				if (manifests.Count > 0)
				{
					CusSeaManTranHead importManifest = manifests[0].ImportManifest;
					ImportManifestItemCollection itemCollection = new ImportManifestItemCollection(importManifest, voyageWrapper.Voyage.Sailings[0], true);

					if (itemCollection.Count > 0)
					{
						ZFormModaliser.ShowDialogAndDispose(new ImportManifestImportForm(itemCollection));
						string messageString = itemCollection.SavedBillsCount != 1 ? " bills were added to the import manifest." : " bill was added to the import manifest.";

						Globals.Message.ShowInformation(itemCollection.SavedBillsCount.ToString() + messageString);
					}
					else
					{
						Globals.Message.ShowInformation("There are no Bills of Lading relating to this voyage that are not already in a customs import manifest.");
					}
				}
				else
				{
					Globals.Message.ShowInformation("There are no Bills of Lading relating to this voyage that are not already in a customs import manifest.");
				}
			}
			else
			{
				Globals.Message.ShowInformation("Sailing schedule should be created before!");
			}
		}

		readonly CustomsJobVoyageWrapper voyageWrapper;
	}
}
