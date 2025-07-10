using System;
using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayout))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		public void TestRepresentationDropEditVisibility()
		{
			AssertControlVisibility(CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, j => !j.IsExport);
		}

		public void TestMethodOfPaymentDropEditVisibility()
		{
			AssertControlVisibility(ShipmentTypeControlBag.Instance.MethodOfPaymentDropEdit, j => j.IsImport);
		}

		public void TestSpecificCircumstanceDropEditVisibility()
		{
			AssertControlVisibility(EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, j => !j.IsImport);
		}

		public void TestIsHighValueOvrdCheckBoxVisibility()
		{
			AssertControlVisibility(EU.GUI.ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, j => j.IsImport);
		}

		public void TestBorderTransportMeansDropEditCaption()
		{
			var control = ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				layout.TryGetCaption(control, declaration, out var resourceStringData);
				AssertEquals("Import Caption", "Border M.O.T.", resourceStringData.Caption);
				AssertEquals("Import FullDescription", "Border Means of Transport", resourceStringData.FullDescription);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				layout.TryGetCaption(control, declaration, out resourceStringData);
				AssertEquals("Export Caption", "Border T.O.ID.", resourceStringData.Caption);
				AssertEquals("Export FullDescription", "Border Type of ID", resourceStringData.FullDescription);
			});
		}

		public void TestCTStatusIDDropEditVisibility()
		{
			AssertControlVisibility(EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, _ => true);
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 4;

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long);
				yield return (CommonMiscOptionsControlBag.Instance.RepresentationDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.MethodOfPaymentDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<JobDeclaration>();

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
