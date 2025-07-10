using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(CriticalityStageMappingCollection))]
	public class CriticalityStageMappingCollectionTest : CodeDescriptionBoolTreeNodeCollectionTest<CriticalityStageMappingCollection>
	{
		#region Implementation

		protected override CriticalityStageMappingCollection GetCollectionToTest()
		{
			var result = new CriticalityStageMappingCollection();
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CriticalityStageMapping();
		}

		#endregion
	}
}
