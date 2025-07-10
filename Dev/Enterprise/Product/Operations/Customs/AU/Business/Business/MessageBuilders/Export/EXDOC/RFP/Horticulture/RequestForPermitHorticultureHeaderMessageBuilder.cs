using System.Globalization;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitHorticultureHeaderMessageBuilder : RequestForPermitHeaderMessageBuilder
	{
		public RequestForPermitHorticultureHeaderMessageBuilder(JobComInvoiceHeader invoiceHeader, string messageTypeToSend)
			: base(invoiceHeader, messageTypeToSend)
		{
		}

		protected override DocumentMessageNameCodedList CommodityType()
		{
			return DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodesSingleChar.Codes.Horticulture);
		}

		protected override void GenerateLotNumber()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_LotNumber.IsEmpty)
			{
				const int MaxLotNumberChars = 70;
				EXDOCMessageUtilities.PopulateFTX(sANCRT,
					TextSubjectQualifierList.AdditionalMarksNumbersInformation,
					MaxLotNumberChars,
					MaxElementLength,
					invoiceHeader.QuarantineExDocHeader.QH_LotNumber);
			}
		}

		protected override ZString ConsigneeReferenceNumber
		{
			get { return ZString.Empty; }
		}

		protected override void GenerateInspectionRequestedDate(SegmentGroup8 group8)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_InspectionRequestedDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group8.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.RequestDate,
					invoiceHeader.QuarantineExDocHeader.QH_InspectionRequestedDate.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmddhhmmss);
			}
		}

		protected override void GenerateAuthorisedStartDate(SegmentGroup8 group8)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_AuthorisedStartDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group8.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.StartDateTime,
					invoiceHeader.QuarantineExDocHeader.QH_AuthorisedStartDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected override void GenerateRFPLines()
		{
			RequestForPermitHorticultureLineMessageBuilder horticultureLineMessageBuilder = new RequestForPermitHorticultureLineMessageBuilder(sANCRT, MessageTypeToSend);
			invoiceHeader.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Ascending);
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				horticultureLineMessageBuilder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
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

		protected override void GenerateAmendmentText()
		{
			if (MessageTypeToSend == EXDOCMessageTypeCodes.Codes.RPL && !invoiceHeader.JobDeclaration.EXDOCAmendmentReason.IsEmpty)
			{
				EXDOCMessageUtilities.Populate8LineFTX(
					sANCRT,
					TextSubjectQualifierList.ChangeInformation,
					MaxAmendmentReasonChars,
					MaxAmendmentReasonElementChars,
					invoiceHeader.JobDeclaration.EXDOCAmendmentReason);
			}
		}

		#region Exclusions

		protected override void GenerateConsigneePhone(SegmentGroup2 group2)
		{
		}

		protected override void GenerateLetterOfCredit()
		{
		}

		protected override void GenerateNotifyParty()
		{
		}

		#endregion
	}
}
