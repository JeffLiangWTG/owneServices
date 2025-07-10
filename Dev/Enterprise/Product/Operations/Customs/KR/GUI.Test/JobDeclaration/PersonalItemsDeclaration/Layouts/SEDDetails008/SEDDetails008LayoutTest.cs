using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(SEDDetails008Layout))]
	sealed class SEDDetails008LayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new SEDDetails008Layout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(15, columns[0].Rows.Count);
			AssertLayout(columns[0].Rows[0], nameof(SEDDetails008ControlBag.Instance.StayPeriodDropEdit));
			AssertLayout(columns[0].Rows[1], nameof(SEDDetails008ControlBag.Instance.CustomsOfficeCodeFindBox));
			AssertLayout(columns[0].Rows[2], nameof(SEDDetails008ControlBag.Instance.DepartmentCodeFindBox));
			AssertLayout(columns[0].Rows[3], nameof(SEDDetails008ControlBag.Instance.HasItemsDropEdit));
			AssertLayout(columns[0].Rows[4], nameof(SEDDetails008ControlBag.Instance.WeaponDropEdit));
			AssertLayout(columns[0].Rows[5], nameof(SEDDetails008ControlBag.Instance.DrugDropEdit));
			AssertLayout(columns[0].Rows[6], nameof(SEDDetails008ControlBag.Instance.AnimalsDropEdit));
			AssertLayout(columns[0].Rows[7], nameof(SEDDetails008ControlBag.Instance.EndangeredItemsDropEdit));
			AssertLayout(columns[0].Rows[8], nameof(SEDDetails008ControlBag.Instance.CounterfeitDropEdit));
			AssertLayout(columns[0].Rows[9], nameof(SEDDetails008ControlBag.Instance.CommercialUseItemsDropEdit));
			AssertLayout(columns[0].Rows[10], nameof(SEDDetails008ControlBag.Instance.ExcessTimeLimitItemsDropEdit));
			AssertLayout(columns[0].Rows[11], nameof(SEDDetails008ControlBag.Instance.PornographyDropEdit));
			AssertLayout(columns[0].Rows[12], nameof(SEDDetails008ControlBag.Instance.BranchCodeGuidFindBox));
			AssertLayout(columns[0].Rows[13], nameof(SEDDetails008ControlBag.Instance.BrokerCodeFindBox));
			AssertLayout(columns[0].Rows[14], nameof(SEDDetails008ControlBag.Instance.ServiceCodeFindBox));
		}

		void AssertLayout(PanelLayoutRow row, string column)
		{
			AssertEquals(2, row.Parts.Count);
			AssertEquals(column, row.Parts[1].Name);
		}
	}
}
