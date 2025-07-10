using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.Accounting.Business.EInvoicing
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public partial class ElectronicInvoicingEligibilityDecider
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer logs should be in English")]
		public static bool IsEligible(IEInvoicingEligibilityLiteTransaction header)
		{
			Argument.NotNull(header, nameof(header));

			var transactionCountry = header.CountryCode;
			var headerDescription = $"{transactionCountry} eInvoicing Eligibility for {header.Ledger} {header.TransactionType} {header.TransactionNumber}:";
			var tracer = ObjectFactory.Get<ITracer>();
			tracer.TraceVerbose(AccountingTraceSourceCodes.eInvoicing, () => headerDescription);

			if (TryGetObjectFactoryEligibilityDecider(transactionCountry, out var eligibilityDecider))
			{
				var result = eligibilityDecider.IsTransactionEligible(header);
				tracer.TraceVerbose(AccountingTraceSourceCodes.eInvoicing, () => $"{result} - Evaluated with IEInvoicingEligibilityDecider.");
				var additionalLogs = eligibilityDecider.GetAdditionalTraceLog(header);
				if (!string.IsNullOrEmpty(additionalLogs))
				{
					tracer.TraceVerbose(AccountingTraceSourceCodes.eInvoicing, () => additionalLogs);
				}

				return result;
			}
			else if (header is TransactionHeader th)
			{
				var result = IsEligible(CreateWrapper(th));
				tracer.TraceVerbose(AccountingTraceSourceCodes.eInvoicing, () => $"{result} - Evaluated with TransactionHeader.");

				return result;
			}
			else if (header is GovernmentInvoice gi)
			{
				var result = IsEligible(CreateWrapper(gi));
				tracer.TraceVerbose(AccountingTraceSourceCodes.eInvoicing, () => $"{result} - Evaluated with GovernmentInvoice.");

				return result;
			}
			else
			{
				tracer.TraceVerbose(AccountingTraceSourceCodes.eInvoicing, () => "eInvoicing Eligibility could not be evaluated.");
				return false;
			}
		}

		public static bool IsEligible(AccComplianceDocumentHeader header) => IsEligible(CreateWrapper(header));

		public static bool IsSupportedCountry(ZString countryCode)
			=> SupportedCountriesOfTransactions.Contains(countryCode)
			|| TryGetObjectFactoryEligibilityDecider(countryCode, out _);

		#region Implementation

		#region Private GetInstance and Constructor

		readonly static ElectronicInvoicingEligibilityDecider Instance = new ElectronicInvoicingEligibilityDecider();

		ElectronicInvoicingEligibilityDecider()
		{
			countrySpecificTransactionHeaderEligibilityConfigurations = CreateCountrySpecificTransactionHeaderEligibilityConfigurations().ToImmutableDictionary();
			countrySpecificComplianceDocumentHeaderEligibilityConfigurations = CreateCountrySpecificComplianceDocumentHeaderEligibilityConfigurations().ToImmutableDictionary();
		}

		Dictionary<ZString, Func<ITransactionHeaderWrapper, bool>> CreateCountrySpecificTransactionHeaderEligibilityConfigurations()
		{
			// NOTE: prefer using the "lite" version via IEInvoicingEligibilityDecider country factory; do not add to this list unless absolutely required.
			var result = new Dictionary<ZString, Func<ITransactionHeaderWrapper, bool>>()
			{
				{ Constants.CountryCodes.Argentina,    ArgentinaSpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.Brazil,       BrazilSpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.Egypt,        EgyptSpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.Hungary,      HungarySpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.Italy,        ItalySpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.Mexico,       MexicoSpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.Turkey,       TurkeySpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.Uruguay,      UruguaySpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.VietNam,      VietnamSpecificTransactionHeaderEligibilityFunction },
				{ Constants.CountryCodes.KoreaSouth,   KoreaSouthSpecificTransactionHeaderEligibilityFunction },
			};
			foreach (var countryCode in TaxCoreCountryHelper.GetTaxCoreSupportedCountries())
			{
				result.Add(countryCode, TaxCoreSpecificTransactionHeaderEligibilityFunction);
			}
			return result;
		}

		Dictionary<ZString, Func<IComplianceDocumentHeaderWrapper, bool>> CreateCountrySpecificComplianceDocumentHeaderEligibilityConfigurations()
			=> new Dictionary<ZString, Func<IComplianceDocumentHeaderWrapper, bool>>()
			{
				{ Constants.CountryCodes.Taiwan, TaiwanSpecificComplianceDocumentHeaderEligibilityFunction },
			};

		#endregion

		internal static bool IsEligible(ITransactionHeaderWrapper header) => Instance.countrySpecificTransactionHeaderEligibilityConfigurations.TryGetValue(header.Company.CountryCode.Code, out var isEligibleFunc) && isEligibleFunc(header);
		internal static bool IsEligible(IComplianceDocumentHeaderWrapper header) => Instance.countrySpecificComplianceDocumentHeaderEligibilityConfigurations.TryGetValue(header.Company.CountryCode.Code, out var isEligibleFunc) && isEligibleFunc(header);

		readonly ImmutableDictionary<ZString, Func<ITransactionHeaderWrapper, bool>> countrySpecificTransactionHeaderEligibilityConfigurations;
		readonly ImmutableDictionary<ZString, Func<IComplianceDocumentHeaderWrapper, bool>> countrySpecificComplianceDocumentHeaderEligibilityConfigurations;

		internal static ITransactionHeaderWrapper CreateWrapper(TransactionHeader header) => new TransactionHeaderWrapper(header);

		internal static ITransactionHeaderWrapper CreateWrapper(GovernmentInvoice header) => new TransactionHeaderWrapper(header);

		internal static IComplianceDocumentHeaderWrapper CreateWrapper(AccComplianceDocumentHeader header) => new ComplianceDocumentHeaderWrapper(header);

		static IEnumerable<ZString> SupportedCountriesOfTransactions => Instance.countrySpecificTransactionHeaderEligibilityConfigurations.Keys;

		static bool TryGetObjectFactoryEligibilityDecider(ZString countryCode, out IEInvoicingEligibilityDecider result)
		{
			var factory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode);
			if (factory is IInstanceProvider<IEInvoicingEligibilityDecider> instanceProvider)
			{
				result = instanceProvider.Get();
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		#endregion
	}
}
