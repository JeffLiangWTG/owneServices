using System.Globalization;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitDairyLineMessageBuilder : RequestForPermitLineMessageBuilder
	{
		public RequestForPermitDairyLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend)
			: base(sANCRT, messageTypeToSend)
		{
		}

		protected override void GenerateBatchCode(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_BatchCode.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIN(group11.GIN.InstantiateAChildAndAddItToChildrenCollection(),
					IdentityNumberQualifierList.BatchNumber,
					invoiceLine.QuarantineExDocLine.QL_BatchCode);
			}
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

		protected override void GeneratePercentageOfMilkProtein(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_PercentOfMilkProtein.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.GetFromString(MilkProtein),
					Percentage,
					invoiceLine.QuarantineExDocLine.QL_PercentOfMilkProtein.ToString(2));
			}
		}

		protected override void GeneratePercentageOfMilkFat(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_PercentOfMilkFat.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.GetFromString(MilkFat),
					Percentage,
					invoiceLine.QuarantineExDocLine.QL_PercentOfMilkFat.ToString(2));
			}
		}

		protected override void GenerateTotalWeightOfMilkProteinInMixtures(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.GetFromString(MilkProtein),
					PropertyMeasuredCodedList.NetWeight,
					EXDOCMetricWeightUnitCodes.Codes.Kilogram,
					invoiceLine.QuarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures.ToString(2));
			}
		}

		protected override void GenerateTotalWeightOfMilkFatInMixtures(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.GetFromString(MilkFat),
					PropertyMeasuredCodedList.NetWeight,
					EXDOCMetricWeightUnitCodes.Codes.Kilogram,
					invoiceLine.QuarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures.ToString(2));
			}
		}

		protected override void GenerateAMLCPerformanceExporterNumber(SegmentGroup13 group13)
		{
			if (invoiceLine.InvoiceHeader.Supplier != null)
			{
				ZString aMLCExporterNumber = invoiceLine.InvoiceHeader.Supplier.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber, invoiceLine.Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia));
				if (!aMLCExporterNumber.IsEmpty)
				{
					EXDOCMessageUtilities.PopulatePNA(group13.PNA.InstantiateAChildAndAddItToChildrenCollection(),
						PartyQualifierList.Exporter,
						aMLCExporterNumber);
				}
			}
		}

		protected override void GenerateIMA1NetWeight(SegmentGroup16 group16, CusContainerInvoiceLinePivot pivot)
		{
			if (!pivot.C2_NetWeight.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group16.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Weights,
										PropertyMeasuredCodedList.TotalNetWeight,
					pivot.C2_NetWeight.ToString(3));
			}
		}

		protected override void GenerateIMA1GrossWeight(SegmentGroup16 group16, CusContainerInvoiceLinePivot pivot)
		{
			if (!pivot.C2_GrossWeight.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group16.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Weights,
					PropertyMeasuredCodedList.ItemGrossWeight,
					pivot.C2_GrossWeight.ToString(3));
			}
		}

		protected override void GenerateIMA1ProductDescription(SegmentGroup16 group16)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_IMA1ProductDesciption.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group16.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					IMA1ProductDescription,
					invoiceLine.QuarantineExDocLine.QL_IMA1ProductDesciption);
			}
		}

		protected override void GenerateIMA1SerialNumber(SegmentGroup16 group16)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_IMA1SerialNumber.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIN(group16.GIN.InstantiateAChildAndAddItToChildrenCollection(),
					IdentityNumberQualifierList.SerialNumber,
					invoiceLine.QuarantineExDocLine.QL_IMA1SerialNumber);
			}
		}

		protected override void GenerateIMA1InvoiceNumber(SegmentGroup16 group16)
		{
			if (!invoiceLine.InvoiceHeader.JZ_InvoiceNumber.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIN(group16.GIN.InstantiateAChildAndAddItToChildrenCollection(),
					IdentityNumberQualifierList.InvoiceLineNumber,
					invoiceLine.InvoiceHeader.JZ_InvoiceNumber);
			}
		}

		protected override void GenerateIMA1InvoiceDate(SegmentGroup16 group16)
		{
			if (!invoiceLine.InvoiceHeader.JZ_InvoiceDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group16.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.InvoiceDateTime,
					invoiceLine.InvoiceHeader.JZ_InvoiceDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected override void GenerateIMA1QuotaYear(SegmentGroup16 group16)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_IMA1QuotaYear.IsEmpty)
			{
				const int MaxQuotaYearChars = 15;
				const int MaxQuotaYearElementLength = 15;
				EXDOCMessageUtilities.PopulateFTX(group16,
					TextSubjectQualifierList.GeneralInformation,
					MaxQuotaYearChars,
					MaxQuotaYearElementLength,
					invoiceLine.QuarantineExDocLine.QL_IMA1QuotaYear);
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

		public const string MilkFat = "MF";
		public const string MilkProtein = "MP";
		public const string IMA1ProductDescription = "IMA1";
	}
}
