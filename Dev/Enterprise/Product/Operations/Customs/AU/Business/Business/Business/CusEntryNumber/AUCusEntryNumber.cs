using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCusEntryNumber : CusEntryNumber
	{
		public AUCusEntryNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZBool IsValidExemption
		{
			get { return CE_EntryType.StartsWith("EX") || new CMRExportExemptionCodesList().ContainsCode(CE_EntryType); }
		}

		public override ZString CE_EntryType
		{
			get { return CMRExportExemptionCodes.Get4CharCode(base.CE_EntryType); }
			set { base.CE_EntryType = CMRExportExemptionCodes.Get3CharCode(value); }
		}

		public static IEnumerable<AUCusEntryNumber> LoadEntryNumber(BusinessObjectFactory factory, ZGuid parentPK, ZString parentTable, ZString countryCode, bool fetchOnlyFromLocalCache = false)
		{
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, parentPK);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, parentTable);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
			query.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
			return factory.Load<AUCusEntryNumber>(query);
		}
	}
}
