using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InterchangeObjCollection))]
	sealed class InterchangeObjCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InterchangeObjCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override InterchangeObjCollection GetCollectionToTest()
		{
			return new InterchangeObjCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InterchangeObj();
		}
	}
}
