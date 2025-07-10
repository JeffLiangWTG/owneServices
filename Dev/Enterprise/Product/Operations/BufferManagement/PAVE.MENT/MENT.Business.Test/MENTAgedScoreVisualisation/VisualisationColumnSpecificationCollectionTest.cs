using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(VisualisationColumnSpecificationCollection))]
	class VisualisationColumnSpecificationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<VisualisationColumnSpecificationCollection>
	{
		protected override VisualisationColumnSpecificationCollection GetCollectionToTest()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Test Visualisation 1", save: false);
			return new VisualisationColumnSpecificationCollection(visualisation);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Test Visualisation 2", save: false);
			return new VisualisationColumnSpecification(visualisation);
		}
	}
}
