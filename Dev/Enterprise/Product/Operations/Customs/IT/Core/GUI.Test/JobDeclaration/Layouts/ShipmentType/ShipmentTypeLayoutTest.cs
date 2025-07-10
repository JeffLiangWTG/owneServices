using System;
using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ShipmentTypeLayout))]
sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
{
	public void TestMessageVersionControlVisibility()
	{
		CombineAssertions("For EXP", () =>
		{
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = "ITF";
			AssertEquals("When EXP and Application code ITF, MessageVersion control visibility", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.MessageVersionDropEdit, declaration));

			declaration.JE_ApplicationCode = "BLT";
			AssertEquals("When EXP and Application code BLT, MessageVersion control visibility", true, Layout.IsVisible(ShipmentTypeControlBag.Instance.MessageVersionDropEdit, declaration));
		});

		CombineAssertions("For IMP", () =>
		{
			declaration.JE_MessageType = "IMP";
			AssertEquals("When IMP, MessageVersion control visibility", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.MessageVersionDropEdit, declaration));
		});
	}

	public void TestIsSecurityDeclarationCheckBoxVisibility()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions("When Declaration is Export but not UCC6", () =>
		{
			AssertEquals("IsSecurityDeclarationCheckBox IsVisible", false, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, declaration));
		});

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration is UCC6 Export", () =>
			{
				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
				AssertEquals("For EntryStyle = CO, IsSecurityDeclarationCheckBox IsVisible", false, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, declaration));

				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
				AssertEquals("For EntryStyle = EX, IsSecurityDeclarationCheckBox IsVisible", true, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, declaration));
			});
		}

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions("When Declaration is Import", () =>
		{
			AssertEquals("IsSecurityDeclarationCheckBox IsVisible", false, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, declaration));
		});
	}

	public void TestBorderTransportMeansDropEditVisibility()
	{
		CombineAssertions("For IMP", () =>
		{
			declaration.JE_MessageType = "IMP";
			AssertEquals("BorderTransportMeansDropEdit control visibility", false, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, declaration));
		});

		CombineAssertions("For EXP", () =>
		{
			declaration.JE_MessageType = "EXP";
			declaration.MessageVersion = "XML";
			AssertEquals("When IsUCC6 true, BorderTransportMeansDropEdit control visibility", true, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, declaration));

			declaration.MessageVersion = "TXT";
			AssertEquals("When IsUCC6 false, BorderTransportMeansDropEdit control visibility", false, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, declaration));
		});
	}

	public void TestAuthorisationNumberDropEditVisibility()
	{
		declaration.JE_MessageType = "EXP";
		CombineAssertions("For EXP", () =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertEquals("When Declaration is EXP but not UCC6, AuthorisationNumberDropEdit control visibility", true, Layout.IsVisible(ShipmentTypeControlBag.Instance.AuthorisationNumberDropEdit, declaration));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("When Declaration is EXP and UCC6, AuthorisationNumberDropEdit control visibility", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.AuthorisationNumberDropEdit, declaration));
			}
		});

		declaration.JE_MessageType = "IMP";
		AssertEquals("When Declaration is IMP, AuthorisationNumberDropEdit control visibility", true, Layout.IsVisible(ShipmentTypeControlBag.Instance.AuthorisationNumberDropEdit, declaration));
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
					Layout.IsVisible(control, declaration));
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.ShipmentTypeLayoutBuilder<JobDeclaration>();

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 3;

	#region Implementation

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.AuthorisationNumberDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.ShipmentTypeControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.MessageVersionDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.ShipmentTypeControlBag.Instance.IsSecurityDeclarationCheckBox, ControlWidthClass.Auto);
		}
	}

	PanelLayout Layout => layout ?? (layout = new ShipmentTypeLayout().Layout);
	PanelLayout layout;

	JobDeclaration declaration;

	#endregion
}
