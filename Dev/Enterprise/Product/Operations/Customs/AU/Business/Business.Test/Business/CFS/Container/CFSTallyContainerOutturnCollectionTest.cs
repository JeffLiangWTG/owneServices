using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CFSTallyContainerOutturnCollection))]
	public class CFSTallyContainerOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CFSTallyContainerOutturnCollection(Wrapper);
		}

		CFSTallyContainerWrapper Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = CFSTallyContainerWrapper.Load(Factory.New<TallyContainer>());
				}
				return wrapper;
			}
		}
		CFSTallyContainerWrapper wrapper;
	}
}
