using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUOrgSupplierPart : OrgSupplierPart
		, Integration.Customs.AU.IOrgSupplierPart
		, IDutyDataFromInvoiceLineProvider
	{
		public AUOrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new AUOrgSupplierPart New(BusinessObjectFactory factory)
		{
			return (AUOrgSupplierPart)factory.
				// split for search/replace
				New(typeof(AUOrgSupplierPart));
		}

		public override void OnSaving()
		{
			if (RemoveNonEssentialValidationForBulkTariffUpdate && !hasSetConcurrencyPolicy && IsInDatabase)
			{
				hasSetConcurrencyPolicy = true;
				SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			}
			base.OnSaving();
		}
		bool hasSetConcurrencyPolicy;

		[ChildEditable(true)]
		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		[ChildEditable(true)]
		public new ClassificationCollection<Classification> ClassificationsForBinding => (ClassificationCollection<Classification>)base.ClassificationsForBinding;

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.Australia);

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<Classification>(this, Core.Constants.CountryCodes.Australia);

		#region Validation
		protected override MasterFiles.Business.OrgSupplierPartValidation GetNewValidation()
		{
			return new OrgSupplierPartValidation(this);
		}
		#endregion

		#region Business Object Overrides

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(PartUnits.ToArray());
				return result.ToArray();
			}
		}

		#endregion

		#region Implementation

		protected void PartUnits_CountChanged(object sender, EventArgs e)
		{
			Validation.ValidateOP_StockKeepingUnit();
		}

		#endregion

		#region IDutyDataFromInvoiceLineProvider Members

		DutyDataFromInvoiceLine IDutyDataFromInvoiceLineProvider.GetDutyDataFromInvoiceLine(ZDateTime dutyDate, ZGuid buyerPK, ZGuid supplierPK)
		{
			DutyDataFromInvoiceLine result = new DutyDataFromInvoiceLine();

			var pivot = GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia).GetImportMatch(Core.Constants.CountryCodes.Australia, buyerPK, supplierPK, dutyDate.Date) as CusClassPartPivot;
			if (pivot != null)
			{
				var importTariff = pivot.Classification?.CC_TariffNum ?? ZString.Empty;
				ZString tariffStatNumber = importTariff.Replace(" ", "").Replace(".", "");

				result.EffectiveDutyDate = dutyDate;
				result.FirstTariffNumber = tariffStatNumber.Left(8);
				result.StatCode = tariffStatNumber.Right(2);

				if (!tariffStatNumber.IsEmpty)
				{
					var effectiveAddInfo = pivot.EffectiveAddInfo;
					var statClassificationWrapper = ClassificationPeriodSnapshotWrapper.Load(Factory, result.FirstTariffNumber, result.StatCode, dutyDate);
					if (statClassificationWrapper != null)
					{
						result.FirstUQ = statClassificationWrapper.QuantityUnit;
						result.SecondUQ = statClassificationWrapper.SecondQuantityUnit;
					}

					result.ICN = effectiveAddInfo.ZA_ICN;
					result.IsGSTExempt = !effectiveAddInfo.ZA_GSTE.IsEmpty;
					result.IsLCTPayable = true;
					result.IsLCTExempt = effectiveAddInfo.ZA_LCTQ == "Y";
					result.IsWETExempt = !effectiveAddInfo.ZA_WETE.IsEmpty || effectiveAddInfo.ZA_WETQ == "Y";
					result.LCTE = effectiveAddInfo.ZA_LCTE;
					result.Preference = effectiveAddInfo.ZA_PST.IsEmpty ? "GEN" : effectiveAddInfo.ZA_PST.ToString();
					result.RateNumber = effectiveAddInfo.ZA_RNO;
					result.SecondTariffNumber = effectiveAddInfo.ZA_CL2.Replace(".", "");

					result.FirstTreatmentCode = effectiveAddInfo.ZA_TreatmentCode_Hidden;
					result.SecondTreatmentCode = effectiveAddInfo.ZA_TR2;
					result.TreatmentRateNumber = effectiveAddInfo.ZA_TRN;
				}
			}
			return result;
		}

		#endregion

		#region ClassPivotValues

		public ZString ImportTreatmentCode
		{
			get
			{
				var distinctTrtCodes = GetDistinctTreatmentCodeFromAllPivots();
				return GetDisplayValue(distinctTrtCodes);
			}
		}

		IEnumerable<ZString> GetDistinctTreatmentCodeFromAllPivots()
		{
			var trtCodeList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
				{
					if (!trtCodeList.Contains(pivot.EffectiveAddInfo.ZA_TreatmentCode_Hidden))
					{
						trtCodeList.Add(pivot.EffectiveAddInfo.ZA_TreatmentCode_Hidden);
						yield return pivot.EffectiveAddInfo.ZA_TreatmentCode_Hidden;
					}
				}
			}
		}

		public ZString InstrumentType
		{
			get
			{
				var distinctInstTypes = GetDistinctInstrumentTypeFromAllPivots();
				return GetDisplayValue(distinctInstTypes);
			}
		}

		IEnumerable<ZString> GetDistinctInstrumentTypeFromAllPivots()
		{
			var instTypeList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
				{
					if (!instTypeList.Contains(pivot.EffectiveAddInfo.ZA_InstrumentType_Hidden))
					{
						instTypeList.Add(pivot.EffectiveAddInfo.ZA_InstrumentType_Hidden);
						yield return pivot.EffectiveAddInfo.ZA_InstrumentType_Hidden;
					}
				}
			}
		}

		public ZString InstrumentCode
		{
			get
			{
				var distinctInstCodes = GetDistinctInstrumentCodeFromAllPivots();
				return GetDisplayValue(distinctInstCodes);
			}
		}

		IEnumerable<ZString> GetDistinctInstrumentCodeFromAllPivots()
		{
			var instCodeList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTI)
				{
					if (!instCodeList.Contains(pivot.EffectiveAddInfo.ZA_InstrumentCode_Hidden))
					{
						instCodeList.Add(pivot.EffectiveAddInfo.ZA_InstrumentCode_Hidden);
						yield return pivot.EffectiveAddInfo.ZA_InstrumentCode_Hidden;
					}
				}
			}
		}

		public ZString ExportClassificationLookup
		{
			get
			{
				var exportClassCodes = GetExpClassCodesFromAllPivots();
				return GetDisplayValue(exportClassCodes);
			}
		}

		IEnumerable<ZString> GetExpClassCodesFromAllPivots()
		{
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.Classification != null && pivot.IsExportClassification)
				{
					yield return pivot.Classification.CC_LookupCode;
				}
			}
		}

		public ZString ImportClassificationLookup
		{
			get
			{
				var importClassCodes = GetImpClassCodesFromAllPivots();
				return GetDisplayValue(importClassCodes);
			}
		}

		IEnumerable<ZString> GetImpClassCodesFromAllPivots()
		{
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.Classification != null && pivot.IsImportClassification)
				{
					yield return pivot.Classification.CC_LookupCode;
				}
			}
		}

		public ZString AddInfoLine
		{
			get
			{
				var addInfoLines = GetAddInfoLinesFromAllPivots();
				return GetDisplayValue(addInfoLines);
			}
		}

		IEnumerable<ZString> GetAddInfoLinesFromAllPivots()
		{
			var addInfoLinesList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				var addInfoLine = pivot.EffectiveAddInfo?.AddInfoLine ?? ZString.Empty;
				if (!addInfoLine.IsEmpty && !addInfoLinesList.Contains(addInfoLine))
				{
					addInfoLinesList.Add(addInfoLine);
					yield return addInfoLine;
				}
			}
		}

		public ZString RFPProduceType
		{
			get
			{
				var distinctProduceTypes = GetDistinctProduceTypeFromAllPivots();
				return GetDisplayValue(distinctProduceTypes);
			}
		}

		IEnumerable<ZString> GetDistinctProduceTypeFromAllPivots()
		{
			var produceTypeList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTE)
				{
					if (!produceTypeList.Contains(pivot.AddInfo.ZA_AQISProduceType_Hidden))
					{
						produceTypeList.Add(pivot.AddInfo.ZA_AQISProduceType_Hidden);
						yield return pivot.AddInfo.ZA_AQISProduceType_Hidden;
					}
				}
			}
		}

		public ZString RFPProduct
		{
			get
			{
				var distinctProducts = GetDistinctProductFromAllPivots();
				return GetDisplayValue(distinctProducts);
			}
		}

		IEnumerable<ZString> GetDistinctProductFromAllPivots()
		{
			var productList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTE)
				{
					if (!productList.Contains(pivot.AddInfo.ZA_AQISProduct_Hidden))
					{
						productList.Add(pivot.AddInfo.ZA_AQISProduct_Hidden);
						yield return pivot.AddInfo.ZA_AQISProduct_Hidden;
					}
				}
			}
		}

		public ZString RFPSupplementaryCode
		{
			get
			{
				var distinctSupplementaryCodes = GetDistinctSupplementaryCodeFromAllPivots();
				return GetDisplayValue(distinctSupplementaryCodes);
			}
		}

		IEnumerable<ZString> GetDistinctSupplementaryCodeFromAllPivots()
		{
			var supplementaryCodeList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTE)
				{
					if (!supplementaryCodeList.Contains(pivot.AddInfo.ZA_AQISSupplementaryCode_Hidden))
					{
						supplementaryCodeList.Add(pivot.AddInfo.ZA_AQISSupplementaryCode_Hidden);
						yield return pivot.AddInfo.ZA_AQISSupplementaryCode_Hidden;
					}
				}
			}
		}

		public ZString RFPPackType
		{
			get
			{
				var distinctPackTypes = GetDistinctPackTypeFromAllPivots();
				return GetDisplayValue(distinctPackTypes);
			}
		}

		IEnumerable<ZString> GetDistinctPackTypeFromAllPivots()
		{
			var packTypeList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTE)
				{
					if (!packTypeList.Contains(pivot.AddInfo.ZA_AQISPackType_Hidden))
					{
						packTypeList.Add(pivot.AddInfo.ZA_AQISPackType_Hidden);
						yield return pivot.AddInfo.ZA_AQISPackType_Hidden;
					}
				}
			}
		}

		public ZString RFPPreservation
		{
			get
			{
				var distinctPreservations = GetDistinctPreservationFromAllPivots();
				return GetDisplayValue(distinctPreservations);
			}
		}

		IEnumerable<ZString> GetDistinctPreservationFromAllPivots()
		{
			var preservationeList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTE)
				{
					if (!preservationeList.Contains(pivot.AddInfo.ZA_AQISPreservation_Hidden))
					{
						preservationeList.Add(pivot.AddInfo.ZA_AQISPreservation_Hidden);
						yield return pivot.AddInfo.ZA_AQISPreservation_Hidden;
					}
				}
			}
		}

		public ZString RFPCutCode
		{
			get
			{
				var distinctCutCodes = GetDistinctCutCodeFromAllPivots();
				return GetDisplayValue(distinctCutCodes);
			}
		}

		IEnumerable<ZString> GetDistinctCutCodeFromAllPivots()
		{
			var cutCodeList = new List<ZString>();
			foreach (CusClassPartPivot pivot in GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
			{
				if (pivot.CI_ChildType == ClassificationTypeList.Codes.HTE)
				{
					if (!cutCodeList.Contains(pivot.AddInfo.ZA_AQISCutCode_Hidden))
					{
						cutCodeList.Add(pivot.AddInfo.ZA_AQISCutCode_Hidden);
						yield return pivot.AddInfo.ZA_AQISCutCode_Hidden;
					}
				}
			}
		}

		ZString GetDisplayValue(IEnumerable<ZString> list)
		{
			var distinctDatas = list.Take(2).ToList();
			return distinctDatas.Count == 0 ? "" : distinctDatas.Count > 1 ? MultipleValues : distinctDatas[0].ToString();
		}

		#endregion

	}
}
