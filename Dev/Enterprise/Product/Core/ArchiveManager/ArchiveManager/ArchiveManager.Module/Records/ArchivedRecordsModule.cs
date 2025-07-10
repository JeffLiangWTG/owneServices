using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.GUI.Records;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ArchiveManager.Module.Records
{
	public class ArchivedRecordsModule : ZFilterGridModule
	{
		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			result.Add(new ZMenuItem(ResString.GetMultilingualString("81cec382-e4a7-4ea1-b5d3-e1c07a9b7500", "Archive Offline"), new EventHandler(HandleArchiveOffline)));
			return result.ToArray();
		}

		void HandleArchiveOffline(object sender, EventArgs e)
		{
			if (Env.Security.ArchivedRecordsArchiveOffline.IsAllowed)
			{
				if (SystemDataRegistry.Instance.EnableOfflineArchiving.Value)
				{
					var mutex = new ZGlobalMutex(MutexIDs.ArchivingOffline);

					if (mutex.Lock())
					{
						var storage = new OfflineStorage();
						storage.ArchiveDateTo = ZDateTime.Now;
						var storageForm = new OfflineStorageForm(storage);
						storageForm.FormMutex = mutex;
						storageForm.Show();
					}
					else
					{
						var info = mutex.GetLockInfo();
						if (info != null)
						{
							Globals.Message.ShowError(Res.GetString("223b68c5-8f0e-4ee5-a58b-b10b6e9d9cb8", "Only one user is allowed to run Offline Archive at a time.\r\nUser: '{0}' is currently running Offline Archive, please try again later.", info.UserWithLock.GS_FullName));
						}
						else
						{
							Globals.Message.ShowError(Res.GetString("a165dc5a-b3df-46f9-8292-a7176395edb5", "Only one user is allowed to run Offline Archive at a time.\r\nThere is currently another user running Offline Archive, please try again later."));
						}
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("21D89F1B-977B-412E-AA2F-D521E705E5A9", "The Archive Offline functionality is currently disabled. Archive Offline can be enabled in the Registry at:  System > Archive Manager > Enable Offline Archiving.\r\nPlease note that the Archive Offline functionality is only available to self-hosted clients."));
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.ArchivedRecordsArchiveOffline);
			}
		}

		public void HandleArchiveOffline_ForTestOnly(object sender, EventArgs e)
			=> HandleArchiveOffline(sender, e);

		protected override BusinessObjectFactory GetNewFactory()
			=> new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			=> ZControllerFactory.Create(ControllerIDs.ArchivedRecords);

		protected override FilterBusinessObject GetNewFilterBusinessObject()
			=> new ArchivedRecordsFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
			=> new ArchivedRecordsFilterControl(GridCollection, (ArchivedRecordsFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
			=> new ArchiveStorageMainCollection(Factory);

		public override ModuleIdentifier ID
			=> ModuleIDs.ArchivedRecords;

		protected override LicenceCheckpoint LicenceCheckPointCore
			=> Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint
			=> Env.Security.ArchivedRecords;

		public override bool AllowNew
			=> false;

		public override bool AllowDelete
			=> false;

		public override bool AllowEdit
			=> false;
	}
}
