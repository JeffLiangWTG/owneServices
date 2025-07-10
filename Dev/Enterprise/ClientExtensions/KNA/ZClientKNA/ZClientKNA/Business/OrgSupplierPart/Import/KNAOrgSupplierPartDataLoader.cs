using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.KNA.Business
{
	internal class KNAOrgSupplierPartDataLoader : AUOrgSupplierPartDataLoad
	{
		internal KNAOrgSupplierPartDataLoader()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString() != Core.Constants.CountryCodes.Australia)
			{
				throw new NotSupportedException();
			}
		}

		protected override IEnumerable<string> GetFieldNames()
		{
			List<string> result = new List<string>(25);
			result.AddRange(base.GetFieldNames());
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

		protected override MasterFiles.Business.PartsDataToLoad GetPartsDataToLoad()
		{
			return new KNAPartsDataToLoad();
		}

		protected override ZString BuildAddInfoData(PartsDataToLoad record1)
		{
			ZString addInfo = base.BuildAddInfoData(record1);
			KNAPartsDataToLoad record = (KNAPartsDataToLoad)record1;
			if (!record.DumpingCountryOfExport.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "DCX=" + record.DumpingCountryOfExport);
			}
			if (!record.DMP.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "DMP=" + record.DMP);
			}
			if (!record.DumpingRateOfExchange.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "DRE=" + record.DumpingRateOfExchange);
			}
			if (!record.DumpingExemptionType.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "DXT=" + record.DumpingExemptionType);
			}
			if (!record.DumpingSpecificationNumber.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "DSN=" + record.DumpingSpecificationNumber);
			}
			if (!record.DXP.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "DXP=" + record.DXP);
			}
			if (!record.GSTE.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "GSTE=" + record.GSTE);
			}
			if (!record.ICN.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "ICN=" + record.ICN);
			}
			if (!record.InvoiceLineAdj.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "ADJ=" + record.InvoiceLineAdj);
			}
			if (!record.TAN.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "TAN=" + record.TAN);
			}
			if (!record.RNO.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "RNO=" + record.RNO);
			}
			if (!record.TILV.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "TILV=" + record.TILV);
			}
			if (!record.TRN.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "TRN=" + record.TRN);
			}
			if (!record.VAN.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "VAN=" + record.VAN);
			}
			if (!record.WRL.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "WRL=" + record.WRL);
			}
			if (!record.WET.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "WET=" + record.WET);
			}
			if (!record.WETQ.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "WETQ=" + record.WETQ);
			}
			if (!record.WETE.IsEmpty)
			{
				addInfo = AppendAddInfo(addInfo, "WETE=" + record.WETE);
			}
			return addInfo;
		}
	}
}
