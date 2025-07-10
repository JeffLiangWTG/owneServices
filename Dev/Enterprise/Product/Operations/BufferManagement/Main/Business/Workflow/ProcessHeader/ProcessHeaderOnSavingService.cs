using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class ProcessHeaderOnSavingService : IOnSavingService, IOnSavingServiceBuilder
	{
		#region Interface impl and hookup

		internal static void HookupFactory(BusinessObjectFactory factory)
		{
			if (factory.ServiceContainer.GetOnSavingService<ProcessHeaderOnSavingService>() == null)
			{
				factory.ServiceContainer.AddOnSavingService(new ProcessHeaderOnSavingService());
			}
		}

		IEnumerable<IOnSavingService> IOnSavingServiceBuilder.Build(IEnumerable<BusinessObject> rows)
		{
			yield return this;
		}

		void IOnSavingService.Apply(IEnumerable<BusinessObject> bizos)
		{
			var bizosToUpdate = new HashSet<ProcessHeader>();
			foreach (var bizo in bizos.Where(b => !b.IsDeleted))
			{
				switch (bizo)
				{
					case ProcessTask task:
						if (task.ProcessHeader is ProcessHeader taskHeader)
						{
							bizosToUpdate.Add(taskHeader);
						}
						break;
					case ProcessHeaderLink link:
						if (link.HeaderFrom is ProcessHeader headerFrom)
						{
							bizosToUpdate.Add(headerFrom);
						}
						if (link.HeaderTo is ProcessHeader headerTo)
						{
							bizosToUpdate.Add(headerTo);
						}
						break;
					case ProcessHeader header:
						bizosToUpdate.Add(header);
						break;
				}
			}
			if (bizosToUpdate.Any())
			{
				UpdateHeaders(bizosToUpdate);
			}
		}

		void IOnSavingService.OnSaveFailed()
		{
			// Nothing needed on failure.
		}

		#endregion

		void UpdateHeaders(IEnumerable<ProcessHeader> headers)
		{
			var saveSettings = ProcessHeader.GetProcessHeaderSaveSettings(headers.First().Factory);

			if (!saveSettings.ReloadingComponentsIgnoringUberFactoryCacheSuppressed)
			{
				ReloadComponentsIgnoringUberFactoryCache(headers);
			}

			if (!saveSettings.UpdatingWorkflowStatusesSuppressed)
			{
				WorkflowStatusUpdater.UpdateWorkflowStatuses(headers, reloadStatusRelatedDataFromDb: true);
			}
			foreach (var header in headers)
			{
				header.OnSavingForService();
			}
		}

		void ReloadComponentsIgnoringUberFactoryCache(IEnumerable<ProcessHeader> headers)
		{
			// we need actual data from db for updating effective branch and department on save
			// we don't change FH_FC_CurrentComponent and w.FH_FC_DedicatedBuffer on save (except possibly resetting dedicated buffer which is fine), so reloading component data here if appropriate
			var componentPKs = headers
				.Where(w => w.FH_FC_CurrentComponentInfo.HasChanges || w.FH_FC_DedicatedBufferInfo.HasChanges)
				.SelectMany(w => new ZGuid[] { w.FH_FC_CurrentComponent, w.FH_FC_DedicatedBuffer })
				.Where(pk => pk.IsValid)
				.Distinct();
			var query = new ZQuery(BMComponentSchema.PK, componentPKs);
			query.ReLoadExistingRows = true;
			var factory = headers.First().Factory;
			factory.Load<BMComponent>(query);
		}
	}
}
