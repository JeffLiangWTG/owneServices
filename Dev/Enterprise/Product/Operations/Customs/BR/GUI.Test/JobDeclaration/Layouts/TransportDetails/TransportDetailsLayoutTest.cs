using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(TransportDetailsLayout))]
	sealed class TransportDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestOceanBillTextBox()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("OceanBillTextBox Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("OceanBillTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("OceanBillTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("OceanBillTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, Declaration));
			});
		}

		public void TestBR_CargoArrivalDocUtilizationDropEdit()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("CargoArrivalDocUtilizationDropEdit Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("CargoArrivalDocUtilizationDropEdit Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("CargoArrivalDocUtilizationDropEdit Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.River;
				AssertEquals("CargoArrivalDocUtilizationDropEdit Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
				AssertEquals("CargoArrivalDocUtilizationDropEdit Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("CargoArrivalDocUtilizationDropEdit Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("CargoArrivalDocUtilizationDropEdit Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals("CargoArrivalDocUtilizationDropEdit Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, Declaration));
			});
		}

		public void TestVesselAndCountryUserControl()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("VesselAndCountryUserControl Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselAndCountryUserControl, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("VesselAndCountryUserControl Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselAndCountryUserControl, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				AssertEquals("VesselAndCountryUserControl Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselAndCountryUserControl, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals("VesselAndCountryUserControl Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselAndCountryUserControl, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
				AssertEquals("VesselAndCountryUserControl Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselAndCountryUserControl, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.River;
				AssertEquals("VesselAndCountryUserControl Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselAndCountryUserControl, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("VesselAndCountryUserControl Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.VesselAndCountryUserControl, Declaration));
			});
		}

		public void TestVehicleRegistrationNumberTextBox()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("VehicleRegistrationNumberTextBox Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("VehicleRegistrationNumberTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				AssertEquals("VehicleRegistrationNumberTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals("VehicleRegistrationNumberTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
				AssertEquals("VehicleRegistrationNumberTextBox Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, Declaration));
			});
		}

		public void TestVoyageNumberTextBox()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("VoyageNumberTextBox Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("VoyageNumberTextBox Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				AssertEquals("VoyageNumberTextBox Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals("VoyageNumberTextBox Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
				AssertEquals("VoyageNumberTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.River;
				AssertEquals("VoyageNumberTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));

				Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("VoyageNumberTextBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, Declaration));
			});
		}

		public void TestPlateTextBox()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("PlateTextBox NOT Visible when MessageType != ISW", false, Layout.IsVisible(TransportDetailsControlBag.Instance.PlateTextBox, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("PlateTextBox Visible when MessageType == ISW and TransportMode == ROA", true, Layout.IsVisible(TransportDetailsControlBag.Instance.PlateTextBox, Declaration));

				Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("PlateTextBox NOT Visible when MessageType == ISW and TransportMode != ROA", false, Layout.IsVisible(TransportDetailsControlBag.Instance.PlateTextBox, Declaration));
			});
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		PanelLayout Layout => layout ?? (layout = new TransportDetailsLayout().Layout);
		PanelLayout layout;

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Long);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Medium);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.PlateTextBox, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.VesselAndCountryUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.VoyageNumberTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.FlightUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.VehicleRegistrationNumberTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 2;

		public JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}
				return declaration;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder<JobDeclaration>();

		JobDeclaration declaration;
	}
}
