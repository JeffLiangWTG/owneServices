using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.PaymentTimesReportingScheme
{
	public class OrgABNImportHelper
	{
		public OrgABNImportHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public string AnalysisABNList(IEnumerable<string> orgAbnList, ZDateTime validToDate, ZDateTimeOffset dateReceived)
		{
			if (orgAbnList == null || !orgAbnList.Any())
			{
				return null;
			}

			var listUpdated = new List<(string OrgCode, string AbnStr)>();
			var listSkipped = new List<(string OrgCode, string AbnStr)>();

			var orgInfoList = QuickLoadOrgInfoListByABNs(orgAbnList, validToDate);

			foreach (var orgInfo in orgInfoList)
			{
				if (!orgInfo.JobRequiredDocumentPK.IsEmpty)
				{
					listSkipped.Add((orgInfo.OrgCode, orgInfo.AbnStr));
					continue;
				}

				CreateDocumentTrackingRecord(orgInfo.OrgPK, validToDate, dateReceived);
				listUpdated.Add((orgInfo.OrgCode, orgInfo.AbnStr));
			}

			var listNotMatched = orgAbnList.Except(listUpdated.Select(x => x.AbnStr).Concat(listSkipped.Select(x => x.AbnStr)));

			return CreateNotificatoinMessage(listNotMatched, listUpdated, listSkipped);
		}

		IEnumerable<OrgInfo> QuickLoadOrgInfoListByABNs(IEnumerable<string> abnList, ZDateTime validToDate)
		{
			var sql = $@"SELECT OH_PK, OH_Code, OK_CustomsRegNo, EQ_PK 
FROM dbo.OrgHeader 
  JOIN dbo.OrgCompanyData ON OB_OH = OH_PK
  JOIN dbo.OrgCusCode ON OK_OH = OH_PK
  LEFT JOIN dbo.JobRequiredDocument ON
    EQ_ParentId = OH_PK AND
    EQ_DocCategory = '{Constants.ReferenceTypes.ComplianceReport}' AND
    EQ_DocType = '{AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines}' AND
    EQ_DocUsage = '{JobRequiredDocument.DocUsage.Creditor}' AND
    EQ_ValidToDate = @ReportingPeriod AND
    EQ_RN_NKRelatedCountry = '{CountryCodes.Australia}' AND
    EQ_ParentTableCode = '{OrgHeaderSchema.Constants.Prefix}'
WHERE
  OB_GC = @CompanyPK AND
  OB_IsCreditor = 1 AND
  OK_RN_NKCodeCountry = '{CountryCodes.Australia}' AND
  OK_CodeType = '{OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber}' AND
  OK_CustomsRegNo IN (SELECT Value FROM @ABNList)
ORDER BY OH_Code";

			//OrgCusCodeSchema.OK_CustomsRegNo has empty tvpName and does not support TVP parameter. For performance reason, below new schema column is created to support TVP.
			var tvpSchema = new SchemaStringColumn(OrgCusCodeSchema.OK_CustomsRegNo.TableSchema, OrgCusCodeSchema.OK_CustomsRegNo.Name, OrgCusCodeSchema.OK_CustomsRegNo.Ordinal,
												OrgCusCodeSchema.OK_CustomsRegNo.SqlDbType, OrgCusCodeSchema.OK_CustomsRegNo.SqlDbDefault, OrgCusCodeSchema.OK_CustomsRegNo.IsNullable,
												TVPHelper.TVP_STRING_LENGTH_MAX, OrgCusCodeSchema.OK_CustomsRegNo.IsLiteralOnly, OrgCusCodeSchema.OK_CustomsRegNo.IsNonBlankFilteredIndexParticipant, TVPHelper.TVP_nvarchar);
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, new ZSqlParameter[] {
					ZSqlParameter.New("@CompanyPK", GlbCompany.CurrentCompany.PK, OrgCompanyDataSchema.OB_GC),
					ZSqlParameter.New("@ABNList", abnList.Select(x => new ZString(x)).ToList(), tvpSchema, true), //TVP value must be ICollection
					ZSqlParameter.New("@ReportingPeriod", validToDate, JobRequiredDocumentSchema.EQ_ValidToDate),
				});

			return collection.Select(x => new OrgInfo((ZGuid)x[OrgHeaderSchema.PK], (ZString)x[OrgHeaderSchema.OH_Code], (ZString)x[OrgCusCodeSchema.OK_CustomsRegNo], (ZGuid)x[JobRequiredDocumentSchema.PK]));
		}

		string CreateNotificatoinMessage(IEnumerable<string> listNotMatched, IEnumerable<(string OrgCode, string AbnStr)> listUpdated, IEnumerable<(string OrgCode, string AbnStr)> listSkipped)
		{
			var builder = new ZStringBuilder();

			if (listUpdated.Any())
			{
				builder.AppendLine(ResString.GetMultilingualString("787C915F-6969-400D-8AFE-F514CCCA0FDD", "Below organizations / ABNs had document tracking record added:"));
				listUpdated.ForEach(x => builder.AppendLine($"{x.AbnStr} - {x.OrgCode}"));
				builder.AppendLine();
			}

			if (listSkipped.Any())
			{
				builder.AppendLine(ResString.GetMultilingualString("34CF70EE-B7C0-416D-8E6B-07D17FB1F42F", "Below organizations / ABNs already had document tracking record created for given reporting period:"));
				listSkipped.ForEach(x => builder.AppendLine($"{x.AbnStr} - {x.OrgCode}"));
				builder.AppendLine();
			}

			if (listNotMatched.Any())
			{
				builder.AppendLine(ResString.GetMultilingualString("32D8B2EB-CF11-4638-A454-4D3C46E1150C", "Below ABNs did not have matched organization:"));
				listNotMatched.ForEach(x => builder.AppendLine($"{x}"));
				builder.AppendLine();
			}

			return builder.ToString().Trim();
		}

		JobRequiredDocument CreateDocumentTrackingRecord(ZGuid parentId, ZDateTime validToDate, ZDateTimeOffset dateReceived)
		{
			var result = Factory.New<JobRequiredDocument>();
			result.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
			result.EQ_DocType = AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines;
			result.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			result.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			result.EQ_ValidToDate = validToDate;
			result.EQ_DateReceived = dateReceived;
			result.EQ_RN_NKRelatedCountry = CountryCodes.Australia;
			result.EQ_ParentID = parentId;
			result.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			result.ParentType = typeof(OrgHeader);

			return result;
		}

		readonly BusinessObjectFactory Factory;

		class OrgInfo
		{
			public OrgInfo(ZGuid orgPK, ZString orgCode, ZString abnStr, ZGuid jobRequiredDocumentPK)
			{
				OrgPK = orgPK;
				OrgCode = orgCode;
				AbnStr = abnStr;
				JobRequiredDocumentPK = jobRequiredDocumentPK;
			}

			public ZGuid OrgPK { get; private set; }
			public ZString OrgCode { get; private set; }
			public ZString AbnStr { get; private set; }
			public ZGuid JobRequiredDocumentPK { get; private set; }
		}
	}
}
