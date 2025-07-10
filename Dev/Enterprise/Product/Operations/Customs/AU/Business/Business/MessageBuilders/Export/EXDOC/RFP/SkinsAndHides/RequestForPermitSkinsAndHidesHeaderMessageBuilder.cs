using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitSkinsAndHidesHeaderMessageBuilder : RequestForPermitHeaderMessageBuilder
	{
		public RequestForPermitSkinsAndHidesHeaderMessageBuilder(JobComInvoiceHeader invoiceHeader, string messageTypeToSend)
			: base(invoiceHeader, messageTypeToSend)
		{
		}

		protected override DocumentMessageNameCodedList CommodityType()
		{
			return DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodesSingleChar.Codes.SkinsAndHides);
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

		protected override void GenerateDTM()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_PackDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(sANCRT.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.PackagingDate,
					invoiceHeader.QuarantineExDocHeader.QH_PackDate.ToString("yyyyMMdd"),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected override void GenerateRFPLines()
		{
			RequestForPermitSkinsAndHidesLineMessageBuilder skinsAndHidesLineMessageBuilder = new RequestForPermitSkinsAndHidesLineMessageBuilder(sANCRT, MessageTypeToSend);
			invoiceHeader.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Ascending);
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				skinsAndHidesLineMessageBuilder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			}
		}

		#region Exclusions

		protected override void GenerateAuthorisingOfficerIdentifier(SegmentGroup9 group9)
		{
		}

		protected override void GenerateInspectionRequestedDate(SegmentGroup8 group8)
		{
		}

		#endregion
	}
}
