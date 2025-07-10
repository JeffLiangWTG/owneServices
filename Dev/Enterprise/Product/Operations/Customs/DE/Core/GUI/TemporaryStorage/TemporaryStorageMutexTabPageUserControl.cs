using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.GUI
{
	public abstract partial class TemporaryStorageMutexTabPageUserControl : ZUserControl
	{
		protected TemporaryStorageMutexTabPageUserControl()
		{
			InitializeComponent();
		}

		public new CusTempStorageJobHeader CurrentDataItem => (CusTempStorageJobHeader)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				UnlockMutexIfLockedByThisInstance();
				CurrentDataItem.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
			base.OnCurrentDataItemChanged(e);
		}

		public void LoadUserControl()
		{
			if (CurrentDataItem != null)
			{
				if (!CusTempStorageDecExists())
				{
					if (Mutex != null && !Mutex.IsLocked)
					{
						if (QueryUserToCreateCusTempStorageDec())
						{
							if (CreateCusTempStorageDec())
							{
								ShowCusTempStorageDecControl();
							}
							else
							{
								ShowCoveringLabel(MutexLockText);
							}
						}
						else
						{
							ShowCoveringLabel(NotToCreateJobText);
						}
					}
					else
					{
						ShowCoveringLabel(MutexLockText);
					}
				}
				else
				{
					ShowCusTempStorageDecControl();
				}
			}
			else
			{
				ShowCoveringLabel(NotToCreateJobText);
			}
		}

		bool CreateCusTempStorageDec()
		{
			ZBool result = Mutex.Lock();
			if (result)
			{
				var cusTempStorageDec = CusTempStorageDecReload() ?? CreateNewCusTempStorageDec();
				CurrentDataItem.CusTempStorageDecs.Add(cusTempStorageDec);
				CurrentDataItem.HasChanges = true;
			}
			return result;
		}

		protected abstract CusTempStorageDec CreateNewCusTempStorageDec();

		bool CusTempStorageDecExists()
		{
			var result = CusTempStorageDecReload() != null;
			if (result)
			{
				Controls.Add(CusTempStorageDecControl);
			}
			return result;
		}

		CusTempStorageDec CusTempStorageDecReload()
		{
			var query = new ZQuery(CusTempStorageDecSchema.STH_SJH, CurrentDataItem.PK);
			query.AddToFilter(CusTempStorageDecSchema.STH_DeclarationType, DeclarationType);
			query.OrderBy = CusTempStorageDecSchema.STH_SystemCreateTimeUtc.Name;
			query.FetchOnlyFromLocalCache = !CurrentDataItem.IsInDatabase;
			query.ReLoadExistingRows = CurrentDataItem.IsInDatabase;
			return CurrentDataItem.Factory.LoadTop1<CusTempStorageDec>(query);
		}

		protected abstract ZString DeclarationType { get; }

		protected abstract MutexID MutexID { get; }

		#region Mutex

		ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null && CurrentDataItem != null)
				{
					fMutex = new ZGlobalMutex(MutexID, CurrentDataItem.PK.ToString());
				}
				return fMutex;
			}
		}
		ZGlobalMutex fMutex;
		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				UnlockMutexIfLockedByThisInstance();
			}
		}

		void UnlockMutexIfLockedByThisInstance()
		{
			if (Mutex != null && Mutex.HasLock)
			{
				Mutex.Unlock();
				((IDisposable)fMutex).Dispose();
				fMutex = null;
			}
		}

		#endregion

		#region Controls

		void ShowCoveringLabel(string text)
		{
			CoveringLabel.Text = text;
			CoveringLabel.Visible = true;
			if (fCusTempStorageDecControl != null)
			{
				fCusTempStorageDecControl.Visible = false;
			}
		}

		protected abstract ZUserControl GetCusTempStorageDecControl();

		ZUserControl CusTempStorageDecControl
		{
			get
			{
				if (fCusTempStorageDecControl == null)
				{
					fCusTempStorageDecControl = GetCusTempStorageDecControl();
				}
				return fCusTempStorageDecControl;
			}
		}
		ZUserControl fCusTempStorageDecControl;

		void ShowCusTempStorageDecControl()
		{
			if (!Controls.Contains(CusTempStorageDecControl))
			{
				Controls.Add(CusTempStorageDecControl);
			}
			CusTempStorageDecControl.Visible = true;
			CoveringLabel.Visible = false;
		}

		#endregion

		#region User Display Text

		protected abstract ZString DeclarationTypeDescription { get; }

		bool QueryUserToCreateCusTempStorageDec()
		{
			return Globals.Message.Show(
				CreateJobQueryText,
				Res.GetString("CusTempStorageDec|QueryUserToCreateCusTempStorageDec", "Create {0} Declaration for this job", DeclarationTypeDescription),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Information) == DialogResult.Yes;
		}

		string MutexLockText
		{
			get { return Res.GetString("CusTempStorageDec|MutexLockText", "Someone else is already in the process of creating a {0} Declaration for this {0} job.\r\nYou should be able to access the {0} Declaration when the person has saved or canceled. Please try later.", DeclarationTypeDescription); }
		}

		string NotToCreateJobText
		{
			get { return Res.GetString("CusTempStorageDec|NotToCreateJobText", "You have chosen not to create a {0} Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a {0} Declaration.", DeclarationTypeDescription); }
		}

		string CreateJobQueryText
		{
			get { return Res.GetString("CusTempStorageDec|CreateJobQueryText", "Are you sure you want to create a {0} Declaration now?", DeclarationTypeDescription); }
		}

		#endregion

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if ((components != null))
				{
					components.Dispose();
				}
				if ((fMutex != null))
				{
					((IDisposable)fMutex).Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
