using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class ContainerUserControlTest : TestCaseWithFactory
{
	public void TestSecondSealNumber()
	{
		using (var userControl = new ContainerUserControl())
		{
			AssertNotNull(userControl.containersUserControl1);

			var controls = userControl.containersUserControl1.Controls.Find("edSecondSealNum", true);
			Assert("edSecondSealNum should exist", controls.Length == 1);

			var field = controls[0] as ZTextBox;
			AssertNotNull(field);
			Assert("edSecondSealNum should be visible", field.Visible);

			var columnStyle = userControl.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainerSchema.CO_SecondSeal.Name);
			Assert("SecondSeal column should be visible", columnStyle.IsVisible);

			AssertEquals("2nd Seal", field.CaptionResourceString.Caption);
			AssertEquals("[UCC 7/18] 2nd Seal", field.CaptionResourceString.FullDescription);
		}
	}

	public void TestSealParties()
	{
		using (var userControl = new ContainerUserControl())
		{
			AssertNotNull(userControl.containersUserControl1);

			var controls = userControl.containersUserControl1.Controls.Find("SealPartyDropEdit", true);
			Assert("SealPartyDropEdit should exist", controls.Length == 1);

			var dropdown = controls[0] as ZDropEdit;
			AssertNotNull(dropdown);
			Assert("SealPartyDropEdit should be visible", dropdown.Visible);

			controls = userControl.containersUserControl1.Controls.Find("AdditionalSealPartyDropEdit", true);
			Assert("AdditionalSealPartyDropEdit should exist", controls.Length == 1);

			dropdown = controls[0] as ZDropEdit;
			AssertNotNull(dropdown);
			Assert("AdditionalSealPartyDropEdit should be visible", dropdown.Visible);
		}
	}

	public void TestSealNumber()
	{
		using (var userControl = new ContainerUserControl())
		{
			AssertNotNull(userControl.containersUserControl1);

			var controls = userControl.containersUserControl1.Controls.Find("edSealNum", true);
			Assert("edSealNum should exist", controls.Length == 1);

			var fields = controls[0] as ZTextBox;
			AssertNotNull(fields);
			Assert("edSealNum should be visible", fields.Visible);

			AssertEquals("Seal", fields.CaptionResourceString.Caption);
			AssertEquals("[7/18] Seal Number.", fields.CaptionResourceString.FullDescription);
		}
	}

	public void TestContainerDetailUserControlTabIndex()
	{
		using (var userControl = new ContainerUserControl())
		{
			foreach (var (controlName, tabIndex) in FieldsDetails)
			{
				var actualfield = userControl.containersUserControl1.Controls.Find(controlName, true)[0];
				if (actualfield == null)
				{
					Assert($"Control: {controlName} does not exists", false);
				}
				else
				{
					TestCaseWithFactory.AssertEquals($"{controlName}: tab index", tabIndex, actualfield.TabIndex);
				}
			}
		}
	}

	public static void TestEdContainerNum()
	{
		using (var userControl = new ContainerUserControl())
		{
			AssertNotNull(userControl);
			var controls = userControl.Controls.Find("edContainerNum", true);
			Assert("edContainerNum should exist", controls.Length == 1);

			var field = controls[0] as ZTextBox;
			AssertNotNull(field);
			Assert("edContainerNum should be visible", field.Visible);
			AssertEquals("edContainerNum Caption", "Container", field.CaptionResourceString.Caption);
			AssertEquals("edContainerNum Full Description", "[UCC 7/10] Container", field.CaptionResourceString.FullDescription);
		}
	}

	public static void TestExportContainerTypeGuidFindBox()
	{
		using (var userControl = new ContainerUserControl())
		{
			AssertNotNull(userControl);
			var controls = userControl.Controls.Find("ExportContainerTypeGuidFindBox", true);
			Assert("ExportContainerTypeGuidFindBox should exist", controls.Length == 1);

			var field = controls[0] as ZGuidFindBox;
			AssertNotNull(field);
			Assert("ExportContainerTypeGuidFindBox should be visible", field.Visible);
			AssertEquals("ExportContainerTypeGuidFindBox Caption", "Type", field.CaptionResourceString.Caption);
			AssertEquals("ExportContainerTypeGuidFindBox Full Description", "[UCC 7/11] Type", field.CaptionResourceString.FullDescription);
		}
	}

	IEnumerable<(string ControlName, int TabIndex)> FieldsDetails => new (string, int)[]
	{
		("edContainerNum", 0),
		("ExportContainerModeDropEdit", 1),
		("DeliveryModeDropEdit", 2),
		("ExportContainerTypeGuidFindBox", 3),
		("edSealNum", 4),
		("SealPartyDropEdit", 5),
		("edSecondSealNum", 6),
		("AdditionalSealPartyDropEdit", 7),
		("WeightsGroupBox", 8),
		("DetailsGroupBox", 9),
		("DetailTabControl", 10),
	};
}
