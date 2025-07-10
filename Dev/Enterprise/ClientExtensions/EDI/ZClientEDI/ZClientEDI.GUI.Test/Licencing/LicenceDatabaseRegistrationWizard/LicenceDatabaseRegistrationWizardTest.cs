using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(LicenceDatabaseRegistrationWizard))]
	public class LicenceDatabaseRegistrationWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLicenceDatabaseCollection()
		{
			var lic1 = Factory.NewWithValidTestData<LicenceDatabase>();
			lic1.LD_Product = "PR1";
			var lic2 = Factory.NewWithValidTestData<LicenceDatabase>();
			lic2.LD_Product = "PR2";
			Factory.Save();

			var licenceDatabaseRegistration = GetNewBusinessObject() as LicenceDatabaseRegistrationWizard;
			Assert(licenceDatabaseRegistration.LicenceDatabaseCollection.Any(l => ((LicenceDatabase)l).LD_Product == "PR1"));
			Assert(licenceDatabaseRegistration.LicenceDatabaseCollection.Any(l => ((LicenceDatabase)l).LD_Product == "PR2"));
		}

		public void TestOrgHeaderCollection()
		{
			var lic1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			lic1.OH_Code = "code1";
			lic1.OH_Category = "NGO";
			var lic2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			lic2.OH_Code = "code2";
			lic2.OH_Category = "NGO";
			Factory.Save();

			var licenceDatabaseRegistration = GetNewBusinessObject() as LicenceDatabaseRegistrationWizard;
			licenceDatabaseRegistration.OrgHeaderCollection.Load(new ZQuery(OrgHeaderSchema.OH_Category, "NGO"));
			Assert(licenceDatabaseRegistration.OrgHeaderCollection.Any(l => ((EDIOrgHeader)l).OH_Code == "code1"));
			Assert(licenceDatabaseRegistration.OrgHeaderCollection.Any(l => ((EDIOrgHeader)l).OH_Code == "code2"));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LicenceDatabaseRegistrationWizard(Factory);
		}

		#endregion
	}
}
