using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsWebServiceItemCollection))]
	sealed class DpsWebServiceItemCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DpsWebServiceItemCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override DpsWebServiceItemCollection GetCollectionToTest()
		{
			return new DpsWebServiceItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DpsWebServiceItem();
		}

		public void TestAllowNew()
		{
			AssertEquals(true, Collection.AllowNew);

			var collection = new DpsWebServiceItemCollection();

			collection.AddNew();
			collection[0].Code = "CODE";
			collection[0].WebServiceUrl = "https://webservice.net";
			collection[0].Role = RoleHelper.Code.Production;

			CombineAssertions("web service urls should allow addition of new element", () =>
			{
				AssertEquals("CODE", collection[0].Code);
				AssertEquals("https://webservice.net", collection[0].WebServiceUrl);
				AssertEquals(RoleHelper.Code.Production, collection[0].Role);
			});
		}

		public void TestAllowRemove()
		{
			AssertEquals(true, Collection.AllowRemove);

			var collection = new DpsWebServiceItemCollection();

			collection.AddNew();
			collection[0].Code = "CODE";
			collection[0].WebServiceUrl = "https://webservice.net";
			collection[0].Role = RoleHelper.Code.Production;

			AssertEquals(1, collection.Count);
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestSystemDefaultValues()
		{
			var collection = new DpsWebServiceItemCollection().DefaultValue;

			CombineAssertions("web service urls should have the default values", () =>
			{
				AssertEquals("SYD1", collection[0].Code);
				AssertEquals("https://dpsv4.wisegrid.net", collection[0].WebServiceUrl);
				AssertEquals(RoleHelper.Code.Production, collection[0].Role);

				AssertEquals("ORD1", collection[1].Code);
				AssertEquals("https://dpsv4-usord.wisegrid.net", collection[1].WebServiceUrl);
				AssertEquals(RoleHelper.Code.ProductionFailover, collection[1].Role);

				AssertEquals("STG1", collection[2].Code);
				AssertEquals("https://dpsv4-test.wisegrid.net", collection[2].WebServiceUrl);
				AssertEquals(RoleHelper.Code.Staging, collection[2].Role);
			});
		}

		public new void TestClone()
		{
			var currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new DpsWebServiceItemCollection();
			var originalItem = collection.AddNew();
			originalItem.Code = "OG01";
			originalItem.WebServiceUrl = "https://webservice.net";
			originalItem.Role = RoleHelper.Code.ProductionFailover;

			var clone = collection.Clone(currentFallbackLevel, Factory);
			AssertNotEquals("Clone should be a different instance.", collection, clone);

			var cloneElement = clone[0] as DpsWebServiceItem;

			CombineAssertions("clone element and original item should have identical properties", () =>
			{
				AssertEquals(cloneElement.Code, originalItem.Code);
				AssertEquals(cloneElement.WebServiceUrl, originalItem.WebServiceUrl);
				AssertEquals(cloneElement.Role, originalItem.Role);
			});
		}
	}
}
