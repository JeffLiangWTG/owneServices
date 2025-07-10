using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	sealed class ValueProviderCollectorTest : TestCase
	{
		public void TestValueProviderCollectionIsCached()
		{
			ValueProviderCollector collector = new ValueProviderCollector();
			Assert("ValueProviderCollection should have been cached", object.ReferenceEquals(collector.ValueProviders, collector.ValueProviders));
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestAllValueProvidersInListAreDocumentedAndNoDuplicatesExist()
		{
			List<Type> types = new List<Type>();
			Dictionary<string, Type> useages = new Dictionary<string, Type>();
			Dictionary<string, Type> explanations = new Dictionary<string, Type>();

			ValueProviderCollector collector = new ValueProviderCollector();
			foreach (ValueProvider provider in collector.ValueProviders.Providers)
			{
				ValueProviderDocumenter documenter = provider.Documentation;

				Type currentType = provider.GetType();
				if (types.Contains(currentType))
				{
					Fail(currentType.ToString() + " should only be included in the ValueProviderCollector.ValueProviders.Providers once.");
				}
				else
				{
					types.Add(currentType);
				}

				AssertNotEquals("documenter.Useage", ZString.Empty, documenter.Useage);
				if (useages.ContainsKey(documenter.Useage))
				{
					Type typeWithDuplicate = useages[documenter.Useage];
					Fail(currentType.ToString() + " contains the same Documentation.Useage as " + typeWithDuplicate.ToString() + "\r\n[" + documenter.Useage + "]\r\n.");
				}
				else
				{
					useages[documenter.Useage] = currentType;
				}

				AssertNotEquals("documenter.Explanation", ZString.Empty, documenter.Explanation);
				if (explanations.ContainsKey(documenter.Explanation))
				{
					Type typeWithDuplicate = explanations[documenter.Explanation];
					Fail(currentType.ToString() + " contains the same Documentation.Explanation as " + typeWithDuplicate.ToString() + "\r\n[" + documenter.Explanation + "]\r\n.");
				}
				else
				{
					explanations[documenter.Explanation] = currentType;
				}
			}
		}

		public void TestDataMatrix_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(DataMatrix));
		}

		public void TestUPCA_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(UPCA));
		}

		public void TestEAN13_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(EAN13));
		}

		public void TestEAN8_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(EAN8));
		}

		public void TestPDF417_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(PDF417));
		}

		public void TestCODE39_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(CODE39));
		}

		public void TestCurrency_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(Currency));
		}

		public void TestCompanyCurrencyCode_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(CompanyCurrencyCode));
		}

		public void TestCompanyCustomsCurrencyCode_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(CompanyCustomsCurrencyCode));
		}

		public void TestBarCode_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(AbriBar128sBarCode));
		}

		public void TestQrCode_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(QrCode));
		}

		public void TestLoginPhoneExtensionAndFaxNumber_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(LoginPhoneExtensionNumber));
			AssertProviderExists(typeof(LoginFaxNumber));
		}

		public void TestGetGeography_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(GetGeography));
		}

		public void TestIsPointInShape_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(IsPointInShape));
		}

		public void TestIsProductionSystem()
		{
			AssertProviderExists(typeof(IsPointInShape));
		}

		public void TestUSCustomsDisbursementChargeCodesValueProvider()
		{
			AssertProviderExists(typeof(USCustomsDisbursementChargeCodesValueProvider));
		}

		public void TestEquivalentComplianceSubTypeCode_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(EquivalentComplianceSubTypeCode));
		}

		public void TestGSTVATConversionExchangeRate_ExistsInProviderCache()
		{
			AssertProviderExists(typeof(GSTVATConversionExchangeRate));
		}

		void AssertProviderExists(Type providerType)
		{
			var providerCache = new ValueProviderCollector().ValueProviders;
			foreach (ValueProvider provider in providerCache.Providers)
			{
				if (provider.GetType() == providerType)
				{
					Assert(true);
					return;
				}
			}
			Fail(string.Format("Provider of type {0} could not be found.", providerType.Name));
		}
	}
}
