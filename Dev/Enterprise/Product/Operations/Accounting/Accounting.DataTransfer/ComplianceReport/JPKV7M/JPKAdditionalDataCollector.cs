//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M
{
	internal class JPKAdditionalDataCollector : ComplianceReportAdditionalDataCollectorBase<TransactionHeaderDetailsJPK>
	{
		public JPKAdditionalDataCollector(AccComplianceReport report) : base(report, ComplianceReportDataCollectionMode.JPKV7M)
		{
			CountryCodesOfEuropeanUnion = GetCountryCodesOfEuropeanUnion(Report);
			OrgProxies = GetCompanyAndBranchesOrgProxies();
			OrgHeaderDetails = GetOrgHeaderDetails(Report);
		}

		internal protected override Dictionary<ZGuid, TransactionHeaderDetailsJPK> GetTransactionHeaderDetails(AccComplianceReport report)
		{
			var result = new Dictionary<ZGuid, TransactionHeaderDetailsJPK>();

			var newFactory = new BusinessObjectFactory();
			var details = new DynamicBusinessObjectCollection(newFactory);

			var sql = FormattableString.Invariant($@"
		WITH
		HeaderSequences
		AS
		(
			SELECT AH_PK = AL_AH, 
				FirstSequence = MIN(ACL_ReportSequence),
				LastSequence = MAX(ACL_ReportSequence),
				EarliestTaxDate = MIN(AL_TaxDate),
				IsGTU_13 = MAX(IIF(ISNULL(VJ_JobType, '') IN ('LTC', 'WIN', 'WOU', 'WST', 'WSC', 'WSJ', 'WVO') OR (ISNULL(JS_TransportMode, '') = 'ROA'), 1, 0))
			FROM dbo.AccComplianceReportTransactionPivot
				JOIN dbo.AccTransactionLines ON AL_PK = ACL_ParentID
				LEFT JOIN dbo.JobHeader ON JH_PK = AL_JH
				LEFT JOIN dbo.ViewGenericJob ON VJ_PK = JH_ParentID
				LEFT JOIN dbo.JobShipment ON JS_PK = JH_ParentID AND JH_ParentTableCode = 'JS'
			WHERE ACL_ACR_Report = @ReportPK AND ACL_ParentTableCode = 'AL' AND AL_AH IS NOT NULL
			GROUP BY AL_AH
		)

		SELECT header.AH_PK, FirstSequence, LastSequence,
			AH_OH, AH_InvoiceDate, EarliestTaxDate, AH_DocumentReceivedDate,
			IsGTU_13 = CAST(IsGTU_13 AS BIT),
			header.AH_Ledger, header.AH_Desc, header.AH_SystemCreateUser CreateUserCode, 
			createUser.GS_FullName CreateUserName, 
			DATEADD(MINUTE, ISNULL(createTimeOffset.Offset, 0), header.AH_SystemCreateTimeUtc) AS CreateTime
		FROM HeaderSequences
			JOIN dbo.AccTransactionHeader header ON header.AH_PK = HeaderSequences.AH_PK
			LEFT JOIN dbo.GlbStaff createUser ON createUser.GS_Code = header.AH_SystemCreateUser
			LEFT JOIN dbo.GlbBranch branch ON branch.GB_PK = header.AH_GB
			OUTER APPLY dbo.GetTimeZoneOffsetInMinutes(branch.GB_RL_NKHomePort, header.AH_SystemCreateTimeUtc) createTimeOffset
");

			var parameters = new List<ZSqlParameter>();
			parameters.Add(ZSqlParameter.New("@ReportPK", report.PK, AccComplianceReportSchema.PK));

			details.Load(sql, parameters.ToArray());

			ZInt lineNumOffSet = report.LineNumOffset;
			var isUsedHeaderDetailsProvider = AdditionalDataProvider is IComplianceReportAdditionalDataHeaderDetailsProvider;
			foreach (DynamicBusinessObject row in details)
			{
				var headerPK = (ZGuid)row["AH_PK"]; // Direct access to DataRow is required
				var firstSequence = (ZInt)(row["FirstSequence"]); // Direct access to DataRow is required
				var lastSequence = (ZInt)(row["LastSequence"]); // Direct access to DataRow is required

				var detailsToAdd = new TransactionHeaderDetailsJPK()
				{
					HeaderSequence = firstSequence >= 0 ? (ZInt)(firstSequence + lineNumOffSet) : firstSequence,
					LastSequence = lastSequence >= 0 ? (ZInt)(lastSequence + lineNumOffSet) : lastSequence,
					Ledger = (ZString)row[AccTransactionHeaderSchema.Constants.AH_Ledger],
					Description = (ZString)row[AccTransactionHeaderSchema.Constants.AH_Desc],
					CreateUserCode = (ZString)row["CreateUserCode"],    // Direct access to DataRow is required
					CreateUserName = (ZString)row["CreateUserName"],    // Direct access to DataRow is required
					CreateTime = isUsedHeaderDetailsProvider ? ((ZDateTime)row["CreateTime"]) : ((ZDateTime)row["CreateTime"]),    // Direct access to DataRow is required
					// JPK additional properties
					OrgPK = (ZGuid)row[AccTransactionHeaderSchema.Constants.AH_OH],
					InvoiceDate = ((ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_InvoiceDate]).Date,
					EarliestTaxDate = ((ZDateTime)row["EarliestTaxDate"]).Date,    // Direct access to DataRow is required
					DocumentReceivedDate = ((ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_DocumentReceivedDate]).Date,
					IsGTU_13 = (ZBool)row["IsGTU_13"],
				};

				result.Add(headerPK, detailsToAdd);
			}

			return result;
		}

		internal HashSet<ZGuid> OrgProxies { get; private set; }

		protected override bool CollectsTaxRegistrationNumbers => true;

		protected override (ZString countryCode, ZString registrationNumber) GetPreferredTaxRegistrationNumber(AccComplianceReport report, OrgHeader orgHeader)
		{
			// Give preference to a registration number of the country of report
			var result = orgHeader.GetCountryCodeAndTaxRegistrationWithoutPrefix(report.Company?.Country?.RN_Code);

			return !result.registrationNumber.IsEmpty ? result : base.GetPreferredTaxRegistrationNumber(report, orgHeader);
		}

		HashSet<ZGuid> GetCompanyAndBranchesOrgProxies()
		{
			var result = new HashSet<ZGuid>();

			var newFactory = new BusinessObjectFactory();
			var details = new DynamicBusinessObjectCollection(newFactory);

			var sql = FormattableString.Invariant($@"
		SELECT GB_OH_OrgProxy
		FROM dbo.GlbBranch
		WHERE GB_IsActive = 1

		UNION ALL

		SELECT GC_OH_OrgProxy
		FROM dbo.GlbCompany
		WHERE GC_IsActive = 1");

			details.Load(sql);
			foreach (var row in details)
			{
				result.Add((ZGuid)row[GlbBranchSchema.Constants.GB_OH_OrgProxy]);
			}

			return result;
		}

		#region Tax Rate Codes and Groups

		internal static class TaxCodes
		{
			internal const string EXEMPT = "EXEMPT";
			internal const string EXCLUDE = "EXCLUDE";

			internal const string FREEPTU = "FREEPTU";
			internal const string FREEPTUREV = "FREEPTUREV";

			internal const string LOWPTU = "LOWPTU";
			internal const string MIDPTU = "MIDPTU";
			internal const string PTU = "PTU";
			internal const string CAPPTU = "CAPPTU";

			internal const string LOWPTUREV = "LOWPTUREV";
			internal const string MIDPTUREV = "MIDPTUREV";
			internal const string PTUREV = "PTUREV";
		}

		internal string[] PtuCapPtuCodes = new[] { TaxCodes.PTU, TaxCodes.CAPPTU };
		internal string[] PtuRangeTaxCodes = new[] { TaxCodes.LOWPTU, TaxCodes.MIDPTU, TaxCodes.PTU, TaxCodes.CAPPTU };
		internal string[] FreePtuTaxCodes = new[] { TaxCodes.FREEPTU, TaxCodes.FREEPTUREV };
		internal string[] RevTaxCodes = new[] { TaxCodes.LOWPTUREV, TaxCodes.MIDPTUREV, TaxCodes.FREEPTUREV, TaxCodes.PTUREV };

		#endregion
	}

	#region Additional Data JPK

	internal class TransactionHeaderDetailsJPK : IComplianceReportTransactionHeaderDetails
	{
		public ZInt HeaderSequence { get; set; }
		public ZInt LastSequence { get; set; }
		public ZGuid OrgPK { get; set; }
		public ZDate InvoiceDate { get; set; }
		public ZDate EarliestTaxDate { get; set; }
		public ZDate DocumentReceivedDate { get; set; }
		public ZBool IsGTU_13 { get; set; }
		public ZString Ledger { get; set; }
		public ZString Description { get; set; }
		public ZString CreateUserCode { get; set; }
		public ZString CreateUserName { get; set; }
		public ZDateTime CreateTime { get; set; }
	}

	#endregion
}
