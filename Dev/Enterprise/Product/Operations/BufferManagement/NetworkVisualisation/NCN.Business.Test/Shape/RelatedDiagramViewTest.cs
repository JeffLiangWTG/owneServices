using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(RelatedDiagramView))]
	class RelatedDiagramViewTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var mainDiagramShape = NetworkTestCase.CreateDiagram(Factory);
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			return new RelatedDiagramView(mainDiagramShape, diagramShape);
		}
	}
}
