using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ExportOrientedUnitsLayout))]
sealed class ExportOrientedUnitsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	public void TestAddControlBehaviour()
	{
		AssertAddControlBehaviour(StuffingAtList.Codes.FAC, INContainerModeList.Codes.ContainerisedAndPackaged, Core.Constants.TransportModes.Sea, false);
		AssertAddControlBehaviour(StuffingAtList.Codes.CFS, INContainerModeList.Codes.ContainerisedAndPackaged, Core.Constants.TransportModes.Sea, true);
		AssertAddControlBehaviour(StuffingAtList.Codes.FAC, INContainerModeList.Codes.ContainerisedAndPackaged, Core.Constants.TransportModes.Air, true);
		AssertAddControlBehaviour(StuffingAtList.Codes.FAC, INContainerModeList.Codes.Liquid, Core.Constants.TransportModes.Sea, true);
		AssertAddControlBehaviour(StuffingAtList.Codes.CFS, INContainerModeList.Codes.ContainerisedAndPackaged, Core.Constants.TransportModes.Air, true);
		AssertAddControlBehaviour(StuffingAtList.Codes.CFS, INContainerModeList.Codes.Liquid, Core.Constants.TransportModes.Sea, true);

		void AssertAddControlBehaviour(ZString stuffingAt, ZString containerMode, ZString transportMode, ZBool isEnable)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_StuffingAt = stuffingAt;
			declaration.JE_ContainerMode = containerMode;
			declaration.JE_TransportMode = transportMode;

			using (var form = new ZForm())
			using (var dynamicPanel = new DynamicLayoutPanel())
			{
				form.Controls.Add(dynamicPanel);
				form.Show();
				dynamicPanel.SetDataBinding(declaration, "");
				dynamicPanel.UpdateLayout(new ExportOrientedUnitsLayout());

				var userControl = dynamicPanel.FindSingle<Control>(CustomsBag.ExportOrientedUnitsDocAddressControl.ControlName);
				AssertEquals(isEnable, userControl.Enabled);
			}
		}
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CustomsBag.ExportOrientedUnitsDocAddressControl, ControlWidthClass.Auto);
			yield return (CustomsBag.ExaminationDateEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.ExaminingOfficerNameTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.ExaminingOfficerDesignationTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.SupervisingOfficerNameTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.SupervisingOfficerDesignationTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.CommissionerateTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.DivisionTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.RangeTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.SealNoTextBox, ControlWidthClass.Auto);
			yield return (CustomsBag.VerifiedDropEdit, ControlWidthClass.Auto);
			yield return (CustomsBag.SampleForwardedDropEdit, ControlWidthClass.Auto);
		}
	}

	ExportOrientedUnitsControlBag CustomsBag => ExportOrientedUnitsControlBag.Instance;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportOrientedUnitsLayoutBuilder();
}
