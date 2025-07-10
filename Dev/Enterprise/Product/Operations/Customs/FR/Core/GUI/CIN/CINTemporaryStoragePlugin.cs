using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.GUI.CusTempStorage;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.GUI.CIN
{
	public class CINTemporaryStoragePlugin : CustomsManifestPlugIn
	{
		public CINTemporaryStoragePlugin(IBusiness hostEntity)
			: base(hostEntity as ForwardingConsol)
		{
			Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);

			if (Consol != null)
			{
				if (Enabled && Header != null)
				{
					Consol.RegisterEditableChildObject(Header);

					if (Header.RelatedBusinessObject == null)
					{
						Header.SetRelatedBusinessObject(Consol, Core.Constants.GenPivotTypes.CusStorageHeaderConsol);
					}
				}
			}
		}

		public CusTempStorageJobHeader Header
		{
			get { return base.BusinessEntity as CusTempStorageJobHeader; }
		}

		protected CusTempStorageJobHeader InternalHeader
		{
			get
			{
				if (fInternalHeader == null)
				{
					fInternalHeader = GetHeader();
					if (fInternalHeader != null)
					{
						StartPlugInSynchroniser();
					}
				}
				return fInternalHeader;
			}
			set
			{
				fInternalHeader = value;
				if (fInternalHeader != null)
				{
					fInternalHeader.SetRelatedBusinessObject(Consol, Core.Constants.GenPivotTypes.CusStorageHeaderConsol);
					if (fMainMenuItem != null)
					{
						fMainMenuItem.Header = fInternalHeader;
					}
				}
			}
		}
		CusTempStorageJobHeader fInternalHeader;

		ForwardingConsol Consol
		{
			get { return (ForwardingConsol)ManifestProvider; }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (fMainMenuItem == null)
			{
				fMainMenuItem = new CusTempStorageFormMenu() { Header = Header, Caption = ResString.GetMultilingualString("B69FA5FF-9C2C-4873-A3A9-14F5195D52C2", "&CIN") };
			}
			return fMainMenuItem;
		}
		protected CusTempStorageFormMenu fMainMenuItem;

		protected override bool ShouldPluginDropdownMenuBeCreated()
		{
			return QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return InternalHeader;
		}

		protected override Control GetNewUserControl()
		{
			return new CINTemporyStorageUserControlForPlugin();
		}

		protected override ZTabPagePlugIn GetTabPage()
		{
			var tabPage = base.GetTabPage();
			tabPage.ExcludeFromBindingOnSave = true;
			return tabPage;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		public override string Name
		{
			get { return "CINTemporaryStoragePlugin"; }
		}

		protected override ZBool IsActive
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Broker; }
		}

		protected override void ChangeTheVisibilityCore()
		{
			this.Enabled = false;
			if (Consol != null)
			{
				this.Enabled = Consol.JK_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.France, StringComparison.Ordinal) && Consol.IsAir;
				if (Enabled && Header != null)
				{
					if (Header.RelatedBusinessObject == null)
					{
						Header.SetRelatedBusinessObject(Consol, Core.Constants.GenPivotTypes.CusStorageHeaderConsol);
					}
					Header.ConsolSynchroniser.SetEnabled(Enabled, Header.ConsolSynchroniser.DetectEnabled);
				}
			}
		}

		#region Create new CIN Temporary Storage
		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			QueryAddTemporaryStorage();
			if (fMainMenuItem != null && fMainMenuItem.Header != Header)
			{
				fMainMenuItem.Header = Header;
			}
			return Header != null;
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return coveringLabelText; }
		}

		protected ZString coveringLabelText;

		protected void QueryAddTemporaryStorage()
		{
			coveringLabelText = string.Empty;

			if (Header == null)
			{
				if (!Mutex.IsLocked)
				{
					if (QueryUserToCreateTemporaryStorage())
					{
						if (!CreateTemporaryStorage())
						{
							coveringLabelText = MutexLockText;
						}
					}
					else
					{
						coveringLabelText = NotToCreateJobText;
					}
				}
				else
				{
					coveringLabelText = MutexLockText;
				}
			}
		}

		public ZBool CreateTemporaryStorage()
		{
			ZBool result = Mutex.Lock();
			if (result)
			{
				CreateNewTemporaryStorage();
				StartPlugInSynchroniser();
				if (!Globals.IsTest)
				{
					Header.HasChanges = true;
				}
			}
			return result;
		}

		void CreateNewTemporaryStorage()
		{
			var header = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeFRC);
			var frcStorageDec = header.CusTempStorageDec;
			InternalHeader = header;

			if (Consol != null)
			{
				if (!Consol.JK_MasterBillNum.IsEmpty)
				{
					var awb = frcStorageDec.CusTempStorageLines.AddNew();
					awb.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ORT_AWB;
					awb.TSL_OwnerReferenceNumber = Consol.JK_MasterBillNum;
				}

				if (Consol.Shipments.Any())
				{
					foreach (ForwardingShipment shipment in Consol.Shipments)
					{
						if (!shipment.JS_HouseBill.IsEmpty)
						{
							var hwb = frcStorageDec.CusTempStorageLines.AddNew();
							hwb.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.ORT_HWB;
							hwb.TSL_OwnerReferenceNumber = shipment.JS_HouseBill;
						}
					}
				}
			}
		}

		void StartPlugInSynchroniser()
		{
			if (Enabled)
			{
				if (Header != null)
				{
					Header.SynchroniseWithParentIfNeeded();
				}
			}
		}

		protected bool QueryUserToCreateTemporaryStorage()
		{
			return QueryUser(Res.GetString("3F1FA7F1-B3EE-4B4E-8344-29402B3D2B8B", "Are you sure you want to create a CIN Temporary Storage now?"), Res.GetString("88AC5060-5C00-46B4-8F41-B59BCF93D037", "Create CIN Temporary Storage for Consol"));
		}

		protected bool QueryUser(string question, string caption)
		{
			var result = Globals.Message.Show(question, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			return (result == DialogResult.Yes);
		}

		protected static string NotToCreateJobText
		{
			get { return Res.GetString("D1CB5BD9-9C40-4A19-AFA4-4502020BD7C1", "You have chosen not to create a CIN Temporary Storage now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a CIN Temporary Storage for this Job."); }
		}
		#endregion

		#region For plugin in Consol

		protected CusTempStorageJobHeader GetHeader()
		{
			CusTempStorageJobHeader header = null;

			if (Consol != null)
			{
				var headerID = GetCusTemporary(Consol);
				if (headerID.IsValid)
				{
					header = Factory.Load<CusTempStorageJobHeader>(headerID);
				}
			}
			return header;
		}

		ZGuid GetCusTemporary(ForwardingConsol consol)
		{
			var pivot = GenPivot.LoadRelation2Pivot(consol, Core.Constants.GenPivotTypes.CusStorageHeaderConsol);
			return pivot != null ? pivot.XX_Relation1ID : ZGuid.Empty;
		}
		#endregion

		#region Mutex
		public ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null)
				{
					fMutex = new ZGlobalMutex(MutexIDs.CusTemporaryStorageMutex, Consol.PK.ToString());
				}
				return fMutex;
			}
		}
		ZGlobalMutex fMutex;

		static string MutexLockText
		{
			get { return Res.GetString("8424B54D-C232-43C3-B8B6-963827E5B17A", "Someone else is already in the process of creating a CIN Temporary Storage for this Consol.\r\nYou should be able to access the CIN Temporary Storage when the person has saved the record. Please try later."); }
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				UnlockMutexIfLockedByThisInstance();
			}
		}

		void UnlockMutexIfLockedByThisInstance()
		{
			if (Mutex.HasLock)
			{
				Mutex.Unlock();
			}
		}

		void UnHookFactorySaveEvent()
		{
			Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					UnlockMutexIfLockedByThisInstance();
					UnHookFactorySaveEvent();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		#endregion
	}
}
