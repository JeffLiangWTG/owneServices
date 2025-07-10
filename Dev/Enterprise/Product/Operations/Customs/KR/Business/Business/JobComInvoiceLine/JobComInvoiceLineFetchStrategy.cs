using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				var columnName = column.ColumnName;
				switch (columnName)
				{
					case JobComInvoiceLine.Schema.AdditionalInformationContent:
						Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
						break;
					case JobComInvoiceLine.Schema.JI_CustomsValue:
					case JobComInvoiceLine.Schema.JI_Calc_CIF_InLocalCurrency:
					case JobComInvoiceLine.Schema.JI_RX_LocalCurrency:
						Factory.AddFetchHint(RefCurrencySchema.RX_Code, BusinessObject.JI_RX_LocalCurrency);
						break;
					case JobComInvoiceLine.Schema.JI_Calc_Balance:
					case JobComInvoiceLine.Schema.JI_Calc_LinesEntered:
					case JobComInvoiceLine.Schema.JI_Calc_LinesTotal:
						Factory.AddFetchHint(JobComInvoiceHeader.Schema.PK, BusinessObject.JI_JZ);
						break;
					case nameof(JobComInvoiceLine.CertificateOfOriginAgencyName):
					case nameof(JobComInvoiceLine.CertificateOfOriginAreaName):
					case nameof(JobComInvoiceLine.CertificateOfOriginCriteriaCode):
					case nameof(JobComInvoiceLine.CertificateOfOriginIssueDate):
					case nameof(JobComInvoiceLine.CertificateOfOriginIssueStatus):
					case nameof(JobComInvoiceLine.CertificateOfOriginIssuingCountry):
					case nameof(JobComInvoiceLine.CertificateOfOriginLineNo):
					case nameof(JobComInvoiceLine.CertificateOfOriginNo):
					case nameof(JobComInvoiceLine.CertificateOfOriginPersonName):
					case nameof(JobComInvoiceLine.CertificateOfOriginProductType):
					case nameof(JobComInvoiceLine.CertificateOfOriginStatus):
					case nameof(JobComInvoiceLine.CertificateOfOriginUQ):
					case nameof(JobComInvoiceLine.COOIssuerType):
					case nameof(JobComInvoiceLine.COOLineNumber):
					case nameof(JobComInvoiceLine.COOTotalNetWeight):
					case nameof(JobComInvoiceLine.COOTotalNetWeightUQ):
					case nameof(JobComInvoiceLine.COOUsedQuantityUQ):
					case nameof(JobComInvoiceLine.COOSplitOrder):
					case nameof(JobComInvoiceLine.CriteriaForDeterminingCountryOfOrigin):
					case nameof(JobComInvoiceLine.PRA_DateOfExpiry):
					case nameof(JobComInvoiceLine.PRA_DateOfIssue):
					case nameof(JobComInvoiceLine.PRA_ReferenceNumber):
					case nameof(JobComInvoiceLine.SupportingDocumentCode):
					case nameof(JobComInvoiceLine.SupportingDocumentReferenceNumber):
						Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, BusinessObject.PK);
						break;
				}
			}
		}
	}
}
