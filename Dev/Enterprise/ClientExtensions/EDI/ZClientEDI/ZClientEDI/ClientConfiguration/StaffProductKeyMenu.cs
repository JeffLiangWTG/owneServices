using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.ClientConfiguration
{
	public static class StaffProductKeyMenu
	{
		public static MultilingualString CreateNewKeyCaption => ResString.GetMultilingualString("60A2FFC9-6FB1-4F08-9F8A-FB01565CB5C9", "Create New Key");

		public static MultilingualString ErrorUnregisteringProductKeyMessage => ResString.GetMultilingualString("18E7F43E-92FF-4095-B5CC-3DE836065944", "An error occurred while unregistering the product key.");

		public static MultilingualString MyCwProductKeysCaption => ResString.GetMultilingualString("32353ac9-8061-4589-a4da-4f3238fbe6e7", "My CW Product Keys");

		public static MultilingualString ProductKeyHasBeenUnregisteredMessage => ResString.GetMultilingualString("209E8B62-8546-4654-851F-3252218B6BF6", "Product Key has been unregistered.");

		public static MultilingualString ReasonMessage => ResString.GetMultilingualString("6071B3EF-3617-46D8-A3D1-8F54BB369AD0", "Reason: ");

		public static MultilingualString UnregisteredCaption => ResString.GetMultilingualString("EAC51B6B-249D-451B-8A66-83FBAF0D791A", "Unregister");

		public static ZToolStripMenuItem Create()
		{
			var myCw1ProductKeysMenuItem = new ZToolStripMenuItem(MyCwProductKeysCaption);
			myCw1ProductKeysMenuItem.DropDownOpening += MyCw1ProductKeysMenuItem_DropDownOpening;
			return myCw1ProductKeysMenuItem;
		}

		static void CreateNewKey_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new CreateNewProductKeyForm());
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static void MyCw1ProductKeysMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			var myCw1ProductKeyToolStripMenuItem = (ZToolStripMenuItem)sender;
			myCw1ProductKeyToolStripMenuItem.DropDownItems.Clear();

			foreach (var db in LicenceDatabase.GetAllProductKeysForUser(GlbStaff.CurrentUser.GS_Code).OrderBy(x => x.LicEnterprise.LE_EnterpriseCode).ThenBy(x => x.LD_ServerCode))
			{
				var subMenuItemText = $"{db.LicEnterprise.LE_EnterpriseCode} {db.LD_ServerCode} {RegistrationStatusDescription(db)}".TrimEnd();
				var subMenuItem = new ZToolStripMenuItem(subMenuItemText);
				subMenuItem.DropDownItems.Add(new ZToolStripMenuItem(UnregisteredCaption, Unregister_Click) { Tag = db.PK });
				myCw1ProductKeyToolStripMenuItem.DropDownItems.Add(subMenuItem);
			}

			myCw1ProductKeyToolStripMenuItem.DropDownItems.Add(new ZToolStripMenuItem(CreateNewKeyCaption, CreateNewKey_Click));
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static void Unregister_Click(object sender, EventArgs e)
		{
			var menuItem = (ZToolStripMenuItem)sender;
			var licenceDatabasePk = (ZGuid)menuItem.Tag;

			try
			{
				LicenceDatabase.Unregister(licenceDatabasePk);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError($"{ErrorUnregisteringProductKeyMessage}\r\n{ReasonMessage}{ex.Message}");
				return;
			}

			Globals.Message.ShowInformation(ProductKeyHasBeenUnregisteredMessage);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static string RegistrationStatusDescription(LicenceDatabase db)
		{
			var registrationStatusDescription = db.Lookups.StatusList.GetDescriptionFromCode(db.LD_Status);
			if (!string.IsNullOrEmpty(registrationStatusDescription))
			{
				registrationStatusDescription = $"- {registrationStatusDescription}";
			}
			return registrationStatusDescription;
		}
	}
}
