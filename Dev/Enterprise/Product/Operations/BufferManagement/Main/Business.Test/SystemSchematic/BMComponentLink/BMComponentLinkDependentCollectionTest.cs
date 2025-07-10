using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentLinkDependentCollection))]
	public class BMComponentLinkDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<BMComponentLinkDependentCollection>
	{
		protected override BMComponentLinkDependentCollection GetCollectionToTest()
		{
			return new BMComponentLinkDependentCollection(GetComponent("Dexter"), BMComponentLinkSchema.FL_FC_ComponentFrom);
		}

		protected BMComponent GetComponent(string name)
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Name = name;
			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			return component;
		}
	}
}
