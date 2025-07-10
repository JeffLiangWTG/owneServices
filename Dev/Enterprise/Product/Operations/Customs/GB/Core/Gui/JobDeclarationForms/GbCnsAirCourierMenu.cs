using System;
using Enterprise.Customs.GB.CNS.ServiceTasks.AirCourier;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class GbCnsAirCourierMenu : ChiefEDIMenu
	{
		public GbCnsAirCourierMenu()
			: base(false)
		{
			this.Text = CnsCourierCaption;
		}
		public const string CnsCourierCaption = "CNS Courier";
		protected override void AddAuditMenuItems()
		{
		}

		protected override void SetupTopLevelMenu()
		{
			MenuItems.Clear();
			var newItem = new ZMenuItem("Create New Courier Consignment", SendToCnsAirCourierNew);
			var amendItem = new ZMenuItem("Delete Existing Courier Consignment", SendToCnsAirCourierAmend);
			MenuItems.Add(newItem);
			MenuItems.Add(amendItem);
		}

		public override void RefreshMenu()
		{
		}

		void SendToCnsAirCourierNew(object sender, EventArgs e)
		{
			SendToCns(AirCourierFromDeclaration.AirCourierMessageTypes.Add);
		}

		void SendToCnsAirCourierAmend(object sender, EventArgs e)
		{
			SendToCns(AirCourierFromDeclaration.AirCourierMessageTypes.Delete);
		}

		void SendToCns(AirCourierFromDeclaration.AirCourierMessageTypes how)
		{
			if (Declaration != null)
			{
				if (FormInternal.FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes)
				{
					var result = GetNewGenerator(how).DoEverything();
					Declaration.Factory.Save();
					Globals.Message.ShowInformation(result, "CNS Courier");
				}
			}
		}

		protected virtual AirCourierFromDeclaration GetNewGenerator(AirCourierFromDeclaration.AirCourierMessageTypes how)
		{
			return new AirCourierFromDeclaration(Declaration, how);
		}

		protected virtual ZForm FormInternal => Form;
	}
}
