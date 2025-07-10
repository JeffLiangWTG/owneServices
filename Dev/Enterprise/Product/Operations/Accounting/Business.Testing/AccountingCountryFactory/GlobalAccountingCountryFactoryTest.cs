using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class GlobalAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestGlobalAccountingCountryFactoryIsNotSingleton()
		{
			var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();
			var globalAccountingCountryFactory2 = ObjectFactory.Get<IGlobalAccountingCountryFactory>();
			AssertNotEquals(globalAccountingCountryFactory, globalAccountingCountryFactory2);
		}

		public void TestGetCountryFactoryForDifferentCountries()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);
			var expectedCountries = GetSupportedCountries();

			var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();

			foreach (var country in countries)
			{
				var countryCode = country.Code;
				var result = globalAccountingCountryFactory.GetCountryFactory(countryCode);
				if (expectedCountries.Contains(countryCode))
				{
					AssertNotNull($"Supported country {countryCode}", result);
				}
				else
				{
					AssertNull($"Not supported country {countryCode}", result);
				}
			}
		}

		public void TestGetCountryFactoryForDifferentCountries_ValidCountries_FactoriesNotSingleton()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);
			var expectedCountries = GetSupportedCountries();

			var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();

			foreach (var country in countries)
			{
				var countryCode = country.Code;
				var countryFactory = globalAccountingCountryFactory.GetCountryFactory(countryCode);
				if (expectedCountries.Contains(countryCode))
				{
					var countryFactory2 = globalAccountingCountryFactory.GetCountryFactory(countryCode);
					AssertNotEquals(countryFactory, countryFactory2);
				}
			}
		}

		public void TestGetCountryFactoryForAnEmptyCountryCode()
		{
			var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();
			var countryCode = ZString.Empty;
			var countryFactory = globalAccountingCountryFactory.GetCountryFactory(countryCode);
			AssertNull(countryFactory);
		}

		public void TestGetCountryFactoryForANonEmptyCountryCode()
		{
			var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();
			var countryCode = "TEST";
			var countryFactory = globalAccountingCountryFactory.GetCountryFactory(countryCode);
			AssertNull(countryFactory);
		}

		public void TestImplementsIComplianceReportsProvider()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);
			var countriesWithIComplianceReportsProvider = new List<string> {
				CountryCodes.Germany,
			};

			var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();

			foreach (var country in countries)
			{
				var countryCode = country.Code;
				var countryFactory = globalAccountingCountryFactory.GetCountryFactory(countryCode);
				var provider = (countryFactory as IInstanceProvider<IComplianceReportsProvider>)?.Get();
				AssertEquals(countriesWithIComplianceReportsProvider.Contains(countryCode), provider is IComplianceReportsProvider);
			}
		}

		public void TestPreventAddingNewCountryFeaturesToObsoleteCountryFactory()
		{
			var obsoleteFeaturesNotMovedToCountryComplianceSolution = new HashSet<Type>()
			{
				typeof(IAccountingCountryFactory),
				typeof(IQRCodeDataProvider),
				typeof(IThresholdProvider),
				typeof(ICountrySpecificLabelTranslator),
				typeof(IOrgTaxRateImportFileFormatProvider),
				typeof(ITaxFrameworkConfigurationDefaults),
				typeof(IComplianceFinancialYear),
				typeof(ITaxIDMacroDataProvider),
				typeof(ITaxFrameworkThresholdMethodProvider),
				typeof(IEInvoicingPreEligibilityProvider),
				typeof(IEInvoicingEligibilityDecider),
				typeof(IReversalStatusCodeConfiguration),
				typeof(IOverrideTransactionLineSequenceProvider),
				typeof(IQueueInvoiceForTransmissionProvider),
				typeof(IMarkIssuedInvoiceAsReversed),
				typeof(IEInvoicingSignatureBuilderProvider),
				typeof(IComplianceReportsProvider),
				typeof(ISurchargeApplicationConfigurationUsesTaxId),
				typeof(IEnableDocumentSigningServiceProvider),
				typeof(ICloudSigningServiceProviderAPIEndpointProvider),
				typeof(IComplianceNumberResetStatus),
				typeof(IComplianceReportDefaultValue),
				typeof(IGovernmentAllocatedIDValidationProvider),
				typeof(IEInvoicingPivotStatusProvider),
				typeof(IReverseDateValidation),
				typeof(IInvoiceDateValidation),
				typeof(ITransactionLinesValidation),
				typeof(IEInvoicingAuthorizationBehaviourProvider),
				typeof(IEInvoicingActionProvider),
				typeof(IAmendStatusCodeProvider),
				typeof(IAmendStatusCodeValidationProvider),
				typeof(IEInvoicingPivotsToRequeueFilterProvider),
				typeof(IEInvoicingDiscardAdditionalPiviotActionTypesProvider),
				typeof(IDebtorNumberProvider),
				typeof(IEquivalentAgreedPaymentMethodProvider),
				typeof(IInvoicePaymentMethodProvider),
				typeof(IReportSAFTWriter),
				typeof(IReportModeAndCreditorSelectorDefault),
				typeof(IComplianceReportGUIActionProvider),
				typeof(IEInvoicingRequeuePivotsStatusProvider),
				typeof(IComplianceSubTypeEditableProvider),
				typeof(ISourceReferenceEditableProvider),
				typeof(IBatchQueueInvoicesForEInvoicingProvider),
				typeof(ISupportResetStatusToDelivered),
				typeof(ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider),
				typeof(IEInvoicingPivotActionTypeProvider),
				typeof(IEInvoicingRequeueProvider),
				typeof(IEInvoicingReversingProvider),
				typeof(IComplianceDocumentVoidingProvider),
				typeof(IComplianceDocumentNumberProvider),
				typeof(IGLJournalTypesProvider)
			};
			var actualFeatures = new HashSet<Type>();
			var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();
			foreach (var countryCode in GetSupportedCountries())
			{
				var countryFactory = globalAccountingCountryFactory.GetCountryFactory(countryCode);
				var inheritedInterfaces = countryFactory.GetType().GetInterfaces().ToList();
				foreach (var inheritedInterface in inheritedInterfaces)
				{
					if (inheritedInterface.IsGenericType && inheritedInterface.GetGenericTypeDefinition() == typeof(IInstanceProvider<>))
					{
						var parameterType = inheritedInterface.GenericTypeArguments.FirstOrDefault();
						if (parameterType != null)
						{
							actualFeatures.Add(parameterType);
						}
					}
					else
					{
						actualFeatures.Add(inheritedInterface);
					}
				}
			}
			var missingInActual = obsoleteFeaturesNotMovedToCountryComplianceSolution.Except(actualFeatures).ToList();
			var unexpectedInActual = actualFeatures.Except(obsoleteFeaturesNotMovedToCountryComplianceSolution).ToList();

			AssertEquals($"The following features from obsoleteFeaturesNotMovedToCountryComplianceSolution are not implemented anywhere in GlobalAccountingCountryFactory: {string.Join(", ", missingInActual.Select(type => type.Name))}", missingInActual.Count, 0);

			AssertEquals($"The following features implemented in GlobalAccountingCountryFactory are not from obsoleteFeaturesNotMovedToCountryComplianceSolution: {string.Join(", ", unexpectedInActual.Select(type => type.Name))}\nGlobalAccountingCountryFactory should not be used to implement new features. Please refer: https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FProduct%2FOperations%2FAccounting%2FBusiness%2FAccountingCountryFactory%2FGlobalAccountingCountryFactory.cs", unexpectedInActual.Count, 0);
		}

		HashSet<string> GetSupportedCountries() => new HashSet<string>
		{
			CountryCodes.Argentina,
			CountryCodes.Australia,
			CountryCodes.Belize,
			CountryCodes.BonaireSintEustatiusAndSaba,
			CountryCodes.Brazil,
			CountryCodes.BurkinaFaso,
			CountryCodes.CapeVerde,
			CountryCodes.Chile,
			CountryCodes.China,
			CountryCodes.Colombia,
			CountryCodes.CookIslands,
			CountryCodes.CostaRica,
			CountryCodes.DominicanRepublic,
			CountryCodes.Egypt,
			CountryCodes.Fiji,
			CountryCodes.FrenchGuyana,
			CountryCodes.FaeroeIslands,
			CountryCodes.Gambia,
			CountryCodes.Germany,
			CountryCodes.Ghana,
			CountryCodes.India,
			CountryCodes.Indonesia,
			CountryCodes.Israel,
			CountryCodes.Italy,
			CountryCodes.Jordan,
			CountryCodes.KoreaSouth,
			CountryCodes.Kyrgyzstan,
			CountryCodes.Latvia,
			CountryCodes.Malaysia,
			CountryCodes.Mauritius,
			CountryCodes.Mayotte,
			CountryCodes.Mexico,
			CountryCodes.Norway,
			CountryCodes.Panama,
			CountryCodes.Philippines,
			CountryCodes.Poland,
			CountryCodes.Portugal,
			CountryCodes.Romania,
			CountryCodes.SaintKittsAndNevis,
			CountryCodes.SaudiArabia,
			CountryCodes.Serbia,
			CountryCodes.Spain,
			CountryCodes.Suriname,
			CountryCodes.Swaziland,
			CountryCodes.Turkey,
			CountryCodes.Turkmenistan,
			CountryCodes.Tuvalu,
			CountryCodes.UnitedKingdom,
			CountryCodes.Uruguay,
			CountryCodes.VietNam,
			CountryCodes.Taiwan,
			CountryCodes.Zimbabwe
		};
	}
}
