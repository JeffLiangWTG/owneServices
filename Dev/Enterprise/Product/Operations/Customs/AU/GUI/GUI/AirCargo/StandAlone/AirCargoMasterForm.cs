using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	/// <summary>
	/// AirCargoMasterForm : this is a form for stand-alone module for Master
	/// </summary>
	public partial class AirCargoMasterForm : ZTemplateForm
	{
		public AirCargoMasterForm(CusMAWB masterBill)
			: base(masterBill)
		{
			SetupMenuItem();
			SetupAirCargoDeclarationUserControl();
			SetupPlugins(masterBill);
			WorkflowTabPage.Initialize(masterBill);
		}

		#region FormCaption

		public override string FormCaption
		{
			get { return "Air Cargo"; }
		}

		#endregion

		public override bool IsResizableByTabPageAllowed => true;

		#region OnCurrentHouseBillChanged

		public void OnCurrentHouseBillChanged(CusHAWB currentHouseBill)
		{
			MasterBill.CurrentHouseBill = currentHouseBill;
			if (AirCargoDeclarationUserControl != null)
			{
				AirCargoDeclarationUserControl.HAWB = currentHouseBill;
			}
		}

		#endregion

		#region Implementation

		protected CusMAWB MasterBill => DataSource as CusMAWB;
		protected AirCargoMasterMenu airCargoMenu;

		#region Manager

		CusMAWBMessageManager fManager;
		protected internal virtual CusMAWBMessageManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new CusMAWBMessageManager(() => MasterBill);
				}
				return fManager;
			}
		}

		#endregion

		#region SetupPlugins

		protected void SetupPlugins(CusMAWB masterBill)
		{
			AirCargoDeclarationUserControl.SetupPlugins();
			PlugIns.AddJobInvoicing(masterBill.InvoicingSupporter);
		}

		#endregion

		#region AirCargoDeclarationUserControl

		internal BaseACAStandAloneUserControl AirCargoDeclarationUserControl;

		protected void SetupAirCargoDeclarationUserControl()
		{
			AirCargoDeclarationUserControl = NewACAStandAloneUserControl();

			AirCargoDeclarationUserControl.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			AirCargoDeclarationUserControl.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusMAWB";
			AirCargoDeclarationUserControl.Dock = DockStyle.Fill;
			AirCargoDeclarationUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			AirCargoDeclarationUserControl.Name = "AirCargoDeclarationUserControl";
			AirCargoDeclarationUserControl.Size = ControlDpiScalingHelper.NewScaledSize(952, 534);
			AirCargoDeclarationUserControl.TabIndex = 0;

			MainTabPage.Controls.Add(AirCargoDeclarationUserControl);
		}

		protected virtual BaseACAStandAloneUserControl NewACAStandAloneUserControl()
		{
			return new CMRACAStandAloneUserControl();
		}

		#endregion

		#region MenuItems

		protected void SetupMenuItem()
		{
			var scanHost = new AirScanForOutturnHost(this, MasterBill);
			airCargoMenu = AirCargoMasterMenuWithScan.New(MasterBill, Manager, scanHost);
			Menu.MenuItems.Add((Menu.MenuItems.Count - 1), airCargoMenu);
		}

		#endregion

		#region ShowPreSaveDialogs

		protected const string BulkZeroLandingMessage = "You changed the master bill number. System is about to zero-land all pre-alerted house bills under this master\r\nand send new original messages. Are you sure you wish to continue?";
		const string OneHouseBillMandatoryBeforeSavingMessage = "You should create at least one house bill before you save this record.";

		ContinueWithSave ShowPreSaveDialogs_CMR()
		{
			return new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(Manager);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (MasterBill.ChildBills.Count == 0)
			{
				Globals.Message.Show(OneHouseBillMandatoryBeforeSavingMessage, "Air Cargo Automation", MessageBoxButtons.OK, MessageBoxIcon.Error);
				result = ContinueWithSave.No;
			}
			if (result == ContinueWithSave.Yes)
			{
				result = ShowPreSaveDialogs_CMR();
			}
			return result;
		}

		#endregion

		#region InformUsersOnMessageSent

		protected void InformUsersOnMessageSent()
		{
			Globals.Message.Show("Messages sent", "Air Cargo Automation", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			HasStartedDisposing = true;
			base.Dispose(isNotFinalizing);
		}

		public bool HasStartedDisposing { get; private set; }

		#endregion

	}
}
