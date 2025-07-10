using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Module.Records
{
	public class ArchivedRecordsFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var filter = filters.AddDateFilter("Archive Date", StorageMainSchema.SM_Archived);
			filter.MultilingualDescription = ResString.GetMultilingualString("d7f062fb-e4d3-483e-9123-64d51ad187bb", "Archive Date");

			filter = filters.AddDateFilter("Offline Date", StorageMainSchema.SM_OffLine);
			filter.MultilingualDescription = ResString.GetMultilingualString("52145c88-5e71-4248-a636-08a2b1a3fc74", "Offline Date");

			var filtersAdded = new Dictionary<string, string>();
			var errorBuilder = new StringBuilder(100);

			var businessObjectProviderCache = ObjectFactory.Get<IArchiveableBusinessObjectProviderCache>();

			foreach (var keyType in businessObjectProviderCache.GetAllReferenceKeyTypesSupported())
			{
				if (!filtersAdded.ContainsKey(keyType.Code))
				{
					filtersAdded.Add(keyType.Code, keyType.Description);
					var code = keyType.Code;
					var desc = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", keyType.Description.GetUnresolvedString(), keyType.Code);
					var referenceFilter = filters.AddTextFilter(desc, delegate(SQLComparisonOperator comparisonOperator, ZString value) { return GetReferenceFilter(code, comparisonOperator, value); });
					referenceFilter.SubGroup = RefSubGroup;
					referenceFilter.MultilingualDescription = ResString.GetMultilingualString("8F2E1096-169C-40C8-A3EE-CE4D28EDA0BD", "{0} ({1})", keyType.Description, (NoResString)keyType.Code);
				}
				else
				{
					var desc = filtersAdded[keyType.Code];
					if (keyType.Description != desc)
					{
						_ = errorBuilder.AppendFormat((NoResString)"ReferenceKey with same code '{0}' found with different descriptions '{1}' and '{2}'", keyType.Code, desc, keyType.Description);
						_ = errorBuilder.AppendLine();
					}
				}
			}

			if (errorBuilder.Length > 0)
			{
				ExceptionReporter.Instance.ReportDeveloperException((NoResString)"ReferenceKey duplicates found", new InvalidOperationException(errorBuilder.ToString()));
			}

			return filters;
		}

		ZQuery GetReferenceFilter(string referenceType, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(StorageReference));
			_ = query.AddToFilter(StorageReferenceSchema.SR_Reference, comparisonOperator, value);
			_ = query.AddToFilter(StorageReferenceSchema.SR_TYPE, referenceType);

			return query;
		}

		ReferenceSubGroup RefSubGroup
			=> refSubGroup ?? (refSubGroup = new ReferenceSubGroup());

		ReferenceSubGroup refSubGroup;

		protected class ReferenceSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ArchiveStorageMain));

				var storageReferenceSubQuery = new ZDBOnlySubQuery(typeof(StorageReference), StorageReferenceSchema.SR_SM);
				_ = storageReferenceSubQuery.AddToFilter(filter);
				result.AddSubQuery(storageReferenceSubQuery, JoinCondition.And);

				return result;
			}
		}
	}
}
