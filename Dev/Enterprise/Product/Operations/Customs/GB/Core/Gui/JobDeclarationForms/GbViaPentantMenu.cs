using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.GB.Pentant.CargoReportMessageCreator;

namespace Enterprise.Customs.GB.GUI
{
	public partial class GBViaPentantMenu : ChiefEDIMenu
	{
		public GBViaPentantMenu()
			: base(false)
		{
			this.Text = "Pentant";
		}

		protected override void AddAuditMenuItems()
		{
		}

		protected override void SetupTopLevelMenu()
		{
			MenuItems.Clear();

			obtainMucrForAcaMenu = new ZMenuItem("Send Cargo Report (obtain MUCR from ACA)", CreateCargoReport);
			amendCargoReportMenu = new ZMenuItem("Amend Cargo Report", AmendCargoReport);
			acaReferenceMenu = new ZMenuItem("Create ACA Reference", CreateAcaReferenceClick);

			MenuItems.Add(acaReferenceMenu);
			MenuItems.Add(obtainMucrForAcaMenu);
			MenuItems.Add(amendCargoReportMenu);
		}

		public override void RefreshMenu()
		{
		}

		void CreateCargoReport(object sender, EventArgs e)
		{
			CreateAmendCargoReport(CargoReportType.CREATE);
		}

		void AmendCargoReport(object sender, EventArgs e)
		{
			CreateAmendCargoReport(CargoReportType.AMEND);
		}

		void CreateAcaReferenceClick(object sender, EventArgs e)
		{
			if (Declaration != null && FormInternal != null)
			{
				Declaration.HasChanges = true;
				Declaration.ForceNewAcaReferenceOnSaving = true;
				if (FormInternal.FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes)
				{
					if (Declaration.JE_MasterUCR.IsEmpty)
					{
						Globals.Message.ShowInformation("ACA Reference is " + Declaration.JE_ACAReference);
					}
					else
					{
						Globals.Message.ShowInformation("ACA Reference not created because a Master UCR already exists.");
					}
				}
			}
		}

		void CreateAmendCargoReport(CargoReportType reportType)
		{
			if (Declaration != null)
			{
				if (FormInternal.FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes)
				{
					if (Declaration.CustomsEntryHeaders.Count > 0 && !Declaration.CustomsEntryHeaders.AreAnyHeadersWaitingForAResponse && !Declaration.JE_ACAReference.IsEmpty)
					{
						var cargoReportMessageHelper = new Pentant.CDSCargoReportMessageCreator(Declaration, reportType);
						if (cargoReportMessageHelper.CreateCargoReportMessage())
						{
							Declaration.Factory.Save();
							Globals.Message.ShowInformation("Cargo Report queued for transmission");
						}
					}
					else
					{
						Globals.Message.ShowInformation("Please ensure you have generated entries, that no response messages are outstanding, and that an ACA Reference has been generated.");
					}
				}
			}
		}

		protected virtual ZForm FormInternal => Form;

		MenuItem acaReferenceMenu;
		MenuItem obtainMucrForAcaMenu;
		MenuItem amendCargoReportMenu;
	}
}
