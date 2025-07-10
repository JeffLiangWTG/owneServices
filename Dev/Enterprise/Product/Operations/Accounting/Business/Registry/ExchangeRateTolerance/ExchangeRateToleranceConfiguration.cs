using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ExchangeRateToleranceConfiguration : RegistryBusinessObjectTemplate
	{
		public ExchangeRateToleranceConfiguration()
		{
		}

		public ExchangeRateToleranceConfiguration(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExchangeRateToleranceConfiguration(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			ExchangeRateToleranceConfiguration castedClone = (ExchangeRateToleranceConfiguration)clone;
			if (ExchangeRateToleranceCollection != null)
			{
				castedClone.exchangeRateToleranceCollection = (ExchangeRateToleranceCollection)ExchangeRateToleranceCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.ExchangeRateToleranceCollection);
			}
		}

		#region Bound Properties

		#region ExchangeRateToleranceCollection

		public ExchangeRateToleranceCollection ExchangeRateToleranceCollection
		{
			get
			{
				if (exchangeRateToleranceCollection == null)
				{
					exchangeRateToleranceCollection = new ExchangeRateToleranceCollection();
					RegisterEditableChildObject(ExchangeRateToleranceCollection);
				}
				return exchangeRateToleranceCollection;
			}
		}
		ExchangeRateToleranceCollection exchangeRateToleranceCollection;

		ZXmlSerializer fExchangeRateToleranceCollection;
		ZXmlSerializer ExchangeRateToleranceCollectionSerialiser
		{
			get
			{
				return fExchangeRateToleranceCollection ?? (fExchangeRateToleranceCollection = ZXmlSerializer.New(typeof(ExchangeRateToleranceCollection)));
			}
		}

		#endregion

		#endregion

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			ExchangeRateToleranceCollectionSerialiser.Serialize(writer, ExchangeRateToleranceCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			exchangeRateToleranceCollection = (ExchangeRateToleranceCollection)ExchangeRateToleranceCollectionSerialiser.Deserialize(reader);

			if (exchangeRateToleranceCollection.Cast<ExchangeRateTolerance>().All(x => x.Currency != ExchangeRateToleranceLookups.AllCurrencyCode))
			{
				exchangeRateToleranceCollection.Add(ExchangeRateTolerance.GetDefaultExchangeRateTolerance());
			}

			exchangeRateToleranceCollection.Sort(new CurrencyCompare());

			RegisterEditableChildObject(ExchangeRateToleranceCollection);
		}

		sealed class CurrencyCompare : IComparer<ExchangeRateTolerance>
		{
			public int Compare(ExchangeRateTolerance x, ExchangeRateTolerance y)
			{
				if (x.Currency == y.Currency && x.Currency == ExchangeRateToleranceLookups.AllCurrencyCode)
				{
					return 0;
				}
				else if (x.Currency == ExchangeRateToleranceLookups.AllCurrencyCode)
				{
					return -1;
				}
				else if (y.Currency == ExchangeRateToleranceLookups.AllCurrencyCode)
				{
					return 1;
				}
				else
				{
					return StringComparer.Ordinal.Compare(x.Currency, y.Currency);
				}
			}
		}
	}
}
