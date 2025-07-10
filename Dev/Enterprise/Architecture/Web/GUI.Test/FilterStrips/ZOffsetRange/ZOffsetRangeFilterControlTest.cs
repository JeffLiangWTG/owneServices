using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class ZOffsetRangeFilterControlTest : ZCompositeWebControlTest
	{
		public void TestDaysOffsetControlMarkup()
		{
			var offsetControl = new ZOffsetRangeFilterControl(OffsetRangeMeasure.Days);
			var controls = offsetControl.Controls;
			var labels = controls.OfType<Label>();
			var inputs = controls.OfType<ZNumericTextBox>();
			var dropdowns = controls.OfType<ZDropDownList>();

			AssertEquals(3, labels.Count());
			AssertNotNull(labels.Single(l => l.Text == "At least"));
			AssertNotNull(labels.Single(l => l.Text == "At most"));
			AssertNotNull(labels.Single(l => l.Text == "Days in the"));

			AssertEquals(2, inputs.Count());
			AssertNotNull(inputs.Single(i => i.BindTo == "PropertyDecimal1" && i.Decimals == 2 && i.MaxLength == 4));
			AssertNotNull(inputs.Single(i => i.BindTo == "PropertyDecimal2" && i.Decimals == 2 && i.MaxLength == 4));

			AssertEquals(1, dropdowns.Count());
			AssertNotNull(dropdowns.Single(d => d.BindTo == "FilterOption" && d.DisplayStyle == Core.OComboBoxDropDownStyle.CodeOnly));
		}

		public void TestHoursOffsetControlMarkup()
		{
			var offsetControl = new ZOffsetRangeFilterControl(OffsetRangeMeasure.Hours);
			var controls = offsetControl.Controls;
			var labels = controls.OfType<Label>();
			var inputs = controls.OfType<ZTimeFilterControl>();
			var dropdowns = controls.OfType<ZDropDownList>();

			AssertEquals(3, labels.Count());
			AssertNotNull(labels.Single(l => l.Text == "At least"));
			AssertNotNull(labels.Single(l => l.Text == "At most"));
			AssertNotNull(labels.Single(l => l.Text == "Hours in the"));

			AssertEquals(2, inputs.Count());
			AssertNotNull(inputs.Single(i => i.BindTo == "Property1"));
			AssertNotNull(inputs.Single(i => i.BindTo == "Property2"));

			AssertEquals(1, dropdowns.Count());
			AssertNotNull(dropdowns.Single(d => d.BindTo == "FilterOption" && d.DisplayStyle == Core.OComboBoxDropDownStyle.CodeOnly));
		}

		protected override Control GetNewControl() => new ZOffsetRangeFilterControl(OffsetRangeMeasure.Days);
	}
}
