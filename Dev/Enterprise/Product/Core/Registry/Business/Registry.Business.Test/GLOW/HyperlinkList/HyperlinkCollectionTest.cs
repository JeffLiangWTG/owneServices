using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HyperlinkCollection))]
	sealed class HyperlinkCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<HyperlinkCollection>
	{
		#region AllowNew

		public void TestAllowNew()
		{
			Assert(Collection.AllowNew);
		}

		#endregion

		#region Overrides

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override HyperlinkCollection GetCollectionToTest()
		{
			return new HyperlinkCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Hyperlink();
		}

		#endregion
	}
}
