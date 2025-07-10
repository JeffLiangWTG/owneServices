using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiLicenceSetting))]
	internal class EdiLicenceSettingActualTest : EnterpriseBusinessObjectTestCase
	{
	}

	[TestsSubclassesOf(
		typeof(EdiLicenceSetting),
		new Type[] { typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute), typeof(TestedAsNonPersistentBusinessObjectAttribute) },
		new Type[] { typeof(IDocumentWrapper), typeof(NonPersistentBusinessObject) },
		ExcludeClientDlls = false)]
	internal abstract class EdiLicenceSettingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			var setting = (EdiLicenceSetting)Factory.New(GetExpectedBusinessObjectType());
			AssertNotEquals("LS9_Type should be set during construction", "", setting.LS9_Type);
			setting.Delete();

			setting = (EdiLicenceSetting)GetNewBusinessObjectForDeleteTest(Factory);
			Factory.Save();
			var settingFromDb = new BusinessObjectFactory() { RefreshEnabled = false }.Load<EdiLicenceSetting>(setting.PK);
			AssertType(GetExpectedBusinessObjectType(), settingFromDb);
		}

		public override void TestFetchForLoad()
		{
			Assert(true);
		}

		public void TestValidation()
		{
			var setting = (EdiLicenceSetting)Factory.New(GetExpectedBusinessObjectType());
			AssertNotEquals("GetNewValidation should be overridden to return a specific type, not the base type", typeof(EdiLicenceSettingValidation), setting.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			// Overriding since the base method fails for derived classes
			// since Database.LicenceSettings.AddNew returns the base type and not the derived type.
			var bizo = (EdiLicenceSetting)factory.New(GetExpectedBusinessObjectType());
			bizo.FillWithValidTestData();
			if (bizo.LS9_LD.IsEmpty)
			{
				var db = factory.New<LicenceDatabase>();
				db.FillWithValidTestData();
				bizo.LS9_LD = db.PK;
			}

			return bizo;
		}
	}
}
