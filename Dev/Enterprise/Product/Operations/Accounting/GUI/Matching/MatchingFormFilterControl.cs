using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	/// <summary>
	/// Filter control for MatchingForm.
	/// </summary>
	public class MatchingFormFilterControl : AccountingOnFormFilterControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new MatchingFormFilterStrip();
		}

		public MatchingFormFilterControl(FilterStripBusinessObject filterBusinessObject)
			: base(null, filterBusinessObject)
		{
		}

		public void ResetFiltersLayout()
		{
			ResetLayout();
		}
	}
}
