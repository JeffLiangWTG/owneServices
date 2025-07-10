using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[XmlRoot("ChargeDetails")] //for compatibility with existing XMLs
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ARCreditNoteApprovalRequestChargeDetails : ApprovalRequestChargeDetails
	{
		#region Schema

		public new abstract class Schema : ApprovalRequestChargeDetails.Schema
		{
			public const string SellAccount = "SellAccount";
			public const string SellCurrency = "SellCurrency";
			public const string OSSellAmount = "OSSellAmount";
			public const string LocalSellAmount = "LocalSellAmount";
			public const string InvoiceType = "InvoiceType";
			public const string OSTaxAmount = "OSTaxAmount";
			public const string LocalTaxAmount = "LocalTaxAmount";
			public const string ExchangeRate = "ExchangeRate";
			public const string TaxCode = "TaxCode";
			public const string SupplyType = "SupplyType";
		}

		#endregion

		[Obsolete("For serializer only")]
		public ARCreditNoteApprovalRequestChargeDetails()
			: base(new BusinessObjectFactory())
		{
		}

		public ARCreditNoteApprovalRequestChargeDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void CopyInstanceSpecificFieldsFrom(ApprovalRequestChargeDetails chargeDetailsToCopy)
		{
			var chargeDetailsToCopy_Casted = chargeDetailsToCopy as ARCreditNoteApprovalRequestChargeDetails;
			if (chargeDetailsToCopy_Casted != null)
			{
				SellAccount = chargeDetailsToCopy_Casted.SellAccount;
				SellCurrency = chargeDetailsToCopy_Casted.SellCurrency;
				OSSellAmount = chargeDetailsToCopy_Casted.OSSellAmount;
				LocalSellAmount = chargeDetailsToCopy_Casted.LocalSellAmount;
				InvoiceType = chargeDetailsToCopy_Casted.InvoiceType;
				OSTaxAmount = chargeDetailsToCopy_Casted.OSTaxAmount;
				LocalTaxAmount = chargeDetailsToCopy_Casted.LocalTaxAmount;
				ExchangeRate = chargeDetailsToCopy_Casted.ExchangeRate;
				TaxCode = chargeDetailsToCopy_Casted.TaxCode;
				SupplyType = chargeDetailsToCopy_Casted.SupplyType;
			}
		}

		protected override bool AreInstanceSpecificFieldsEqual(ApprovalRequestChargeDetails b)
		{
			var b_Casted = b as ARCreditNoteApprovalRequestChargeDetails;
			if (b_Casted != null)
			{
				return
					SellAccount == b_Casted.SellAccount &&
					SellCurrency == b_Casted.SellCurrency &&
					OSSellAmount == b_Casted.OSSellAmount &&
					LocalSellAmount == b_Casted.LocalSellAmount &&
					InvoiceType == b_Casted.InvoiceType &&
					OSTaxAmount == b_Casted.OSTaxAmount &&
					LocalTaxAmount == b_Casted.LocalTaxAmount &&
					ExchangeRate == b_Casted.ExchangeRate &&
					TaxCode == b_Casted.TaxCode &&
					SupplyType == b_Casted.SupplyType;
			}

			return false;
		}

		public ZString ApprovalType { get; set; }
		ZDecimal multiplier => ApprovalType == Core.Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal ? -1 : 1;

		#region SellAccount

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|SellAccount", Caption = "Sell Account")]
		public ZString SellAccount
		{
			get { return SellAccount_cached; }
			set
			{
				SetNonPersistentPropertyValue(SellAccountInfo, ref SellAccount_cached, value);
			}
		}
		ZString SellAccount_cached;

		ZPropertyInfo SellAccountInfo
		{
			get { return GetZPropertyInfo(Schema.SellAccount); }
		}

		#endregion

		#region SellCurrency

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|SellCurrency", Caption = "Sell Currency")]
		public ZString SellCurrency
		{
			get { return SellCurrency_cached; }
			set
			{
				SetNonPersistentPropertyValue(SellCurrencyInfo, ref SellCurrency_cached, value);
			}
		}
		ZString SellCurrency_cached;

		ZPropertyInfo SellCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.SellCurrency); }
		}

		#endregion

		#region OSSellAmount

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|OSSellAmount", Caption = "Sell Amount")]
		public ZDecimal OSSellAmount
		{
			get { return OSSellAmount_cached; }
			set
			{
				SetNonPersistentPropertyValue(OSSellAmountInfo, ref OSSellAmount_cached, value);
			}
		}
		ZDecimal OSSellAmount_cached;

		ZPropertyInfo OSSellAmountInfo
		{
			get { return GetZPropertyInfo(Schema.OSSellAmount); }
		}

		public ZDecimal OSSellAmountForDisplay => OSSellAmount * multiplier;

		#endregion

		#region OSTaxAmount

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|OSTaxAmount", Caption = "OS Tax Amount")]
		public ZDecimal OSTaxAmount
		{
			get { return OSTaxAmount_cached; }
			set
			{
				SetNonPersistentPropertyValue(OSTaxAmountInfo, ref OSTaxAmount_cached, value);
			}
		}
		ZDecimal OSTaxAmount_cached;

		ZPropertyInfo OSTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.OSTaxAmount); }
		}

		#endregion

		#region LocalTaxAmount

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|LocalTaxAmount", Caption = "Local Tax Amount")]
		public ZDecimal LocalTaxAmount
		{
			get { return LocalTaxAmount_cached; }
			set
			{
				SetNonPersistentPropertyValue(LocalTaxAmountInfo, ref LocalTaxAmount_cached, value);
			}
		}
		ZDecimal LocalTaxAmount_cached;

		ZPropertyInfo LocalTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.LocalTaxAmount); }
		}

		#endregion

		#region ExchangeRate

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|ExchangeRate", Caption = "Exchange Rate")]
		public ZDecimal ExchangeRate
		{
			get { return ExchangeRate_cached; }
			set
			{
				SetNonPersistentPropertyValue(ExchangeRateInfo, ref ExchangeRate_cached, value);
			}
		}
		ZDecimal ExchangeRate_cached;

		ZPropertyInfo ExchangeRateInfo
		{
			get { return GetZPropertyInfo(Schema.ExchangeRate); }
		}

		#endregion

		#region LocalSellAmount

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|LocalSellAmount", Caption = "Local Sell Amount")]
		public ZDecimal LocalSellAmount
		{
			get { return LocalSellAmount_cached; }
			set
			{
				SetNonPersistentPropertyValue(LocalSellAmountInfo, ref LocalSellAmount_cached, value);
			}
		}
		ZDecimal LocalSellAmount_cached;

		ZPropertyInfo LocalSellAmountInfo
		{
			get { return GetZPropertyInfo(Schema.LocalSellAmount); }
		}

		public ZDecimal LocalSellAmountForDisplay => LocalSellAmount * multiplier;

		#endregion

		#region InvoiceType

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|InvoiceType", Caption = "Invoice Type")]
		public ZString InvoiceType
		{
			get { return InvoiceType_cached; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceTypeInfo, ref InvoiceType_cached, value);
			}
		}
		ZString InvoiceType_cached;

		ZPropertyInfo InvoiceTypeInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceType); }
		}

		#endregion

		#region TaxCode

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|TaxCode", Caption = "Tax Code")]
		public ZString TaxCode
		{
			get { return TaxCode_cached; }
			set
			{
				SetNonPersistentPropertyValue(TaxCodeInfo, ref TaxCode_cached, value);
			}
		}
		ZString TaxCode_cached;

		ZPropertyInfo TaxCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxCode); }
		}

		#endregion

		#region SupplyType

		[ResourceStringData("ARCreditNoteApprovalChargeDetails|SupplyType", Caption = "Supply Type")]
		public ZString SupplyType
		{
			get => SupplyType_cached;
			set => SetNonPersistentPropertyValue(SupplyTypeInfo, ref SupplyType_cached, value);
		}
		ZString SupplyType_cached;

		ZPropertyInfo SupplyTypeInfo => GetZPropertyInfo(Schema.SupplyType);

		#endregion

		#region IXmlSerializable Members

		protected override void ReadXmlForInstanceSpecificFields(XElement xElement)
		{
			xElement.TryToSetValueFromXElelment<ZString>(Schema.SellAccount, x => SellAccount = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.SellCurrency, x => SellCurrency = x);
			xElement.TryToSetValueFromXElelment<ZDecimal>(Schema.OSSellAmount, x => OSSellAmount = x);
			xElement.TryToSetValueFromXElelment<ZDecimal>(Schema.LocalSellAmount, x => LocalSellAmount = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.InvoiceType, x => InvoiceType = x);
			xElement.TryToSetValueFromXElelment<ZDecimal>(Schema.OSTaxAmount, x => OSTaxAmount = x);
			xElement.TryToSetValueFromXElelment<ZDecimal>(Schema.LocalTaxAmount, x => LocalTaxAmount = x);
			xElement.TryToSetValueFromXElelment<ZDecimal>(Schema.ExchangeRate, x => ExchangeRate = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.TaxCode, x => TaxCode = x);
			xElement.TryToSetValueFromXElelment<ZString>(Schema.SupplyType, x => SupplyType = x);
		}

		protected override void WriteXmlForInstanceSpecificFields(XmlWriter writer)
		{
			writer.WriteElementString(Schema.SellAccount, SellAccount);
			writer.WriteElementString(Schema.SellCurrency, SellCurrency);
			writer.WriteElementString(Schema.OSSellAmount, OSSellAmount.ToString());
			writer.WriteElementString(Schema.LocalSellAmount, LocalSellAmount.ToString());
			writer.WriteElementString(Schema.InvoiceType, InvoiceType);
			writer.WriteElementString(Schema.OSTaxAmount, OSTaxAmount.ToString());
			writer.WriteElementString(Schema.LocalTaxAmount, LocalTaxAmount.ToString());
			writer.WriteElementString(Schema.ExchangeRate, ExchangeRate.ToString());
			writer.WriteElementString(Schema.TaxCode, TaxCode);
			writer.WriteElementString(Schema.SupplyType, SupplyType);
		}

		#endregion
	}
}
