using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.AirlineMessagingCargoIMPVersion
{
	[TestedType(typeof(CargoImpVersionConfiguration))]
	sealed class CargoImpVersionConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValue()
		{
			var config = new CargoImpVersionConfiguration();
			AssertEquals(0, config.AirlineImpVersionMappings.Count);
			AssertEquals("V16", config.DefaultImpVersion);
		}

		public void TestValidateDefaultImpVersion()
		{
			var bizObj = (CargoImpVersionConfiguration)BizObj;

			bizObj.DefaultImpVersion = string.Empty;
			AssertHasErrorContaining(bizObj.DefaultImpVersionInfo, "Please enter a Default IMP Version.");

			bizObj.DefaultImpVersion = "ABC";
			AssertHasErrorContaining(bizObj.DefaultImpVersionInfo, "Enter a valid Default IMP Version.");

			bizObj.DefaultImpVersion = "V16";
			AssertNoErrors(bizObj.DefaultImpVersionInfo);

			bizObj.DefaultImpVersion = "V17";
			AssertNoErrors(bizObj.DefaultImpVersionInfo);

			using (bizObj.GetValidationSuspender())
			{
				AssertEquals("Precondition", true, bizObj.IsValidationSuspended);

				bizObj.DefaultImpVersion = "ABC";
				AssertNoErrors(bizObj.DefaultImpVersionInfo);

				bizObj.RunPreSaveValidation();
				AssertHasErrorContaining(bizObj.DefaultImpVersionInfo, "Enter a valid Default IMP Version.");

				bizObj.DefaultImpVersion = "V16";
				bizObj.RunPreSaveValidation();
				AssertNoErrors(bizObj.DefaultImpVersionInfo);

				bizObj.DefaultImpVersion = "V17";
				bizObj.RunPreSaveValidation();
				AssertNoErrors(bizObj.DefaultImpVersionInfo);
			}
		}

		public void TestSortAirlinePrefix()
		{
			var config = new CargoImpVersionConfiguration();
			config.AirlineImpVersionMappings.Add(new AirlineImpVersion()
			{
				AirlinePrefix = "001",
				ImpVersion = "V17"
			});
			config.AirlineImpVersionMappings.Add(new AirlineImpVersion()
			{
				AirlinePrefix = "999",
				ImpVersion = "V17"
			});
			config.AirlineImpVersionMappings.Add(new AirlineImpVersion()
			{
				AirlinePrefix = "020",
				ImpVersion = "V17"
			});
			FreightDataRegistry.Instance.AirlineMessagingCargoImpVersion.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
			Factory.Save();

			var airlinePrefixArray = FreightDataRegistry.Instance.AirlineMessagingCargoImpVersion.Value.AirlineImpVersionMappings
				.OfType<AirlineImpVersion>()
				.Select(x => x.AirlinePrefix)
				.ToArray();

			AssertArrayEqualsByElements(new ZString[] {
				"001",
				"020",
				"999",
			}, airlinePrefixArray);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new CargoImpVersionConfiguration();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new CargoImpVersionConfiguration();
		}
	}
}
