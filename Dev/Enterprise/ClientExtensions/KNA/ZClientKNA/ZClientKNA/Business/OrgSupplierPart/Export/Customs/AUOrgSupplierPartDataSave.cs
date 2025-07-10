using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUOrgSupplierPartDataSave : Customs.Business.OrgSupplierPartDataSave, Integration.Customs.AU.IAUCusOrgSupplierPartDataLoad
	{
		public class PartsDataToSave : MasterFiles.Business.PartsDataToSave
		{
			public ZString Pref_Origin;
			public ZString Pref_Rule;
			public ZString Pref_Scheme;
			public ZString Pref_InstrumentType;
			public ZString Pref_InstrumentNo;
		}

		protected override List<String> NewColumnNames()
		{
			List<String> result = new List<String>(47);
			result.AddRange(base.NewColumnNames());
			result.Add("Pref_Origin");
			result.Add("Pref_Rule");
			result.Add("Pref_Scheme");
			result.Add("Pref_InstrumentType");
			result.Add("Pref_InstrumentNo");
			return result;
		}

		protected override MasterFiles.Business.PartsDataToSave NewPartsDataToSave()
		{
			return new PartsDataToSave();
		}

		protected override void MapCountrySpecificClassification(OrgSupplierPart bizo, MasterFiles.Business.PartsDataToSave commonRecord)
		{
			AUOrgSupplierPart orgSupplierPart = bizo as AUOrgSupplierPart;
			PartsDataToSave record = commonRecord as PartsDataToSave;
			CusClassPartPivot cusClassPartLink = GetPivot(orgSupplierPart, "IMP");
			if (cusClassPartLink != null)
			{
				var addInfo = cusClassPartLink.AddInfo;
				record.Pref_InstrumentType = addInfo.PRI_InstrumentType;
				record.Pref_InstrumentNo = addInfo.PRI_InstrumentNo;
				record.Pref_Origin = addInfo.ZA_POC;
				record.Pref_Scheme = addInfo.ZA_PST;
				record.Pref_Rule = addInfo.ZA_PRT;
				record.PartOrigin = addInfo.ZA_ORG;
			}
			else
			{
				record.Pref_InstrumentType = ZString.Empty;
				record.Pref_InstrumentNo = ZString.Empty;
				record.Pref_Origin = ZString.Empty;
				record.Pref_Scheme = ZString.Empty;
				record.Pref_Rule = ZString.Empty;
				record.PartOrigin = ZString.Empty;
			}

			MapCountrySpecificClassificationCore(orgSupplierPart, "IMP", out record.PartClassification);
			MapCountrySpecificClassificationCore(orgSupplierPart, "EXP", out record.PartExportClassification);
		}

		void MapCountrySpecificClassificationCore(AUOrgSupplierPart orgSupplierPart, string importOrExport, out ZString recordField)
		{
			Classification classification = GetClassification(orgSupplierPart, importOrExport);
			if (classification != null)
			{
				recordField = classification.CC_LookupCode;
			}
			else
			{
				recordField = ZString.Empty;
			}
		}

		protected override IList<String> ConvertRecordToFieldList(MasterFiles.Business.PartsDataToSave commonRecord)
		{
			List<String> result = new List<String>(47);
			result.AddRange(base.ConvertRecordToFieldList(commonRecord));

			PartsDataToSave record = commonRecord as PartsDataToSave;
			result.Add(record.Pref_Origin);
			result.Add(record.Pref_Rule);
			result.Add(record.Pref_Scheme);
			result.Add(record.Pref_InstrumentType);
			result.Add(record.Pref_InstrumentNo);
			return result;
		}

		Classification GetClassification(AUOrgSupplierPart enterprisePart, string importOrExport)
		{
			Classification result = null;
			CusClassPartPivot pivot = GetPivot(enterprisePart, importOrExport);
			if (pivot != null)
			{
				result = Factory.Load<Classification>(pivot.CI_CC);
			}
			return result;
		}

		protected CusClassPartPivot GetPivot(AUOrgSupplierPart enterprisePart, string importOrExport)
		{
			CusClassPartPivot result = null;
			ZQuery existingCusClassPartFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, enterprisePart.PK);
			CusClassPartPivot[] cusClassPartLinks = (CusClassPartPivot[])enterprisePart.Factory.Load(typeof(CusClassPartPivot), existingCusClassPartFilter);
			if (cusClassPartLinks.Length > 0)
			{
				bool isImport = (importOrExport == Classification.ClassificationType.IMP);
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
			Classification result = (Classification)Factory.Load(typeof(Classification), lookupPK);
			return (result != null) && result.CC_ClassificationType == Classification.ClassificationType.IMP;
		}
	}
}
