using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(ReportsGridFieldsLayout))]
	class ReportsGridFieldsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				var euBag = ReportsGridFieldsControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.ConsignmentGuidDropEdit, ControlWidthClass.Auto),
					(euBag.OfficeOfExitCodeFindBox, ControlWidthClass.Auto)
				};

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.TransportIDTextBox, ControlWidthClass.Auto),
					(euBag.TransportNationalityDropEdit, ControlWidthClass.Auto),
					(euBag.TransportTypeDropEdit, ControlWidthClass.Auto)
				};
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ReportsGridFieldsLayoutBuilder<Business.CusExitReport>();
	}
}
