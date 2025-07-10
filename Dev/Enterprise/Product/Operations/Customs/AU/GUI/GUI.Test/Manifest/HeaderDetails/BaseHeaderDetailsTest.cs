using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.ExportManifest.GUI.Testing
{
	sealed class BaseHeaderDetailsTest : TestCaseWithFactory
	{
		public void TestDefaultState()
		{
			using (BaseHeaderDetails control = new InitiallyTrueHeaderDetailsHelper())
			{
				AssertVisibility(true, control);
				AssertEquals("Voyage No:", control.VoyageFlightNoLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VoyageNumber, control.VoyageFlightNoTextBox.BindTo);
			}

			using (BaseHeaderDetails control = new InitiallyFalseHeaderDetailsHelper())
			{
				AssertVisibility(false, control);
				AssertEquals("Voyage No:", control.VoyageFlightNoLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VoyageNumber, control.VoyageFlightNoTextBox.BindTo);
			}
		}

		public void TestStateForSea()
		{
			using (BaseHeaderDetails control = new InitiallyTrueHeaderDetailsHelper())
			{
				control.ShowForSea();
				AssertVisibility(true, control);
				AssertEquals("Voyage No:", control.VoyageFlightNoLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VoyageNumber, control.VoyageFlightNoTextBox.BindTo);
				AssertEquals("Vessel:", control.VesselIDMasterAirWaybillLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VesselName, control.VesselCodeFindBox.BindTo);
				AssertEquals("Lloyds/IMO:", control.LloydsLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_LloydsIMO, control.LloydsIMOTextBox.BindTo);
			}

			using (BaseHeaderDetails control = new InitiallyFalseHeaderDetailsHelper())
			{
				control.ShowForSea();
				AssertVisibility(false, control);
				AssertEquals("Voyage No:", control.VoyageFlightNoLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VoyageNumber, control.VoyageFlightNoTextBox.BindTo);
				AssertEquals("Vessel:", control.VesselIDMasterAirWaybillLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VesselName, control.VesselCodeFindBox.BindTo);
				AssertEquals("Lloyds/IMO:", control.LloydsLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_LloydsIMO, control.LloydsIMOTextBox.BindTo);
			}
		}

		public void TestStateForAir()
		{
			using (BaseHeaderDetails control = new InitiallyTrueHeaderDetailsHelper())
			{
				control.ShowForAir();
				AssertVisibility(false, control);
				AssertEquals("Flight No:", control.VoyageFlightNoLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_FlightNumber, control.VoyageFlightNoTextBox.BindTo);
			}

			using (BaseHeaderDetails control = new InitiallyFalseHeaderDetailsHelper())
			{
				control.ShowForAir();
				AssertVisibility(false, control);
				AssertEquals("Flight No:", control.VoyageFlightNoLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_FlightNumber, control.VoyageFlightNoTextBox.BindTo);
			}
		}

		public void TestSwitchingFromAirToSea()
		{
			using (BaseHeaderDetails control = new InitiallyTrueHeaderDetailsHelper())
			{
				control.ShowForAir();
				control.ShowForSea();
				AssertVisibility(true, control);
				AssertEquals("Voyage No:", control.VoyageFlightNoLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VoyageNumber, control.VoyageFlightNoTextBox.BindTo);
				AssertEquals("Vessel:", control.VesselIDMasterAirWaybillLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_VesselName, control.VesselCodeFindBox.BindTo);
				AssertEquals("Lloyds/IMO:", control.LloydsLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_LloydsIMO, control.LloydsIMOTextBox.BindTo);
			}
		}

		public void TestSwitchingFromSeaToAir()
		{
			using (BaseHeaderDetails control = new InitiallyTrueHeaderDetailsHelper())
			{
				control.ShowForSea();
				control.ShowForAir();
				AssertVisibility(false, control);
				AssertEquals("Flight No:", control.VoyageFlightNoLabel.Text);
				AssertEquals(Customs.Business.AutoExportCustomsManifestHeader.Schema.ED_FlightNumber, control.VoyageFlightNoTextBox.BindTo);
			}
		}

		public void TestRecalculateTotals()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_TransportMode = Core.Constants.TransportCodes.Sea;
			ExportCustomsManifestLines line1 = header.Lines.AddNew();
			ExportCustomsManifestLines line2 = header.Lines.AddNew();
			line1.EL_NumberOfContainers = 2;
			line1.EL_NumberOfPackages = 3;
			line2.EL_NumberOfContainers = 5;
			line2.EL_NumberOfPackages = 6;
			header.ED_NoOfContainer = 1;
			header.ED_NoOfPacks = 1;
			using (BaseHeaderDetails control = new BaseHeaderDetails())
			using (ZForm form = new ZForm(header))
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals((short)1, header.ED_NoOfContainer);
				AssertEquals(1, header.ED_NoOfPacks);
				control.RecalculateTotalsButton.PerformClick();
				AssertEquals((short)7, header.ED_NoOfContainer);
				AssertEquals(9, header.ED_NoOfPacks);
			}
		}

		void AssertVisible(bool expected, Control control)
		{
			AssertEquals(expected, control.Visible);
		}

		void AssertVisibility(bool expectedVisibility, BaseHeaderDetails control)
		{
			AssertVisible(expectedVisibility, control.VesselIDMasterAirWaybillLabel);
			AssertVisible(expectedVisibility, control.VesselCodeFindBox);
			AssertVisible(expectedVisibility, control.TotalContainerCountCalcEdit);
			AssertVisible(expectedVisibility, control.TotalContainerCountLabel);
			AssertVisible(expectedVisibility, control.TotalEmptyContainerCountCalcEdit);
			AssertVisible(expectedVisibility, control.TotalEmptyContainerCountLabel);
		}

		sealed class InitiallyTrueHeaderDetailsHelper : BaseHeaderDetails
		{
			public InitiallyTrueHeaderDetailsHelper()
			{
				VesselIDMasterAirWaybillLabel.Visible = true;
				VesselCodeFindBox.Visible = true;
				TotalContainerCountCalcEdit.Visible = true;
				TotalContainerCountLabel.Visible = true;
				TotalEmptyContainerCountCalcEdit.Visible = true;
				TotalEmptyContainerCountLabel.Visible = true;
			}
		}

		sealed class InitiallyFalseHeaderDetailsHelper : BaseHeaderDetails
		{
			public InitiallyFalseHeaderDetailsHelper()
			{
				VesselIDMasterAirWaybillLabel.Visible = false;
				VesselCodeFindBox.Visible = false;
				TotalContainerCountCalcEdit.Visible = false;
				TotalContainerCountLabel.Visible = false;
				TotalEmptyContainerCountCalcEdit.Visible = false;
				TotalEmptyContainerCountLabel.Visible = false;
			}
		}
	}
}
