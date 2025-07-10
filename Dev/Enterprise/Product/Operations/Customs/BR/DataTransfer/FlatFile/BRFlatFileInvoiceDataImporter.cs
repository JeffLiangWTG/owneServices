using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using BRInvoiceLineFields = Enterprise.Customs.BR.DataTransfer.BRFlatFileInvoiceDataImporter.Constants.InvoiceLineFields;

namespace Enterprise.Customs.BR.DataTransfer
{
	public class BRFlatFileInvoiceDataImporter : Customs.DataTransfer.FlatFileInvoiceDataImporter
	{
		public BRFlatFileInvoiceDataImporter(string fileName, BaseJobDeclaration toJobDec)
			: base(fileName, toJobDec)
		{
		}

		#region Implementation

		protected override void PopulateInvoiceLine(BaseJobComInvoiceLine invLine, string[] fieldValues)
		{
			base.PopulateInvoiceLine(invLine, fieldValues);
			var invoiceLine = (JobComInvoiceLine)invLine;

			if (invoiceLine.IsExport)
			{
				PopulateExportInvoiceLine(invoiceLine, fieldValues);
			}
		}

		void PopulateExportInvoiceLine(JobComInvoiceLine invoiceLine, string[] fieldValues)
		{
			SetValue(invoiceLine.JI_NFeNumberInfo, fieldValues, BRInvoiceLineFields.NfeNumber);
			SetValue(invoiceLine.JI_NFeItemNumberInfo, fieldValues, BRInvoiceLineFields.NfeItem);
			SetValue(invoiceLine.JI_ProcedureInfo, fieldValues, BRInvoiceLineFields.FirstCPC);
			SetValue(invoiceLine.JI_SecondCPCInfo, fieldValues, BRInvoiceLineFields.SecondCPC);
			SetValue(invoiceLine.JI_ThirdCPCInfo, fieldValues, BRInvoiceLineFields.ThirdCPC);
			SetValue(invoiceLine.JI_FourthCPCInfo, fieldValues, BRInvoiceLineFields.FourthCPC);
			SetValue(invoiceLine.JI_CargoPriorityInfo, fieldValues, BRInvoiceLineFields.CargoPriority);
			SetValue(invoiceLine.ComplementaryDescriptionInfo, fieldValues, BRInvoiceLineFields.ComplementaryDescription);
			SetValue(invoiceLine.JI_RN_NKCountryOfExportInfo, fieldValues, BRInvoiceLineFields.CountryOfDestination);
			SetValue(invoiceLine.JI_FinancedValueInfo, fieldValues, BRInvoiceLineFields.FinanceValue);
			SetValue(invoiceLine.JI_ExportJustificationInfoInfo, fieldValues, BRInvoiceLineFields.Justification);
			SetValue(invoiceLine.JI_IntendedTermDaysInfo, fieldValues, BRInvoiceLineFields.IntendedTerm);
			SetValue(invoiceLine.JI_DigitalServiceDossierInfo, fieldValues, BRInvoiceLineFields.DigitalService);
			SetValue(invoiceLine.JI_NFeLinePriceInfo, fieldValues, BRInvoiceLineFields.NfeLinePrice);
		}

		#endregion

		#region Constants

		public new class Constants : Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants
		{
			public new class InvoiceLineFields : Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceLineFields
			{
				public const int NfeNumber = 37;
				public const int NfeItem = 38;
				public const int FirstCPC = 39;
				public const int SecondCPC = 40;
				public const int ThirdCPC = 41;
				public const int FourthCPC = 42;
				public const int CargoPriority = 43;
				public const int ComplementaryDescription = 44;
				public const int CountryOfDestination = 45;
				public const int FinanceValue = 46;
				public const int Justification = 47;
				public const int IntendedTerm = 48;
				public const int DigitalService = 49;
				public const int NfeLinePrice = 50;
			}
		}

		#endregion
	}
}
