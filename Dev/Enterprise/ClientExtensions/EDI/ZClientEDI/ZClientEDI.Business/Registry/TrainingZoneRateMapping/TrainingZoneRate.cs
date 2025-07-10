using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	internal interface ICurrentFactory
	{
		BusinessObjectFactory CurrentFactory { get; }
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class TrainingZoneRate : RegistryBusinessObjectTemplate, ICurrentFactory
	{
		#region Schema

		public static class Schema
		{
			public const string Zone = "Zone";
			public const string RateAmount = "RateAmount";
			public const string Currency = "Currency";
		}

		#endregion

		#region Properties

		#region Zone

		public ZGuid ZonePK
		{
			get { return zonePK; }
			set
			{
				SetNonPersistentPropertyValue(ZonePKInfo, ref zonePK, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateZonePK();
				}
			}
		}

		public RefZoneHeader Zone
		{
			get { return CurrentFactory.Load<RefZoneHeader>(ZonePK); }
		}

		public ZPropertyInfo ZonePKInfo
		{
			get { return GetZPropertyInfo(nameof(ZonePK)); }
		}

		ZGuid zonePK;

		#endregion

		#region RateAmount

		public ZDecimal RateAmount
		{
			get { return rateAmount; }
			set
			{
				SetNonPersistentPropertyValue(RateAmountInfo, ref rateAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRateAmount();
				}
			}
		}

		public ZPropertyInfo RateAmountInfo
		{
			get { return GetZPropertyInfo(nameof(RateAmount)); }
		}

		ZDecimal rateAmount;

		#endregion

		#region Currency

		[MaxLength(RefCurrency.Schema.RX_CodeMaxLength)]
		public ZString CurrencyCode
		{
			get { return currencyCode; }
			set
			{
				SetNonPersistentPropertyValue(CurrencyCodeInfo, ref currencyCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCurrencyCode();
				}
			}
		}

		public RefCurrency Currency
		{
			get { return RefCurrency.LoadFromCurrencyCode(CurrentFactory, CurrencyCode); }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		ZString currencyCode;

		#endregion

		#endregion

		#region Lookups

		public TrainingZoneRateLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new TrainingZoneRateLookups(this);
				}
				return lookups;
			}
		}

		TrainingZoneRateLookups lookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public TrainingZoneRateValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual TrainingZoneRateValidation GetNewValidation()
		{
			return new TrainingZoneRateValidation(this);
		}

		#endregion

		#region Get Clone and Equality

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TrainingZoneRate();
		}

		public override bool Equals(object obj)
		{
			TrainingZoneRate otherRate = obj as TrainingZoneRate;
			if (otherRate == null)
			{
				return false;
			}

			return
#if DEBUG
 DummyPKToEnsureUniquenessForTest == otherRate.DummyPKToEnsureUniquenessForTest &&
#endif
 ZonePK == otherRate.ZonePK &&
				RateAmount == otherRate.RateAmount &&
								CurrencyCode == otherRate.CurrencyCode;
		}

		public override int GetHashCode()
		{
			return
			ZonePK.GetHashCode() ^
			RateAmount.GetHashCode() ^
			CurrencyCode.GetHashCode();
		}

#if DEBUG
		public ZGuid DummyPKToEnsureUniquenessForTest;
#endif

		#endregion

		#region XML Serialization

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Zone, ZonePK.ToString());
			writer.WriteElementString(Schema.RateAmount, RateAmount.ToString());
			writer.WriteElementString(Schema.Currency, CurrencyCode.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			ZonePK = new ZGuid(wrapper.ReadElementString(Schema.Zone));
			RateAmount = wrapper.ReadElementStringAsZDecimal(Schema.RateAmount);
			CurrencyCode = wrapper.ReadElementString(Schema.Currency);
		}

		#endregion

		public void SetParentCollection(TrainingZoneRateCollection collection)
		{
			if (ParentCollection != null && ParentCollection != collection)
			{
				throw new InvalidOperationException("Should only have one parent collection");
			}

			ParentCollection = collection;
		}

		[BusinessObjectTestExclude] // These properties implement setters on collections, which is generally not advised.
		public TrainingZoneRateCollection ParentCollection { get; private set; }

		#region ITrainingZoneRate Members

		BusinessObjectFactory ICurrentFactory.CurrentFactory
		{
			get { return CurrentFactory; }
		}

		#endregion
	}
}

