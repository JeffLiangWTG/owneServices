using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCMessageDecoderContainer
	{
		public EXDOCMessageDecoderContainer(SegmentGroup16 group16)
		{
			this.group16 = group16;
		}

		public void Process()
		{
			ProcessEQD();
			ProcessMEA();
			ProcessIMD();
			ProcessGIN();
			ProcessDTM();
			ProcessFTX();
			ProcessGroup17();
		}

		void ProcessEQD()
		{
			foreach (EQDSegment eQD in group16.EQD)
			{
				if (eQD.EquipmentQualifier == EquipmentQualifierList.Container)
				{
					containerNumber = eQD.EquipmentIdentification.EquipmentIdentificationNumber;
				}
			}
		}

		void ProcessMEA()
		{
			foreach (MEASegment mEA in group16.MEA)
			{
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.Weights &&
						mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.TotalNetWeight)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out iMA1NetWeight);
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.Weights &&
						mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.ItemGrossWeight)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out iMA1GrossWeight);
				}
			}
		}

		void ProcessIMD()
		{
			foreach (IMDSegment iMD in group16.IMD)
			{
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitDairyLineMessageBuilder.IMA1ProductDescription)
				{
					iMA1ProductDescription += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2;
				}
			}
		}

		void ProcessGIN()
		{
			foreach (GINSegment gIN in group16.GIN)
			{
				if (gIN.IdentityNumberQualifier == IdentityNumberQualifierList.SerialNumber)
				{
					iMA1SerialNumber += gIN.IdentityNumberRange1.IdentityNumber1 + gIN.IdentityNumberRange1.IdentityNumber2;
				}
				if (gIN.IdentityNumberQualifier == IdentityNumberQualifierList.InvoiceLineNumber)
				{
					iMA1InvoiceNumber += gIN.IdentityNumberRange1.IdentityNumber1 + gIN.IdentityNumberRange1.IdentityNumber2;
				}
			}
		}

		void ProcessDTM()
		{
			foreach (DTMSegment dTM in group16.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.InvoiceDateTime)
				{
					ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out iMA1InvoiceDate, "yyyyMMdd");
				}
			}
		}

		void ProcessFTX()
		{
			foreach (FTXSegment fTX in group16.FTX)
			{
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.GeneralInformation)
				{
					iMA1QuotaYear += fTX.TextLiteral.FreeText1 + fTX.TextLiteral.FreeText2 + fTX.TextLiteral.FreeText3 + fTX.TextLiteral.FreeText4 + fTX.TextLiteral.FreeText5;
				}
			}
		}

		void ProcessGroup17()
		{
			foreach (SegmentGroup17 group17 in group16.Group17)
			{
				ProcessSEL(group17);
			}
		}

		void ProcessSEL(SegmentGroup17 group17)
		{
			foreach (SELSegment sEL in group17.SEL)
			{
				containerSeal = sEL.SealNumber;
			}
		}

		public ZString containerSeal;
		public ZString iMA1QuotaYear;
		public ZDateTime iMA1InvoiceDate;
		public ZString iMA1InvoiceNumber;
		public ZString iMA1SerialNumber;
		public ZString iMA1ProductDescription;
		public ZDecimal iMA1GrossWeight;
		public ZDecimal iMA1NetWeight;
		public ZString containerNumber;
		readonly SegmentGroup16 group16;
	}
}
