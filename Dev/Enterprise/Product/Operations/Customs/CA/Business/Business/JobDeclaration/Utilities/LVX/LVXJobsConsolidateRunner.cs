using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class LVXJobsConsolidateRunner
	{
		public LVXJobsConsolidateRunner(ILVXJobsConsolidateRunnerLog log, BusinessObjectFactory factory)
		{
			Log = log;
			Factory = Argument.NotNull(factory, "factory");
		}

		protected readonly BusinessObjectFactory Factory;
		protected readonly ILVXJobsConsolidateRunnerLog Log;

		#region SelectLVXJobPKs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<(ZGuid DeclarationPK, ZGuid BranchPK)> SelectLVXJobPKsReadyForConsolidation()
		{
			var factory = new BusinessObjectFactory();
			var query = GetSelectLVXJobPKsReadyForConsolidationQuery();
			var dynamicBOs = new DynamicBusinessObjectCollection(factory);
			if (!query.IsNoResultQuery)
			{
				dynamicBOs.Load(GetSelectPKQueryString(query.GetAsWhereClause(false)), new ZSqlParameterCollection(query.Params));
			}
			return dynamicBOs.Select(x => (new ZGuid(x[JobDeclarationSchema.Constants.PK]), new ZGuid(x[JobDeclarationSchema.Constants.JE_GB])));
		}

		static ZQuery GetSelectLVXJobPKsReadyForConsolidationQuery()
		{
			var query = GetLVXDeclarationQuery(null);
			query.AddSubQuery(GetInvoiceQuery(), JoinCondition.And);
			var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration });
			query.AddSubQuery(entryQuery, JoinCondition.And);
			var invoiceQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			invoiceQuery.AddToFilter(ModelViewColumnHelper.GetModuleFilterQuery(JobComInvoiceHeader.Schema.PK, ModelViewPK, ModelView, "JZ_ReadyForConsolidation", SQLComparisonOperator.Equal, true));
			query.AddSubQuery(invoiceQuery, JoinCondition.And);
			return query;
		}

		public IEnumerable<ZGuid> SelectLVXJobPKs(LVXSelectionCriteriaBO selectionCriteria)
		{
			Argument.NotNull(selectionCriteria, "selectionCriteria");
			var query = GetLVXSelectQuery(selectionCriteria);
			var dynamicBOs = new DynamicBusinessObjectCollection(Factory);
			dynamicBOs.Load(GetSelectPKQueryString(query.GetAsWhereClause(false)), new ZSqlParameterCollection(query.Params));
			return dynamicBOs.Select(bo => new ZGuid(bo[JobDeclarationSchema.Constants.PK])).Distinct();
		}

		static ZString GetSelectPKQueryString(ZString whereClause)
		{
			return string.Format(CultureInfo.InvariantCulture, @" SELECT {0}, {1} FROM {2} {3} Order By {4}",
			 JobDeclarationSchema.Constants.PK, JobDeclarationSchema.Constants.JE_GB, JobDeclarationSchema.Constants.TableName, whereClause, JobDeclarationSchema.JE_DeclarationReference.Name);
		}

		static ZQuery GetLVXSelectQuery(LVXSelectionCriteriaBO selectionCriteria)
		{
			var fistPeriodDay = new ZDateTime(selectionCriteria.PeriodYear, selectionCriteria.PeriodMonth, 1);
			var query = GetLVXDeclarationQuery(GlbCompany.CurrentCompany.PK);
			query.AddToFilter(JobDeclarationSchema.JE_EntryAuthorisationDate, SQLComparisonOperator.GreaterThanOrEqualTo, fistPeriodDay);
			query.AddToFilter(JobDeclarationSchema.JE_EntryAuthorisationDate, SQLComparisonOperator.LessThanOrEqualTo, fistPeriodDay.AddMonths(1).AddDays(-1));

			query.AddSubQuery(GetInvoiceQuery(), JoinCondition.And);

			if (selectionCriteria.OH_Importer.IsValid)
			{
				query.AddToFilter(JobDeclarationSchema.JE_OH_Importer, selectionCriteria.OH_Importer);
			}
			if (selectionCriteria.Branch.IsValid)
			{
				query.AddToFilter(JobDeclarationSchema.JE_GB, selectionCriteria.Branch);
			}
			if (!selectionCriteria.ProvinceOfClearance.IsEmpty)
			{
				query.AddToFilter(ConsolidateByProvinceOfClearanceStrategy.GetProvinceOfClearanceQuery(selectionCriteria.ProvinceOfClearance));
			}
			if (!selectionCriteria.GS_NKBroker.IsEmpty)
			{
				query.AddToFilter(JobDeclarationSchema.JE_GS_NKCusAgent, selectionCriteria.GS_NKBroker);
			}
			return query;
		}

		static ZDBOnlyQuery GetLVXDeclarationQuery(ZGuid? comanyPK)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.LVSForConsolidation);
			query.AddToFilter(JobDeclarationSchema.JE_EntryAuthorisationDate, SQLComparisonOperator.NotEqual, DBNull.Value);
			if (comanyPK.HasValue)
			{
				query.AddToFilter(JobDeclarationSchema.JE_GC, comanyPK.Value);
			}
			else
			{
				var subQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
				subQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
				subQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
				query.AddSubQuery(JobDeclarationSchema.JE_GC, subQuery, JoinCondition.And);
			}

			return query;
		}

		static ZDBOnlySubQuery GetInvoiceQuery()
		{
			var invoiceQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE, true);
			var pivotQuert = new ZDBOnlySubQuery(typeof(CustomsGenPivot), GenPivotSchema.XX_Relation1ID);
			pivotQuert.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
			pivotQuert.AddToFilter(GenPivotSchema.XX_Relation2TableCode, JobDeclarationSchema.Constants.Prefix);
			pivotQuert.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);
			invoiceQuery.AddSubQuery(pivotQuert, JoinCondition.And);
			return invoiceQuery;
		}

		#endregion

		#region ModelViewColumnHelper

		static ModelViewColumnQueryHelper<JobComInvoiceHeader> ModelViewColumnHelper
		{
			get
			{
				return new ModelViewColumnQueryHelper<JobComInvoiceHeader>();
			}
		}
		const string ModelView = "CAJobComInvoiceHeader";
		const string ModelViewPK = "JZ_PK";

		#endregion

		#region ConsolidateLVXJobs

		public void ConsolidateLVXJobs(JobDeclaration[] declarations, bool needNotify)
		{
			if (declarations.Length > 0)
			{
				if (Log != null)
				{
					Log.SetSectionProgressMax(declarations.Length);
				}

				List<JobDeclaration> associatedLVSDeclarations = new List<JobDeclaration>();
				foreach (var lvxJob in declarations)
				{
					var lvxNeedToConsolidate = Factory.Load<JobDeclaration>(lvxJob.PK);
					if (lvxNeedToConsolidate == null)
					{
						continue;
					}

					LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvxNeedToConsolidate, Factory, associatedLVSDeclarations, (lvx) => lvx.GetDeclarationIdLink(), NotifyFormatAndBumpSectionProgress, GetConsolidationStrategies);
				}

				try
				{
					LVXJobsConsolidateHelper.RaiseSaving(associatedLVSDeclarations, () =>
					{
						if (needNotify)
						{
							Factory.Save();
						}
					}, NotifyForMessageInitiator);
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}
			}
		}

#if DEBUG
		protected virtual
#endif
		IEnumerable<IConsolidationStrategy> GetConsolidationStrategies(JobDeclaration lvxJob)
		{
			return LVXJobsConsolidateHelper.GetConsolidationStrategies(lvxJob);
		}
		#endregion

		#region DeConsolidateLVXJobs

		public void DeConsolidateLVXJobs(JobDeclaration[] declarations)
		{
			if (declarations.Length > 0)
			{
				if (Log != null)
				{
					Log.SetSectionProgressMax(declarations.Length);
				}

				foreach (var lvx in declarations)
				{
					var lvxToBeDeConsolidated = Factory.Load<JobDeclaration>(lvx.PK);
					if (lvxToBeDeConsolidated == null || lvxToBeDeConsolidated.IsCancelled)
					{
						NotifyFormatAndBumpSectionProgress(OperationalActionLogErrorLevel.Warning, LVXJobsConsolidateHelper.GetDeclarationDeactivedLog, lvx.GetDeclarationIdLink());
					}
					else
					{
						var invoice = lvxToBeDeConsolidated.Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault();
						if (invoice == null)
						{
							NotifyFormatAndBumpSectionProgress(OperationalActionLogErrorLevel.Warning, LVXJobsConsolidateHelper.GetDeclarationHasNoInvoicesLog, lvxToBeDeConsolidated.GetDeclarationIdLink());
						}
						else if (!invoice.SupportAdditionalDeclarations)
						{
							NotifyFormatAndBumpSectionProgress(OperationalActionLogErrorLevel.Warning, LVXJobsConsolidateHelper.GetDeclarationNotAllowMultipleDeclarationLog, lvxToBeDeConsolidated.GetDeclarationIdLink());
						}
						else
						{
							var lvs = invoice.FirstAdditionalDeclaration;
							if (lvs == null)
							{
								NotifyFormatAndBumpSectionProgress(OperationalActionLogErrorLevel.Warning, LVXJobsConsolidateHelper.GetLVXHasNoAdditionalDeclarationLog, lvx.GetDeclarationIdLink());
							}
							else
							{
								if (lvs.HasAB3AcceptedOrWaiting)
								{
									NotifyFormatAndBumpSectionProgress(OperationalActionLogErrorLevel.Warning, LVXJobsConsolidateHelper.GetLVXHasAdditionalDeclarationWithB3AcceptedLog, lvx.GetDeclarationIdLink(), lvs.GetDeclarationIdLink());
								}
								else
								{
									lvs.MessageInitiator = new LVXJobNotificationCollector(NotifyForMessageInitiator);
									LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(invoice, lvs);
									NotifyFormatAndBumpSectionProgress(OperationalActionLogErrorLevel.Informational, LVXJobsConsolidateHelper.GetLVXDetachedFromConsolidatedLVSLog, lvxToBeDeConsolidated.GetDeclarationIdLink(), lvs.GetDeclarationIdLink());
								}
							}
						}
					}
				}
			}

			Factory.Save();
		}

		#endregion

		#region Utility

		void NotifyForMessageInitiator(LogType logType, string format, params object[] args)
		{
			Log?.NotifyFormat(GetActionLogErrorLevel(logType), format, args);
		}

		void NotifyFormatAndBumpSectionProgress(LogType logType, string format, params object[] args)
		{
			NotifyFormatAndBumpSectionProgress(GetActionLogErrorLevel(logType), format, args);
		}

		void NotifyFormatAndBumpSectionProgress(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
		{
			if (Log != null)
			{
				Log.NotifyFormat(errorLevel, format, args);
				Log.BumpSectionProgress();
			}
		}

		OperationalActionLogErrorLevel GetActionLogErrorLevel(LogType logType)
		{
			var errorLevel = OperationalActionLogErrorLevel.Informational;
			switch (logType)
			{
				case LogType.Debug:
					errorLevel = OperationalActionLogErrorLevel.Debug;
					break;
				case LogType.Error:
					errorLevel = OperationalActionLogErrorLevel.Error;
					break;
				case LogType.Information:
					errorLevel = OperationalActionLogErrorLevel.Informational;
					break;
				case LogType.Warning:
					errorLevel = OperationalActionLogErrorLevel.Warning;
					break;
			}

			return errorLevel;
		}

		#endregion

		#region GetLogs

		#endregion
	}
}
