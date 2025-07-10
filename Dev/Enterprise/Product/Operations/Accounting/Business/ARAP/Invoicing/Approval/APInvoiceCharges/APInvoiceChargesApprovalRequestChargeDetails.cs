using System;
using System.Xml;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceChargesApprovalRequestChargeDetails : ApprovalRequestChargeDetails
	{
		#region Schema

		public new abstract class Schema : ApprovalRequestChargeDetails.Schema
		{
			public const string CostCurrency = "CostCurrency";
			public const string OSCostAmount = "OSCostAmount";
			public const string LocalCostAmount = "LocalCostAmount";
		}

		#endregion

		[Obsolete("For serializer only")]
		public APInvoiceChargesApprovalRequestChargeDetails()
			: base(new BusinessObjectFactory())
		{
		}

		public APInvoiceChargesApprovalRequestChargeDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void CopyInstanceSpecificFieldsFrom(ApprovalRequestChargeDetails chargeDetailsToCopy)
		{
			var chargeDetailsToCopy_Casted = chargeDetailsToCopy as APInvoiceChargesApprovalRequestChargeDetails;
			if (chargeDetailsToCopy_Casted != null)
			{
				CostCurrency = chargeDetailsToCopy_Casted.CostCurrency;
				OSCostAmount = chargeDetailsToCopy_Casted.OSCostAmount;
				LocalCostAmount = chargeDetailsToCopy_Casted.LocalCostAmount;
			}
		}

		protected override bool AreInstanceSpecificFieldsEqual(ApprovalRequestChargeDetails b)
		{
			var b_Casted = b as APInvoiceChargesApprovalRequestChargeDetails;
			if (b_Casted != null)
			{
				return
					CostCurrency == b_Casted.CostCurrency &&
					OSCostAmount == b_Casted.OSCostAmount &&
					LocalCostAmount == b_Casted.LocalCostAmount;
			}

			return false;
		}

		#region CostCurrency

		[ResourceStringData("APInvoiceChargesApprovalChargeDetails|CostCurrency", Caption = "Cost Currency")]
		public ZString CostCurrency
		{
			get { return costCurrency; }
			set
			{
				SetNonPersistentPropertyValue(CostCurrencyInfo, ref costCurrency, value);
			}
		}
		ZString costCurrency;

		ZPropertyInfo CostCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.CostCurrency); }
		}

		#endregion

		#region OSCostAmount

		[DecimalPlaces(nameof(OSDecimals))]
		[ResourceStringData("APInvoiceChargesApprovalChargeDetails|OSCostAmount", Caption = "Cost Amount")]
		public ZDecimal OSCostAmount
		{
			get { return osCostAmount; }
			set
			{
				SetNonPersistentPropertyValue(OSCostAmountInfo, ref osCostAmount, value);
			}
		}
		ZDecimal osCostAmount;

		ZPropertyInfo OSCostAmountInfo
		{
			get { return GetZPropertyInfo(Schema.OSCostAmount); }
		}

		#endregion

		#region LocalCostAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		[ResourceStringData("APInvoiceChargesApprovalChargeDetails|LocalCostAmount", Caption = "Local Cost Amount")]
		public ZDecimal LocalCostAmount
		{
			get { return localCostAmount; }
			set
			{
				SetNonPersistentPropertyValue(LocalCostAmountInfo, ref localCostAmount, value);
			}
		}
		ZDecimal localCostAmount;

		ZPropertyInfo LocalCostAmountInfo
		{
			get { return GetZPropertyInfo(Schema.LocalCostAmount); }
		}

		#endregion

		#region Decimals

		public int? OSDecimals => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CostCurrency)?.Decimals;

		#endregion

		#region IXmlSerializable Members

		protected override void ReadXmlForInstanceSpecificFields(XElement xElement)
		{
			xElement.TryToSetValueFromXElelment<ZString>(Schema.CostCurrency, x => CostCurrency = x);
			xElement.TryToSetValueFromXElelment<ZDecimal>(Schema.OSCostAmount, x => OSCostAmount = x);
			xElement.TryToSetValueFromXElelment<ZDecimal>(Schema.LocalCostAmount, x => LocalCostAmount = x);
		}

		protected override void WriteXmlForInstanceSpecificFields(XmlWriter writer)
		{
			writer.WriteElementString(Schema.CostCurrency, CostCurrency);
			writer.WriteElementString(Schema.OSCostAmount, OSCostAmount.ToString());
			writer.WriteElementString(Schema.LocalCostAmount, LocalCostAmount.ToString());
		}

		#endregion
	}
}
