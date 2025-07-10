using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitMeatHeaderMessageBuilder : RequestForPermitHeaderMessageBuilder
	{
		public RequestForPermitMeatHeaderMessageBuilder(JobComInvoiceHeader invoiceHeader, string messageTypeToSend)
			: base(invoiceHeader, messageTypeToSend)
		{
		}

		protected override DocumentMessageNameCodedList CommodityType()
		{
			return DocumentMessageNameCodedList.GetFromString(EXDOCCommodityCodesSingleChar.Codes.Meat);
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
					string.Empty,
					string.Empty);
			}
		}

		protected override void GenerateShipStoresIndicator()
		{
			EXDOCMessageUtilities.PopulateGIS(sANCRT,
				invoiceHeader.QuarantineExDocHeader.QH_ShipsStores ? "Y" : "N",
				ShipStoresIndicatorQualifier);
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

		protected override void GenerateQuotaType()
		{
			const int MaxQuotaTypeChars = 5;

			if (!invoiceHeader.QuarantineExDocHeader.QH_QuotaType.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateFTX(
					sANCRT,
					TextSubjectQualifierList.QuotationInstructionInformation,
					MaxQuotaTypeChars,
					MaxQuotaTypeChars,
					invoiceHeader.QuarantineExDocHeader.QH_QuotaType);
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

		protected override void GenerateConsigneeRepresentativeName(SegmentGroup2 group2)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_ConsigneeAgentName.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.ConsigneesAgent,
					ZString.Empty,
					NameComponentQualifierList.WholeName,
					invoiceHeader.QuarantineExDocHeader.QH_ConsigneeAgentName);
			}
		}

		protected override void GenerateConsigneeRepresentativeName(SegmentGroup3 group3)
		{
		}

		protected override void GenerateRFPLines()
		{
			RequestForPermitMeatLineMessageBuilder meatLineMessageBuilder = new RequestForPermitMeatLineMessageBuilder(sANCRT, MessageTypeToSend);
			invoiceHeader.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Ascending);
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				meatLineMessageBuilder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
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
			if (!invoiceHeader.QuarantineExDocHeader.QH_TrueAndCompleteIndicator.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIS(sANCRT,
					invoiceHeader.QuarantineExDocHeader.QH_TrueAndCompleteIndicator.Left(1),
					TrueAndCompleteIndicator);
			}
		}

		protected override void GenerateAvAnimalAgeText()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_AvAnimalAge.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateFTX(
					sANCRT,
					TextSubjectQualifierList.AdditionalAttributeInformation,
					MaxAvAnimalAgeChars,
					MaxAvAnimalAgeChars,
					invoiceHeader.QuarantineExDocHeader.QH_AvAnimalAge);
			}
		}

		protected override void GenerateApprovedCertifier()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_ApprovedCertifier.IsEmpty)
			{
				SegmentGroup8 group8 = sANCRT.Group8.InstantiateAChildAndAddItToChildrenCollection();
				SegmentGroup9 group9 = group8.Group9.InstantiateAChildAndAddItToChildrenCollection();
				EXDOCMessageUtilities.PopulatePNA(group9.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.CertifyingParty,
					invoiceHeader.QuarantineExDocHeader.QH_ApprovedCertifier);
			}
		}

		#region Exclusions

		protected override void GenerateAQISRegion()
		{
		}

		protected override void GenerateLetterOfCredit()
		{
		}

		#endregion

		public const string ShipStoresIndicatorQualifier = "SST";
	}
}
