using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitFishHeaderMessageBuilder : RequestForPermitHeaderMessageBuilder
	{
		public RequestForPermitFishHeaderMessageBuilder(JobComInvoiceHeader invoiceHeader, string messageTypeToSend)
			: base(invoiceHeader, messageTypeToSend)
		{
		}

		protected override DocumentMessageNameCodedList CommodityType()
		{
			return DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodesSingleChar.Codes.Fish);
		}

		protected override void GenerateOriginCatchingZone()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_OriginCatchZone.IsEmpty)
			{
				const int MaxOriginCatchingZoneChars = 105;
				const int MaxOriginCatchingZoneElementLength = 35;
				EXDOCMessageUtilities.PopulateFTX(sANCRT,
					TextSubjectQualifierList.GetFromString(OriginCatchingZoneSubjectQualifer),
					MaxOriginCatchingZoneChars,
					MaxOriginCatchingZoneElementLength,
					invoiceHeader.QuarantineExDocHeader.QH_OriginCatchZone);
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

		protected override void GenerateRFPLines()
		{
			RequestForPermitFishLineMessageBuilder fishLineMessageBuilder = new RequestForPermitFishLineMessageBuilder(sANCRT, MessageTypeToSend);
			invoiceHeader.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Ascending);
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				fishLineMessageBuilder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			}
		}

		#region Exclusions

		protected override void GenerateLetterOfCredit()
		{
		}

		protected override void GenerateNotifyParty()
		{
		}

		#endregion

		public const string OriginCatchingZoneSubjectQualifer = "OCZ";
	}
}
