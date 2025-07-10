using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(NGTRegistryItem))]
	sealed class NGTRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<NGT>
	{
		public void TestRegistryOptions()
		{
			AssertEquals("Options", RegistryOptions.IsOnlyForController, GetNewRegistryItem().Options);
		}

		protected override StronglyTypedRegistryItem<NGT, NGT> GetNewRegistryItem()
		{
			return new NGTRegistryItem("", null, null, null);
		}
	}
}
