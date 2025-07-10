using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayout))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		public void TestControlsVisibilityForImport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "CTStatusIDDropEdit", true);
					AssertControlVisibility(userControl, "ApplicationCodeDropEdit", true);
				});
			}
		}

		public void TestControlsVisibilityForMiscellaneousCustoms()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "CTStatusIDDropEdit", true);
					AssertControlVisibility(userControl, "ApplicationCodeDropEdit", true);
				});
			}
		}

		public void TestControlsVisibilityForExport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "CTStatusIDDropEdit", false);
					AssertControlVisibility(userControl, "ApplicationCodeDropEdit", true);
				});
			}
		}

		public void TestControlsVisibilityForBorderTransportMeansDropEdit()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
					{
						AssertControlVisibility(userControl, "BorderTransportMeansDropEdit", true);
					}

					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
					{
						AssertControlVisibility(userControl, "BorderTransportMeansDropEdit", true);
					}
				});
			}
		}

		public void TestCTStatusIDDropEditVisibility()
		{
			AssertControlVisibility(EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, d => !declaration.IsExport);
		}

		public void TestCaptionForTransportModeDropEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var layout = new ShipmentTypeLayout().Layout;
			layout.TryGetCaption(Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, declaration, out var captionResourceString);
			AssertEquals("[7/4] Trans. Mode", captionResourceString.ShortCaption);
			AssertEquals("[7/4] Transport Mode", captionResourceString.MediumCaption);
			AssertEquals("[7/4] Transport Mode at Border", captionResourceString.Caption);
			AssertEquals("[7/4] Mode of transport at the border", captionResourceString.FullDescription);
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<EU.Business.Declaration.JobDeclaration>();

		void AssertControlVisibility(Control userControl, string controlName, bool isVisible)
		{
			AssertEquals(controlName, isVisible, userControl.FindSingle<Control>(controlName).Visible);
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
