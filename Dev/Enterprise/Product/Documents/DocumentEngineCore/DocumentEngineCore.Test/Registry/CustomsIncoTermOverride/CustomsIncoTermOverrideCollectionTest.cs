using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(CustomsIncoTermOverrideCollection))]
	public class CustomsIncoTermOverrideCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CustomsIncoTermOverrideCollection>
	{
		public void TestAddNew_WithParameters()
		{
			CustomsIncoTermOverrideCollection collection = new CustomsIncoTermOverrideCollection();
			CustomsIncoTermOverride incoTerm = collection.AddNew("FDD", Core.Constants.IncoTerms.FreeAlongsideShip);
			AssertEquals("CustomsCode", "FDD", incoTerm.CustomsCode);
			AssertEquals("InternationalCode", Core.Constants.IncoTerms.FreeAlongsideShip, incoTerm.InternationalCode);
			AssertCollectionContains(incoTerm, collection);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CustomsIncoTermOverrideCollection GetCollectionToTest()
		{
			return new CustomsIncoTermOverrideCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CustomsIncoTermOverride();
		}
	}
}
