using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(GlbExternalPasswordCUSCollection))]
	sealed class GlbExternalPasswordCUSCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbExternalPasswordCUSCollection(Factory.NewWithValidTestData<GlbStaff>());
		}

		public void TestFilter()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			company1.GC_Code = "TC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "TB1";
			branch1.GB_IsActive = true;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			company2.GC_Code = "TC2";
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "TB2";
			branch2.GB_IsActive = true;
			Factory.Save();
			using (DisposableEnvironment.ForCompany(company1.GC_Code))
			{
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_Code = "TS1";
				var cusPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
				cusPassword.GP_GC = company1.PK;
				cusPassword.GP_GS = staff1.PK;
				cusPassword.GP_PasswordType = JPPasswordType.Codes.CUS;
				var uvcPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
				uvcPassword.GP_GC = company1.PK;
				uvcPassword.GP_GS = staff1.PK;
				uvcPassword.GP_PasswordType = PasswordTypesList.Codes.UVC;
				var itbPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
				itbPassword.GP_GC = company1.PK;
				itbPassword.GP_GS = staff1.PK;
				itbPassword.GP_PasswordType = PasswordTypesList.Codes.ITB;
				var cusPasswordCo2 = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
				cusPasswordCo2.GP_GC = company2.PK;
				cusPasswordCo2.GP_GS = staff1.PK;
				cusPasswordCo2.GP_PasswordType = JPPasswordType.Codes.CUS;
				Factory.Save();
				var collection = new GlbExternalPasswordCUSCollection(staff1);
				collection.Load();
				CombineAssertions(() =>
				{
					AssertCollectionContains("CUS Password", cusPassword, collection);
					AssertCollectionNotContains("UVC", uvcPassword, collection);
					AssertCollectionNotContains("ITB", itbPassword, collection);
					AssertCollectionNotContains("CUS other company", cusPasswordCo2, collection);
					AssertEquals("collection.Count", 1, collection.Count);
				});
			}
		}

		protected override Type GetExpectedCollectionType() => typeof(GlbExternalPasswordCUSCollection);
	}
}
