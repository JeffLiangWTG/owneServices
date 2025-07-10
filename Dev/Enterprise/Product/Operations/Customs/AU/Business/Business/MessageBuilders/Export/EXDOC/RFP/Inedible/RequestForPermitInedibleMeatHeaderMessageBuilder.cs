using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitInedibleMeatHeaderMessageBuilder : RequestForPermitHeaderMessageBuilder
	{
		public RequestForPermitInedibleMeatHeaderMessageBuilder(JobComInvoiceHeader invoiceHeader, string messageTypeToSend)
			: base(invoiceHeader, messageTypeToSend)
		{
		}

		protected override DocumentMessageNameCodedList CommodityType()
		{
			return DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodesSingleChar.Codes.InedibleMeat);
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
			if (!invoiceHeader.QuarantineExDocHeader.QH_AbsoluteTemperature.IsEmpty || (!invoiceHeader.QuarantineExDocHeader.QH_MinimumTemperature.IsEmpty && !invoiceHeader.QuarantineExDocHeader.QH_MaximumTemperature.IsEmpty))
			{
				EXDOCMessageUtilities.PopulateMEA(sANCRT.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Temperature,
					PropertyMeasuredCodedList.X_ShippingTolerance,
					invoiceHeader.QuarantineExDocHeader.QH_TemperatureUM,
					invoiceHeader.QuarantineExDocHeader.QH_AbsoluteTemperature.IsEmpty ? "" : invoiceHeader.QuarantineExDocHeader.QH_AbsoluteTemperature.ToString(2),
					invoiceHeader.QuarantineExDocHeader.QH_MinimumTemperature.IsEmpty ? "" : invoiceHeader.QuarantineExDocHeader.QH_MinimumTemperature.ToString(2),
					invoiceHeader.QuarantineExDocHeader.QH_MaximumTemperature.IsEmpty ? "" : invoiceHeader.QuarantineExDocHeader.QH_MaximumTemperature.ToString(2)
				);
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
			RequestForPermitInedibleMeatLineMessageBuilder inedibleMeatLineMessageBuilder = new RequestForPermitInedibleMeatLineMessageBuilder(sANCRT, MessageTypeToSend);
			invoiceHeader.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Ascending);
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				inedibleMeatLineMessageBuilder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			}
		}

		#region Exclusions

		protected override void GenerateLetterOfCredit()
		{
		}

		#endregion
	}
}
