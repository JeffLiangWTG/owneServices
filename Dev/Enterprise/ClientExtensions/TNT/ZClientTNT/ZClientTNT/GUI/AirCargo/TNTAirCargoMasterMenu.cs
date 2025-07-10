using System;
using System.Windows.Forms;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT.GUI
{
	public class TNTAirCargoMasterMenu : AirCargoMasterMenu
	{
		#region Construction

		protected TNTAirCargoMasterMenu(CusMAWB masterBill, CusMAWBMessageManager manager)
			: base(masterBill, manager)
		{
		}

		protected TNTAirCargoMasterMenu(ForwardingConsol consol, CusMAWBMessageManager manager)
			: base(consol, manager)
		{
		}

		#endregion

		#region Factory Methods

		public static void RegisterThisTypeOverride()
		{
			OverridableNewMAWBDelegate.Value = new NewMAWBDelegate(OverriddenNewMAWB);
			OverridableNewConsolDelegate.Value = new NewConsolDelegate(OverriddenNewConsol);
		}

		static AirCargoMasterMenu OverriddenNewMAWB(CusMAWB masterBill, CusMAWBMessageManager manager)
		{
			return new TNTAirCargoMasterMenu(masterBill, manager);
		}

		static AirCargoMasterMenu OverriddenNewConsol(ForwardingConsol consol, CusMAWBMessageManager manager)
		{
			return new TNTAirCargoMasterMenu(consol, manager);
		}

		#endregion

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			MenuItems.Add("-");
			CreateDeclarationsMenuItem = new ZMenuItem(CreateDeclarationsMenuHeading, new EventHandler(OnCreateDeclaration_Click));
			MenuItems.Add(CreateDeclarationsMenuItem);
		}

		internal const string CreateDeclarationsMenuHeading = "Auto Create Declarations";

		MenuItem CreateDeclarationsMenuItem;

		void OnCreateDeclaration_Click(object sender, EventArgs e)
		{
			if (MasterBill != null)
			{
				if (MasterBill.HasChanges)
				{
					Globals.Message.ShowError("You must save the form before you can create declarations.");
				}
				else
				{
					CreateDeclarationsFromAirCargoForMaster();
				}
			}
		}

		void CreateDeclarationsFromAirCargoForMaster()
		{
			ZForm parentForm = (ZForm)((MainMenu)Parent).GetForm();
			ZFormModaliser.Show(new DeclarationFromAirCargoForm(new DeclarationsFromCusMAWBCreator(MasterBill)), parentForm);
		}
	}
}
