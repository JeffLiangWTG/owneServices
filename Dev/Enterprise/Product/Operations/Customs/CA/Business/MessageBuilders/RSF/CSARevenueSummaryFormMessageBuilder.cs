using System.Globalization;
using System.Linq;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.CA.Business
{
	public class CSARevenueSummaryFormMessageBuilder : D99BMessageBuilder<ICSARevenueSummaryForm, CUSDECMessage, RSFMessage>
	{
		public CSARevenueSummaryFormMessageBuilder(ICSARevenueSummaryForm data, MessageSubTypes messageSubType)
			: base(data, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage()
		{
			var unh = CreateUNH();
			CreateBGM();
			CreateDTM();
			CreateGroup1();
			CreateGroup8();
			CreateUNS1();
			CreateGroup10ForDebits();
			CreateGroup10ForCredits();
			CreateGroup10ForInterimPayments();
			CreateGroup10ForCustomsAssessments();
			CreateUNS2();
			CreateGroup49();
			CreateUNT(unh);
		}

		UNHSegment CreateUNH()
		{
			var unh = edifactMessage.UNH[0];
			D99BMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, Enterprise.Edifact.D99B.Elements.MessageTypeList.CustomsDeclarationMessage, "S", "99B", ControllingAgencyList.UnCefact);
			interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);
			return unh;
		}

		void CreateBGM()
		{
			var bgm = edifactMessage.BGM[0];
			D99BMessageUtilities.PopoulateBGMWithDocumentNameCode(bgm, DocumentNameCodeList.BookingRequest, EDIMessage.MessageNumberPlaceHolder, MessageFunctionCode);
			interpretation.AddNewSegmentInterpretation(bgm, () => data.StatementNumber);
		}

		void CreateDTM()
		{
			var dtm1 = edifactMessage.DTM[0];
			var dtm2 = edifactMessage.DTM[1];
			var dtm3 = edifactMessage.DTM[2];
			var dtm4 = edifactMessage.DTM[3];
			D99BMessageUtilities.PopulateDTM(dtm1, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime, data.DocumentMessageDateTime, "yyyyMMdd", DateTimePeriodFormatCodeList.Ccyymmdd);
			D99BMessageUtilities.PopulateDTM(dtm2, DateTimePeriodFunctionCodeQualifierList.ActivityReportingDate, data.RSFMonth, "yyyyMM", DateTimePeriodFormatCodeList.Ccyymm);
			D99BMessageUtilities.PopulateDTM(dtm3, DateTimePeriodFunctionCodeQualifierList.ReportStartDate, data.PeriodStartDateTime, "yyyyMMdd", DateTimePeriodFormatCodeList.Ccyymmdd);
			D99BMessageUtilities.PopulateDTM(dtm4, DateTimePeriodFunctionCodeQualifierList.ReportEndDate, data.PeriodEndDateTime, "yyyyMMdd", DateTimePeriodFormatCodeList.Ccyymmdd);
			interpretation.AddNewSegmentInterpretation(dtm1, () => data.DocumentMessageDateTime);
			interpretation.AddNewSegmentInterpretation(dtm2, () => data.RSFMonth);
			interpretation.AddNewSegmentInterpretation(dtm3, () => data.PeriodStartDateTime);
			interpretation.AddNewSegmentInterpretation(dtm4, () => data.PeriodEndDateTime);
		}

		void CreateGroup1()
		{
			var group1 = edifactMessage.Group1[0];
			var rff = group1.RFF[0];
			D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.NationalGovernmentBusinessIdentificationNumber, data.BusinessNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => data.BusinessNumber);
		}

		void CreateGroup8()
		{
			var group8 = edifactMessage.Group8[0];
			var moa = group8.MOA[0];
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.DeclaredTotalCustomsValue, data.VFD);
			interpretation.AddNewSegmentInterpretation(moa, () => data.VFD);
		}

		void CreateUNS1()
		{
			var uns1 = edifactMessage.UNS1[0];
			D99BMessageUtilities.PopulateUNS(uns1, SectionIdentificationList.HeaderDetailSectionSeparation);
			interpretation.AddUNS1Interpretation(uns1);
		}

		void CreateUNS2()
		{
			var uns2 = edifactMessage.UNS2[0];
			D99BMessageUtilities.PopulateUNS(uns2, SectionIdentificationList.DetailSummarySectionSeparation);
			interpretation.AddUNS2Interpretation(uns2);
		}

		void CreateGroup10ForDebits()
		{
			var group10 = edifactMessage.Group10[0];
			var dms = group10.DMS[0];
			D99BMessageUtilities.PopulateDMSWithNameCode(dms, DocumentNameCodeList.DebitNoteRelatedToFinancialAdjustments);
			interpretation.AddNewSegmentInterpretation(dms, () => DocumentNameCodeList.DebitNoteRelatedToFinancialAdjustments);
			var count = 0;
			foreach (var debit in data.Debits.OrderBy(x => x.CodeID))
			{
				CreateGroup21ForDebit(group10, debit, count);
				++count;
			}
		}

		void CreateGroup21ForDebit(SegmentGroup10 group10, ICSARSFItem debit, int index)
		{
			var group21 = group10.Group21[index];
			var lin = group21.LIN[0];
			if (debit.CodeID == CSARSFDebitCodes.Codes._490101 ||
				debit.CodeID == CSARSFDebitCodes.Codes._491211)
			{
				D99BMessageUtilities.PopulateLIN(lin, debit.LineItemNumber, ActionRequestNotificationDescriptionCodeList.NotAmended);
			}
			else if (debit.CodeID == CSARSFDebitCodes.Codes._490102 ||
				debit.CodeID == CSARSFDebitCodes.Codes._491212)
			{
				D99BMessageUtilities.PopulateLIN(lin, debit.LineItemNumber, ActionRequestNotificationDescriptionCodeList.Amendments);
			}
			else
			{
				D99BMessageUtilities.PopulateLIN(lin, debit.LineItemNumber);
			}
			interpretation.AddNewSegmentInterpretation(lin, () => debit.LineItemNumber);
			var moa = group21.MOA[0];
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.LineItemAmount, debit.MonetaryAmount);
			interpretation.AddNewSegmentInterpretation(moa, () => debit.MonetaryAmount);
		}

		void CreateGroup10ForCredits()
		{
			var group10 = edifactMessage.Group10[1];
			var dms = group10.DMS[0];
			D99BMessageUtilities.PopulateDMSWithNameCode(dms, DocumentNameCodeList.CreditNoteRelatedToFinancialAdjustments);
			interpretation.AddNewSegmentInterpretation(dms, () => DocumentNameCodeList.CreditNoteRelatedToFinancialAdjustments);
			var count = 0;
			foreach (var credit in data.Credits.OrderBy(x => x.CodeID))
			{
				CreditsGroup21ForCredit(group10, credit, count);
				++count;
			}
		}

		void CreditsGroup21ForCredit(SegmentGroup10 group10, ICSARSFItem credit, int index)
		{
			var group21 = group10.Group21[index];
			var lin = group21.LIN[0];
			D99BMessageUtilities.PopulateLIN(lin, credit.LineItemNumber);
			interpretation.AddNewSegmentInterpretation(lin, () => credit.LineItemNumber);
			var moa = group21.MOA[0];
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.LineItemAmount, credit.MonetaryAmount);
			interpretation.AddNewSegmentInterpretation(moa, () => credit.MonetaryAmount);
		}

		void CreateGroup10ForInterimPayments()
		{
			var group10 = edifactMessage.Group10[2];
			var dms = group10.DMS[0];
			D99BMessageUtilities.PopulateDMSWithNameCode(dms, DocumentNameCodeList.CurrentAccount);
			interpretation.AddNewSegmentInterpretation(dms, () => DocumentNameCodeList.CurrentAccount);
			var count = 0;
			foreach (var interimPayment in data.InterimPayments.OrderBy(x => x.CodeID))
			{
				CreditsGroup21ForInterimPayment(group10, interimPayment, count);
				++count;
			}
		}

		void CreditsGroup21ForInterimPayment(SegmentGroup10 group10, ICSARSFItem interimPayment, int index)
		{
			var group21 = group10.Group21[index];
			var lin = group21.LIN[0];
			D99BMessageUtilities.PopulateLIN(lin, interimPayment.LineItemNumber);
			interpretation.AddNewSegmentInterpretation(lin, () => interimPayment.LineItemNumber);
			var moa = group21.MOA[0];
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.LineItemAmount, interimPayment.MonetaryAmount);
			interpretation.AddNewSegmentInterpretation(moa, () => interimPayment.MonetaryAmount);
		}

		void CreateGroup10ForCustomsAssessments()
		{
			var group10 = edifactMessage.Group10[3];
			var dms = group10.DMS[0];
			D99BMessageUtilities.PopulateDMSWithNameCode(dms, DocumentNameCodeList.CollateralAccount);
			interpretation.AddNewSegmentInterpretation(dms, () => DocumentNameCodeList.CollateralAccount);
			var count = 0;
			foreach (var customsAssessment in data.CustomsAssessments.OrderBy(x => x.CodeID))
			{
				CreditsGroup21ForCustomsAssessment(group10, customsAssessment, count);
				++count;
			}
		}

		void CreditsGroup21ForCustomsAssessment(SegmentGroup10 group10, ICSARSFItem customsAssessment, int index)
		{
			var group21 = group10.Group21[index];
			var assessmentCode = customsAssessmentsCodes.GetDescriptionFromCode(customsAssessment.Type) ?? customsAssessment.Type;
			var lin = group21.LIN[0];
			D99BMessageUtilities.PopulateLIN(lin, assessmentCode);
			interpretation.AddNewSegmentInterpretation(lin, () => assessmentCode);
			if (assessmentCode != CustomsAssessmentsCodes.Descriptions.B2Dash1)
			{
				var moa = group21.MOA[0];
				D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.LineItemAmount, customsAssessment.MonetaryAmount);
				interpretation.AddNewSegmentInterpretation(moa, () => customsAssessment.MonetaryAmount);

				var loc = group21.LOC[0];
				D99BMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.CustomsOfficeOfRegistrationOfPreviousCustomsDeclaration, customsAssessment.PortCode);
				interpretation.AddNewSegmentInterpretation(loc, () => customsAssessment.PortCode);
			}
			var group22 = group21.Group22[0];
			var doc = group22.DOC[0];
			D99BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.PreviousCustomsDocumentMessage, customsAssessment.LineItemNumber);
			interpretation.AddNewSegmentInterpretation(doc, () => customsAssessment.LineItemNumber);
		}

		readonly CustomsAssessmentsCodes customsAssessmentsCodes = new CustomsAssessmentsCodes();

		void CreateGroup49()
		{
			var group49 = edifactMessage.Group49[0];
			var tax = group49.TAX[0];
			D99BMessageUtilities.PopulateTAX(tax, DutyTaxFeeFunctionQualifierList.TotalOfAllDutiesTaxesAndFeeTypesCustomsDeclaration);
			interpretation.AddNewSegmentInterpretation(tax, () => DutyTaxFeeFunctionQualifierList.TotalOfAllDutiesTaxesAndFeeTypesCustomsDeclaration);
			var moa = group49.MOA[0];
			D99BMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.MessageTotalDutyTaxFeeAmount, data.TotalPayment);
			interpretation.AddNewSegmentInterpretation(moa, () => data.TotalPayment);
		}

		void CreateUNT(UNHSegment unh)
		{
			var unt = edifactMessage.UNT[0];
			D99BMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture), unh.MessageReferenceNumber);
			interpretation.AddUNTInterpretation(unt, unh.MessageReferenceNumber);
		}
	}
}
