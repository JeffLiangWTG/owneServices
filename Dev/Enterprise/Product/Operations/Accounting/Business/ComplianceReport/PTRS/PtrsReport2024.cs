using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReport2024 : PtrsReportBase
	{
		public PtrsReport2024(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ATR_ReturnType = ReturnType.PTRS;
			ATR_Status = string.Empty;
			ATR_Version = 1;
		}

		#region Report based summaries

		[ResourceStringData("83f1c6a0-fc4c-41b4-9364-069c1329208a", Caption = "Most Common Payment Term")]
		public ZInt MostCommonPaymentTerm { get => mostCommonPaymentTerm.Value; private set => mostCommonPaymentTerm.Value = value; }

		[ResourceStringData("8289f669-2e96-47e1-a46b-9e3303780870", Caption = "Minimum Payment Term")]
		public ZInt PaymentTermMin { get => paymentTermMin.Value; private set => paymentTermMin.Value = value; }

		[ResourceStringData("118f6bd8-3163-4edc-ac28-b3652c23d100", Caption = "Maximum Payment Term")]
		public ZInt PaymentTermMax { get => paymentTermMax.Value; private set => paymentTermMax.Value = value; }

		[ResourceStringData("a7dafb1f-82f2-41e3-8798-ecff5516f055", Caption = "Average Payment Time")]
		[DecimalPlaces(2)]
		public ZDecimal AveragePaymentTime { get => averagePaymentTime.Value; private set => averagePaymentTime.Value = value; }

		[ResourceStringData("5a4b3172-73bf-4725-94cb-645e4421591b", Caption = "Median Payment Time")]
		[DecimalPlaces(2)]
		public ZDecimal MedianPaymentTime { get => medianPaymentTime.Value; private set => medianPaymentTime.Value = value; }

		[ResourceStringData("08fd77f6-5f80-42d9-85b5-163d14092b46", Caption = "80th Percentile Payment Time")]
		public ZInt PaymentTimeOf80thPercentile { get => paymentTimeOf80thPercentile.Value; private set => paymentTimeOf80thPercentile.Value = value; }

		[ResourceStringData("aafddec2-2726-40af-8d71-d90987a73e65", Caption = "95th Percentile Payment Time")]
		public ZInt PaymentTimeOf95thPercentile { get => paymentTimeOf95thPercentile.Value; private set => paymentTimeOf95thPercentile.Value = value; }

		[ResourceStringData("0b63bde4-972a-4600-ac77-35da90197854", Caption = "% Paid within Term")]
		[DecimalPlaces(2)]
		public ZDecimal PercentagePaidWithinTerm { get => percentagePaidWithinTerm.Value; private set => percentagePaidWithinTerm.Value = value; }

		[ResourceStringData("56e99e81-ff62-4c07-9977-e9d0892b55e9", Caption = "% Paid within 30 days")]
		[DecimalPlaces(2)]
		public ZDecimal PercentagePaidWithin30days { get => percentagePaidWithin30days.Value; private set => percentagePaidWithin30days.Value = value; }

		[ResourceStringData("bebd2d8f-82ce-4bf2-96d8-deac3ee64a9f", Caption = "% Paid between 31 and 60 days")]
		[DecimalPlaces(2)]
		public ZDecimal PercentagePaidBetween31And60Days { get => percentagePaidBetween31And60Days.Value; private set => percentagePaidBetween31And60Days.Value = value; }

		[ResourceStringData("462b9481-314f-459c-843e-99779e238e2d", Caption = "% Paid after 60 days")]
		[DecimalPlaces(2)]
		public ZDecimal PercentagePaidAfter60Days { get => percentagePaidAfter60Days.Value; private set => percentagePaidAfter60Days.Value = value; }

		[ResourceStringData("5bf0f975-49f1-41f2-b42f-f965ab4a4ad5", Caption = "Small Business Payment %")]
		[DecimalPlaces(2)]
		public ZDecimal SmallBusinessPaymentPercentage { get => smallBusinessPaymentPercentage.Value; private set => smallBusinessPaymentPercentage.Value = value; }

		#endregion

		#region Columns to store summaries

		PtrsReportNumberColumn mostCommonPaymentTerm => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(MostCommonPaymentTerm));

		PtrsReportNumberColumn paymentTermMin => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(PaymentTermMin));

		PtrsReportNumberColumn paymentTermMax => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(PaymentTermMax));

		PtrsReportAmountColumn averagePaymentTime => this.GetOrCreateColumn<PtrsReportAmountColumn>(nameof(AveragePaymentTime));

		PtrsReportAmountColumn medianPaymentTime => this.GetOrCreateColumn<PtrsReportAmountColumn>(nameof(MedianPaymentTime));

		PtrsReportNumberColumn paymentTimeOf80thPercentile => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(PaymentTimeOf80thPercentile));

		PtrsReportNumberColumn paymentTimeOf95thPercentile => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(PaymentTimeOf95thPercentile));

		PtrsReportPercentColumn percentagePaidWithinTerm => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(PercentagePaidWithinTerm), 2);

		PtrsReportPercentColumn percentagePaidWithin30days => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(PercentagePaidWithin30days), 2);

		PtrsReportPercentColumn percentagePaidBetween31And60Days => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(PercentagePaidBetween31And60Days), 2);

		PtrsReportPercentColumn percentagePaidAfter60Days => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(PercentagePaidAfter60Days), 2);

		PtrsReportPercentColumn smallBusinessPaymentPercentage => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(SmallBusinessPaymentPercentage), 2);

		#endregion

		#region Get data

		protected override void GetDataFromComplianceReportLines()
		{
			if (ComplianceReport != null && !IsSubmitted && PtrsAllPaymentsReport != null)
			{
				var helper = Factory.ServiceContainer.GetService<IPtrsReport2024Helper>();
				if (helper != null)
				{
					var allPaymentsData = new PtrsAllPaymentsReport2024Data()
					{
						SmallBusinessPartialPaymentAmount = PtrsAllPaymentsReport.SmallBusinessPartialPaymentAmount,
						SmallBusinessFullPaymentAmount = PtrsAllPaymentsReport.SmallBusinessFullPaymentAmount,
						OthersPartialPaymentAmount = PtrsAllPaymentsReport.OthersPartialPaymentAmount,
						OthersFullPaymentAmount = PtrsAllPaymentsReport.OthersFullPaymentAmount
					};

					var data = helper.CalculatePtrsReport2024Data(ComplianceReport, allPaymentsData);

					MostCommonPaymentTerm = data.MostCommonPaymentTerm;
					PaymentTermMin = data.PaymentTermMin;
					PaymentTermMax = data.PaymentTermMax;

					AveragePaymentTime = data.AveragePaymentTime;
					MedianPaymentTime = data.MedianPaymentTime;

					PaymentTimeOf80thPercentile = data.PaymentTimeOf80thPercentile;
					PaymentTimeOf95thPercentile = data.PaymentTimeOf95thPercentile;

					PercentagePaidWithinTerm = data.PercentagePaidWithinTerm;
					PercentagePaidWithin30days = data.PercentagePaidWithin30days;
					PercentagePaidBetween31And60Days = data.PercentagePaidBetween31And60Days;
					PercentagePaidAfter60Days = data.PercentagePaidAfter60Days;

					SmallBusinessPaymentPercentage = data.SmallBusinessPaymentPercentage;
				}
			}
		}

		#endregion

		#region Related All Payments Report

		public AccComplianceReport AllPaymentsComplianceReport
		{
			get
			{
				if (allPaymentsComplianceReport == null && ComplianceReport != null)
				{
					var query = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, ComplianceReport.ACR_GC_Company);
					query.AddToFilter(AccComplianceReportSchema.ACR_ReportType, ComplianceReportTypes.PaymentTimesAllPayments2024ReportType);
					query.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, ComplianceReport.ACR_DateFrom);
					query.AddToFilter(AccComplianceReportSchema.ACR_DateTo, ComplianceReport.ACR_DateTo);
					allPaymentsComplianceReport = Factory.LoadTop1<AccComplianceReport>(query);
				}
				return allPaymentsComplianceReport;
			}
		}
		AccComplianceReport allPaymentsComplianceReport;

		public PtrsAllPaymentsReport2024 PtrsAllPaymentsReport
		{
			get
			{
				if (ptrsAllPaymentsReport == null && AllPaymentsComplianceReport != null)
				{
					ptrsAllPaymentsReport = Factory.LoadTop1<PtrsAllPaymentsReport2024>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, AllPaymentsComplianceReport.PK));
				}
				return ptrsAllPaymentsReport;
			}
		}
		PtrsAllPaymentsReport2024 ptrsAllPaymentsReport;

		#endregion

	}
}
