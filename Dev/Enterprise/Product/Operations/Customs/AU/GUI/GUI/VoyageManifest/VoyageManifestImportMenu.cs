using System;
using CargoWise.Windows.UI;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class VoyageManifestImportMenu : KMenuItem
	{
		protected VoyageManifestImportMenu()
		{
			this.Text = "Import Data";
			SetupMenuItems();
			SetupOurMenuItemsIfNone();
		}

		#region Delegate Constructor

		protected delegate VoyageManifestImportMenu ConstructorDelegate();

		[ThreadStatic]
		protected static ConstructorDelegate constructor;

		public static VoyageManifestImportMenu New()
		{
			VoyageManifestImportMenu result = null;

			if (constructor == null)
			{
				result = new VoyageManifestImportMenu();
			}
			else
			{
				result = constructor();
			}

			return result;
		}

		#endregion

		#region SetupMenuItems

		protected virtual void SetupMenuItems()
		{
		}

		#endregion

		#region TranHead

		protected internal CusSeaManTranHead TranHead
		{
			set { fTranHead = value; }
			get { return fTranHead; }
		}
		CusSeaManTranHead fTranHead;

		#endregion

		#region Implementation

		void SetupOurMenuItemsIfNone()
		{
			if (MenuItems.Count == 0)
			{
				MenuItems.Add(new ZMenuItem("Import ShipNET", new EventHandler(OnClickShipNetEvent)));
			}
		}

#if DEBUG
		protected virtual
#endif
 void OnClickShipNetEvent(object sender, EventArgs args)
		{
			using (var form = DataImporterForm.Create(BillingInterfaceName.ShipNETImport))
			{
				form.Importer = new ShipnetFlatFileDataImporter(TranHead);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		#endregion
	}
}
