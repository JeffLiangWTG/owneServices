using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUInvoiceDataTransferTool : InvoiceDataTransferTool
	{
		public AUInvoiceDataTransferTool(bool isStandAlone, AddInfoDataTransferTool addInfoDataTransferTool)
			: base(isStandAlone)
		{
			this.AddInfoDataTransferTool = addInfoDataTransferTool;
		}

		protected readonly AddInfoDataTransferTool AddInfoDataTransferTool;

		protected override void ImportInvoiceLineDetail(BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLine xsdInvoiceLine, IValueObjectImportContext context)
		{
			JobComInvoiceLine aUInvLine = invoiceLine as JobComInvoiceLine;
			if (aUInvLine != null && xsdInvoiceLine != null)
			{
				if (xsdInvoiceLine.LineClassification != null)
				{
					context.SetPropertyInfoValue(aUInvLine.AddInfo.ZA_PRFInfo, xsdInvoiceLine.LineClassification.Preference, ForeignKeyType.None, "Preference");
					context.SetPropertyInfoValue(aUInvLine.AddInfo.ZA_TreatmentCode_HiddenInfo, xsdInvoiceLine.LineClassification.TreatmentCode, ForeignKeyType.None, "TreatmentCode");
				}

				aUInvLine.JI_IsPackToBondForLine = xsdInvoiceLine.Bond;
			}

			base.ImportInvoiceLineDetail(invoiceLine, xsdInvoiceLine, context);
		}

		protected override void ImportInvoiceHeaderAdditionalInfo(BaseJobComInvoiceHeader invHead, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
			JobComInvoiceHeader aUInvHeader = invHead as JobComInvoiceHeader;
			if (aUInvHeader != null)
			{
				AddInfoDataTransferTool.ImportAddInfos(aUInvHeader.AddInfo.ZPropertyInfoHash, addCustomsDetails, context);
			}
		}

		protected override void ImportInvoiceLinesAdditionalInfo(BaseJobComInvoiceLine invLine, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
			JobComInvoiceLine aUInvLine = invLine as JobComInvoiceLine;
			if (aUInvLine != null)
			{
				AddInfoDataTransferTool.ImportAddInfos(aUInvLine.AddInfo.ZPropertyInfoHash, addCustomsDetails, context);
			}
		}

		protected override void ExportInvoiceLineSummary(Xsd.InvoiceLineSummary summary, BaseJobComInvoiceLine line)
		{
			base.ExportInvoiceLineSummary(summary, line);

			if (summary != null)
			{
				JobComInvoiceLine aUInvLine = line as JobComInvoiceLine;
				summary.TransportAndInsurance.Value = (aUInvLine.CusEntryLine != null) ? aUInvLine.JI_Calc_CustomsTransportAndInsuranceInLocalCurrency : aUInvLine.TransportAndInsuranceInLocalCurrency;
			}
		}

		public override void ExportInvoiceLineDetails(Xsd.InvoiceLine newInvoiceLine, BaseJobComInvoiceLine invoiceLine, INotifications notify)
		{
			base.ExportInvoiceLineDetails(newInvoiceLine, invoiceLine, notify);

			JobComInvoiceLine aUInvLine = invoiceLine as JobComInvoiceLine;

			Classification @class = aUInvLine.Classification;
			if (@class != null)
			{
				newInvoiceLine.LineClassification.InstrumentType = @class.InstrumentType;
				newInvoiceLine.LineClassification.InstrumentCode = @class.InstrumentCode;
				newInvoiceLine.LineClassification.TreatmentCode = @class.TreatmentCode;
			}

			newInvoiceLine.Bond = aUInvLine.JI_IsPackToBondForLine;
		}

		protected override void ExportInvoiceHeaderAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, BaseJobComInvoiceHeader invoiceHeader, IValueObjectExportContext context)
		{
			JobComInvoiceHeader aUInvHeader = invoiceHeader as JobComInvoiceHeader;
			if (aUInvHeader != null)
			{
				AddInfoDataTransferTool.ExportAddInfos(addCustomsDetails, aUInvHeader.AddInfo.ZPropertyInfoHash);
			}
		}

		protected override void ExportInvoiceLinesAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, BaseJobComInvoiceLine invoiceLine, IValueObjectExportContext context)
		{
			JobComInvoiceLine aUInvLine = invoiceLine as JobComInvoiceLine;
			if (aUInvLine != null)
			{
				AddInfoDataTransferTool.ExportAddInfos(addCustomsDetails, aUInvLine.AddInfo.ZPropertyInfoHash);
			}
		}
	}
}
