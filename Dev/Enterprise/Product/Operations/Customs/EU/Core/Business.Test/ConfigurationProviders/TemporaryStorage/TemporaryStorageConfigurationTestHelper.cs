using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public static class TemporaryStorageConfigurationTestHelper
	{
		public static IDisposable TemporarilyClearConfigurationAndSetSupportLRNGeneration(TemporaryStorageHeader temporaryStorageHeader, bool configurationValue)
		{
			return TemporarilyClearConfigurationAndThenSetConfiguration(temporaryStorageHeader, nameof(TemporaryStorageConfiguration.SupportLRNGeneration) + "Core", configurationValue);
		}

		public static IDisposable TemporarilyClearConfigurationAndSetSupportAgentDefaulting(TemporaryStorageHeader temporaryStorageHeader, bool configurationValue)
		{
			return TemporarilyClearConfigurationAndThenSetConfiguration(temporaryStorageHeader, nameof(TemporaryStorageConfiguration.SupportAgentDefaulting) + "Core", configurationValue);
		}

		static IDisposable TemporarilyClearConfigurationAndThenSetConfiguration(TemporaryStorageHeader temporaryStorageHeader, ZString configurationName, object configurationValue)
		{
			ClearTemporaryStorageHeaderConfiguration(temporaryStorageHeader);
			return TemporarilySetConfiguration(temporaryStorageHeader.Factory, configurationName, configurationValue, temporaryStorageHeader.DataGrouping);
		}

		static IDisposable TemporarilySetConfiguration(BusinessObjectFactory factory, ZString configurationName, object configurationValue, string countryOrGrouping)
		{
			var configurationMock = new Mock<TemporaryStorageConfiguration>();
			configurationMock.CallBase = true;

			switch (configurationName)
			{
				case "SupportLRNGenerationCore":
					configurationMock.Protected()
						.Setup<bool>("SupportLRNGenerationCore", ItExpr.IsAny<BusinessObject>())
						.Returns((bool)configurationValue);
					break;

				case "SupportAgentDefaultingCore":
					configurationMock.Protected()
						.Setup<bool>("SupportAgentDefaultingCore", ItExpr.IsAny<BusinessObject>())
						.Returns((bool)configurationValue);
					break;

				default:
					throw new ArgumentException("Invalid configuration name.", nameof(configurationName));
			}

			return UpdateObjectFactory(factory, configurationMock, countryOrGrouping);
		}

		static IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<TemporaryStorageConfiguration> configurationMock, string countryOrGrouping)
		{
			const string configurationTypeName = nameof(TemporaryStorageConfiguration);
			countryOrGrouping ??= GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			factory.ClearCachedValue<TemporaryStorageConfiguration>($"{configurationTypeName}_{countryOrGrouping}");

			var objectHandleMock = new Mock<ObjectHandle>();
			_ = objectHandleMock.Setup(m => m.GetObject()).Returns(configurationMock.Object);

			var configuration = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			return ObjectFactory.Substitute(configurationTypeName, configuration);
		}

		static void ClearTemporaryStorageHeaderConfiguration(TemporaryStorageHeader temporaryStorageHeader)
		{
			typeof(TemporaryStorageHeader).GetField("configuration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(temporaryStorageHeader, null);
		}
	}
}
