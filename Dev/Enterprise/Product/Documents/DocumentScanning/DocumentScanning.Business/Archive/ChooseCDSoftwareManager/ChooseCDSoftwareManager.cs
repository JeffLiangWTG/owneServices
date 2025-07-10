using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public class ChooseCDSoftwareManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ChooseCDSoftwareManager(DocumentFactory factory, ArchiveEDocsManager archiver)
			: base(factory)
		{
			fArchiveManager = archiver;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			UseOwn = true;
		}

		#region MasterFactory

		public DocumentFactory MasterFactory
		{
			get { return (DocumentFactory)Factory; }
		}

		#endregion

		#region Archive Manager

		public ArchiveEDocsManager ArchiveManager
		{
			get { return fArchiveManager; }
		}

		readonly ArchiveEDocsManager fArchiveManager;

		#endregion

		#region UseXP

		public ZBool UseXP
		{
			get { return fUseXP; }
			set
			{
				fUseXP = value;
				fUseOwn = !value;
				UseXPInfo.RefreshBinding();
			}
		}

		ZBool fUseXP;

		public ZPropertyInfo UseXPInfo
		{
			get { return GetZPropertyInfo(nameof(UseXP)); }
		}

		#endregion

		#region UseOwn

		public ZBool UseOwn
		{
			get { return fUseOwn; }
			set
			{
				fUseOwn = value;
				fUseXP = !value;
				UseOwnInfo.RefreshBinding();
			}
		}

		ZBool fUseOwn;

		public ZPropertyInfo UseOwnInfo
		{
			get { return GetZPropertyInfo(nameof(UseOwn)); }
		}

		#endregion

		#region UserSoftware

		public UserChoiceCDWriter UserSoftware
		{
			get
			{
				if (fUserSoftware == null)
				{
					fUserSoftware = new UserChoiceCDWriter(MasterFactory, ArchiveManager);
				}
				return fUserSoftware;
			}
		}

		UserChoiceCDWriter fUserSoftware;

		#endregion

		public CDWriter SelectedSoftware
		{
			get
			{
				return UserSoftware;
			}
		}

		public void DisableOptionsNotAvailable()
		{
		}

		public void DisableSoftwareNotUsed()
		{
		}
	}
}
