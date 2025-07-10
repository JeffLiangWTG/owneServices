using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoHouseForm : CusHAWBForm
	{
		[Obsolete("This is for designer only", true)]
		public AirCargoHouseForm()
		{
		}

		public AirCargoHouseForm(CusHAWB businessEntity)
			: base(businessEntity)
		{
			SetUpMenuItems();
			workflowTabPage.Initialize(businessEntity);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool AllowNew => false;

		#region SetupMenuItems

		void SetUpMenuItems()
		{
			AirCargoShipmentMenu houseMenu = AirCargoShipmentMenu.New(Manager);
			this.Menu.MenuItems.Add((Menu.MenuItems.Count - 1), houseMenu);
		}

		#endregion

		#region ShowPreSaveDialogs

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && HouseBill != null)
			{
				result = new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(Manager);
			}
			return result;
		}

		#endregion

		#region Manager

		protected virtual CusHAWBMessageManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new CusHAWBMessageManager(() => HouseBill);
				}
				return fManager;
			}
		}
		CusHAWBMessageManager fManager;

		#endregion
	}
}
