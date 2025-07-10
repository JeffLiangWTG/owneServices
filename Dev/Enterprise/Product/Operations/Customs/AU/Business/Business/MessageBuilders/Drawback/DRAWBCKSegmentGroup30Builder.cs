using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DRWBCKSegmentGroup30Builder
	{
		public DRWBCKSegmentGroup30Builder(JobComInvoiceLine jobComInvLine, SegmentGroup30 group30, int lineNumber)
		{
			this.Group30 = group30;
			this.LineNumber = lineNumber;
			this.JobComInvLine = jobComInvLine;
		}
		protected readonly SegmentGroup30 Group30;
		protected readonly int LineNumber;
		protected readonly JobComInvoiceLine JobComInvLine;

		public void PopulateSegment()
		{
			PopulateCST(LineNumber);
			PopulateFTX();
			PopulateMEA();
			PopulateGroup33MOA();
			PopulateRFF();
			PopulateTAX();
			PopulateGroup41MOA();
		}

		protected void PopulateCST(int lineNumber)
		{
			string lineActionCode = "I";
			string lineNum = (lineNumber + 1).ToString();
			MessageUtilities.PopulateCST(Group30.CST[0], lineNum, lineActionCode);
		}

		protected void PopulateFTX()
		{
			if (!JobComInvLine.JI_Description.IsEmpty)
			{
				FTXSegment fTX1 = Group30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX1.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GoodsDescription;
				fTX1.TextLiteral.FreeTextValue1 = JobComInvLine.JI_Description.Left(250);
			}
			if (!JobComInvLine.AddInfo.ZA_DARC_Hidden.IsEmpty)
			{
				FTXSegment fTX2 = Group30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX2.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.TariffStatements;
				fTX2.TextReference.FreeTextValueCode = JobComInvLine.AddInfo.ZA_DARC_Hidden;
				fTX2.TextReference.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}

		protected void PopulateMEA()
		{
			ZString netQuantity = ZArchitecture.Core.Utilities.Round(JobComInvLine.JI_CustomsQuantity, 5).ToString();
			ZString netQuantityUnit = JobComInvLine.JI_CustomsUnitQty.Trim();
			if (!netQuantityUnit.IsEmpty || !JobComInvLine.JI_CustomsQuantity.IsEmpty)
			{
				MessageUtilities.PopulateMEA(Group30.MEA[0], MeasurementAttributeCodeList.LineItemMeasurement, null, netQuantityUnit, netQuantity);
			}
		}

		protected void PopulateGroup33MOA()
		{
			SegmentGroup33 group33 = Group30.Group33.InstantiateAChildAndAddItToChildrenCollection();
			ZDecimal amount = JobComInvLine.DrawbackCustomsValue;
			if (!amount.IsEmpty)
			{
				MOASegment mOA = group33.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.CustomsValue;
				mOA.MonetaryAmount.MonetaryAmountValue = amount.ToString(2);
			}
		}

		protected void PopulateRFF()
		{
			SegmentGroup35 group35 = Group30.Group35.InstantiateAChildAndAddItToChildrenCollection();
			PopulateRFF(group35, ReferenceFunctionCodeQualifierList.CustomsTariffNumber, JobComInvLine.TariffNumber);
			PopulateRFF(group35, ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber, JobComInvLine.DrawbackImportDeclarationNumber, JobComInvLine.DrawbackImportDeclarationLine.ToString());
			PopulateRFF(group35, ReferenceFunctionCodeQualifierList.ApplicableInstructionsOrStandards, JobComInvLine.DrawbackAssesmentMethod);
			PopulateRFF(group35, ReferenceFunctionCodeQualifierList.ExportDeclaration, JobComInvLine.ExportDeclarationNumber);
		}

		protected void PopulateRFF(SegmentGroup35 group35, ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, ZString referenceIdentifier)
		{
			PopulateRFF(group35, referenceFunctionCodeQualifier, referenceIdentifier, null);
		}

		protected void PopulateRFF(SegmentGroup35 group35, ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, ZString referenceIdentifier, ZString lineNumber)
		{
			if (!referenceIdentifier.IsEmpty)
			{
				RFFSegment rFF = group35.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceFunctionCodeQualifier = referenceFunctionCodeQualifier;
				rFF.Reference.ReferenceIdentifier = referenceIdentifier;
				rFF.Reference.LineNumber = lineNumber;
			}
		}

		protected void PopulateTAX()
		{
			ZDecimal value = JobComInvLine.DrawbackDutyRate;
			if (!value.IsEmpty)
			{
				TAXSegment tAX = Group41.TAX.InstantiateAChildAndAddItToChildrenCollection();
				tAX.DutyTaxFeeFunctionQualifier = DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem;
				tAX.DutyTaxFeeType.DutyTaxFeeTypeNameCode = DutyTaxFeeTypeNameCodeList.CustomsDuty;
				tAX.DutyTaxFeeDetail.DutyTaxFeeRate = value.ToString(2);
			}
		}

		protected void PopulateGroup41MOA()
		{
			ZDecimal amount = JobComInvLine.DrawbackDutyAmount;
			if (!amount.IsEmpty)
			{
				MOASegment mOA = Group41.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.DutyAmount;
				mOA.MonetaryAmount.MonetaryAmountValue = amount.ToString(2);
			}
		}

		SegmentGroup41 Group41
		{
			get
			{
				if (group41 == null)
				{
					group41 = Group30.Group41.InstantiateAChildAndAddItToChildrenCollection();
				}
				return group41;
			}
		}
		SegmentGroup41 group41;
	}
}
