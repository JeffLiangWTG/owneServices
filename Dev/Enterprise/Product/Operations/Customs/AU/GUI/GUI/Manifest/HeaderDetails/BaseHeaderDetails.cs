using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	public partial class BaseHeaderDetails : ZUserControl
	{
		public BaseHeaderDetails()
		{
			InitializeComponent();
			visibilityKeeper.SetVisibilityIfOriginallyVisible(PackDepotDocAddressControl, false);
		}

		public void ShowForSea()
		{
			SetVisibility(true);
			VoyageFlightNoLabel.Text = "Voyage No:";
			VoyageFlightNoTextBox.BindTo = Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VoyageNumber;
		}

		public void ShowForAir()
		{
			SetVisibility(false);
			VoyageFlightNoLabel.Text = "Flight No:";
			VoyageFlightNoTextBox.BindTo = Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_FlightNumber;
		}

		public void ShowForConsolidation(bool isConsolidation, bool isCTO)
		{
			visibilityKeeper.SetVisibilityIfOriginallyVisible(PackDepotDocAddressControl, isConsolidation);
		}

		void SetVisibility(bool isSea)
		{
			visibilityKeeper.SetVisibilityIfOriginallyVisible(VesselCodeFindBox, isSea);
			visibilityKeeper.SetVisibilityIfOriginallyVisible(LloydsIMOTextBox, isSea);
			visibilityKeeper.SetVisibilityIfOriginallyVisible(VesselIDMasterAirWaybillLabel, isSea);

			visibilityKeeper.SetVisibilityIfOriginallyVisible(TotalContainerCountCalcEdit, isSea);
			visibilityKeeper.SetVisibilityIfOriginallyVisible(TotalContainerCountLabel, isSea);

			visibilityKeeper.SetVisibilityIfOriginallyVisible(TotalEmptyContainerCountCalcEdit, isSea);
			visibilityKeeper.SetVisibilityIfOriginallyVisible(TotalEmptyContainerCountLabel, isSea);
		}

		class OriginalVisibilityKeeper
		{
			public OriginalVisibilityKeeper()
			{
			}

			public void SetVisibilityIfOriginallyVisible(Control control, bool visibilityValue)
			{
				if (!visibilityDictionary.ContainsKey(control))
				{
					visibilityDictionary.Add(control, control.Visible);
				}

				if (visibilityDictionary[control])
				{
					control.Visible = visibilityValue;
				}
			}

			readonly Dictionary<Control, bool> visibilityDictionary = new Dictionary<Control, bool>();
		}

		readonly OriginalVisibilityKeeper visibilityKeeper = new OriginalVisibilityKeeper();

		void RecalculateTotalsButton_Click(object sender, EventArgs e)
		{
			ExportCustomsManifestHeader header = ((ZForm)FindForm()).BusinessEntity as ExportCustomsManifestHeader;
			if (header != null)
			{
				header.CalculateTotalContainersFromLines();
				header.CalculateTotalPackagesFromLines();
			}
		}
	}
}
