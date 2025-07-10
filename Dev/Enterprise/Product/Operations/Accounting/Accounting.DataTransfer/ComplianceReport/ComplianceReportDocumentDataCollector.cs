using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	internal class ComplianceReportDocumentDataCollector
	{
		public ComplianceReportDocumentDataCollector(AccComplianceReport report)
		{
			Report = report;
			ComplianceDocumentHeader = GetDocumentHeaderDetails();
		}

		protected readonly AccComplianceReport Report;

		internal readonly ComplianceDocumentHeaderDetails[] ComplianceDocumentHeader;

		ComplianceDocumentHeaderDetails[] GetDocumentHeaderDetails()
		{
			var query = new ZQuery(AccComplianceDocumentHeaderSchema.PK, Report.ReportLines.Cast<AccComplianceReportLine>().Select(x => x.AH_PK).Distinct());
			var complianceDocumentHeaders = new BusinessObjectFactory().Load<AccComplianceDocumentHeader>(query);

			var result = new List<ComplianceDocumentHeaderDetails>();

			foreach (var documentHeader in complianceDocumentHeaders)
			{
				var headerDetail = new ComplianceDocumentHeaderDetails();
				headerDetail.Ledger = documentHeader.ADH_Ledger;
				headerDetail.ComplianceSubType = documentHeader.ADH_ComplianceSubType;
				headerDetail.VATRegistrationNum = documentHeader.VATRegistrationNum;
				headerDetail.ReportingPeriod = documentHeader.ADH_ReportingPeriod;
				headerDetail.DocumentDate = documentHeader.ADH_DocumentDate;
				headerDetail.DocumentNumber = documentHeader.ADH_DocumentNumber;
				headerDetail.DocumentStatus = documentHeader.ADH_DocumentStatus;
				headerDetail.ExTaxAmount = documentHeader.Amount;
				headerDetail.TaxAmount = documentHeader.TaxAmount;
				headerDetail.CustomRelated = documentHeader.ADH_CustomRelated;
				headerDetail.SupportingReason = documentHeader.ADH_SupportingReason;
				headerDetail.LineRateCode = documentHeader.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().Select(x => x.TaxRate.AT_Code).ToArray();
				headerDetail.SupportingDocumentType = documentHeader.ADH_SupportingDocumentType;
				headerDetail.SupportingDocumentNumber = documentHeader.ADH_SupportingDocumentNumber;
				headerDetail.OrgHeaderCategory = documentHeader.OrgHeaderCategory;
				headerDetail.TransactionType = documentHeader.ADH_TransactionType;
				headerDetail.OrgHeaderCountryCode = documentHeader.Organisation?.CountryCode ?? ZString.Empty;

				result.Add(headerDetail);
			}

			return result.ToArray();
		}

		internal ComplianceSequenceDetails[] GetUnusedComplianceSequenceDetails()
		{
			var newFactory = new BusinessObjectFactory();

			var sequenceQuery = new ZQuery();
			sequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_StartDate, SQLComparisonOperator.GreaterThanOrEqualTo, Report.ACR_DateFrom);
			sequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_ExpiryDate, SQLComparisonOperator.LessThanOrEqualTo, Report.ACR_DateTo);
			sequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_IsActive, true);
			sequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_GC_Company, Report.ACR_GC_Company);
			var subTypes = Report.GetComplianceSubTypes();
			if (subTypes.Length > 0)
			{
				sequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_SequenceClass, subTypes);
			}
			var sequences = newFactory.Load<AccComplianceSequence>(sequenceQuery);

			var result = new List<ComplianceSequenceDetails>();
			foreach (var sequence in sequences)
			{
				var sequenceDetail = new ComplianceSequenceDetails();
				sequenceDetail.ComplianceSubType = sequence.XD_SequenceClass;
				sequenceDetail.Prefix = sequence.XD_Prefix;
				sequenceDetail.NextNumber = sequence.XD_NextNumber;
				sequenceDetail.EndNumber = sequence.XD_EndNumber;
				sequenceDetail.MaximumNumberDigits = sequence.XD_MaximumNumberDigits;
				sequenceDetail.ExpiryDate = sequence.XD_ExpiryDate;

				result.Add(sequenceDetail);
			}

			return result.ToArray();
		}
	}

	#region Additional Data

	internal class ComplianceDocumentHeaderDetails
	{
		public ZString Ledger { get; set; }
		public ZString ComplianceSubType { get; set; }
		public ZString VATRegistrationNum { get; set; }
		public ZString[] LineRateCode { get; set; }
		public ZString DocumentStatus { get; set; }
		public ZDateTime DocumentDate { get; set; }
		public ZInt ReportingPeriod { get; set; }
		public ZString DocumentNumber { get; set; }
		public ZDecimal ExTaxAmount { get; set; }
		public ZDecimal TaxAmount { get; set; }
		public ZBool CustomRelated { get; set; }
		public ZString SupportingReason { get; set; }
		public ZString SupportingDocumentType { get; set; }
		public ZString SupportingDocumentNumber { get; set; }
		public ZString OrgHeaderCategory { get; set; }
		public ZString TransactionType { get; set; }
		public ZString OrgHeaderCountryCode { get; set; }
	}

	internal class ComplianceSequenceDetails
	{
		public ZString ComplianceSubType { get; set; }
		public ZString Prefix { get; set; }
		public ZDecimal NextNumber { get; set; }
		public ZDecimal EndNumber { get; set; }
		public ZByte MaximumNumberDigits { get; set; }
		public ZDateTime ExpiryDate { get; set; }
	}

	#endregion
}
