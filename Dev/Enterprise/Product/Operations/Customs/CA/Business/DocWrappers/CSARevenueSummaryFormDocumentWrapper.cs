using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Customs.CA.Business
{
	public class CSARevenueSummaryFormDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		public CSARevenueSummaryFormDocumentWrapper(CusStatementHeader rsf)
		{
			if (!rsf.IsCSARSF)
			{
				throw new InvalidOperationException("Statement Header should be Revenue Summary Form type.");
			}
			this.cSARevenueSummaryForm = rsf;
			GenerateData();
		}

		readonly CusStatementHeader cSARevenueSummaryForm;

		public void GenerateData()
		{
			GenerateHeader();
			GenerateDebitAmounts1stPage();
			GenerateCreditAmounts1stPage();
			GenerateInterimAmounts1stPage();
			GenerateCustomsAssessments();
			GenerateAssessmentTotals();
		}

		#region Header

		public ZString BusinessNumber { get; private set; }
		public ZString CSAImporterName { get; private set; }
		public ZString RSFMonth { get; private set; }
		public ZString PeriodStartDate { get; private set; }
		public ZString PeriodEndDate { get; private set; }
		public ZDecimal VFDOfCurrentMonthTransactions { get; private set; }
		public ZString FilingID { get; private set; }

		void GenerateHeader()
		{
			BusinessNumber = cSARevenueSummaryForm.B2_ImporterCustomsID;
			CSAImporterName = cSARevenueSummaryForm.ImporterName;
			RSFMonth = cSARevenueSummaryForm.B2_PeriodEndDate.ToString("yyyy/MM");
			PeriodStartDate = cSARevenueSummaryForm.B2_PeriodStartDate.ToString("yyyy/MM/dd");
			PeriodEndDate = cSARevenueSummaryForm.B2_PeriodEndDate.ToString("yyyy/MM/dd");
			VFDOfCurrentMonthTransactions = 0m;
			FilingID = CACustomsDataRegistry.Instance.AccountSecurityNo.Value;
		}

		#endregion

		#region Net Total of Debits and Credits in First Page

		public ZDecimal NetTotalOfDebitsAndCredits1stPage { get; private set; }

		#endregion

		#region Debit Amounts of First Page

		public ZDecimal Debit49010Originals { get; private set; }
		public ZDecimal Debit49010Adjustments { get; private set; }
		public ZDecimal Debit49121Originals { get; private set; }
		public ZDecimal Debit49121Adjustments { get; private set; }
		public ZDecimal Debit49011 { get; private set; }
		public ZDecimal Debit49475 { get; private set; }
		public ZDecimal Debit49443 { get; private set; }
		public ZDecimal Debit49555 { get; private set; }
		public ZDecimal Debit49454 { get; private set; }
		public ZDecimal DebitSubtotal1stPage { get; private set; }

		void GenerateDebitAmounts1stPage()
		{
			var debits = cSARevenueSummaryForm.Debits.Cast<CSARSFPayment>();
			Debit49010Originals = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._490101)?.Amount ?? ZDecimal.Zero;
			Debit49010Adjustments = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._490102)?.Amount ?? ZDecimal.Zero;
			Debit49121Originals = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._491211)?.Amount ?? ZDecimal.Zero;
			Debit49121Adjustments = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._491212)?.Amount ?? ZDecimal.Zero;
			Debit49011 = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49011)?.Amount ?? ZDecimal.Zero;
			Debit49475 = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49475)?.Amount ?? ZDecimal.Zero;
			Debit49443 = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49443)?.Amount ?? ZDecimal.Zero;
			Debit49555 = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49555)?.Amount ?? ZDecimal.Zero;
			Debit49454 = debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49454)?.Amount ?? ZDecimal.Zero;
		}

		#endregion

		#region Credit Amounts of First Page

		public ZDecimal Credit49010 { get; private set; }
		public ZDecimal Credit49121 { get; private set; }
		public ZDecimal Credit49443 { get; private set; }
		public ZDecimal Credit49017 { get; private set; }
		public ZDecimal Credit49018 { get; private set; }
		public ZDecimal Credit49019 { get; private set; }
		public ZDecimal Credit49409 { get; private set; }
		public ZDecimal Credit49437 { get; private set; }
		public ZDecimal Credit49555 { get; private set; }
		public ZDecimal CreditSubtotal1stPage { get; private set; }

		void GenerateCreditAmounts1stPage()
		{
			var credits = cSARevenueSummaryForm.Credits.Cast<CSARSFPayment>();
			Credit49010 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49010)?.Amount ?? ZDecimal.Zero;
			Credit49121 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49121)?.Amount ?? ZDecimal.Zero;
			Credit49443 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49443)?.Amount ?? ZDecimal.Zero;
			Credit49017 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49017)?.Amount ?? ZDecimal.Zero;
			Credit49018 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49018)?.Amount ?? ZDecimal.Zero;
			Credit49019 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49019)?.Amount ?? ZDecimal.Zero;
			Credit49409 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49409)?.Amount ?? ZDecimal.Zero;
			Credit49437 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49437)?.Amount ?? ZDecimal.Zero;
			Credit49555 = credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49555)?.Amount ?? ZDecimal.Zero;
		}

		#endregion

		#region Interim Payments of First Page

		public ZDecimal Interim49010 { get; private set; }
		public ZDecimal Interim49121 { get; private set; }
		public ZDecimal InterimSubtotal1stPage { get; private set; }

		void GenerateInterimAmounts1stPage()
		{
			var interims = cSARevenueSummaryForm.InterimPayments.Cast<CSARSFPayment>();
			Interim49010 = interims.FirstOrDefault(x => x.CodeID == CSARSFInterimPaymentCodes.Codes._49010)?.Amount ?? ZDecimal.Zero;
			Interim49121 = interims.FirstOrDefault(x => x.CodeID == CSARSFInterimPaymentCodes.Codes._49121)?.Amount ?? ZDecimal.Zero;
		}
		#endregion

		#region Customs Assessments

		public CSARSFAssessment Assessment1 { get; private set; }
		public CSARSFAssessment Assessment2 { get; private set; }
		public CSARSFAssessment Assessment3 { get; private set; }
		[BusinessObjectTestExclude]
		public BusinessObjectCollectionWrapper<CSARSFAssessment> CustomsAssessments2ndPage { get; private set; }
		public ZDecimal CustomsAssessmentsSubtotal1stPage { get; private set; }
		public ZDecimal CustomsAssessmentsSubtotal2ndPage { get; private set; }
		public ZBool Hide2ndPage { get; private set; }

		void GenerateCustomsAssessments()
		{
			var assessments = cSARevenueSummaryForm.CustomsAssessments.Cast<CSARSFAssessment>().OrderBy(x => x.Type).ToArray();
			var subTotal1stPage = 0m;
			var subTotal2ndPage = 0m;
			if (assessments.Length > 0)
			{
				Assessment1 = assessments[0];
				subTotal1stPage += Assessment1.Amount;
			}
			if (assessments.Length > 1)
			{
				Assessment2 = assessments[1];
				subTotal1stPage += Assessment2.Amount;
			}
			if (assessments.Length > 2)
			{
				Assessment3 = assessments[2];
				subTotal1stPage += Assessment3.Amount;
			}
			if (assessments.Length > 3)
			{
				var assessmentList = assessments.ToList();
				assessmentList.Remove(Assessment1);
				assessmentList.Remove(Assessment2);
				assessmentList.Remove(Assessment3);
				CustomsAssessments2ndPage = new BusinessObjectCollectionWrapper<CSARSFAssessment>(assessmentList);
				subTotal2ndPage = assessmentList.Sum(x => x.Amount);
				Hide2ndPage = false;
			}
			else
			{
				Hide2ndPage = true;
			}
			CustomsAssessmentsSubtotal1stPage = subTotal1stPage;
			CustomsAssessmentsSubtotal2ndPage = subTotal2ndPage;
		}

		#endregion

		#region Assessment Totals

		public ZDecimal NetTotalCurrentMonthRevenueDistribution { get; private set; }
		public ZDecimal SubtotalInterimPayment { get; private set; }
		public ZDecimal SubtotalCustomsAssessments { get; private set; }
		public ZDecimal TotalPayment { get; private set; }

		void GenerateAssessmentTotals()
		{
			DebitSubtotal1stPage = cSARevenueSummaryForm.DebitsTotal;
			CreditSubtotal1stPage = cSARevenueSummaryForm.CreditsTotal;
			InterimSubtotal1stPage = cSARevenueSummaryForm.InterimPaymentsTotal;
			NetTotalOfDebitsAndCredits1stPage = DebitSubtotal1stPage - CreditSubtotal1stPage;
			NetTotalCurrentMonthRevenueDistribution = NetTotalOfDebitsAndCredits1stPage;
			SubtotalInterimPayment = InterimSubtotal1stPage;
			SubtotalCustomsAssessments = CustomsAssessmentsSubtotal1stPage + CustomsAssessmentsSubtotal2ndPage;
			TotalPayment = NetTotalOfDebitsAndCredits1stPage + InterimSubtotal1stPage + SubtotalCustomsAssessments;
		}

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => cSARevenueSummaryForm.PK;
	}
}
