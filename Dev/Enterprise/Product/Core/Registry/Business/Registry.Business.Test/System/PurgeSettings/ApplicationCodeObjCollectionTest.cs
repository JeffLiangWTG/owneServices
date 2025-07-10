using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ApplicationCodeObjCollection))]
	sealed class ApplicationCodeObjCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ApplicationCodeObjCollection>
	{
		public void TestAddApplicationCode()
		{
			var collection = new ApplicationCodeObjCollection();
			AssertEquals(0, collection.Count);

			collection.AddApplicationCode("AAA",
				new InterchangeObjCollection() { new InterchangeObj() { Selected = true } },
				new MessageSubTypePurgeTypeObjCollection().Add("VVV", "VVV", 2, TimeUnit.Week));
			AssertEquals(1, collection.Count);
			var applicationCodeObj = collection[0];
			AssertEquals("AAA", applicationCodeObj.ApplicationCode);
			AssertEquals(1, applicationCodeObj.Interchanges.Count);
			AssertEquals(true, applicationCodeObj.Interchanges[0].Selected);
			AssertEquals(1, applicationCodeObj.MessageTypes.Count);
			AssertEquals("VVV", applicationCodeObj.MessageTypes[0].MessageSubType);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ApplicationCodeObjCollection GetCollectionToTest()
		{
			return new ApplicationCodeObjCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ApplicationCodeObj();
		}
	}
}
