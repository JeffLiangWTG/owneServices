using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class ConsolidationBatchExportAdapter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ConsolidationBatchExportAdapter(BusinessObjectFactory factory, ZGuid parentConsolidationGroupPK, bool includeManualEliminationJournals)
			: base(factory)
		{
			this.parentConsolidationGroupPK = parentConsolidationGroupPK;
			this.includeManualEliminationJournals = includeManualEliminationJournals;
		}

		readonly ZGuid parentConsolidationGroupPK;
		readonly bool includeManualEliminationJournals;

		ZInt fromPeriod;
		public ZInt FromPeriod
		{
			get { return fromPeriod; }
			set { SetNonPersistentPropertyValue(FromPeriodInfo, ref fromPeriod, value); }
		}

		public ZPropertyInfo FromPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(FromPeriod)); }
		}

		ZInt toPeriod;
		public ZInt ToPeriod
		{
			get { return toPeriod; }
			set { SetNonPersistentPropertyValue(ToPeriodInfo, ref toPeriod, value); }
		}

		public ZPropertyInfo ToPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(ToPeriod)); }
		}

		public ConsolidationBatchExportRowCollection RowsForExport
		{
			get { return rowsForExport ?? (rowsForExport = new ConsolidationBatchExportRowCollection(Factory)); }
		}
		ConsolidationBatchExportRowCollection rowsForExport;

		public void PopulateRowsForExport()
		{
			var exporter = new ConsolidationBatchExporter(GetBatchPKs(), includeManualEliminationJournals);
			var detailsRows = exporter.Export();

			foreach (var detailsRow in detailsRows)
			{
				RowsForExport.Add(new ConsolidationBatchExportRow(Factory, detailsRow));
			}
		}

		public void CreateEliminationJournalPerPeriod()
		{
			new EliminationJournalCreator().Create(GetBatchPKs(true));
		}

		public IEnumerable<ZGuid> GetBatchPKs(bool excludeBatchesThatAlreadyHaveJournals = false)
		{
			var parentGroup = Factory.Load<AccConsolidationGroup>(parentConsolidationGroupPK);
			var allConsolidationGroups = new List<ZGuid>(parentGroup.GetAllGetChildGroupsIncludingDescendents().Select(x => x.PK));
			allConsolidationGroups.Add(parentGroup.PK);

			var query = new ZDBOnlyQuery(typeof(AccConsolidationBatch));
			query.AddToFilter(AccConsolidationBatchSchema.YB_YR_ConsolidationGroup, allConsolidationGroups);

			if (excludeBatchesThatAlreadyHaveJournals)
			{
				query.AddToFilter(AccConsolidationBatchSchema.YB_AH_EliminationJournal, null);
			}

			var subQuery = new ZDBOnlySubQuery(typeof(AccPeriodManagement), AccConsolidationBatchSchema.YB_AM_Period);
			subQuery.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.GreaterThanOrEqualTo, FromPeriod);
			subQuery.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.LessThanOrEqualTo, ToPeriod);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return Factory.Load<AccConsolidationBatch>(query).Select(x => x.PK);
		}

		public IExportCollectionInfo GetMultiTypeCollectionInfo()
		{
			var info = new ExportCollectionInfoImpl(Factory, RowsForExport);
			AddProperties<ConsolidationBatchExportRow>(info, ResString.GetMultilingualString("9a3ac78d-711a-4a4d-b06e-b41044ad69df", "Export Row"));
			return info;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Class Name does not need to be localised")]
		void AddProperties<T>(ExportCollectionInfoImpl info, MultilingualString name)
		{
			ImportPropertyInfoCollection fields = new ImportPropertyInfoCollection();
			string schemaClassName = "Schema";
			Type schemaType = typeof(T).BaseType.GetNestedType(schemaClassName) ?? typeof(T).BaseType.BaseType.GetNestedType(schemaClassName);
			if (schemaType != null)
			{
				foreach (FieldInfo fieldInfo in schemaType.GetFields())
				{
					if (fieldInfo.FieldType.Name == "String" && !fieldInfo.Name.EndsWith("MaxLength"))
					{
						fields.Add(new ImportPropertyInfoImpl<T>(fieldInfo.Name));
					}
				}
			}
			info.Add(name, typeof(T), fields);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateFromPeriod();
			ValidateToPeriod();
		}

		void ValidateFromPeriod()
		{
			FromPeriodInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FromPeriodInfo);
			MandatoryValidation.CheckNotNegative(FromPeriodInfo);
			MandatoryValidation.CheckNotZero(FromPeriodInfo);
		}

		void ValidateToPeriod()
		{
			FromPeriodInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ToPeriodInfo);
			MandatoryValidation.CheckNotNegative(ToPeriodInfo);
			MandatoryValidation.CheckNotZero(ToPeriodInfo);
		}
	}
}
