using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	// NB, hosted by ForwardingShipment or EU JobDeclaration
	public class ExitSummaryPlugIn : CustomsPlugIn
	{
		public ExitSummaryPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
			ChangeTheVisibility();
		}

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			ChangeTheVisibility();
		}

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			if (HostBusinessEntity is JobDeclaration declaration)
			{
				declaration.JE_MessageTypeInfo.ValueChanged += SourceKeyFields_ValueChanged;
			}
			else if (HostBusinessEntity is ForwardingShipment shipment)
			{
				shipment.JS_RL_NKDestinationInfo.ValueChanged += SourceKeyFields_ValueChanged;
				shipment.JS_RL_NKOriginInfo.ValueChanged += SourceKeyFields_ValueChanged;
			}
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			if (HostBusinessEntity is JobDeclaration declaration)
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= SourceKeyFields_ValueChanged;
			}
			else if (HostBusinessEntity is ForwardingShipment shipment)
			{
				shipment.JS_RL_NKDestinationInfo.ValueChanged -= SourceKeyFields_ValueChanged;
				shipment.JS_RL_NKOriginInfo.ValueChanged -= SourceKeyFields_ValueChanged;
			}
		}

		void SourceKeyFields_ValueChanged(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		public override string Name => (NoResString)"EU Exit Summary";

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override Control GetNewUserControl() => new ExitSummaryUserControl();

		protected override IBusiness GetBusinessEntityForPlugIn() => ExitHeader;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore() => ExitHeader != null;

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			CreateExitHeaderIfRequired();
			if (fMainMenuItem != null && fMainMenuItem.ExitHeader != ExitHeader)
			{
				fMainMenuItem.ExitHeader = ExitHeader;
			}
			return ExitHeader != null;
		}

		public override ZString PlugInNotDisplayedMessage => coveringLabel;

		ZString coveringLabel;

		protected void CreateExitHeaderIfRequired()
		{
			if (ExitHeader == null)
			{
				if (!Mutex.IsLocked)
				{
					if (GetConfirmationFromUsersToRunExitSummary(Res.GetString("484D72AB-262D-402B-986B-3FAAC86F51D9", "Are you sure that you want to create the Exit Declaration?")) == DialogResult.Yes)
					{
						if (!CreateExitHeader())
						{
							coveringLabel = MutexLockText;
						}
					}
					else
					{
						coveringLabel = Res.GetString("13C3F8EF-89E8-48FB-A456-3EAE203E86E7", "The Exit Declaration has not been created.");
					}
				}
				else
				{
					coveringLabel = MutexLockText;
				}
			}
		}

		public ZBool CreateExitHeader()
		{
			ZBool result = Mutex.Lock();
			if (result)
			{
				var hostEntity = (BusinessObject)HostBusinessEntity;
				fExitHeader = hostEntity.Factory.New<CusExitControlHeader>();
				fExitHeader.CEH_ParentID = hostEntity.PK;
				fExitHeader.CEH_ParentTableCode = hostEntity.TablePrefix;
				hostEntity.RegisterEditableChildObject(fExitHeader);

				if (!Globals.IsTest)
				{
					ExitHeader.HasChanges = true;
				}
			}
			return result;
		}

		protected DialogResult GetConfirmationFromUsersToRunExitSummary(string question)
		{
			return Globals.Message.Show(
						question,
						Res.GetString("85B63955-7661-4264-AC95-68FAB7CA6488", "Exit Summary"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);
		}

		protected override void ChangeTheVisibilityCore()
		{
			var result = GlbCompany.CurrentCompany.Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion
				&& ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>().IsExitControlPluginEnabledForCurrentCompany;

			if (HostBusinessEntity is JobDeclaration declaration)
			{
				result = result && !declaration.IsImport;
			}
			else if (HostBusinessEntity is ForwardingShipment shipment)
			{
				result = result && shipment.IsExport();
			}
			Enabled = result;
		}

		#region Mutex
		public ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null)
				{
					fMutex = new ZGlobalMutex(MutexIDs.CusExitControlHeaderMutex, ((BusinessObject)HostBusinessEntity).PK.ToString());
				}
				return fMutex;
			}
		}
		ZGlobalMutex fMutex;

		static string MutexLockText
			=> Res.GetString("8E7A615E-E6AB-46A0-9D64-84D0224239E6",
				"Someone else is already in the process of creating an Exit Summary.\r\nYou should be able to access the Exit Summary when the person has saved the record. Please try later.");

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

		public CusExitControlHeader ExitHeader
		{
			get
			{
				if (fExitHeader == null)
				{
					var hostEntity = (BusinessObject)HostBusinessEntity;
					fExitHeader = hostEntity.Factory.LoadTop1<CusExitControlHeader>(new ZQuery(CusExitControlHeaderSchema.CEH_ParentID, SQLComparisonOperator.Equal, hostEntity.PK));
				}
				return fExitHeader;
			}
		}
		CusExitControlHeader fExitHeader;

		#region Menu

		protected override bool ShouldPluginDropdownMenuBeCreated() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();

		protected override MenuItem GetNewTopLevelMenu()
		{
			return fMainMenuItem ?? (fMainMenuItem = new EcsMessagingMenu
			{
				ExitHeader = ExitHeader
			});
		}
		protected EcsMessagingMenu fMainMenuItem;

		#endregion
	}
}
