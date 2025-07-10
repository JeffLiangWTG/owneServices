using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3DeclarationMessageCollection))]
	sealed class G3DeclarationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new G3DeclarationMessageCollection(Factory);
		}

		public void TestLoadElements()
		{
			var g3Declaration = Factory.New<G3EDIMessage>();
			g3Declaration.EM_MessageType = "G3D";

			var g3Revoke = Factory.New<G3EDIMessage>();
			g3Revoke.EM_MessageType = "G3R";

			var collection = new G3DeclarationMessageCollection(Factory);
			collection.Load();

			Assert(collection.Contains(g3Declaration));
			Assert(collection.Contains(g3Revoke));
		}
	}
}
