using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class EliminationJournalCreator
	{
		public EliminationJournalCreator()
		{
		}

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		public delegate void ShowErrorHandler(string message, string caption);
		public ShowErrorHandler ShowError;

		public delegate void NoTransactionFoundHandler(string message, string caption);
		public NoTransactionFoundHandler NoTransactionFound;

		public void Create(IEnumerable<ZGuid> consolidationBatchPks)
		{
			var journalCount = 0;
			var consolidationBatches = Factory.Load<AccConsolidationBatch>(new ZQuery(AccConsolidationBatchSchema.PK, consolidationBatchPks));

			var consolidationBatchGroup = from c in consolidationBatches
																		group c by new { c.YB_YR_ConsolidationGroup, c.YB_GC_Company, c.YB_AM_Period } into g
																		select new { GroupPk = g.Key.YB_YR_ConsolidationGroup, CompanyPk = g.Key.YB_GC_Company, Period = g.Key.YB_AM_Period, ConsolidationBatchs = g };

			foreach (var item in consolidationBatchGroup)
			{
				var postingCompany = Factory.Load<GlbCompany>(item.CompanyPk);

				if (postingCompany.FirstActiveBranch == null)
				{
					ShowError(Res.GetString("9286DE7B-0F10-4962-AD0D-91F1CADCEBDC", "Consolidation Batch has a company which has no active branch"), Res.GetString("EF606AA5-8A23-421F-8935-5A681D64C512", "Elimination Journals"));
					return;
				}
			}

			foreach (var item in consolidationBatchGroup)
			{
				var eliminationCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(item.CompanyPk.ToGuid(), Guid.Empty, Guid.Empty).EliminationCategory;
				if (eliminationCategory != null)
				{
					var groupedConsolidationBatchPks = new List<ZGuid>();

					foreach (var batch in item.ConsolidationBatchs)
					{
						groupedConsolidationBatchPks.Add(batch.PK);
					}

					var consolidationBatchRows = new ConsolidationBatchExporter(groupedConsolidationBatchPks, false).Export();

					if (ShouldCreateEliminationJournal(consolidationBatchRows))
					{
						var postingCompany = Factory.Load<GlbCompany>(item.CompanyPk);

						using (postingCompany.FirstActiveBranch.SetAsTemporaryContext())
						{
							var factoryForJournalCreation = new BusinessObjectFactory();
							var eliminationJournal = CreateJournalWithLines(factoryForJournalCreation, eliminationCategory.Code, item.Period, postingCompany.FirstActiveBranch.PK, postingCompany.PK, consolidationBatchRows);

							var groupedConsolidationBatches = factoryForJournalCreation.Load<AccConsolidationBatch>(new ZQuery(AccConsolidationBatchSchema.PK, groupedConsolidationBatchPks));
							foreach (var batch in groupedConsolidationBatches)
							{
								if (batch.EliminationJournal == null)
								{
									batch.YB_AH_EliminationJournal = eliminationJournal.PK;
								}
								else
								{
									ShowError(Res.GetString("a82cc42b-3800-4a1a-a5f0-60db304bbd53", "Consolidation Batch already has an elimination journal"), Res.GetString("e9b24d5d-2acd-452b-b7be-b4868e386020", "Elimination Journals"));
									return;
								}
							}

							eliminationJournal.RunPreSaveValidation();
#if DEBUG
							if (Globals.IsTest)
							{
								EliminationJournals_TestOnly.Add(eliminationJournal);
							}
#endif

							if (eliminationJournal.HasErrors)
							{
								var group = factoryForJournalCreation.Load<AccConsolidationGroup>(item.GroupPk);
								var journalDesc = Res.GetString("ebc6dd42-ec70-4434-989e-502322710694", "Company: {0}, Group: {1}, Transaction Category {2}", eliminationJournal.Company.GC_Code, group.YR_Code, eliminationJournal.AH_TransactionCategory);
								var errorDetails = string.Join("\r\n", new ZNotificationCollector(eliminationJournal, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().Select(x => x.Message));
								var message = Res.GetString("acb731f7-56b2-4d64-a3b2-0117cac1d1c7", "When trying to create an elimination journal ({0}), the following errors occurred: \r\n\r\n{1}", journalDesc, errorDetails);
								ShowError(message, Res.GetString("e9b24d5d-2acd-452b-b7be-b4868e386020", "Elimination Journals"));
								return;
							}
							else
							{
								factoryForJournalCreation.Save();
								journalCount++;
							}
						}
					}
				}
			}
			if (journalCount <= 0 && NoTransactionFound != null)
			{
				var caption = Res.GetString("b8831b72-6297-4aa0-82b9-aa6091904a0e", "No journal was created");
				var message = Res.GetString("52631516-3f63-4f1f-8762-663ce0e733d5", "No matching transaction found for creating elimination journals");
				NoTransactionFound(message, caption);
			}
		}

		GLJournals.GLJournal CreateJournalWithLines(BusinessObjectFactory factoryToCreate, ZString transactionCategory, ZGuid periodPK, ZGuid branckPK, ZGuid companyPK, IEnumerable<ConsolidationBatchDetailsRow> consolidationBatchRows)
		{
			var journal = factoryToCreate.New<GLJournals.GLJournal>();
			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			journal.AH_TransactionCategory = transactionCategory;
			journal.AH_Desc = Res.GetString("8b4fbdc3-4b83-4271-af29-cee12524511b", "Elimination General Ledger Journal");
			journal.AH_GB = branckPK;
			journal.AH_GC = companyPK;
			journal.PostPeriod = Factory.LoadTop1<AccPeriodManagement>(new ZQuery(AccPeriodManagementSchema.PK, SQLComparisonOperator.Equal, periodPK)).AM_Period;

			foreach (var consolidationBatchRow in consolidationBatchRows)
			{
				if (!consolidationBatchRow.TransactionOrganisationCode.IsEmpty)
				{
					CreateJournalLine(journal, consolidationBatchRow);
				}
			}
			return journal;
		}

		void CreateJournalLine(GLJournals.GLJournal eliminationJournal, ConsolidationBatchDetailsRow consolidationBatchRow)
		{
			GLJournalLine line = (GLJournalLine)eliminationJournal.Lines.AddNew();
			line.SetContext(BusinessContext.AutoEliminationJournal);
			line.AL_LineType = TransactionTypes.GLStandardJournal;
			line.UnsignedOSLineAmount = Math.Abs(consolidationBatchRow.AmountInPostingCompanyCurrency);
			line.DebitCreditSign = consolidationBatchRow.AmountInPostingCompanyCurrency > 0 ? nameof(DebitCredit.CR) : nameof(DebitCredit.DR); //ToDo - check
			var branch = Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, consolidationBatchRow.BranchCode.Trim());
			line.AL_GB = branch != null ? branch.PK : Guid.Empty;
			var department = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, consolidationBatchRow.DepartmentCode.Trim());
			line.AL_GE = department != null ? department.PK : Guid.Empty;
			var glAccount = Factory.LoadFromUniqueKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, consolidationBatchRow.GLAccount.Trim());
			line.AL_AG = glAccount != null ? glAccount.PK : Guid.Empty;
			line.AL_Desc = Res.GetString("8b4fbdc3-4b83-4271-af29-cee12524511b", "Elimination General Ledger Journal");
			var orgnization = Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, consolidationBatchRow.TransactionOrganisationCode.Trim());
			line.AL_OH = orgnization != null ? orgnization.PK : Guid.Empty;
			line.AL_GC = eliminationJournal.AH_GC;
		}

#if DEBUG
		public List<GLJournals.GLJournal> EliminationJournals_TestOnly = new List<GLJournals.GLJournal>();
#endif

#if DEBUG
		protected virtual
#endif
		bool ShouldCreateEliminationJournal(IEnumerable<ConsolidationBatchDetailsRow> consolidationBatchRows)
		{
			return consolidationBatchRows.Any(x => !string.IsNullOrEmpty(x.TransactionOrganisationCode));
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	public class EliminationJournalCreatorDebug : EliminationJournalCreator
	{
		protected override bool ShouldCreateEliminationJournal(IEnumerable<ConsolidationBatchDetailsRow> consolidationBatchRows)
		{
			return true;
		}
	}
}

#endif
#endregion
