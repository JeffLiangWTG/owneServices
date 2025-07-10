using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(CriticalityStageMappingCollectionView))]
	public class CriticalityStageMappingCollectionViewTest : CodeDescriptionBoolTreeViewTest<CriticalityStageMappingCollectionView>
	{
		public void TestAllowNew()
		{
			var allNodes = new CriticalityStageMappingCollection();

			var view1 = new CriticalityStageMappingCollectionView(allNodes, ZGuid.Empty);
			AssertEquals(false, view1.AllowNew);

			var view2 = new CriticalityStageMappingCollectionView(allNodes, ZGuid.NewZGuid());
			AssertEquals(true, view2.AllowNew);
		}

		protected override CriticalityStageMappingCollectionView GetCollectionToTest()
		{
			var allNodes = new CriticalityStageMappingCollection();
			return new CriticalityStageMappingCollectionView(allNodes, ZGuid.Empty);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = new CriticalityStageMapping();
			return result;
		}
	}
}
