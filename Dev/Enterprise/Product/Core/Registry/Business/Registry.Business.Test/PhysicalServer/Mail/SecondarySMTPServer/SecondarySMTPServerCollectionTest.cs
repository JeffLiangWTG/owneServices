using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(SecondarySMTPServerCollection))]
	sealed class SecondarySMTPServerCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SecondarySMTPServerCollection>
	{
		#region Implementation
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override SecondarySMTPServerCollection GetCollectionToTest()
		{
			return new SecondarySMTPServerCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SecondarySMTPServer();
		}

		#endregion
	}
}
