using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitDairyHeaderMessageBuilder : RequestForPermitHeaderMessageBuilder
	{
		public RequestForPermitDairyHeaderMessageBuilder(JobComInvoiceHeader invoiceHeader, string messageTypeToSend)
			: base(invoiceHeader, messageTypeToSend)
		{
		}

		protected override DocumentMessageNameCodedList CommodityType()
		{
			return DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodesSingleChar.Codes.Dairy);
		}

		protected override void GenerateAdditionalInformation()
		{
			if (!invoiceHeader.JobDeclaration.EXDOCAdditionalInformation.IsEmpty)
			{
				const int MaxAdditionalInfoChars = 34650;
				EXDOCMessageUtilities.PopulateFTX(sANCRT,
					TextSubjectQualifierList.AdditionalInformation,
					MaxAdditionalInfoChars,
					RequestForPermitHeaderMessageBuilder.MaxElementLength,
					invoiceHeader.JobDeclaration.EXDOCAdditionalInformation);
			}
		}

		protected override void GenerateTranshipmentStorageTemperature()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_AbsoluteTemperature.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(sANCRT.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Temperature,
					PropertyMeasuredCodedList.X_ShippingTolerance,
					invoiceHeader.QuarantineExDocHeader.QH_TemperatureUM,
					invoiceHeader.QuarantineExDocHeader.QH_AbsoluteTemperature.ToString(2),
					ZString.Empty,
					ZString.Empty);
			}
		}

		protected override void GenerateAMLCQuotaIndicator()
		{
			EXDOCMessageUtilities.PopulateGIS(sANCRT,
				invoiceHeader.QuarantineExDocHeader.QH_AMLCQuota ? "Y" : "N",
				AMLCQuotaIndicatorQualifier);
		}

		protected override void GenerateAMLCQuotaYear()
		{
			const int MaxQuotaYearChars = 7;

			if (invoiceHeader.QuarantineExDocHeader.QH_AMLCQuota)
			{
				EXDOCMessageUtilities.PopulateFTX(
					sANCRT,
					TextSubjectQualifierList.NegotiationTermsAdditional,
					MaxQuotaYearChars,
					MaxQuotaYearChars,
					invoiceHeader.QuarantineExDocHeader.QH_AMLCQuotaYear);
			}
		}

		protected override void GenerateProductUseIndicator()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_ProductUseIndicator.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIS(sANCRT,
					invoiceHeader.QuarantineExDocHeader.QH_ProductUseIndicator,
					ProductUseIndicator);
			}
		}

		protected override void GenerateRFPLines()
		{
			RequestForPermitDairyLineMessageBuilder dairyLineMessageBuilder = new RequestForPermitDairyLineMessageBuilder(sANCRT, MessageTypeToSend);
			invoiceHeader.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Ascending);
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				dairyLineMessageBuilder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			}
		}

		protected override void GenerateDeclarationOfComplianceIndicator()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_DecOfCompliance.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIS(sANCRT,
					invoiceHeader.QuarantineExDocHeader.QH_DecOfCompliance.Left(1),
					DeclarationOfComplianceIndicator);
			}
		}

		protected override void GenerateTrueAndCompleteIndicator()
		{
			EXDOCMessageUtilities.PopulateGIS(sANCRT,
				invoiceHeader.QuarantineExDocHeader.QH_TrueAndCompleteIndicator.Left(1),
				TrueAndCompleteIndicator);
		}

		protected override void GenerateImportedProductFlag()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_ImportedProductFlag.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIS(sANCRT,
					invoiceHeader.QuarantineExDocHeader.QH_ImportedProductFlag.Left(1),
					ImportedProductFlagIndicator);
			}
		}

		#region Exclusions

		protected override void GenerateAuthorisedEndDate(SegmentGroup8 group8)
		{
		}

		protected override void GenerateLetterOfCredit()
		{
		}

		#endregion

		public const string ImportedProductFlagIndicator = "IPF";
	}
}
