using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ExchangeRateToleranceCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new ExchangeRateTolerance this[int i]
		{
			get { return SetParent((ExchangeRateTolerance)Elements[i]); }
		}

		public new ExchangeRateTolerance AddNew()
		{
			return SetParent((ExchangeRateTolerance)base.AddNew());
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExchangeRateToleranceCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return SetParent(new ExchangeRateTolerance());
		}

		ExchangeRateTolerance SetParent(ExchangeRateTolerance method)
		{
			method.ParentCollection = this;
			return method;
		}

		public ExchangeRateTolerance FindValueWithFallback(string currencyCode)
		{
			ExchangeRateTolerance bestResult = null;

			foreach (ExchangeRateTolerance item in this)
			{
				if (item.Currency == currencyCode)
				{
					bestResult = item;
					break;
				}
				else if (item.Currency == ExchangeRateToleranceLookups.AllCurrencyCode)
				{
					bestResult = item;
				}
			}

			return bestResult ?? ExchangeRateTolerance.GetDefaultExchangeRateTolerance();
		}
	}
}
