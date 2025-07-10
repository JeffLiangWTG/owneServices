using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business
{
	[TestedType(typeof(VisualizerMenuTemplatePivotCollection))]
	sealed class VisualizerMenuTemplatePivotCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new VisualizerMenuTemplatePivotCollection(Factory, new ZQuery());
		}

		#endregion
	}
}
