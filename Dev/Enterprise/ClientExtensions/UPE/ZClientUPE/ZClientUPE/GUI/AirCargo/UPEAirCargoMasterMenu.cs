using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEAirCargoMasterMenu : AirCargoMasterMenu
	{
		protected UPEAirCargoMasterMenu(CusMAWB masterBill, CusMAWBMessageManager manager)
			: base(masterBill, manager)
		{
		}

		protected UPEAirCargoMasterMenu(ForwardingConsol consol, CusMAWBMessageManager manager)
			: base(consol, manager)
		{
		}

		#region Factory Method Overriding

		public static void RegisterThisTypeOverride()
		{
			OverridableNewMAWBDelegate.Value = new NewMAWBDelegate(OverriddenNewMAWB);
			OverridableNewConsolDelegate.Value = new NewConsolDelegate(OverriddenNewConsol);
		}

		static AirCargoMasterMenu OverriddenNewMAWB(CusMAWB masterBill, CusMAWBMessageManager manager)
		{
			return new UPEAirCargoMasterMenu(masterBill, manager);
		}

		static AirCargoMasterMenu OverriddenNewConsol(ForwardingConsol consol, CusMAWBMessageManager manager)
		{
			return new UPEAirCargoMasterMenu(consol, manager);
		}

		internal static bool IsSubTypeRegistered
		{
			get { return OverridableNewMAWBDelegate.IsOverriden && OverridableNewMAWBDelegate.IsOverriden; }
		}

		#endregion

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			MatchingStatisticsMenuItem = new ZMenuItem("Show Matching Statistics", new EventHandler(OnShowMatchingStatistics_Click));
			MenuItems.Add(MatchingStatisticsMenuItem);
			MenuItems.Add("-");
		}

		public MenuItem MatchingStatisticsMenuItem;

		void OnShowMatchingStatistics_Click(object sender, EventArgs e)
		{
			int totalCount = 0;
			int totalAutoMatched = 0;
			int totalManualMatched = 0;

			if (MasterBill != null)
			{
				totalCount = MasterBill.ChildBills.Count;
				foreach (UPECusHAWB uPECusHAWB in MasterBill.ChildBills)
				{
					totalAutoMatched += uPECusHAWB.TotalAutoMatches;
					totalManualMatched += uPECusHAWB.TotalManualMatches;
				}
			}

			Globals.Message.ShowInformation(
				string.Format("Total Housebills = {0}\nTotal AutoMatches = {1}\nTotal Manual Matches = {2}",
				totalCount, totalAutoMatched, totalManualMatched));
		}
	}
}
