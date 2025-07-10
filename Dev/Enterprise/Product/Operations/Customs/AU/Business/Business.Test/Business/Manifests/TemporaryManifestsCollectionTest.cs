using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TemporaryManifestsCollection))]
	public class TemporaryManifestsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryManifestsCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, collection.AllowRemove);
		}

		#region Overrides

		protected override TemporaryManifestsCollection GetCollectionToTest()
		{
			return new TemporaryManifestsCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TemporaryManifest(Factory, new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			collection = new TemporaryManifestsCollection(Factory);
		}

		TemporaryManifestsCollection collection;

		#endregion
	}
}
