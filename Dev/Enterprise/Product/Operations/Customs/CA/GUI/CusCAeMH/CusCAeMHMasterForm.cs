using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CusCAeMHMasterForm : ZTemplateForm
	{
		public CusCAeMHMasterForm()
		{
			InitializeComponent();
		}

		public CusCAeMHMasterForm(CusCAeMHMaster master)
			: base(master)
		{
			InitializeComponent();
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)master).InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			workflowTabPage.Initialize(master);
			SetupMenuItem();
		}

		void SetupMenuItem()
		{
			var messagingMenu = new CusCAeMHMessageMenu(new ACIHouseBillMultiMessageManager(() => MasterBill));
			messagingMenu.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B17DC012-5C51-43E8-ADEF-60BFB463E959", "CBSA Messaging");
			Menu.MenuItems.Add((Menu.MenuItems.Count - 1), messagingMenu);
		}

		CusCAeMHMaster MasterBill
		{
			get { return (CusCAeMHMaster)base.DataSource; }
		}

		public override string FormCaption
		{
			get
			{
				var result = new ZStringBuilder();
				if (!MasterBill.BP_MessageReference.IsEmpty)
				{
					result.Append(MasterBill.BP_MessageReference);
					result.Append(" ");
				}
				result.Append(ResString.GetMultilingualString("92425b89-5998-4d33-b763-71883b8c335d", "House Bill eManifest"));
				return result.ToString();
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && BusinessEntity is CusCAeMHMaster master)
			{
				result = master.ShowAutoSendingWithdrawalMessageDialog(CloseSender);
			}
			return result;
		}

		public override bool IsResizableByTabPageAllowed => true;

		ISendsMessagesToCustoms CloseSender
		{
			get { return fCloseSender ?? (fCloseSender = new SendsMessagesToCustomsGUI()); }
		}
		ISendsMessagesToCustoms fCloseSender;
	}
}
