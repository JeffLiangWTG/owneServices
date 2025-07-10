using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using CAInvoiceHeaderFields = Enterprise.Customs.CA.DataTransfer.CAFlatFileInvoiceDataImporter.Constants.InvoiceHeaderFields;
using CAInvoiceLineFields = Enterprise.Customs.CA.DataTransfer.CAFlatFileInvoiceDataImporter.Constants.InvoiceLineFields;

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CAFlatFileInvoiceDataImporter : Customs.DataTransfer.FlatFileInvoiceDataImporter
	{
		public CAFlatFileInvoiceDataImporter(string fileName, BaseJobDeclaration toJobDec)
			: base(fileName, toJobDec)
		{
			supportMultipleInvoiceHeaders = !ToJobDec.IsLVX;
		}

		#region Implementation

		protected override bool SupportMultipleInvoiceHeaders => supportMultipleInvoiceHeaders;
		readonly bool supportMultipleInvoiceHeaders;

		JobDeclaration ToJobDec => (JobDeclaration)toJobDec;

		#region AddNewInvoice

		protected override BaseJobComInvoiceHeader AddNewInvoice()
		{
			return ToJobDec.LVXInvoiceHeader ?? base.AddNewInvoice();
		}

		protected override BaseJobComInvoiceLine AddNewInvoiceLine()
		{
			return ToJobDec.LVXInvoiceHeader?.JobComInvoiceLines.AddNew() ?? base.AddNewInvoiceLine();
		}

		#endregion

		protected override void PopulateInvoiceHeader(BaseJobComInvoiceHeader invHead, string[] fieldValues)
		{
			base.PopulateInvoiceHeader(invHead, fieldValues);
			var invoiceHeader = (JobComInvoiceHeader)invHead;

			SetValue(invoiceHeader.JZ_RN_NKDefaultOriginInfo, fieldValues, CAInvoiceHeaderFields.CountryOfOrigin);
			SetValue(invoiceHeader.JZ_RW_NKOriginStateInfo, fieldValues, CAInvoiceHeaderFields.StateOfOrigin);

			if (invoiceHeader.IsImport)
			{
				PopulateImportInvoiceHeader(invoiceHeader, fieldValues);
			}
		}

		void PopulateImportInvoiceHeader(JobComInvoiceHeader invoiceHeader, string[] fieldValues)
		{
			SetValue(invoiceHeader.CA_RN_NKExportInfo, fieldValues, CAInvoiceHeaderFields.CountryOfExport);
			if (invoiceHeader.CA_RN_NKExport == Core.Constants.CountryCodes.UnitedStates)
			{
				SetValue(invoiceHeader.CA_USStateOfExportInfo, fieldValues, CAInvoiceHeaderFields.StateOfExport);
			}

			SetValue(invoiceHeader.CA_RN_NKTranshipmentInfo, fieldValues, CAInvoiceHeaderFields.TranshipmentCountry);
			SetValue(invoiceHeader.CA_RL_NKLastPortInfo, fieldValues, CAInvoiceHeaderFields.PlaceOfDirectShipment);
			SetValue(invoiceHeader.JZ_ValuationDateOverrideInfo, fieldValues, CAInvoiceHeaderFields.DateOfDirectShipment);
			SetValue(invoiceHeader.CA_TradeZoneInfo, fieldValues, CAInvoiceHeaderFields.TradeZone);
			if (invoiceHeader.CA_RN_NKExport == Core.Constants.CountryCodes.UnitedStates)
			{
				SetValue(invoiceHeader.CA_USPortOfExitInfo, fieldValues, CAInvoiceHeaderFields.USPortOfExit);
			}

			if (fieldValues.Length > CAInvoiceHeaderFields.TariffTreatment)
			{
				ZString treatmentCode = fieldValues[CAInvoiceHeaderFields.TariffTreatment];
				if (!treatmentCode.IsEmpty)
				{
					SetPropertyInfoValue(invoiceHeader.CA_TreatmentCodeInfo, treatmentCode.PadLeft(2, '0'));
				}
			}
			SetValue(invoiceHeader.CA_ValueForDutyCodeInfo, fieldValues, CAInvoiceHeaderFields.DutyCodeValue);
		}

		protected override void PopulateInvoiceLine(BaseJobComInvoiceLine invLine, string[] fieldValues)
		{
			base.PopulateInvoiceLine(invLine, fieldValues);
			var invoiceLine = (JobComInvoiceLine)invLine;

			SetValue(invoiceLine.JI_StateOrRegionOfOriginInfo, fieldValues, CAInvoiceLineFields.OriginState);

			if (invoiceLine.IsImport)
			{
				PopulateImportInvoiceLine(invoiceLine, fieldValues);
			}
			else if (invoiceLine.IsExport)
			{
				PopulateExportInvoiceLine(invoiceLine, fieldValues);
			}
		}

		void PopulateExportInvoiceLine(JobComInvoiceLine invoiceLine, string[] fieldValues)
		{
			SetValue(invoiceLine.CA_ConveyanceIdentificationNumberInfo, fieldValues, CAInvoiceLineFields.ConveyanceIDs);
			if (fieldValues.Length > CAInvoiceLineFields.ExportPermits)
			{
				ZString permitNumsAsAString = fieldValues[CAInvoiceLineFields.ExportPermits];
				invoiceLine.Permits.Add(permitNumsAsAString);
			}
		}

		void PopulateImportInvoiceLine(JobComInvoiceLine invoiceLine, string[] fieldValues)
		{
			if (fieldValues.Length > CAInvoiceLineFields.TreatmentCode)
			{
				ZString treatmentCode = fieldValues[CAInvoiceLineFields.TreatmentCode];
				if (!treatmentCode.IsEmpty)
				{
					SetPropertyInfoValue(invoiceLine.CA_TreatmentCodeInfo, treatmentCode.PadLeft(2, '0'));
				}
			}
			SetValue(invoiceLine.CA_99TariffCodeInfo, fieldValues, CAInvoiceLineFields.TariffCode);
			SetValue(invoiceLine.CA_ValueForDutyCodeInfo, fieldValues, CAInvoiceLineFields.DutyCodeValue);
			SetValue(invoiceLine.CA_AuthorityNumberInfo, fieldValues, CAInvoiceLineFields.AuthorityNumber);
			SetValue(invoiceLine.CA_TRSNumberInfo, fieldValues, CAInvoiceLineFields.TRSNumber);
			SetValue(invoiceLine.CA_PageNumberInfo, fieldValues, CAInvoiceLineFields.PageNumber);
			AddNewDutyAndTax(invoiceLine, DutyAndTaxTypes.Codes.GST, fieldValues, CAInvoiceLineFields.GSTStatusCode);
			AddNewDutyAndTax(invoiceLine, DutyAndTaxTypes.Codes.SIMADuty, fieldValues, CAInvoiceLineFields.SIMACode);
			AddNewDutyAndTax(invoiceLine, DutyAndTaxTypes.Codes.ExciseTax, fieldValues, CAInvoiceLineFields.ExciseExemptionCode);
			SetValue(invoiceLine.CA_RequirementIDInfo, fieldValues, CAInvoiceLineFields.CFIAReqNum);
			SetValue(invoiceLine.CA_RequirementVerInfo, fieldValues, CAInvoiceLineFields.CFIAReqVer);
			SetValue(invoiceLine.CA_AirsCodeInfo, fieldValues, CAInvoiceLineFields.CFIAAirsCode);
			SetValue(invoiceLine.CA_DestinationProvinceInfo, fieldValues, CAInvoiceLineFields.CFIADestProvince);
			SetValue(invoiceLine.CA_RN_NKCFIAOriginInfo, fieldValues, CAInvoiceLineFields.CFIAOriginCountry);
			if (invoiceLine.CA_RN_NKCFIAOrigin == Core.Constants.CountryCodes.UnitedStates)
			{
				SetValue(invoiceLine.CA_CFIAUSStateOfOriginInfo, fieldValues, CAInvoiceLineFields.CFIAOriginState);
			}
			SetValue(invoiceLine.CA_EndUseInfo, fieldValues, CAInvoiceLineFields.CFIAEndUse);
			SetValue(invoiceLine.CA_MiscIDInfo, fieldValues, CAInvoiceLineFields.CFIAMiscID);

			if (fieldValues.Length > CAInvoiceLineFields.CFIARegNums)
			{
				ZString regNumsAsAString = fieldValues[CAInvoiceLineFields.CFIARegNums];
				invoiceLine.CFIARegistrationNumbers.Add(regNumsAsAString);
			}

			if (fieldValues.Length > CAInvoiceLineFields.SITTCertNums)
			{
				ZString certNumsAsAString = fieldValues[CAInvoiceLineFields.SITTCertNums];
				invoiceLine.SITTCertificationNumbers.Add(certNumsAsAString);
			}

			SetValue(invoiceLine.CA_ImportReasonCodeInfo, fieldValues, CAInvoiceLineFields.ImportReason);
			SetValue(invoiceLine.CA_ModelInfo, fieldValues, CAInvoiceLineFields.ProductModel);
			SetValue(invoiceLine.CA_ModelNumberInfo, fieldValues, CAInvoiceLineFields.ProductNum);
			SetValue(invoiceLine.JI_BrandNameInfo, fieldValues, CAInvoiceLineFields.ProductBrand);
			SetValue(invoiceLine.CA_TIINInfo, fieldValues, CAInvoiceLineFields.TireImporterID);
			SetValue(invoiceLine.CA_TypeSizeInfo, fieldValues, CAInvoiceLineFields.TypeOrSize);

			if (fieldValues.Length > CAInvoiceLineFields.TireCompliantCompletion)
			{
				invoiceLine.CA_CompliantCompletion = CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.IsValidYesReply(fieldValues[CAInvoiceLineFields.TireCompliantCompletion]);
			}
			if (fieldValues.Length > CAInvoiceLineFields.TireImportDateCompliant)
			{
				invoiceLine.CA_CompliantImportDate = CAOrgSupplierPartAndClassificationDataLoad_PGAHelper.IsValidYesReply(fieldValues[CAInvoiceLineFields.TireImportDateCompliant]);
			}
		}

		void AddNewDutyAndTax(JobComInvoiceLine invoiceLine, ZString taxType, string[] fieldValues, int position)
		{
			if (fieldValues.Length > position)
			{
				ZString val = fieldValues[position];
				if (!val.IsEmpty)
				{
					var tax = invoiceLine.DutiesAndTaxes.AddNew(taxType);
					SetValue(tax.C1_ExemptCodeInfo, fieldValues, position);
					if (DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(tax.C1_TaxType))
					{
						tax.C1_Override = true;
					}
					else
					{
						tax.C1_Override = false;
					}
					if (tax.C1_TaxType == DutyAndTaxTypes.Codes.GST)
					{
						tax.C1_Code = DutyAndTaxManager.NormalGSTRefNum;
					}
				}
			}
		}

		protected override void SetDateTimeValue(CargoWise.EntityFramework.ZPropertyInfo propertyInfo, ZString fieldValue)
		{
			if (fieldValue.Length >= 6)
			{
				base.SetDateTimeValue(propertyInfo, fieldValue.PadRight(14, '0'));
			}
		}

		#endregion

		#region Constants

		public new class Constants : Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants
		{
			public new class InvoiceHeaderFields : Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceHeaderFields
			{
				public new const int RecordLength = 47;

				public const int CountryOfOrigin = 36;
				public const int StateOfOrigin = 37;
				public const int CountryOfExport = 38;
				public const int StateOfExport = 39;
				public const int TranshipmentCountry = 40;
				public const int PlaceOfDirectShipment = 41;
				public const int DateOfDirectShipment = 42;
				public const int TradeZone = 43;
				public const int USPortOfExit = 44;
				public const int TariffTreatment = 45;
				public const int DutyCodeValue = 46;
			}

			public new class InvoiceLineFields : Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceLineFields
			{
				public new const int RecordLength = 65;

				public const int HSCode = Enterprise.Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceLineFields.TariffCode;

				public new const int TariffCode = 37;
				public const int DutyCodeValue = 38;
				public const int AuthorityNumber = 39;
				public const int TRSNumber = 40;
				public const int GSTStatusCode = 41;
				public const int SIMACode = 42;
				public const int ExciseExemptionCode = 43;
				public const int ConveyanceIDs = 44;
				public const int ExportPermits = 45;
				public const int CFIAReqNum = 46;
				public const int CFIAReqVer = 47;
				public const int CFIAAirsCode = 48;
				public const int CFIADestProvince = 49;
				public const int CFIAOriginCountry = 50;
				public const int CFIAOriginState = 51;
				public const int CFIAEndUse = 52;
				public const int CFIAMiscID = 53;
				public const int CFIARegNums = 54;
				public const int SITTCertNums = 55;
				public const int ImportReason = 56;
				public const int ProductModel = 57;
				public const int ProductNum = 58;
				public const int ProductBrand = 59;
				public const int TireImporterID = 60;
				public const int TypeOrSize = 61;
				public const int TireCompliantCompletion = 62;
				public const int TireImportDateCompliant = 63;
				public const int PageNumber = 64;
			}
		}

		#endregion
	}
}
