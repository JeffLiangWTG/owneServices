using System;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(LicenceEnterpriseKey))]
	internal class LicenceEnterpriseKeyTest : RegistryBusinessObjectTemplateTestCase<LicenceEnterpriseKey>
	{
		#region Properties

		public void TestLE_PK()
		{
			LicenceEnterprise enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			LicenceEnterpriseKey internalEnterprise = NewPopulatedBusinessObject();
			internalEnterprise.LE_PK = enterprise.PK;

			AssertEquals(enterprise.PK, internalEnterprise.LE_PK);
		}

		#endregion

		#region Lookups

		public void TestLookups()
		{
			LicenceEnterpriseKey internalEnterprise = NewPopulatedBusinessObject();

			AssertNotNull(internalEnterprise.Lookups);
			AssertEquals(typeof(LicenceEnterpriseKeyLookups), internalEnterprise.Lookups.GetType());
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override LicenceEnterpriseKey GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override LicenceEnterpriseKey GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		LicenceEnterpriseKey NewPopulatedBusinessObject()
		{
			LicenceEnterpriseKey result = new LicenceEnterpriseKey(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			return result;
		}

		#endregion
	}
}
