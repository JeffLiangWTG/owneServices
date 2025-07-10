using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAPivotCollectionForContainer))]
	sealed class CusSCAPivotCollectionForContainerTest : ActiveBusinessObjectCollectionTestCase<CusSCAPivotCollectionForContainer>
	{
		#region Implementation

		CusSCAContainer cusSCAContainer;
		CusSCAContainer CusSCAContainer
		{
			get
			{
				if (cusSCAContainer == null)
				{
					cusSCAContainer = Factory.New<CusSCAContainer>();
				}
				return cusSCAContainer;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusSCAPivot>();
		}

		protected override CusSCAPivotCollectionForContainer GetCollectionToTest()
		{
			return new CusSCAPivotCollectionForContainer(CusSCAContainer);
		}

		#endregion
	}
}
