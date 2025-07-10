using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework.Testing
{
	[CodeAlive("used in DependentBusinessObjectCollectionWithClusterKeyParentTest.TestRelationshipFilter")]
	sealed class DependentBusinessObjectCollectionWithClusterKeyParent : DependentBusinessObjectCollection<DummyDependantWithClusterKeyBusinessObject, DummyWithDependentsAndClusterKeyBusinessObject>
	{
		public DependentBusinessObjectCollectionWithClusterKeyParent(DummyWithDependentsAndClusterKeyBusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}
	}
}
