using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TGE.Business.Testing
{
	[TestedType(typeof(TGEEventRegistryBusinessObjectCollection))]
	public class TGEEventRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TGEEventRegistryBusinessObjectCollection>
	{
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override TGEEventRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new TGEEventRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TGEEventRegistryBusinessObject();
		}
	}
}
