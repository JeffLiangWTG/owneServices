using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CalculateAccountingAgeProcessor
	{
		public CalculateAccountingAgeProcessor(ILogger serviceLogger)
		{
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}

#if DEBUG
		internal CalculateAccountingAgeProcessor(ILogger serviceLogger, int batchSize)
		{
			this.batchSize = batchSize;
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}
#endif

		readonly ILogger logger;
		readonly int batchSize = 50;

		public void Process(ZGuid companyPK)
		{
			var dynamicBOs = GetCandidateDeclarationPKs(companyPK);

			foreach (var batchDynamicBOs in dynamicBOs.Batch(batchSize))
			{
				var pks = batchDynamicBOs.Select(bo => new ZGuid(bo[JobDeclarationSchema.Constants.PK]));

				var factory = new BusinessObjectFactory();
				var declarations = factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, pks));
				foreach (var declaration in declarations)
				{
					declaration.UpdateCA_AccountingAge();
				}

				try
				{
					factory.Save();
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}
			}

			logger.Information(string.Format(CultureInfo.CurrentCulture, "Accounting Age Calculation Finished for {0} Declaration(s)", dynamicBOs.Count));
		}

		public DynamicBusinessObjectCollection GetCandidateDeclarationPKs(ZGuid companyPK)
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			var declarationPKs = new DynamicBusinessObjectCollection(factory);
			declarationPKs.Load(GetCandidateDeclarationPKSql(), new[] { ZSqlParameter.New("@CompanyPK", companyPK, JobDeclarationSchema.JE_GC) });
			return declarationPKs;
		}

		string GetCandidateDeclarationPKSql()
		{
			var querySql = $@"
	SELECT 
		JE_PK 
	FROM 
		dbo.CAJobDeclaration
	WHERE 
		JE_MessageType = '{JobMessageTypeList.Codes.Import}' 
		AND JE_MessageSubType <> '{B3EntryTypeList.Codes.NoB3}' 
		AND JE_EntryAuthorisationDate IS NOT NULL
		AND JE_GC = @CompanyPK
		AND JE_K84AccountingDate IS NULL
		AND JE_CSAEntry = 0
		AND EXISTS (SELECT 1
					FROM dbo.CusEntryHeader
					WHERE
						CH_ClusterKey = JE_ClusterKey
						AND CH_MessageType IN ('{MessageTypeList.Codes.B3CUSDEC}', '{MessageTypeList.Codes.CommercialAccountingDeclaration}')
						AND CH_EntryStatus <> '{B3EntryStatusList.Codes.Accepted}'
						AND CH_EntryStatus <> '{B3EntryStatusList.Codes.Confirmed}'
						AND CH_EntryStatus <> '{CADEntryStatusList.Codes.Approved}'
					)
";

			return querySql;
		}
	}
}
