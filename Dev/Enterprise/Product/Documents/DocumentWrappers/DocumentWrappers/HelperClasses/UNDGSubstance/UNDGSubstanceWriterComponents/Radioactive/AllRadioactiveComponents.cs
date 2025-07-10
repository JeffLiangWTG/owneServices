using System.Collections.Generic;

namespace Enterprise.DocumentWrappers
{
	static class AllRadioactiveComponents
	{
		public static IEnumerable<IUNDGSummaryWriterComponent> GetRadioactiveComponents()
		{
			yield return new RadionuclideComponent();
			yield return new MaterialFormDescriptionComponent();
			yield return new RadioactiveLabelCategoryComponent();
			yield return new RadioactiveTransportIndexComponent();
			yield return new HRCQComponent();
			yield return new FissileExceptedComponent();
		}
	}
}
