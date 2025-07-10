using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class CAConsolACIMenu : MessageManagementMenu
	{
		protected internal CAConsolACIMenu(CusSCAOceanBill oceanBill, ConsolACIMessageManager manager)
			: base(manager)
		{
			Text = "ACI";
			_oceanBill = oceanBill;
		}

		protected internal CAConsolACIMenu(ForwardingConsol consol, ConsolACIMessageManager manager)
			: base(manager)
		{
			Text = "ACI";
			this.consol = consol;
		}

		protected delegate CAConsolACIMenu NewCusSCAOceanBillDelegate(CusSCAOceanBill oceanBill, ConsolACIMessageManager manager);
		[ThreadStatic]
		protected static NewCusSCAOceanBillDelegate overriddenNewCusSCAOceanBillDelegate;
		public static CAConsolACIMenu New(CusSCAOceanBill oceanBill, ConsolACIMessageManager manager)
		{
			CAConsolACIMenu result;
			if (overriddenNewCusSCAOceanBillDelegate == null)
			{
				result = new CAConsolACIMenu(oceanBill, manager);
			}
			else
			{
				result = overriddenNewCusSCAOceanBillDelegate(oceanBill, manager);
			}
			return result;
		}

		protected delegate CAConsolACIMenu NewConsolDelegate(ForwardingConsol consol, ConsolACIMessageManager manager);
		[ThreadStatic]
		protected static NewConsolDelegate overriddenNewConsolDelegate;

		public static CAConsolACIMenu New(ForwardingConsol consol, ConsolACIMessageManager manager)
		{
			CAConsolACIMenu result;
			if (overriddenNewConsolDelegate == null)
			{
				result = new CAConsolACIMenu(consol, manager);
			}
			else
			{
				result = overriddenNewConsolDelegate(consol, manager);
			}
			return result;
		}

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			if (consol != null)
			{
				SetupMenuItems();
			}
			else
			{
				ConfigureMenuForLicenceOrSecurityDenied();
			}
		}

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			SetMenuVisibility();
		}

		void SetupMenuItems()
		{
			refreshAll = new ZMenuItem(ResString.GetMultilingualString("1207f09d-47ec-4b90-b25e-d77a9c3e3be0", "Refresh ACI Data"), refreshAll_Click);
			MenuItems.Add(refreshAll);
		}
		internal MenuItem refreshAll;

		void refreshAll_Click(object sender, EventArgs e)
		{
			if (IsMessagingAllowed && OceanBill != null && !OceanBill.IsDeleted && consol != null)
			{
				Manager.RefreshAll((IConsolSendsMessagesToCustoms)Sender);
			}
		}

		protected override ISendsMessagesToCustoms Sender
		{
			get { return new MessagingActionsController(); }
		}

		protected CusSCAOceanBill OceanBill
		{
			get
			{
				if (_oceanBill == null)
				{
					_oceanBill = Manager.OceanBill;
				}
				return _oceanBill;
			}
		}
		CusSCAOceanBill _oceanBill;

		void SetMenuVisibility()
		{
			sendMessages.Visible = CurrentCheckPoint.IsAllowed;
			withdrawMessages.Visible = CurrentCheckPoint.IsAllowed;
			resetToOriginal.Visible = CurrentCheckPoint.IsAllowed;
		}

		SecurityCheckpoint CurrentCheckPoint
		{
			get { return Env.Security.ACIReportNew; }
		}

		void ConfigureMenuForLicenceOrSecurityDenied()
		{
			MenuItems.Clear();
			if (accessDeniedMenuItem == null)
			{
				accessDeniedMenuItem = new ZMenuItem(ResString.GetMultilingualString("2936d8fe-2769-4d75-986d-24cdcb0b0d00", "Access Denied, click this menu for detail."), new EventHandler(ShowLicenceOrSecurityError));
			}
			MenuItems.Add(accessDeniedMenuItem);
		}
		MenuItem accessDeniedMenuItem;

		void ShowLicenceOrSecurityError(object sender, EventArgs e)
		{
			CurrentCheckPoint.ShowError();
		}

		#region Implementation

		readonly ForwardingConsol consol;

		ConsolACIMessageManager Manager
		{
			get { return (ConsolACIMessageManager)base.manager; }
		}

		#endregion
	}
}
