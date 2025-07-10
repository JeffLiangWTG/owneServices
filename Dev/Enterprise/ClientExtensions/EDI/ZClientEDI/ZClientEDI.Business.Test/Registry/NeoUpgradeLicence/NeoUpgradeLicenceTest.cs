using System;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(NeoUpgradeLicence))]
	internal sealed class NeoUpgradeLicenceTest : RegistryBusinessObjectTemplateTestCase<NeoUpgradeLicence>
	{
		public void TestEnterpriseCodeIsSetFromSelectedLicence()
		{
			var licence1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licence1.LE_EnterpriseCode = "LE1";

			var licence2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			licence2.LE_EnterpriseCode = "LE2";

			Factory.Save();

			var upgradeLicence = NewPopulatedBusinessObject();
			AssertEquals(ZString.Empty, upgradeLicence.EnterpriseCode);

			upgradeLicence.LicencePK = licence1.PK;
			AssertEquals("LE1", upgradeLicence.EnterpriseCode);

			upgradeLicence.LicencePK = licence2.PK;
			AssertEquals("LE2", upgradeLicence.EnterpriseCode);

			upgradeLicence.LicencePK = ZGuid.NewZGuid();
			AssertEquals(ZString.Empty, upgradeLicence.EnterpriseCode);
		}

		public void TestValidateLicencePK()
		{
			var upgradeLicence = NewPopulatedBusinessObject();
			AssertEquals(ZGuid.Empty, upgradeLicence.LicencePK);
			AssertNoErrors(upgradeLicence.LicencePKInfo);

			upgradeLicence.ValidateLicencePK();
			AssertHasErrors(upgradeLicence.LicencePKInfo);

			upgradeLicence.LicencePK = ZGuid.NewZGuid();
			upgradeLicence.ValidateLicencePK();
			AssertHasErrors(upgradeLicence.LicencePKInfo);

			var licence = Factory.NewWithValidTestData<LicenceEnterprise>();
			licence.LE_EnterpriseCode = "LIC";
			Factory.Save();

			upgradeLicence.LicencePK = licence.PK;
			upgradeLicence.ValidateLicencePK();
			AssertNoErrors(upgradeLicence.LicencePKInfo);
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override NeoUpgradeLicence GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override NeoUpgradeLicence GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		NeoUpgradeLicence NewPopulatedBusinessObject() => new NeoUpgradeLicence(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

		#endregion
	}
}
