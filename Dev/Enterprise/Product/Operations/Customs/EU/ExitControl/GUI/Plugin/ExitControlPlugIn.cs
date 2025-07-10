using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.ExitControl.Business;
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

namespace Enterprise.Customs.EU.ExitControl.GUI.PlugIn
{
	// NB, hosted by ForwardingShipment or EU JobDeclaration
	public class ExitControlPlugIn : CustomsPlugIn
	{
		public ExitControlPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
			ChangeTheVisibility();
		}

		public override void OnUserControlShown()
		{
			if (needToReDefaultData && ExitHeader is CusExitHeader header)
			{
				header.DefaultDataFromParent(false);
			}
			base.OnUserControlShown();
			ChangeTheVisibility();
			var parent = ExitHeader?.Parent;
			needToReDefaultData = parent is ForwardingShipment || parent is JobDeclaration;
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
			else if (HostBusinessEntity is ForwardingConsol consol)
			{
				consol.JK_RL_NKLoadPortInfo.ValueChanged += SourceKeyFields_ValueChanged;
				consol.JK_RL_NKDischargePortInfo.ValueChanged += SourceKeyFields_ValueChanged;
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
			else if (HostBusinessEntity is ForwardingConsol consol)
			{
				consol.JK_RL_NKLoadPortInfo.ValueChanged -= SourceKeyFields_ValueChanged;
				consol.JK_RL_NKDischargePortInfo.ValueChanged -= SourceKeyFields_ValueChanged;
			}
		}

		void SourceKeyFields_ValueChanged(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		public override string Name => (NoResString)"EU Exit Control";

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override Control GetNewUserControl()
		{
			_ = TopLevelMenu;
			var exitControlUserControl = new ExitControlUserControl();
			exitControlUserControl.SetExitControlMenuItem(fMainMenuItem);
			return exitControlUserControl;
		}

		protected override IBusiness GetBusinessEntityForPlugIn() => ExitHeader;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore() => ExitHeader != null;

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			CreateExitHeaderIfRequired();
			SetMainMenuItem();
			return ShouldPlugInGUIAndBusinessEntityBeCreatedCore();
		}

		public override ZString PlugInNotDisplayedMessage => coveringLabel;

		ZString coveringLabel;

		protected void CreateExitHeaderIfRequired()
		{
			if (ExitHeader == null)
			{
				if (!Mutex.IsLocked)
				{
					if (GetConfirmationFromUsersToRunExitSummary(Res.GetString("7F07F0D2-BAFD-4C5E-9E12-7284C7C55886", "Are you sure that you want to create the Exit Declaration?")) == DialogResult.Yes)
					{
						var lockedSuccessfully = Mutex.Lock();
						if (lockedSuccessfully)
						{
							var query = new ZQuery(CusExitHeaderSchema.CXH_ParentID, SQLComparisonOperator.Equal, ((BusinessObject)HostBusinessEntity).PK);
							query.AddToFilter(CusExitHeaderSchema.CXH_ApplicationCode, SQLComparisonOperator.Equal, CusExitHeaderApplicationCodeList.Codes.ExitControl);
							var recentlyCreatedExitHeader = new ReadOnlyBusinessObjectFactory().LoadTop1<CusExitHeader>(query);
							if (recentlyCreatedExitHeader == null)
							{
								CreateExitHeader();
							}
							else
							{
								coveringLabel = Res.GetString("58F444DC-405D-417F-8784-92B1EF5A6A09", "In the meantime an Exit Declaration has been created and linked to this Shipment. Please reload to access it.");
							}
						}
						else
						{
							coveringLabel = MutexLockText;
						}
						Mutex.Unlock();
					}
					else
					{
						coveringLabel = Res.GetString("B1D34981-DEBE-4848-B2C1-2C19CB85FC34", "The Exit Declaration has not been created.");
					}
				}
				else
				{
					coveringLabel = MutexLockText;
				}
			}
		}

		void SetMainMenuItem()
		{
			if (fMainMenuItem != null && ExitHeader is CusExitHeader header && fMainMenuItem.ExitHeader != header)
			{
				fMainMenuItem.ExitHeader = header;
			}
		}

		void CreateExitHeader()
			{
				var hostEntity = (BusinessObject)HostBusinessEntity;
				fExitHeader = NewExitHeader(hostEntity);
				fExitHeader.CXH_ParentID = hostEntity.PK;
				fExitHeader.CXH_ParentTableCode = hostEntity.TablePrefix;
				fExitHeader.DefaultDataFromParent(true);
				needToReDefaultData = false;
				hostEntity.RegisterEditableChildObject(fExitHeader);

				if (!Globals.IsTest)
				{
					ExitHeader.HasChanges = true;
				}
			}

		protected virtual CusExitHeader NewExitHeader(BusinessObject hostEntity) => hostEntity.Factory.New<CusExitHeader>();

		bool needToReDefaultData;

		protected DialogResult GetConfirmationFromUsersToRunExitSummary(string question)
		{
			return Globals.Message.Show(
						question,
						Res.GetString("423D37AF-1339-4864-8713-0986D44A1C72", "Exit Control"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);
		}

		protected override void ChangeTheVisibilityCore()
		{
			var result = GlbCompany.CurrentCompany.Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion
				&& ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>().IsExitControlPluginEnabledForCurrentCompany;

			if (HostBusinessEntity is JobDeclaration declaration)
			{
				result = result && declaration.ExitControlTabVisible;
			}
			else if (HostBusinessEntity is ForwardingShipment shipment)
			{
				result = result && (shipment.IsExport() || shipment.IsCrossTrade());
			}
			else if (HostBusinessEntity is ForwardingConsol consol)
			{
				result = result && (consol.IsExport() || consol.IsCrossTrade());
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
			=> Res.GetString("F7FEAFEE-4B3D-4185-9212-4CE94A140D62",
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

		public CusExitHeader ExitHeader
		{
			get
			{
				if (fExitHeader == null)
				{
					var hostEntity = (BusinessObject)HostBusinessEntity;
					var query = new ZQuery(CusExitHeaderSchema.CXH_ParentID, SQLComparisonOperator.Equal, hostEntity.PK);
					query.AddToFilter(CusExitHeaderSchema.CXH_ApplicationCode, SQLComparisonOperator.Equal, CusExitHeaderApplicationCodeList.Codes.ExitControl);
					fExitHeader = hostEntity.Factory.LoadTop1<CusExitHeader>(query);
					needToReDefaultData = fExitHeader != null;
				}
				return fExitHeader;
			}
		}
		CusExitHeader fExitHeader;

		#region Menu

		protected override bool ShouldPluginDropdownMenuBeCreated() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();

		protected override MenuItem GetNewTopLevelMenu()
		{
			return fMainMenuItem ?? (fMainMenuItem = new ExitControlMenuItem { ExitHeader = ExitHeader });
		}
		ExitControlMenuItem fMainMenuItem;

		#endregion
	}
}
