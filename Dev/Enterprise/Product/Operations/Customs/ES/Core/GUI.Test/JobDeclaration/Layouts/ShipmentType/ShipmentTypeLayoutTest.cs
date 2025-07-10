using System;
using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayout))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.ShipmentTypeLayoutBuilder<EU.Business.Declaration.JobDeclaration>();

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, ControlWidthClass.Auto);
			}
		}

		public void TestSpecificCircumstanceDropEditVisibility()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("SpecificCircumstanceDropEdit", false, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("SpecificCircumstanceDropEdit", true, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, declaration));
		}

		public void TestBorderTransportMeansDropEditVisibility()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("BorderTransportMeansDropEdit", true, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, declaration));
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.H1))
			{
				AssertEquals("BorderTransportMeansDropEdit", true, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, declaration));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertEquals("BorderTransportMeansDropEdit", false, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, declaration));
			}
		}

		public void TestBorderTransportMeansDropEditCaption()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("SetCaptions for BorderTransportMeansDropEdit", true, layout.TryGetCaptionData(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, declaration, out var captionData));
				AssertEquals("data.Length", 1, captionData.Count);
				AssertEquals("captionData.Caption", "Border M.O.T.", captionData["BorderTransportMeansDropEdit"].Caption);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("SetCaptions for BorderTransportMeansDropEdit", true, layout.TryGetCaptionData(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, declaration, out var captionData));
				AssertEquals("Data Length", 1, captionData.Count);
				AssertEquals("Short Caption", "Arr. MOT", captionData["BorderTransportMeansDropEdit"].ShortCaption);
				AssertEquals("Medium Caption", "Arrival M.O.T.", captionData["BorderTransportMeansDropEdit"].MediumCaption);
				AssertEquals("Caption", "Arrival M.O.T.", captionData["BorderTransportMeansDropEdit"].Caption);
			}
		}

		public void TestIsSecurityDeclarationCheckBoxVisibility()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				CombineAssertions("When Declaration is Export but not UCC6", () =>
				{
					AssertEquals("IsSecurityDeclarationCheckBox IsVisible", false, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, declaration));
				});
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions("When Declaration is UCC6 Export", () =>
				{
					declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
					AssertEquals("For EntryStyle = CO, IsSecurityDeclarationCheckBox IsVisible", false, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, declaration));

					declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
					AssertEquals("For EntryStyle = EX, IsSecurityDeclarationCheckBox IsVisible", true, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, declaration));
				});
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CombineAssertions("When Declaration is Import", () =>
			{
				AssertEquals("IsSecurityDeclarationCheckBox IsVisible", false, layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, declaration));
			});
		}

		public void TestCTStatusIDDropEditVisibility()
		{
			AssertControlVisibility(EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, _ => true);
		}

		void AssertControlVisibility(ControlReference control, Func<JobDeclaration, bool> isVisible)
		{
			CombineAssertions(() =>
			{
				foreach (ICodeDescription msgType in declaration.Lookups.MessageTypeList)
				{
					declaration.JE_MessageType = msgType.Code;
					AssertEquals($"When Declaration is {msgType.Code}, {control.ControlName} visibility",
						isVisible.Invoke(declaration),
						layout.IsVisible(control, declaration));
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			layout = new ShipmentTypeLayout().Layout;
		}

		PanelLayout layout;
		JobDeclaration declaration;
	}
}
