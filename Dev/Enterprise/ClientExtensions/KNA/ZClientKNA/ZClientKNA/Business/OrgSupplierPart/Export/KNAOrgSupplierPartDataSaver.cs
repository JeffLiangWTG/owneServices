using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.KNA.Business
{
	internal class KNAOrgSupplierPartDataSaver : AUOrgSupplierPartDataSave
	{
		readonly ZQuery displayResultsQuery;

		public KNAOrgSupplierPartDataSaver(ZQuery displayResultsQuery)
		{
			this.displayResultsQuery = displayResultsQuery;
			this.displayResultsQuery.MaximumRows = null;
		}

		internal KNAOrgSupplierPartDataSaver()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString() != Core.Constants.CountryCodes.Australia)
			{
				throw new NotSupportedException();
			}
		}

		protected override List<String> NewColumnNames()
		{
			List<String> result = new List<String>(66);
			result.AddRange(base.NewColumnNames());
			result.Add("DumpingCountryOfExport");
			result.Add("DMP");
			result.Add("DumpingRateOfExchange");
			result.Add("DumpingExemptionType");
			result.Add("DumpingSpecificationNumber");
			result.Add("DXP");
			result.Add("GSTE");
			result.Add("ICN");
			result.Add("InvoiceLineAdj");
			result.Add("TAN");
			result.Add("RNO");
			result.Add("TILV");
			result.Add("TRN");
			result.Add("VAN");
			result.Add("WRL");
			result.Add("WET");
			result.Add("WETQ");
			result.Add("WETE");
			return result;
		}

		protected override MasterFiles.Business.PartsDataToSave NewPartsDataToSave()
		{
			return new KNAPartsDataToSave();
		}

		protected override void MapCountrySpecificClassification(OrgSupplierPart bizo, MasterFiles.Business.PartsDataToSave commonRecord)
		{
			base.MapCountrySpecificClassification(bizo, commonRecord);
			AUOrgSupplierPart orgSupplierPart = bizo as AUOrgSupplierPart;
			KNAPartsDataToSave record = commonRecord as KNAPartsDataToSave;

			CusClassPartPivot cusClassPartLink = GetPivot(orgSupplierPart, "IMP");
			if (cusClassPartLink != null)
			{
				var addInfo = cusClassPartLink.AddInfo;
				record.DumpingCountryOfExport = addInfo.ZA_DCX;
				record.DMP = addInfo.ZA_DMP.ToString("0.000");
				record.DumpingRateOfExchange = addInfo.ZA_DRE.ToString("0.000");
				record.DumpingExemptionType = addInfo.ZA_DXT;
				record.DumpingSpecificationNumber = addInfo.ZA_DSN;
				record.DXP = addInfo.ZA_DXP;
				record.GSTE = addInfo.ZA_GSTE;
				record.ICN = addInfo.ZA_ICN;
				record.InvoiceLineAdj = addInfo.ZA_ADJ;
				record.TAN = addInfo.ZA_TAN;
				record.RNO = addInfo.ZA_RNO;
				record.TILV = addInfo.ZA_TILV;
				record.TRN = addInfo.ZA_TRN;
				record.VAN = addInfo.ZA_VAN;
				record.WRL = addInfo.ZA_WRL.ToString();
				record.WET = addInfo.ZA_WET.ToString("0.000");
				record.WETQ = addInfo.ZA_WETQ;
				record.WETE = addInfo.ZA_WETE;
			}
			else
			{
				record.DumpingCountryOfExport = ZString.Empty;
				record.DMP = ZString.Empty;
				record.DumpingRateOfExchange = ZString.Empty;
				record.DumpingExemptionType = ZString.Empty;
				record.DumpingSpecificationNumber = ZString.Empty;
				record.DXP = ZString.Empty;
				record.GSTE = ZString.Empty;
				record.ICN = ZString.Empty;
				record.InvoiceLineAdj = ZString.Empty;
				record.TAN = ZString.Empty;
				record.RNO = ZString.Empty;
				record.TILV = ZString.Empty;
				record.TRN = ZString.Empty;
				record.VAN = ZString.Empty;
				record.WRL = ZString.Empty;
				record.WET = ZString.Empty;
				record.WETQ = ZString.Empty;
				record.WETE = ZString.Empty;
			}
		}

		protected override IList<String> ConvertRecordToFieldList(MasterFiles.Business.PartsDataToSave commonRecord)
		{
			List<String> result = new List<String>(66);
			result.AddRange(base.ConvertRecordToFieldList(commonRecord));

			KNAPartsDataToSave record = commonRecord as KNAPartsDataToSave;
			result.Add(record.DumpingCountryOfExport);
			result.Add(record.DMP);
			result.Add(record.DumpingRateOfExchange);
			result.Add(record.DumpingExemptionType);
			result.Add(record.DumpingSpecificationNumber);
			result.Add(record.DXP);
			result.Add(record.GSTE);
			result.Add(record.ICN);
			result.Add(record.InvoiceLineAdj);
			result.Add(record.TAN);
			result.Add(record.RNO);
			result.Add(record.TILV);
			result.Add(record.TRN);
			result.Add(record.VAN);
			result.Add(record.WRL);
			result.Add(record.WET);
			result.Add(record.WETQ);
			result.Add(record.WETE);
			return result;
		}

		protected override BusinessObjectCollection NewBusinessObjectCollection()
		{
			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory);
			collection.Load(displayResultsQuery);
			return collection;
		}
	}
}
