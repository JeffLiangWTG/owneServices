using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiLicenceSettingFlattenedCollection))]
	public class EdiLicenceSettingFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EdiLicenceSettingFlattenedCollection>
	{
		protected override EdiLicenceSettingFlattenedCollection GetCollectionToTest()
		{
			return new EdiLicenceSettingFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EdiLicenceSettingFlattened();
		}
	}
}
