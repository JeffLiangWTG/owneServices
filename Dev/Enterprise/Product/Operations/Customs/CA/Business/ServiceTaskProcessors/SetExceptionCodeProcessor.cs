using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class SetExceptionCodeProcessor
	{
		public SetExceptionCodeProcessor(ILogger serviceLogger)
		{
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}

		readonly ILogger logger;
		readonly int batchSize = 50;
		public void Process(GlbCompany company)
		{
			var companyPK = company.PK.ToGuid();
			var timeFrame = CACustomsDataRegistry.Instance.TimeFrameForExceptionReporting.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			if (timeFrame < 0)
			{
				return;
			}

			var dynamicBOs = GetCandidateDeclarationPKs(company, ZDateTime.Now.Date.AddDays(-timeFrame));
			var calculator = new DeclarationExceptionCodeCalculator(companyPK);
			foreach (var batchDynamicBOs in dynamicBOs.Batch(batchSize))
			{
				var pks = batchDynamicBOs.Select(bo => new ZGuid(bo[JobDeclarationSchema.Constants.PK]));

				var factory = new BusinessObjectFactory();
				var declarations = factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, pks));
				foreach (var declaration in declarations)
				{
					calculator.SetDeclarationException(declaration, factory);
					if (declaration.CA_DeclarationException.IsEmpty)
					{
						logger.Information(ZString.Format("Clear Exception Code for declaration {0}.", declaration.JE_DeclarationReference));
					}
					else
					{
						logger.Information(ZString.Format("Set Exception Code {0} to declaration {1}.", declaration.CA_DeclarationException, declaration.JE_DeclarationReference));
					}
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
		}

		public DynamicBusinessObjectCollection GetCandidateDeclarationPKs(GlbCompany company, ZDate effectiveDate)
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			var declarationPKs = new DynamicBusinessObjectCollection(factory);
			declarationPKs.Load(GetCandidateDeclarationPKSql(), new[]
			{
				ZSqlParameter.New("@CompanyPK", company.PK, GlbBranchSchema.GB_GC),
				ZSqlParameter.New("@EffectiveDate", effectiveDate, JobDeclarationSchema.JE_EntrySubmittedDate)
			});
			return declarationPKs;
		}

		string GetCandidateDeclarationPKSql()
		{
			return $@"
SELECT 
	JE_PK
FROM 
	dbo.CAJobDeclaration
	INNER JOIN dbo.GlbBranch ON GB_PK = JE_GB AND GB_GC = @CompanyPK
WHERE
	JE_MessageType <> '{JobMessageTypeList.Codes.Export}' 
	AND JE_EntrySubmittedDate IS NOT NULL 
	AND JE_EntrySubmittedDate > @EffectiveDate
	AND JE_K84AccountingDate IS NULL";
		}
	}
}
