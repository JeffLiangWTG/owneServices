using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(ResolutionAndClosureBehaviourCollectionView))]
	public class ResolutionAndClosureBehaviourCollectionViewTest : CodeDescriptionBoolTreeViewTest<ResolutionAndClosureBehaviourCollectionView>
	{
		protected override ResolutionAndClosureBehaviourCollectionView GetCollectionToTest()
		{
			var allNodes = new ResolutionAndClosureBehaviourCollection();
			return new ResolutionAndClosureBehaviourCollectionView(allNodes, ZGuid.Empty);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = new ResolutionAndClosureBehaviour();
			return result;
		}
	}
}
