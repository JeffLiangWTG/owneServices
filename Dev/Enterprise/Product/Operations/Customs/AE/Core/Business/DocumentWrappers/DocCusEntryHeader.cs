using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

public class DocCusEntryHeader : DocBaseCusEntryHeader
{
	DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		: base(cusEntryHeader, factoryToWrap)
	{
	}

	public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
	{
		if (cusEntryHeader == null)
		{
			return null;
		}
		else
		{
			return new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
		}
	}

	#region Wrapped BizObj

	public DocDeclaration Declaration
	{
		get { return DocDeclaration.New(CusEntryHeader.Declaration, Factory); }
	}

	#endregion

	#region ZDecimal Fields

		protected override ZDecimal EntryFeeCore
		{
			get
			{
				return CusEntryHeader.Charges.GetAmount(Registry.EntryChargeTypeList.Codes.RegistrationFee);
			}
		}

		public ZDecimal DutyAmount
		{
			get
			{
				return CusEntryHeader.Charges.GetAmount(Registry.EntryChargeTypeList.Codes.DutyAmount);
			}
		}

	#endregion

	#region ZDateTime fields

	public ZDateTime EntryDate
	{
		get
		{
			ZDateTime result = ZDateTime.Empty;
			int step = 0;
			foreach (EDIMessage message in CusEntryHeader.Messages)
			{
				if (step == 0
					&& message.EM_ReceiveTransmit == EDIMessage.Direction.Receive
					&& message.EM_MessageSubType == "C")
				{
					result = message.EM_SystemCreateTimeUtc;
					step = 1;
				}

				if (result < message.EM_SystemCreateTimeUtc
					&& message.EM_ReceiveTransmit == EDIMessage.Direction.Receive
					&& message.EM_MessageSubType == "C")
				{
					result = message.EM_SystemCreateTimeUtc;
				}
			}
			return result;
		}
	}

	#endregion

	#region ZString Fields

	JobComInvoiceHeader FirstInvoiceHeader
	{
		get
		{
			if (fFirstInvoiceHeader == null && CusEntryHeader.InvoiceHeaders.Length > 0)
			{
				fFirstInvoiceHeader = CusEntryHeader.InvoiceHeaders[0];
			}
			return fFirstInvoiceHeader;
		}
	}
	JobComInvoiceHeader fFirstInvoiceHeader;

	public ZString Remarks1
	{
		get
		{
			ZString result = ZString.Empty;
			if (FirstInvoiceHeader != null)
			{
				ZDecimal termAmount = ZDecimal.Zero;
				ZString incoTerm = FirstInvoiceHeader.JZ_IncoTerm;
				if (incoTerm == "CIF" || incoTerm == "CFR")
				{
					termAmount = ZArchitecture.Core.Utilities.Round(FirstInvoiceHeader.JZ_Calc_CIFAmount, FirstInvoiceHeader.CalcCIFCurrency.Decimals);
				}
				else if (incoTerm == "FOB")
				{
					termAmount = ZArchitecture.Core.Utilities.Round(FirstInvoiceHeader.JZ_Calc_FOBAmount, FirstInvoiceHeader.CalcFOBCurrency.Decimals);
				}

				result = (termAmount != ZDecimal.Zero) ? new ZString(incoTerm + " " + termAmount + " " + FirstInvoiceHeader.CalcFOBCurrency.RX_Code) : ZString.Empty;
			}
			return result;
		}
	}

	public ZString Remarks2
	{
		get
		{
			ZString result = ZString.Empty;
			if (FirstInvoiceHeader != null && FirstInvoiceHeader.JZ_IncoTerm == "FOB")
			{
				ZDecimal freightAmount = ZArchitecture.Core.Utilities.Round((FirstInvoiceHeader.JZ_Calc_CIFAmount - FirstInvoiceHeader.JZ_Calc_FOBAmount), FirstInvoiceHeader.CalcCIFCurrency.Decimals);
				ZString currency = FirstInvoiceHeader.CalcFOBCurrency == null ? ZString.Empty : FirstInvoiceHeader.CalcFOBCurrency.RX_Code;
				result = "FRT " + freightAmount + " " + currency;
			}
			return result;
		}
	}

	#endregion

	#region Collections

	public DocCusEntryLineCollection EntryLines
	{
		get
		{
			if (fEntryLines == null)
			{
				fEntryLines = new DocCusEntryLineCollection(CusEntryHeader.MergedLines, Factory);
				fEntryLines.Sort("LineNumber", System.ComponentModel.ListSortDirection.Ascending);
			}
			return fEntryLines;
		}
	}
	DocCusEntryLineCollection fEntryLines;

	#endregion

	#region Implementation

	CusEntryHeader CusEntryHeader
	{
		get { return (CusEntryHeader)WrappedObject; }
	}

	#endregion

}
