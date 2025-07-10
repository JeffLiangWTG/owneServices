using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(ResolutionAndClosureBehaviourCollection))]
	public class ResolutionAndClosureBehaviourCollectionTest : CodeDescriptionBoolTreeNodeCollectionTest<ResolutionAndClosureBehaviourCollection>
	{
		#region Implementation

		protected override ResolutionAndClosureBehaviourCollection GetCollectionToTest()
		{
			var result = new ResolutionAndClosureBehaviourCollection();
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ResolutionAndClosureBehaviour();
		}

		#endregion
	}
}
