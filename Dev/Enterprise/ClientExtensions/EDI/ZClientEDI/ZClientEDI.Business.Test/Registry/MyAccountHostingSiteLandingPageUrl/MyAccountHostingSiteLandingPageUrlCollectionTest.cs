using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	[TestedType(typeof(MyAccountHostingSiteLandingPageUrlCollection))]
	public class MyAccountHostingSiteLandingPageUrlCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MyAccountHostingSiteLandingPageUrlCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MyAccountHostingSiteLandingPageUrl(new FallbackLevel(Environment.Env.CurrentCompany, Environment.Env.CurrentBranch, Environment.Env.CurrentDepartment), Factory, GetCollectionToTest());
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override MyAccountHostingSiteLandingPageUrlCollection GetCollectionToTest()
		{
			return collection ?? (collection = new MyAccountHostingSiteLandingPageUrlCollection());
		}

		MyAccountHostingSiteLandingPageUrlCollection collection;
	}
}
