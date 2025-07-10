using System;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing
{
	public static class ConfigurationTestHelper
	{
		public static IDisposable TemporarySetupUCC6((IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) data, ZBool configurationValue)
		{
			data.configurationMock.Protected()
				.Setup<ZBool>("IsUCC6Core", ItExpr.IsAny<BusinessObject>())
				.Returns(configurationValue);
			return data.disposable;
		}

		public static IDisposable TemporarySetupUCC5((IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) data, ZBool configurationValue)
		{
			data.configurationMock.Protected()
				.Setup<ZBool>("IsUCC5Core", ItExpr.IsAny<BusinessObject>())
				.Returns(configurationValue);
			return data.disposable;
		}

		public static IDisposable TemporarySetupUseEucdmSupportingDocumentGoodsShipmentAndItem((IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) data, ZBool useEucdmSupportingDocumentGoodsShipment, ZBool useEucdmSupportingDocumentGoodsShipmentAndItem)
		{
			data.configurationMock.Protected()
				.Setup<ZBool>("UseEucdmSupportingDocumentGoodsShipmentCore", ItExpr.IsAny<BusinessObject>())
				.Returns(useEucdmSupportingDocumentGoodsShipment);

			data.configurationMock.Protected()
				.Setup<ZBool>("UseEucdmSupportingDocumentGoodsShipmentAndItemCore", ItExpr.IsAny<BusinessObject>())
				.Returns(useEucdmSupportingDocumentGoodsShipmentAndItem);

			return data.disposable;
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(JobDeclaration declaration, ZString configurationName, object configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, configurationName, configurationValue).disposable;
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(JobDeclaration declaration, ZString configurationName, object configurationValue)
		{
			ClearDeclarationConfiguration(declaration);
			return TemporarilySetConfiguration_DeclarationAndReturnMock(declaration.Factory, configurationName, configurationValue, declaration.GetDefaultDataGroupingCode());
		}

		public static IDisposable TemporarilySetConfiguration_Declaration(BusinessObjectFactory factory, ZString configurationName, object configurationValue, string countryOrGrouping)
		{
			return TemporarilySetConfiguration_DeclarationAndReturnMock(factory, configurationName, configurationValue, countryOrGrouping).disposable;
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilySetConfiguration_DeclarationAndReturnMock(BusinessObjectFactory factory, ZString configurationName, object configurationValue, string countryOrGrouping)
		{
			var configurationMock = new Mock<DeclarationConfiguration>() { CallBase = true };
			configurationMock.CallBase = true;

			switch (configurationName)
			{
				case "IsUCC5Core":
					configurationMock.Protected()
						.Setup<ZBool>("IsUCC5Core", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				case "IsUCC6Core":
					configurationMock.Protected()
						.Setup<ZBool>("IsUCC6Core", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				case "DV1DetailsSupportCore":
					configurationMock.Protected()
						.Setup<ZBool>("DV1DetailsSupportCore", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				case "GetNewEntryLineConfiguration":
					configurationMock.Protected()
						.Setup<EntryLineConfiguration>("GetNewEntryLineConfiguration")
						.Returns((EntryLineConfiguration)configurationValue);
					break;
				case "GetNewInvoiceLineConfiguration":
					configurationMock.Protected()
						.Setup<InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
						.Returns((InvoiceLineConfiguration)configurationValue);
					break;
				case "GetNewInvoiceHeaderConfiguration":
					configurationMock.Protected()
						.Setup<InvoiceHeaderConfiguration>("GetNewInvoiceHeaderConfiguration")
						.Returns((InvoiceHeaderConfiguration)configurationValue);
					break;
				case "GetNewInstructionConfiguration":
					configurationMock.Protected()
						.Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
						.Returns((InstructionConfiguration)configurationValue);
					break;
				case "UseUniversalFeeCalculationCore":
					configurationMock.Protected()
						.Setup<ZBool>("UseUniversalFeeCalculationCore", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				case "IsAESFullUCC6Core":
					configurationMock.Protected()
						.Setup<ZBool>("IsAESFullUCC6Core", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				case "IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore":
					configurationMock.Protected()
						.Setup<ZBool>("IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				case "UCCAdditionalInfosSupportCore":
					configurationMock.Protected()
						.Setup<ZBool>("UCCAdditionalInfosSupportCore", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				case "IsTransitionPeriodAES30Core":
					configurationMock.Protected()
						.Setup<ZBool>("IsTransitionPeriodAES30Core", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				case "GetValidationDeciderCore":
					configurationMock.Protected()
						.Setup<IDeclarationValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<BusinessObject>())
						.Returns((IDeclarationValidationDecider)configurationValue);
					break;
				case "GetPackageValidationDeciderCore":
					configurationMock.Protected()
						.Setup<IPackageValidationDecider>("GetPackageValidationDeciderCore", ItExpr.IsAny<BusinessObject>())
						.Returns((IPackageValidationDecider)configurationValue);
					break;
				case "IsPopulateAuthorisationsForOfficeOfPresentationEnabledCore":
					configurationMock.Protected()
						.Setup<ZBool>("IsPopulateAuthorisationsForOfficeOfPresentationEnabledCore", ItExpr.IsAny<JobDeclaration>())
						.Returns(new ZBool(configurationValue));
					break;
				case "GetCustomsOfficeValidationDeciderCore":
					configurationMock.Protected()
						.Setup<ICustomsOfficeValidationDecider>("GetCustomsOfficeValidationDeciderCore", ItExpr.IsAny<BusinessObject>())
						.Returns((ICustomsOfficeValidationDecider)configurationValue);
					break;
				case "LockNumberOfEntryLinesForRegisteredEntryCore":
					configurationMock.Protected()
						.Setup<ZBool>("LockNumberOfEntryLinesForRegisteredEntryCore")
						.Returns(new ZBool(configurationValue));
					break;
				case "UseIDDDocumentCore":
					configurationMock.Protected()
						.Setup<ZBool>("UseIDDDocumentCore", ItExpr.IsAny<BusinessObject>())
						.Returns(new ZBool(configurationValue));
					break;
				default:
					throw new ArgumentException("Invalid configuration name.", nameof(configurationName));
			}

			var disposable = UpdateObjectFactory(factory, configurationMock, countryOrGrouping);
			return (disposable, configurationMock);
		}

		internal static IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<DeclarationConfiguration> configurationMock, string countryOrGrouping = null)
		{
			var configurationTypeName = nameof(DeclarationConfiguration);
			countryOrGrouping ??= GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			factory.ClearCachedValue<DeclarationConfiguration>($"{configurationTypeName}_{countryOrGrouping}");

			var objectHandleMock = new Mock<ObjectHandle>();
			_ = objectHandleMock.Setup(m => m.GetObject()).Returns(configurationMock.Object);

			var configuration = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			return ObjectFactory.Substitute(configurationTypeName, configuration);
		}

		public static IDisposable TemporarilyClearPartPivotConfigurationAndThenSetPartPivotConfiguration(CusClassPartPivot partPivot, ZString configurationName, object configurationValue)
		{
			ClearPartPivotConfiguration(partPivot);
			return TemporarilySetConfiguration_PartPivot(partPivot.Factory, configurationName, configurationValue);
		}

		public static IDisposable TemporarilySetConfiguration_PartPivot(BusinessObjectFactory factory, ZString configurationName, object configurationValue)
		{
			var configurationTypeName = nameof(CusClassPartPivotConfiguration);
			var configurationMock = new Mock<CusClassPartPivotConfiguration>();
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			configurationMock.CallBase = true;

			switch (configurationName)
			{
				case "UCCAdditionalInfosSupportCore":
					configurationMock.Protected()
						.Setup<ZBool>("UCCAdditionalInfosSupportCore", ItExpr.IsAny<CusClassPartPivot>())
						.Returns(new ZBool(configurationValue));
					break;
				default:
					throw new ArgumentException("Invalid configuration name.", nameof(configurationName));
			}

			factory.ClearCachedValue<CusClassPartPivotConfiguration>($"{configurationTypeName}_{country}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(configurationMock.Object);

			var configuration = new KeyObjectHandleDictionaryObject
			{
				{ country, objectHandleMock.Object }
			};

			return ObjectFactory.Substitute(configurationTypeName, configuration);
		}

		public static IDisposable TemporarilySetDeclarationConfiguration(BusinessObjectFactory factory, ZString configurationName, object configurationValue, string countryOrGrouping = null)
		{
			return TemporarilySetConfiguration_Declaration(factory, configurationName, configurationValue, countryOrGrouping);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(JobDeclaration declaration, bool configurationValue)
		{
			ClearDeclarationConfiguration(declaration);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>() { CallBase = true };
			invoiceHeaderConfigurationMock.Protected().Setup<ZBool>("AgreedPlaceCodeSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(configurationValue);
			return TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceHeaderConfiguration", invoiceHeaderConfigurationMock.Object, declaration.GetDefaultDataGroupingCode());
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration<T>(JobDeclaration declaration, string configurationName, T configurationValue)
		{
			ClearDeclarationConfiguration(declaration);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>() { CallBase = true };
			invoiceHeaderConfigurationMock.Protected().Setup<T>(configurationName + "Core").Returns(configurationValue);
			return TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceHeaderConfiguration", invoiceHeaderConfigurationMock.Object, declaration.GetDefaultDataGroupingCode());
		}

		public static void ClearDeclarationConfiguration(JobDeclaration declaration)
		{
			typeof(JobDeclaration).GetField("configuration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(declaration, null);
		}

		public static void ClearPartPivotConfiguration(CusClassPartPivot partPivot)
		{
			typeof(CusClassPartPivot).GetField("configuration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(partPivot, null);
		}

		public static IDisposable TemporarilySetUseUniversalFeeCalculationConfiguration(BusinessObjectFactory factory, bool configurationValue, string countryOrGrouping = null)
		{
			return TemporarilySetConfiguration_Declaration(factory, nameof(DeclarationConfiguration.UseUniversalFeeCalculation) + "Core", configurationValue, countryOrGrouping);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.IsUCC5) + "Core", configurationValue);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.IsUCC6) + "Core", configurationValue);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndSetLockNumberOfEntryLinesForRegisteredEntryConfiguration(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.LockNumberOfEntryLinesForRegisteredEntry) + "Core", configurationValue);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, configurationValue);
		}

		public static void ClearCusTempStorageJobHeaderConfiguration(CusTempStorageJobHeader header)
		{
			typeof(CusTempStorageJobHeader).GetField("configuration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(header, null);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfiguration(JobDeclaration declaration, string configurationName, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfigurationAndReturnMock(declaration, configurationName, configurationValue).disposable;
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfigurationAndReturnMock(JobDeclaration declaration, string configurationName, bool configurationValue)
		{
			var instructionConfigurationMock = new Mock<InstructionConfiguration>() { CallBase = true };
			instructionConfigurationMock
				.Protected()
				.Setup<ZBool>(configurationName, ItExpr.IsAny<JobDeclaration>())
				.Returns(configurationValue);

			return TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "GetNewInstructionConfiguration", instructionConfigurationMock.Object);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionFiscalReferencesSupportConfiguration(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfiguration(declaration, "FiscalReferencesSupportCore", configurationValue);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAuthorisationsSupportConfiguration(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfiguration(declaration, "AuthorisationsSupportCore", configurationValue);
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionGuaranteesSupportConfiguration(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionDeclarationConfigurationAndReturnMock(declaration, "GuaranteesSupportCore", configurationValue);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfiguration(declaration, "AdditionalSupplyChainActorSupportCore", configurationValue);
		}

		public static IDisposable TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSealsSupportConfiguration(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfiguration(declaration, "SealsSupportCore", configurationValue);
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfigurationAndReturnMock(declaration, "SupportingDocumentsSupportCore", configurationValue);
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfigurationAndReturnMock(declaration, "PreviousDocumentsSupportCore", configurationValue);
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionRequestedDocumentsSupportConfigurationAndReturnMock(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionConfigurationAndReturnMock(declaration, "RequestedDocumentsSupportCore", configurationValue);
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionDeclarationConfigurationAndReturnMock(declaration, "AdditionalInfosSupportCore", configurationValue);
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSpecialProceduresSupportConfigurationAndReturnMock(JobDeclaration declaration, bool configurationValue)
		{
			return TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionDeclarationConfigurationAndReturnMock(declaration, "SpecialProceduresSupportCore", configurationValue);
		}

		public static (IDisposable disposable, Mock<DeclarationConfiguration> configurationMock) TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionDeclarationConfigurationAndReturnMock(JobDeclaration declaration, ZString configurationName, bool configurationValue)
		{
			var instructionConfigurationMock = new Mock<InstructionConfiguration>() { CallBase = true };
			instructionConfigurationMock
				.Protected()
				.Setup<ZBool>(configurationName, ItExpr.IsAny<JobDeclaration>(), ItExpr.IsAny<CusEntryInstruction>())
				.Returns(configurationValue);

			return TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "GetNewInstructionConfiguration", instructionConfigurationMock.Object);
		}

		public static IDisposable TemporarilySetInvoiceLineValidationDeciderRule(JobDeclaration declaration, Expression<Func<IInvoiceLineValidationDecider, bool>> expression, bool configurationValue)
		{
			ClearDeclarationConfiguration(declaration);

			var invoiceLineValidationDeciderMock = new Mock<IInvoiceLineValidationDecider>() { CallBase = true };
			invoiceLineValidationDeciderMock.Setup(expression)
				.Returns(configurationValue);

			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>() { CallBase = true };
			invoiceLineConfigurationMock.Protected()
						.Setup<IInvoiceLineValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobComInvoiceLine>())
						.Returns(invoiceLineValidationDeciderMock.Object);

			return TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, declaration.GetDefaultDataGroupingCode());
		}
	}
}
