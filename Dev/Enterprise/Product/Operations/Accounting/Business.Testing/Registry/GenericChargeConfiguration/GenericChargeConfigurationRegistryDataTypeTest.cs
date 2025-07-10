using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GenericChargeConfigurationRegistryDataType))]
	class GenericChargeConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GenericChargeConfigurationRegistryDataType>
	{
		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs1, NonPersistentBusinessObject rhs1)
		{
			var lhs = lhs1 as GenericChargeConfiguration;
			var rhs = rhs1 as GenericChargeConfiguration;

			AssertEquals("BankAccountPK", lhs.ChargePK, rhs.ChargePK);
		}

		#region Implementation

		protected override GenericChargeConfigurationRegistryDataType GetNewDataType()
		{
			return new GenericChargeConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "GenericChargeConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var charge1 = GetCharge(new Guid("61083EAF-5ADC-4124-8FD2-2DC00819C0AE"), "NA1");
			var charge2 = GetCharge(new Guid("8BAD59A0-F8BD-4569-B2AA-054F05F41458"), "NA2");

			Factory.Save();

			var settings = new GenericChargeConfigurationCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var setting1 = settings.AddNew();
			setting1.ChargePK = charge1.PK;

			var setting2 = settings.AddNew();
			setting2.ChargePK = charge2.PK;

			return new[] { new ValidSampleAndBinaryValueInDB(settings, DataType.Serialise(settings)) };
		}

		#endregion

		BusinessObject GetCharge(Guid pk, string code)
		{
			var result = GetOrNew<AccChargeCode>(Factory, pk, (charge) =>
			{
				charge.AC_GC = Env.CurrentCompany.PK;
				charge.AC_ChargeType = "MRG";
			});
			result.AC_Code = code;

			return result;
		}

		#region Factory

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get { return (factory) ?? (factory = new BusinessObjectFactory()); }
		}

		#endregion
	}
}
