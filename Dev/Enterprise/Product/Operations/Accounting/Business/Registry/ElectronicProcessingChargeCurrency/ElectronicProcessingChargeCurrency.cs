using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ElectronicProcessingChargeCurrency : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string CurrencyPK = "CurrencyPK";
			public const string ValidFromDate = "ValidFromDate";
		}

		#endregion Schema

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ElectronicProcessingChargeCurrency();
		}

		public ElectronicProcessingChargeCurrencyCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ElectronicProcessingChargeCurrencyCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		[RelatedBusinessObject("Currency")]
		[List("Currencies")]
		public ZGuid CurrencyPK
		{
			get => currencyPK;
			set
			{
				SetNonPersistentPropertyValue(CurrencyPKInfo, ref currencyPK, value);

				ValidateCurrencyPK();
			}
		}
		ZGuid currencyPK = ZGuid.Empty;

		public ZPropertyInfo CurrencyPKInfo
		{
			get { return GetZPropertyInfo(Schema.CurrencyPK, ResString.GetMultilingualString("9803FFD9-D81F-480E-8F94-3CF176F83DC5", "Currency")); }
		}

		public RefCurrency Currency
		{
			get { return CurrentFactory.Load<RefCurrency>(currencyPK); }
		}

		public RefCurrencyCollection Currencies
		{
			get
			{
				return new RefCurrencyCollection(CurrentFactory);
			}
		}

		public ZDateTime ValidFromDate
		{
			get => validFromDate;
			set
			{
				SetNonPersistentPropertyValue(ValidFromDateInfo, ref validFromDate, value);

				ValidateValidFromDate();
			}
		}
		ZDateTime validFromDate = ZDateTime.Empty;

		public ZPropertyInfo ValidFromDateInfo
		{
			get { return GetZPropertyInfo(Schema.ValidFromDate); }
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CurrencyPK, CurrencyPK.ToString());
			writer.WriteElementString(Schema.ValidFromDate, ValidFromDate.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CurrencyPK = new ZGuid(reader.ReadElementString(Schema.CurrencyPK));
			ValidFromDate = new ZDateTime(reader.ReadElementString(Schema.ValidFromDate));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCurrencyPK();
			ValidateValidFromDate();
		}

		void ValidateCurrencyPK()
		{
			CurrencyPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CurrencyPKInfo);
			ListValidation.ErrorIfInvalidPK(CurrencyPKInfo);
		}

		void ValidateValidFromDate()
		{
			var errorMessage = ResString.GetMultilingualString("D74F80CE-6A61-44CA-AC62-2F8E00BE0596", "This {0} has been duplicated and must be unique.", Schema.ValidFromDate);
			ValidFromDateInfo.ClearAllNotifications();

			if (CheckDuplicated())
			{
				ValidFromDateInfo.AddError(errorMessage);
			}

			bool CheckDuplicated()
			{
				if (ParentCollection == null)
				{
					return false;
				}

				return ParentCollection.OfType<ElectronicProcessingChargeCurrency>()
					.Except([this])
					.Any(item => item.ValidFromDate == ValidFromDate);
			}
		}
	}
}
