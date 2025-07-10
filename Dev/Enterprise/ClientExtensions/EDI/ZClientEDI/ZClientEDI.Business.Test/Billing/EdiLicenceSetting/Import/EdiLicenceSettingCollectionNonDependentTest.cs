using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiLicenceSettingCollectionNonDependent))]
	public class EdiLicenceSettingCollectionNonDependentTest : ActiveBusinessObjectCollectionTestCase<EdiLicenceSettingCollectionNonDependent>
	{
		public void TestAdhocCollection()
		{
			var collection = new EdiLicenceSettingCollectionNonDependent(Factory);
			var item1 = Factory.New<EdiLicenceSetting>();
			var item2 = Factory.New<EdiLicenceSetting>();
			collection.Add(item1);
			collection.Add(item2);
			AssertEquals(2, collection.Count);
		}

		protected override EdiLicenceSettingCollectionNonDependent GetCollectionToTest()
		{
			return new EdiLicenceSettingCollectionNonDependent(Factory);
		}
	}
}
