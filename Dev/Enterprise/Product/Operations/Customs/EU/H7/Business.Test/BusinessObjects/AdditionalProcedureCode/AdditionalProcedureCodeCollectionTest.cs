using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AdditionalProcedureCodeCollection))]
	class AdditionalProcedureCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAdditionalProcedureCodeType()
		{
			var collection = (AdditionalProcedureCodeCollection)GetCollectionToTest();
			var additionalProcedureCode = collection.AddNew();
			AssertType<AdditionalProcedureCode>(additionalProcedureCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<AsycudaBill>().AdditionalProcedureCodes;
	}
}
