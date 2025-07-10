using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirInventoryForm : ZTemplateForm, IPreviousNextControlProvider
	{
		public CcsukAirInventoryForm(CusMAWB cusMAWB)
			: base(cusMAWB)
		{
			this.cusMAWB = cusMAWB;
			this.SetDataBinding(cusMAWB, string.Empty);
			AddMenu(cusMAWB);
			AddDocumentsMenu(this);
			cusMAWB.MasterLevelHouseHelper.CcsukLicenceLoginHandler += new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
			WorkflowTabPage.Initialize(cusMAWB);
		}

		void AddMenu(ICcsukCusAwb cusAwb)
		{
			var manager = new CusAwbDelegateProvider(delegate
			{ return cusAwb; });
			var menu = new CcsukMenu(manager, this);
			MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, menu);
		}

		public override string FormCaption
		{
			get { return cusMAWB.HumanReadableName; }
		}

		public override Size MinimumSize
		{
			get { return CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 494); }
			set { base.MinimumSize = value; }
		}

		protected override bool SupportsEDocs
		{
			get { return true; }
		}

		protected override bool ShowNotesTab
		{
			get { return true; }
		}

		internal static void AddDocumentsMenu(ZTemplateForm form)
		{
			form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		void HandleShedLicenceLogin(object sender, Customs.Business.LicenceLoginEventArgs e)
		{
			e.LoginHasBeenAttempted = true;
			e.LicenceCheckPoint.Login(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			if (cusMAWB != null)
			{
				cusMAWB.MasterLevelHouseHelper.CcsukLicenceLoginHandler -= new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
			}
			base.Dispose(disposing);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			result = SendAutoFrcIfNeeded(result, cusMAWB);
			return result;
		}

		internal static ContinueWithSave SendAutoFrcIfNeeded(ContinueWithSave result, ICcsukCusAwb awb)
		{
			try
			{
				if (result == ContinueWithSave.Yes && awb != null && awb.HasChanges && GBCustomsDataRegistry.Instance.CcsukAllowAutoFrc.Value)
				{
					Customs.Business.IMessageManager manager = awb.GetMessageManagerForAmendmentDetection();
					result = new Customs.GUI.SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(manager);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = ContinueWithSave.No;
				new Customs.GUI.SendsMessagesToCustomsGUI().WarnUserAboutSomething(ex.Message, "Could not calculate/generate messages");
			}
			return result;
		}

		readonly CusMAWB cusMAWB;
	}
}
