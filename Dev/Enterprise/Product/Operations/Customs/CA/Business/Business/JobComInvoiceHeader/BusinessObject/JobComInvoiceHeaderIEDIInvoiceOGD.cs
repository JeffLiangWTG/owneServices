using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	partial class JobComInvoiceHeader : IEDIInvoiceOGD
	{
		#region IEDIInvoiceOGD Members

		IDocAddress IEDIInvoiceOGD.Manufacturer => ManufacturerAddress;

		#endregion

		#region IEDIInvoiceAQ Members

		ZDate IEDIInvoiceAQ.InvoiceDate
		{
			get { return JZ_InvoiceDate.Date; }
		}

		ZDecimal IEDIInvoiceAQ.InvoiceAmount
		{
			get { return JZ_InvoiceAmount; }
		}

		ZString IEDIInvoiceAQ.InvoiceCurrency
		{
			get { return JZ_RX_NKInvoice_Currency; }
		}

		ZDecimal IEDIInvoiceAQ.IncludedOFTAndONS
		{
			get { return GetAmountInCanadianDollars(IncludedOverseasFreight, true) + GetAmountInCanadianDollars(IncludedOverseasInsurance, true); }
		}

		ZDecimal IEDIInvoiceAQ.IncludedConstruction
		{
			get { return GetAmountInCanadianDollars(IncludedConstruction, true); }
		}

		ZDecimal IEDIInvoiceAQ.IncludedPacking
		{
			get { return GetAmountInCanadianDollars(IncludedPackingCosts, true); }
		}

		ZDecimal IEDIInvoiceAQ.ExcludedOFTAndONS
		{
			get { return GetAmountInCanadianDollars(ExcludedOverseasFreight, true) + GetAmountInCanadianDollars(ExcludedOverseasInsurance, true); }
		}

		ZDecimal IEDIInvoiceAQ.ExcludedCommission
		{
			get { return GetAmountInCanadianDollars(ExcludedCommission, true); }
		}

		ZDecimal IEDIInvoiceAQ.ExcludedPacking
		{
			get { return GetAmountInCanadianDollars(ExcludedPackingCosts, true); }
		}

		ZString IEDIInvoiceAQ.OtherReference
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				builder.Append(CA_OtherReference);
				if (!CA_LVSCarrier.IsEmpty)
				{
					builder.Append(Res.GetString("34fd3cf6-dac4-4bac-8ea9-9a43dc519e30", "Carrier: {0} Invoice Date: {1}", CA_LVSCarrier, JZ_InvoiceDate.IsEmpty ? "" : JZ_InvoiceDate.ToString("dd MMM yyyy", CultureInfo.CurrentCulture)));
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		IDocAddress IEDIInvoiceAQ.Shipper
		{
			get
			{
				if (IsImport)
				{
					var supplierPickupDeliveryAddress = SupplierPickupDeliveryAddress;
					return supplierPickupDeliveryAddress.E2_AddressOverride || supplierPickupDeliveryAddress.E2_OA_Address.IsValid && supplierPickupDeliveryAddress.E2_OA_Address != Vendor.E2_OA_Address
						? supplierPickupDeliveryAddress : null;
				}
				else
				{
					return SupplierPickupDeliveryAddress;
				}
			}
		}

		ZString IEDIInvoiceAQ.DepartmentRuling
		{
			get { return CA_DepartmentRuling; }
		}

		ZString IEDIInvoiceAQ.LastPortName
		{
			get
			{
				var lastPort = !CA_RL_NKLastPort.IsEmpty ? LastPort : null;
				if (lastPort == null && JobDeclaration != null && !JobDeclaration.JE_RL_NKPortOfLoading.IsEmpty)
				{
					lastPort = JobDeclaration.PortOfLoading;
				}
				return lastPort != null ? lastPort.RL_PortName.Left(25) : ZString.Empty;
			}
		}

		ZDate IEDIInvoiceAQ.LastPortDate
		{
			get { return JZ_ValuationDateOverride.IsEmpty && JobDeclaration != null ? JobDeclaration.JE_ExportDate.Date : JZ_ValuationDateOverride.Date; }
		}

		ZString IEDIInvoiceAQ.TranshipmentCountry
		{
			get { return CA_RN_NKTranshipment; }
		}

		ZString IEDIInvoiceAQ.ConditionsOfSale
		{
			get { return CA_ConditionsOfSale; }
		}

		ZString IEDIInvoiceAQ.TermsOfPayment
		{
			get { return CA_TermsOfPayment; }
		}

		ZBool IEDIInvoiceAQ.ServicesInd
		{
			get { return CA_ServicesInd; }
		}

		ZBool IEDIInvoiceAQ.RoyaltyInd
		{
			get { return CA_RoyaltyInd; }
		}

		#endregion

		#region IEDIInvoiceMin Members

		ZString IEDIInvoiceMin.InvoiceNumber
		{
			get { return JZ_InvoiceNumber; }
		}

		IDocAddress IEDIInvoiceMin.Vendor
		{
			get { return Vendor; }
		}

		IDocAddress Vendor
		{
			get
			{
				if (IsImport)
				{
					var supplierDocumentaryAddress = SupplierDocumentaryAddress;
					return supplierDocumentaryAddress.E2_AddressOverride || supplierDocumentaryAddress.E2_OA_Address.IsValid ? supplierDocumentaryAddress : JobDeclaration.SupplierDocumentaryAddress;
				}
				else
				{
					return !JZ_OH_Supplier.IsEmpty ? Supplier.MainAddress : (!JobDeclaration.JE_OH_Supplier.IsEmpty ? JobDeclaration.Supplier.MainAddress : null);
				}
			}
		}

		IDocAddress IEDIInvoiceMin.Purchaser => BuyerDocumentaryAddress;

		IDocAddress IEDIInvoiceMin.Consignee
		{
			get
			{
				var importer = JobDeclaration.ImporterOfRecordAddress.HasRealOrganisation ? JobDeclaration.ImporterOfRecordAddress : JobDeclaration.ImporterDocumentaryAddress;
				var consignee = FinalConsigneeAddress;
				return importer.E2_OA_Address == consignee.E2_OA_Address ? null : consignee;
			}
		}

		IDocAddress IEDIInvoiceMin.Exporter
		{
			get { return Exporter; }
		}

		IDocAddress Exporter => ExporterDocumentaryAddress;

		ZString IEDIInvoiceMin.HeaderOrigin
		{
			get { return CountryOfOrigin; }
		}

		ZString IEDIInvoiceMin.CommonCountryOfOrigin
		{
			get { return JZ_RN_NKDefaultOrigin.IsEmpty || InvoiceLines.Cast<JobComInvoiceLine>().Any(l => !l.JI_CountryOfOrigin.IsEmpty && l.Origin != CountryOfOrigin) ? (ZString)"VAR" : CountryOfOrigin; }
		}

		ZString IEDIInvoiceMin.CommonCountryOfExport
		{
			get { return CommonCountryOfExport; }
		}

		ZString CommonCountryOfExport
		{
			get { return CA_RN_NKExport == Constants.CountryCodes.UnitedStates ? new ZString("U" + CA_USStateOfExport) : CA_RN_NKExport; }
		}

		IEnumerable<IEDIInvoiceLineOGD> IEDIInvoiceMin.InvoiceLines
		{
			get
			{
				var entryHeader = JobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
				((AllCusEntryLineCollection)entryHeader.AllEntryLines).Sort((Comparison<CusEntryLine>)CompareEntryLine);
				var lineNumber = 0;
				var lastPage = 0;
				foreach (var entryLine in entryHeader.AllEntryLines)
				{
					if (entryLine.RandomLine.InvoiceHeader == this)
					{
						if (lastPage != entryLine.RandomLine.CA_PageNumber)
						{
							lastPage = entryLine.RandomLine.CA_PageNumber;
							lineNumber = 1;
						}

						entryLine.LineNumber = lineNumber++;
						yield return entryLine;
					}
				}
			}
		}

		#endregion

		#region Implementation

		int CompareEntryLine(CusEntryLine line1, CusEntryLine line2)
		{
			var result = line1.RandomLine.CA_PageNumber.CompareTo(line2.RandomLine.CA_PageNumber);
			if (result == 0)
			{
				result = line1.RandomLine.JI_LineNo.CompareTo(line2.RandomLine.JI_LineNo);
			}
			return result;
		}

		internal ZDecimal GetAmountInCanadianDollars(Money money, bool exact)
		{
			ZDecimal result = 0m;
			if (money != null && money.Currency != null)
			{
				if (money.Currency.Code == Constants.CurrencyCodes.Canada)
				{
					result = money.Amount;
				}
				else
				{
					result = exact ? CurrencyConverter.ConvertExact(money, CanadianDollars).Amount
								: CurrencyConverter.ConvertRounded(money, CanadianDollars).Amount;
				}
			}
			return result;
		}

		ICurrency CanadianDollars
		{
			get
			{
				return canadianDollars ??
					   (canadianDollars = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.Canada));
			}
		}

		ICurrency canadianDollars;

		#endregion
	}
}
