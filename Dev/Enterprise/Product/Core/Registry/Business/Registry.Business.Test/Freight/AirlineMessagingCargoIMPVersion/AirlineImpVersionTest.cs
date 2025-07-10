using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.AirlineMessagingCargoIMPVersion
{
	[TestedType(typeof(AirlineImpVersion))]
	sealed class AirlineImpVersionTest : RegistryBusinessObjectTemplateTestCase<AirlineImpVersion>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var registryCollection = RegistryValue.AirlineImpVersionMappings;
			return registryCollection.AddNew();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override AirlineImpVersion GetBusinessObjectToClone()
		{
			return (AirlineImpVersion)GetNewBusinessObject();
		}

		protected override AirlineImpVersion GetBusinessObjectToSerialise()
		{
			return (AirlineImpVersion)GetNewBusinessObject();
		}

		public void TestValueReload()
		{
			var config = new CargoImpVersionConfiguration();
			config.AirlineImpVersionMappings.RemoveAndDeleteAll();

			var newCargoImpVersionBusinessObject = new AirlineImpVersion
			{
				AirlinePrefix = "123",
				ImpVersion = "V17"
			};
			newCargoImpVersionBusinessObject.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			config.AirlineImpVersionMappings.Add(newCargoImpVersionBusinessObject);
			FreightDataRegistry.Instance.AirlineMessagingCargoImpVersion.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var registryReload = FreightDataRegistry.Instance.AirlineMessagingCargoImpVersion.Value.AirlineImpVersionMappings.Cast<AirlineImpVersion>();
			var reloadBusinessObject = registryReload.FirstOrDefault();

			AssertEquals(1, registryReload.Count());
			AssertEquals("123", reloadBusinessObject.AirlinePrefix);
			AssertEquals("V17", reloadBusinessObject.ImpVersion);
		}

		public void TestSetAirlinePrefix()
		{
			var businessObjectCollection = RegistryValue.AirlineImpVersionMappings;
			var newItem = businessObjectCollection.AddNew();

			newItem.AirlinePrefix = "";
			AssertHasError(newItem.AirlinePrefixInfo, "Please enter a value.");

			newItem.AirlinePrefix = "NotValidPrefix";
			AssertHasError(newItem.AirlinePrefixInfo, "Enter a valid selection.");

			foreach (ICodeDescription airlinePrefixCodeDescription in newItem.AirlinePrefixes)
			{
				newItem.AirlinePrefix = airlinePrefixCodeDescription.Code;
				AssertNoErrors(newItem.AirlinePrefixInfo);
			}

			var anotherNewItem = businessObjectCollection.AddNew();
			anotherNewItem.AirlinePrefix = newItem.AirlinePrefix;
			AssertHasError(anotherNewItem.AirlinePrefixInfo, $"Airline prefix of '{anotherNewItem.AirlinePrefix}' has already been specified.");
		}

		public void TestSetImpVersion()
		{
			var businessObjectCollection = RegistryValue.AirlineImpVersionMappings;
			var newItem = businessObjectCollection.AddNew();

			newItem.ImpVersion = "";
			AssertHasError(newItem.ImpVersionInfo, "Please enter a value.");

			newItem.ImpVersion = "NotValidData";
			AssertHasError(newItem.ImpVersionInfo, "Enter a valid selection.");

			foreach (ICodeDescription codeDescription in newItem.ImpVersions)
			{
				newItem.ImpVersion = codeDescription.Code;
				AssertNoErrors(newItem.AirlinePrefixInfo);
			}
		}

		public void TestPreSaveValidation()
		{
			var registryCollection = RegistryValue.AirlineImpVersionMappings;

			var validRow = registryCollection.AddNew();
			validRow.AirlinePrefix = validRow.AirlinePrefixes.Cast<ICodeDescription>().FirstOrDefault().Code;
			validRow.ImpVersion = validRow.ImpVersions.Cast<ICodeDescription>().FirstOrDefault().Code;

			var duplicatedItem = registryCollection.AddNew();
			duplicatedItem.AirlinePrefix = validRow.AirlinePrefixes.Cast<ICodeDescription>().FirstOrDefault().Code;
			duplicatedItem.ImpVersion = validRow.ImpVersions.Cast<ICodeDescription>().FirstOrDefault().Code;

			var invalidRow = registryCollection.AddNew();
			invalidRow.AirlinePrefix = "AA";
			invalidRow.ImpVersion = "BB";

			var emptyRow = registryCollection.AddNew();
			emptyRow.AirlinePrefix = ZString.Empty;
			emptyRow.ImpVersion = ZString.Empty;

			validRow.RunPreSaveValidation();
			AssertNoErrors(validRow);

			duplicatedItem.RunPreSaveValidation();
			AssertHasError(duplicatedItem.AirlinePrefixInfo, $"Airline prefix of '{duplicatedItem.AirlinePrefix}' has already been specified.");

			invalidRow.RunPreSaveValidation();
			AssertHasError(invalidRow.AirlinePrefixInfo, "Enter a valid selection.");
			AssertHasError(invalidRow.ImpVersionInfo, "Enter a valid selection.");

			emptyRow.RunPreSaveValidation();
			AssertHasError(emptyRow.AirlinePrefixInfo, "Please enter a value.");
			AssertHasError(emptyRow.ImpVersionInfo, "Please enter a value.");
		}

		CargoImpVersionConfiguration RegistryValue
		{
			get
			{
				var result = FreightDataRegistry.Instance.AirlineMessagingCargoImpVersion.Value;
				result.AirlineImpVersionMappings.RemoveAll();
				result.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				return result;
			}
		}
	}
}
