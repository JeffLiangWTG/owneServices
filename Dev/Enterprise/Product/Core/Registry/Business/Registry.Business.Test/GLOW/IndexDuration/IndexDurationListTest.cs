using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IndexDurationList))]
	sealed class IndexDurationListTest : RegistryBusinessObjectCollectionTemplateTestCase<IndexDurationList>
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

		protected override IndexDurationList GetCollectionToTest()
		{
			return new IndexDurationList();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IndexDuration();
		}

		#endregion
	}
}
