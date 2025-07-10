using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public partial class InvoiceTotalRounding : RegistryBusinessObjectTemplate
	{
		#region Constructors

		public InvoiceTotalRounding()
		{
		}

		public InvoiceTotalRounding(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string Currency = "Currency";
			public const string RoundingOption = "RoundingOption";
			public const string RoundToCurrencyUnit = "RoundToCurrencyUnit";
		}

		#endregion

		#region Properties

		#region Currency

		[List("Currencies")]
		[MaxLength(3)]
		public ZString Currency
		{
			get => fCurrency;
			set
			{
				SetNonPersistentPropertyValue(CurrencyInfo, ref fCurrency, value);
				if (!IsValidationSuspended)
				{
					ValidateCurrency();
				}
				CurrencyInfo.RefreshBinding();
			}
		}
		ZString fCurrency;

		void ValidateCurrency()
		{
			CurrencyInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(CurrencyInfo, (IMultilingualString)ResString.GetMultilingualString("4743E0B4-58F9-4C10-834A-63776596A659", "Currency Code"));
			ListValidation.ErrorIfInvalidCode(CurrencyInfo);

			if (!CurrencyInfo.HasErrors() && ParentCollection != null && ParentCollection.Cast<InvoiceTotalRounding>().Count(x => x.Currency == Currency) > 1)
			{
				CurrencyInfo.AddError(Res.GetString("91470CE3-5C1B-4552-BC09-C1C18F576B25", "Same Currency not allowed more than once, please select another currency."));
			}

			if (!CurrencyInfo.HasErrors())
			{
				var currency = CurrentFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Currency));
				if (currency?.RX_SubUnitRatio != 100)
				{
					CurrencyInfo.AddError(Res.GetString("EA6988F3-C01F-4099-BC6F-54B7CEC51915", "Only currencies with 100 minor units can be selected, please select another currency."));
				}
			}
		}

		public ZPropertyInfo CurrencyInfo => GetZPropertyInfo(Schema.Currency);

		public RefCurrencyCollection Currencies => new RefCurrencyCollection(CurrentFactory);

		#endregion

		#region Rounding Option

		[List("RoundingOptions")]
		[MaxLength(3)]
		public ZString RoundingOption
		{
			get => fRoundingOption;
			set
			{
				SetNonPersistentPropertyValue(RoundingOptionInfo, ref fRoundingOption, value);
				if (!IsValidationSuspended)
				{
					ValidateRoundingOption();
					ValidateRoundToCurrencyUnit();
				}
				RoundingOptionInfo.RefreshBinding();
			}
		}
		ZString fRoundingOption;

		void ValidateRoundingOption()
		{
			RoundingOptionInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(RoundingOptionInfo, (IMultilingualString)ResString.GetMultilingualString("91D89307-4737-4319-815D-36869153129F", "Rounding Option"));
			ListValidation.ErrorIfInvalidCode(RoundingOptionInfo);

			if (!RoundingOptionInfo.HasErrors() && RoundingOption == AccountingConstants.RoundingOptionsCodes.RoundBasedOnMidpoint && RoundToCurrencyUnit != AccountingConstants.RoundToCurrencyUnits.OneMajorUnit)
			{
				RoundingOptionInfo.AddError(Res.GetString("A0E7B502-DDE4-4BFC-85B5-DDE141B67D45", "The round based on midpoint option can only be used with round to currency unit set to 1.00."));
			}
		}

		public ZPropertyInfo RoundingOptionInfo => GetZPropertyInfo(Schema.RoundingOption);

		public CodeDescriptionPairList RoundingOptions => AccountingConstants.RoundingOptionsList;

		public InvoiceRoundingOption RoundingOptionEnum => ConvertRoundingOption(RoundingOption);

		InvoiceRoundingOption ConvertRoundingOption(string roundingOption)
		{
			switch (roundingOption)
			{
				case RoundingOptionsCodes.AlwaysRoundUp:
					return InvoiceRoundingOption.ARU;
				case RoundingOptionsCodes.AlwaysRoundDown:
					return InvoiceRoundingOption.ARD;
				case RoundingOptionsCodes.RoundBasedOnMidpoint:
					return InvoiceRoundingOption.RBM;
				default:
					throw new ArgumentException($"{roundingOption} is not defined in InvoiceRoundingOption enum.");
			}
		}

		#endregion

		#region RoundToCurrencyUnit

		[List("RoundToCurrencyUnitsList")]
		[MaxLength(4)]
		public ZString RoundToCurrencyUnit
		{
			get => fRoundToCurrencyUnit;
			set
			{
				SetNonPersistentPropertyValue(RoundToCurrencyUnitInfo, ref fRoundToCurrencyUnit, value);
				if (!IsValidationSuspended)
				{
					ValidateRoundToCurrencyUnit();
					ValidateRoundingOption();
				}
				RoundToCurrencyUnitInfo.RefreshBinding();
			}
		}
		ZString fRoundToCurrencyUnit;

		void ValidateRoundToCurrencyUnit()
		{
			RoundToCurrencyUnitInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(RoundToCurrencyUnitInfo, (IMultilingualString)ResString.GetMultilingualString("CD95E2B1-A8B6-4B31-9FB6-0F42DB10DF5B", "Round To Currency Unit"));
			ListValidation.ErrorIfInvalidCode(RoundToCurrencyUnitInfo);
		}

		public ZPropertyInfo RoundToCurrencyUnitInfo => GetZPropertyInfo(Schema.RoundToCurrencyUnit);

		public CodeDescriptionPairList RoundToCurrencyUnitsList => AccountingConstants.RoundToCurrencyUnitsList;

		public InvoiceRoundCurrencyUnit RoundToCurrencyUnitEnum => ConvertInvoiceRoundCurrencyUnit(RoundToCurrencyUnit);

		InvoiceRoundCurrencyUnit ConvertInvoiceRoundCurrencyUnit(string roundToCurrencyUnit)
		{
			switch (roundToCurrencyUnit)
			{
				case RoundToCurrencyUnits.FiveMinorUnits:
					return InvoiceRoundCurrencyUnit.FiveMinor;
				case RoundToCurrencyUnits.TenMinorUnits:
					return InvoiceRoundCurrencyUnit.TenMinor;
				case RoundToCurrencyUnits.TwentyMinorUnits:
					return InvoiceRoundCurrencyUnit.TwentyMinor;
				case RoundToCurrencyUnits.FiftyMinorUnits:
					return InvoiceRoundCurrencyUnit.FiftyMinor;
				case RoundToCurrencyUnits.OneMajorUnit:
					return InvoiceRoundCurrencyUnit.OneMajor;
				default:
					throw new ArgumentException($"{roundToCurrencyUnit} is not defined in InvoiceRoundCurrencyUnit enum.");
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCurrency();
			ValidateRoundingOption();
			ValidateRoundToCurrencyUnit();
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new InvoiceTotalRounding(factory);

		public InvoiceTotalRoundingCollection ParentCollection => ((IBusinessObjectInternals)this).ParentCollections.Length > 0 ? (InvoiceTotalRoundingCollection)((IBusinessObjectInternals)this).ParentCollections[0] : new InvoiceTotalRoundingCollection();

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Currency = new ZString(reader.ReadElementString(Schema.Currency));
			RoundingOption = new ZString(reader.ReadElementString(Schema.RoundingOption));
			RoundToCurrencyUnit = new ZString(reader.ReadElementString(Schema.RoundToCurrencyUnit));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Currency, Currency);
			writer.WriteElementString(Schema.RoundingOption, RoundingOption);
			writer.WriteElementString(Schema.RoundToCurrencyUnit, RoundToCurrencyUnit);
		}

#if DEBUG
		public InvoiceRoundingOption ConvertRoundingOption_ForTestOnly(string roundingOption) => ConvertRoundingOption(roundingOption);
		public InvoiceRoundCurrencyUnit ConvertInvoiceRoundCurrencyUnit_ForTestOnly(string roundToCurrencyUnit) => ConvertInvoiceRoundCurrencyUnit(roundToCurrencyUnit);

#endif
	}
}
