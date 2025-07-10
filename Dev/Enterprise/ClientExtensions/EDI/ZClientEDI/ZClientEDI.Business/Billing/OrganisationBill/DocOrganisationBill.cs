using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DocOrganisationBill : DocBaseWrapper
	{
		protected DocOrganisationBill(OrganisationBill organisationBill, BusinessObjectFactory factory, ZGuid wrappedOrganisationPK)
			: base(organisationBill, factory)
		{
			organisation = Factory.Load<EDIOrgHeader>(wrappedOrganisationPK);

			AddGeneralSummarySections(wrappedOrganisationPK);
			if (organisationBill.OrganisationPK == wrappedOrganisationPK)
			{
				AddBillingSummarySections();
			}
		}

		public static DocOrganisationBill New(OrganisationBill organisationBill, BusinessObjectFactory factory, ZGuid wrappedOrganisationPK)
		{
			return (organisationBill != null) ? new DocOrganisationBill(organisationBill, factory, wrappedOrganisationPK) : null;
		}

		OrganisationBill OrganisationBill
		{
			get { return (OrganisationBill)WrappedObject; }
		}

		#region Add General Summary Sections

		void AddGeneralSummarySections(ZGuid wrappedOrganisationPK)
		{
			GeneralSummaryLines.RemoveAndDeleteAll();
			GeneralSummaryLinesWithoutLicenceUnits.RemoveAndDeleteAll();
			for (int i = 0; i < OrganisationBill.SystemBills.Count; ++i)
			{
				var systemBill = OrganisationBill.SystemBills[i];
				bool systemBillHasLicenceUnits = systemBill.HasLicenceUnits;
				var lines = systemBillHasLicenceUnits ? GeneralSummaryLines : GeneralSummaryLinesWithoutLicenceUnits;
				int initialLineCount = lines.Count;
				SummarySection[] summarySections = systemBill.GetGeneralSummarySections(wrappedOrganisationPK);
				if (summarySections.Length > 0)
				{
					foreach (SummarySection summarySection in summarySections)
					{
						summarySection.Header.TotalDescription = systemBillHasLicenceUnits ? ZString.Format("Total ({0}): ", systemBill.CurrencyCode) : ZString.Format("Total ({0})", systemBill.CurrencyCode);
						summarySection.Header.TotalLicenceUnitsDescription = "Total Licence Units: ";
					}
					lines.PopulateFromSummarySections(summarySections);
				}

				ZString sortValue = i.ToString("00#", CultureInfo.InvariantCulture);
				for (int j = initialLineCount; j < lines.Count; ++j)
				{
					lines[j].Header.SortValue = sortValue;
				}
			}
		}

		#endregion

		#region Add Billing Summary Sections

		void AddBillingSummarySections()
		{
			CreateDiscountAndSurchargeSummary();
			CreateDatabaseMinimumFeeSummary();
			CreateGroupSummary();
			CreateDepositSummary();
			CreatePaymentSummary();
			AddPrepaymentSummarySection();
		}

		void CreateDiscountAndSurchargeSummary()
		{
			var discountTypes = BillingConstants.GetDiscountTypeList();
			var surchargeDescription = discountTypes.GetDescriptionFromCode(BillingConstants.DiscountType.Surcharge);

			DiscountSummaryLines.RemoveAndDeleteAll();

			foreach (SystemBill systemBill in OrganisationBill.SystemBills)
			{
				IEnumerable<SummarySection> summarySections = systemBill.GetDiscountSummarySections();
				summarySections = summarySections.Concat(systemBill.GetSurchargeSummarySections());

				foreach (SummarySection section in summarySections)
				{
					section.Header.MainDescription = "Usage Calculation";
					foreach (SummaryLine line in section.Lines)
					{
						if (!line.MainDescription.Contains(surchargeDescription, StringComparison.Ordinal))
						{
							line.MainDescription = systemBill.SystemDescription + " " + line.MainDescription;
						}
					}

					DiscountSummaryLines.PopulateFromSummarySection(section);
				}
			}
		}

		void CreateDatabaseMinimumFeeSummary()
		{
			DatabaseMinimumFeeSummaryLines.RemoveAndDeleteAll();

			if (OrganisationBill.SystemMinimumFeeAmount > 0)
			{
				var minimumFeeSection = new SummarySection(Factory);
				minimumFeeSection.Header.MainDescription = "System Licence";
				minimumFeeSection.Header.AmountDescription = string.Format(CultureInfo.InvariantCulture, "Total ({0})", InvoiceCurrency);
				minimumFeeSection.Header.TotalAmount = OrganisationBill.SystemMinimumFeeAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				minimumFeeSection.Header.TotalDescription = string.Format(CultureInfo.InvariantCulture, "Total ({0})", InvoiceCurrency);

				bool hasMultiplePeriod = OrganisationBill.SystemMinimumFees.GroupBy(x => x.PeriodStart).Skip(1).Any();

				foreach (var minimumFee in OrganisationBill.SystemMinimumFees)
				{
					var db = Factory.Load<LicenceDatabase>(minimumFee.DatabasePk);
					var feeType = db.LD_LicenceType == DatabaseTypes.Codes.Production ? "Minimum Fee Adjustment" : "Non-Production System Fee";

					SummaryLine amountLine = minimumFeeSection.Lines.AddNew();
					amountLine.MainDescription = "Server " + db.LD_ServerCode + " " + feeType;
					if (hasMultiplePeriod)
					{
						amountLine.MainDescription += " " + minimumFee.PeriodStart.ToString("MMM yyyy", CultureInfo.InvariantCulture);
					}
					amountLine.Amount = minimumFee.Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				}

				DatabaseMinimumFeeSummaryLines.PopulateFromSummarySection(minimumFeeSection);
			}
		}

		void CreateGroupSummary()
		{
			GroupSummaryLines.RemoveAndDeleteAll();
			GroupSummaryLinesWithoutLicenceUnits.RemoveAndDeleteAll();
			foreach (SystemBill systemBill in OrganisationBill.SystemBills)
			{
				var lines = systemBill.HasLicenceUnits ? GroupSummaryLines : GroupSummaryLinesWithoutLicenceUnits;
				lines.PopulateFromSummarySections(systemBill.GetGroupSummarySections());
			}

			bool hasMultipleTaxes = GroupSummaryLines.HasMultipleTaxes || GroupSummaryLinesWithoutLicenceUnits.HasMultipleTaxes;
			GroupSummaryLines.ShowTaxCodes(hasMultipleTaxes);
			GroupSummaryLinesWithoutLicenceUnits.ShowTaxCodes(hasMultipleTaxes);
		}

		void CreatePaymentSummary()
		{
			PaymentSummaryLines.RemoveAndDeleteAll();

			SummarySection paymentSection = new SummarySection(Factory);
			paymentSection.Header.MainDescription = "Payment Summary";
			paymentSection.Header.TotalAmount = OrganisationBill.TotalDue.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			paymentSection.Header.TotalDescription = string.Format(CultureInfo.CurrentCulture, "Total ({0})", InvoiceCurrency);

			foreach (SystemBill systemBill in OrganisationBill.SystemBills)
			{
				if (systemBill.Amount != 0)
				{
					SummaryLine amountLine = paymentSection.Lines.AddNew();
					amountLine.MainDescription = systemBill.SystemDescription + " Amount";

					ZDecimal amount = OrganisationBill.AmountInInvoiceCurrency(systemBill.Amount, systemBill.CurrencyCode);
					amountLine.Amount = amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				}

				if (systemBill.DiscountAmount != 0)
				{
					SummaryLine discountLine = paymentSection.Lines.AddNew();
					discountLine.MainDescription = systemBill.SystemDescription + " Discount";

					ZDecimal amount = OrganisationBill.AmountInInvoiceCurrency(systemBill.DiscountAmount, systemBill.CurrencyCode);
					discountLine.Amount = (-amount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				}

				if (systemBill.SurchargeAmount != 0)
				{
					SummaryLine surchargeLine = paymentSection.Lines.AddNew();
					surchargeLine.MainDescription = systemBill.SystemDescription + " Surcharge";

					ZDecimal amount = OrganisationBill.AmountInInvoiceCurrency(systemBill.SurchargeAmount, systemBill.CurrencyCode);
					surchargeLine.Amount = amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				}
			}

			if (OrganisationBill.SystemMinimumFeeAmount > 0)
			{
				SummaryLine minimumFeeLine = paymentSection.Lines.AddNew();
				minimumFeeLine.MainDescription = "System Licence Amount";
				minimumFeeLine.Amount = OrganisationBill.SystemMinimumFeeAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			}

			if (OrganisationBill.DepositDeductedInInvoiceCurrency > 0)
			{
				SummaryLine depositLine = paymentSection.Lines.AddNew();
				depositLine.MainDescription = "Deposit Deducted";
				depositLine.Amount = "-" + OrganisationBill.DepositDeductedInInvoiceCurrency.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			}

			if (OrganisationBill.ProcessingFeeAmount != 0)
			{
				SummaryLine processingFeeLine = paymentSection.Lines.AddNew();
				processingFeeLine.MainDescription = OrganisationBill.ProcessingFeeDescription;
				processingFeeLine.Amount = OrganisationBill.ProcessingFeeAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			}

			PaymentSummaryLines.PopulateFromSummarySection(paymentSection);
		}

		void CreateDepositSummary()
		{
			if (OrganisationBill.DepositDeducted > 0)
			{
				DepositSummaryLines.RemoveAndDeleteAll();

				SummarySection depositSection = new SummarySection(Factory);
				depositSection.Header.MainDescription = "Deposit Summary";

				ZDecimal depositLeft = OrganisationBill.Deposit - OrganisationBill.DepositDeducted;
				depositSection.Header.TotalDescription = "Closing Deposit";
				depositSection.Header.TotalAmount = depositLeft.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

				SummaryLine depositLine = depositSection.Lines.AddNew();
				depositLine.MainDescription = "Opening Deposit";
				depositLine.Amount = OrganisationBill.Deposit.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

				SummaryLine depositDeductedLine = depositSection.Lines.AddNew();
				depositDeductedLine.MainDescription = "Deposit Deducted";
				depositDeductedLine.Amount = "-" + OrganisationBill.DepositDeducted.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

				DepositSummaryLines.PopulateFromSummarySection(depositSection);
			}
		}

		void AddPrepaymentSummarySection()
		{
			var bill = OrganisationBill;
			var prepay = bill.PrepayNext;

			if (prepay.PrepaymentBalanceRequired != 0m || prepay.FuturePrepaymentBalanceRequired != 0m) //invoicing tab -> predetermined balance
			{
				var line = PrepaymentSummaryLines.AddNew();
				line.Currency = bill.InvoiceCurrencyCode;
				line.Column1 = prepay.CurrentPrepaymentBalance.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				line.Column2 = prepay.PrepaymentBalanceRequired.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				line.Column3 = EDIDataRegistry.Instance.RevisedPrepaymentBalanceDescription.Value;
				line.Column4 = prepay.FuturePrepaymentBalanceRequired != 0m ?
						prepay.FuturePrepaymentBalanceRequired.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture) : "";

				var topUpRequired = Math.Max(new[] { prepay.FuturePrepaymentBalanceRequired, prepay.PrepaymentBalanceRequired }.FirstOrDefault(x => x != 0m) - prepay.CurrentPrepaymentBalance, 0m);
				line.Column5 = topUpRequired.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				var totalPaymentRequired = topUpRequired + prepay.CurrentInvoiceTotalAmount;
				line.FinalAmount = totalPaymentRequired.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			}
		}

		#endregion

		#region Summary Lines

		public ZBool IsEmpty
		{
			get
			{
				return GeneralSummaryLines.Count == 0
					&& GeneralSummaryLinesWithoutLicenceUnits.Count == 0
					&& DiscountSummaryLines.Count == 0
					&& GroupSummaryLines.Count == 0
					&& GroupSummaryLinesWithoutLicenceUnits.Count == 0
					&& DepositSummaryLines.Count == 0
					&& PaymentSummaryLines.Count == 0;
			}
		}

		public SummaryLineCollection GeneralSummaryLines
		{
			get { return generalSummaryLines ?? (generalSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection generalSummaryLines;

		public SummaryLineCollection GeneralSummaryLinesWithoutLicenceUnits
		{
			get { return generalSummaryLinesWithoutLicenceUnits ?? (generalSummaryLinesWithoutLicenceUnits = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection generalSummaryLinesWithoutLicenceUnits;

		public SummaryLineCollection DiscountSummaryLines
		{
			get { return discountSummaryLines ?? (discountSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection discountSummaryLines;

		public SummaryLineCollection GroupSummaryLines
		{
			get { return groupSummaryLines ?? (groupSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection groupSummaryLines;

		public SummaryLineCollection GroupSummaryLinesWithoutLicenceUnits
		{
			get { return groupSummaryLinesWithoutLicenceUnits ?? (groupSummaryLinesWithoutLicenceUnits = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection groupSummaryLinesWithoutLicenceUnits;

		public SummaryLineCollection DepositSummaryLines
		{
			get { return depositSummaryLines ?? (depositSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection depositSummaryLines;

		public SummaryLineCollection PaymentSummaryLines
		{
			get { return paymentSummaryLines ?? (paymentSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection paymentSummaryLines;

		public SummaryLineCollection DatabaseMinimumFeeSummaryLines
		{
			get { return databaseMinimumFeeSummaryLines ?? (databaseMinimumFeeSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection databaseMinimumFeeSummaryLines;

		public SummaryLineCollection PrepaymentSummaryLines
		{
			get { return prepaymentSummaryLines ?? (prepaymentSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection prepaymentSummaryLines;

		#endregion

		public void ClearAllSummaryLines()
		{
			DatabaseMinimumFeeSummaryLines.RemoveAndDeleteAll();
			DepositSummaryLines.RemoveAndDeleteAll();
			DiscountSummaryLines.RemoveAndDeleteAll();
			GeneralSummaryLines.RemoveAndDeleteAll();
			GeneralSummaryLinesWithoutLicenceUnits.RemoveAndDeleteAll();
			GroupSummaryLines.RemoveAndDeleteAll();
			GroupSummaryLinesWithoutLicenceUnits.RemoveAndDeleteAll();
			PaymentSummaryLines.RemoveAndDeleteAll();
		}

		#region Properties

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(organisation, Factory); }
		}
		readonly EDIOrgHeader organisation;

		public ZString Period
		{
			get { return OrganisationBill.DateTo.ToString("MMM yyyy", CultureInfo.InvariantCulture); }
		}

		public ZString InvoiceCurrency
		{
			get { return OrganisationBill.InvoiceCurrencyCode; }
		}

		public ZString DepositCurrency
		{
			get { return OrganisationBill.DepositCurrencyCode; }
		}

		#endregion
	}
}

