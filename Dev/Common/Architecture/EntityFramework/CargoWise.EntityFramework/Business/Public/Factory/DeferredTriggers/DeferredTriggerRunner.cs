using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using static System.FormattableString;

namespace CargoWise.EntityFramework
{
	class DeferredTriggerRunner : IDeferredTriggerRunner
	{
		#region DeferAndReturnTriggers

		IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>> IDeferredTriggerRunner.DeferAndReturnTriggers(BusinessObjectFactory factory, IEnumerable<BusinessObject> businessObjectsInLastSaveOrder)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(businessObjectsInLastSaveOrder, nameof(businessObjectsInLastSaveOrder));

			var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>(StringComparer.OrdinalIgnoreCase);
			var allTriggers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var allBizosGrouped = businessObjectsInLastSaveOrder.ToLookup(b => b.GetType());
			var addMethodCache = new Dictionary<IReadOnlyCollection<TriggerMetaData>, Action<TriggerMetaData>>();

			foreach (var bizOTypeGroup in allBizosGrouped)
			{
				var attributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOTypeGroup.Key);
				foreach (var attribute in attributes)
				{
					var bizOs = GetBizOsThatRequireDeferredTriggers(bizOTypeGroup, attribute);
					if (bizOs.Count > 0)
					{
						allTriggers.Add(attribute.TriggerName);

						var (values, column) = GetValuesForDeferredTriggers(factory, bizOTypeGroup.Key, bizOs, attribute);
						var metaData = new TriggerMetaData(attribute, bizOTypeGroup.Key, column, values);
						var key = !attribute.IsSuspendTriggerOnly
							? Invariant($"{attribute.StoredProcName}|{attribute.ExtraParamsForStoredProc}")
							: attribute.TriggerName;

						if (!deferredTriggers.TryGetValue(key, out var sameProcedureTriggers))
						{
							// Since IReadOnlyDictionary does not have a Covariant Generic Argument for the Value (the list in this case),
							// Dictionary<string, List<TriggerMetaData>> cannot be returned as IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>>.
							// To prevent having to recreate the dictionary, we just store the Add method of the list and use it to append to the list.
							var list = new List<TriggerMetaData>();
							addMethodCache[list] = list.Add;
							deferredTriggers[key] = sameProcedureTriggers = list;
						}

						addMethodCache[sameProcedureTriggers].Invoke(metaData);
					}
				}
			}

			allTriggers.ForEach(t => SuspendTrigger(factory, t));

			return deferredTriggers;
		}

		static void SuspendTrigger(IDbConnected connected, string triggerName)
		{
			using (var command = connected.Connection.Command(Invariant($"EXEC dbo.SuspendTrigger '{triggerName}'")))
			{
				command.ExecuteNonQuery();
			}
		}

		static IReadOnlyCollection<BusinessObject> GetBizOsThatRequireDeferredTriggers(IGrouping<Type, BusinessObject> bizOTypeGroup, DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			var bizOsThatRequireDeferredTriggers = new List<BusinessObject>();
			var deferStrategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(attribute.DeferTriggerConditionStrategyType.Name);

			foreach (var bizO in bizOTypeGroup)
			{
				if (deferStrategy.RunType == TriggerRunType.InsertOrUpdate)
				{
					if (!bizO.IsDeleted && deferStrategy.ShouldDeferTrigger(bizO))
					{
						bizOsThatRequireDeferredTriggers.Add(bizO);
					}
				}
				else if (deferStrategy.RunType == TriggerRunType.Delete)
				{
					if (bizO.IsDeleted)
					{
						bizOsThatRequireDeferredTriggers.Add(bizO);
					}
				}
			}

			return bizOsThatRequireDeferredTriggers;
		}

		static (IEnumerable<ZGuid> Values, SchemaColumn Column) GetValuesForDeferredTriggers(BusinessObjectFactory factory, Type bizOType, IReadOnlyCollection<BusinessObject> bizOs, DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			SchemaColumn schemaColumn = null;
			var values = Enumerable.Empty<ZGuid>();

			if (!attribute.IsSuspendTriggerOnly)
			{
				var deferStrategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(attribute.DeferTriggerConditionStrategyType.Name);
				var tableSchema = BusinessObjectFactory.GetTableSchemaFromType(bizOType);
				schemaColumn = tableSchema.GetSchemaColumn(attribute.ColumnWithRowPkToRunStoredProcOn);

				if (deferStrategy.RunType == TriggerRunType.Delete)
				{
					values = GetValuesForDeletedBizO(factory, bizOs, tableSchema, schemaColumn);
				}
				else if (deferStrategy.RunType == TriggerRunType.InsertOrUpdate)
				{
					values = GetValuesForInsertedOrUpdatedBizO(bizOs, attribute, schemaColumn).ToArray();
				}
				else
				{
					throw new InvalidOperationException(Invariant($"RunType of Defer Strategy Type {deferStrategy.GetType()} was invalid"));
				}
			}

			return (values, schemaColumn);
		}

		static IEnumerable<ZGuid> GetValuesForDeletedBizO(BusinessObjectFactory factory, IReadOnlyCollection<BusinessObject> bizOs, ITableSchema tableSchema, SchemaColumn schemaColumn)
		{
			IEnumerable<ZGuid> result;

			var pkColumn = tableSchema.PK;
			var pks = bizOs.Select(b => b.PK).ToArray();
			if (schemaColumn == pkColumn)
			{
				result = pks;
			}
			else
			{
				var dynamicCollection = new DynamicBusinessObjectCollection(factory);

				var tvp = ZSqlParameter.New("@PKs", pks, pkColumn, isTableValued: true);
				dynamicCollection.Load(
					Invariant($"SELECT {schemaColumn.Name} FROM {tableSchema.TableName} WHERE {pkColumn.Name} IN (SELECT Value FROM {tvp.ParameterName})"),
					new[] { tvp });

				result = dynamicCollection.Cast<DynamicBusinessObject>().Select(d => (ZGuid)d[schemaColumn.Name]).ToArray();
			}

			return result;
		}

		static IEnumerable<ZGuid> GetValuesForInsertedOrUpdatedBizO(IReadOnlyCollection<BusinessObject> bizOs, DeferTriggerAndRunBeforeCommitAttribute attribute, SchemaColumn schemaColumn)
		{
			foreach (var bizO in bizOs)
			{
				if (attribute.ValueToRunStoredProcWith.HasFlag(ValueVersion.Current) || schemaColumn.IsPKColumn || !bizO.IsInDatabase)
				{
					yield return (ZGuid)bizO[schemaColumn.Name];
				}

				if (attribute.ValueToRunStoredProcWith.HasFlag(ValueVersion.Original) && !schemaColumn.IsPKColumn && bizO.IsInDatabase)
				{
					yield return (ZGuid)bizO.GetZPropertyInfo(attribute.ColumnWithRowPkToRunStoredProcOn).OriginalValue;
				}
			}
		}

		#endregion

		#region RunDeferredTriggers

		void IDeferredTriggerRunner.RunDeferredTriggers(IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>> deferredTriggers, IDbConnected factory)
		{
			Argument.NotNull(deferredTriggers, nameof(deferredTriggers));
			Argument.NotNull(factory, nameof(factory));

			var connection = factory.Connection;
			var allTriggers = new HashSet<string>();

			foreach (var deferredTriggersByCheckProcedure in deferredTriggers.OrderBy(CheckProceduresFirst))
			{
				TriggerMetaData referenceMetaData = null;

				foreach (var trigger in deferredTriggersByCheckProcedure.Value)
				{
					var triggerName = trigger.Attribute.TriggerName;
					if (allTriggers.Add(triggerName))
					{
						if (!trigger.Attribute.IsSuspendTriggerOnly)
						{
							referenceMetaData = trigger;
						}

						connection.ExecuteNonQuery(Invariant($"EXEC dbo.ResumeTrigger '{triggerName}'"));
					}
				}

				var valuesToBatch = deferredTriggersByCheckProcedure.Value.Where(o => !o.Attribute.IsSuspendTriggerOnly).SelectMany(o => o.Values).ToArray();
				if (valuesToBatch.Length > 0)
				{
					if (referenceMetaData == null)
					{
						throw new InvalidOperationException(Invariant($"No reference meta data found for Deferred Check procedure {deferredTriggersByCheckProcedure.Key}."));
					}

					var (procedureName, extraParamsForStoredProc, columnForParameter) = GetProcedureInfo(referenceMetaData);

#if NETFRAMEWORK
					var batchValues = valuesToBatch.Chunk(BatchSize);
#else
					var batchValues = IEnumerableExtensions.Chunk(valuesToBatch, BatchSize);
#endif

					foreach (var values in batchValues)
					{
						try
						{
							using (var command = connection.Command(Invariant($"EXEC dbo.{procedureName} @Values{extraParamsForStoredProc}")))
							{
								command.AddTableValuedParameter("@Values", columnForParameter, values);
								command.ExecuteNonQuery();
							}
						}
						catch (SqlException ex)
						{
							if (ex.Message.StartsWith(ExceptionExtensions.TriggerPrefix.TriggerLikelyConcurrencyError))
							{
								throw new ZConcurrencyCheckFailureException(ex.Message, ExceptionExtensions.TriggerPrefix.TriggerLikelyConcurrencyError, shouldReprocess: false);
							}
							else
							{
								throw new ZDataException(ex, null, connection);
							}
						}
					}
				}
			}

			static bool CheckProceduresFirst(KeyValuePair<string, IReadOnlyCollection<TriggerMetaData>> item) =>
				item.Value.First().Attribute.IsSuspendTriggerOnly;

			static (string ProcName, string ExtraParams, SchemaColumn Column) GetProcedureInfo(TriggerMetaData metaData)
			{
				var procedureName = metaData.Attribute.StoredProcName;
				var extraParamsForStoredProc = string.IsNullOrEmpty(metaData.Attribute.ExtraParamsForStoredProc)
					? ""
					: Invariant($", {metaData.Attribute.ExtraParamsForStoredProc}");

				return (procedureName, extraParamsForStoredProc, metaData.Column);
			}
		}

		const int BatchSize = 500;

		#endregion
	}
}
