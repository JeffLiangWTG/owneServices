using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PurgeSettings))]
	sealed class PurgeSettingsTest : RegistryBusinessObjectTemplateTestCase<PurgeSettings>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override PurgeSettings GetBusinessObjectToClone()
		{
			return (PurgeSettings)GetNewBusinessObject();
		}

		protected override PurgeSettings GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PurgeSettings(new PurgeSettingsRegistryDataType());
		}

		[ExpectNoExceptions]
		public void TestDeserialiseBackwardsCompatibility()
		{
			IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);

			var businessObjectToSerialise = new PurgeSettingsWithAllHistoricalElementsForTest();
			byte[] serialisedValue = dummyDataType.Serialise(businessObjectToSerialise);

			dummyDataType.Deserialise(serialisedValue);
		}

		public void TestApplicationCodeDoesNotExistInMultiplePurgeConfig()
		{
			var appCodeHash = new HashSet<ZString>();
			var duplicateList = new List<ZString>();

			var res = EDIMessagePurgeSettings.GetAllPurgeSettings();
			foreach (ApplicationCodeObj appCode in res)
			{
				if (!appCodeHash.Add(appCode.ApplicationCode))
				{
					duplicateList.Add(appCode.ApplicationCode);
				}
			}

			Assert($"The following Application Codes exist in two or more PurgeSettingsConfig\r\n{string.Join("\r\n", duplicateList)}", duplicateList.IsNullOrEmpty()); 
		}
	}
}
