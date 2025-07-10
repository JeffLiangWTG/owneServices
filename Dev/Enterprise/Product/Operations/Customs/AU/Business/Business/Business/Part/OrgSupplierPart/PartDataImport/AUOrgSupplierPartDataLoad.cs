using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUOrgSupplierPartDataLoad : ECB.OrgSupplierPartDataLoad, Integration.Customs.AU.IAUCusOrgSupplierPartDataLoad
	{
		public class PartsDataToLoad : MasterFiles.Business.PartsDataToLoad
		{
			public ZString Pref_Origin;
			public ZString Pref_Rule;
			public ZString Pref_Scheme;
			public ZString Pref_InstrumentType;
			public ZString Pref_InstrumentNo;
			public ZString PartAddInfo;
		}

		protected override IEnumerable<string> GetFieldNames()
		{
			List<string> result = new List<string>();
			result.Add("Pref_Origin");
			result.Add("Pref_Rule");
			result.Add("Pref_Scheme");
			result.Add("Pref_InstrumentType");
			result.Add("Pref_InstrumentNo");
			return result;
		}

		new static class FieldNames
		{
			public const string Pref_Origin = "Pref_Origin";
			public const string Pref_Rule = "Pref_Rule";
			public const string Pref_Scheme = "Pref_Scheme";
			public const string Pref_InstrumentType = "Pref_InstrumentType";
			public const string Pref_InstrumentNo = "Pref_InstrumentNo";
		}

		protected override MasterFiles.Business.PartsDataToLoad GetPartsDataToLoad()
		{
			return new PartsDataToLoad();
		}

		protected virtual ZString BuildAddInfoData(PartsDataToLoad record)
		{
			ZString addInfo = ZString.Empty;
			if (!record.Pref_Rule.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "PRT=" + record.Pref_Rule);
			}

			if (!record.Pref_InstrumentType.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "PRI=" + record.Pref_InstrumentType);
				if (!record.Pref_InstrumentNo.IsEmpty)
				{
					addInfo = addInfo + ":" + record.Pref_InstrumentNo;
				}
			}

			if (!record.Pref_Origin.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "POC=" + record.Pref_Origin);
			}

			if (!record.Pref_Scheme.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "PST=" + record.Pref_Scheme);
			}

			if (!record.PartOrigin.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "ORG=" + record.PartOrigin);
			}

			return addInfo;
		}

		protected ZString AppendAddInfo(ZString addInfo, ZString newAddInfo)
		{
			return addInfo == "" ? newAddInfo : addInfo += "*" + newAddInfo;
		}

		void AddCusClassPartPivot(OrgSupplierPart enterprisePart, ZGuid cusClassPK, string importOrExport, ZString addInfo)
		{
			ZQuery cusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_CC, cusClassPK);
			cusClassPartFilter.AddToFilter(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
			var cusClassPartLink = enterprisePart.Factory.LoadTop1<CusClassPartPivot>(cusClassPartFilter);
			if (cusClassPartLink == null)
			{
				CusClassPartPivot newCusClassPartLink = (CusClassPartPivot)enterprisePart.Factory.New(typeof(CusClassPartPivot));
				newCusClassPartLink.CI_CC = cusClassPK;
				newCusClassPartLink.CI_OP = enterprisePart.PK;
				newCusClassPartLink.CI_AddInfo = addInfo;
				var pivotType = ZString.Empty;
				switch (importOrExport)
				{
					case Classification.ClassificationType.IMP:
						pivotType = ECB.ClassificationTypeList.Codes.HTI;
						break;
					case Classification.ClassificationType.EXP:
						pivotType = ECB.ClassificationTypeList.Codes.HTE;
						break;
				}
				newCusClassPartLink.CI_ChildType = pivotType;
			}
		}

		protected override ZGuid GetClassificationPK(string partClassCode, string classType)
		{
			ZGuid result = base.GetClassificationPK(partClassCode, classType);

			ZQuery classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, partClassCode);
			classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classType);
			classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
			var classification = Factory.LoadTop1<Classification>(classFilter);
			if (classification != null)
			{
				result = classification.PK;
			}

			return result;
		}

		protected override string GetUQFromTariff(ZString classificationLookup, ZString classType)
		{
			var tariffUQ = base.GetUQFromTariff(classificationLookup, classType);

			var classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, classificationLookup);
			classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classType);
			classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
			var enterpriseClass = Factory.LoadTop1<Classification>(classFilter);
			if (enterpriseClass != null)
			{
				var importTariff = AUCClassWrapper.Load(Factory, enterpriseClass.CC_TariffNum, ZDateTime.Today);
				if (importTariff != null)
				{
					tariffUQ = importTariff.ZZ1_ZZ8_UQ1.ToUpper();
				}
			}

			return tariffUQ;
		}

		protected override void LoadCountrySpecificDataForLookup(OrgSupplierPart enterprisePart, ZString lookupCode, ZString importOrExport, MasterFiles.Business.PartsDataToLoad dataToLoad)
		{
			ZGuid classificationPK = GetClassificationPK(lookupCode, importOrExport);
			if (!classificationPK.IsEmpty)
			{
				AddAddInfoData(enterprisePart, importOrExport, dataToLoad, classificationPK);
			}
		}

		protected override void LoadCountrySpecificDataForTariffNum(OrgSupplierPart enterprisePart, ZString tariffNum, ZString importOrExport, MasterFiles.Business.PartsDataToLoad dataToLoad)
		{
			base.LoadCountrySpecificDataForTariffNum(enterprisePart, tariffNum, importOrExport, dataToLoad);
			AddAddInfoData(enterprisePart, importOrExport, dataToLoad, ZGuid.Empty);
		}

		void AddAddInfoData(OrgSupplierPart enterprisePart, ZString importOrExport, MasterFiles.Business.PartsDataToLoad dataToLoad, ZGuid classificationPK)
		{
			ZString addInfo = ZString.Empty;
			if (importOrExport == Classification.ClassificationType.IMP)
			{
				addInfo = BuildAddInfoData((PartsDataToLoad)dataToLoad);
			}

			var childType = importOrExport == Classification.ClassificationType.IMP ? "HTI" : "HTE";
			var pivotsQuery = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
			pivotsQuery.AddToFilter(CusClassPartPivotSchema.CI_ChildType, childType);
			BusinessObject[] pivots = (BusinessObject[])Factory.Load<Integration.Customs.IBaseCusClassPartPivot>(pivotsQuery);
			if (pivots.Length == 0)
			{
				AddCusClassPartPivot(enterprisePart, classificationPK, importOrExport, addInfo);
			}
			else
			{
				var pivot = (CusClassPartPivot)pivots[0];
				var lookup = Factory.Load<Classification>(classificationPK);
				if (lookup != null && lookup.CC_TariffNum != pivot.CI_TariffNum)
				{
					pivot.CI_TariffNum = ZString.Empty;
					pivot.CI_CC = lookup.PK;
				}

				if (addInfo != pivot.CI_AddInfo)
				{
					if (pivot.CI_AddInfo.Length > 0)
					{
						string previousAddInfoValue = "Previous Add Info for this part: " + pivot.CI_AddInfo;
						enterprisePart.Notes.AddNew(true, "Data Import Update: Previous AddInfo Details", previousAddInfoValue);
					}

					pivot.CI_AddInfo = addInfo;
				}
			}
		}

		protected override void SaveAndClearPreviousClassificationLookupDetailsIfPresentAndDifferent(OrgSupplierPart enterprisePart, ZString importLookup, ZString exportLookup, MasterFiles.Business.PartsDataToLoad partData)
		{
			var part = (AUOrgSupplierPart)enterprisePart;

			if (!importLookup.IsEmpty)
			{
				Classification existingImportClassification = GetClassification(part, Classification.ClassificationType.IMP);
				ZGuid newImpClassificationPK = GetClassificationPK(importLookup, Classification.ClassificationType.IMP);
				if (existingImportClassification != null && newImpClassificationPK != ZGuid.Empty)
				{
					SaveDetailsAndClearIfChanged(enterprisePart, existingImportClassification, newImpClassificationPK, GetPivotPK(part, Classification.ClassificationType.IMP), Classification.ClassificationType.IMP);
				}
			}

			if (!exportLookup.IsEmpty)
			{
				Classification existingExportClassification = GetClassification(part, Classification.ClassificationType.EXP);
				ZGuid newExpClassificationPK = GetClassificationPK(exportLookup, Classification.ClassificationType.EXP);
				if (existingExportClassification != null && newExpClassificationPK != ZGuid.Empty)
				{
					SaveDetailsAndClearIfChanged(enterprisePart, existingExportClassification, newExpClassificationPK, GetPivotPK(part, Classification.ClassificationType.EXP), Classification.ClassificationType.EXP);
				}
			}
		}

		#region Implementation

		void SaveDetailsAndClearIfChanged(OrgSupplierPart enterprisePart, Classification currentClass, ZGuid newClassPK, ZGuid pivotPK, string importExport)
		{
			if (currentClass.PK != newClassPK)
			{
				string partAddInfo = "";
				var partPivotLink = (CusClassPartPivot)enterprisePart.Factory.Load(typeof(CusClassPartPivot), pivotPK);
				if (partPivotLink != null)
				{
					partAddInfo = partPivotLink.CI_AddInfo.Replace("_Hidden", "");
					partPivotLink.CI_CC = newClassPK;
				}

				string lookup = (importExport == Classification.ClassificationType.IMP ? "Previous Import Lookup: " : "Previous Export Lookup: ") + currentClass.CC_LookupCode;
				string addInfo = (importExport == Classification.ClassificationType.IMP && partAddInfo.Length > 0) ? "Part Add Info: " + partAddInfo : "";
				enterprisePart.Notes.AddNew(true, "Data Import Update: Previous Lookup Details", lookup + System.Environment.NewLine + addInfo);
			}
		}

		protected override void SetRecordUQFromTariff(MasterFiles.Business.PartsDataToLoad record)
		{
			if (!record.PartClassification.IsEmpty)
			{
				record.PartUQ = GetUQFromTariff(record.PartClassification, "IMP");
			}
		}

		Classification GetClassification(AUOrgSupplierPart enterprisePart, string importExport)
		{
			Classification result = null;
			CusClassPartPivot pivot = GetRequiredGuidFromPivot(enterprisePart, importExport);
			if (pivot != null)
			{
				result = Factory.Load<Classification>(pivot.CI_CC);
			}
			return result;
		}

		ZGuid GetPivotPK(AUOrgSupplierPart enterprisePart, string importExport)
		{
			CusClassPartPivot pivot = GetRequiredGuidFromPivot(enterprisePart, importExport);
			return (pivot != null) ? pivot.PK : ZGuid.Empty;
		}

		CusClassPartPivot GetRequiredGuidFromPivot(AUOrgSupplierPart enterprisePart, string importExport)
		{
			CusClassPartPivot result = null;

			ZQuery existingCusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
			existingCusClassPartFilter.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.Australia);
			CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), existingCusClassPartFilter);
			if (cusClassPartLinks.Length > 0)
			{
				bool isImport = (importExport == Classification.ClassificationType.IMP);
				result = GetCorrectPivotForType(cusClassPartLinks, isImport);
			}

			return result;
		}

		CusClassPartPivot GetCorrectPivotForType(CusClassPartPivot[] cusClassPartLinks, bool isImport)
		{
			CusClassPartPivot result = null;
			if (PivotRecIsImport(cusClassPartLinks[0].CI_CC) == isImport)
			{
				result = cusClassPartLinks[0];
			}
			else if (cusClassPartLinks.Length > 1)
			{
				if (PivotRecIsImport(cusClassPartLinks[1].CI_CC) == isImport)
				{
					result = cusClassPartLinks[1];
				}
			}

			return result;
		}

		bool PivotRecIsImport(ZGuid lookupPK)
		{
			var result = Factory.Load<Classification>(lookupPK);
			return (result != null) && result.CC_ClassificationType == Classification.ClassificationType.IMP;
		}

		#endregion

	}
}
