using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class StandardXMLEventProcessor : StandardXmlProcessor
	{
		internal override bool NewMessageAvailable(out string[] companyCodes)
		{
			string sql = @"
			SELECT GC_Code
			FROM 
				(SELECT EM_GB, MIN(EM_SystemCreateTimeUtc) as OldestCreateTime 
				FROM dbo.EDIMessage
				WHERE
				EM_ApplicationCode = @AppCode 
				AND EM_Status = @MessageStatus 
				AND EM_MessageType = @MessageType 
				AND EM_ReceiveTransmit = @ReceiveTransmit
				AND EM_MessageSubType = @EVT
				GROUP BY EM_GB) as t
			INNER JOIN dbo.GlbBranch 
			ON t.EM_GB = GlbBranch.GB_PK
			INNER JOIN dbo.GlbCompany
			ON GlbBranch.GB_GC = GlbCompany.GC_PK
			ORDER BY OldestCreateTime";

			var sqlParams = new ZSqlParameterCollection();
			AddCommonSqlParameters(sqlParams);
			var queryResult = new DynamicBusinessObjectCollection(FactoryProvider.Current);
			queryResult.Load(sql, sqlParams);

			if (queryResult.Count == 0)
			{
				companyCodes = Array.Empty<string>();
				return false;
			}

			var companyList = new List<string>();
			Array.ForEach(queryResult.ToArray(), (dynamicBusinessObject) => { companyList.Add((ZString)dynamicBusinessObject["GC_Code"]); });
			companyCodes = companyList.ToArray();
			return true;
		}

		internal override List<ZGuid> GetEDIMessagePKs()
		{
			string sql = "SELECT TOP 200 " + EDIMessageSchema.PK.Name + @" FROM dbo.EDIMessage WHERE 
			EM_ApplicationCode = @AppCode 
			AND EM_Status = @MessageStatus 
			AND EM_MessageType = @MessageType 
			AND EM_ReceiveTransmit = @ReceiveTransmit
			AND EM_GB IN (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = @CurrentCompanyPK)
			AND EM_MessageSubType = @EVT
			ORDER BY EM_SystemCreateTimeUtc";

			var sqlParams = new ZSqlParameterCollection();
			AddCommonSqlParameters(sqlParams);
			sqlParams.Add(ZSqlParameter.New("@CurrentCompanyPK", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC));
			var queryResult = new DynamicBusinessObjectCollection(FactoryProvider.Current);
			queryResult.Load(sql, sqlParams);

			var result = new List<ZGuid>();
			Array.ForEach(queryResult.ToArray(), (dynamicBusinessObject) => { result.Add((ZGuid)dynamicBusinessObject[EDIMessageSchema.PK.Name]); });
			return result;
		}

		static void AddCommonSqlParameters(ZSqlParameterCollection sqlParams)
		{
			sqlParams.Add(ZSqlParameter.New("@AppCode", ApplicationCodeList.Codes.XMS, EDIMessageSchema.EM_ApplicationCode));
			sqlParams.Add(ZSqlParameter.New("@MessageStatus", EDIMessage.Status.Queued, EDIMessageSchema.EM_Status));
			sqlParams.Add(ZSqlParameter.New("@MessageType", EDIMessageTypeList.Codes.XMS, EDIMessageSchema.EM_MessageType));
			sqlParams.Add(ZSqlParameter.New("@ReceiveTransmit", EDIMessage.Direction.Receive, EDIMessageSchema.EM_ReceiveTransmit));
			sqlParams.Add(ZSqlParameter.New("@EVT", EDIMessageSubTypeList.Codes.Events, EDIMessageSchema.EM_MessageSubType));
		}
	}
}