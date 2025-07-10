using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ExchangeRateTolerance : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Currency = "Currency";
			public const string ExchangeRateTolerancePercentage = "ExchangeRateTolerancePercentage";
		}

		#endregion

		public ExchangeRateTolerance()
		{
		}

		public static ExchangeRateTolerance GetDefaultExchangeRateTolerance()
		{
			var defaultValue = new ExchangeRateTolerance();
			defaultValue.Currency = ExchangeRateToleranceLookups.AllCurrencyCode;
			defaultValue.ExchangeRateTolerancePercentage = 0;

			return defaultValue;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExchangeRateTolerance();
		}

		public ExchangeRateToleranceCollection ParentCollection;

		public override MultilingualString ReasonForNotAbleToDelete => IsDefaultItem
			? ResString.GetMultilingualString("C4B2EB29-D5C3-47A9-87A5-767259C74871", "This is a system defined value and cannot be deleted.")
			: base.ReasonForNotAbleToDelete;

		public override bool CanDelete => base.CanDelete && !IsDefaultItem;

		bool IsDefaultItem => Currency == ExchangeRateToleranceLookups.AllCurrencyCode;

		[List("Lookups.CurrencyList")]
		public ZString Currency
		{
			get => currency;
			set
			{
				SetNonPersistentPropertyValue(CurrencyInfo, ref currency, value);
				if (!IsValidationSuspended)
				{
					ValidateCurrency();
				}
			}
		}
		ZString currency = ZString.Empty;

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.Currency); }
		}

		void ValidateCurrency()
		{
			CurrencyInfo.ClearAllNotifications();
			Validation.ValidateCurrency();
		}

		public bool Currency_ReadOnly => Currency == ExchangeRateToleranceLookups.AllCurrencyCode;

		[DecimalPlaces(2)]
		public ZDecimal ExchangeRateTolerancePercentage
		{
			get => exchangeRateTolerancePercentage;
			set
			{
				SetNonPersistentPropertyValue(ExchangeRateTolerancePercentageInfo, ref exchangeRateTolerancePercentage, value);
				if (!IsValidationSuspended)
				{
					ValidateExchangeRateTolerance();
				}
			}
		}
		ZDecimal exchangeRateTolerancePercentage = 0;

		public ZPropertyInfo ExchangeRateTolerancePercentageInfo
		{
			get { return GetZPropertyInfo(Schema.ExchangeRateTolerancePercentage); }
		}

		void ValidateExchangeRateTolerance()
		{
			ExchangeRateTolerancePercentageInfo.ClearAllNotifications();
			Validation.ValidateExchangeRateTolerancePercentage();
		}

		public ExchangeRateToleranceLookups Lookups => lookups ?? (lookups = new ExchangeRateToleranceLookups(this));
		ExchangeRateToleranceLookups lookups;

		public ExchangeRateToleranceValidation Validation => validation ?? (validation = new ExchangeRateToleranceValidation(this));
		ExchangeRateToleranceValidation validation;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Currency, Currency);
			writer.WriteElementString(Schema.ExchangeRateTolerancePercentage, ExchangeRateTolerancePercentage.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Currency = reader.ReadElementString(Schema.Currency);
			ExchangeRateTolerancePercentage = new ZDecimal(reader.ReadElementString(Schema.ExchangeRateTolerancePercentage));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCurrency();
			ValidateExchangeRateTolerance();
		}
	}
}
