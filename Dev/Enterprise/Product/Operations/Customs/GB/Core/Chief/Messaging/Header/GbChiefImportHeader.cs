using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.GB.Chief.CusDec;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Chief.Messaging
{
	public class GbChiefImportHeader : GbChiefHeader, IImportHeader
	{
		public const string FreightApportionmentIndicatorPositiveFlag = "1";

		public GbChiefImportHeader(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override ZString JobType
		{
			get { return "I"; }
		}

		public override ZString HMRC_ASG_CODE(CusDecMessageTypeFunction originalOrReplacement)
		{
			int asnCode = 0;

			if (IsICR)
			{ asnCode = 708; }
			else if (IsIFD)
			{ asnCode = 700; }
			else if (IsIFW)
			{ asnCode = 702; }
			else if (IsISD)
			{ asnCode = 710; }
			else if (IsISW)
			{ asnCode = 712; }

			if (originalOrReplacement == CusDecMessageTypeFunction.Original)
			{
				return asnCode.ToString("D3");
			}
			else if (originalOrReplacement == CusDecMessageTypeFunction.Replacement)
			{
				return (asnCode + 1).ToString("D3");
			}
			else if (originalOrReplacement == CusDecMessageTypeFunction.Delete)
			{
				return "760"; // delete
			}

			return ZString.Empty;
		}

		public ZString SCND_DAN
		{
			get { return EntryHeader.Declaration.ZG_VATDeferNumber; }
		}

		public ZString SCND_DAN_PFX
		{
			get { return EntryHeader.Declaration.ZG_VATDeferType; }
		}

		/// <summary>
		/// Works out the first EU country that the shipment hit, and the date thereof. 
		/// </summary>
		public FirstEuArrivalCountryAndPlace FirstEuArrival
		{
			get
			{
				return new FirstEuArrivalCountryAndPlace();
			}
		}

		public IOrganisation NotifyParty
		{
			get
			{
				throw new NotImplementedException("Daniel to do");
			}
		}

		protected override IDocAddress Customer
		{
			get { return ConsigneeDocAddress; }
		}

		#region IImportHeader Members

		public ZDecimal AdjustmentForVATValue => declaration.ZG_VATAdjAmt;

		public ZString AdjustmentForVATValueCurrency => declaration.ZG_RX_NKVATAdj;

		public ZDecimal OSAirTransportAmount => declaration.ZG_OSAirTransportAmount;

		public ZString OSAirTransportLoad
		{
			get { return declaration.JE_IATALoadPort; }
		}

		public ZString FirstDeferredPayment
		{
			get { return declaration.JE_DefermentAccountNumber; }
		}

		public ZString FirstDeferredPaymentPFX
		{
			get { return declaration.JE_PaymentMethod; }
		}

		public ZString SecondDeferredPayment
		{
			get { return declaration.ZG_VATDeferNumber; }
		}

		public ZString SecondDeferredPaymentPFX
		{
			get { return declaration.ZG_VATDeferType; }
		}

		public ZDecimal DiscountAmount => declaration.ZG_DiscAmt;

		public ZString DiscountAmountCurrency => declaration.ZG_RX_NKDisc;

		public ZDecimal DiscountPercentage => declaration.ZG_DiscPerc;

		public ZString FreightApportionmentIndicator => declaration.ZG_ApportionByWeight ? (ZString)"1" : ZString.Empty;

		/*Where all items on the declaration are entered under SPV, this box should be left blank.
		This box is not to be completed when freight charges have been included in the invoice amount (i.e.
		boxes 22 and 42). In this case, the adjustment code (box 45) indicates that:
		– the invoice price is either CIF or post-CIF (codes B, C, D, E, G, H, I, or J; or
		– a manual calculation of Customs duty (code M) is being undertaken.
		If this box is completed CHIEF will include the charges in its calculation of the customs and other values
		abated by any air transport deduction arising from the information declared in Boxes 61 and 62.
		Enter in the first subdivision the code from Appendix C1 for the currency in which the charges are being
		declared. This must be the same currency as was used to declare any air transport costs in Box 62.
		Enter in the second subdivision the total amount of freight charges to no more than two decimal places.
		The charges may be entered in the currency shown on the commercial transport documents but the
		appropriate currency code from the list in Appendix C1 must then be shown in the first subdivision of this
		box.
		For goods imported by surface transport (land or sea) when the freight charge includes the cost of
		transport within the Community (which is not part of the value for customs duty), any deduction
		necessary to arrive at the dutiable element (i.e. the costs incurred for transport outside the Community)
		must be made before the box is completed so that only the dutiable element is declared. Note: For VAT
		the non-dutiable element must be added back into Box 68.		
		 */
		public ZDecimal FreightCharges => declaration.ZG_FrtChgAmt;

		public ZString FreightChargesCurrency => declaration.ZG_RX_NKFrtChg;

		public ZDecimal InsuranceAmount => declaration.ZG_InsAmt;

		public ZString InsuranceCurrency => declaration.ZG_RX_NKIns;

		public ZString OtherChargesDeductionsCurrency => declaration.ZG_RX_NKOthChg;

		public ZDecimal OtherChargesDeductionsValue => declaration.ZG_OthChgAmt;

		public ZDecimal TotalAmountInvoiced
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				bool isMultiCurrency = EntryHeader.IsMultiInvoiceCurrency;
				foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
				{
					if (entryLine.CL_CustomsPostedStatus == Enterprise.Customs.Business.EntryLineStatusList.Codes.Active)
					{
						if (isMultiCurrency)
						{
							result += entryLine.TotalLinePriceInLocalCurrency;
						}
						else
						{
							result += entryLine.TotalLinePrice.Amount;
						}
					}
				}
				return result;
			}
		}

		public ZBool FECCountryOfExport
		{
			get { return EntryHeader.Declaration.JE_FecDSP; }
		}

		#endregion

		protected override GbLine GetNewGbLine(CusEntryLine entryLine)
		{
			return new GbChiefImportLine(this, (Business.Declaration.CusEntryLine)entryLine);
		}
	}
}
