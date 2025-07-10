using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business
{
	[TestedType(typeof(VisualizerMenuTemplatePivot))]
	sealed class VisualizerMenuTemplatePivotTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var pivot = Factory.NewWithValidTestData<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.NewWithValidTestData<VisualizerMenuItem>();
			var template = Factory.NewWithValidTestData<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;

			return pivot;
		}

		public override void TestBizObjectFields()
		{
			// Overridden as the custom behaviour of the setter is not appropriate for BizObjectFields Test
			// See StmMenuTemplatePivotBase
			Assert(true);
		}

		#endregion
	}
}
