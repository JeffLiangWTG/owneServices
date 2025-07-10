using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUFlatFileInvoiceDataImporter : FlatFileInvoiceDataImporter
	{
		public AUFlatFileInvoiceDataImporter(string fileName, BaseJobDeclaration toJobDec)
			: base(fileName, toJobDec)
		{
		}

		#region Field Constants

		public new class Constants : FlatFileInvoiceDataImporter.Constants
		{
			public new class InvoiceHeaderFields : FlatFileInvoiceDataImporter.Constants.InvoiceHeaderFields
			{
				public new const int RecordLength = 39;

				public const int PreferenceOrigin = 36;
				public const int PreferenceScheme = 37;
				public const int PreferenceRule = 38;
			}

			public new sealed class InvoiceLineFields : FlatFileInvoiceDataImporter.Constants.InvoiceLineFields
			{
				public new const int RecordLength = 41;

				public const int PreferenceOrigin = 37;
				public const int PreferenceScheme = 38;
				public const int PreferenceRule = 39;
				public const int AddInfo = 40;
			}
		}

		#endregion

		protected override System.Type GetOrgSupplierPartType()
		{
			return typeof(OrgSupplierPart);
		}

		protected override void PopulateInvoiceHeader(BaseJobComInvoiceHeader invHead, string[] fieldValues)
		{
			base.PopulateInvoiceHeader(invHead, fieldValues);
			if (invHead is JobComInvoiceHeader)
			{
				JobComInvoiceHeader header = (JobComInvoiceHeader)invHead;
				if (header.AddInfo.IsImportCMR)
				{
					SetValue(header.AddInfo.ZA_POCInfo, fieldValues, Constants.InvoiceHeaderFields.PreferenceOrigin);
					SetValue(header.AddInfo.ZA_PSTInfo, fieldValues, Constants.InvoiceHeaderFields.PreferenceScheme);
					SetValue(header.AddInfo.ZA_PRTInfo, fieldValues, Constants.InvoiceHeaderFields.PreferenceRule);
					SetValue(header.AddInfo.ZA_HeaderREL_HiddenInfo, fieldValues, Constants.InvoiceHeaderFields.Related);
				}
				else
				{
					SetValue(header.AddInfo.ZA_PRFInfo, fieldValues, Constants.InvoiceHeaderFields.Preference);
				}

				if (fieldValues.Length > Constants.InvoiceHeaderFields.TaxExempt)
				{
					ZString taxExempt = (ZString)fieldValues[Constants.InvoiceHeaderFields.TaxExempt];
					header.AddInfo.ZA_GSTE = taxExempt.Left(header.AddInfo.ZA_GSTEInfo.MaxLength);
				}
			}
		}

		protected override void PopulateInvoiceLine(BaseJobComInvoiceLine invLine, string[] fieldValues)
		{
			base.PopulateInvoiceLine(invLine, fieldValues);
			if (invLine is JobComInvoiceLine)
			{
				JobComInvoiceLine line = (JobComInvoiceLine)invLine;

				SetValue(line.AddInfo.ZA_AUState_HiddenInfo, fieldValues, Constants.InvoiceLineFields.OriginState);
				SetValue(line.AddInfo.ZA_TreatmentCode_HiddenInfo, fieldValues, Constants.InvoiceLineFields.TreatmentCode);
				if (line.AddInfo.IsImportCMR)
				{
					SetValue(line.AddInfo.ZA_POCInfo, fieldValues, Constants.InvoiceLineFields.PreferenceOrigin);
					SetValue(line.AddInfo.ZA_PSTInfo, fieldValues, Constants.InvoiceLineFields.PreferenceScheme);
					SetValue(line.AddInfo.ZA_PRTInfo, fieldValues, Constants.InvoiceLineFields.PreferenceRule);
					if (fieldValues.Length > Constants.InvoiceLineFields.AddInfo)
					{
						fieldValues[Constants.InvoiceLineFields.AddInfo] = (line.AddInfo.AddInfoLine + "*" + fieldValues[Constants.InvoiceLineFields.AddInfo]).Trim('*');
					}
					SetValue(line.AddInfo.AddInfoLineInfo, fieldValues, Constants.InvoiceLineFields.AddInfo);
				}
				else
				{
					SetValue(line.AddInfo.ZA_PRFInfo, fieldValues, Constants.InvoiceLineFields.Preference);
				}
			}
		}
	}
}
