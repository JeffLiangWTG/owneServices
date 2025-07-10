using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirInventoryFormHouse : ZTemplateForm, IPreviousNextControlProvider, IPostingButtonsProvider
	{
		public CcsukAirInventoryFormHouse(CusHAWB cusHAWB)
			: base(cusHAWB)
		{
			hawb = cusHAWB;
			this.SetDataBinding(cusHAWB, string.Empty);
			AddMenu(cusHAWB);
			CcsukAirInventoryForm.AddDocumentsMenu(this);
			cusHAWB.CcsukLicenceLoginHandler += new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
			WireUnderbondVisiblityTogglerAndCallInitially();
			WorkflowTabPage.Initialize(cusHAWB);
		}
		void WireUnderbondVisiblityTogglerAndCallInitially()
		{
			var controls = Controls.Find("underbondUserControl1", true);
			if (controls != null && controls.Length == 1)
			{
				underBondControl = controls[0] as UnderbondUserControl;
				if (underBondControl != null)
				{
					hawb.OnPimaChanged += new CusMAWB.PimaChangedEventHandler(underBondControl.ToggleRemovalTabVisibility);
					underBondControl.ToggleRemovalTabVisibility(hawb);
				}
			}
		}
		UnderbondUserControl underBondControl;

		void AddMenu(ICcsukCusAwb cusAwb)
		{
			var manager = new CusAwbDelegateProvider(delegate
			{ return cusAwb; });
			var menu = new CcsukMenu(manager, this);
			MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, menu);
		}

		public override string FormCaption
		{
			get { return hawb.HumanReadableName; }
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

		protected override bool AllowNew
		{
			get { return false; }
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
			if (hawb != null)
			{
				hawb.CcsukLicenceLoginHandler -= new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
				if (underBondControl != null)
				{
					hawb.OnPimaChanged -= new CusMAWB.PimaChangedEventHandler(underBondControl.ToggleRemovalTabVisibility);
				}
			}
			base.Dispose(disposing);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			result = CcsukAirInventoryForm.SendAutoFrcIfNeeded(result, hawb);
			return result;
		}

		readonly CusHAWB hawb;
	}
}
