using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2JobDocAddressCollection))]
	internal class ICS2JobDocAddressCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var billScreening = Factory.NewWithValidTestData<AsycudaBillScreening>();
			return new ICS2JobDocAddressCollection(billScreening);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ICS2JobDocAddress>();
	}
}
