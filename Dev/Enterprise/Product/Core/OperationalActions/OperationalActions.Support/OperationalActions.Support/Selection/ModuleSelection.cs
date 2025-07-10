using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Support
{
	public sealed class ModuleSelection : IFilterRecordsSelection
	{
		public ModuleSelection(IZFilterGridModule module)
		{
			this.module = module;
			ExclusionReasons = new List<String>();
		}

		public IList<String> ExclusionReasons
		{
			get;
			private set;
		}

		public ISelectedRecords GetSelectedRecords()
		{
			ExclusionReasons.Clear();
			BusinessObject[] targets = module.GetSelectedBusinessObjects();
			bool noTargetsSelected = targets.Length == 0;

			targets = FilterExcludedTargets(targets, noTargetsSelected);

			return new SelectedRecords()
			{
				PrimaryKeys = Array.ConvertAll(targets, (bizObj) => bizObj.PK),
				AutoSelectedAllKeys = noTargetsSelected
			};
		}

		public IEnumerable<ISelectedRecords> GetAllFilterRecords(Type bizObjType)
		{
			ExclusionReasons.Clear();
			BusinessObject lastBizoRead = null;
			IEnumerable<BusinessObject> batch;

			var isFilterValid = ValidateFilter();
			if (isFilterValid)
			{
				var reader = ((IBusinessObjectReaderProvider)module).BusinessObjectReaderWithFilter as FilteredBusinessObjectReader;
				reader.BatchSize = Env.Registry.OperationalActionsRecordBatchSize;
				while ((batch = reader.LoadNextBatchInANewFactory(lastBizoRead)).Any())
				{
					lastBizoRead = batch.LastOrDefault();
					BusinessObject[] targets = batch.ToArray();
					targets = FilterExcludedTargets(targets, false);

					yield return new SelectedRecords()
					{
						PrimaryKeys = Array.ConvertAll(targets, (bizObj) => bizObj.PK),
						AutoSelectedAllKeys = false
					};
				}
			}
		}

		bool ValidateFilter()
		{
			module.CommitAllFilters();
			module.RunPreSaveValidation();
			if (module.HasErrors)
			{
				ZArchitecture.Environment.Globals.Message.ShowError(Res.GetString("cd71fb28-0822-4151-9962-0629d0b4df7d",
					"There are errors in the filter. Please correct these before running Operational Actions."), Res.GetString("223b81d6-c8a3-4721-9bb6-5e07fd75b5d6", "Filter Errors"));
				return false;
			}

			return true;
		}

		BusinessObject[] FilterExcludedTargets(BusinessObject[] targets, bool noTargetsSelected)
		{
			if (noTargetsSelected)
			{
				targets = module.GridCollection.ToArray();
			}

			targets = targets.Where(x =>
			{
				var y = x as ICanBeExcludedFromOperationalActions;
				if (y != null)
				{
					if (y.ShouldExclude)
					{
						ExclusionReasons.Add(y.ReasonForExclusion);
					}
					return !y.ShouldExclude;
				}
				return true;
			}).ToArray();
			return targets;
		}

		public int FilterRowCount => ValidateFilter() ? module.ExactRowCount : 0;

		public IZFilterGridModule Module { get { return module; } }
		readonly IZFilterGridModule module;
	}
}
