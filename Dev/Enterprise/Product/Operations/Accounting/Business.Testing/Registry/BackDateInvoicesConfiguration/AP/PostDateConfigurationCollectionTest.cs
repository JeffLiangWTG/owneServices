using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PostDateConfigurationCollection))]
	public class PostDateConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PostDateConfigurationCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override PostDateConfigurationCollection GetCollectionToTest()
		{
			return new PostDateConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PostDateConfiguration();
		}

		#endregion
	}
}
