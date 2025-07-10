using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers
{
	public class CusEntryLineDocumentWrapper : NonPersistentBusinessObject
	{
		public CusEntryLineDocumentWrapper(CusEntryLine entryLine)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		public CusEntryLine EntryLine;

		readonly int decimalPlacesNumber = 2;

		#region EntryLine

		public ZShort EntryLineNo => EntryLine.CL_LineNumber;

		public ZString FormattedTariff => EntryLine.FormattedTariff;

		public ZString GoodsDescription => EntryLine.GoodsDescription;

		public ZDecimal ItemPrice => Utilities.Round(EntryLine.CL_CustomsValue, decimalPlacesNumber);

		#region For Customs Invoice Fields

		public ZString CountryOfOrigin => EntryLine.RandomLine.JI_CountryOfOrigin;

		public ZString PrimaryPreference => EntryLine.RandomLine.JI_PrimaryPreference;

		public ZString Procedure => EntryLine.RandomLine.JI_Procedure;

		public ZString Quota => EntryLine.RandomLine.JI_ConcessionOrder;

		public ZString PreviousEntryNumber => EntryLine.RandomLine.JI_PreviousEntryNumber;

		public ZShort PreviousEntryLineNumber => EntryLine.RandomLine.JI_PreviousEntryLineNumber;

		public ZDecimal CustomsQuantity => EntryLine.CustomsQuantity;

		public ZString CustomsUnitQty => EntryLine.CustomsUnitQty;

		public ZDecimal CustomsSecondQuantity => InvoiceLines.Sum(x => x.JI_CustomsSecondQuantity);

		public ZString CustomsSecondUnitQty => EntryLine.RandomLine.JI_CustomsSecondUnitQty;

		public ZDecimal GrossWeightInKG => InvoiceLines.Sum(x => x.GrossWeightInKG);

		public ZDecimal LinePrice => Utilities.Round(InvoiceLines.Sum(x => x.JI_LinePrice), decimalPlacesNumber);

		public ZString LinePriceCurrency => EntryLine.RandomLine.JI_RX_NKLinePriceCurr;

		public ZDecimal Freight => Utilities.Round(InvoiceLines.Sum(x => x.JI_Calc_FreightInInvoiceCurr), decimalPlacesNumber);

		public ZDecimal Insurance => Utilities.Round(InvoiceLines.Sum(x => x.JI_Calc_InsuranceInInvoiceCurr), decimalPlacesNumber);

		public ZDecimal VATGSTBaseValue => Utilities.Round(InvoiceLines.Sum(x => x.JI_Calc_CIF), decimalPlacesNumber);

		public ZDecimal TotalDuty => Utilities.Round(InvoiceLines.Sum(x => x.JI_Calc_DutyAmountIncludingWHEstimate), decimalPlacesNumber);

		public ZDecimal TotalVATGST => Utilities.Round(InvoiceLines.Sum(x => x.JI_Calc_GSTVATAmountIncludingWHEstimate), decimalPlacesNumber);

		public ZString LocalCurrency => EntryLine.RandomLine.LocalCurrency.RX_Code;

		IEnumerable<JobComInvoiceLine> InvoiceLines => invoiceLines ?? (invoiceLines = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray());
		JobComInvoiceLine[] invoiceLines;

		#endregion

		#endregion

		#region Collections

		public BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> DutyAndFeesCollection => dutyAndFeesCollection ?? (dutyAndFeesCollection = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>(DutyAndFees.Select(x => new DutyFeeInformationDocWrapper(x))));
		BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> dutyAndFeesCollection;

		IEnumerable<CusEntryLineFee> DutyAndFees => dutyAndFees ?? (dutyAndFees = EntryLine.Fees.Cast<CusEntryLineFee>().ToArray());
		CusEntryLineFee[] dutyAndFees;

		public ZDecimal DutyAndFeesAmount => Utilities.Round(DutyAndFees.Sum(x => x.CF_ChargeAmount.Round(decimalPlacesNumber)), decimalPlacesNumber);

		public BusinessObjectCollectionWrapper<SupportingDocumentDocWrapper> SupportingDocuments
		{
			get
			{
				if (fSupportingDocuments == null)
				{
					var documents = InvoiceLines.Select(x => x.SupportingDocuments.Cast<SupportingDocument>().Concat(x.InvoiceHeader.SupportingDocuments.Cast<SupportingDocument>()));
					fSupportingDocuments = new BusinessObjectCollectionWrapper<SupportingDocumentDocWrapper>(documents.SelectMany(x => x).Select(x => new SupportingDocumentDocWrapper(x)));
					fSupportingDocuments.Distinct();
				}
				return fSupportingDocuments;
			}
		}
		BusinessObjectCollectionWrapper<SupportingDocumentDocWrapper> fSupportingDocuments;

		#endregion
	}
}
