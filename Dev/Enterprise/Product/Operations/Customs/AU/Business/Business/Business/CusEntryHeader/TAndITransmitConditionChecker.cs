using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TAndITransmitConditionChecker
	{
		public TAndITransmitConditionChecker(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
			oFT = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true);
			oNS = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasInsurance, false, true);
			nonDutiableFIFT = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ForeignInlandFreight, false, true);
			nonDutiableOTH = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, true);
		}

		public bool ShouldTransmitTAndIForHeader
		{
			get
			{
				CalculateShouldTransmitTAndI();
				return fShouldTransmitTAndIForHeader || fShouldTransmitTAndIForLine;
			}
		}
		internal bool fShouldTransmitTAndIForHeader;

		public bool ShouldTransmitTAndIForLine
		{
			get
			{
				CalculateShouldTransmitTAndI();
				return fShouldTransmitTAndIForLine;
			}
		}
		internal bool fShouldTransmitTAndIForLine;

		public void RefreshCalculation()
		{
			isCalculationUpToDate = false;

			fShouldTransmitTAndIForHeader = false;
			fShouldTransmitTAndIForLine = false;
			fDoesInvoiceHeaderTILV = false;

			DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH = false;
			DoesNonDutiableFIFTOrOTHExistInGroupOrInvoice = false;
			DoChargesExistDistributedByOtherThanValue = false;
			DoLinesHaveTILVInAddInfo = false;
			DoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH = false;
			DoOFTOrONSOrNonDutiableFIFTOrOTHExistInMultipleGroupInvoices = false;
			DoChargesExistDistributedByOtherThanValue = false;
		}
		internal bool isCalculationUpToDate;

#if DEBUG
		public bool IsCalculationUpToDateExposedForTesting
		{
			get { return isCalculationUpToDate; }
		}
#endif

		#region Implementation

		internal void CalculateShouldTransmitTAndI()
		{
			if (!isCalculationUpToDate)
			{
				var declaration = entryHeader.Declaration;
				if (declaration.AddInfo.ZA_IsManualTILV_Hidden || declaration.IsConsolidated)
				{
					fShouldTransmitTAndIForHeader = true;
					fShouldTransmitTAndIForLine = true;
				}
				else
				{
					fShouldTransmitTAndIForLine = entryHeader.AddInfo.ZA_NegativeCVAdjusted_Hidden == "Y";

					JobComInvoiceHeader[] invoices = entryHeader.InvoiceHeaders;
					foreach (JobComInvoiceHeader invoice in invoices)
					{
						DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH |=
							(
								invoices.Length > 1 && invoice.Charges.HasChargeOfThisTypeAndAmountAndCurrency(oFT, oNS, nonDutiableFIFT, nonDutiableOTH)
							);

						if (!fShouldTransmitTAndIForHeader)
						{
							DoesNonDutiableFIFTOrOTHExistInGroupOrInvoice |=
								(
									invoice.Charges.HasChargeOfThisTypeAndAmountAndCurrency(nonDutiableFIFT, nonDutiableOTH) ||
									invoice.GroupCharges.HasChargeOfThisTypeAndAmountAndCurrency(nonDutiableFIFT, nonDutiableOTH)
								);
						}

						if (!fShouldTransmitTAndIForHeader)
						{
							UpdateDoesInvoiceHeaderTILV(fDoesInvoiceHeaderTILV | !invoice.AddInfo.ZA_TILV.IsEmpty);
						}

						if (!fShouldTransmitTAndIForLine)
						{
							DoChargesExistDistributedByOtherThanValue |=
								invoice.Charges.HasChargesDistributedByOtherThanValue(oFT, oNS, nonDutiableFIFT, nonDutiableOTH);
						}

						if (!fShouldTransmitTAndIForLine)
						{
							foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
							{
								DoLinesHaveTILVInAddInfo |= !invoiceLine.AddInfo.ZA_TILV.IsEmpty;

								if (!fShouldTransmitTAndIForLine)
								{
									DoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH |= invoiceLine.Charges.HasChargeOfThisTypeAndAmountAndCurrency(oFT, oNS, nonDutiableFIFT, nonDutiableOTH);
								}
								if (fShouldTransmitTAndIForLine)
								{
									break;
								}
							}
						}

						if (fShouldTransmitTAndIForHeader && fShouldTransmitTAndIForLine)
						{
							break;
						}
					}

					if (!fShouldTransmitTAndIForLine)
					{
						List<JobComInvoiceGroupHeader> subGroupInvoicesWithOFTOrONSOrNonDutiableFIFT = new List<JobComInvoiceGroupHeader>();

						foreach (JobComInvoiceGroupHeader groupInvoice in entryHeader.GroupInvoices)
						{
							if (invoices.Length > 1 &&
								groupInvoice.Charges.HasChargeOfThisTypeAndAmountAndCurrency(oFT, oNS, nonDutiableFIFT, nonDutiableOTH) &&
								!subGroupInvoicesWithOFTOrONSOrNonDutiableFIFT.Contains(groupInvoice) &&
								groupInvoice.PK != groupInvoice.JobDeclaration.JobComInvoiceGroupHeaders[0].PK)
							{
								subGroupInvoicesWithOFTOrONSOrNonDutiableFIFT.Add(groupInvoice);

								if (subGroupInvoicesWithOFTOrONSOrNonDutiableFIFT.Count > 0)
								{
									DoOFTOrONSOrNonDutiableFIFTOrOTHExistInMultipleGroupInvoices = true;
									break;
								}
							}

							if (!fShouldTransmitTAndIForLine)
							{
								DoChargesExistDistributedByOtherThanValue |=
									groupInvoice.Charges.HasChargesDistributedByOtherThanValue(oFT, oNS, nonDutiableFIFT, nonDutiableOTH);
							}
						}
					}
				}

				isCalculationUpToDate = true;
			}
		}

		internal bool DoesNonDutiableFIFTOrOTHExistInGroupOrInvoice
		{
			get { return fDoesNonDutiableFIFTOrOTHExistInGroupOrInvoice; }
			set
			{
				fDoesNonDutiableFIFTOrOTHExistInGroupOrInvoice = value;
				fShouldTransmitTAndIForHeader |= value;
			}
		}
		bool fDoesNonDutiableFIFTOrOTHExistInGroupOrInvoice;

		void UpdateDoesInvoiceHeaderTILV(bool value)
		{
			fDoesInvoiceHeaderTILV = value;
			fShouldTransmitTAndIForHeader |= value;

			if (value && entryHeader.InvoiceHeaders.Length > 1)
			{
				fShouldTransmitTAndIForLine = true;
			}
		}
		bool fDoesInvoiceHeaderTILV;

		internal bool DoOFTOrONSOrNonDutiableFIFTOrOTHExistInMultipleGroupInvoices
		{
			get { return fDoOFTOrONSOrNonDutiableFIFTOrOTHExistInMultipleGroupInvoices; }
			set
			{
				fDoOFTOrONSOrNonDutiableFIFTOrOTHExistInMultipleGroupInvoices = value;
				fShouldTransmitTAndIForLine |= value;
			}
		}
		bool fDoOFTOrONSOrNonDutiableFIFTOrOTHExistInMultipleGroupInvoices;

		internal bool DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH
		{
			get { return fDoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH; }
			set
			{
				fDoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH = value;
				fShouldTransmitTAndIForLine |= value;
			}
		}
		bool fDoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH;

		internal bool DoLinesHaveTILVInAddInfo
		{
			get { return fDoLinesHaveTILVInAddInfo; }
			set
			{
				fDoLinesHaveTILVInAddInfo = value;
				fShouldTransmitTAndIForLine |= value;
			}
		}
		bool fDoLinesHaveTILVInAddInfo;

		internal bool DoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH
		{
			get { return fDoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH; }
			set
			{
				fDoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH = value;
				fShouldTransmitTAndIForLine |= value;
			}
		}
		bool fDoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH;

		internal bool DoChargesExistDistributedByOtherThanValue
		{
			get { return fDoChargesExistDistributedByOtherThanValue; }
			set
			{
				fDoChargesExistDistributedByOtherThanValue = value;
				fShouldTransmitTAndIForLine |= value;
			}
		}
		bool fDoChargesExistDistributedByOtherThanValue;

		readonly CusEntryHeader entryHeader;
		readonly ChargeCodeChargeKey oFT;
		readonly ChargeCodeChargeKey oNS;
		readonly ChargeCodeChargeKey nonDutiableFIFT;
		readonly ChargeCodeChargeKey nonDutiableOTH;

		#endregion

		#region Test
#if DEBUG

		public class TestDataSetUp
		{
			public JobDeclaration Declaration;
			public JobComInvoiceGroupHeader TopGroup;
			public JobComInvoiceGroupHeader SubGroup1;
			public JobComInvoiceGroupHeader SubGroup2;
			public CusEntryHeader EntryHeader;
			public CusEntryHeader EntryHeader2;
			public CusEntryLine EntryLine;
			public CusEntryLine EntryLine2;

			public JobComInvoiceHeader Invoice;
			public JobComInvoiceHeader Invoice2;
			public JobComInvoiceLine InvoiceLine;
			public JobComInvoiceLine InvoiceLine2;

			public void SetUpSingleEntryForSingleInvoice(BusinessObjectFactory factory)
			{
				Nullify();
				Declaration = factory.New<JobDeclaration>();
				Declaration.JE_ApplicationCode = "CMR";
				Declaration.JE_MessageType = "IMP";
				TopGroup = Declaration.JobComInvoiceGroupHeaders[0];
				EntryHeader = Declaration.CustomsEntryHeaders.AddNew();
				EntryLine = EntryHeader.MergedLines.AddNew();
				EntryLine.CL_LineNumber = (short)1;
				Invoice = Declaration.Invoices.AddNew();
				Invoice.JZ_InvoiceAmount = 10000m;
				Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				Invoice.JZ_IncoTerm = "FOB";
				InvoiceLine = Invoice.JobComInvoiceLines.AddNew();
				InvoiceLine.JI_LinePrice = 10000m;
				InvoiceLine.JI_CL = EntryLine.PK;
			}

			public void SetUpSingleEntryForTwoInvoices(BusinessObjectFactory factory)
			{
				Nullify();
				Declaration = factory.New<JobDeclaration>();
				Declaration.JE_ApplicationCode = "CMR";
				Declaration.JE_MessageType = "IMP";
				TopGroup = Declaration.JobComInvoiceGroupHeaders[0];
				EntryHeader = Declaration.CustomsEntryHeaders.AddNew();
				EntryLine = EntryHeader.MergedLines.AddNew();
				EntryLine.CL_LineNumber = (short)1;
				EntryLine2 = EntryHeader.MergedLines.AddNew();
				EntryLine2.CL_LineNumber = (short)2;

				Invoice = Declaration.Invoices.AddNew();
				Invoice.JZ_InvoiceAmount = 10000m;
				Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				Invoice.JZ_IncoTerm = "FOB";
				InvoiceLine = Invoice.JobComInvoiceLines.AddNew();
				InvoiceLine.JI_LinePrice = 10000m;
				InvoiceLine.JI_CL = EntryLine.PK;

				Invoice2 = Declaration.Invoices.AddNew();
				Invoice2.JZ_InvoiceAmount = 10000m;
				Invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				Invoice2.JZ_IncoTerm = "FOB";
				InvoiceLine2 = Invoice2.JobComInvoiceLines.AddNew();
				InvoiceLine2.JI_LinePrice = 10000m;
				InvoiceLine2.JI_CL = EntryLine2.PK;
			}

			public void SetUpTwoEntriesForTwoInvoices(BusinessObjectFactory factory)
			{
				Nullify();
				Declaration = factory.New<JobDeclaration>();
				Declaration.JE_ApplicationCode = "CMR";
				Declaration.JE_MessageType = "IMP";
				TopGroup = Declaration.JobComInvoiceGroupHeaders[0];
				EntryHeader = Declaration.CustomsEntryHeaders.AddNew();
				EntryLine = EntryHeader.MergedLines.AddNew();
				EntryLine.CL_LineNumber = (short)1;

				EntryHeader2 = Declaration.CustomsEntryHeaders.AddNew();
				EntryLine2 = EntryHeader2.MergedLines.AddNew();
				EntryLine2.CL_LineNumber = (short)1;

				Invoice = Declaration.Invoices.AddNew();
				Invoice.JZ_InvoiceAmount = 10000m;
				Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				Invoice.JZ_IncoTerm = "FOB";
				InvoiceLine = Invoice.JobComInvoiceLines.AddNew();
				InvoiceLine.JI_LinePrice = 10000m;
				InvoiceLine.JI_CL = EntryLine.PK;

				Invoice2 = Declaration.Invoices.AddNew();
				Invoice2.JZ_InvoiceAmount = 10000m;
				Invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				Invoice2.JZ_IncoTerm = "FOB";
				InvoiceLine2 = Invoice2.JobComInvoiceLines.AddNew();
				InvoiceLine2.JI_LinePrice = 10000m;
				InvoiceLine2.JI_CL = EntryLine2.PK;
			}

			public void SetUpSingleEntryWithTwoInvoicesUnderTwoGroupInvoices(BusinessObjectFactory factory)
			{
				Nullify();
				Declaration = factory.New<JobDeclaration>();
				Declaration.JE_ApplicationCode = "CMR";
				Declaration.JE_MessageType = "IMP";
				TopGroup = Declaration.JobComInvoiceGroupHeaders[0];
				EntryHeader = Declaration.CustomsEntryHeaders.AddNew();
				EntryLine = EntryHeader.MergedLines.AddNew();
				EntryLine.CL_LineNumber = (short)1;
				EntryLine2 = EntryHeader.MergedLines.AddNew();
				EntryLine2.CL_LineNumber = (short)2;

				TopGroup = Declaration.JobComInvoiceGroupHeaders[0];
				SubGroup1 = TopGroup.JobComInvoiceGroupHeaders.AddNew();
				SubGroup2 = TopGroup.JobComInvoiceGroupHeaders.AddNew();

				Invoice = SubGroup1.JobComInvoiceHeaders.AddNew();
				Invoice.JZ_InvoiceAmount = 10000m;
				Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				Invoice.JZ_IncoTerm = "FOB";
				InvoiceLine = Invoice.JobComInvoiceLines.AddNew();
				InvoiceLine.JI_LinePrice = 10000m;
				InvoiceLine.JI_CL = EntryLine.PK;

				Invoice2 = SubGroup2.JobComInvoiceHeaders.AddNew();
				Invoice2.JZ_InvoiceAmount = 10000m;
				Invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				Invoice2.JZ_IncoTerm = "FOB";
				InvoiceLine2 = Invoice2.JobComInvoiceLines.AddNew();
				InvoiceLine2.JI_LinePrice = 10000m;
				InvoiceLine2.JI_CL = EntryLine2.PK;
			}

			void Nullify()
			{
				Declaration = null;
				EntryHeader = null;
				EntryHeader2 = null;
				EntryLine = null;
				EntryLine2 = null;
				Invoice = null;
				Invoice2 = null;
				InvoiceLine = null;
				InvoiceLine2 = null;
			}
		}
#endif
		#endregion
	}
}
