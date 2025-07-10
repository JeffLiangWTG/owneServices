using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business
{
	[TestedType(typeof(AWSPrivateCACollection))]
	class AWSPrivateCACollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AWSPrivateCACollection>
	{
		#region Implementation
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AWSPrivateCACollection GetCollectionToTest()
		{
			return new AWSPrivateCACollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AWSPrivateCA();
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			Assert(Collection.AllowNew);
		}

		#endregion
	}
}
