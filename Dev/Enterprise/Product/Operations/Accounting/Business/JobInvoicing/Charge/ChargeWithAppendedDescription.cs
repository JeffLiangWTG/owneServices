using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class ChargeWithAppendedDescription : NonPersistentBusinessObject
	{
		public ChargeWithAppendedDescription(Charge charge, BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(charge, "Charge");
			this.charge = charge;
			ResetOriginalDescription();
		}
		readonly Charge charge;

		public ZGuid ChargePK
		{
			get { return charge.PK; }
		}

		#region JR_AC
		[List("Lookups.ChargeCodes")]
		public ZGuid JR_AC
		{
			get { return charge.JR_AC; }
		}

		#endregion

		public ZString OriginalDescription
		{
			get
			{
				return originalDescription;
			}
		}
		ZString originalDescription;

#if DEBUG
		public ZString LastTextToAppend_ForTestOnly => lastTextToAppend;
#endif

		ZString lastTextToAppend;

		void ResetOriginalDescription()
		{
			originalDescription = (ZString)charge.JR_DescInfo.OriginalValue;
		}

		[MaxLength("AppendedTextMaxLength")]
		public ZString TextToAppend
		{
			get { return textToAppend; }
			set { SetNonPersistentPropertyValue(TextToAppendInfo, ref textToAppend, value); }
		}
		ZString textToAppend;
		public ZPropertyInfo TextToAppendInfo
		{
			get { return GetZPropertyInfo(nameof(TextToAppend)); }
		}

		public int AppendedTextMaxLength
		{
			get { return JobCharge.Schema.JR_DescMaxLength - OriginalDescription.Length; }
		}

		public ZString JR_Desc
		{
			get { return OriginalDescription + TextToAppend; }
		}

		#region JR_JH
		[ReadOnly(true)]
		[List("Lookups.Jobs")]
		public ZGuid JR_JH
		{
			get { return charge.JR_JH; }
		}
		#endregion

		#region JR_GB

		[ReadOnly(true)]
		[List("Lookups.Branches")]
		public ZGuid JR_GB
		{
			get { return charge.JR_GB; }
		}

		#endregion

		#region JR_GE

		[List("Lookups.Departments")]
		public ZGuid JR_GE
		{
			get { return charge.JR_GE; }
		}
		#endregion

		public ZString JR_RX_NKSellCurrency
		{
			get { return charge.JR_RX_NKSellCurrency; }
		}

		public ZDecimal JR_OSSellAmt
		{
			get { return charge.JR_OSSellAmt; }
		}

		public ZDecimal JR_LocalSellAmt
		{
			get { return charge.JR_LocalSellAmt; }
		}

		#region JR_AT_SellGSTRate

		[List("GSTCollection")]
		public ZGuid JR_AT_SellGSTRate
		{
			get { return charge.JR_AT_SellGSTRate; }
		}

		#endregion

		public ZDecimal JR_OSSellGSTAmt_Calc
		{
			get { return charge.JR_OSSellGSTAmt_Calc; }
		}

		public ZString JR_InvoiceType
		{
			get { return charge.JR_InvoiceType; }
		}

		public ZString ChargeType
		{
			get { return charge.ChargeType; }
		}

		public ZString JR_RX_NKCostCurrency
		{
			get { return charge.JR_RX_NKCostCurrency; }
		}

		public ZDecimal JR_OSCostAmt
		{
			get { return charge.JR_OSCostAmt; }
		}

		public ZDecimal JR_LocalCostAmt
		{
			get { return charge.JR_LocalCostAmt; }
		}

		#region JR_AT_CostGSTRate

		[List("GSTCollection")]
		public ZGuid JR_AT_CostGSTRate
		{
			get { return charge.JR_AT_CostGSTRate; }
		}
		#endregion

		public ZDecimal JR_OSCostGSTAmt
		{
			get { return charge.JR_OSCostGSTAmt_Calc; }
		}

		public JobChargeLookups Lookups
		{
			get { return charge.Lookups; }
		}

		public AccTaxRateCollection GSTCollection
		{
			get { return charge.GSTCollection; }
		}

		public override bool IsDeleted
		{
			get { return charge.IsDeleted; }
		}

		public void AppendText()
		{
			charge.JR_Desc = JR_Desc;
			lastTextToAppend = TextToAppend;
		}

		public void CancelAppendText()
		{
			TextToAppend = lastTextToAppend;
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				TextToAppend = ZString.Empty;
				lastTextToAppend = ZString.Empty;
				ResetOriginalDescription();
			}
		}
	}
}