using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.GlobalChargeCode;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocGlobalChargeCodeCollection))]
	public class DocGlobalChargeCodeCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocGlobalChargeCodeCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var chargeCode = Factory.New<GlobalChargeCodeMapOrganization>();
			return DocGlobalChargeCode.New(chargeCode, Factory);
		}

		protected override DocGlobalChargeCodeCollection GetCollectionToTest()
		{
			return new DocGlobalChargeCodeCollection(Factory);
		}

		public void TestCollectionInitialisationConstructor()
		{
			var chargeCodeMaps = System.Array.Empty<GlobalChargeCodeMap>();
			var coll = new DocGlobalChargeCodeCollection(chargeCodeMaps, Factory);
			AssertNotNull(coll);
			AssertEquals(coll.Count, 0);

			chargeCodeMaps = new GlobalChargeCodeMap[] { Factory.New<GlobalChargeCodeMapOrganization>() };
			coll = new DocGlobalChargeCodeCollection(chargeCodeMaps, Factory);
			AssertNotNull(coll);
			AssertEquals(coll.Count, 1);

			chargeCodeMaps = new GlobalChargeCodeMap[] { Factory.New<GlobalChargeCodeMapOrganization>(), Factory.New<GlobalChargeCodeMapOrganization>() };
			coll = new DocGlobalChargeCodeCollection(chargeCodeMaps, Factory);
			AssertNotNull(coll);
			AssertEquals(coll.Count, 2);
		}
	}
}
