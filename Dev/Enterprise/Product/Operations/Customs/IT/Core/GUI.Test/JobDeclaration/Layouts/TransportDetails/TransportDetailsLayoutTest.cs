using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(TransportDetailsLayout))]
sealed class TransportDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestOverrideValuesCheckBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_JS = ZGuid.Empty;
			AssertEquals("When declaration is standalone", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));

			declaration.JE_JS = ZGuid.NewZGuid();
			AssertEquals("When declaration is linked to a shipment", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));
		});
	}

	public void TestMasterBillTextBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = "";
			AssertEquals("When Transport Mode is not AIR", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));

			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Transport Mode is AIR", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
		});
	}

	public void TestOceanBillTextBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = "";
			AssertEquals("When Transport Mode is not SEA", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));

			declaration.JE_TransportMode = "SEA";
			AssertEquals("When Transport Mode is SEA", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
		});
	}

	public void TestVesselCodeFindBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = "";
			AssertEquals("When Transport Mode is not SEA", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));

			declaration.JE_TransportMode = "SEA";
			AssertEquals("When Transport Mode is SEA", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));
		});
	}

	public void TestVesselCodeFindBoxVisibility_WhenDeclarationIsImport()
	{
		declaration.JE_MessageType = "IMP";
		AssertEquals("When Declaration is IMP, VesselCodeFindBox Is Visible", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));
	}

	public void TestFlightAndNationalityUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = "";
			AssertEquals("When Transport Mode is not AIR", false, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Transport Mode is AIR", true, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
		});
	}

	public void TestFlightAndNationalityUserControlVisibility_WhenDeclarationIsImport()
	{
		declaration.JE_MessageType = "IMP";
		AssertEquals("When Declaration is IMP, FlightAndNationalityUserControl is Visible", false, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
	}

	public void TestVoyageAndNationalityUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = "";
			AssertEquals("When Transport Mode is not SEA", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = "SEA";
			AssertEquals("When Transport Mode is SEA", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
		});
	}

	public void TestVoyageAndNationalityUserControlVisibility_WhenDeclarationIsImport()
	{
		declaration.JE_MessageType = "IMP";
		AssertEquals("When Declaration is IMP, VoyageAndNationalityUserControl is Visible", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
	}

	public void TestTransportIDAndNationalityUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = "";
			AssertEquals("When Transport Mode is not AIR nor SEA", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Transport Mode is AIR", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = "SEA";
			AssertEquals("When Transport Mode is SEA", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
		});
	}

	public void TestTransportIDAndNationalityUserControlVisibility_WhenDeclarationIsImport()
	{
		declaration.JE_MessageType = "IMP";
		AssertEquals("When Declaration is IMP, TransportIDAndNationalityUserControl is Visible", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
	}

	public void TestPortOfLoadingUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Message Type is IMP and Transport Mode is AIR", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));

			declaration.JE_TransportMode = "";
			AssertEquals("When Message Type is IMP and Transport Mode is not AIR", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));

			declaration.JE_MessageType = "";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Message Type is not IMP and Transport Mode is AIR", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
		});
	}

	public void TestTransportDetailsPortOfLoadingWithIATAUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Message Type is IMP and Transport Mode is AIR", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration));

			declaration.JE_TransportMode = "";
			AssertEquals("When Message Type is IMP and Transport Mode is not AIR", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration));

			declaration.JE_MessageType = "";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Message Type is not IMP and Transport Mode is AIR", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration));
		});
	}

	public void TestInlandModeOfTransportDropEditVisibility()
	{
		CombineAssertions(() =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
			{
				AssertEquals("When declaration is not UCC6", true, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, declaration));
			}

			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			{
				declaration.JE_MessageType = "EXP";
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, transportModeInland: "AIR", expectedVisible: true);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, transportModeInland: "RAI", expectedVisible: true);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, transportModeInland: "ROA", expectedVisible: true);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, transportModeInland: "MAI", expectedVisible: false);

				declaration.JE_MessageType = "IMP";
				AssertEquals("When declaration is UCC6 Import", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, declaration));
			}
		});
	}

	public void TestInlandTransportModeAndMeansUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
			{
				AssertEquals("When declaration is not UCC6", false, layout.IsVisible(TransportDetailsControlBag.Instance.InlandTransportModeAndMeansUserControl, declaration));
			}

			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			{
				declaration.JE_MessageType = "EXP";
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", TransportDetailsControlBag.Instance.InlandTransportModeAndMeansUserControl, transportModeInland: "AIR", expectedVisible: false);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", TransportDetailsControlBag.Instance.InlandTransportModeAndMeansUserControl, transportModeInland: "RAI", expectedVisible: false);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", TransportDetailsControlBag.Instance.InlandTransportModeAndMeansUserControl, transportModeInland: "ROA", expectedVisible: false);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", TransportDetailsControlBag.Instance.InlandTransportModeAndMeansUserControl, transportModeInland: "MAI", expectedVisible: true);

				declaration.JE_MessageType = "IMP";
				AssertEquals("When declaration is UCC6 Import", true, layout.IsVisible(TransportDetailsControlBag.Instance.InlandTransportModeAndMeansUserControl, declaration));
			}
		});
	}

	public void TestInlandTransportDetailsUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
			{
				AssertEquals("When declaration is not UCC6", true, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, declaration));
			}

			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("When declaration is UCC6 Export", false, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, declaration));

				declaration.JE_MessageType = "IMP";
				AssertEquals("When declaration is UCC6 Import", true, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, declaration));
			}
		});
	}

	public void TestTransportInlandIDAndNationalityUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
			{
				AssertEquals("When declaration is not UCC6", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
			}

			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			{
				declaration.JE_MessageType = "EXP";
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, transportModeInland: "AIR", expectedVisible: false);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, transportModeInland: "RAI", expectedVisible: false);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, transportModeInland: "ROA", expectedVisible: false);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, transportModeInland: "MAI", expectedVisible: true);

				declaration.JE_MessageType = "IMP";
				AssertEquals("When declaration is UCC6 Import", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration));
			}
		});
	}

	public void TestTransportInlandAirUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
			{
				AssertEquals("When declaration is not UCC6", false, layout.IsVisible(TransportDetailsControlBag.Instance.TransportInlandAirUserControl, declaration));
			}

			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			{
				declaration.JE_MessageType = "EXP";
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", TransportDetailsControlBag.Instance.TransportInlandAirUserControl, transportModeInland: "AIR", expectedVisible: true);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", TransportDetailsControlBag.Instance.TransportInlandAirUserControl, transportModeInland: "MAI", expectedVisible: false);

				declaration.JE_MessageType = "IMP";
				AssertEquals("When declaration is UCC6 Import", false, layout.IsVisible(TransportDetailsControlBag.Instance.TransportInlandAirUserControl, declaration));
			}
		});
	}

	public void TestTransportInlandRailUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
			{
				AssertEquals("When declaration is not UCC6", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, declaration));
			}

			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			{
				declaration.JE_MessageType = "EXP";
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, transportModeInland: "RAI", expectedVisible: true);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, transportModeInland: "MAI", expectedVisible: false);

				declaration.JE_MessageType = "IMP";
				AssertEquals("When declaration is UCC6 Import", false, layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, declaration));
			}
		});
	}

	public void TestTransportInlandRoadUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
			{
				AssertEquals("When declaration is not UCC6", false, layout.IsVisible(TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
			}

			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			{
				declaration.JE_MessageType = "EXP";
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, transportModeInland: "ROA", expectedVisible: true);
				AssertControlVisibilityForTransportModeInland("When declaration is UCC6 Export", TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, transportModeInland: "MAI", expectedVisible: false);

				declaration.JE_MessageType = "IMP";
				AssertEquals("When declaration is UCC6 Import", false, layout.IsVisible(TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
			}
		});
	}

	public void TestTransportNationalityFindBoxVisibility()
	{
		declaration.JE_MessageType = "IMP";
		AssertEquals("When declaration is IMP", true, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportNationalityCodeFindBox, declaration));

		declaration.JE_MessageType = "EXP";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			AssertEquals("When declaration is EXP UCC6", false, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportNationalityCodeFindBox, declaration));
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			AssertEquals("When declaration is EXP NON UCC6", false, layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportNationalityCodeFindBox, declaration));
		}
	}

	void AssertControlVisibilityForTransportModeInland(string message, ControlReference controlReference, ZString transportModeInland, bool expectedVisible)
	{
		declaration.JE_TransportModeInland = transportModeInland;
		AssertEquals($"{message} and JE_TransportModeInland = {transportModeInland}", expectedVisible, layout.IsVisible(controlReference, declaration));
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 3;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder();

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		layout = ((IPanelLayoutProvider)new TransportDetailsLayout()).Layout;
	}

	JobDeclaration declaration;
	PanelLayout layout;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.TransportDetailsControlBag.Instance.TransportNationalityCodeFindBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, ControlWidthClass.Auto);
			yield return (TransportDetailsControlBag.Instance.InlandTransportModeAndMeansUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (TransportDetailsControlBag.Instance.TransportInlandAirUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, ControlWidthClass.Auto);
			yield return (TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, ControlWidthClass.Auto);
		}
	}
}
