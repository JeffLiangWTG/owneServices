using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.UPE.Business
{
	public class DocCalloutCharge : DocumentWrapper
	{
		protected DocCalloutCharge(CalloutCharge calloutCharge, BusinessObjectFactory factoryToWrap)
			: base(calloutCharge, factoryToWrap)
		{
		}

		public static DocCalloutCharge New(CalloutCharge calloutCharge, BusinessObjectFactory factoryToWrap)
		{
			return (calloutCharge == null) ? null : new DocCalloutCharge(calloutCharge, factoryToWrap);
		}

		protected CalloutCharge CalloutCharge
		{
			get { return (CalloutCharge)WrappedObject; }
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public ZDecimal Amount
		{
			get { return CalloutCharge.Amount; }
		}

		public ZDecimal NonTaxableAmount
		{
			get { return CalloutCharge.NonTaxableAmount; }
		}

		public ZDecimal TaxableAmount
		{
			get { return CalloutCharge.TaxableAmount; }
		}

		public ZDecimal Discount
		{
			get { return CalloutCharge.Discount; }
		}

		public ZDecimal NettAmount
		{
			get { return CalloutCharge.NettAmount; }
		}

		public ZDecimal GSTAmount
		{
			get { return CalloutCharge.GSTAmount; }
		}

		public DocBankAccount BankAccount
		{
			get { return CalloutCharge.JR_AB.IsValid ? DocBankAccount.New(CalloutCharge.Factory, CalloutCharge.JR_AB) : null; }
		}

		public DocChargeCode ChargeCode
		{
			get { return CalloutCharge.JR_AC.IsValid ? DocChargeCode.New(CalloutCharge.Factory, CalloutCharge.JR_AC) : null; }
		}

		public ZDecimal AgentDeclaredCostAmt
		{
			get { return CalloutCharge.JR_AgentDeclaredCostAmt; }
		}

		public ZDecimal AgentDeclaredSellAmt
		{
			get { return CalloutCharge.JR_AgentDeclaredSellAmt; }
		}

		public ZDateTime APInvoiceDate
		{
			get { return CalloutCharge.JR_APInvoiceDate; }
		}

		public ZString APInvoiceNum
		{
			get { return CalloutCharge.JR_APInvoiceNum; }
		}

		public ZString ChargeType
		{
			get { return CalloutCharge.JR_ChargeType; }
		}

		public ZString ChequeNo
		{
			get { return CalloutCharge.JR_ChequeNo; }
		}

		public ZBool CostRated
		{
			get { return CalloutCharge.JR_CostRated; }
		}

		public ZGuid CostSplitGroup
		{
			get { return CalloutCharge.JR_E6; }
		}

		public ZString CostSplitMethod
		{
			get
			{
				string result = "";
				JobConsolCost relatedConsolCost = null;
				if (CalloutCharge.JR_E6.IsValid)
				{
					relatedConsolCost = Factory.Load<JobConsolCost>(CalloutCharge.JR_E6);
					if (relatedConsolCost != null)
					{
						result = relatedConsolCost.E6_ApportionmentMethod;
					}
				}
				return result;
			}
		}

		public ZDecimal DeclaredOSCostAmt
		{
			get { return CalloutCharge.JR_DeclaredOSCostAmt; }
		}

		public ZString Desc
		{
			get { return CalloutCharge.JR_Desc; }
		}

		public ZShort DisplaySequence
		{
			get { return CalloutCharge.JR_DisplaySequence; }
		}

		public DocBranch Branch
		{
			get { return CalloutCharge.JR_GB.IsValid ? DocBranch.New(CalloutCharge.Branch, Factory) : null; }
		}

		public ZString InvoiceType
		{
			get { return CalloutCharge.JR_InvoiceType; }
		}

		public ZBool IsIncludedInProfitShare
		{
			get { return CalloutCharge.JR_IsIncludedInProfitShare; }
		}

		public DocForwardingConsol Consol
		{
			get { return CalloutCharge.ParentConsolCost != null ? DocForwardingConsol.New(CalloutCharge.Factory, CalloutCharge.ParentConsolCost.E6_ParentID) : null; }
		}

		public ZDecimal LineCFX
		{
			get { return CalloutCharge.JR_LineCFX; }
		}

		public ZDecimal LocalCostAmt
		{
			get { return CalloutCharge.JR_LocalCostAmt; }
		}

		public ZDecimal LocalSellAmt
		{
			get { return CalloutCharge.JR_LocalSellAmt; }
		}

		public ZDecimal MarginPercentage
		{
			get { return CalloutCharge.JR_MarginPercentage; }
		}

		public DocOrganisation CostAccount
		{
			get { return CalloutCharge.JR_OH_CostAccount.IsValid ? DocOrganisation.New(CalloutCharge.Factory, CalloutCharge.JR_OH_CostAccount) : null; }
		}

		public DocOrganisation SellAccount
		{
			get { return CalloutCharge.JR_OH_SellAccount.IsValid ? DocOrganisation.New(CalloutCharge.Factory, CalloutCharge.JR_OH_SellAccount) : null; }
		}

		public ZDecimal OSCostAmt
		{
			get { return CalloutCharge.JR_OSCostAmt; }
		}

		public ZDecimal OSCostExRate
		{
			get { return CalloutCharge.JR_OSCostExRate; }
		}

		public ZDecimal OSCostWHTAmt
		{
			get { return CalloutCharge.JR_OSCostWHTAmt; }
		}

		public ZDecimal OSSellAmt
		{
			get { return CalloutCharge.JR_OSSellAmt; }
		}

		public ZDecimal OSSellExRate
		{
			get { return CalloutCharge.JR_OSSellExRate; }
		}

		public ZDecimal OSSellGSTAmt
		{
			get { return 0; }
		}

		public ZDecimal OSSellWHTAmt
		{
			get { return CalloutCharge.JR_OSSellWHTAmt; }
		}

		public ZDateTime PaymentDate
		{
			get { return CalloutCharge.JR_PaymentDate; }
		}

		public ZString PaymentType
		{
			get { return CalloutCharge.JR_PaymentType; }
		}

		public ZBool PreventInvoicePrintGrouping
		{
			get { return CalloutCharge.JR_PreventInvoicePrintGrouping; }
		}

		public ZBool SellRated
		{
			get { return CalloutCharge.JR_SellRated; }
		}

		public ZBool IsFirstChargeInCollection
		{
			get { return ParentCollections.Count > 0 && this == ((IList)ParentCollections.First())[0]; }
		}
	}
}
