using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public static class NctsConfigurationTestHelper
	{
		public static IDisposable TemporarilySetNctsHeaderConfigurationAllowMixedCaseAuthorisationNumbers(BusinessObjectFactory factory, bool allowMixedCaseAuthorisationNumbers)
			=> TemporarilySetConfiguration<ZBool>(factory, "AllowMixedCaseAuthorisationNumbersCore", allowMixedCaseAuthorisationNumbers, ItExpr.IsAny<NctsHeader>());

		public static IDisposable TemporarilySetNctsHeaderConfiguration(BusinessObjectFactory factory, bool docDataPlugInSupportForDepartureMovement)
			=> TemporarilySetConfiguration<ZBool>(factory, "DocDataPlugInSupportForDepartureMovement", docDataPlugInSupportForDepartureMovement, ItExpr.IsAny<NctsHeader>());

		public static IDisposable TemporarilySetNctsHeaderConfigurationUseOrgProxyFallback(BusinessObjectFactory factory, bool useOrgProxyFallBack)
		{
			return TemporarilySetConfiguration<ZBool>(factory, "UseCompanyOrgProxyFallBackCore", useOrgProxyFallBack, ItExpr.IsAny<NctsHeader>());
		}

		public static IDisposable TemporarilySetAllGuaranteeFallbackConfigurations(BusinessObjectFactory factory, bool useDeclarantFallBack, bool useBranchProxyFallBack, bool useOrgProxyFallBack)
		{
			return TemporarilySetAllGuaranteeFallBackConfigurations<ZBool>(factory, useDeclarantFallBack, useBranchProxyFallBack, useOrgProxyFallBack, ItExpr.IsAny<NctsHeader>());
		}

		public static IDisposable TemporarilySetNctsHeaderConfigurationIsBondedWarehouseSupported(BusinessObjectFactory factory, bool isBondedWarehouseSupported)
		{
			return TemporarilySetConfiguration<ZBool>(factory, "IsBondedWarehouseSupportedCore", isBondedWarehouseSupported, ItExpr.IsAny<NctsHeader>());
		}

		public static IDisposable TemporarilySetNctsHeaderConfigurationUseGuaranteeGridValidation(BusinessObjectFactory factory, bool isGuaranteeGridValidationSet)
		{
			return TemporarilySetConfiguration<ZBool>(factory, "UseGuaranteeGridValidationCore", isGuaranteeGridValidationSet, ItExpr.IsAny<NctsHeader>());
		}

		public static IDisposable TemporarilySetNctsHeaderConfigurationClearExistingGuaranteesConfiguration(BusinessObjectFactory factory, bool isClearExistingGuaranteesConfigurationSet)
		{
			return TemporarilySetConfiguration<ZBool>(factory, "ClearExistingGuaranteesConfigurationCore", isClearExistingGuaranteesConfigurationSet, ItExpr.IsAny<NctsHeader>());
		}

		public static IDisposable TemporarilySetNctsHeaderMiscTabPageSupportConfiguration(BusinessObjectFactory factory, bool miscTabPageSupport)
			=> TemporarilySetConfiguration<ZBool>(factory, "MiscTabPageSupportCore", miscTabPageSupport, ItExpr.IsAny<NctsHeader>());

		public static IDisposable TemporarilySetGoodsItemsConfiguration<TConfiguration>(BusinessObjectFactory factory, string setupConfigurationName, TConfiguration configurationToInject, params object[] setupConfigurationArgs)
		{
			var goodsItemsConfigurationMock = new Mock<GoodsItemsConfiguration>();
			goodsItemsConfigurationMock
				.Protected()
				.Setup<TConfiguration>(setupConfigurationName, setupConfigurationArgs)
				.Returns(configurationToInject);

			return TemporarilySetConfiguration(factory, "GetNewGoodsItemsConfiguration", goodsItemsConfigurationMock.Object);
		}

		public static IDisposable TemporarilySetGuaranteeOverrideSupportConfiguration(BusinessObjectFactory factory, bool overrideSupport)
		{
			var guaranteeConfigurationMock = new Mock<GuaranteeConfiguration>();
			guaranteeConfigurationMock
				.Protected()
				.Setup<bool>("OverrideSupportCore", ItExpr.IsAny<NctsHeader>())
				.Returns(overrideSupport);

			return TemporarilySetConfiguration(factory, "GetNewGuaranteeConfiguration", guaranteeConfigurationMock.Object);
		}

		public static IDisposable TemporarilySetGuaranteeDefaultLiabilityAmountButtonConfiguration(BusinessObjectFactory factory, bool allowDefaultLiabilityAmount, decimal defaultLiabilityAmount)
		{
			var guaranteeConfigurationMock = new Mock<GuaranteeConfiguration>();
			guaranteeConfigurationMock
				.Protected()
				.Setup<bool>("AllowDefaultLiabilityAmountCore")
				.Returns(allowDefaultLiabilityAmount);
			guaranteeConfigurationMock
				.Protected()
				.Setup<decimal>("DefaultLiabilityAmountCore")
				.Returns(defaultLiabilityAmount);

			return TemporarilySetConfiguration(factory, "GetNewGuaranteeConfiguration", guaranteeConfigurationMock.Object);
		}

		public static IDisposable TemporarilySetUseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsConfigurationConfiguration(BusinessObjectFactory factory, bool useDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods)
		{
			var guaranteeConfigurationMock = new Mock<GuaranteeConfiguration>();
			guaranteeConfigurationMock
				.Protected()
				.Setup<bool>("UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsCore")
				.Returns(useDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods);
			return TemporarilySetConfiguration(factory, "GetNewGuaranteeConfiguration", guaranteeConfigurationMock.Object);
		}

		public static IDisposable TemporarilySetMessageSendingConfiguration(BusinessObjectFactory factory, MessageSendingConfiguration messageSendingConfiguration)
			=> TemporarilySetConfiguration(factory, "GetNewMessageSendingConfiguration", messageSendingConfiguration);

		public static IDisposable TemporarilySetMessageSendingConfiguration(BusinessObjectFactory factory, Dictionary<string, bool> propertyDic)
		{
			var messageSendingConfiguration = new Mock<MessageSendingConfiguration>();
			messageSendingConfiguration.SetupAllProperties();

			var protectedMock = messageSendingConfiguration.Protected();
			foreach (var property in propertyDic)
			{
				protectedMock.SetupGet<bool>(property.Key).Returns(property.Value);
			}
			return TemporarilySetConfiguration(factory, "GetNewMessageSendingConfiguration", messageSendingConfiguration.Object);
		}

		public static IDisposable TemporarilySetValidationConfigurationRule(BusinessObjectFactory factory, string publicPropertyName, bool value)
		{
			return TemporarilySetValidationConfigurationRule(factory, (publicPropertyName, value));
		}

		public static IDisposable TemporarilySetValidationConfigurationRule(BusinessObjectFactory factory, params (string publicPropertyName, bool value)[] rules)
		{
			var configuration = new Mock<NctsConfiguration> { CallBase = true };

			var validationRuleConfiguration = new Mock<ValidationRuleConfiguration>() { CallBase = true };

			rules.ForEach(rule =>
			{
				var method = string.Format("{0}Core", rule.publicPropertyName);
				validationRuleConfiguration.Protected().Setup<bool>(method).Returns(rule.value);
			});
			configuration
				.Protected()
				.Setup<ValidationRuleConfiguration>("GetNewValidationRuleConfiguration")
				.Returns(validationRuleConfiguration.Object);

			return UpdateObjectFactory(factory, configuration);
		}

		public static IDisposable TemporarilySetValidationConfigurationRuleForArrivalWithLiabilityCalculation(BusinessObjectFactory factory, bool isLiabilityCalculationForArrivalSupported,
			string publicPropertyName, bool value)
		{
			return TemporarilySetValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, isLiabilityCalculationForArrivalSupported, (publicPropertyName, value));
		}

		public static IDisposable TemporarilySetValidationConfigurationRuleForArrivalWithLiabilityCalculation(BusinessObjectFactory factory, bool isLiabilityCalculationForArrivalSupported, params (string publicPropertyName, bool value)[] rules)
		{
			var configuration = new Mock<NctsConfiguration> { CallBase = true };

			var validationRuleConfiguration = new Mock<ValidationRuleConfiguration>() { CallBase = true };
			rules.ForEach(rule =>
			{
				var method = string.Format("{0}Core", rule.publicPropertyName);
				validationRuleConfiguration.Protected().Setup<bool>(method).Returns(rule.value);
			});
			configuration
				.Protected()
				.Setup<ValidationRuleConfiguration>("GetNewValidationRuleConfiguration")
				.Returns(validationRuleConfiguration.Object);

			var goodsItemConfigurationMock = new Mock<GoodsItemsConfiguration>();
			goodsItemConfigurationMock
				.Protected()
				.Setup<ZBool>("IsLiabilityCalculationForArrivalSupportedCore")
				.Returns(isLiabilityCalculationForArrivalSupported);

			configuration
				.Protected()
				.Setup<GoodsItemsConfiguration>("GetNewGoodsItemsConfiguration")
				.Returns(goodsItemConfigurationMock.Object);

			return UpdateObjectFactory(factory, configuration);
		}

		public static IDisposable TemporarilySetConfiguration<TConfiguration>(BusinessObjectFactory factory, string setupConfigurationName, TConfiguration configurationToInject, params object[] setupConfigurationArgs)
		{
			var nctsConfigurationMock = new Mock<NctsConfiguration> { CallBase = true };
			nctsConfigurationMock
				.Protected()
				.Setup<TConfiguration>(setupConfigurationName, setupConfigurationArgs)
				.Returns(configurationToInject);

			return UpdateObjectFactory(factory, nctsConfigurationMock);
		}

		public static IDisposable TemporarilySetAllGuaranteeFallBackConfigurations<TConfiguration>(BusinessObjectFactory factory, TConfiguration configurationToInject1, TConfiguration configurationToInject2, TConfiguration configurationToInject3, params object[] setupConfigurationArgs)
		{
			var nctsConfigurationMock = new Mock<NctsConfiguration> { CallBase = true };

			nctsConfigurationMock
				.Protected()
				.Setup<TConfiguration>("UseDeclarantFallBackCore", setupConfigurationArgs)
				.Returns(configurationToInject1);

			nctsConfigurationMock
				.Protected()
				.Setup<TConfiguration>("UseBranchOrgProxyFallBackCore", setupConfigurationArgs)
				.Returns(configurationToInject2);

			nctsConfigurationMock
				.Protected()
				.Setup<TConfiguration>("UseCompanyOrgProxyFallBackCore", setupConfigurationArgs)
				.Returns(configurationToInject3);

			return UpdateObjectFactory(factory, nctsConfigurationMock);
		}

		public static IDisposable TemporarilySetOfficeCodeAutomaticSequenceNumberEnabled(BusinessObjectFactory factory, bool value)
		{
			var officeCodeConfigurationMock = new Mock<NctsEuOfficeCodeConfiguration>();
			officeCodeConfigurationMock.Setup(x => x.AutomaticSequenceNumberEnabled).Returns(value);

			return TemporarilySetConfiguration(factory, "GetNewNctsEuOfficeCodeConfiguration", officeCodeConfigurationMock.Object);
		}

		public static IDisposable TemporarilySetUseLocalReferenceNumberIgnoreInDatabaseCheckCore(BusinessObjectFactory factory, bool useLocalReferenceNumberIgnoreInDatabaseCheckCore)
		{
			var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
			nctsConfigurationMock
				.Protected()
				.Setup<ZBool>("UseLocalReferenceNumberIgnoreInDatabaseCheckCore")
				.Returns(useLocalReferenceNumberIgnoreInDatabaseCheckCore);

			return UpdateObjectFactory(factory, nctsConfigurationMock);
		}

		public static IDisposable TemporarilySetIsDepartureRetransmissionSupportedCore(BusinessObjectFactory factory, bool isDepartureRetransmissionSupportedCore)
		{
			var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
			nctsConfigurationMock
				.Protected()
				.Setup<ZBool>("IsDepartureRetransmissionSupportedCore")
				.Returns(isDepartureRetransmissionSupportedCore);

			return UpdateObjectFactory(factory, nctsConfigurationMock);
		}

		internal static IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<NctsConfiguration> nctsConfigurationMock)
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{currentCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(nctsConfigurationMock.Object);

			var nctsConfiguration = new KeyObjectHandleDictionaryObject
			{
				{ currentCountryCode, objectHandleMock.Object }
			};

			return ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration);
		}

		public static void RunAssertionsInAndOutPhase5TransitionPeriod(Action insidePhase5, Action outsidePhase5)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				code: Constants.FunctionalityTypes.NCTSTransitionPeriod,
				dataGroupingCode: RefDataGroupingCodes.EuropeanUnionEUN,
				effectiveDate: ZDate.Today,
				value: true))
			{
				insidePhase5();
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				code: Constants.FunctionalityTypes.NCTSTransitionPeriod,
				dataGroupingCode: RefDataGroupingCodes.EuropeanUnionEUN,
				effectiveDate: ZDate.Today,
				value: false))
			{
				outsidePhase5();
			}
		}

		public static IDisposable TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(BusinessObjectFactory factory, bool isLiabilityCalculationForArrivalSupported)
		{
			var goodsItemConfigurationMock = new Mock<GoodsItemsConfiguration>();
			goodsItemConfigurationMock
				.Protected()
				.Setup<ZBool>("IsLiabilityCalculationForArrivalSupportedCore")
				.Returns(isLiabilityCalculationForArrivalSupported);

			return TemporarilySetConfiguration(factory, "GetNewGoodsItemsConfiguration", goodsItemConfigurationMock.Object);
		}
	}
}
