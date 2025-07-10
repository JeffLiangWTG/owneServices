using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ChargeCodeToGlobalChargeCodeMap
	{
		readonly BusinessObjectFactory factory;
		readonly IDictionary<ZGuid, IList<ZGuid>> chargeCodeToGlobalChargeCodeLookUp = new Dictionary<ZGuid, IList<ZGuid>>();

		protected ChargeCodeToGlobalChargeCodeMap(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public IEnumerable<GlobalChargeCodeMap> this[ZGuid chargeCodeId]
		{
			get
			{
				if (!WasIncludedInQuery(chargeCodeId)) { return Array.Empty<GlobalChargeCodeMap>(); }
				var globalChargeCodePKs = chargeCodeToGlobalChargeCodeLookUp[chargeCodeId];
				var result = globalChargeCodePKs.Select(globalChargeCodePK => factory.Load<GlobalChargeCodeMap>(globalChargeCodePK)).ToList();
				return result;
			}
		}

		public bool WasIncludedInQuery(ZGuid chargeCodeId)
		{
			return chargeCodeToGlobalChargeCodeLookUp.ContainsKey(chargeCodeId);
		}

		public static ChargeCodeToGlobalChargeCodeMap GetARMap(BusinessObjectFactory factory, ZGuid orgHeaderPK, IEnumerable<ZGuid> chargeCodePKs)
		{
			var map = new ChargeCodeToGlobalChargeCodeMap(factory);

			if (!chargeCodePKs.Any())
			{
				return map;
			}

			foreach (var pk in chargeCodePKs)
			{
				map.chargeCodeToGlobalChargeCodeLookUp[pk] = new List<ZGuid>();
			}

			string sqlChargeCodeIdList = String.Join(",", chargeCodePKs.Select(c => c.ToSqlGuid()).ToArray());

			string sql = @"
SELECT " + AccGlobalChargeCodeMapPivotSchema.Constants.YP_AC + @", " + AccGlobalChargeCodeMapSchema.Constants.PK + @"
FROM " + AccGlobalChargeCodeMapPivotSchema.Constants.SqlSchemaName + "." + AccGlobalChargeCodeMapPivotSchema.Constants.TableName + @" 
JOIN " + AccGlobalChargeCodeMapSchema.Constants.SqlSchemaName + "." + AccGlobalChargeCodeMapSchema.Constants.TableName + @" 
	ON " + AccGlobalChargeCodeMapSchema.Constants.PK + @" = " + AccGlobalChargeCodeMapPivotSchema.Constants.YP_YG + @"
		AND " + AccGlobalChargeCodeMapPivotSchema.Constants.YP_TYPE + @" = @LedgerType
WHERE " + AccGlobalChargeCodeMapPivotSchema.Constants.YP_AC + @" IN (" + sqlChargeCodeIdList + @")
	AND " + AccGlobalChargeCodeMapSchema.Constants.YG_OH + @" = @OrgHeaderPK";

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@LedgerType", LedgerTypes.AccountsReceivable, AccGlobalChargeCodeMapPivotSchema.YP_TYPE);
			parameters.Add("@OrgHeaderPK", orgHeaderPK, OrgHeaderSchema.PK);

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, parameters);

			var globalChargeCodePKs = new HashSet<ZGuid>();

			foreach (DynamicBusinessObject item in collection)
			{
				var chargeCodePK = new ZGuid(item[AccGlobalChargeCodeMapPivotSchema.Constants.YP_AC]);
				var globalChargCodePK = new ZGuid(item[AccGlobalChargeCodeMapSchema.Constants.PK]);
				map.chargeCodeToGlobalChargeCodeLookUp[chargeCodePK].Add(globalChargCodePK);
				globalChargeCodePKs.Add(globalChargCodePK);
			}

			// Preloading of all global charge codes for this mapping, to reduce number of queries
			factory.Load<GlobalChargeCodeMap>(new ZQuery(AccGlobalChargeCodeMapSchema.PK, globalChargeCodePKs));

			return map;
		}
	}
}
