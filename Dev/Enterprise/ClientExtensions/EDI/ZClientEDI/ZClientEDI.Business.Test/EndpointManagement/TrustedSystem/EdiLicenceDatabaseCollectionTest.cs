using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	[TestedType(typeof(EdiLicenceDatabaseCollection))]
	public class EdiLicenceDatabaseCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiLicenceDatabaseCollection>
	{
		public void TestThrowIfRelationshipNotSupported()
		{
			AssertNoExceptionThrown("Property throwIfRelationshipNotSupported should be set to false", () =>
			{
				Collection.Add(Factory.NewWithValidTestData<LicenceDatabase>());
			});
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiLicenceDatabaseCollection);
		}
	}
}
