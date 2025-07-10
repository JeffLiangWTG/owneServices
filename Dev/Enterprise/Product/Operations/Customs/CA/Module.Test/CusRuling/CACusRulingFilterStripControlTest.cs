using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class CACusRulingFilterStripControlTest : ZFilterStripControlTest
	{
		public void TestFilteredGridDetails()
		{
			var filterObject = new CACusRulingFilterStripBusinessObject();
			var gridCollection = new ZZRefCusRulingCombinedCollection(Factory);
			using (var control = new CACusRulingFilterStripControl(gridCollection, filterObject))
			{
				control.Show();
				AssertEquals("Remission Number", control.FilteredGrid.GetColumnCaption(CACusRuling.Schema.ZZX_RulingNumber));
				AssertEquals("Remission Type", control.FilteredGrid.GetColumnCaption(CACusRuling.Schema.ZZX_RulingType));
				AssertEquals("Remission Type Description", control.FilteredGrid.GetColumnCaption(CACusRuling.Schema.RulingTypeDescription));
			}
		}
	}
}
