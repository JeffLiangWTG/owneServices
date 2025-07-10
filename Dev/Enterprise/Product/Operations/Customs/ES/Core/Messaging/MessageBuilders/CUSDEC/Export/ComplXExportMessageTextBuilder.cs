using CargoWise.Types;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Edifact.V921ES.Segments;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	internal static class ComplXExportMessageTextBuilder
	{
		internal static void PopulateCUSDECMessage(CUSDECMessage message, IComplXExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.PopulateUNHSegment(message.UNH[0], MessageReleaseNumberList.TrialRelease1992, "ECS003");
			CommonMessageTextBuilder.PopulateBGMSegment(message.BGM[0], DocumentMessageNameCodedList.GoodsDeclarationForExportation, source.LocalReferenceNumber, MessageFunctionCodedList.GetFromString(source.MessageType));
			PopulateCSTSegment(message.CST[0], source);
			PopulateSG6Groups(message.Group6, source);
			PopulateSG7Group(message.Group7, source);
			PopulateSG8Group(message.Group8, source);
			CommonMessageTextBuilder.PopulateUNS1Segment(message.UNS1[0]);
			PopulateSG30Group(message.Group30, source);
			CommonMessageTextBuilder.PopulateUNS2Segment(message.UNS2[0]);
			PopulateCNTSegments(message.CNT, source);
			CommonMessageTextBuilder.PopulateUNTSegment(message.UNT[0], message.CountIncludingUNT);
		}

		static void PopulateCSTSegment(CSTSegment cstSegment, IComplXExportMessageDataProvider source)
		{
			cstSegment.CustomsIdentityCodes2.CustomsCodeIdentification = "X";
			cstSegment.CustomsIdentityCodes2.CodeListQualifier = CodeListQualifierList.CustomsDeclarationType;
			cstSegment.CustomsIdentityCodes2.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;

			cstSegment.CustomsIdentityCodes5.CustomsCodeIdentification = source.CustomsProcedureCategory5;
			cstSegment.CustomsIdentityCodes5.CodeListQualifier = CodeListQualifierList.CustomsOffice;
			cstSegment.CustomsIdentityCodes5.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.EsSpanishCustoms;
		}

		static void PopulateSG6Groups(SegmentGroup6MessageSection sg6Section, IComplXExportMessageDataProvider source)
		{
			CommonExportMessageTextBuilder.AddNewNADWithEmailInSG6Group(sg6Section, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.Declarant); //NAD+Declarant
		}

		static void PopulateSG7Group(SegmentGroup7MessageSection sg7Section, IComplXExportMessageDataProvider source)
		{
			CommonExportMessageTextBuilder.AddNewSG7Group(sg7Section, source.TermsOfDeliveryCode, source.DeliveryLocation, ZString.Empty);
		}

		static void PopulateSG8Group(SegmentGroup8MessageSection sg8Section, IComplXExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewMOAInSG8Group(sg8Section, MonetaryAmountTypeQualifierList.InvoiceTotalAmount, source.TotalAmount, source.TotalAmountCurrencyCode);
		}

		static void PopulateSG30Group(SegmentGroup30MessageSection sg30Section, IComplXExportMessageDataProvider source)
		{
			foreach (IComplXExportLine line in source.Lines)
			{
				AddNewSG30Group(sg30Section, line);
			}
		}

		static void AddNewSG30Group(SegmentGroup30MessageSection sg30Section, IComplXExportLine source)
		{
			var sg30 = sg30Section.InstantiateAChildAndAddItToChildrenCollection();
			AddCSTSegmentOnSG30(sg30.CST[0], source);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.GrossWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 3, MeasurementDimensionCodedList.GrossWeight);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.NetWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 3, MeasurementDimensionCodedList.NetNetWeight);
			CommonMessageTextBuilder.AddNewMEASegment(sg30, source.OtherUnitsNumber, source.OtherUnitsQualifier, MeasurementApplicationQualifierList.Measurement);
			CommonMessageTextBuilder.AddNewMOASegmentSG33Group(sg30.Group33, MonetaryAmountTypeQualifierList.StatisticalValue, source.TotalGoodValueInEuros);
			AddNewSG37Groups(sg30.Group37, source);
		}

		static void AddCSTSegmentOnSG30(CSTSegment cstSegment, IComplXExportLine source)
		{
			cstSegment.GoodsItemNumber = Utilities.FormatNumberFromZIntNational(source.GoodsItemNumber, 0);
		}

		static void AddNewSG37Groups(SegmentGroup37MessageSection sg37Section, IComplXExportLine source)
		{
			foreach (var doc in source.Documents)
			{
				CommonExportMessageTextBuilder.AddNewSG37(sg37Section, doc.Name, doc.Number, ZString.Empty, doc.DateOfIssue, doc.DateOfExpiry);
			}
		}

		static void PopulateCNTSegments(CNTSegmentMessageSection cntSection, IComplXExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, Utilities.FormatNumberFromZIntNational(source.TotalNumberOfGoods, 0), ControlQualifierList.NumberOfCustomsItemDetailLines);
		}
	}
}
