using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BankAccountBasedOnCurrency : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string Currency = "Currency";
			public const string CurrencyDescription = "CurrencyDescription";
			public const string BankAccount = "BankAccount";
		}

		#endregion

		public BankAccountBasedOnCurrency()
		{
		}

		public BankAccountBasedOnCurrency(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BankAccountBasedOnCurrency(fallbackLevel, factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCurrency();
			ValidateBankAccount();
		}

		#region Bound Properties

		#region CurrencyDescription

		[CargoWise.ComponentModel.MaxLength(70)]
		public ZString CurrencyDescription
		{
			get
			{
				var currencyObject = CurrencyList.GetBusinessObjectFromCode(Currency);
				if (currencyObject != null)
				{
					fCurrencyDescription = currencyObject[RefCurrencySchema.RX_Desc].ToString();
				}
				return fCurrencyDescription;
			}
			set
			{
				SetNonPersistentPropertyValue<ZString>(CurrencyDescriptionInfo, ref fCurrencyDescription, value);
			}
		}

		public ZPropertyInfo CurrencyDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CurrencyDescription); }
		}

		ZString fCurrencyDescription;

		#endregion

		#region Currency

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Currency
		{
			get { return fCurrency; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(CurrencyInfo, ref fCurrency, value);
				if (!IsValidationSuspended)
				{
					ValidateCurrency();
				}
			}
		}

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.Currency); }
		}

		public void ValidateCurrency()
		{
			CurrencyInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CurrencyInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CurrencyInfo, Res.GetString("d400bca1-6fa7-484a-93a2-db60eaffa19b", "There must be only one line for each currency."));
			ListValidation.ErrorIfInvalidCode(CurrencyInfo, CurrencyList);
		}

		ZString fCurrency;
		#endregion

		#region BankAccount

		public ZGuid BankAccount
		{
			get { return fBankAccount; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(BankAccountInfo, ref fBankAccount, value);
				if (!IsValidationSuspended)
				{
					ValidateBankAccount();
				}
			}
		}

		public ZPropertyInfo BankAccountInfo
		{
			get { return GetZPropertyInfo(Schema.BankAccount); }
		}

		public void ValidateBankAccount()
		{
			BankAccountInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BankAccountInfo);
#if DEBUG
			if (!DoNotPerformListValidationOnBankAccount)
#endif
			{
				ListValidation.ErrorIfInvalidPK(BankAccountInfo, BankAccountList);
			}
		}

#if DEBUG
		internal bool DoNotPerformListValidationOnBankAccount;
#endif
		ZGuid fBankAccount;
		#endregion

		#endregion

		#region Lookups

		#region Currency List

		public IActiveBusinessObjectCollection CurrencyList
		{
			get
			{
				if (fCurrencyList == null || !fCurrencyList.Factory.IsOwnedByCurrentThread)
				{
					fCurrencyList = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefCurrencyCollection>(), new object[] { CurrentFactory });
				}

				return fCurrencyList;
			}
		}
		IActiveBusinessObjectCollection fCurrencyList;

		#endregion

		#region BankAccount List

		public BusinessObjectCollection BankAccountList
		{
			get
			{
				object[] parameters = CurrentFallbackLevel == null ? new object[] { CurrentFactory } : new object[] { CurrentFactory, new ZQuery(AccBankAccountSchema.AB_GC, CurrentFallbackLevel.CompanyPK(false)) };
				var bankAccountList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccBankAccountCollection>(), parameters);
				bankAccountList.Load();
				return bankAccountList;
			}
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Currency, Currency.ToString());
			writer.WriteElementString(Schema.BankAccount, BankAccount.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Currency = new ZString(reader.ReadElementString(Schema.Currency));
			BankAccount = new ZGuid(reader.ReadElementString(Schema.BankAccount));
		}

		#endregion
	}
}
