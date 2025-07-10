using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ExchangeRateTypes = Enterprise.Core.Constants.ExchangeRateTypes;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	[TestedType(typeof(MyWrapperCollection))]
	sealed class GenericWrapperCollectionNonInheritedTest : GenericWrapperCollectionTest<GenericWrapperCollectionNonInheritedTest.MyWrapperCollection>
	{
		[TestDate(2006, 12, 25)]
		public void TestCurrencyConversionWorksWhenWrappedObjectIsNotAnICurrencyConverterProvider()
		{
			SetupForCurrencyConversionTests();
			MyWrapperCollection collection = new MyWrapperCollection(Factory);
			collection.Add(new MyWrapper(120.00m, "NZD", Factory));
			collection.Add(new MyWrapper(120.00m, "USD", Factory));

			IBODocDataProviderCollection dataProviderCollection = collection;
			AssertEquals("collection.Total('MegaBucks', '')", "90.00 ERN", dataProviderCollection.Total("MegaBucks", "", null));
		}

		[TestDate(2006, 12, 25)]
		public void TestCurrencyConversionWorksWhenWrappedObjectIsAnICurrencyConverterProvider()
		{
			SetupForCurrencyConversionTests();
			MyWrapperCollection collection = new MyWrapperCollection(Factory);
			collection.Add(new MyWrapper(120.00m, "NZD", Factory, new MyBO(Factory)));
			collection.Add(new MyWrapper(120.00m, "USD", Factory, new MyBO(Factory)));
			IBODocDataProviderCollection dataProviderCollection = collection;
			AssertEquals("collection.Total('MegaBucks', '')", "64.00 ERN", dataProviderCollection.Total("MegaBucks", "", null));
		}

		[TestDate(2006, 12, 25)]
		public void TestMoneyWrapperCollectionTotalWithFilter()
		{
			SetupForCurrencyConversionTests();
			var collection = new MyWrapperCollection(Factory)
			{
				new MyWrapper(120.00m, "NZD", Factory),
				new MyWrapper(120.00m, "USD", Factory),
				new MyWrapper(120.00m, "USD", Factory)
			};

			var dataProviderCollection = (IBODocDataProviderCollection)collection;
			AssertEquals("collection.Total('MegaBucks', '{CurrencyCode}' == 'USD')", "240.00 USD", dataProviderCollection.Total("MegaBucks", "", "\"{CurrencyCode}\" == \"USD\""));
			AssertEquals("collection.Total('MegaBucks', '{CurrencyCode}' == 'NZD')", "120.00 NZD", dataProviderCollection.Total("MegaBucks", "", "\"{CurrencyCode}\" == \"NZD\""));
			AssertEquals("collection.Total('MegaBucks', '{CurrencyCode}' != ' ')", "120.00 ERN", dataProviderCollection.Total("MegaBucks", "", "\"{CurrencyCode}\" != \"\""));
		}

		#region Implementation
		void SetupForCurrencyConversionTests()
		{
			TestCaseHelper.ClearTable(RefExchangeRateSchema.Constants.TableName);

			RefCurrency nZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");

			GetNewExchangeRate(nZD, 2m, ExchangeRateTypes.Code.BuyRate, new ZDateTime(2006, 12, 25), new ZDateTime(2006, 12, 31));
			GetNewExchangeRate(nZD, 3m, ExchangeRateTypes.Code.SellRate, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 7));
			GetNewExchangeRate(uSD, 4m, ExchangeRateTypes.Code.BuyRate, new ZDateTime(2006, 12, 25), new ZDateTime(2006, 12, 31));
			GetNewExchangeRate(uSD, 5m, ExchangeRateTypes.Code.CustomsRate, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 7));
		}

		RefExchangeRate GetNewExchangeRate(RefCurrency currency, ZDecimal rate, ZString type, ZDateTime startDate, ZDateTime finishDate)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();
			result.RE_RX_NKExCurrency = currency.RX_Code;
			result.RE_ExRateType = type.ToString();
			result.RE_GC = GlbCompany.CurrentCompany.PK;
			result.RE_SellRate = rate;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = finishDate;
			return result;
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new MyWrapper(23.45, "NZD", Factory);
		}

		protected override MyWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new MyWrapperCollection(Factory);
		}

		public class MyBO : NonPersistentBusinessObject, ICurrencyConverterProvider
		{
			public MyBO(BusinessObjectFactory factory)
				: base(factory)
			{
				fCurrencyConverter = new RefCurrencyCurrencyConverter(factory, new ZDateTime(2007, 1, 2), ExchangeRateType.All, 7);
			}

			public CurrencyConverter CurrencyConverter
			{
				get { return fCurrencyConverter; }
			}
			readonly CurrencyConverter fCurrencyConverter;
		}

		public class MyWrapper : GenericWrapper
		{
			public MyWrapper(ZDecimal value, ZString currencyCode, BusinessObjectFactory factory, MyBO wrappedBO)
				: this(wrappedBO, factory, new Money(value, RefCurrency.LoadFromCurrencyCode(factory, currencyCode)))
			{
				CurrencyCode = currencyCode;
			}

			public MyWrapper(ZDecimal value, ZString currencyCode, BusinessObjectFactory factory)
				: this(null, factory, new Money(value, RefCurrency.LoadFromCurrencyCode(factory, currencyCode)))
			{
				CurrencyCode = currencyCode;
			}

			MyWrapper(MyBO wrappedBO, BusinessObjectFactory factory, Money megaBucks)
				: base(wrappedBO, factory)
			{
				fMegaBucks = new MoneyWrapper(megaBucks, factory);
			}

			public MoneyWrapper MegaBucks
			{
				get { return fMegaBucks; }
			}
			readonly MoneyWrapper fMegaBucks;

			public ZString CurrencyCode { get; set; }
		}

		public class MyWrapperCollection : GenericWrapperCollection<MyWrapper>
		{
			public MyWrapperCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}
		#endregion
	}
}
