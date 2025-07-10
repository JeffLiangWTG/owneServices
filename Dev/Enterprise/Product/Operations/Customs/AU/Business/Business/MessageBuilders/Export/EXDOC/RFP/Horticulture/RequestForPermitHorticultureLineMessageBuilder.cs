using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitHorticultureLineMessageBuilder : RequestForPermitLineMessageBuilder
	{
		public RequestForPermitHorticultureLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend)
			: base(sANCRT, messageTypeToSend)
		{
		}

		protected override void GenerateCutType(SegmentGroup11 group11)
		{
		}

		protected override void GenerateProductSourceState(SegmentGroup11 group11)
		{
			if (!invoiceLine.JI_AUState.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(group11.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.MutuallyDefined,
					invoiceLine.JI_AUState);
			}
		}

		protected override void GenerateAdditionalDeclarationInformation(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_AddtionalDeclarationComments.IsEmpty)
			{
				const int MaxAdditionalDeclarationInformationChars = 34650;
				const int MaxAdditionalDeclarationInformationElementLength = 70;
				EXDOCMessageUtilities.PopulateFTX(group11,
					TextSubjectQualifierList.AdditionalExportInformation,
					MaxAdditionalDeclarationInformationChars,
					MaxAdditionalDeclarationInformationElementLength,
					invoiceLine.QuarantineExDocLine.QL_AddtionalDeclarationComments);
			}
		}

		protected override void GenerateCodedStatement(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_StatementNumber1.IsEmpty || !invoiceLine.QuarantineExDocLine.QL_StatementNumber2.IsEmpty ||
				!invoiceLine.QuarantineExDocLine.QL_StatementNumber3.IsEmpty || !invoiceLine.QuarantineExDocLine.QL_StatementNumber4.IsEmpty ||
				!invoiceLine.QuarantineExDocLine.QL_StatementNumber5.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateFTX(group11,
					TextSubjectQualifierList.MutuallyDefined,
					invoiceLine.QuarantineExDocLine.QL_StatementNumber1.IsEmpty ? string.Empty : invoiceLine.QuarantineExDocLine.QL_StatementNumber1.ToString(),
					invoiceLine.QuarantineExDocLine.QL_StatementNumber2.IsEmpty ? string.Empty : invoiceLine.QuarantineExDocLine.QL_StatementNumber2.ToString(),
					invoiceLine.QuarantineExDocLine.QL_StatementNumber3.IsEmpty ? string.Empty : invoiceLine.QuarantineExDocLine.QL_StatementNumber3.ToString(),
					invoiceLine.QuarantineExDocLine.QL_StatementNumber4.IsEmpty ? string.Empty : invoiceLine.QuarantineExDocLine.QL_StatementNumber4.ToString(),
					invoiceLine.QuarantineExDocLine.QL_StatementNumber5.IsEmpty ? string.Empty : invoiceLine.QuarantineExDocLine.QL_StatementNumber5.ToString());
			}
		}

		protected override void GenerateFreeTextStatement(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_StatementText.IsEmpty)
			{
				const int MaxFreeTextStatementChars = 350;
				const int MaxFreeTextStatementElementLength = 70;
				EXDOCMessageUtilities.PopulateFTX(group11,
					TextSubjectQualifierList.CertificationStatements,
					MaxFreeTextStatementChars,
					MaxFreeTextStatementElementLength,
					invoiceLine.QuarantineExDocLine.QL_StatementText);
			}
		}

		protected override void GenerateGrowerNumber(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_GrowerNumber.IsEmpty)
			{
				const int MaxGrowerNumberChars = 25;
				const int MaxGrowerNumberElementLength = 25;
				EXDOCMessageUtilities.PopulateFTX(group11,
					TextSubjectQualifierList.AdditionalMarksNumbersInformation,
					MaxGrowerNumberChars,
					MaxGrowerNumberElementLength,
					invoiceLine.QuarantineExDocLine.QL_GrowerNumber);
			}
		}

		protected override void GenerateCustomsWeights(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightUQ.IsEmpty && invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeight > 0)
			{
				MEASegment mea = group11.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mea.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.CustomsLineItemMeasurement;  //  MEA 6311, AAF
				mea.MeasurementDetails.PropertyMeasuredCoded = PropertyMeasuredCodedList.ShippedQuantity;  //  MEA C502/6313, SQ
				mea.ValueRange.MeasureUnitQualifier = invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightUQ; // MEA C174/6411
				mea.ValueRange.MeasurementValue = invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeight.ToString(); // MEA C174/6314
			}
		}
	}
}
