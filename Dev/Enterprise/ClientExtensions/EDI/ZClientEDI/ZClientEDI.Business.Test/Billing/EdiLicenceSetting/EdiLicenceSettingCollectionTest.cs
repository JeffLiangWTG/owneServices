using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiLicenceSettingCollection))]
	internal class EdiLicenceSettingCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiLicenceSettingCollection>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			EdiLicenceSetting item = Collection.AddNew();
			AssertEquals("Master", Database.PK, item.LS9_LD);
		}

		#region Implementation

		LicenceDatabase Database;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiLicenceSettingCollection);
		}

		protected override EdiLicenceSettingCollection GetCollectionToTest()
		{
			Database = Factory.NewWithValidTestData<LicenceDatabase>();
			return new EdiLicenceSettingCollection(Database);
		}

		#endregion
	}
}
